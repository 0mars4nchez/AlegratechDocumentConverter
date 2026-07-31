using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de aplicar el tema visual (paleta de colores y estilos) de la
/// aplicación. Actualmente solo soporta el modo claro, pero expone una API preparada
/// para futuros temas adicionales.
/// </summary>
public interface IThemeService
{
    /// <summary>Tema actualmente aplicado.</summary>
    AppTheme CurrentTheme { get; }

    /// <summary>Aplica el tema indicado fusionando los diccionarios de recursos correspondientes.</summary>
    void ApplyTheme(AppTheme theme);
}
