; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
; http://www.jrsoftware.org/ishelp/index.php?topic=setupsection

#define MyAppName "Process Overwatch"
#define MyAppShortName "Process Overwatch"
#define MyAppVersion GetVersionNumbersString("..\bin\Release\net9.0-windows\ProcessOverwatch.Controller.exe")
#define Copyright = 'Copyright © '+GetDateTimeString('yyyy','','')


[Setup]
AppId={{CE96205F-A9A3-4DCB-9BC9-EAD482FB8058}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppCopyright={#Copyright}
DefaultDirName={commonpf}\ProcessOverwatch\Controller
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppShortName}_{#MyAppVersion}
UninstallDisplayIcon={uninstallexe}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: everyone-modify

[Files]
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Controller.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Akka.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Codecs.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Handlers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Transport.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Google.Protobuf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.ObjectPool.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Controller.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\ProcessOverwatch Controller"; Filename: "{app}\ProcessOverwatch.Controller.exe"
Name: "{group}\Uninstall ProcessOverwatch Controller"; Filename: "{uninstallexe}"
Name: "{commondesktop}\ProcessOverwatch Controller"; Filename: "{app}\ProcessOverwatch.Controller.exe"; 

[Run]


