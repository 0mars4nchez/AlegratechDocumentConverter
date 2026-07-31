namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Resultado devuelto por <see cref="Services.Interfaces.IMarkItDownService"/> tras intentar
/// convertir un único archivo. Encapsula tanto el caso de éxito como el de fallo para que el
/// llamador nunca necesite capturar excepciones del proceso de Python.
/// </summary>
/// <param name="IsSuccess">Indica si la conversión finalizó correctamente.</param>
/// <param name="OutputMarkdownPath">Ruta completa del archivo .md generado, si tuvo éxito.</param>
/// <param name="ErrorMessage">Mensaje de error legible, si la conversión falló.</param>
/// <param name="DurationMs">Duración total de la conversión en milisegundos.</param>
/// <param name="Metadata">Metadatos extraídos del documento (título, autor, fecha, etc.), si se solicitaron.</param>
/// <param name="MarkdownContent">Contenido Markdown generado, usado para copiar al portapapeles.</param>
public sealed record ConversionResult(
    bool IsSuccess,
    string? OutputMarkdownPath,
    string? ErrorMessage,
    long DurationMs,
    IReadOnlyDictionary<string, string>? Metadata,
    string? MarkdownContent)
{
    /// <summary>Crea un resultado exitoso.</summary>
    public static ConversionResult Success(
        string outputPath,
        long durationMs,
        string? markdownContent,
        IReadOnlyDictionary<string, string>? metadata = null) =>
        new(true, outputPath, null, durationMs, metadata, markdownContent);

    /// <summary>Crea un resultado fallido.</summary>
    public static ConversionResult Failure(string errorMessage, long durationMs) =>
        new(false, null, errorMessage, durationMs, null, null);
}
