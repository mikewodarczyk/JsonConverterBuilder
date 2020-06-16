using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class JsonConverterBuilderTests : CodeRefactoringProviderTestFixture
    {     

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;

        [Fact]
        public void ReturnSimpleCase()
        {
            string initialCode = GetInitialCodeFileContents("EmptyClassC.cs");
            string expectedCode = GetExpectedResultFileContents("EmptyClassC.cs");

            Test(initialCode, expectedCode);
        }


        [Fact]
        public void SimpleVariablesTest()
        {
            string initialCode = GetInitialCodeFileContents("ClassWithSimpleVariables.cs");
            string expectedCode = GetExpectedResultFileContents("ClassWithSimpleVariables.cs");

            Test(initialCode, expectedCode);
        }




    }
}
