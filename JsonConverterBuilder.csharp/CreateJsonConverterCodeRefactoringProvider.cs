using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace JsonConverterBuilder.csharp
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CreateJsonConverterCodeRefactoringProvider)), Shared]

    public class CreateJsonConverterCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            Document document = context.Document;
            Microsoft.CodeAnalysis.Text.TextSpan textSpan = context.Span;
            CancellationToken cancellationToken = context.CancellationToken;

            CompilationUnitSyntax root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken token = root.FindToken(textSpan.Start);
        //    var compilation = CSharpCompilation.Create("dummy").AddSyntaxTrees(root.SyntaxTree);
        //    var diag = compilation.GetDiagnostics();
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            //var properties = token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            //foreach(var property in properties)
            //{
            //    PropertyDeclarationSyntax pds = property as PropertyDeclarationSyntax;
            //    var si = semanticModel.GetSymbolInfo(property);
            //    if (si.CandidateSymbols.Count() > 0)
            //    {
            //        si = si;
            //    }
            //    foreach (var cn in property.ChildNodes()) {
            //        si = semanticModel.GetSymbolInfo(cn);
            //        string txt = cn.ToString();
            //        if (si.CandidateSymbols.Count() > 0 )
            //        {
            //            si = si;
            //        }
            //    }
            //}

            if (token.Kind() == SyntaxKind.IdentifierToken
                && token.Parent.Kind() == SyntaxKind.ClassDeclaration )
            {
                string className = token.Text;
               

                CreateJsonConverterCodeAction action = new CreateJsonConverterCodeAction($"Create JsonConverter for {className}",
                    (c) => Task.FromResult(CreateJsonConverterForClass(document, semanticModel, token, root, c)));                
                context.RegisterRefactoring(action);
            }
        }

        private Document CreateJsonConverterForClass(Document document,
                                            SemanticModel semanticModel,
                                            SyntaxToken token,
                                            CompilationUnitSyntax root,
                                            CancellationToken cancellationToken)
        {
            string className = token.Text;
            CompilationUnitSyntax oldRoot = (CompilationUnitSyntax)semanticModel.SyntaxTree.GetRoot();
            //CompilationUnitSyntax newRoot = oldRoot.AddUsingsIfMissing(token, "System.Text.Json")
            //                                       .AddUsingsIfMissing(token, "System.Text.Json.Serialization");
                                                   // .AddAttributeForJsonConverter(token, className);

            AddJsonAttributeConverterSyntaxRewrier rewriter = new AddJsonAttributeConverterSyntaxRewrier(className,semanticModel, root, token);
            SyntaxNode newSource = rewriter.Visit(oldRoot);

            return document.WithSyntaxRoot(newSource);
        }



        //private SyntaxNode GetNamespaceNode(SyntaxToken token)
        //{
        //    var searchToken = token.Parent;
        //    while (searchToken.Kind() != SyntaxKind.NamespaceDeclaration) { searchToken = searchToken.Parent; }
        //    return searchToken;
        //}       



        private class CreateJsonConverterCodeAction : CodeAction
        {
            private readonly string title;
            private readonly Func<CancellationToken, Task<Document>> createChangedDocument;

            public CreateJsonConverterCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
            {
                this.title = title;
                this.createChangedDocument = createChangedDocument;
            }

            public override string Title { get { return title; } }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                return createChangedDocument(cancellationToken);
            }
        }
    }
    

    public class AddJsonAttributeConverterSyntaxRewrier : CSharpSyntaxRewriter
    {
        public readonly string ClassName;
        private readonly SemanticModel SemanticModel;
        private readonly CompilationUnitSyntax OriginalRoot;
        private readonly SyntaxToken Token;

        public AddJsonAttributeConverterSyntaxRewrier(string className, SemanticModel semanticModel, 
            CompilationUnitSyntax root, 
            SyntaxToken token)
        {
            ClassName = className;
            SemanticModel = semanticModel;
            OriginalRoot = root;
            Token = token;
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            CompilationUnitSyntax result = node;
            if ( result.Usings.All(u => u.Name.ToString() != "System.Text.Json"))
            {
                result = result.AddUsings(UsingDirective(IdentifierName("System.Text.Json")));
            }
            if (result.Usings.All(u => u.Name.ToString() != "System.Text.Json.Serialization"))
            {
                result = result.AddUsings(UsingDirective(IdentifierName("System.Text.Json.Serialization")));
            }
            return base.VisitCompilationUnit(result);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.Text == ClassName)
            {
                return base.VisitClassDeclaration(
                    node.WithAttributeLists(
                    new SyntaxList<AttributeListSyntax>(
                    AttributeList().AddAttributes(
                        Attribute(IdentifierName("JsonConverter"),
                        AttributeArgumentList(
                            SeparatedList<AttributeArgumentSyntax>(
                                new List<AttributeArgumentSyntax>() {
                                    AttributeArgument(
                                        TypeOfExpression(
                                            IdentifierName(Identifier(ClassName + "JsonConverter")))
                                        )
                                    }
                                )
                            )))))
                    );
            }
            else
            {
                return base.VisitClassDeclaration(node);
            }
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            ClassDeclarationSyntax classDec = CreateJsonConverterClassDeclaration();

            return base.VisitNamespaceDeclaration(
                node.AddMembers(classDec)
            );
        }

        private ClassDeclarationSyntax CreateJsonConverterClassDeclaration()
        {
            ClassDeclarationSyntax classDec = ClassDeclaration(ClassName + "JsonConverter")
                            .WithLeadingTrivia(CarriageReturnLineFeed)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(
                                new List<BaseTypeSyntax>() {
                    SimpleBaseType(SyntaxFactory.ParseTypeName("JsonConverter<"+ClassName+">"))
                                }
                            )));

            classDec = classDec.AddMembers(CreateReadMethod());
            foreach(string key in GetListMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(GetListMethods[key]);
            }
            foreach (string key in GetArrayMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(GetArrayMethods[key]);
            }
            foreach (string key in GetDictionaryMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(GetDictionaryMethods[key]);
            }
            foreach (string key in GetObjectMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(GetObjectMethods[key]);
            }

            classDec = classDec.AddMembers(CreateWriteMethod());
            foreach (string key in WriteDictionaryMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(WriteDictionaryMethods[key]);
            }
            foreach (string key in WriteObjectMethods.Keys.OrderBy(x => x))
            {
                classDec = classDec.AddMembers(WriteObjectMethods[key]);
            }
            return classDec;
        }

        private MethodDeclarationSyntax CreateReadMethod()
        {
            return MethodDeclaration(IdentifierName(ClassName), "Read")
                .WithModifiers(TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader"))
                    .WithModifiers(TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)))
                    ,
                    Parameter(Identifier("typeToConvert")).WithType(IdentifierName("Type"))
                    ,
                    Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions"))
                    }
                )))
                .WithBody(
                        CreateVariableDeclarations(
                        WhileStatement(SyntaxFactory.ParseExpression("true"),
                        Block(
                            SyntaxFactory.ParseStatement("reader.Read();"),
                           SwitchStatement(SyntaxFactory.ParseExpression("reader.TokenType"),
                              new SyntaxList<SwitchSectionSyntax>(
                                    new List<SwitchSectionSyntax>() {
                            SwitchSection(
                               SingletonList(
                                    (SwitchLabelSyntax)CaseSwitchLabel(IdentifierName("JsonTokenType.StartObject")
                                )
                              )
                              ,
                              SingletonList((StatementSyntax)BreakStatement())
                            ),

                            SwitchSection(
                               SingletonList(
                                    (SwitchLabelSyntax)CaseSwitchLabel(IdentifierName("JsonTokenType.EndObject")
                                )
                              )
                              ,
                              GenerateNewObjectStatements()
                            ),

                            SwitchSection(
                               SingletonList(
                                    (SwitchLabelSyntax)CaseSwitchLabel(IdentifierName("JsonTokenType.PropertyName")
                                )
                              )
                              ,
                              PropertiesSwitchStatement()
                            ),

                           SwitchSection(
                              SingletonList((SwitchLabelSyntax)DefaultSwitchLabel())
                              ,
                              SingletonList((StatementSyntax)BreakStatement())
                            )
                           })
                        )
                           )
                )
            ));
        }

        private SyntaxList<StatementSyntax> GenerateNewObjectStatements()
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            List<string> parameterNames = new List<string>();
            foreach (var property in properties)
            {
                var si = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
                
                if (si.DeclaredAccessibility == Accessibility.Public)
                {
                   var n = si.Name;
                    var typeName = si.Type.ToString();
                   bool isRequired = ! typeName.EndsWith("?");
                    if (isRequired)
                    {
                        statements.Add(ParseStatement($"if ({n} == null) throw new JsonException(\"{ClassName} is missing property {n}\");")
                                                            .WithTrailingTrivia(ElasticCarriageReturnLineFeed));

                        if (NeedsValueInConstructor(typeName))
                        {
                            parameterNames.Add(si.Name + ".Value");
                        }
                        else
                        {
                            parameterNames.Add(si.Name);
                        }
                    }
                    else
                    {
                        parameterNames.Add(n);
                    }
                }
            }
            
            statements.Add(ParseStatement($"return new {ClassName}({String.Join(", ",parameterNames)});"));
            return new SyntaxList<StatementSyntax>(statements);
        }

        private bool NeedsValueInConstructor(string t)
        {
            switch (t) { 
            case "int":  return true;
            case "int?": return true;
            case "doble": return true;
            case "double?": return true;
            case "float": return true;
            case "float?": return true;
            case "decimal": return true;
            case "decimal?": return true;
            case "DateTime": return true;
            case "DateTime?" : return true;
                case "System.DateTime": return true;
                case "System.DateTime?": return true;
                default: return false;              
            };
        }


        private bool NeedsValue(string t)
        {
            switch (t)
            {
                case "int?": return true;
                case "double?":  return true;
                case "float?": return true;
                case "decimal?":  return true;
                case "DateTime?":  return true;
                case "System.DateTime?": return true;
                default: return false;                
            };
        }

        private BlockSyntax CreateVariableDeclarations(WhileStatementSyntax whileStatement)
        {
            BlockSyntax block = Block();
            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            foreach (var property in properties)
            {
                var si = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
                if (si.DeclaredAccessibility == Accessibility.Public)
                {
                    var n = si.Name;
                    var variableSuffix = si.Type.ToString().EndsWith("?") ? "" : "?";
                    block = block.AddStatements(ParseStatement($"{si.Type}{variableSuffix} {n} = null;")
                        .WithTrailingTrivia(ElasticCarriageReturnLineFeed)
                        );
                }
            }

            if (block.Statements.Count > 0)
            {
                StatementSyntax lastStmt = block.Statements.Last();

                BlockSyntax blockWithCR = Block();
                foreach (StatementSyntax stmt in block.Statements)
                {
                    if (stmt != lastStmt)
                    {
                        blockWithCR = blockWithCR.AddStatements(stmt);
                    }
                    else
                    {
                        blockWithCR = blockWithCR.AddStatements(stmt.WithTrailingTrivia(CarriageReturnLineFeed, CarriageReturnLineFeed));
                    }
                }
                blockWithCR = blockWithCR.AddStatements(whileStatement.WithoutLeadingTrivia());
                return blockWithCR;
            } 
            else
            {
                return block.AddStatements(whileStatement.WithoutLeadingTrivia());
            }
        }

        private MethodDeclarationSyntax CreateWriteMethod()
        {
            return MethodDeclaration(IdentifierName("void"), "Write")
                .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                        Parameter(Identifier("writer")).WithType(IdentifierName("Utf8JsonWriter"))
                    ,
                        Parameter(Identifier("value")).WithType(IdentifierName(ClassName))
                    ,
                        Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions"))
                    }
                )))
                .WithBody(
                    Block( GenerateWriteStatements()
                        )
                    );
        }

        private StatementSyntax[] GenerateWriteStatements()
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(SyntaxFactory.ParseStatement("writer.WriteStartObject();")
              .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            foreach (var property in properties)
            {
                var si = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
                if (si.DeclaredAccessibility == Accessibility.Public)
                {
                    var n = si.Name;
                    var typeName = si.Type.ToString();

                    if (typeName.EndsWith("?"))
                    {
                        var ifStatement = IfStatement(
                            ParseExpression($"value.{n} != null")
                            ,
                            Block(CreateWriteStatement(typeName, n)));
                        statements.Add(ifStatement);
                    }
                    else
                    {
                        foreach (StatementSyntax stmt in CreateWriteStatement(typeName, n))
                        {
                            statements.Add(stmt);
                        }
                    }
                }
            }

            statements.Add(SyntaxFactory.ParseStatement("writer.WriteEndObject();").WithLeadingTrivia(ElasticSpace).WithTrailingTrivia(ElasticCarriageReturn));
            return statements.ToArray();
        }

        enum TypeNameType
        {
            ListType,
            ArrayType,
            DictionaryType,
            ObjectType,
            DateTimeType,
            DefaultType
        }

        private TypeNameType  GetTypeNameType(string typeName)
        {
            if (IsListType(typeName)) return TypeNameType.ListType;
            if (IsArrayType(typeName)) return TypeNameType.ArrayType;
            if (IsDictionaryType(typeName)) return TypeNameType.DictionaryType;
            if (IsObjectType(typeName)) return TypeNameType.ObjectType;
            if (typeName == "DateTime" || typeName == "DateTime?") return TypeNameType.DateTimeType;
            return TypeNameType.DefaultType;
        }

        private SyntaxList<StatementSyntax> CreateWriteStatement(string typeName, string parameterName)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();

            switch (GetTypeNameType(typeName)) 
            {
                case TypeNameType.ObjectType:
                    {
                        string objType = ObjectType(typeName);
                        statements.Add(ParseStatement($"Write{objType}(writer, nameof({ClassName}.{parameterName}), value.{parameterName}, options);").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        if (!WriteObjectMethods.ContainsKey(objType))
                        {
                            WriteObjectMethods.Add(objType, CreateWriteObjectMethod(objType));
                        }
                    }
                    break;

                case TypeNameType.DictionaryType:
                    {
                        string keyType = DictionaryKeyType(typeName);
                        string valueType = DictionaryValueType(typeName);
                        statements.Add(ParseStatement($"WriteDictionary<{keyType}, {valueType}>(writer, nameof({ClassName}.{parameterName}), value.{parameterName}, (k, v) => writer.{WriteMethod(valueType)}(k{TypeToStringMethod(keyType)}, v));").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        if (!WriteDictionaryMethods.ContainsKey("generic"))
                        {
                            WriteDictionaryMethods.Add("generic", CreateWriteDictionaryMethod());
                        }
                    }
                    break;

                case TypeNameType.ListType:
                    {
                        string subtype = ListSubtype(typeName);
                        statements.Add(ParseStatement($"writer.WritePropertyName(nameof({ClassName}.{parameterName}));").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        statements.Add(ParseStatement($"writer.WriteStartArray();").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        statements.Add(ParseStatement($@"foreach({subtype} x in value.{parameterName})
            {{
                writer.{WriteMethod(subtype)}Value(x);
            }}").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        statements.Add(ParseStatement($"writer.WriteEndArray();").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                    }
                    break;
                case TypeNameType.ArrayType:
                    {
                        {
                            string subtype = ArraySubtype(typeName);
                            statements.Add(ParseStatement($"writer.WritePropertyName(nameof({ClassName}.{parameterName}));").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                            statements.Add(ParseStatement($"writer.WriteStartArray();").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                            statements.Add(ParseStatement($@"foreach({subtype} x in value.{parameterName})
            {{
                writer.{WriteMethod(subtype)}Value(x);
            }}").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                            statements.Add(ParseStatement($"writer.WriteEndArray();").WithTrailingTrivia(ElasticCarriageReturnLineFeed));
                        }
                    }
                    break;
                 case TypeNameType.DateTimeType:
                    {
                        var suffix = NeedsValue(typeName) ? ".Value" : "";
                        statements.Add(ParseStatement($"writer.WriteString(nameof({ClassName}.{parameterName}),value.{parameterName}{suffix}.ToString(\"yyyy-MM-dd HH:mm:ss.ffffff\"));").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
                    }
                    break;
                default:
                    {
                        var toString = GetToStringMethod(typeName);
                        var suffix = NeedsValue(typeName) ? ".Value" : "";
                        statements.Add(ParseStatement($"writer.{WriteMethod(typeName)}(nameof({ClassName}.{parameterName}),value.{parameterName}{suffix}{toString});").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
                    }
                    break;
            }
            return new SyntaxList<StatementSyntax>(statements);
        }


        private object GetToStringMethod(string typeName)
        {
            switch (typeName)
            {
                case "DateTime": return  ".ToString(\"yyyy-MM-dd HH:mm:ss.ffffff\")";
                case "DateTime?": return ".ToString(\"yyyy-MM-dd HH:mm:ss.ffffff\")";
                default: return "";
            };
        }

        private string WriteMethod(string typeName)
        {
            switch (typeName)
            {
                case "int": return "WriteNumber";
                case "int?":
                    return "WriteNumber";
                case "float":
                    return "WriteNumber";
                case "float?":
                    return "WriteNumber";
                case "double":
                    return "WriteNumber";
                case "double?":
                    return "WriteNumber";
                case "decimal":
                    return "WriteNumber";
                case "decimal?":
                    return "WriteNumber";
                case "string":
                    return "WriteString";
                case "string?":
                    return "WriteString";
                case "String":
                    return "WriteString";
                case "String?":
                    return "WriteString";
                case "DateTime":
                    return "WriteString";
                case "DateTime?":
                    return "WriteString";
                default: return "Write" + CapitilizeFirstChar(typeName);
            }
        }


        private string TypeToStringMethod(string typeName)
        {
            switch (typeName)
            {
               case  "int": return ".ToString()";
               case "int?": return ".Value.ToString()";
               case "float": return ".ToString()";
               case "float?": return ".Value.ToString()";
               case "double": return ".ToString()";
               case "double?": return ".Value.ToString()";
               case "decimal": return ".ToString()";
               case "decimal?": return ".Value.ToString()";
               case "string": return "";
               case "string?": return ".Value";
               case "String": return "";
               case "String?": return ".Value";
               case "DateTime": return ".ToString(\"yyyy-MM-dd HH:mm:ss.fff\")";
               case "DateTime?": return ".Value.ToString(\"yyyy-MM-dd HH:mm:ss.fff\")";
               default: return "Write" + CapitilizeFirstChar(typeName);
            }
        }


        private SyntaxList<StatementSyntax> PropertiesSwitchStatement()
        {
            List<SwitchSectionSyntax> statements = new List<SwitchSectionSyntax>();

            // add switch statement for each property
            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            List<string> parameterNames = new List<string>();
            foreach (var property in properties)
            {
                var si = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;

                if (si.DeclaredAccessibility == Accessibility.Public)
                {
                    var n = si.Name;
                    var typeName = si.Type.ToString();

                    statements.Add(SwitchSection(
                            SingletonList((SwitchLabelSyntax)CaseSwitchLabel(ParseExpression($"nameof({ClassName}.{n})")))
                            ,
                            CreateParameterReadStatements(n, typeName)
                        )
                    ); ;
                }
            }


            statements.Add(SwitchSection(
                    SingletonList((SwitchLabelSyntax)DefaultSwitchLabel())
                    ,
                    SingletonList((StatementSyntax)BreakStatement())
                )
            );

            SwitchStatementSyntax switchStatement = SwitchStatement(
                SyntaxFactory.ParseExpression("reader.GetString()"),
                new SyntaxList<SwitchSectionSyntax>(statements)
             );
            
            return new SyntaxList<StatementSyntax>(
                new SyntaxList<StatementSyntax>(
                    new List<StatementSyntax>() {
                         switchStatement,
                         BreakStatement()
                   }
                )
             );
        }

        private bool IsArrayType(string typeName)
        {
            Regex arrayPattern = new Regex("[a-zA-Z0-9][[][]]");
            return arrayPattern.IsMatch(typeName);
        }
        private string ArraySubtype(string typeName)
        {
            return typeName.Substring(0, typeName.Length - 2); // remove trailing []
        }


        private bool IsListType(string typeName)
        {
            return typeName.StartsWith("List<");
        }
        private string ListSubtype(string typeName)
        {
            return typeName.Substring(5, typeName.Length - 6); // remove Lit< and trailing >
        }

        private bool IsDictionaryType(string typeName)
        {
            Regex pattern = new Regex("Dictionary<.*>");
            return pattern.IsMatch(typeName);
        }
        private string DictionaryKeyType(string typeName)
        {
            string typeParts = typeName.Replace("Dictionary<", "");
            typeParts = typeParts.Substring(0, typeParts.Length - 1);
            string[] parts = typeParts.Split(',');
            return parts[0].Trim();
        }
        private string DictionaryValueType(string typeName)
        {
            string typeParts = typeName.Replace("Dictionary<", "");
            if (typeParts.EndsWith(">?"))
            {
                typeParts = typeParts.Substring(0, typeParts.Length - 2);
            }
            else
            {
                typeParts = typeParts.Substring(0, typeParts.Length - 1);
            }
            string[] parts = typeParts.Split(',');
            return parts[1].Trim();
        }

        private bool IsObjectType(string typeName)
        {
            if (IsDictionaryType(typeName)) return false;
            if (IsListType(typeName)) return false;
            if (IsArrayType(typeName)) return false;
            switch (typeName)
            {
                case "int": return false;
                case "int?": return false;
                case "float": return false;
                case "float?": return false;
                case "double": return false;
                case "double?": return false;
                case "decimal": return false;
                case "decimal?": return false;
                case "DateTime": return false;
                case "DateTime?": return false;
                case "string": return false;
                case "String": return false;
                case "string?": return false;
                case "String?": return false;
                default: return true;
            };            
        }

        private string ObjectType(string typeName)
        {
            return typeName.Replace("?",""); 
        }


        Dictionary<string, MethodDeclarationSyntax> GetListMethods = new Dictionary<string, MethodDeclarationSyntax>();

        Dictionary<string, MethodDeclarationSyntax> GetArrayMethods = new Dictionary<string, MethodDeclarationSyntax>();

        Dictionary<string, MethodDeclarationSyntax> GetDictionaryMethods = new Dictionary<string, MethodDeclarationSyntax>();

        Dictionary<string, MethodDeclarationSyntax> WriteObjectMethods = new Dictionary<string, MethodDeclarationSyntax>();

        Dictionary<string, MethodDeclarationSyntax> WriteDictionaryMethods = new Dictionary<string, MethodDeclarationSyntax>();

        Dictionary<string, MethodDeclarationSyntax> GetObjectMethods = new Dictionary<string, MethodDeclarationSyntax>();

        private SyntaxList<StatementSyntax> CreateParameterReadStatements(string parameterName, string typeName)
        {
            switch (GetTypeNameType(typeName))
            {
                case TypeNameType.ObjectType:
                    {
                        string objType = ObjectType(typeName);
                        if (!GetObjectMethods.ContainsKey(objType))
                        {
                            GetObjectMethods.Add(objType, CreateGetObjectMethod(objType));
                        }
                        return CreateObjectReadStatements(parameterName, objType);

                    }
                case TypeNameType.ListType:
                    {
                        string listType = ListSubtype(typeName);
                        if (!GetListMethods.ContainsKey(listType))
                        {
                            GetListMethods.Add(listType, CreateGetListMethod(listType));
                        }
                        return CreateListReadStatements(parameterName, listType);
                    }
                case TypeNameType.ArrayType:
                    {
                        string listType = ArraySubtype(typeName);
                        if (!GetArrayMethods.ContainsKey(listType))
                        {
                            GetArrayMethods.Add(listType, CreateGetArrayMethod(listType));
                        }
                        return CreateArrayReadStatements(parameterName, listType);
                    }
                case TypeNameType.DictionaryType:
                    {
                        string keyType = DictionaryKeyType(typeName);
                        string valueType = DictionaryValueType(typeName);
                        if (!GetDictionaryMethods.ContainsKey(valueType))
                        {
                            GetDictionaryMethods.Add(valueType, CreateGetDictionaryMethod(valueType));
                        }
                        return CreateDictionaryReadStatements(parameterName, keyType, valueType);
                    }
                case TypeNameType.DateTimeType:
                    return CreateDateTimeReadStatements(parameterName, typeName);
                default:
                    return CreateSimpleParameterReadStatements(parameterName, typeName);
            }
        }


        private string ReadListMethodName(string typeName)
        {
            return $"ReadList{CapitilizeFirstChar(typeName)}";
        }
        private string ReadDictionaryMethodName(string typeName, string keyType)
        {
            return $"ReadDictionary{CapitilizeFirstChar(typeName)}<{keyType}>";
        }

        private string ReadArrayMethodName(string typeName)
        {
            return $"ReadArray{CapitilizeFirstChar(typeName)}";
        }

        private MethodDeclarationSyntax CreateWriteObjectMethod(string objType)
        {
            return MethodDeclaration(ParseTypeName("void"),
                "Write" + objType)
                .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  ))
                .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("writer")).WithType(IdentifierName("Utf8JsonWriter")),
                    Parameter(Identifier("propertyName")).WithType(IdentifierName("string")),
                    Parameter(Identifier("value")).WithType(IdentifierName(objType)),
                    Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions"))
                  }
                )))
                .WithBody(Block(ParseStatement(@"writer.WritePropertyName(propertyName);").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"{objType}JsonConverter converter = new {objType}JsonConverter();").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement(@"converter.Write(writer, value, options);").WithTrailingTrivia(ElasticCarriageReturnLineFeed)
            ));
        }


        private MethodDeclarationSyntax CreateWriteDictionaryMethod()
        {
            return MethodDeclaration(ParseTypeName("void"),
                "WriteDictionary<KT,VT>")
                .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  ))
                .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("writer")).WithType(IdentifierName("Utf8JsonWriter")),
                    Parameter(Identifier("propertyName")).WithType(IdentifierName("string")),
                    Parameter(Identifier("dict")).WithType(IdentifierName("Dictionary<KT,VT>")),
                    Parameter(Identifier("writeKeyValuePair")).WithType(IdentifierName("Action<KT,VT>"))
                  }
                )))
                .WithConstraintClauses(
                new SyntaxList<TypeParameterConstraintClauseSyntax>(
                        new List<TypeParameterConstraintClauseSyntax>()
                        {
                           TypeParameterConstraintClause(IdentifierName("KT"),
                           SingletonSeparatedList<TypeParameterConstraintSyntax>(TypeConstraint(IdentifierName("notnull"))))
                        }
                    )
                )
                .WithBody(Block(ParseStatement(@"writer.WritePropertyName(propertyName);").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement(@"writer.WriteStartObject();").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement(@"foreach (KT key in dict.Keys)
            {
                writeKeyValuePair(key,dict[key]);
            }").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement(@"writer.WriteEndObject();").WithTrailingTrivia(ElasticCarriageReturnLineFeed)
            ));
        }

        private MethodDeclarationSyntax CreateGetObjectMethod(string objType)
        {
            return MethodDeclaration(
                 ParseTypeName(objType),
                  "Read" + objType)
              .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  )
              )
              .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader")),
                    Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions"))
                  }
              )))
              .WithBody(Block(ParseStatement($@"{objType}JsonConverter converter = new {objType}JsonConverter();").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"return converter.Read(ref reader, typeof({objType}), options);").WithTrailingTrivia(ElasticCarriageReturnLineFeed).WithLeadingTrivia(Whitespace("            "))
            ));
        }


        private MethodDeclarationSyntax CreateGetListMethod(string listType)
        {

            return MethodDeclaration(
                 ParseTypeName("List<" + listType + ">?"),
                  ReadListMethodName(listType))
              .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  )
              )
              .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader"))
                  }
              )))
              .WithBody(Block(ParseStatement($@" bool inArray = true;").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"List<{listType}>? someList = null;").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"while (inArray)
            {{
                reader.Read();
                switch (reader.TokenType)
                {{
                    case JsonTokenType.StartArray:
                        someList = new List<{listType}>();
                        break;
                    case JsonTokenType.EndArray:
                        inArray = false;
                        break;
                    default:
                        someList?.Add(reader.{GetTypeTranslation(listType)}());
                        break;
                }}
            }}").WithTrailingTrivia(CarriageReturnLineFeed),
            ParseStatement($@"return someList;").WithTrailingTrivia(ElasticCarriageReturnLineFeed).WithLeadingTrivia(Whitespace("            "))
            ));      
        }

        private MethodDeclarationSyntax CreateGetArrayMethod(string listType)
        {

            return MethodDeclaration(
                 ParseTypeName(listType + "[]?"),
                  ReadArrayMethodName(listType))
              .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  )
              )
              .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader"))
                  }
              )))
              .WithBody(Block(ParseStatement(@" bool inArray = true;").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"List<{listType}>? someList = null;").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"while (inArray)
            {{
                reader.Read();
                switch (reader.TokenType)
                {{
                    case JsonTokenType.StartArray:
                        someList = new List<{listType}>();
                        break;
                    case JsonTokenType.EndArray:
                        inArray = false;
                        break;
                    default:
                        someList?.Add(reader.{GetTypeTranslation(listType)}());
                        break;
                }}
            }}").WithTrailingTrivia(CarriageReturnLineFeed),
            ParseStatement("return someList?.ToArray();").WithTrailingTrivia(ElasticCarriageReturnLineFeed).WithLeadingTrivia(Whitespace("            "))
            ));
        }

        private MethodDeclarationSyntax CreateGetDictionaryMethod(string valueType)
        {

            return MethodDeclaration(
                 ParseTypeName("Dictionary<T1,"  + valueType + ">"),
                  ReadDictionaryMethodName(valueType,"T1"))
              .WithModifiers(TokenList(
                  SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                  SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                  )
              )
              .WithParameterList(ParameterList(SeparatedList(
                  new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader")),
                    Parameter(Identifier("stringToKeyType")).WithType(IdentifierName("Func<string,T1>"))
                  }
              )))
              .WithConstraintClauses(
                new SyntaxList<TypeParameterConstraintClauseSyntax>(
                        new List<TypeParameterConstraintClauseSyntax>()
                        {
                           TypeParameterConstraintClause(IdentifierName("T1"),
                           SingletonSeparatedList<TypeParameterConstraintSyntax>(TypeConstraint(IdentifierName("notnull"))))
                        }
                    )
                )
              .WithBody(Block(ParseStatement($@"Dictionary<T1, {valueType}> dict = new Dictionary<T1, {valueType}>();").WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            ParseStatement($@"while (true)
            {{
                reader.Read();
                switch (reader.TokenType)
                {{
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        return dict;
                    case JsonTokenType.PropertyName:
                        string keyString = reader.GetString();
                        T1 key = stringToKeyType(keyString);
                        reader.Read();
                        {valueType} value = reader.{GetTypeTranslation(valueType)}();
                        dict.Add(key, value);
                        break;
                }}
            }}").WithTrailingTrivia(CarriageReturnLineFeed)
            ));
        }

        private SyntaxList<StatementSyntax> CreateObjectReadStatements(string parameterName, string typeName)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(ParseStatement($"{parameterName} = Read{typeName}(reader, options);").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            statements.Add(BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            return new SyntaxList<StatementSyntax>(statements);
        }


        private SyntaxList<StatementSyntax> CreateListReadStatements(string parameterName, string typeName)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(ParseStatement($"{parameterName} = {ReadListMethodName(typeName)}(reader);").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            statements.Add(BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            return new SyntaxList<StatementSyntax>(statements);
        }

        private SyntaxList<StatementSyntax> CreateArrayReadStatements(string parameterName, string typeName)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(ParseStatement($"{parameterName} = {ReadArrayMethodName(typeName)}(reader);").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            statements.Add(BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            return new SyntaxList<StatementSyntax>(statements);
        }

        private SyntaxList<StatementSyntax> CreateDictionaryReadStatements(string parameterName, string keyType, string valueType)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(ParseStatement($"{parameterName} = {ReadDictionaryMethodName(valueType,keyType)}(reader, s => {GetStringToTypeMethod(keyType,"s")});").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            statements.Add(BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed));
            return new SyntaxList<StatementSyntax>(statements);
        }


        private SyntaxList<StatementSyntax> CreateDateTimeReadStatements(string parameterName, string typeName)
        {
            return new SyntaxList<StatementSyntax>(new List<StatementSyntax>()
                            {
                                ParseStatement("reader.Read();").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                ParseStatement($"{parameterName} = DateTime.Parse(reader.GetString());").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed)
                            });
        }


        private SyntaxList<StatementSyntax> CreateSimpleParameterReadStatements(string parameterName, string typeName)
        {
            return new SyntaxList<StatementSyntax>(new List<StatementSyntax>()
                            {
                                ParseStatement("reader.Read();").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                ParseStatement($"{parameterName} = reader.{GetTypeTranslation(typeName)}();").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed)
                            });
        }

        private string GetStringToTypeMethod(string typeName, string stringParameterName)
        {
            switch (typeName)
            {
                case "int" : return $"int.Parse({stringParameterName})";
                case "int?" : return $"int.Parse({stringParameterName})";
                case "string" : return stringParameterName;
                case "string?" : return stringParameterName;
                case "String" : return stringParameterName;
                case "String?" : return stringParameterName;
                case "double" : return $"double.Parse({stringParameterName})";
                case "double?" : return $"double.Parse({stringParameterName})";
                case "decimal": return $"decimal.Parse({stringParameterName})";
                case "decimal?": return $"decimal.Parse({stringParameterName})";
                case "float" : return $"float.Parse({stringParameterName})";
                case "float?" : return $"float.Parse({stringParameterName})";
                case "DateTime?" : return $"DateTime.Parse({stringParameterName})";
                case "DateTime" : return $"DateTime.Parse({stringParameterName})";
                    default: return stringParameterName;
            };
        }


        private string GetTypeTranslation(string getTypeName)
        {
            switch (getTypeName)
            {
                case "int" : return "GetInt32";
                case "int?": return "GetInt32";
                case "string": return "GetString";
                case "string?": return "GetString";
                case "String": return "GetString";
                case "String?": return "GetString";
                case "double": return "GetDouble";
                case "double?": return "GetDouble";
                case "decimal": return "GetDecimal";
                case "decimal?": return "GetDecimal";
                case "float": return "GetFloat";
                case "float?": return "GetFloat";
                case "DateTime?": return "GetString";
                case "DateTime": return "GetString";
                default : return "Get" + CapitilizeFirstChar(getTypeName);
            };
        }

        private object CapitilizeFirstChar(string typeName)
        {
            if (typeName.Length == 0) return "";
            if (typeName.Length == 1) return typeName.ToUpperInvariant();
            
            return typeName.Substring(0, 1).ToUpperInvariant() + typeName.Substring(1);
            
        }
    }
}
