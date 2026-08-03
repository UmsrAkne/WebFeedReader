using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebFeedReader.Utils
{
    internal sealed class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number => reader.TryGetInt32(out var value) ? value != 0 : throw new JsonException("Failed to read number as boolean."),
                JsonTokenType.String => bool.TryParse(reader.GetString(), out var value) ? value : throw new JsonException("Failed to read string as boolean."),
                _ => throw new JsonException($"Unexpected token type {reader.TokenType} for boolean."),
            };
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}