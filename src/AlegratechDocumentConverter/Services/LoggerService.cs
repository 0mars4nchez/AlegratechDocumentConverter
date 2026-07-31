using System.IO;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;
using LogLevel = AlegratechDocumentConverter.Models.LogLevel;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="ILoggerService"/> que escribe cada entrada en un archivo de
/// texto diario bajo <c>%AppData%\Alegratech\DocumentConverter\logs</c> y notifica a los
/// suscriptores (normalmente el ViewModel) para actualizar el panel de log en la interfaz.
/// Todas las excepciones de E/S se capturan internamente: un fallo al escribir el log nunca
/// debe interrumpir la conversión de documentos.
/// </summary>
public sealed class LoggerService : ILoggerService
{
    private readonly object _syncRoot = new();

    /// <inheritdoc/>
    public event Action<LogEntry>? EntryLogged;

    /// <inheritdoc/>
    public string CurrentLogFilePath { get; }

    public LoggerService()
    {
        var logsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Alegratech", "DocumentConverter", "logs");

        try
        {
            Directory.CreateDirectory(logsDirectory);
        }
        catch
        {
            // Si no se puede crear el directorio de logs, se continúa sin persistencia en disco.
        }

        CurrentLogFilePath = Path.Combine(logsDirectory, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
    }

    /// <inheritdoc/>
    public void LogInfo(string message, string? fileName = null) =>
        Write(new LogEntry(DateTime.Now, LogLevel.Info, fileName, message));

    /// <inheritdoc/>
    public void LogSuccess(string message, string? fileName = null, long? durationMs = null) =>
        Write(new LogEntry(DateTime.Now, LogLevel.Success, fileName, message, durationMs));

    /// <inheritdoc/>
    public void LogWarning(string message, string? fileName = null) =>
        Write(new LogEntry(DateTime.Now, LogLevel.Warning, fileName, message));

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null, string? fileName = null)
    {
        var fullMessage = exception is null ? message : $"{message} :: {exception.GetType().Name}: {exception.Message}";
        Write(new LogEntry(DateTime.Now, LogLevel.Error, fileName, fullMessage));
    }

    private void Write(LogEntry entry)
    {
        try
        {
            lock (_syncRoot)
            {
                File.AppendAllText(CurrentLogFilePath, entry.ToDisplayString() + Environment.NewLine);
            }
        }
        catch
        {
            // Se ignora cualquier error de escritura en disco: el registro en memoria (evento)
            // sigue funcionando y la aplicación nunca debe fallar por causa del logging.
        }
        finally
        {
            try
            {
                EntryLogged?.Invoke(entry);
            }
            catch
            {
                // Un suscriptor que lance una excepción no debe afectar al resto de la aplicación.
            }
        }
    }
}
