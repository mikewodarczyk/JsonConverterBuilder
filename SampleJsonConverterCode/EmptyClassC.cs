using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(EmptyClassCJsonConverter))]
    public class EmptyClassC
    {
    }

    public class EmptyClassCJsonConverter : JsonConverter<EmptyClassC>
    {
        public override EmptyClassC Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        return new EmptyClassC();
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

        public override void Write(Utf8JsonWriter writer, EmptyClassC value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}
