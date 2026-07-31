using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlegratechDocumentConverter.Converters;

/// <summary>
/// Convierte un valor booleano en <see cref="Visibility"/>. Admite el parámetro de conversión
/// "Invert" para invertir la lógica (útil para ocultar un elemento cuando la condición es verdadera).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("La conversión inversa no está soportada.");
}
