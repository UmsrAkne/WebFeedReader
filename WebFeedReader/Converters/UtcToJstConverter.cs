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

            // DateTimeOffset.ToLocalTime() は OS のタイムゾーンを使う
            var local = dto.ToLocalTime();

            return local.ToString("yyyy/MM/dd HH:mm", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}