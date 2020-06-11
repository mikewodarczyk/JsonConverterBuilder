﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;

namespace Roslyn.UnitTestFramework
{
    public abstract class CodeRefactoringProviderTestFixture : CodeActionProviderTestFixture
    {

        protected string GetInitialCodeFileContents(string shortFilename)
        {
            string path = "../../../../SampleCodeBeforeConversion/" + shortFilename;
            return System.IO.File.ReadAllText(path).Replace("/*[|*/","[|").Replace("/*|]*/","|]");
        }


        protected string GetExpectedResultFileContents(string shortFilename)
        {
            string path = "../../../../SampleJsonConverterCode/" + shortFilename;
            return System.IO.File.ReadAllText(path);
        }

        private IEnumerable<CodeAction> GetRefactoring(Document document, TextSpan span)
        {
            CodeRefactoringProvider provider = CreateCodeRefactoringProvider;
            List<CodeAction> actions = new List<CodeAction>();
            CodeRefactoringContext context = new CodeRefactoringContext(document, span, (a) => actions.Add(a), CancellationToken.None);
            provider.ComputeRefactoringsAsync(context).Wait();
            return actions;
        }

        protected void TestNoActions(string markup)
        {
            if (!markup.Contains('\r'))
            {
                markup = markup.Replace("\n", "\r\n");
            }

            MarkupTestFile.GetSpan(markup, out string code, out TextSpan span);

            Document document = CreateDocument(code);
            IEnumerable<CodeAction> actions = GetRefactoring(document, span);

            Assert.True(actions == null || actions.Count() == 0);
        }

        protected void Test(
            string markup,
            string expected,
            ITestOutputHelper output = null,
            int actionIndex = 0,
            bool compareTokens = false)
        {
            if (!markup.Contains('\r'))
            {
                markup = markup.Replace("\n", "\r\n");
            }

            if (!expected.Contains('\r'))
            {
                expected = expected.Replace("\n", "\r\n");
            }

            MarkupTestFile.GetSpan(markup, out string code, out TextSpan span);

            Document document = CreateDocument(code);
            IEnumerable<CodeAction> actions = GetRefactoring(document, span);

            Assert.NotNull(actions);

            CodeAction action = actions.ElementAt(actionIndex);
            Assert.NotNull(action);

            ApplyChangesOperation edit = action.GetOperationsAsync(CancellationToken.None).Result.OfType<ApplyChangesOperation>().First();
            VerifyDocument(expected, compareTokens, edit.ChangedSolution.GetDocument(document.Id),output);
        }

        protected abstract CodeRefactoringProvider CreateCodeRefactoringProvider { get; }
    }
}
