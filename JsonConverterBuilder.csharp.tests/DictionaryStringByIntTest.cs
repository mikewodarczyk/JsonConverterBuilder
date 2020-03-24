using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class DictionaryStringByIntTest : CodeRefactoringProviderTestFixture
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
    public class [|ClassWithDictionaryOfStringsByInt|]
    {
        public Dictionary<int, string> SomeStringsByInt { get; }
        public Dictionary<int, string>? MaybeSomeStringsByInt { get; }

        public ClassWithDictionaryOfStringsByInt(Dictionary<int, string> someStringsByInt, Dictionary<int, string>? maybeSomeStringsByInt)
        {
            SomeStringsByInt = someStringsByInt;
            MaybeSomeStringsByInt = maybeSomeStringsByInt;
        }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassWithDictOfStringsByInt.cs");

            Test(initialCode, expectedCode);
        }
    }
}
