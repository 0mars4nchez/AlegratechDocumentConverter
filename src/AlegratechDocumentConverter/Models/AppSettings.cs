namespace AlegratechDocumentConverter.Models;

/// <summary>
/// Tema visual de la aplicación. Actualmente solo se implementa <see cref="Light"/>,
/// pero la estructura permite añadir <c>Dark</c> en el futuro sin romper la API.
/// </summary>
public enum AppTheme
{
    Light
}

/// <summary>
/// Configuración persistente de la aplicación, almacenada en disco como JSON mediante
/// <see cref="Services.Interfaces.ISettingsService"/>. Recuerda la última carpeta de salida,
/// las últimas opciones de conversión, el tema y el tamaño/posición de la ventana.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Última carpeta de salida utilizada por el usuario.</summary>
    public string LastOutputDirectory { get; set; } = string.Empty;

    /// <summary>Últimas opciones de conversión seleccionadas.</summary>
    public ConversionOptions LastConversionOptions { get; set; } = new();

    /// <summary>Tema visual seleccionado.</summary>
    public AppTheme Theme { get; set; } = AppTheme.Light;

    /// <summary>Ancho de la ventana principal en píxeles independientes del dispositivo.</summary>
    public double WindowWidth { get; set; } = 1180;

    /// <summary>Alto de la ventana principal en píxeles independientes del dispositivo.</summary>
    public double WindowHeight { get; set; } = 780;

    /// <summary>Posición izquierda de la ventana. <c>null</c> indica que debe centrarse.</summary>
    public double? WindowLeft { get; set; }

    /// <summary>Posición superior de la ventana. <c>null</c> indica que debe centrarse.</summary>
    public double? WindowTop { get; set; }

    /// <summary>Indica si la ventana estaba maximizada la última vez que se cerró.</summary>
    public bool IsMaximized { get; set; }
}
