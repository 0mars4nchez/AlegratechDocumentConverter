namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Entrada inmutable del registro de actividad de la aplicación. Cada conversión, advertencia
/// o error queda representado por una instancia de esta clase.
/// </summary>
/// <param name="Timestamp">Momento exacto en el que ocurrió el evento.</param>
/// <param name="Level">Severidad del evento.</param>
/// <param name="FileName">Nombre del archivo relacionado, si aplica.</param>
/// <param name="Message">Mensaje descriptivo del evento.</param>
/// <param name="DurationMs">Duración de la operación en milisegundos, si aplica.</param>
public sealed record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string? FileName,
    string Message,
    long? DurationMs = null)
{
    /// <summary>
    /// Devuelve una representación de una sola línea apta para archivo de log o para la UI.
    /// </summary>
    public string ToDisplayString()
    {
        var prefix = Level switch
        {
            LogLevel.Success => "[ÉXITO]",
            LogLevel.Warning => "[ADVERTENCIA]",
            LogLevel.Error => "[ERROR]",
            _ => "[INFO]"
        };

        var fileSegment = string.IsNullOrWhiteSpace(FileName) ? string.Empty : $" ({FileName})";
        var durationSegment = DurationMs.HasValue ? $" - {DurationMs} ms" : string.Empty;

        return $"{Timestamp:HH:mm:ss} {prefix} {Message}{fileSegment}{durationSegment}";
    }
}
