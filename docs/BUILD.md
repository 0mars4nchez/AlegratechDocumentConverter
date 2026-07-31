# Guía de compilación y empaquetado

## 1. Abrir y ejecutar en Visual Studio 2022 (modo desarrollo)

1. Abra `AlegratechDocumentConverter.sln` en Visual Studio 2022 (17.12 o superior), con la
   carga de trabajo **".NET desktop development"** instalada.
2. Visual Studio restaurará automáticamente los paquetes NuGet (`CommunityToolkit.Mvvm`,
   `SharpVectors.Wpf`) al abrir la solución.
3. Antes de poder **convertir** documentos en modo desarrollo, prepare el entorno de Python
   embebido (paso 2 más abajo) al menos una vez; la interfaz funciona sin él, pero mostrará
   una advertencia y la conversión fallará hasta que exista `Python\python.exe`.
4. Presione **F5** para compilar y ejecutar.

## 2. Preparar el entorno de Python embebido

Este paso se realiza **una sola vez** en la máquina de desarrollo (o cada vez que se quiera
actualizar la versión de Python o de MarkItDown). El resultado se reutiliza tanto para depurar
en Visual Studio como para generar el instalador final.

```powershell
cd Python
powershell -ExecutionPolicy Bypass -File .\setup_python_embed.ps1
```

El script descarga la distribución "embeddable" oficial de Python (python.org), instala `pip`
mediante `get-pip.py` y ejecuta `pip install -r requirements.txt` para instalar MarkItDown y
todas sus dependencias opcionales (`markitdown[all]`) directamente dentro de `Python\`.

Al finalizar, la carpeta `Python\` debe contener, entre otros: `python.exe`,
`Lib\site-packages\markitdown\`, `markitdown_convert.py` y `requirements.txt`.

> Para depurar la aplicación WPF directamente desde Visual Studio (F5), copie o enlace la
> carpeta `Python\` resultante junto al ejecutable de salida
> (`src\AlegratechDocumentConverter\bin\Debug\net9.0-windows\Python`), ya que
> `PythonRuntimeService` busca el entorno en la carpeta `Python` junto al `.exe` en ejecución.

## 3. Generar el instalador final (Setup.exe)

Con el entorno de Python ya preparado (paso 2), ejecute:

```bat
installer\build_installer.bat
```

Este script automatiza:

1. `dotnet restore` y `dotnet publish` en modo **Release**, `win-x64`, **self-contained**
   (el usuario final no necesita instalar el runtime de .NET).
2. Copia de `Python\` dentro de la carpeta de publicación.
3. Compilación de `installer\AlegratechSetup.iss` con Inno Setup 6 (`ISCC.exe`).

El instalador final se genera en `dist\AlegratechDocumentConverterSetup.exe`. Al ejecutarlo,
la aplicación queda completamente instalada y funcional: no requiere instalar Python, pip,
Visual Studio Code ni el runtime de .NET por separado.

## 4. Actualizar la versión de MarkItDown

Edite `Python\requirements.txt` con la versión deseada y vuelva a ejecutar
`Python\setup_python_embed.ps1` (paso 2) antes de generar un nuevo instalador.
