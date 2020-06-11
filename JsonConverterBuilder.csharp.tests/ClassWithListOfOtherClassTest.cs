using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JsonConverterBuilder.csharp.tests
{
    public class ClassWithListOfOtherClassTest : CodeRefactoringProviderTestFixture
    {
        private readonly ITestOutputHelper output;

        public ClassWithListOfOtherClassTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;


        [Fact]
        public void BooleanTest()
        {
            string initialCode = GetInitialCodeFileContents("ClassContainingListOfClass.cs");
            string expectedCode = GetExpectedResultFileContents("ClassContainingListOfClass.cs");
            Test(initialCode, expectedCode,output);
        }
    }
}

