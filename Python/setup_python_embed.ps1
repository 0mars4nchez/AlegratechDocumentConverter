<#
.SYNOPSIS
    Prepara el entorno de Python portable embebido para "Alegratech Document Converter".

.DESCRIPTION
    Este script se ejecuta UNA SOLA VEZ durante el proceso de construccion (build) del
    instalador, en la maquina de desarrollo. NO se ejecuta en la maquina del usuario final.
    Descarga la distribucion "embeddable" oficial de Python desde python.org, la configura
    para permitir la instalacion de paquetes (pip) y sitios (site-packages), instala la
    libreria MarkItDown y copia el script de conversion, dejando la carpeta "Python" lista
    para ser incluida por el instalador de Inno Setup.

.PARAMETER PythonVersion
    Version de Python a descargar (debe existir como distribucion "embeddable" de 64 bits).

.PARAMETER TargetDirectory
    Carpeta destino donde se construira el entorno portable. Por defecto, la carpeta "Python"
    junto a este script (la misma que luego se incluye en el instalador).

.EXAMPLE
    ./setup_python_embed.ps1
    ./setup_python_embed.ps1 -PythonVersion 3.12.7
#>

param(
    [string]$PythonVersion = "3.12.7",
    [string]$TargetDirectory = "$PSScriptRoot"
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

$embedZipName = "python-$PythonVersion-embed-amd64.zip"
$embedUrl = "https://www.python.org/ftp/python/$PythonVersion/$embedZipName"
$getPipUrl = "https://bootstrap.pypa.io/get-pip.py"

$tempDirectory = Join-Path $env:TEMP "AlegratechPythonSetup"
New-Item -ItemType Directory -Force -Path $tempDirectory | Out-Null

$embedZipPath = Join-Path $tempDirectory $embedZipName
$getPipPath = Join-Path $tempDirectory "get-pip.py"

Write-Step "Descargando el entorno embebido de Python $PythonVersion..."
Invoke-WebRequest -Uri $embedUrl -OutFile $embedZipPath

Write-Step "Extrayendo el entorno embebido en '$TargetDirectory'..."
if (Test-Path $TargetDirectory) {
    Get-ChildItem -Path $TargetDirectory -Exclude "markitdown_convert.py", "requirements.txt", "setup_python_embed.ps1" |
        Remove-Item -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $TargetDirectory | Out-Null
Expand-Archive -Path $embedZipPath -DestinationPath $TargetDirectory -Force

# Habilita "import site" y añade Lib\site-packages al archivo ._pth generado por la
# distribucion embebida, requisito indispensable para poder usar pip e instalar MarkItDown.
$pthFile = Get-ChildItem -Path $TargetDirectory -Filter "python3*._pth" | Select-Object -First 1
if (-not $pthFile) {
    throw "No se encontró el archivo ._pth en el entorno embebido descargado."
}

Write-Step "Configurando '$($pthFile.Name)' para habilitar site-packages..."
$pthContent = Get-Content $pthFile.FullName
$pthContent = $pthContent -replace '^#\s*import site', 'import site'
if ($pthContent -notcontains "Lib\site-packages") {
    $pthContent += "Lib\site-packages"
}
Set-Content -Path $pthFile.FullName -Value $pthContent

Write-Step "Descargando get-pip.py..."
Invoke-WebRequest -Uri $getPipUrl -OutFile $getPipPath

$pythonExe = Join-Path $TargetDirectory "python.exe"

Write-Step "Instalando pip en el entorno embebido..."
& $pythonExe $getPipPath --no-warn-script-location

Write-Step "Instalando MarkItDown y dependencias (requirements.txt)..."
& $pythonExe -m pip install --no-warn-script-location -r (Join-Path $PSScriptRoot "requirements.txt")

Write-Step "Entorno de Python embebido listo en '$TargetDirectory'."
Write-Host ""
Write-Host "El script 'markitdown_convert.py' y 'requirements.txt' ya se encuentran en esta misma carpeta." -ForegroundColor Green
Write-Host "A continuación, ejecute 'installer\build_installer.bat' para publicar la aplicación y generar el instalador." -ForegroundColor Green
