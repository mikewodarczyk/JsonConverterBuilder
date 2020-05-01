using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrBronners.PricingChangeSchema
{
    [JsonConverter(typeof(ClassWithBoolJsonConverter))]
    public class ClassWithBool
    {
        public ClassWithBool(bool value, bool? maybeBoolValue)
        {
            Value = value;
            MaybeBoolValue = maybeBoolValue;
        }

        public bool Value { get; set; }
        public bool? MaybeBoolValue { get; set; }
    }

    public class ClassWithBoolJsonConverter : JsonConverter<ClassWithBool>
    {
        public override ClassWithBool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool? Value = null;
            bool? MaybeBoolValue = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (Value == null) throw new JsonException("ClassWithBool is missing property Value");
                        return new ClassWithBool(Value.Value, MaybeBoolValue);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithBool.Value):
                                reader.Read();
                                Value = reader.GetBoolean();
                                break;
                            case nameof(ClassWithBool.MaybeBoolValue):
                                reader.Read();
                                MaybeBoolValue = reader.GetBoolean();
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

        public override void Write(Utf8JsonWriter writer, ClassWithBool value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean(nameof(ClassWithBool.Value),value.Value);
            if (value.MaybeBoolValue != null)
            {
                writer.WriteBoolean(nameof(ClassWithBool.MaybeBoolValue),value.MaybeBoolValue.Value);
            }

            writer.WriteEndObject();
        }
    }
}
