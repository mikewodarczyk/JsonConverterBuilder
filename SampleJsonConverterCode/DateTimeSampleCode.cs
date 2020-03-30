using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrBronners.PricingChangeSchema
{
    [JsonConverter(typeof(BasePriceChangeJsonConverter))]
    public class BasePriceChange : IEquatable<BasePriceChange>
    {
        public BasePriceChange(DateTime date, string sku, decimal? newPrice, string createdByUser)
        {
            Date = date;
            Sku = sku;
            NewPrice = newPrice;
            CreatedByUser = createdByUser;
        }

        public DateTime Date { get; set; }
        public string Sku { get; set; }
        public decimal? NewPrice { get; set; }
        public string CreatedByUser { get; set; }

        public override string ToString()
        {
            return $"{{Date:{Date},Sku:{Sku},NewPrice:{NewPrice}}}";
        }

        public BasePriceChange DeepCopy()
        {
            BasePriceChange ch = new BasePriceChange(
              date: Date,
              sku: Sku,
              newPrice: NewPrice,
              createdByUser: ""
            );
            return ch;
        }

        public bool Equals(BasePriceChange other)
        {
            return Date == other.Date &&
                Sku == other.Sku &&
                NewPrice == other.NewPrice &&
                CreatedByUser == other.CreatedByUser;
        }

        public static bool operator ==(BasePriceChange? a, BasePriceChange? b)
        {
            if (a is null) return b is null;
            return b is object && a.Equals(b);
        }

        public static bool operator !=(BasePriceChange? a, BasePriceChange? b)
        {
            if (a is null) return b is object;
            return b is null || !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Date, Sku, NewPrice, CreatedByUser);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return obj is BasePriceChange && Equals((BasePriceChange)obj);
        }
    }

    public class BasePriceChangeJsonConverter : JsonConverter<BasePriceChange>
    {
        public override BasePriceChange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime? Date = null;
            string? Sku = null;
            decimal? NewPrice = null;
            string? CreatedByUser = null;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.EndObject:
                        if (Date == null) throw new JsonException("BasePriceChange is missing property Date");
                        if (Sku == null) throw new JsonException("BasePriceChange is missing property Sku");
                        if (CreatedByUser == null) throw new JsonException("BasePriceChange is missing property CreatedByUser");
                        return new BasePriceChange(Date.Value, Sku, NewPrice, CreatedByUser);
                    case JsonTokenType.PropertyName:
                        switch (reader.GetString())
                        {
                            case nameof(BasePriceChange.Date):
                                reader.Read();
                                Date = DateTime.Parse(reader.GetString());
                                break;
                            case nameof(BasePriceChange.Sku):
                                reader.Read();
                                Sku = reader.GetString();
                                break;
                            case nameof(BasePriceChange.NewPrice):
                                reader.Read();
                                NewPrice = reader.GetDecimal();
                                break;
                            case nameof(BasePriceChange.CreatedByUser):
                                reader.Read();
                                CreatedByUser = reader.GetString();
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

        public override void Write(Utf8JsonWriter writer, BasePriceChange value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(BasePriceChange.Date),value.Date.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            writer.WriteString(nameof(BasePriceChange.Sku),value.Sku);
            if (value.NewPrice != null)
            {
                writer.WriteNumber(nameof(BasePriceChange.NewPrice),value.NewPrice.Value);
            }

            writer.WriteString(nameof(BasePriceChange.CreatedByUser),value.CreatedByUser);
            writer.WriteEndObject();
        }
    }
}
