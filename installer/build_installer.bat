@echo off
setlocal enabledelayedexpansion

REM ============================================================================
REM Alegratech Document Converter - Script de construccion del instalador
REM ----------------------------------------------------------------------------
REM Ejecuta, en orden, todos los pasos necesarios para producir el instalador
REM final "AlegratechDocumentConverterSetup.exe":
REM   1. Restaura y publica la aplicacion WPF en modo self-contained (win-x64),
REM      de forma que el usuario final no necesite instalar .NET.
REM   2. Copia el entorno de Python embebido (carpeta Python\) ya preparado
REM      mediante Python\setup_python_embed.ps1 dentro de la publicacion.
REM   3. Invoca al compilador de Inno Setup (ISCC.exe) para generar el .exe.
REM
REM Prerrequisitos en la máquina de desarrollo (NO en la del usuario final):
REM   - .NET 9 SDK
REM   - Haber ejecutado: powershell -File Python\setup_python_embed.ps1
REM   - Inno Setup 6 instalado (https://jrsoftware.org/isinfo.php)
REM ============================================================================

cd /d "%~dp0\.."

set "SOLUTION_DIR=%cd%"
set "PROJECT=%SOLUTION_DIR%\src\AlegratechDocumentConverter\AlegratechDocumentConverter.csproj"
set "PUBLISH_DIR=%SOLUTION_DIR%\publish"
set "PYTHON_SOURCE=%SOLUTION_DIR%\Python"
set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"

echo === [1/4] Restaurando dependencias NuGet ===
dotnet restore "%PROJECT%"
if errorlevel 1 goto :error

echo === [2/4] Publicando la aplicacion (Release, win-x64, self-contained) ===
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
dotnet publish "%PROJECT%" ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%PUBLISH_DIR%"
if errorlevel 1 goto :error

echo === [3/4] Copiando el entorno de Python embebido ===
if not exist "%PYTHON_SOURCE%\python.exe" (
    echo.
    echo [ERROR] No se encontro Python\python.exe.
    echo Ejecute primero: powershell -ExecutionPolicy Bypass -File "%PYTHON_SOURCE%\setup_python_embed.ps1"
    goto :error
)
xcopy /e /i /y "%PYTHON_SOURCE%" "%PUBLISH_DIR%\Python" >nul
if errorlevel 1 goto :error

echo === [4/4] Compilando el instalador con Inno Setup ===
if not exist "%ISCC%" (
    echo.
    echo [ERROR] No se encontro Inno Setup 6 en "%ISCC%".
    echo Descarguelo desde https://jrsoftware.org/isinfo.php e instalelo antes de continuar.
    goto :error
)
"%ISCC%" "%SOLUTION_DIR%\installer\AlegratechSetup.iss"
if errorlevel 1 goto :error

echo.
echo ============================================================================
echo  Instalador generado correctamente en: %SOLUTION_DIR%\dist\AlegratechDocumentConverterSetup.exe
echo ============================================================================
goto :eof

:error
echo.
echo [ERROR] La construccion del instalador fallo. Revise los mensajes anteriores.
exit /b 1
