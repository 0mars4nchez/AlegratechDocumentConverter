using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using AlegratechDocumentConverter.Helpers;
using AlegratechDocumentConverter.ViewModels;

namespace AlegratechDocumentConverter.Views;

/// <summary>
/// Code-behind de la ventana principal. Se mantiene deliberadamente mínimo, conforme al
/// patrón MVVM: solo contiene la lógica que exige la API de WPF y que no puede expresarse de
/// forma razonable mediante enlaces de datos (restaurar/guardar geometría de ventana y
/// desplazar automáticamente el panel de log hacia la última entrada).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainViewModel viewModel)
            {
                var (width, height, left, top, isMaximized) = viewModel.GetWindowGeometry();

                Width = Math.Max(width, MinWidth);
                Height = Math.Max(height, MinHeight);

                var (clampedLeft, clampedTop) = WindowGeometryHelper.ClampToVisibleArea(left, top, Width, Height);
                Left = clampedLeft;
                Top = clampedTop;

                if (isMaximized)
                {
                    WindowState = WindowState.Maximized;
                }

                viewModel.LogLines.CollectionChanged += OnLogLinesChanged;
            }
        }
        catch
        {
            // Un fallo al restaurar la geometría de la ventana no debe impedir que la
            // aplicación se muestre; en ese caso se conservan los valores por defecto de XAML.
        }
    }

    private void OnLogLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (LogListBox.Items.Count > 0)
        {
            LogListBox.ScrollIntoView(LogListBox.Items[^1]);
        }
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            if (DataContext is MainViewModel viewModel)
            {
                var isMaximized = WindowState == WindowState.Maximized;
                var normalWidth = isMaximized ? RestoreBounds.Width : Width;
                var normalHeight = isMaximized ? RestoreBounds.Height : Height;
                var normalLeft = isMaximized ? RestoreBounds.Left : Left;
                var normalTop = isMaximized ? RestoreBounds.Top : Top;

                viewModel.PersistSettings(normalWidth, normalHeight, normalLeft, normalTop, isMaximized);
            }
        }
        catch
        {
            // No se debe impedir el cierre de la aplicación por un fallo al persistir la configuración.
        }
    }
}
