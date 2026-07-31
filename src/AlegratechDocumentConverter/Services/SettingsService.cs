using System.IO;
using System.Text.Json;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="ISettingsService"/> que persiste la configuración de usuario
/// como JSON legible en <c>%AppData%\Alegratech\DocumentConverter\settings.json</c>.
/// La carga es completamente tolerante a fallos: cualquier error de lectura o deserialización
/// se traduce en una configuración con valores predeterminados, nunca en una excepción.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc/>
    public string SettingsFilePath { get; }

    public SettingsService()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Alegratech", "DocumentConverter");

        SettingsFilePath = Path.Combine(directory, "settings.json");
    }

    /// <inheritdoc/>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            // Configuración corrupta o inaccesible: se recurre a los valores predeterminados
            // en lugar de bloquear el arranque de la aplicación.
            return new AppSettings();
        }
    }

    /// <inheritdoc/>
    public void Save(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, SerializerOptions);

            // Escritura atómica: se escribe a un archivo temporal y luego se reemplaza,
            // para evitar dejar el archivo de configuración corrupto ante un cierre abrupto.
            var tempPath = SettingsFilePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, SettingsFilePath, overwrite: true);
            File.Delete(tempPath);
        }
        catch
        {
            // Un fallo al guardar la configuración no debe interrumpir el uso de la aplicación.
        }
    }
}
