using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlegratechDocumentConverter.Models;
using AlegratechDocumentConverter.Services.Interfaces;

namespace AlegratechDocumentConverter.Services;

/// <summary>
/// Implementación de <see cref="IMarkItDownService"/> que invoca el intérprete de Python
/// embebido (<c>python.exe</c>) para ejecutar el script <c>markitdown_convert.py</c>, el cual
/// utiliza la librería MarkItDown para transformar el documento de origen en Markdown.
/// La comunicación entre procesos se realiza mediante argumentos de línea de comandos y la
/// lectura de un único documento JSON impreso por el script en su salida estándar.
/// </summary>
public sealed class MarkItDownService : IMarkItDownService
{
    private readonly IPythonRuntimeService _pythonRuntimeService;
    private readonly ILoggerService _loggerService;

    public MarkItDownService(IPythonRuntimeService pythonRuntimeService, ILoggerService loggerService)
    {
        _pythonRuntimeService = pythonRuntimeService;
        _loggerService = loggerService;
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> ConvertAsync(
        string inputPath,
        string outputPath,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var fileName = Path.GetFileName(inputPath);

        if (!_pythonRuntimeService.ValidateRuntime(out var runtimeError))
        {
            stopwatch.Stop();
            return ConversionResult.Failure(runtimeError ?? "El entorno de Python embebido no es válido.", stopwatch.ElapsedMilliseconds);
        }

        Process? process = null;

        try
        {
            var metadataPath = options.ExtractMetadata
                ? Path.ChangeExtension(outputPath, ".metadata.json")
                : null;

            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonRuntimeService.PythonExecutablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetDirectoryName(_pythonRuntimeService.PythonExecutablePath)
            };

            startInfo.ArgumentList.Add(_pythonRuntimeService.ConverterScriptPath);
            startInfo.ArgumentList.Add("--input");
            startInfo.ArgumentList.Add(inputPath);
            startInfo.ArgumentList.Add("--output");
            startInfo.ArgumentList.Add(outputPath);

            if (options.KeepImages)
            {
                startInfo.ArgumentList.Add("--keep-images");
            }

            if (metadataPath is not null)
            {
                startInfo.ArgumentList.Add("--metadata-output");
                startInfo.ArgumentList.Add(metadataPath);
            }

            process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            var stdOutBuilder = new StringBuilder();
            var stdErrBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    stdOutBuilder.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    stdErrBuilder.AppendLine(e.Data);
                }
            };

            if (!process.Start())
            {
                stopwatch.Stop();
                return ConversionResult.Failure("No se pudo iniciar el proceso de Python.", stopwatch.ElapsedMilliseconds);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (cancellationToken.Register(() => TryKill(process)))
            {
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }

            stopwatch.Stop();

            if (cancellationToken.IsCancellationRequested)
            {
                return ConversionResult.Failure("La conversión fue cancelada por el usuario.", stopwatch.ElapsedMilliseconds);
            }

            var stdOut = stdOutBuilder.ToString();
            var scriptResult = TryParseResult(stdOut);

            if (scriptResult is null)
            {
                var stdErr = stdErrBuilder.ToString();
                var errorMessage = string.IsNullOrWhiteSpace(stdErr)
                    ? $"El proceso de Python finalizó con código {process.ExitCode} sin devolver un resultado válido."
                    : stdErr.Trim();

                return ConversionResult.Failure(errorMessage, stopwatch.ElapsedMilliseconds);
            }

            if (!scriptResult.Success)
            {
                return ConversionResult.Failure(
                    scriptResult.Error ?? "Error desconocido durante la conversión.",
                    stopwatch.ElapsedMilliseconds);
            }

            string? markdownContent = null;
            try
            {
                if (scriptResult.OutputPath is not null && File.Exists(scriptResult.OutputPath))
                {
                    markdownContent = await File.ReadAllTextAsync(scriptResult.OutputPath, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception readEx)
            {
                _loggerService.LogWarning(
                    $"No se pudo leer el contenido Markdown generado para copiarlo al portapapeles: {readEx.Message}",
                    fileName);
            }

            IReadOnlyDictionary<string, string>? metadata = null;
            if (metadataPath is not null && File.Exists(metadataPath))
            {
                metadata = TryReadMetadata(metadataPath);
            }

            return ConversionResult.Success(
                scriptResult.OutputPath ?? outputPath,
                stopwatch.ElapsedMilliseconds,
                markdownContent,
                metadata);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return ConversionResult.Failure("La conversión fue cancelada por el usuario.", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _loggerService.LogError("Excepción inesperada al invocar el proceso de Python.", ex, fileName);
            return ConversionResult.Failure($"Error inesperado: {ex.Message}", stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            process?.Dispose();
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // El proceso puede haber finalizado justo antes de intentar cancelarlo; se ignora.
        }
    }

    private PythonScriptResult? TryParseResult(string standardOutput)
    {
        if (string.IsNullOrWhiteSpace(standardOutput))
        {
            return null;
        }

        // El script imprime exactamente una línea JSON con el resultado. Se busca esa línea
        // de forma robusta por si se imprimieron avisos adicionales antes.
        var lines = standardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (!line.StartsWith('{'))
            {
                continue;
            }

            try
            {
                var result = JsonSerializer.Deserialize<PythonScriptResult>(line, JsonOptions);
                if (result is not null)
                {
                    return result;
                }
            }
            catch (JsonException)
            {
                // La línea parecía JSON pero no lo era (o pertenecía a otro mensaje); se continúa buscando.
            }
        }

        return null;
    }

    private IReadOnlyDictionary<string, string>? TryReadMetadata(string metadataPath)
    {
        try
        {
            var json = File.ReadAllText(metadataPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _loggerService.LogWarning($"No se pudieron leer los metadatos generados: {ex.Message}");
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// DTO que refleja el contrato JSON impreso por <c>markitdown_convert.py</c> en su salida
    /// estándar al finalizar la conversión de un archivo.
    /// </summary>
    private sealed class PythonScriptResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("output_path")]
        public string? OutputPath { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("duration_ms")]
        public long DurationMs { get; set; }
    }
}
