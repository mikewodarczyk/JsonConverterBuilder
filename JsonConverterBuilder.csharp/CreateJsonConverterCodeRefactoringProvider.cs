using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            CompilationUnitSyntax newRoot = AddUsingsIfMissing(oldRoot, token, "System.Text.Json");

            return document.WithSyntaxRoot(newRoot);
        }

        private CompilationUnitSyntax AddUsingsIfMissing(CompilationUnitSyntax root, SyntaxToken token, string usingNamespace)
        {
            if (MissingUsingToken(root, usingNamespace))
            {
                // return AddUsings(root, token, usingNamespace);
                return root;
            }
            return root;
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

        private SyntaxNode AddUsings(SyntaxNode root, SyntaxToken token, string usingNaespace)
        {
            throw new NotImplementedException();
        }

        private bool MissingUsingToken(CompilationUnitSyntax root, string usingNamespace)
        {
            var ud = root.Usings[0];
            var firstChildToken = ud.ChildNodes().First();
            var tokens = ud.ChildNodes();



            return root.Usings.Any(x => x.Name.ToString() == usingNamespace);
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
}
