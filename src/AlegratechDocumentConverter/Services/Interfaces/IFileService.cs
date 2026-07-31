using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de las operaciones sobre el sistema de archivos: validación de
/// archivos de entrada, cálculo de rutas de salida y apertura del explorador de Windows.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Construye un <see cref="FileItem"/> a partir de una ruta de archivo, calculando su
    /// tamaño, extensión y categoría. Devuelve <c>null</c> si el archivo no existe o no es
    /// de un tipo soportado.
    /// </summary>
    FileItem? CreateFileItem(string path);

    /// <summary>Formatea un tamaño en bytes a una cadena legible (KB, MB, GB).</summary>
    string FormatFileSize(long sizeInBytes);

    /// <summary>Se asegura de que el directorio indicado exista, creándolo si es necesario.</summary>
    void EnsureDirectoryExists(string directoryPath);

    /// <summary>
    /// Calcula la ruta de salida .md para un archivo de entrada dado, respetando las opciones
    /// de "crear carpeta por documento" y "sobrescribir archivos existentes".
    /// </summary>
    string ResolveOutputPath(FileItem file, string outputDirectory, ConversionOptions options);

    /// <summary>Abre el explorador de Windows en la carpeta indicada.</summary>
    void OpenFolderInExplorer(string directoryPath);

    /// <summary>Copia el texto indicado al portapapeles del sistema.</summary>
    void CopyToClipboard(string text);
}
