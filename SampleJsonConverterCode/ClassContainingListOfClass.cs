using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassContainingListOfClassJsonConverter))]
    public class ClassContainingListOfClass
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

    public class ClassContainingListOfClassJsonConverter : JsonConverter<ClassContainingListOfClass>
    {
        public override ClassContainingListOfClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<SomeOtherClass>? OtherClassLst = null;
            List<SomeOtherClass>? MaybeOtherClassLst = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (OtherClassLst == null) throw new JsonException("ClassContainingListOfClass is missing property OtherClassLst");
                        return new ClassContainingListOfClass(OtherClassLst, MaybeOtherClassLst);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassContainingListOfClass.OtherClassLst):
                                OtherClassLst = ReadList<SomeOtherClass>(ref reader, options, new SomeOtherClassJsonConverter());
                                break;
                            case nameof(ClassContainingListOfClass.MaybeOtherClassLst):
                                MaybeOtherClassLst = ReadList<SomeOtherClass>(ref reader, options, new SomeOtherClassJsonConverter());
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

        private static List<T> ReadList<T>(ref Utf8JsonReader reader, JsonSerializerOptions options, JsonConverter<T> converter)
        {
            List<T> lst = new List<T>();
            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.EndArray:
                        return lst;
                    default:
                        lst.Add(converter.Read(ref reader, typeof(T), options));
                        break;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, ClassContainingListOfClass value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteList<SomeOtherClass>(writer,nameof(ClassContainingListOfClass.OtherClassLst),value.OtherClassLst,options,new SomeOtherClassJsonConverter());
            if (value.MaybeOtherClassLst != null)
            {
                WriteList<SomeOtherClass>(writer,nameof(ClassContainingListOfClass.MaybeOtherClassLst),value.MaybeOtherClassLst,options,new SomeOtherClassJsonConverter());
            }

            writer.WriteEndObject();
        }

        private static void WriteList<T>(Utf8JsonWriter writer, string propertyName, List<T> values, JsonSerializerOptions options, JsonConverter<T> converter)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteStartArray();
            foreach (T value in values)
            {
                converter.Write(writer,value,options);
            }

            writer.WriteEndArray();
        }
    }
}
