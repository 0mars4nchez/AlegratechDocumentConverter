using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Abstrae la interacción con cuadros de diálogo nativos de Windows (selección de archivos,
/// selección de carpeta y mensajes al usuario), de forma que el ViewModel no dependa
/// directamente de tipos de WPF y sea más fácil de probar.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Muestra el diálogo estándar de selección de archivos, filtrado por las extensiones
    /// soportadas. Devuelve una colección vacía si el usuario cancela.
    /// </summary>
    IReadOnlyList<string> ShowOpenFilesDialog();

    /// <summary>
    /// Muestra el diálogo estándar de selección de carpeta. Devuelve <c>null</c> si el
    /// usuario cancela.
    /// </summary>
    string? ShowFolderBrowserDialog(string? initialDirectory);

    /// <summary>Muestra un mensaje informativo al usuario.</summary>
    void ShowInformation(string message, string title = "Alegratech Document Converter");

    /// <summary>Muestra un mensaje de advertencia al usuario.</summary>
    void ShowWarning(string message, string title = "Alegratech Document Converter");

    /// <summary>Muestra un mensaje de error al usuario.</summary>
    void ShowError(string message, string title = "Alegratech Document Converter");

    /// <summary>Muestra un diálogo de confirmación (sí/no) y devuelve la respuesta del usuario.</summary>
    bool ShowConfirmation(string message, string title = "Alegratech Document Converter");
}
