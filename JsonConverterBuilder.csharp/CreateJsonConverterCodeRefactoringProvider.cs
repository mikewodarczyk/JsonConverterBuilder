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

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (token.Kind() == SyntaxKind.IdentifierToken
                && token.Parent.Kind() == SyntaxKind.ClassDeclaration )
            {
                string className = token.Text;
                CreateJsonConverterCodeAction action = new CreateJsonConverterCodeAction($"Create JsonConverter for {className}",
                    (c) => Task.FromResult(CreateJsonConverterForClass(document, semanticModel, token, c)));                
                context.RegisterRefactoring(action);
            }
        }

        private Document CreateJsonConverterForClass(Document document,
                                            SemanticModel semanticModel,
                                            SyntaxToken token,                                            
                                            CancellationToken cancellationToken)
        {
            string className = token.Text;
            CompilationUnitSyntax oldRoot = (CompilationUnitSyntax)semanticModel.SyntaxTree.GetRoot();
            //CompilationUnitSyntax newRoot = oldRoot.AddUsingsIfMissing(token, "System.Text.Json")
            //                                       .AddUsingsIfMissing(token, "System.Text.Json.Serialization");
                                                   // .AddAttributeForJsonConverter(token, className);

            AddJsonAttributeConverterSyntaxRewrier rewriter = new AddJsonAttributeConverterSyntaxRewrier(className,semanticModel);
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

        public AddJsonAttributeConverterSyntaxRewrier(string className, SemanticModel semanticModel)
        {
            ClassName = className;
            SemanticModel = semanticModel;
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

            ClassDeclarationSyntax classDec = ClassDeclaration(ClassName + "JsonConverter")
                .WithLeadingTrivia(CarriageReturnLineFeed)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(
                    new List<BaseTypeSyntax>() {
                    SimpleBaseType(SyntaxFactory.ParseTypeName("JsonConverter<"+ClassName+">"))
                    }
                )));

            classDec = classDec.AddMembers(
                MethodDeclaration(IdentifierName(ClassName), "Read")
                .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("reader")).WithType(IdentifierName("Utf8JsonReader"))
                    .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                    ,
                    Parameter(Identifier("typeToConvert")).WithType(IdentifierName("Type"))
                    ,
                    Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions"))
                    }
                )))
                .WithBody(
                    Block(

                        WhileStatement(SyntaxFactory.ParseExpression("true"),
                        Block(
                            SyntaxFactory.ParseStatement("reader.Read();"),
                           SwitchStatement(SyntaxFactory.ParseExpression("reader.SyntaxTokenType"),
                            SwitchSection(
                                DefaultSwitchLabel,BreakStatement())
                            // SwitchSection(
                            //    new  SyntaxList<SwitchLabelSyntax>(
                            //     new List<SwitchLabelSyntax>()
                            // {
                            //     DefaultSwitchLabel()
                            // })
                            // ,
                            // new SyntaxList<StatementSyntax>()
                            //)
                            ))

                        )
                )
            ); ;

            /*
             *  
            while(true)
            {
                reader.Read();
                switch(reader.TokenType)
                {
                    case JsonTokenType.StartObject: break;
                    case JsonTokenType.EndObject:
                        return new C();
                    case JsonTokenType.PropertyName:
                        switch(reader.GetString())
                        {
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
             */


            return base.VisitNamespaceDeclaration(  
                node.AddMembers(classDec)                
            );
        }

    }
}
