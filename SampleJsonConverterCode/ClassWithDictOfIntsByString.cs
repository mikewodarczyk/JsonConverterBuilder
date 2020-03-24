using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassWithDictionaryOfIntsByStringJsonConverter))]
    public class ClassWithDictionaryOfIntsByString
    {
        public Dictionary<string, int> SomeIntsByString { get; }
        public Dictionary<string, int>? MaybeSomeIntsByString { get; }

        public ClassWithDictionaryOfIntsByString(Dictionary<string, int> someIntsByString, Dictionary<string, int>? maybeSomeIntsByString)
        {
            SomeIntsByString = someIntsByString;
            MaybeSomeIntsByString = maybeSomeIntsByString;
        }
    }

    public class ClassWithDictionaryOfIntsByStringJsonConverter : JsonConverter<ClassWithDictionaryOfIntsByString>
    {
        public override ClassWithDictionaryOfIntsByString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Dictionary<string, int>? SomeIntsByString = null;
            Dictionary<string, int>? MaybeSomeIntsByString = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (SomeIntsByString == null) throw new JsonException("ClassWithDictionaryOfIntsByString is missing property SomeIntsByString");
                        return new ClassWithDictionaryOfIntsByString(SomeIntsByString, MaybeSomeIntsByString);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithDictionaryOfIntsByString.SomeIntsByString):
                                SomeIntsByString = ReadDictionaryInt<string>(reader, s => s);
                                break;
                            case nameof(ClassWithDictionaryOfIntsByString.MaybeSomeIntsByString):
                                MaybeSomeIntsByString = ReadDictionaryInt<string>(reader, s => s);
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

        private static Dictionary<T1,int> ReadDictionaryInt<T1>(Utf8JsonReader reader, Func<string,T1> stringToKeyType) where T1 : notnull
        {
            Dictionary<T1, int> dict = new Dictionary<T1, int>();
            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        return dict;
                    case JsonTokenType.PropertyName:
                        string keyString = reader.GetString();
                        T1 key = stringToKeyType(keyString);
                        reader.Read();
                        int value = reader.GetInt32();
                        dict.Add(key, value);
                        break;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, ClassWithDictionaryOfIntsByString value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteDictionary<string, int>(writer, nameof(ClassWithDictionaryOfIntsByString.SomeIntsByString), value.SomeIntsByString, (k, v) => writer.WriteNumber(k, v));
            if (value.MaybeSomeIntsByString != null)
            {
                WriteDictionary<string, int>(writer, nameof(ClassWithDictionaryOfIntsByString.MaybeSomeIntsByString), value.MaybeSomeIntsByString, (k, v) => writer.WriteNumber(k, v));
            }

            writer.WriteEndObject();
        }

        private static void WriteDictionary<KT,VT>(Utf8JsonWriter writer, string propertyName, Dictionary<KT,VT> dict, Action<KT,VT> writeKeyValuePair) where KT : notnull
        {
            writer.WritePropertyName(propertyName);
            writer.WriteStartObject();
            foreach (KT key in dict.Keys)
            {
                writeKeyValuePair(key,dict[key]);
            }

            writer.WriteEndObject();
        }
    }
}
