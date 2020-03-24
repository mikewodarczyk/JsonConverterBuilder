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
            string initialCode =
@"using System;

namespace SampleJsonConverterCode
{
    public class [|EmptyClassC|]
    {
    }
}
";

            string expectedCode = GetExpectedResultFileContents("EmptyClassC.cs");

            Test(initialCode, expectedCode);
        }


        [Fact]
        public void SimpleVariablesTest()
        {
            string initialCode =
@"using System;

#nullable enable

namespace SampleJsonConverterCode
{
    public class [|SimpleVariables|]
    {
        public string Name { get; }
        public int SomeInt { get; }
        public DateTime ADateTime { get; }
        public string? MaybeAName { get; }
        public int? MaybeAnInt { get; }
        public DateTime? MaybeADateTime { get; }

        public SimpleVariables(string name, int someInt, DateTime aDateTime, string? maybeAName, int? maybeAnInt, DateTime? maybeADateTime)
        {
            Name = name;
            SomeInt = someInt;
            ADateTime = aDateTime;
            MaybeAName = maybeAName;
            MaybeAnInt = maybeAnInt;
            MaybeADateTime = maybeADateTime;
        }
    }
}
";
            string expectedCode = GetExpectedResultFileContents("ClassWithSimpleVariables.cs");

            Test(initialCode, expectedCode);
        }




    }
}
