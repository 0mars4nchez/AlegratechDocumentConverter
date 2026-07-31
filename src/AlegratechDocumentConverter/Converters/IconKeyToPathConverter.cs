using System.Globalization;
using System.Windows.Data;

namespace AlegratechDocumentConverter.Converters;

/// <summary>
/// Convierte la clave de icono de un archivo (por ejemplo, "pdf" o "word") en la URI de
/// paquete (pack URI) del recurso SVG correspondiente, ubicado en <c>Resources/Icons</c>,
/// para ser consumido por el control <c>SvgViewbox</c> de SharpVectors.
/// </summary>
public sealed class IconKeyToPathConverter : IValueConverter
{
    private const string BasePath = "pack://application:,,,/Resources/Icons/";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value as string;
        if (string.IsNullOrWhiteSpace(key))
        {
            key = "generic";
        }

        return new Uri($"{BasePath}{key}.svg", UriKind.Absolute);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("La conversión inversa no está soportada.");
}
