using System;
using System.Globalization;
using System.Windows.Data;

namespace WebFeedReader.Converters
{
    public sealed class UtcToJstConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTimeOffset dto)
            {
                return Binding.DoNothing;
            }

            // 入力される日時が UTC だと仮定し、強制的に JST に変換して表示する。
            var local = dto.AddHours(9);
            return local.ToString("yyyy/MM/dd HH:mm", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}