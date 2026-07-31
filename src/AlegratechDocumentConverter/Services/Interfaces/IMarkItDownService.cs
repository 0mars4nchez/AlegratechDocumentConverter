using AlegratechDocumentConverter.Models;

namespace AlegratechDocumentConverter.Services.Interfaces;

/// <summary>
/// Servicio responsable de invocar la librería Python MarkItDown (a través del entorno
/// embebido) para convertir un documento a Markdown. Es el único punto de la aplicación
/// que se comunica con el proceso externo de Python.
/// </summary>
public interface IMarkItDownService
{
    /// <summary>
    /// Convierte de forma asíncrona el archivo indicado a Markdown, guardando el resultado
    /// en <paramref name="outputPath"/>. Nunca lanza excepciones: cualquier fallo se refleja
    /// en el <see cref="ConversionResult"/> devuelto.
    /// </summary>
    /// <param name="inputPath">Ruta completa del archivo de origen.</param>
    /// <param name="outputPath">Ruta completa donde se debe guardar el archivo .md resultante.</param>
    /// <param name="options">Opciones de conversión seleccionadas por el usuario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación en curso.</param>
    Task<ConversionResult> ConvertAsync(
        string inputPath,
        string outputPath,
        ConversionOptions options,
        CancellationToken cancellationToken);
}
