namespace WebFeedReader.Utils
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 数値で来てるならintで読んで、0以外ならtrue
            return reader.GetInt32() != 0;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value ? 1 : 0);
        }
    }
}