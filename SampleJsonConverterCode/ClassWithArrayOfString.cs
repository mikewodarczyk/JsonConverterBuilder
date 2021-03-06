﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassWithArrayOfStringsJsonConverter))]
    public class ClassWithArrayOfStrings
    {
        public ClassWithArrayOfStrings(string[] someStrings)
        {
            SomeStrings = someStrings ?? throw new ArgumentNullException(nameof(someStrings));
        }

        public string[] SomeStrings { get; }
    }

    public class ClassWithArrayOfStringsJsonConverter : JsonConverter<ClassWithArrayOfStrings>
    {
        public override ClassWithArrayOfStrings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[]? SomeStrings = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (SomeStrings == null) throw new JsonException("ClassWithArrayOfStrings is missing property SomeStrings");
                        return new ClassWithArrayOfStrings(SomeStrings);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithArrayOfStrings.SomeStrings):
                                SomeStrings = ReadArrayString(reader);
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

        private static string[]? ReadArrayString(Utf8JsonReader reader)
        {
            bool inArray = true;
            List<string>? someList = null;
            while (inArray)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        someList = new List<string>();
                        break;
                    case JsonTokenType.EndArray:
                        inArray = false;
                        break;
                    default:
                        someList?.Add(reader.GetString());
                        break;
                }
            }
            return someList?.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, ClassWithArrayOfStrings value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(ClassWithArrayOfStrings.SomeStrings));
            writer.WriteStartArray();
            foreach (string x in value.SomeStrings)
            {
                writer.WriteStringValue(x);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
