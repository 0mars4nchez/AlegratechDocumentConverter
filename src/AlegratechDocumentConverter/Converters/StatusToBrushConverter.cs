using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Converters;

/// <summary>
/// Convierte un <see cref="ConversionStatus"/> en el <see cref="Brush"/> correspondiente,
/// tomado de los recursos de tema de la aplicación (Themes/Colors.xaml), de modo que el
/// color se mantenga centralizado en un único lugar.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resourceKey = value switch
        {
            ConversionStatus.Success => "SuccessBrush",
            ConversionStatus.Failed => "ErrorBrush",
            ConversionStatus.Processing => "PrimaryBlueBrush",
            ConversionStatus.Skipped => "WarningBrush",
            _ => "MediumGrayBrush"
        };

        return Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("La conversión inversa no está soportada.");
}
