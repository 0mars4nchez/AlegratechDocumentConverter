using System.IO;
using System.Diagnostics;
using System.Windows;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="IFileService"/> que encapsula todo el acceso al sistema de
/// archivos: creación de modelos <see cref="FileItem"/>, formateo de tamaños, resolución de
/// rutas de salida y utilidades del explorador de Windows y el portapapeles.
/// </summary>
public sealed class FileService : IFileService
{
    private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB" };

    /// <inheritdoc/>
    public FileItem? CreateFileItem(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            if (!SupportedFileTypes.IsSupported(extension))
            {
                return null;
            }

            var info = new FileInfo(path);

            return new FileItem
            {
                FullPath = path,
                FileName = info.Name,
                SizeInBytes = info.Length,
                Extension = extension,
                Category = SupportedFileTypes.GetCategory(extension)
            };
        }
        catch
        {
            // Cualquier problema de acceso (permisos, ruta demasiado larga, etc.) se traduce
            // en "archivo no válido" en lugar de propagar la excepción.
            return null;
        }
    }

    /// <inheritdoc/>
    public string FormatFileSize(long sizeInBytes)
    {
        double size = sizeInBytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < SizeUnits.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{size:0} {SizeUnits[unitIndex]}"
            : $"{size:0.##} {SizeUnits[unitIndex]}";
    }

    /// <inheritdoc/>
    public void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <inheritdoc/>
    public string ResolveOutputPath(FileItem file, string outputDirectory, ConversionOptions options)
    {
        var baseFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var targetDirectory = options.CreateFolderPerDocument
            ? Path.Combine(outputDirectory, SanitizeForPath(baseFileName))
            : outputDirectory;

        EnsureDirectoryExists(targetDirectory);

        var outputPath = Path.Combine(targetDirectory, baseFileName + ".md");

        if (options.OverwriteExistingFiles || !File.Exists(outputPath))
        {
            return outputPath;
        }

        // Se genera un nombre alternativo único para no sobrescribir un archivo existente
        // cuando la opción correspondiente está desactivada.
        var counter = 1;
        string candidatePath;
        do
        {
            candidatePath = Path.Combine(targetDirectory, $"{baseFileName} ({counter}).md");
            counter++;
        } while (File.Exists(candidatePath));

        return candidatePath;
    }

    /// <inheritdoc/>
    public void OpenFolderInExplorer(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{directoryPath}\"",
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // No se pudo abrir el explorador (por ejemplo, en un entorno restringido);
            // se ignora silenciosamente ya que no es una operación crítica.
        }
    }

    /// <inheritdoc/>
    public void CopyToClipboard(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch
        {
            // El acceso al portapapeles puede fallar si otro proceso lo tiene bloqueado;
            // no se considera un error fatal para la conversión.
        }
    }

    private static string SanitizeForPath(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "documento" : sanitized;
    }
}
