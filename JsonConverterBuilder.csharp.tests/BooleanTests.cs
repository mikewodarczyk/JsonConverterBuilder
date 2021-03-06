﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslyn.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JsonConverterBuilder.csharp.tests
{
    public class BooleanTests : CodeRefactoringProviderTestFixture
    {

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider => new CreateJsonConverterCodeRefactoringProvider();

        protected override string LanguageName => LanguageNames.CSharp;
    

        [Fact]
        public void BooleanTest()
        {
            string initialCode =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrBronners.PricingChangeSchema
{
    public class [|ClassWithBool|]
    {
        public ClassWithBool(bool value, bool? maybeBoolValue)
        {
            Value = value;
            MaybeBoolValue = maybeBoolValue;
        }

        public bool Value { get; set; }
        public bool? MaybeBoolValue { get; set; }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("BooleanSampleCode.cs");

            Test(initialCode, expectedCode);
        }


        [Fact]
        public void BoolAndBooleanTest()
        {
            string initialCode =
@"using System;

namespace SampleJsonConverterCode
{
    public class [|ClassContainingBoolean|]
    {
        public ClassContainingBoolean(bool aBoolVar, bool aBooleanVar)
        {
            this.ABoolVar = aBoolVar;
            this.ABooleanVar = aBooleanVar;
        }

        public bool ABoolVar { get;  }
        public Boolean ABooleanVar { get; }
    }
}
";

            string expectedCode = GetExpectedResultFileContents("ClassContainingBoolean.cs");

            Test(initialCode, expectedCode);
        }


    }
}
