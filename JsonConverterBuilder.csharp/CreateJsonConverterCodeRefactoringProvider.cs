﻿using Microsoft.CodeAnalysis;
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

    //public static class CompilationUnitSyntaxExtensions
    //{
    //    public static CompilationUnitSyntax AddNewUsing(this CompilationUnitSyntax root, SyntaxToken token, string usingNamespace)
    //    {
    //        var name = IdentifierName(usingNamespace);
    //        return root.AddUsings(UsingDirective(name).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
    //    }

    //    public static bool MissingUsingToken(this CompilationUnitSyntax root, string usingNamespace)
    //    {          
    //        return !root.Usings.Any(x => x.Name.ToString() == usingNamespace);
    //    }


    //    public static CompilationUnitSyntax AddUsingsIfMissing(this CompilationUnitSyntax root, SyntaxToken token, string usingNamespace)
    //    {
    //        if (root.MissingUsingToken(usingNamespace))
    //        {
    //            return root.AddNewUsing(token, usingNamespace);               
    //        }
    //        return root;
    //    }

    //    //public static CompilationUnitSyntax AddAttributeForJsonConverter(this CompilationUnitSyntax root, SyntaxToken token , string className)
    //    //{           
    //    //    if (token != null )
    //    //    {
    //    //        ClassDeclarationSyntax classDec = token.Parent as ClassDeclarationSyntax;
    //    //        if ( classDec.ChildTokens().All(n => 
    //    //        n == null ||
    //    //        n.Kind() != SyntaxKind.IdentifierName ||
    //    //        n.Text == null ||
    //    //        n.Text != "JsonConverter" ||
    //    //        n.Parent == null || 
    //    //        n.Parent.Kind() != SyntaxKind.Attribute))
    //    //        {
    //    //            AttributeListSyntax newList = AttributeList().AddAttributes(
    //    //                Attribute(IdentifierName("JsonConverter"),
    //    //                AttributeArgumentList(
    //    //                    SeparatedList<AttributeArgumentSyntax>(
    //    //                        new List<AttributeArgumentSyntax>() {
    //    //                            AttributeArgument(
    //    //                                TypeOfExpression(
    //    //                                    IdentifierName(Identifier(className + "JsonConverter")))
    //    //                                )
    //    //                            }                                    
    //    //                        )
    //    //                    )));


    //    //            SyntaxNode classDecWithAttribute = classDec.AddAttributeLists(newList);
    //    //            SyntaxNode origNs = classDec.Parent;
    //    //            SyntaxNode ns = origNs.ReplaceNode(classDec, classDecWithAttribute);
    //    //            bool wasItThere = root.Contains(origNs);

    //    //            return root.ReplaceNode(origNs, ns);                    
    //    //        }
    //    //    }
    //    //    return root;
    //    //}
    //}


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

            return classDec.AddMembers(CreateReadMethod())
                .AddMembers(CreateWriteMethod());
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

                        if (NeedsValue(typeName))
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

        private bool NeedsValue(string t)
        {
            if (t == "string") return false;
            if (t == "string?") return false;
            return true;
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
                        .WithTrailingTrivia(CarriageReturnLineFeed)
                        .WithTrailingTrivia(ElasticSpace));
                }
            }

            block = block.AddStatements(whileStatement);
            return block;
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
              .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));

            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            foreach (var property in properties)
            {
                var si = SemanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
                if (si.DeclaredAccessibility == Accessibility.Public)
                {
                    var n = si.Name;
                    var typeName = si.Type.ToString();
                    var suffix = NeedsValue(typeName) ? ".Value" : "";

                    if (si.Type.ToString().EndsWith("?"))
                    {
                        var ifStatement = IfStatement(
                            ParseExpression($"value.{n} != null")
                            ,
                            ParseStatement($"writer.{WriteMethod(typeName)}(nameof({ClassName}.{n}),value.{n}{suffix});").WithLeadingTrivia(ElasticCarriageReturnLineFeed)                            );
                        statements.Add(ifStatement);
                    }
                    else
                    {
                        statements.Add(ParseStatement($"writer.{WriteMethod(typeName)}(nameof({ClassName}.{n}),value.{n});").WithLeadingTrivia(ElasticCarriageReturnLineFeed));
                    }
                }
            }

            statements.Add(SyntaxFactory.ParseStatement("writer.WriteEndObject();").WithLeadingTrivia(ElasticSpace));
            return statements.ToArray();
        }

        private string WriteMethod(string typeName)
        {
            return typeName switch
            {
                "int" => "WriteNumber",
                "int?" => "WriteNumber",
                "float" => "WriteNumber",
                "float?" => "WriteNumber",
                "double" => "WriteNumber",
                "double?" => "WriteNumber",
                "decimal" => "WriteNumber",
                "decimal?" => "WriteNumber",
                "string" => "WriteString",
                "string?" => "WriteString",
                "String" => "WriteString",
                "String?" => "WriteString",
                _ => "Write" + CapitilizeFirstChar(typeName)
            };
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
                            new SyntaxList<StatementSyntax>(new List<StatementSyntax>()
                            {
                                ParseStatement("reader.Read();").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                ParseStatement($"{n} = reader.{GetTypeTranslation(typeName)}();").WithLeadingTrivia(ElasticCarriageReturnLineFeed),
                                BreakStatement().WithLeadingTrivia(ElasticCarriageReturnLineFeed)
                            })
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

        private string GetTypeTranslation(string getTypeName)
        {
            return getTypeName switch
            {
                "int" => "GetInt32",
                "int?" => "GetInt32",
                "string" => "GetString",
                "string?" => "GetString",
                "String" => "GetString",
                "String?" => "GetString",
                "double" => "GetDouble",
                "double?" => "GetDouble",
                "float" => "GetFloat",
                "float?" => "GetFloat",
                _ => "Get" + CapitilizeFirstChar(getTypeName)
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
