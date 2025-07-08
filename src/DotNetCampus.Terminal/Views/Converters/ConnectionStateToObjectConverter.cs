using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Views.Converters;

public class ConnectionStateToObjectConverter<T> : IValueConverter where T : class
{
    public T? Testing { get; set; }

    public T? Online { get; set; }

    public T? Offline { get; set; }

    public T? Default { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ConnectionState.Testing => Testing ?? Default,
            ConnectionState.Online => Online ?? Default,
            ConnectionState.Offline => Offline ?? Default,
            _ => Default,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ConnectionStateToBrushConverter : ConnectionStateToObjectConverter<IBrush>;
