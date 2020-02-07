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

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken token = root.FindToken(textSpan.Start);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (token.Kind() == SyntaxKind.ClassDeclaration)
            {
                string className = token.Text;
                CreateJsonConverterCodeAction action = new CreateJsonConverterCodeAction($"Create JsonConverter for {className}",
                    (c) => Task.FromResult(CreateJsonConverterForClass(document, semanticModel, className, c)));                
                context.RegisterRefactoring(action);
            }
        }

        private Document CreateJsonConverterForClass(Document document,
                                            SemanticModel semanticModel,
                                            string className,                                            
                                            CancellationToken cancellationToken)
        {
            SyntaxNode oldRoot = semanticModel.SyntaxTree.GetRoot();
            SyntaxNode newRoot = oldRoot;

            return document.WithSyntaxRoot(newRoot);
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
