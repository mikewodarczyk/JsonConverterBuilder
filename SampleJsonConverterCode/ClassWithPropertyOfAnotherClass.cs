using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassWithPropertyOfAnotherClassJsonConverter))]
    public class ClassWithPropertyOfAnotherClass
    {
        public EmptyClassC AnotherClassObj { get; }
        public EmptyClassC? MaybeAnotherClassObj { get; }

        public ClassWithPropertyOfAnotherClass(EmptyClassC anotherClassObj, EmptyClassC? maybeAnotherClassObj)
        {
            AnotherClassObj = anotherClassObj;
            MaybeAnotherClassObj = maybeAnotherClassObj;
        }
    }

    public class ClassWithPropertyOfAnotherClassJsonConverter : JsonConverter<ClassWithPropertyOfAnotherClass>
    {
        public override ClassWithPropertyOfAnotherClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            EmptyClassC? AnotherClassObj = null;
            EmptyClassC? MaybeAnotherClassObj = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (AnotherClassObj == null) throw new JsonException("ClassWithPropertyOfAnotherClass is missing property AnotherClassObj");
                        return new ClassWithPropertyOfAnotherClass(AnotherClassObj, MaybeAnotherClassObj);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithPropertyOfAnotherClass.AnotherClassObj):
                                AnotherClassObj = ReadEmptyClassC(reader, options);
                                break;
                            case nameof(ClassWithPropertyOfAnotherClass.MaybeAnotherClassObj):
                                MaybeAnotherClassObj = ReadEmptyClassC(reader, options);
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

        private static EmptyClassC ReadEmptyClassC(Utf8JsonReader reader, JsonSerializerOptions options)
        {
            EmptyClassCJsonConverter converter = new EmptyClassCJsonConverter();
            return converter.Read(ref reader, typeof(EmptyClassC), options);
        }

        public override void Write(Utf8JsonWriter writer, ClassWithPropertyOfAnotherClass value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteEmptyClassC(writer, nameof(ClassWithPropertyOfAnotherClass.AnotherClassObj), value.AnotherClassObj, options);
            if (value.MaybeAnotherClassObj != null)
            {
                WriteEmptyClassC(writer, nameof(ClassWithPropertyOfAnotherClass.MaybeAnotherClassObj), value.MaybeAnotherClassObj, options);
            }

            writer.WriteEndObject();
        }

        private static void WriteEmptyClassC(Utf8JsonWriter writer, string propertyName, EmptyClassC value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(propertyName);
            EmptyClassCJsonConverter converter = new EmptyClassCJsonConverter();
            converter.Write(writer, value, options);
        }
    }
}
