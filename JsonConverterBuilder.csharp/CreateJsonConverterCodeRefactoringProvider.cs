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
            CompilationUnitSyntax newRoot = oldRoot.AddUsingsIfMissing(token, "System.Text.Json")
                                                   .AddUsingsIfMissing(token, "System.Text.Json.Serialization")
                                                   .AddAttributeForJsonConverter(token, className);

            return document.WithSyntaxRoot(newRoot);
        }

      

        private SyntaxNode GetNamespaceNode(SyntaxToken token)
        {
            var searchToken = token.Parent;
            while (searchToken.Kind() != SyntaxKind.NamespaceDeclaration) { searchToken = searchToken.Parent; }
            return searchToken;
        }

        private SyntaxNode CreateUsingNode(string usingNamespace)
        {
            throw new NotImplementedException();
        }

       

     
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
    
    public static class CompilationUnitSyntaxExtensions
    {
        public static CompilationUnitSyntax AddNewUsing(this CompilationUnitSyntax root, SyntaxToken token, string usingNamespace)
        {
            var name = IdentifierName(usingNamespace);
            return root.AddUsings(UsingDirective(name).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
        }

        public static bool MissingUsingToken(this CompilationUnitSyntax root, string usingNamespace)
        {          
            return !root.Usings.Any(x => x.Name.ToString() == usingNamespace);
        }


        public static CompilationUnitSyntax AddUsingsIfMissing(this CompilationUnitSyntax root, SyntaxToken token, string usingNamespace)
        {
            if (root.MissingUsingToken(usingNamespace))
            {
                return root.AddNewUsing(token, usingNamespace);               
            }
            return root;
        }

        public static CompilationUnitSyntax AddAttributeForJsonConverter(this CompilationUnitSyntax root, SyntaxToken token , string className)
        {           
            if (token != null )
            {
                ClassDeclarationSyntax classDec = token.Parent as ClassDeclarationSyntax;
                if ( classDec.ChildTokens().All(n => 
                n == null ||
                n.Kind() != SyntaxKind.IdentifierName ||
                n.Text == null ||
                n.Text != "JsonConverter" ||
                n.Parent == null || 
                n.Parent.Kind() != SyntaxKind.Attribute))
                {
                    AttributeListSyntax newList = AttributeList().AddAttributes(
                        Attribute(IdentifierName("JsonConverter"),
                        AttributeArgumentList(
                            SeparatedList<AttributeArgumentSyntax>(
                                new List<AttributeArgumentSyntax>() {
                                    AttributeArgument(
                                        TypeOfExpression(
                                            IdentifierName(Identifier(className + "JsonConverter")))
                                        )
                                    }                                    
                                )
                            )));


                    SyntaxNode classDecWithAttribute = classDec.AddAttributeLists(newList);
                    return root.ReplaceNode(classDec, classDecWithAttribute);                    
                }
            }
            return root;
        }
    }
}
