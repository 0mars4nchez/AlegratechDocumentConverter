using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Helpers;

/// <summary>
/// Utilidades de solo lectura para traducir una categoría o extensión de archivo en
/// información apta para la interfaz gráfica: el nombre del recurso de icono SVG a utilizar
/// y una etiqueta legible en español para la columna "Tipo" de la lista de archivos.
/// </summary>
public static class FileTypeHelper
{
    /// <summary>
    /// Devuelve el nombre de archivo (sin extensión) del icono SVG que representa la
    /// extensión indicada, ubicado en <c>Resources/Icons</c>.
    /// </summary>
    public static string GetIconKey(string extension)
    {
        var normalized = extension.TrimStart('.').ToLowerInvariant();

        return normalized switch
        {
            "pdf" => "pdf",
            "doc" or "docx" => "word",
            "ppt" or "pptx" => "powerpoint",
            "xls" or "xlsx" => "excel",
            "csv" => "csv",
            "txt" => "txt",
            "html" or "htm" => "html",
            "zip" => "zip",
            "jpg" or "jpeg" or "png" or "tiff" or "tif" or "bmp" or "gif" or "webp" => "image",
            "mp3" or "wav" or "m4a" => "audio",
            _ => "generic"
        };
    }

    /// <summary>Devuelve una etiqueta legible en español para la categoría de archivo indicada.</summary>
    public static string GetCategoryLabel(FileCategory category) => category switch
    {
        FileCategory.Document => "Documento",
        FileCategory.Spreadsheet => "Hoja de cálculo",
        FileCategory.Presentation => "Presentación",
        FileCategory.Image => "Imagen",
        FileCategory.Audio => "Audio",
        FileCategory.Archive => "Archivo comprimido",
        FileCategory.PlainText => "Texto plano",
        FileCategory.Web => "Página web",
        _ => "Desconocido"
    };

    /// <summary>Devuelve una etiqueta legible en español para el estado de conversión indicado.</summary>
    public static string GetStatusLabel(ConversionStatus status) => status switch
    {
        ConversionStatus.Pending => "Pendiente",
        ConversionStatus.Processing => "Procesando...",
        ConversionStatus.Success => "Completado",
        ConversionStatus.Failed => "Error",
        ConversionStatus.Skipped => "Omitido",
        _ => "Desconocido"
    };
}
