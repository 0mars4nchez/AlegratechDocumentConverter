using System.Windows;
using System.Windows.Input;

namespace AlegratechDocumentConverter.Commands;

/// <summary>
/// Propiedad adjunta que permite enlazar el evento nativo <c>Drop</c> de WPF a un
/// <see cref="ICommand"/> del ViewModel, manteniendo la zona de arrastrar y soltar (drag &amp; drop)
/// alineada con el patrón MVVM sin necesidad de lógica de arrastre en el code-behind de la vista.
/// El comando recibe como parámetro un arreglo de rutas (<c>string[]</c>) de los archivos soltados.
/// </summary>
public static class DropBehavior
{
    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(DropBehavior),
            new PropertyMetadata(null, OnDropCommandChanged));

    public static void SetDropCommand(UIElement element, ICommand value) =>
        element.SetValue(DropCommandProperty, value);

    public static ICommand GetDropCommand(UIElement element) =>
        (ICommand)element.GetValue(DropCommandProperty);

    private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        element.AllowDrop = true;
        element.Drop -= OnElementDrop;
        element.DragOver -= OnElementDragOver;

        if (e.NewValue is ICommand)
        {
            element.Drop += OnElementDrop;
            element.DragOver += OnElementDragOver;
        }
    }

    private static void OnElementDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static void OnElementDrop(object sender, DragEventArgs e)
    {
        if (sender is not UIElement element)
        {
            return;
        }

        var command = GetDropCommand(element);
        if (command is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths && command.CanExecute(paths))
        {
            command.Execute(paths);
        }

        e.Handled = true;
    }
}
