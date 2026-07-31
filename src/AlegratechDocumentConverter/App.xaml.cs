using System.Windows;
using System.Windows.Threading;
using AlegratechDocumentConverter.Services;
using AlegratechDocumentConverter.Services.Interfaces;
using AlegratechDocumentConverter.ViewModels;
using AlegratechDocumentConverter.Views;

namespace AlegratechDocumentConverter;

/// <summary>
/// Punto de entrada de la aplicación. Actúa como composition root manual (sin contenedor DI
/// externo) para mantener el proyecto ligero, y centraliza el manejo global de excepciones
/// para garantizar que la aplicación nunca se cierre de forma inesperada.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Servicio de registro utilizado también por el manejador global de excepciones.
    /// </summary>
    private ILoggerService? _loggerService;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        RegisterGlobalExceptionHandlers();

        try
        {
            // --- Composition root: se crean las dependencias concretas y se inyectan --------
            ISettingsService settingsService = new SettingsService();
            ILoggerService loggerService = new LoggerService();
            _loggerService = loggerService;

            IThemeService themeService = new ThemeService();
            IDialogService dialogService = new DialogService();
            IFileService fileService = new FileService();
            IPythonRuntimeService pythonRuntimeService = new PythonRuntimeService(loggerService);
            IMarkItDownService markItDownService = new MarkItDownService(pythonRuntimeService, loggerService);

            var settings = settingsService.Load();
            themeService.ApplyTheme(settings.Theme);

            var runtimeIsValid = pythonRuntimeService.ValidateRuntime(out var runtimeError);
            if (!runtimeIsValid)
            {
                loggerService.LogWarning(
                    $"El entorno de Python embebido no se encontró o es inválido: {runtimeError}. " +
                    "La conversión no estará disponible hasta reinstalar la aplicación.");
            }

            var mainViewModel = new MainViewModel(
                markItDownService,
                fileService,
                settingsService,
                loggerService,
                dialogService,
                themeService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            // Bajo ninguna circunstancia la aplicación debe cerrarse por un fallo en el arranque.
            _loggerService?.LogError("Error crítico durante el arranque de la aplicación.", ex);
            MessageBox.Show(
                $"Ocurrió un error al iniciar Alegratech Document Converter:\n\n{ex.Message}\n\n" +
                "Revise el archivo de registro para más detalles.",
                "Alegratech Document Converter",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Registra manejadores de excepciones no controladas en todos los contextos posibles
    /// (hilo de UI, hilos de fondo y tareas asíncronas no observadas) para cumplir el requisito
    /// de que la aplicación nunca se cierre de forma abrupta.
    /// </summary>
    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception, "Error no controlado en el hilo de interfaz.");
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            HandleUnhandledException(ex, "Error no controlado en el dominio de la aplicación.");
        }
    }

    private void OnUnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception, "Error no observado en una tarea asíncrona.");
        e.SetObserved();
    }

    private void HandleUnhandledException(Exception ex, string context)
    {
        try
        {
            _loggerService?.LogError(context, ex);
            MessageBox.Show(
                $"{context}\n\n{ex.Message}\n\nLa aplicación continuará en ejecución.",
                "Alegratech Document Converter",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch
        {
            // Si incluso el manejo de la excepción falla, se ignora silenciosamente
            // para evitar un bucle de errores; nunca se debe permitir el cierre forzado.
        }
    }
}
