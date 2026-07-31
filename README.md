# Alegratech Document Converter

Aplicación de escritorio para Windows que ofrece una interfaz gráfica moderna (Fluent Design,
estilo Office 365 / Visual Studio) sobre la librería [MarkItDown](https://github.com/microsoft/markitdown)
de Microsoft, para convertir documentos a Markdown sin que el usuario final necesite instalar
Python, pip ni utilizar la línea de comandos.

## Características

- Arrastrar y soltar (o seleccionar) archivos PDF, DOC/DOCX, PPT/PPTX, XLS/XLSX, CSV, TXT,
  HTML, ZIP, imágenes (JPEG, PNG, TIFF, BMP, GIF, WEBP) y audio (MP3, WAV, M4A).
- Opciones configurables: mantener imágenes, carpeta por documento, abrir carpeta al finalizar,
  sobrescribir existentes, log detallado, extracción de metadatos y copia al portapapeles.
- Progreso en tiempo real: porcentaje, tiempo restante estimado y velocidad.
- Registro de actividad persistente en disco (`%AppData%\Alegratech\DocumentConverter\logs`).
- Configuración persistente en JSON (`%AppData%\Alegratech\DocumentConverter\settings.json`).
- Python embebido y portable: el usuario final no instala nada adicional.

## Arquitectura

Proyecto WPF (.NET 9) en C#, con patrón MVVM (CommunityToolkit.Mvvm) y separación en capas:

```
src/AlegratechDocumentConverter/
├── Models/          Entidades y value objects del dominio (POCO, sin dependencias de WPF)
├── ViewModels/       MainViewModel, FileItemViewModel (CommunityToolkit.Mvvm)
├── Views/             MainWindow (XAML + code-behind mínimo)
├── Services/          MarkItDownService, FileService, SettingsService, ThemeService,
│                      LoggerService, DialogService, PythonRuntimeService (+ interfaces)
├── Helpers/           Utilidades sin estado (formateo, mapeo de iconos, geometría de ventana)
├── Converters/        IValueConverter para binding XAML
├── Commands/          Comportamiento adjunto de Drag & Drop (MVVM puro)
├── Themes/            Colors.xaml, Typography.xaml, Styles.xaml (Fluent Design)
└── Resources/Icons/   Iconos SVG (renderizados con SharpVectors.Wpf)

Python/                Entorno embebido + markitdown_convert.py + requirements.txt
installer/              Script de Inno Setup + script de construcción del instalador
```

La comunicación con MarkItDown se realiza invocando `Python/python.exe` (entorno portable
embebido) como proceso hijo mediante `Process.Start()`, pasándole `markitdown_convert.py` y
leyendo un resultado JSON de su salida estándar. Ver `docs/BUILD.md` para instrucciones
completas de compilación y empaquetado.

## Requisitos para compilar (máquina de desarrollo)

- Visual Studio 2022 (17.12+) con la carga de trabajo ".NET desktop development"
- .NET 9 SDK
- PowerShell (incluido en Windows) para preparar el entorno de Python embebido
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) para generar el instalador

Consulte [`docs/BUILD.md`](docs/BUILD.md) para el procedimiento paso a paso.
