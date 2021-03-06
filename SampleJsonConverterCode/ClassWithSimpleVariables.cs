﻿using System;
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
        public DateTime ADateTime { get; }
        public string? MaybeAName { get; }
        public int? MaybeAnInt { get; }
        public DateTime? MaybeADateTime { get; }

        public SimpleVariables(string name, int someInt, DateTime aDateTime, string? maybeAName, int? maybeAnInt, DateTime? maybeADateTime)
        {
            Name = name;
            SomeInt = someInt;
            ADateTime = aDateTime;
            MaybeAName = maybeAName;
            MaybeAnInt = maybeAnInt;
            MaybeADateTime = maybeADateTime;
        }
    }

    public class SimpleVariablesJsonConverter : JsonConverter<SimpleVariables>
    {
        public override SimpleVariables Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? Name = null;
            int? SomeInt = null;
            DateTime? ADateTime = null;
            string? MaybeAName = null;
            int? MaybeAnInt = null;
            DateTime? MaybeADateTime = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (Name == null) throw new JsonException("SimpleVariables is missing property Name");
                        if (SomeInt == null) throw new JsonException("SimpleVariables is missing property SomeInt");
                        if (ADateTime == null) throw new JsonException("SimpleVariables is missing property ADateTime");
                        return new SimpleVariables(Name, SomeInt.Value, ADateTime.Value, MaybeAName, MaybeAnInt, MaybeADateTime);
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
                            case nameof(SimpleVariables.ADateTime):
                                reader.Read();
                                ADateTime = DateTime.Parse(reader.GetString());
                                break;
                            case nameof(SimpleVariables.MaybeAName):
                                reader.Read();
                                MaybeAName = reader.GetString();
                                break;
                            case nameof(SimpleVariables.MaybeAnInt):
                                reader.Read();
                                MaybeAnInt = reader.GetInt32();
                                break;
                            case nameof(SimpleVariables.MaybeADateTime):
                                reader.Read();
                                MaybeADateTime = DateTime.Parse(reader.GetString());
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
            writer.WriteString(nameof(SimpleVariables.Name),value.Name);
            writer.WriteNumber(nameof(SimpleVariables.SomeInt),value.SomeInt);
            writer.WriteString(nameof(SimpleVariables.ADateTime),value.ADateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            if (value.MaybeAName != null)
            {
                writer.WriteString(nameof(SimpleVariables.MaybeAName),value.MaybeAName);
            }

            if (value.MaybeAnInt != null)
            {
                writer.WriteNumber(nameof(SimpleVariables.MaybeAnInt),value.MaybeAnInt.Value);
            }

            if (value.MaybeADateTime != null)
            {
                writer.WriteString(nameof(SimpleVariables.MaybeADateTime),value.MaybeADateTime.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            }

            writer.WriteEndObject();
        }
    }
}
