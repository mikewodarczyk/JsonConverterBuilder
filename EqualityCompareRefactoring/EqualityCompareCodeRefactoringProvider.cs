using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace EqualityCompareRefactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(EqualityCompareCodeRefactoringProvider)), Shared]


    public class EqualityCompareCodeRefactoringProvider : CodeRefactoringProvider
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
                && token.Parent.Kind() == SyntaxKind.ClassDeclaration)
            {
                string className = token.Text;

                CreateEqualityCompareCodeAction action = new CreateEqualityCompareCodeAction($"Create EqualityCompare for {className}",
                    (c) => Task.FromResult(CreateEqualityCompareConverterForClass(document, semanticModel, token, root, c)));
                context.RegisterRefactoring(action);
            }
        }

        private Document CreateEqualityCompareConverterForClass(Document document,
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

            AddEqualotyCompareSyntaxRewrier rewriter = new AddEqualotyCompareSyntaxRewrier(className, semanticModel, root, token);
            SyntaxNode newSource = rewriter.Visit(oldRoot);

            return document.WithSyntaxRoot(newSource);
        }

        private class CreateEqualityCompareCodeAction : CodeAction
        {
            private readonly string title;
            private readonly Func<CancellationToken, Task<Document>> createChangedDocument;

            public CreateEqualityCompareCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
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


    public class AddEqualotyCompareSyntaxRewrier : CSharpSyntaxRewriter
    {
        public readonly string ClassName;
        private readonly SemanticModel SemanticModel;
        private readonly CompilationUnitSyntax OriginalRoot;
        private readonly SyntaxToken Token;

        public AddEqualotyCompareSyntaxRewrier(string className, SemanticModel semanticModel,
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
            return base.VisitCompilationUnit(result);
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            NamespaceDeclarationSyntax result = node;
            if (!result.ContainsDirectives)
            {
                //result = result.WithNamespaceKeyword(
                //    result.NamespaceKeyword.WithLeadingTrivia(
                //        result.NamespaceKeyword.LeadingTrivia.Add(
                //               NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true))
                //         )
                //     )
                // );

            }
            else
            {
                var trivia = result.NamespaceKeyword.LeadingTrivia;

                foreach (SyntaxTrivia st in trivia)
                {
                    var x = st;
                }
            }
            return base.VisitNamespaceDeclaration(result);
        }



        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.Text == ClassName)
            {
                ClassDeclarationSyntax nodeModified = node.WithBaseList(
                            (node.BaseList ?? BaseList().WithoutLeadingTrivia()).AddTypes(
                                SimpleBaseType(IdentifierName(Identifier($"IEquatable<{ClassName}>")).WithoutLeadingTrivia()
                            ))
                    ).WithIdentifier(node.Identifier.WithoutTrivia());

                nodeModified = nodeModified.WithMembers(
                    node.Members.Add(CreateEqualsOperator(ClassName, node))
                    .Add(CreateEqualsObjOperator(ClassName))
                    .Add(CreateOperatorEqualsOperator(ClassName))
                    .Add(CreateOperatorNotEqualsOperator(ClassName))
                     .Add(CreateGetHashCode(node))
                    );

                return base.VisitClassDeclaration(
                    nodeModified
                );
            }
            else
            {
                return base.VisitClassDeclaration(node);
            }
        }

        private MemberDeclarationSyntax CreateEqualsOperator(string className, ClassDeclarationSyntax node)
        {
            return MethodDeclaration(IdentifierName("bool"), Identifier("Equals"))
                 .WithModifiers(TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("other")).WithType(IdentifierName(className + "?"))
                    }
                )))
                .WithBody(
                    Block()
                        .AddStatements(ParseStatement("if (this is null) return other is null;").WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                        .AddStatements(
                        ReturnStatement(
                            BinaryExpression(SyntaxKind.LogicalAndExpression,
                               BinaryExpression(SyntaxKind.IsExpression, IdentifierName("other"), IdentifierName(ClassName)),
                               CreateComparisonOfAllProperties(node)
                               )
                            )

                        // ParseStatement($"return other is {ClassName} && true;")
                        )
                );
        }

        private ExpressionSyntax CreateComparisonOfAllProperties(ClassDeclarationSyntax node)
        {
            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            if (properties.Count == 0) {
                return LiteralExpression(SyntaxKind.TrueLiteralExpression);
            }
            else
            {
                ExpressionSyntax result = null;
                foreach (var prop in properties)
                {
                    string propertyName = SemanticModel.GetDeclaredSymbol(prop).Name;
                    if (result == null)
                    {
                        result = BinaryExpression(SyntaxKind.EqualsExpression,
                            IdentifierName(propertyName),
                            IdentifierName("other." + propertyName)).WithLeadingTrivia(CarriageReturnLineFeed);
                    }
                    else
                    {
                        result = BinaryExpression(SyntaxKind.LogicalAndExpression,
                            result,
                            BinaryExpression(SyntaxKind.EqualsExpression,
                           IdentifierName(propertyName),
                           IdentifierName("other." + propertyName)).WithLeadingTrivia(CarriageReturnLineFeed)
                           );
                    }
                }
                return result;
            }
        }

        private MemberDeclarationSyntax CreateEqualsObjOperator(string className)
        {
            return MethodDeclaration(IdentifierName("bool"), Identifier("Equals"))
                 .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("other")).WithType(IdentifierName("object?"))
                    }
                )))
                .WithBody(
                    Block()
                        .AddStatements(ParseStatement("if (this is null) return other is null;").WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                        .AddStatements(ParseStatement($"return other is {ClassName} variables && Equals(variables);"))
                );
        }

        private MemberDeclarationSyntax CreateOperatorEqualsOperator(string className)
        {
            return MethodDeclaration(IdentifierName("bool"), Identifier("operator=="))
                 .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("a")).WithType(IdentifierName(ClassName + "?")),
                    Parameter(Identifier("b")).WithType(IdentifierName(ClassName + "?"))
                    }
                )))
                .WithBody(
                    Block()
                        .AddStatements(ParseStatement("if (a is null) return b is null;").WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                        .AddStatements(ParseStatement($"return b is object && a.Equals(b);"))
                );
        }

        private MemberDeclarationSyntax CreateOperatorNotEqualsOperator(string className)
        {
            return MethodDeclaration(IdentifierName("bool"), Identifier("operator!="))
                 .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)
                    )
                )
                .WithParameterList(ParameterList(SeparatedList(
                    new List<ParameterSyntax>() {
                    Parameter(Identifier("a")).WithType(IdentifierName(ClassName + "?")),
                    Parameter(Identifier("b")).WithType(IdentifierName(ClassName + "?"))
                    }
                )))
                .WithBody(
                    Block()
                        .AddStatements(ParseStatement($"return !(a == b);"))
                );
        }

        private MemberDeclarationSyntax CreateGetHashCode(ClassDeclarationSyntax node)
        {
            return MethodDeclaration(IdentifierName("int"), Identifier("GetHashCode"))
                 .WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                    )
                )
                .WithBody(
                    Block()
                        .AddStatements(ReturnStatement(HashCodeExpression(node))
                ));
        }

        private ExpressionSyntax HashCodeExpression(ClassDeclarationSyntax node)
        {
            var properties = Token.Parent.DescendantNodes().Where(n => n.IsKind(SyntaxKind.PropertyDeclaration)).ToList();
            if (properties.Count == 0)
            {
                return ParseExpression("0");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("HashCode.Combine(");
                sb.Append(string.Join(", ", properties.Select(x => SemanticModel.GetDeclaredSymbol(x).Name)));
                sb.Append(")");
                return ParseExpression(sb.ToString());
            }
        }
    }
}

    
