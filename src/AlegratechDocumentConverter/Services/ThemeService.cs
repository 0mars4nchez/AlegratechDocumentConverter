using System.Windows;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="IThemeService"/>. La aplicación solicita únicamente modo
/// claro (Light) según los requisitos de diseño, pero la clase está preparada para admitir
/// temas adicionales en el futuro sin necesidad de modificar a sus consumidores (principio
/// abierto/cerrado de SOLID).
/// </summary>
public sealed class ThemeService : IThemeService
{
    private const string ColorsDictionaryPath = "Themes/Colors.xaml";

    /// <inheritdoc/>
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    /// <inheritdoc/>
    public void ApplyTheme(AppTheme theme)
    {
        // Actualmente solo existe el diccionario de colores en modo claro. Se deja la
        // estructura de "switch" preparada para cuando se añada un tema oscuro.
        var dictionaryUri = theme switch
        {
            AppTheme.Light => ColorsDictionaryPath,
            _ => ColorsDictionaryPath
        };

        try
        {
            var application = Application.Current;
            if (application is null)
            {
                return;
            }

            var existing = application.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source is not null && d.Source.OriginalString.Contains("Colors.xaml"));

            var newDictionary = new ResourceDictionary
            {
                Source = new Uri(dictionaryUri, UriKind.Relative)
            };

            if (existing is not null)
            {
                var index = application.Resources.MergedDictionaries.IndexOf(existing);
                application.Resources.MergedDictionaries[index] = newDictionary;
            }
            else
            {
                application.Resources.MergedDictionaries.Add(newDictionary);
            }

            CurrentTheme = theme;
        }
        catch
        {
            // Si el tema no se puede aplicar (por ejemplo, en tiempo de diseño), se conserva
            // el tema por defecto ya cargado desde App.xaml, sin interrumpir la aplicación.
        }
    }
}
