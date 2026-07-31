using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de la persistencia de la configuración de usuario en formato JSON,
/// incluyendo la última carpeta de salida, opciones de conversión, tema y geometría de ventana.
/// </summary>
public interface ISettingsService
{
    /// <summary>Ruta completa del archivo de configuración en disco.</summary>
    string SettingsFilePath { get; }

    /// <summary>
    /// Carga la configuración desde disco. Si el archivo no existe o está corrupto,
    /// devuelve una configuración con valores predeterminados sin lanzar excepciones.
    /// </summary>
    AppSettings Load();

    /// <summary>Persiste la configuración proporcionada en disco de forma atómica.</summary>
    void Save(AppSettings settings);
}
