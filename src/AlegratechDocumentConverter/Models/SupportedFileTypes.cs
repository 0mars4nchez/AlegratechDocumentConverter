namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Categoría general a la que pertenece un tipo de archivo soportado. Se utiliza para
/// seleccionar el icono y para mostrar una etiqueta legible en la lista de archivos.
/// </summary>
public enum FileCategory
{
    Document,
    Spreadsheet,
    Presentation,
    Image,
    Audio,
    Archive,
    PlainText,
    Web,
    Unknown
}

/// <summary>
/// Punto único de verdad sobre las extensiones de archivo soportadas por MarkItDown y su
/// clasificación. Evita duplicar listas de extensiones en distintas partes de la aplicación
/// (principio DRY / SOLID - responsabilidad única).
/// </summary>
public static class SupportedFileTypes
{
    /// <summary>
    /// Diccionario de extensión (en minúsculas, sin punto) a categoría de archivo.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, FileCategory> ExtensionMap =
        new Dictionary<string, FileCategory>(StringComparer.OrdinalIgnoreCase)
        {
            ["pdf"] = FileCategory.Document,
            ["doc"] = FileCategory.Document,
            ["docx"] = FileCategory.Document,
            ["ppt"] = FileCategory.Presentation,
            ["pptx"] = FileCategory.Presentation,
            ["xls"] = FileCategory.Spreadsheet,
            ["xlsx"] = FileCategory.Spreadsheet,
            ["csv"] = FileCategory.Spreadsheet,
            ["txt"] = FileCategory.PlainText,
            ["html"] = FileCategory.Web,
            ["htm"] = FileCategory.Web,
            ["zip"] = FileCategory.Archive,
            ["jpg"] = FileCategory.Image,
            ["jpeg"] = FileCategory.Image,
            ["png"] = FileCategory.Image,
            ["tiff"] = FileCategory.Image,
            ["tif"] = FileCategory.Image,
            ["bmp"] = FileCategory.Image,
            ["gif"] = FileCategory.Image,
            ["webp"] = FileCategory.Image,
            ["mp3"] = FileCategory.Audio,
            ["wav"] = FileCategory.Audio,
            ["m4a"] = FileCategory.Audio
        };

    /// <summary>
    /// Extensiones aceptadas, expuestas como colección para construir filtros de diálogo
    /// y validaciones de arrastrar y soltar.
    /// </summary>
    public static IReadOnlyCollection<string> AllExtensions => (IReadOnlyCollection<string>)ExtensionMap.Keys;

    /// <summary>
    /// Indica si la extensión (con o sin punto inicial) pertenece a un tipo de archivo soportado.
    /// </summary>
    public static bool IsSupported(string extension)
    {
        var normalized = Normalize(extension);
        return ExtensionMap.ContainsKey(normalized);
    }

    /// <summary>
    /// Obtiene la categoría asociada a una extensión, o <see cref="FileCategory.Unknown"/>
    /// si no está soportada.
    /// </summary>
    public static FileCategory GetCategory(string extension)
    {
        var normalized = Normalize(extension);
        return ExtensionMap.TryGetValue(normalized, out var category) ? category : FileCategory.Unknown;
    }

    /// <summary>
    /// Construye el filtro para <c>Microsoft.Win32.OpenFileDialog</c> con todas las extensiones
    /// soportadas, más un grupo "Todos los archivos soportados".
    /// </summary>
    public static string BuildOpenFileDialogFilter()
    {
        var allPatterns = string.Join(";", AllExtensions.Select(ext => $"*.{ext}"));
        return $"Todos los archivos soportados|{allPatterns}|Todos los archivos (*.*)|*.*";
    }

    private static string Normalize(string extension)
    {
        return extension.TrimStart('.').Trim().ToLowerInvariant();
    }
}
