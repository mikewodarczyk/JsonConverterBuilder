using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(SimpleVariablesJsonConverter))]
    public class SimpleVariables
    {
        public string Name { get; }
        public int SomeInt { get; }
        public string? MaybeAName { get; }
        public int? MaybeAnInt { get; }

        public SimpleVariables(string name, int someInt, string? maybeAName, int? maybeAnInt)
        {
            Name = name;
            SomeInt = someInt;
            MaybeAName = maybeAName;
            MaybeAnInt = maybeAnInt;
        }
    }

    public class SimpleVariablesJsonConverter : JsonConverter<SimpleVariables>
    {
        public override SimpleVariables Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? Name = null;
            int? SomeInt = null;
            string? MaybeAName = null;
            int? MaybeAnInt = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject: break;
                    case JsonTokenType.EndObject:
                        if (Name != null &&
                            SomeInt.HasValue)
                        {
                            return new SimpleVariables(Name, SomeInt.Value, MaybeAName, MaybeAnInt);
                        } 
                        else
                        {
                            throw new JsonException("SimpleVariables missing property");
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
                            case nameof(SimpleVariables.MaybeAName):
                                reader.Read();
                                MaybeAName = reader.GetString();
                                break;
                            case nameof(SimpleVariables.MaybeAnInt):
                                reader.Read();
                                MaybeAnInt = reader.GetInt32();
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
            if (value.MaybeAName != null)
            {
                writer.WriteString(nameof(SimpleVariables.MaybeAName), value.MaybeAName);
            }
            if (value.MaybeAnInt != null)
            {
                writer.WriteNumber(nameof(SimpleVariables.MaybeAnInt), value.MaybeAnInt.Value);
            }
            writer.WriteEndObject();
        }
    }
}
