using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    public class /*[|*/ClassContainingListOfClass/*|]*/
    {
        public ClassContainingListOfClass(List<SomeOtherClass> otherClassLst, List<SomeOtherClass>? maybeOtherClassLst)
        {
            OtherClassLst = otherClassLst;
            MaybeOtherClassLst = maybeOtherClassLst;
        }

        public List<SomeOtherClass> OtherClassLst { get; }
        public List<SomeOtherClass>? MaybeOtherClassLst { get; }
    }

    [JsonConverter(typeof(SomeOtherClassJsonConverter))]
    public class SomeOtherClass
    {
        public SomeOtherClass(string AValue)
        {
            this.AValue = AValue;
        }

        public string AValue { get; }
    }

    public class SomeOtherClassJsonConverter : JsonConverter<SomeOtherClass>
    {
        public override SomeOtherClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? AValue = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (AValue == null) throw new JsonException("SomeOtherClass is missing property AValue");
                        return new SomeOtherClass(AValue);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(SomeOtherClass.AValue):
                                reader.Read();
                                AValue = reader.GetString();
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

        public override void Write(Utf8JsonWriter writer, SomeOtherClass value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(SomeOtherClass.AValue),value.AValue);
            writer.WriteEndObject();
        }
    }
}
