using EqualityCompareRefactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using Xunit;

namespace EqualityCompareRefactor.csharp.tests
{
    public class EmptyClassCTest : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new EqualityCompareCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;

        [Fact]
        public void ReturnSimpleCase()
        {
            string initialCode = GetInitialCodeFileContents("EmptyClassC.cs");
            string expectedCode = GetExpectedResultFileContents("SampleEqualityCompareRefactoringOutput","EmptyClassC.cs");

            Test(initialCode, expectedCode);
        }

    }
}
