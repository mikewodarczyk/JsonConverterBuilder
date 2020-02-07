using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(CJsonConverter))]
    public class C
    {
    }

    public class CJsonConverter : JsonConverter<C>
    {
        public override C Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            while(true)
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
