; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
; http://www.jrsoftware.org/ishelp/index.php?topic=setupsection

#define MyAppName "Process Overwatch"
#define MyAppShortName "Process Overwatch"
#define MyAppVersion GetVersionNumbersString("..\bin\Release\net9.0-windows7.0\ProcessOverwatch.Controller.exe")
#define Copyright = 'Copyright © '+GetDateTimeString('yyyy','','')
#define MyAppExeName "ProcessOverwatch.Controller.exe"


[Setup]
AppId={{CE96205F-A9A3-4DCB-9BC9-EAD482FB8058}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVername={#MyAppName} {#MyAppVersion}
AppCopyright={#Copyright}
DefaultDirName={commonpf}\ProcessOverwatch\Controller
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=no
OutputBaseFilename={#MyAppShortName} Setup {#MyAppVersion}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
DisableWelcomePage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: everyone-modify

[Files]
Source: "..\bin\Release\net9.0-windows7.0\ProcessOverwatch.Controller.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\ProcessOverwatch.Controller.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Akka.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\DotNetty.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\DotNetty.Codecs.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\DotNetty.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\DotNetty.Handlers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\DotNetty.Transport.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Google.Protobuf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.ObjectPool.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\ProcessOverwatch.Controller.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\ProcessOverwatch.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows7.0\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\ProcessOverwatch Controller"; Filename: "{app}\ProcessOverwatch.Controller.exe"
Name: "{group}\Uninstall ProcessOverwatch Controller"; Filename: "{uninstallexe}"
Name: "{commondesktop}\ProcessOverwatch Controller"; Filename: "{app}\ProcessOverwatch.Controller.exe"; 

