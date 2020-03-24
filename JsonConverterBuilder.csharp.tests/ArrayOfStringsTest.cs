using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class ArrayOfStringsTest : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;
    

        [Fact]
        public void JsonConverterArrayOfStringsTest()
        {
            string initialCode =
@"using System;
using System.Collections.Generic;

#nullable enable

namespace SampleJsonConverterCode
{
    public class [|ClassWithArrayOfStrings|]
    {
        public ClassWithArrayOfStrings(string[] someStrings)
        {
            SomeStrings = someStrings ?? throw new ArgumentNullException(nameof(someStrings));
        }

        public string[] SomeStrings { get; }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassWithArrayOfString.cs");

            Test(initialCode, expectedCode);
        }

    }
}
