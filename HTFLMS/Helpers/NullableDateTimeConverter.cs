using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HTFLMS.Helper
{

    public class NullableDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String && string.IsNullOrEmpty(reader.GetString()))
                return null;

            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.GetString(), out var dateTimeValue))
                return dateTimeValue;

            throw new JsonException("Unexpected token type for nullable DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
