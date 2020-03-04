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
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        return new C();
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
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


        [Fact]
        public void SimpleVariablesTest()
        {
            string initialCode =
@"using System;

namespace SampleJsonConverterCode
{
    public class [|SimpleVariables|]
    {
        public string Name { get; }
        public int SomeInt { get; }

        public SimpleVariables(string name, int someInt)
        {
            Name = name;
            SomeInt = someInt;
        }
    }
";

            string expectedCode =
@"using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(SimpleVariablesJsonConverter))]
    public class SimpleVariables
    {
        public string Name { get; }
        public int SomeInt { get; }

        public SimpleVariables(string name, int someInt)
        {
            Name = name;
            SomeInt = someInt;
        }
    }

    public class SimpleVariablesJsonConverter : JsonConverter<SimpleVariables>
    {
        public override SimpleVariables Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? Name = null;
            int? SomeInt = null;
            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (Name != null &&
                            SomeInt.HasValue)
                        {
                            return new SimpleVariables(Name, SomeInt.Value);
                        } 
                        else
                        {
                            throw new JsonException(""SimpleVariables missing property"");
                        }
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(SimpleVariables.Name):
                                reader.Read();
                                Name = reader.GetString();
                                break;
                            case nameof(SimpleVariables.SomeInt):
                                reader.Read();
                                SomeInt = reader.GetInt32();
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, SimpleVariables value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(SimpleVariables.Name), value.Name);
            writer.WriteNumber(nameof(SimpleVariables.SomeInt), value.SomeInt);
            writer.WriteEndObject();
        }
    }
}
";

            Test(initialCode, expectedCode);
        }



    }
}
