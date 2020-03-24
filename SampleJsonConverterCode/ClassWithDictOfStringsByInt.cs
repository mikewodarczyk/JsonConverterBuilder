using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace SampleJsonConverterCode
{
    [JsonConverter(typeof(ClassWithDictionaryOfStringsByIntJsonConverter))]
    public class ClassWithDictionaryOfStringsByInt
    {
        public Dictionary<int, string> SomeStringsByInt { get; }
        public Dictionary<int, string>? MaybeSomeStringsByInt { get; }

        public ClassWithDictionaryOfStringsByInt(Dictionary<int, string> someStringsByInt, Dictionary<int, string>? maybeSomeStringsByInt)
        {
            SomeStringsByInt = someStringsByInt;
            MaybeSomeStringsByInt = maybeSomeStringsByInt;
        }
    }

    public class ClassWithDictionaryOfStringsByIntJsonConverter : JsonConverter<ClassWithDictionaryOfStringsByInt>
    {
        public override ClassWithDictionaryOfStringsByInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Dictionary<int, string>? SomeStringsByInt = null;
            Dictionary<int, string>? MaybeSomeStringsByInt = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (SomeStringsByInt == null) throw new JsonException("ClassWithDictionaryOfStringsByInt is missing property SomeStringsByInt");
                        return new ClassWithDictionaryOfStringsByInt(SomeStringsByInt, MaybeSomeStringsByInt);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(ClassWithDictionaryOfStringsByInt.SomeStringsByInt):
                                SomeStringsByInt = ReadDictionaryString<int>(reader, s => int.Parse(s));
                                break;
                            case nameof(ClassWithDictionaryOfStringsByInt.MaybeSomeStringsByInt):
                                MaybeSomeStringsByInt = ReadDictionaryString<int>(reader, s => int.Parse(s));
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

        private static Dictionary<T1,string> ReadDictionaryString<T1>(Utf8JsonReader reader, Func<string,T1> stringToKeyType) where T1 : notnull
        {
            Dictionary<T1, string> dict = new Dictionary<T1, string>();
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
                        string value = reader.GetString();
                        dict.Add(key, value);
                        break;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, ClassWithDictionaryOfStringsByInt value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteDictionary<int, string>(writer, nameof(ClassWithDictionaryOfStringsByInt.SomeStringsByInt), value.SomeStringsByInt, (k, v) => writer.WriteString(k.ToString(), v));
            if (value.MaybeSomeStringsByInt != null)
            {
                WriteDictionary<int, string>(writer, nameof(ClassWithDictionaryOfStringsByInt.MaybeSomeStringsByInt), value.MaybeSomeStringsByInt, (k, v) => writer.WriteString(k.ToString(), v));
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
