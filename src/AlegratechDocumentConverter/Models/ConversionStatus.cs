namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Representa el estado del ciclo de vida de la conversión de un archivo individual.
/// </summary>
public enum ConversionStatus
{
    /// <summary>El archivo fue añadido y espera a ser procesado.</summary>
    Pending,

    /// <summary>El archivo se está procesando actualmente.</summary>
    Processing,

    /// <summary>El archivo se convirtió correctamente.</summary>
    Success,

    /// <summary>Ocurrió un error durante la conversión del archivo.</summary>
    Failed,

    /// <summary>El archivo fue omitido (por ejemplo, ya existía el destino y no se permitió sobrescribir).</summary>
    Skipped
}
