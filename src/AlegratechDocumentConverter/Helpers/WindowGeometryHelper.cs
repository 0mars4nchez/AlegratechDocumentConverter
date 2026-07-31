using System.Windows;

namespace AlegratechDocumentConverter.Helpers;

/// <summary>
/// Utilidades para restaurar de forma segura el tamaño y la posición de la ventana principal
/// a partir de la configuración persistida, evitando que la ventana aparezca fuera del área
/// visible si, por ejemplo, se desconectó un monitor externo desde la última ejecución.
/// </summary>
public static class WindowGeometryHelper
{
    /// <summary>
    /// Calcula una posición segura para la ventana, garantizando que quede al menos
    /// parcialmente visible dentro del área de trabajo de la pantalla principal.
    /// </summary>
    public static (double Left, double Top) ClampToVisibleArea(double? left, double? top, double width, double height)
    {
        var workArea = SystemParameters.WorkArea;

        if (!left.HasValue || !top.HasValue)
        {
            var centeredLeft = workArea.Left + Math.Max(0, (workArea.Width - width) / 2);
            var centeredTop = workArea.Top + Math.Max(0, (workArea.Height - height) / 2);
            return (centeredLeft, centeredTop);
        }

        var minVisibleMargin = 80;

        var clampedLeft = Math.Min(Math.Max(left.Value, workArea.Left - width + minVisibleMargin), workArea.Right - minVisibleMargin);
        var clampedTop = Math.Min(Math.Max(top.Value, workArea.Top), workArea.Bottom - minVisibleMargin);

        return (clampedLeft, clampedTop);
    }
}
