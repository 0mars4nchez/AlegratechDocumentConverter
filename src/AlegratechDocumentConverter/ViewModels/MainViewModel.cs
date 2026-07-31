using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;
using LogLevel = AlegratechDocumentConverter.Models.LogLevel;

namespace AlegratechDocumentConverter.ViewModels;

/// <summary>
/// ViewModel principal de la aplicación. Orquesta la selección de archivos, la configuración
/// de opciones de conversión, el directorio de salida y el proceso completo de conversión a
/// Markdown, delegando cada responsabilidad concreta en los servicios inyectados (patrón MVVM
/// con inversión de dependencias, conforme a SOLID).
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IMarkItDownService _markItDownService;
    private readonly IFileService _fileService;
    private readonly ISettingsService _settingsService;
    private readonly ILoggerService _loggerService;
    private readonly IDialogService _dialogService;
    private readonly IThemeService _themeService;

    private CancellationTokenSource? _conversionCts;

    private AppSettings _loadedSettings = new();

    /// <summary>Colección de archivos seleccionados por el usuario, en el orden de adición.</summary>
    public ObservableCollection<FileItemViewModel> Files { get; } = new();

    /// <summary>Líneas de texto mostradas en el panel de log inferior.</summary>
    public ObservableCollection<string> LogLines { get; } = new();

    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private bool _keepImages;

    [ObservableProperty]
    private bool _createFolderPerDocument;

    [ObservableProperty]
    private bool _openFolderWhenFinished = true;

    [ObservableProperty]
    private bool _overwriteExistingFiles;

    [ObservableProperty]
    private bool _showDetailedLog;

    [ObservableProperty]
    private bool _extractMetadata;

    [ObservableProperty]
    private bool _copyMarkdownToClipboard;

    [ObservableProperty]
    private bool _isConverting;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string _remainingTimeText = "--:--";

    [ObservableProperty]
    private string _speedText = "0 archivos/s";

    [ObservableProperty]
    private string _statusSummaryText = "Listo para convertir";

    public MainViewModel(
        IMarkItDownService markItDownService,
        IFileService fileService,
        ISettingsService settingsService,
        ILoggerService loggerService,
        IDialogService dialogService,
        IThemeService themeService)
    {
        _markItDownService = markItDownService;
        _fileService = fileService;
        _settingsService = settingsService;
        _loggerService = loggerService;
        _dialogService = dialogService;
        _themeService = themeService;

        _loggerService.EntryLogged += OnLogEntryLogged;

        LoadPersistedSettings();
    }

    private void LoadPersistedSettings()
    {
        try
        {
            var settings = _settingsService.Load();
            _loadedSettings = settings;
            OutputDirectory = string.IsNullOrWhiteSpace(settings.LastOutputDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : settings.LastOutputDirectory;

            var options = settings.LastConversionOptions;
            KeepImages = options.KeepImages;
            CreateFolderPerDocument = options.CreateFolderPerDocument;
            OpenFolderWhenFinished = options.OpenFolderWhenFinished;
            OverwriteExistingFiles = options.OverwriteExistingFiles;
            ShowDetailedLog = options.ShowDetailedLog;
            ExtractMetadata = options.ExtractMetadata;
            CopyMarkdownToClipboard = options.CopyMarkdownToClipboard;
        }
        catch (Exception ex)
        {
            _loggerService.LogWarning($"No se pudo cargar la configuración previa: {ex.Message}");
        }
    }

    /// <summary>
    /// Persiste la configuración actual (carpeta de salida, opciones, tema y geometría de
    /// ventana). La geometría de ventana se combina desde el objeto proporcionado por la vista.
    /// </summary>
    public void PersistSettings(double windowWidth, double windowHeight, double? windowLeft, double? windowTop, bool isMaximized)
    {
        var settings = new AppSettings
        {
            LastOutputDirectory = OutputDirectory,
            Theme = AppTheme.Light,
            WindowWidth = windowWidth,
            WindowHeight = windowHeight,
            WindowLeft = windowLeft,
            WindowTop = windowTop,
            IsMaximized = isMaximized,
            LastConversionOptions = new ConversionOptions
            {
                KeepImages = KeepImages,
                CreateFolderPerDocument = CreateFolderPerDocument,
                OpenFolderWhenFinished = OpenFolderWhenFinished,
                OverwriteExistingFiles = OverwriteExistingFiles,
                ShowDetailedLog = ShowDetailedLog,
                ExtractMetadata = ExtractMetadata,
                CopyMarkdownToClipboard = CopyMarkdownToClipboard
            }
        };

        _settingsService.Save(settings);
    }

    /// <summary>
    /// Devuelve la geometría de ventana persistida en la última ejecución, utilizada por la
    /// vista para restaurar tamaño y posición al iniciar la aplicación.
    /// </summary>
    public (double Width, double Height, double? Left, double? Top, bool IsMaximized) GetWindowGeometry() =>
        (_loadedSettings.WindowWidth, _loadedSettings.WindowHeight, _loadedSettings.WindowLeft, _loadedSettings.WindowTop, _loadedSettings.IsMaximized);

    [RelayCommand]
    private void SelectFiles()
    {
        try
        {
            var paths = _dialogService.ShowOpenFilesDialog();
            AddFiles(paths);
        }
        catch (Exception ex)
        {
            _loggerService.LogError("Error al abrir el diálogo de selección de archivos.", ex);
            _dialogService.ShowError($"No se pudieron seleccionar los archivos:\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void BrowseOutputDirectory()
    {
        try
        {
            var selected = _dialogService.ShowFolderBrowserDialog(OutputDirectory);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                OutputDirectory = selected;
            }
        }
        catch (Exception ex)
        {
            _loggerService.LogError("Error al abrir el diálogo de selección de carpeta.", ex);
            _dialogService.ShowError($"No se pudo seleccionar la carpeta de salida:\n{ex.Message}");
        }
    }

    /// <summary>
    /// Comando invocado por <see cref="Commands.DropBehavior"/> cuando el usuario suelta
    /// archivos sobre la zona de arrastrar y soltar (drag &amp; drop).
    /// </summary>
    [RelayCommand]
    private void AddDroppedFiles(string[]? paths)
    {
        if (paths is { Length: > 0 })
        {
            AddFiles(paths);
        }
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        var addedCount = 0;
        var skippedCount = 0;

        foreach (var path in paths)
        {
            try
            {
                if (Files.Any(f => string.Equals(f.FullPath, path, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var item = _fileService.CreateFileItem(path);
                if (item is null)
                {
                    skippedCount++;
                    continue;
                }

                Files.Add(new FileItemViewModel(item, _fileService));
                addedCount++;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"No se pudo añadir el archivo '{path}'.", ex);
            }
        }

        if (addedCount > 0)
        {
            _loggerService.LogInfo($"Se añadieron {addedCount} archivo(s) a la lista.");
        }

        if (skippedCount > 0)
        {
            _loggerService.LogWarning($"Se omitieron {skippedCount} archivo(s) por tener un formato no soportado.");
        }

        ConvertCommand.NotifyCanExecuteChanged();
        RefreshStatusSummary();
    }

    [RelayCommand]
    private void RemoveFile(FileItemViewModel? fileItem)
    {
        if (fileItem is null)
        {
            return;
        }

        Files.Remove(fileItem);
        ConvertCommand.NotifyCanExecuteChanged();
        RefreshStatusSummary();
    }

    [RelayCommand]
    private void ClearFiles()
    {
        Files.Clear();
        ProgressPercentage = 0;
        RemainingTimeText = "--:--";
        SpeedText = "0 archivos/s";
        ConvertCommand.NotifyCanExecuteChanged();
        RefreshStatusSummary();
    }

    private bool CanConvert() => Files.Count > 0 && !IsConverting && !string.IsNullOrWhiteSpace(OutputDirectory);

    /// <summary>
    /// Se invoca automáticamente (generado por CommunityToolkit.Mvvm) cada vez que cambia
    /// <see cref="OutputDirectory"/>, para reevaluar si el botón CONVERTIR debe habilitarse.
    /// </summary>
    partial void OnOutputDirectoryChanged(string value) => ConvertCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanConvert))]
    private async Task ConvertAsync()
    {
        if (!CanConvert())
        {
            return;
        }

        try
        {
            _fileService.EnsureDirectoryExists(OutputDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError("No se pudo crear o acceder al directorio de salida.", ex);
            _dialogService.ShowError($"No se pudo utilizar la carpeta de salida:\n{ex.Message}");
            return;
        }

        _conversionCts = new CancellationTokenSource();
        var token = _conversionCts.Token;

        IsConverting = true;
        ProgressPercentage = 0;
        ConvertCommand.NotifyCanExecuteChanged();

        var options = new ConversionOptions
        {
            KeepImages = KeepImages,
            CreateFolderPerDocument = CreateFolderPerDocument,
            OpenFolderWhenFinished = OpenFolderWhenFinished,
            OverwriteExistingFiles = OverwriteExistingFiles,
            ShowDetailedLog = ShowDetailedLog,
            ExtractMetadata = ExtractMetadata,
            CopyMarkdownToClipboard = CopyMarkdownToClipboard
        };

        var totalFiles = Files.Count;
        var completedFiles = 0;
        var successCount = 0;
        var failureCount = 0;
        long totalBytesProcessed = 0;
        var overallStopwatch = Stopwatch.StartNew();
        string? lastMarkdownContent = null;

        _loggerService.LogInfo($"Iniciando conversión de {totalFiles} archivo(s) hacia '{OutputDirectory}'.");

        foreach (var fileViewModel in Files)
        {
            if (token.IsCancellationRequested)
            {
                fileViewModel.UpdateStatus(ConversionStatus.Skipped, "Cancelado por el usuario.");
                completedFiles++;
                continue;
            }

            fileViewModel.UpdateStatus(ConversionStatus.Processing);
            StatusSummaryText = $"Convirtiendo {fileViewModel.FileName} ({completedFiles + 1} de {totalFiles})...";

            if (ShowDetailedLog)
            {
                _loggerService.LogInfo("Procesando archivo...", fileViewModel.FileName);
            }

            try
            {
                var outputPath = _fileService.ResolveOutputPath(fileViewModel.Model, OutputDirectory, options);
                var result = await _markItDownService
                    .ConvertAsync(fileViewModel.FullPath, outputPath, options, token)
                    .ConfigureAwait(true);

                if (result.IsSuccess)
                {
                    fileViewModel.UpdateStatus(ConversionStatus.Success, null, result.DurationMs, result.OutputMarkdownPath);
                    successCount++;
                    totalBytesProcessed += fileViewModel.Model.SizeInBytes;
                    lastMarkdownContent = result.MarkdownContent ?? lastMarkdownContent;

                    _loggerService.LogSuccess(
                        "Convertido correctamente.",
                        fileViewModel.FileName,
                        result.DurationMs);
                }
                else
                {
                    fileViewModel.UpdateStatus(ConversionStatus.Failed, result.ErrorMessage, result.DurationMs);
                    failureCount++;

                    _loggerService.LogError(
                        result.ErrorMessage ?? "Error desconocido durante la conversión.",
                        null,
                        fileViewModel.FileName);
                }
            }
            catch (Exception ex)
            {
                fileViewModel.UpdateStatus(ConversionStatus.Failed, ex.Message);
                failureCount++;
                _loggerService.LogError("Excepción inesperada durante la conversión del archivo.", ex, fileViewModel.FileName);
            }

            completedFiles++;
            UpdateProgress(completedFiles, totalFiles, overallStopwatch, totalBytesProcessed);
        }

        overallStopwatch.Stop();
        IsConverting = false;
        ConvertCommand.NotifyCanExecuteChanged();

        StatusSummaryText = $"Finalizado: {successCount} correcto(s), {failureCount} con error(es) de {totalFiles} archivo(s).";
        _loggerService.LogInfo(StatusSummaryText);

        if (CopyMarkdownToClipboard && !string.IsNullOrEmpty(lastMarkdownContent))
        {
            _fileService.CopyToClipboard(lastMarkdownContent);
        }

        if (OpenFolderWhenFinished && successCount > 0)
        {
            _fileService.OpenFolderInExplorer(OutputDirectory);
        }

        if (failureCount > 0)
        {
            _dialogService.ShowWarning(
                $"La conversión finalizó con {failureCount} archivo(s) con error. Revise el log para más detalles.");
        }

        _conversionCts?.Dispose();
        _conversionCts = null;
    }

    [RelayCommand]
    private void CancelConversion()
    {
        try
        {
            _conversionCts?.Cancel();
            _loggerService.LogWarning("Se solicitó la cancelación de la conversión en curso.");
        }
        catch (Exception ex)
        {
            _loggerService.LogError("No se pudo cancelar la conversión en curso.", ex);
        }
    }

    private void UpdateProgress(int completedFiles, int totalFiles, Stopwatch overallStopwatch, long totalBytesProcessed)
    {
        ProgressPercentage = totalFiles == 0 ? 0 : (double)completedFiles / totalFiles * 100.0;

        var elapsedSeconds = Math.Max(overallStopwatch.Elapsed.TotalSeconds, 0.001);
        var filesPerSecond = completedFiles / elapsedSeconds;
        SpeedText = filesPerSecond >= 1
            ? $"{filesPerSecond:0.0} archivos/s"
            : $"{(totalBytesProcessed / 1024.0 / elapsedSeconds):0.0} KB/s";

        var remainingFiles = totalFiles - completedFiles;
        if (filesPerSecond > 0 && remainingFiles > 0)
        {
            var remainingSeconds = remainingFiles / filesPerSecond;
            var remaining = TimeSpan.FromSeconds(remainingSeconds);
            RemainingTimeText = remaining.TotalHours >= 1
                ? remaining.ToString(@"hh\:mm\:ss")
                : remaining.ToString(@"mm\:ss");
        }
        else
        {
            RemainingTimeText = "00:00";
        }
    }

    private void RefreshStatusSummary()
    {
        StatusSummaryText = Files.Count == 0
            ? "Listo para convertir"
            : $"{Files.Count} archivo(s) en la lista";
    }

    private void OnLogEntryLogged(LogEntry entry)
    {
        if (entry.Level == LogLevel.Info && !ShowDetailedLog)
        {
            return;
        }

        LogLines.Add(entry.ToDisplayString());

        // Se limita el historial visible en memoria para evitar un crecimiento indefinido
        // durante sesiones largas con muchos archivos.
        const int maxVisibleLines = 500;
        while (LogLines.Count > maxVisibleLines)
        {
            LogLines.RemoveAt(0);
        }
    }
}
