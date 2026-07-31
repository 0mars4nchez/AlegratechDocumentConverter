using CommunityToolkit.Mvvm.ComponentModel;
using AlegratechDocumentConverter.Helpers;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.ViewModels;

/// <summary>
/// ViewModel que envuelve un <see cref="FileItem"/> del dominio para exponerlo a la interfaz
/// gráfica con soporte de notificación de cambios (<see cref="ObservableObject"/>). Mantiene
/// separada la capa de modelos (POCO puro) de la capa de presentación (WPF/MVVM).
/// </summary>
public sealed partial class FileItemViewModel : ObservableObject
{
    private readonly IFileService _fileService;

    /// <summary>Modelo de dominio subyacente.</summary>
    public FileItem Model { get; }

    [ObservableProperty]
    private ConversionStatus _status;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private long? _durationMs;

    [ObservableProperty]
    private string? _outputPath;

    public FileItemViewModel(FileItem model, IFileService fileService)
    {
        Model = model;
        _fileService = fileService;
        _status = model.Status;
        _errorMessage = model.ErrorMessage;
        _durationMs = model.DurationMs;
        _outputPath = model.OutputPath;
    }

    /// <summary>Nombre del archivo de origen.</summary>
    public string FileName => Model.FileName;

    /// <summary>Ruta completa del archivo de origen.</summary>
    public string FullPath => Model.FullPath;

    /// <summary>Tamaño del archivo formateado de forma legible (KB, MB, etc.).</summary>
    public string FileSizeFormatted => _fileService.FormatFileSize(Model.SizeInBytes);

    /// <summary>Etiqueta legible en español del tipo de archivo.</summary>
    public string TypeLabel => FileTypeHelper.GetCategoryLabel(Model.Category);

    /// <summary>Nombre del recurso de icono SVG asociado a la extensión del archivo.</summary>
    public string IconKey => FileTypeHelper.GetIconKey(Model.Extension);

    /// <summary>Etiqueta legible en español del estado actual de conversión.</summary>
    public string StatusLabel => FileTypeHelper.GetStatusLabel(Status);

    /// <summary>
    /// Actualiza el estado de la conversión y sincroniza el modelo de dominio subyacente,
    /// notificando también a las propiedades calculadas dependientes (como <see cref="StatusLabel"/>).
    /// </summary>
    public void UpdateStatus(ConversionStatus status, string? errorMessage = null, long? durationMs = null, string? outputPath = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
        DurationMs = durationMs;
        OutputPath = outputPath;

        Model.Status = status;
        Model.ErrorMessage = errorMessage;
        Model.DurationMs = durationMs;
        Model.OutputPath = outputPath;

        OnPropertyChanged(nameof(StatusLabel));
    }
}
