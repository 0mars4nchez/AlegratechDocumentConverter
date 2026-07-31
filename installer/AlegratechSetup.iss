; ============================================================================
; Alegratech Document Converter - Script de Inno Setup
; ----------------------------------------------------------------------------
; Genera un instalador Setup.exe autocontenido que deja la aplicación
; completamente funcional sin que el usuario deba instalar .NET, Python,
; pip ni ninguna otra herramienta.
;
; Requisitos antes de compilar este script:
;   1. Haber ejecutado Python\setup_python_embed.ps1 (una sola vez).
;   2. Haber ejecutado installer\build_installer.bat, que publica la
;      aplicación .NET en self-contained y copia el entorno de Python
;      dentro de la carpeta de publicación.
; ============================================================================

#define MyAppName "Alegratech Document Converter"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Alegratech"
#define MyAppExeName "AlegratechDocumentConverter.exe"
#define MyPublishDir "..\publish"

[Setup]
AppId={{6C6E6B2D-6E38-4C1A-9C6D-9B7E7A4D1F20}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=AlegratechDocumentConverterSetup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupIconFile=..\src\AlegratechDocumentConverter\Resources\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
DisableWelcomePage=no
ShowLanguageDialog=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Copia recursiva de la publicación completa: ejecutable, dependencias .NET
; (self-contained) y la carpeta Python con el entorno embebido + MarkItDown.
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Elimina la configuración y los logs generados por la aplicación al desinstalar.
Type: filesandordirs; Name: "{userappdata}\Alegratech\DocumentConverter"

[Code]
// Verifica que el sistema operativo sea de 64 bits, requisito del entorno de Python embebido
// y de la publicación self-contained en win-x64.
function InitializeSetup(): Boolean;
begin
  if not Is64BitInstallMode then
  begin
    MsgBox('Alegratech Document Converter requiere un sistema operativo Windows de 64 bits.', mbCriticalError, MB_OK);
    Result := False;
  end
  else
  begin
    Result := True;
  end;
end;
