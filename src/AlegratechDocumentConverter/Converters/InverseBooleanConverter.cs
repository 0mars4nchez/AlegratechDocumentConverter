using System.Globalization;
using System.Windows.Data;

namespace AlegratechDocumentConverter.Converters;

/// <summary>Invierte un valor booleano. Útil para habilitar/deshabilitar controles según una condición negada.</summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}
