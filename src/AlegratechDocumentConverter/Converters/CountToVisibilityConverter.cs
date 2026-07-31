using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlegratechDocumentConverter.Converters;

/// <summary>
/// Convierte un número entero (típicamente el tamaño de una colección) en <see cref="Visibility"/>:
/// visible cuando es mayor que cero, colapsado en caso contrario. El parámetro "Invert" invierte la lógica,
/// lo que resulta útil para mostrar un mensaje de "lista vacía" superpuesto a la zona de drag &amp; drop.
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value is int i ? i : 0;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        var isVisible = invert ? count == 0 : count > 0;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("La conversión inversa no está soportada.");
}
