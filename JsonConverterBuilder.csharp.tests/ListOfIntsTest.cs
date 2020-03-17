using Microsoft.CodeAnalysis;
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

            string expectedCode =
@"using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassWithListOfIntsJsonConverter))]
    public class ClassWithListOfInts
    {
        public ClassWithListOfInts(List<int> someInts)
        {
            SomeInts = someInts ?? throw new ArgumentNullException(nameof(someInts));
        }

        public List<int> SomeInts { get; }
    }

    public class ClassWithListOfIntsJsonConverter : JsonConverter<ClassWithListOfInts>
    {
        public override ClassWithListOfInts Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<int>? SomeInts = null;
            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (SomeInts == null) throw new JsonException(""ClassWithListOfInts is missing property SomeInts"");
                        return new ClassWithListOfInts(SomeInts);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithListOfInts.SomeInts):
                                SomeInts = ReadListInt(reader);
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

        private static List<int>? ReadListInt(Utf8JsonReader reader)
        {
            bool inArray = true;
            List<int>? someList = null;
            while (inArray)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        someList = new List<int>();
                        break;
                    case JsonTokenType.EndArray:
                        inArray = false;
                        break;
                    default:
                        someList?.Add(reader.GetInt32());
                        break;
                }
            }
            return someList;
}

        public override void Write(Utf8JsonWriter writer, ClassWithListOfInts value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(ClassWithListOfInts.SomeInts));
            writer.WriteStartArray();
            foreach (int x in value.SomeInts)
            {
                writer.WriteNumberValue(x);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
";

            Test(initialCode, expectedCode);
        }

    }
}
