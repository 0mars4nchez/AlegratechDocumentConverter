namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Nivel de severidad de una entrada del registro (log) de la aplicación.
/// </summary>
public enum LogLevel
{
    /// <summary>Mensaje informativo general.</summary>
    Info,

    /// <summary>Operación completada satisfactoriamente.</summary>
    Success,

    /// <summary>Advertencia que no impide continuar la operación.</summary>
    Warning,

    /// <summary>Error que impidió completar una operación específica.</summary>
    Error
}
