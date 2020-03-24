using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class ClassWithPropertyOfAnotherClassTest : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;


        [Fact]
        public void JsonConverterArrayOfStringsTest()
        {
            string initialCode =
@"using System;

#nullable enable

namespace SampleJsonConverterCode
{
    public class [|ClassWithPropertyOfAnotherClass|]
    {
        public EmptyClassC AnotherClassObj { get; }
        public EmptyClassC? MaybeAnotherClassObj { get; }

        public ClassWithPropertyOfAnotherClass(EmptyClassC anotherClassObj, EmptyClassC? maybeAnotherClassObj)
        {
            AnotherClassObj = anotherClassObj;
            MaybeAnotherClassObj = maybeAnotherClassObj;
        }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassWithPropertyOfAnotherClass.cs");

            Test(initialCode, expectedCode);
        }
    }
}
