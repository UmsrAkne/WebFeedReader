using System.Text.RegularExpressions;

namespace WebFeedReader.Utils
{
    public static class DateTimeFormatFixer
    {
        public static string FixDateTimeFormat(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            // 正規表現で "yyyy-MM-dd HH:mm:ss" の形式を探し、スペースを T に置換する
            // ターゲット： "2026-01-31 11:45:46" -> "2026-01-31T11:45:46"
            // ※日付と時刻の間のスペース（\s）だけを置換対象にします
            var pattern = @"(\d{4}-\d{2}-\d{2})\s(\d{2}:\d{2}:\d{2})";
            return Regex.Replace(json, pattern, "$1T$2");
        }
    }
}