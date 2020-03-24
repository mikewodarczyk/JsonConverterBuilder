using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class DictionaryIntByStringTest : CodeRefactoringProviderTestFixture
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
    public class [|ClassWithDictionaryOfIntsByString|]
    {
        public Dictionary<string, int> SomeIntsByString { get; }
        public Dictionary<string, int>? MaybeSomeIntsByString { get; }

        public ClassWithDictionaryOfIntsByString(Dictionary<string, int> someIntsByString, Dictionary<string, int>? maybeSomeIntsByString)
        {
            SomeIntsByString = someIntsByString;
            MaybeSomeIntsByString = maybeSomeIntsByString;
        }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassWithDictOfIntsByString.cs");

            Test(initialCode, expectedCode);
        }
    }
}
