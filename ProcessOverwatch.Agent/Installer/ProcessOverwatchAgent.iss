#define MyAppName "Process Overwatch Service"
#define MyAppShortName "Process Overwatch Service"
#define MyAppVersion GetVersionNumbersString("..\bin\Release\net9.0-windows\publish\ProcessOverwatch.Agent.exe")
#define Copyright = 'Copyright © '+GetDateTimeString('yyyy','','')

[Setup]
AppId={{668659C0-018A-4901-B5A7-969F85E4757D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVername={#MyAppName}
AppCopyright={#Copyright}
DefaultDirName={commonpf}\ProcessOverwatch\Agent
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputBaseFilename={#MyAppShortName} Setup {#MyAppVersion}
UninstallDisplayIcon={uninstallexe}
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: {app}; Permissions: everyone-modify

[Files]
Source: "..\bin\Release\net9.0-windows\publish\ProcessOverwatch.Agent.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\publish\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net9.0-windows\publish\appsettings.Development.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Uninstall ProcessOverwatch Agent"; Filename: "{uninstallexe}"

[Run]
Filename: {sys}\sc.exe; Parameters: "create ProcessOverwatchService start=auto binPath= ""{app}\ProcessOverwatch.Agent.exe"" DisplayName= ""Process Overwatch Service""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "start processoverwatchservice" ; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop ProcessOverwatchService" ; Flags: runhidden 
Filename: {sys}\sc.exe; Parameters: "delete ProcessOverwatchService" ; Flags: runhidden

[Code]
const
  MyServiceName = 'ProcessOverwatchService'; 
  
function DoesServiceExist(ServiceName: String): Boolean;
var
  ResultCode: Integer;
begin
  if Exec(ExpandConstant('{sys}\sc.exe'), 'query "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := (ResultCode = 0)
  else
    Result := False;
end;

function StopService(ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  // Attempt to stop the service; if already stopped it will succeed silently
  if Exec(ExpandConstant('{sys}\sc.exe'), 'stop "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := (ResultCode = 0)
  else
    Result := False;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Installed: Boolean;
  Stopped: Boolean;
begin
  Result := True; // allow installer to continue by default

  if CurPageID = wpReady then
  begin
    Installed := DoesServiceExist(MyServiceName);
    if Installed then
    begin
      Stopped := StopService(MyServiceName);
      if not Stopped then
      begin
        MsgBox('The service "' + MyServiceName + '" could not be stopped. Please stop it manually before continuing installation.', mbError, MB_OK);
        Result := False; // cancel install and stay on the Ready page
      end;
    end;
  end;
end;
