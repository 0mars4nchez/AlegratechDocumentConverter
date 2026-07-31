#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
markitdown_convert.py

Script de conversion invocado por "Alegratech Document Converter" (aplicacion WPF) a traves
del entorno de Python portable embebido (Python/python.exe). Recibe la ruta de un archivo de
origen y utiliza la libreria MarkItDown de Microsoft para convertirlo a Markdown, guardando el
resultado en la ruta de salida indicada.

Contrato de comunicacion con la aplicacion C#:
    - Se imprime EXACTAMENTE una linea JSON en la salida estandar (stdout) con el resultado,
      tanto en caso de exito como de error. La aplicacion C# (MarkItDownService) busca esa
      linea para interpretar el resultado.
    - Cualquier informacion de depuracion adicional se escribe en stderr y nunca interfiere
      con el contrato de stdout.
    - Este script nunca debe finalizar por una excepcion no controlada: todos los errores se
      capturan y se traducen en un JSON con "success": false.

Uso:
    python markitdown_convert.py --input "C:\\ruta\\entrada.pdf" --output "C:\\ruta\\salida.md"
                                  [--keep-images] [--metadata-output "C:\\ruta\\meta.json"]
"""

import argparse
import inspect
import json
import os
import sys
import time
import traceback


def build_arg_parser() -> argparse.ArgumentParser:
    """Construye el parser de argumentos de linea de comandos del script."""
    parser = argparse.ArgumentParser(
        description="Convierte un documento a Markdown utilizando la libreria MarkItDown."
    )
    parser.add_argument("--input", required=True, help="Ruta completa del archivo de origen.")
    parser.add_argument("--output", required=True, help="Ruta completa del archivo .md de salida.")
    parser.add_argument(
        "--keep-images",
        action="store_true",
        help="Conserva las imagenes embebidas como data URIs dentro del Markdown generado.",
    )
    parser.add_argument(
        "--metadata-output",
        default=None,
        help="Ruta opcional donde guardar en JSON los metadatos extraidos del documento.",
    )
    return parser


def emit_result(success: bool, output_path: str | None = None, error: str | None = None, duration_ms: int = 0) -> None:
    """Imprime en stdout la unica linea JSON que constituye el contrato con la aplicacion C#."""
    payload = {
        "success": success,
        "output_path": output_path,
        "error": error,
        "duration_ms": duration_ms,
    }
    print(json.dumps(payload, ensure_ascii=False))
    sys.stdout.flush()


def extract_markdown_text(result) -> str:
    """
    Obtiene el texto Markdown del resultado devuelto por MarkItDown, siendo tolerante a las
    variaciones del nombre del atributo entre versiones de la libreria
    (`text_content` en las versiones estables, `markdown` en integraciones mas recientes).
    """
    for attribute_name in ("text_content", "markdown"):
        value = getattr(result, attribute_name, None)
        if value:
            return value
    return str(result)


def convert_document(input_path: str, output_path: str, keep_images: bool, metadata_output: str | None) -> str:
    """
    Ejecuta la conversion propiamente dicha: invoca MarkItDown, escribe el archivo .md
    resultante y, opcionalmente, un archivo de metadatos JSON asociado.
    Devuelve el texto Markdown generado.
    """
    from markitdown import MarkItDown  # Importacion diferida: si la libreria falta, se reporta como error controlado.

    converter = MarkItDown(enable_plugins=False)

    convert_kwargs = {}
    if keep_images:
        try:
            signature = inspect.signature(converter.convert)
            if "keep_data_uris" in signature.parameters:
                convert_kwargs["keep_data_uris"] = True
        except (TypeError, ValueError):
            pass  # Si no se puede inspeccionar la firma, simplemente se omite la opcion.

    result = converter.convert(input_path, **convert_kwargs)
    markdown_text = extract_markdown_text(result)

    output_dir = os.path.dirname(output_path)
    if output_dir:
        os.makedirs(output_dir, exist_ok=True)

    with open(output_path, "w", encoding="utf-8") as handle:
        handle.write(markdown_text)

    if metadata_output:
        write_metadata(result, input_path, markdown_text, metadata_output)

    return markdown_text


def write_metadata(result, input_path: str, markdown_text: str, metadata_output: str) -> None:
    """Guarda un archivo JSON con los metadatos disponibles del documento convertido."""
    metadata = {}

    for attribute_name in ("title", "author", "date", "language"):
        value = getattr(result, attribute_name, None)
        if value:
            metadata[attribute_name] = str(value)

    metadata["source_file"] = os.path.basename(input_path)
    metadata["character_count"] = str(len(markdown_text))

    try:
        with open(metadata_output, "w", encoding="utf-8") as handle:
            json.dump(metadata, handle, ensure_ascii=False, indent=2)
    except OSError:
        # Los metadatos son informacion adicional: un fallo al guardarlos no debe invalidar
        # una conversion que, por lo demas, fue exitosa.
        pass


def main() -> None:
    args = build_arg_parser().parse_args()
    start_time = time.time()

    try:
        if not os.path.isfile(args.input):
            raise FileNotFoundError(f"El archivo de origen no existe: {args.input}")

        output_path = convert_document(args.input, args.output, args.keep_images, args.metadata_output)
        duration_ms = int((time.time() - start_time) * 1000)
        emit_result(True, output_path=args.output, duration_ms=duration_ms)

    except Exception as exc:  # Se captura toda excepcion para preservar el contrato de salida JSON.
        duration_ms = int((time.time() - start_time) * 1000)
        error_message = f"{type(exc).__name__}: {exc}"
        traceback.print_exc(file=sys.stderr)
        emit_result(False, error=error_message, duration_ms=duration_ms)
        sys.exit(1)


if __name__ == "__main__":
    main()
