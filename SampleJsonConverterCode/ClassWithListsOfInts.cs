using System;
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
                        if (SomeInts == null) throw new JsonException("ClassWithListOfInts is missing property SomeInts");
                        return new ClassWithListOfInts(SomeInts);                        
                    case JsonTokenType.PropertyName:
                        reader.Read();
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithListOfInts.SomeInts):
                                bool inArray = true;
                                while ( inArray  ) 
                                {
                                    reader.Read();
                                    switch(reader.TokenType)
                                    {
                                        case JsonTokenType.StartArray:
                                            SomeInts = new List<int>();
                                            break;
                                        case JsonTokenType.EndArray:
                                            inArray = false;
                                            break;
                                        default:
                                            SomeInts.Add(reader.GetInt32());
                                            break;
                                    }
                                }
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

        public override void Write(Utf8JsonWriter writer, ClassWithListOfInts value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(ClassWithListOfInts.SomeInts));
            writer.WriteStartArray();
            foreach(int x in value.SomeInts)
            {
                writer.WriteNumberValue(x);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
