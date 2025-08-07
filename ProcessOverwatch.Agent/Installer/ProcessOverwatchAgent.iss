#define MyAppName "Process Overwatch Service"
#define MyAppShortName "Process Overwatch Service"
#define MyAppVersion GetVersionNumbersString("..\bin\Release\net9.0-windows\ProcessOverwatch.Agent.exe")
#define Copyright = 'Copyright © '+GetDateTimeString('yyyy','','')

[Setup]
AppId={{668659C0-018A-4901-B5A7-969F85E4757D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppCopyright={#Copyright}
DefaultDirName={commonpf}\ProcessOverwatch\Agent
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppShortName}_Setup_{#MyAppVersion}
UninstallDisplayIcon={uninstallexe}
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: everyone-modify

[Files]
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Agent.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Agent.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Akka.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Akka.Remote.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Codecs.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Handlers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\DotNetty.Transport.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Google.Protobuf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.Binder.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.EnvironmentVariables.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.FileExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Configuration.UserSecrets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Diagnostics.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Diagnostics.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.FileProviders.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.FileProviders.Physical.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.FileSystemGlobbing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Hosting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Hosting.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Logging.EventSource.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.ObjectPool.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Options.ConfigurationExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Microsoft.Win32.SystemEvents.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Agent.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\ProcessOverwatch.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Diagnostics.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Drawing.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Security.Cryptography.ProtectedData.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Security.Permissions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.Windows.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\System.ServiceProcess.ServiceController.dll"; DestDir: "{app}"; Flags: ignoreversion


[Icons]
Name: "{group}\Uninstall ProcessOverwatch Agent"; Filename: "{uninstallexe}"

[Run]
;Filename: {sys}\sc.exe; Parameters: "create processoverwatchservice start=auto binPath= ""{app}\ProcessOverwatch.Agent.exe""" ; Flags: runhidden
; Create the service with a display name and auto start
Filename: {sys}\sc.exe; Parameters: "create ProcessOverwatchService start=auto binPath= ""{app}\ProcessOverwatch.Agent.exe"" DisplayName= ""Process Overwatch Service""" ; Flags: runhidden

; Start the service after creation
Filename: {sys}\sc.exe; Parameters: "start processoverwatchservice" ; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop ProcessOverwatchService" ; Flags: runhidden 
Filename: {sys}\sc.exe; Parameters: "delete ProcessOverwatchService" ; Flags: runhidden

