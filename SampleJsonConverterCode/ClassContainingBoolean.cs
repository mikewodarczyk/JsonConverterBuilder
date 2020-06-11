using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassContainingBooleanJsonConverter))]
    public class ClassContainingBoolean
    {
        public ClassContainingBoolean(bool aBoolVar, bool aBooleanVar)
        {
            this.ABoolVar = aBoolVar;
            this.ABooleanVar = aBooleanVar;
        }

        public bool ABoolVar { get;  }
        public Boolean ABooleanVar { get; }
    }

    public class ClassContainingBooleanJsonConverter : JsonConverter<ClassContainingBoolean>
    {
        public override ClassContainingBoolean Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool? ABoolVar = null;
            Boolean? ABooleanVar = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (ABoolVar == null) throw new JsonException("ClassContainingBoolean is missing property ABoolVar");
                        if (ABooleanVar == null) throw new JsonException("ClassContainingBoolean is missing property ABooleanVar");
                        return new ClassContainingBoolean(ABoolVar.Value, ABooleanVar.Value);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassContainingBoolean.ABoolVar):
                                reader.Read();
                                ABoolVar = reader.GetBoolean();
                                break;
                            case nameof(ClassContainingBoolean.ABooleanVar):
                                reader.Read();
                                ABooleanVar = reader.GetBoolean();
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

        public override void Write(Utf8JsonWriter writer, ClassContainingBoolean value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean(nameof(ClassContainingBoolean.ABoolVar),value.ABoolVar);
            writer.WriteBoolean(nameof(ClassContainingBoolean.ABooleanVar),value.ABooleanVar);
            writer.WriteEndObject();
        }
    }
}
