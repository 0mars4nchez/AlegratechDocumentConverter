using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de registrar la actividad de la aplicación (información, éxitos,
/// advertencias y errores), tanto en un archivo de log persistente como en memoria para que
/// la interfaz gráfica pueda mostrar el panel de log inferior en tiempo real.
/// </summary>
public interface ILoggerService
{
    /// <summary>Se dispara cada vez que se registra una nueva entrada de log.</summary>
    event Action<LogEntry>? EntryLogged;

    /// <summary>Ruta del archivo de log actualmente en uso.</summary>
    string CurrentLogFilePath { get; }

    /// <summary>Registra un mensaje informativo.</summary>
    void LogInfo(string message, string? fileName = null);

    /// <summary>Registra una operación completada con éxito, opcionalmente con su duración.</summary>
    void LogSuccess(string message, string? fileName = null, long? durationMs = null);

    /// <summary>Registra una advertencia que no interrumpe el flujo de trabajo.</summary>
    void LogWarning(string message, string? fileName = null);

    /// <summary>Registra un error, incluyendo opcionalmente la excepción original.</summary>
    void LogError(string message, Exception? exception = null, string? fileName = null);
}
