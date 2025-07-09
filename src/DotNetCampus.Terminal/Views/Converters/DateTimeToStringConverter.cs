using System.Globalization;
using Avalonia.Data.Converters;

namespace DotNetCampus.Terminal.Views.Converters;

/// <summary>
/// 时间到字符串的转换器，显示精确到秒的时间格式 (yyyy-MM-dd HH:mm:ss)
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset dateTime)
        {
            return "未同步";
        }

        // 直接显示精确到秒的时间格式
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("DateTimeToStringConverter 不支持反向转换");
    }
}
