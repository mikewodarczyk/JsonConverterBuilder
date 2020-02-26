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
    class [|C|]
    {
    }
}
";

            string expectedCode =
@"using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(CJsonConverter))]
    class C
    {
    }

    public class CJsonConverter : JsonConverter<C>
    {
        public override C Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            while (true)
            {
                reader.Read();
                switch(reader.TokenType)
                {
                    case JsonTokenType.StartObject: break;
                    case JsonTokenType.EndObject:
                        return new C();
                    case JsonTokenType.PropertyName:
                        switch(reader.GetString())
                        {
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, C value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}
";

            Test(initialCode, expectedCode);
        }

    }
}
