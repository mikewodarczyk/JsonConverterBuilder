﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class ListOfIntsTest : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;
    

        [Fact]
        public void JsonConverterListOfIntsTest()
        {
            string initialCode =
@"using System;
using System.Collections.Generic;

#nullable enable

namespace SampleJsonConverterCode
{
    public class [|ClassWithListOfInts|]
    {
        public ClassWithListOfInts(List<int> someInts)
        {
            SomeInts = someInts ?? throw new ArgumentNullException(nameof(someInts));
        }

        public List<int> SomeInts { get; }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassWithListsOfInts.cs");

            Test(initialCode, expectedCode);
        }

    }
}
