using System.Globalization;
using System.Windows.Data;

namespace AlegratechDocumentConverter.Converters;

/// <summary>Convierte una duración en milisegundos (nullable) en un texto legible ("1.2 s" o "—").</summary>
public sealed class DurationToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long ms)
        {
            return "—";
        }

        return ms < 1000 ? $"{ms} ms" : $"{ms / 1000.0:0.0} s";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("La conversión inversa no está soportada.");
}
