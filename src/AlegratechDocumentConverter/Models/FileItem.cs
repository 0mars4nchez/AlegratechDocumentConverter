namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Modelo de dominio que representa un archivo seleccionado por el usuario para su conversión.
/// Es una clase de datos simple (POCO), sin dependencias de WPF, para mantener la capa de
/// modelos independiente de la interfaz gráfica (Clean Architecture).
/// </summary>
public sealed class FileItem
{
    /// <summary>Ruta completa del archivo de origen.</summary>
    public required string FullPath { get; init; }

    /// <summary>Nombre del archivo, sin ruta.</summary>
    public required string FileName { get; init; }

    /// <summary>Tamaño del archivo en bytes.</summary>
    public long SizeInBytes { get; init; }

    /// <summary>Extensión del archivo, sin punto y en minúsculas.</summary>
    public required string Extension { get; init; }

    /// <summary>Categoría del archivo, usada para elegir icono y validar soporte.</summary>
    public FileCategory Category { get; init; }

    /// <summary>Estado actual del proceso de conversión para este archivo.</summary>
    public ConversionStatus Status { get; set; } = ConversionStatus.Pending;

    /// <summary>Ruta del archivo Markdown generado, una vez completada la conversión.</summary>
    public string? OutputPath { get; set; }

    /// <summary>Mensaje de error, si la conversión falló.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Duración de la conversión en milisegundos, una vez completada.</summary>
    public long? DurationMs { get; set; }
}
