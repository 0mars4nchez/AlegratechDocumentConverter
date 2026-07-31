using System.IO;
using System.Windows;
using Microsoft.Win32;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="IDialogService"/> basada en los diálogos nativos de Windows
/// expuestos por <c>Microsoft.Win32</c> (sin dependencia de Windows Forms).
/// </summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc/>
    public IReadOnlyList<string> ShowOpenFilesDialog()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = SupportedFileTypes.BuildOpenFileDialogFilter(),
            Title = "Seleccionar archivos a convertir"
        };

        return dialog.ShowDialog() == true ? dialog.FileNames : Array.Empty<string>();
    }

    /// <inheritdoc/>
    public string? ShowFolderBrowserDialog(string? initialDirectory)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Seleccionar carpeta de salida",
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    /// <inheritdoc/>
    public void ShowInformation(string message, string title = "Alegratech Document Converter") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    /// <inheritdoc/>
    public void ShowWarning(string message, string title = "Alegratech Document Converter") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    /// <inheritdoc/>
    public void ShowError(string message, string title = "Alegratech Document Converter") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    /// <inheritdoc/>
    public bool ShowConfirmation(string message, string title = "Alegratech Document Converter") =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
