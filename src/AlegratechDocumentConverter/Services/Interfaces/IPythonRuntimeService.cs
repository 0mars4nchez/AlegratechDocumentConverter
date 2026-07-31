namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de localizar y validar el entorno de Python portable embebido junto
/// a la aplicación, de forma que el usuario final nunca necesite instalar Python por su cuenta.
/// </summary>
public interface IPythonRuntimeService
{
    /// <summary>Ruta completa al ejecutable <c>python.exe</c> embebido.</summary>
    string PythonExecutablePath { get; }

    /// <summary>Ruta completa al script <c>markitdown_convert.py</c> que realiza la conversión.</summary>
    string ConverterScriptPath { get; }

    /// <summary>
    /// Verifica que tanto el intérprete de Python como el script de conversión y la librería
    /// MarkItDown estén presentes y sean utilizables.
    /// </summary>
    /// <param name="errorMessage">Mensaje descriptivo del problema, si la validación falla.</param>
    bool ValidateRuntime(out string? errorMessage);
}
