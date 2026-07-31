using System.IO;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="IPythonRuntimeService"/>. Localiza el entorno de Python
/// portable que se distribuye embebido junto al ejecutable de la aplicación (carpeta
/// <c>Python</c> junto al .exe), de modo que el usuario final nunca necesite instalar
/// Python, pip ni ninguna herramienta adicional.
/// </summary>
public sealed class PythonRuntimeService : IPythonRuntimeService
{
    private readonly ILoggerService _loggerService;

    /// <inheritdoc/>
    public string PythonExecutablePath { get; }

    /// <inheritdoc/>
    public string ConverterScriptPath { get; }

    /// <summary>Ruta a la carpeta raíz del entorno Python embebido.</summary>
    public string PythonRootDirectory { get; }

    public PythonRuntimeService(ILoggerService loggerService)
    {
        _loggerService = loggerService;

        PythonRootDirectory = Path.Combine(AppContext.BaseDirectory, "Python");
        PythonExecutablePath = Path.Combine(PythonRootDirectory, "python.exe");
        ConverterScriptPath = Path.Combine(PythonRootDirectory, "markitdown_convert.py");
    }

    /// <inheritdoc/>
    public bool ValidateRuntime(out string? errorMessage)
    {
        if (!File.Exists(PythonExecutablePath))
        {
            errorMessage = $"No se encontró el intérprete de Python embebido en '{PythonExecutablePath}'.";
            _loggerService.LogWarning(errorMessage);
            return false;
        }

        if (!File.Exists(ConverterScriptPath))
        {
            errorMessage = $"No se encontró el script de conversión en '{ConverterScriptPath}'.";
            _loggerService.LogWarning(errorMessage);
            return false;
        }

        var siteLib = Path.Combine(PythonRootDirectory, "Lib", "site-packages", "markitdown");
        if (!Directory.Exists(siteLib))
        {
            errorMessage = "La librería MarkItDown no está instalada en el entorno de Python embebido.";
            _loggerService.LogWarning(errorMessage);
            return false;
        }

        errorMessage = null;
        return true;
    }
}
