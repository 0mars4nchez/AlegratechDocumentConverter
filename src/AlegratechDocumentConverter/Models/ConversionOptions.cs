namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Conjunto de opciones seleccionables por el usuario que controlan el comportamiento del
/// proceso de conversión. Se serializa como parte de <see cref="AppSettings"/> para recordar
/// la última configuración utilizada.
/// </summary>
public sealed class ConversionOptions
{
    /// <summary>Mantiene las imágenes embebidas extraídas del documento original.</summary>
    public bool KeepImages { get; set; }

    /// <summary>Crea una subcarpeta con el nombre del documento para cada archivo convertido.</summary>
    public bool CreateFolderPerDocument { get; set; }

    /// <summary>Abre el explorador de archivos en la carpeta de salida al finalizar el proceso.</summary>
    public bool OpenFolderWhenFinished { get; set; } = true;

    /// <summary>Sobrescribe archivos .md existentes en el destino sin preguntar.</summary>
    public bool OverwriteExistingFiles { get; set; }

    /// <summary>Muestra información detallada (paso a paso) en el panel de log inferior.</summary>
    public bool ShowDetailedLog { get; set; }

    /// <summary>Extrae y guarda metadatos del documento (autor, fecha, título, etc.) junto al .md.</summary>
    public bool ExtractMetadata { get; set; }

    /// <summary>Copia el contenido Markdown resultante al portapapeles al finalizar.</summary>
    public bool CopyMarkdownToClipboard { get; set; }

    /// <summary>
    /// Crea una copia superficial de las opciones actuales, útil para no compartir referencias
    /// mutables entre el ViewModel y la configuración persistida.
    /// </summary>
    public ConversionOptions Clone() => (ConversionOptions)MemberwiseClone();
}
