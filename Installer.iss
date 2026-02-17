; Define version and setup filename with iscc.exe /DMyAppVersion=v1.0 /DMyAppSetupFile=ComicRackSetup_v1.0 Installer.iss
#define MyAppName "ComicRack Community Edition .net9"
#ifndef MyAppVersion
#define MyAppVersion "v0.9.182"
#endif
#ifndef MyAppSetupFile
#define MyAppSetupFile "ComicRackSetup"
#endif
#define MyAppPublisher "ComicRack Community"
#define MyAppURL "https://github.com/nadiar/ComicRackCE"
#define MyAppExeName "ComicRack.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{0FA63C63-846C-49B7-9A4B-553EF8EBEF0B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
ChangesAssociations=yes
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=ComicRack\bin\Release\net9.0-windows\License.txt
PrivilegesRequired=admin
OutputDir=.
OutputBaseFilename={#MyAppSetupFile}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
AlwaysShowComponentsList=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=ComicRack\Icons\uninst_103.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
RestartIfNeededByRun=false

[Messages]
// define wizard title and tray status msg
// both are normally defined in innosetup's default.isl (install folder)
SetupAppTitle = {#MyAppName} {#MyAppVersion} Setup
SetupWindowTitle = {#MyAppName} {#MyAppVersion} Setup 

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

; Options displayed during setup (component selection)
[Types]
Name: "full";    Description: "Full installation";
Name: "typical"; Description: "Typical installation";
Name: "compact"; Description: "Compact installation";
Name: "custom";  Description: "Custom installation"; Flags: iscustom

; The compotent definition
[Components]
Name: "app";       Description: "ComicRack Community Edition (Required)";   Types: full typical compact custom; Flags: fixed
Name: "start_menu";Description: "Start Menu";                               Types: full typical
Name: "desktop";   Description: "Desktop Shortcut";                         Types: full typical
Name: "associate"; Description: "Associate eComic extensions";              Types: full typical
Name: "languages"; Description: "Language Packs";                           Types: full
Name: "additional";Description: "Additional images, icons and backgrounds"; Types: full

[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "ComicRack\bin\Release\net9.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\Changes.txt"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\ComicRack.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\ComicRack.deps.json"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\ComicRack.ini"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\DefaultLists.txt"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\License.txt"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\NewsTemplate.html"; DestDir: "{app}"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\ReadMe.txt"; DestDir: "{app}"; Flags: ignoreversion isreadme; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\Help\*"; DestDir: "{app}\Help"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\Languages\*"; DestDir: "{app}\Languages"; Flags: ignoreversion; Components: languages
Source: "ComicRack\bin\Release\net9.0-windows\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\Resources\Icons\*"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion; Components: additional
Source: "ComicRack\bin\Release\net9.0-windows\Resources\Textures\*"; DestDir: "{app}\Resources\Textures"; Flags: ignoreversion recursesubdirs; Components: additional
Source: "ComicRack\bin\Release\net9.0-windows\Scripts\*"; DestDir: "{app}\Scripts"; Flags: ignoreversion; Components: app
Source: "ComicRack\bin\Release\net9.0-windows\_CommonRedist\VC_redist.x64.exe"; DestDir: {tmp}; Flags: dontcopy

[Registry]
; Comics
Root: HKA; Subkey: "Software\Classes\cYo.ComicRack";                       ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "eComic"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRack\DefaultIcon";           ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"",1"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRack\shell";                 ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "open"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRack\shell\open";            ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "Open eComic with ComicRack CE"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRack\shell\open\command";    ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}""  ""%1"""

; Comic Lists
Root: HKA; Subkey: "Software\Classes\cYo.ComicList";                    ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "eComic List"
Root: HKA; Subkey: "Software\Classes\cYo.ComicList\DefaultIcon";        ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"",2"
Root: HKA; Subkey: "Software\Classes\cYo.ComicList\shell";              ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "open"
Root: HKA; Subkey: "Software\Classes\cYo.ComicList\shell\open";         ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "Import eComic List into ComicRack CE"
Root: HKA; Subkey: "Software\Classes\cYo.ComicList\shell\open\command"; ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"" -il ""%1"""

; ComicRack Plugins
Root: HKA; Subkey: "Software\Classes\cYo.ComicRackPlugin";                       ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "ComicRack Plugin"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRackPlugin\DefaultIcon";           ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"",3"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRackPlugin\shell";                 ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "open"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRackPlugin\shell\open";            ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "Install Plugin into ComicRack CE"
Root: HKA; Subkey: "Software\Classes\cYo.ComicRackPlugin\shell\open\command";    ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"" -ip ""%1"""

; Extensions
Root: HKA; Subkey: "Software\Classes\.cbz";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbz\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbr";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbr\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cb7";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cb7\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbt";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbt\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbw";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbw\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRack"
Root: HKA; Subkey: "Software\Classes\.cbl";                                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicList"
Root: HKA; Subkey: "Software\Classes\.cbl\OpenWithProgIDs";                   ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicList"
Root: HKA; Subkey: "Software\Classes\.crplugin";                              ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueData: "cYo.ComicRackPlugin"
Root: HKA; Subkey: "Software\Classes\.crplugin\OpenWithProgIDs";              ValueType: string; Flags: uninsdeletevalue; Components: associate; ValueName: "cYo.ComicRackPlugin"

; Application specific
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#MyAppExeName}"; ValueType: string; Flags: uninsdeletevalue; ValueData: "{autopf}\{#MyAppName}\{#MyAppExeName}"
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}";                       ValueType: string; Flags: uninsdeletevalue; ValueData: "{#MyAppName}"; ValueName: "FriendlyAppName"
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\DefaultIcon";           ValueType: string; Flags: uninsdeletevalue; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"",1"
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\shell\open";            ValueType: string; Flags: uninsdeletevalue; ValueData: "{#MyAppName}"; ValueName: "FriendlyAppName"
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\shell\open\command";    ValueType: string; Flags: uninsdeletevalue; ValueData: """{autopf}\{#MyAppName}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes";        ValueType: string; Flags: uninsdeletevalue; ValueName: ".cb7"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes";        ValueType: string; Flags: uninsdeletevalue; ValueName: ".cbz"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes";        ValueType: string; Flags: uninsdeletevalue; ValueName: ".cbr"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes";        ValueType: string; Flags: uninsdeletevalue; ValueName: ".cbt"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes";        ValueType: string; Flags: uninsdeletevalue; ValueName: ".cbw"; ValueData: ""

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}";                Components: start_menu
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}";     Components: start_menu
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; Components: start_menu
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}";          Components: desktop

[Run]
Filename: "{tmp}\VC_redist.x64.exe"; Parameters: "/install /passive /norestart"; \
    Check: Is64BitInstallMode and VC2022RedistNeedsInstall; \
    Flags: waituntilterminated; \
    StatusMsg: "Installing VC++ 2022 redistributables..."
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Set the minimum .NET runtime version. Requires .NET 9.0 Desktop Runtime
const
  NETRuntimeLabel = '.NET 9.0 Desktop Runtime';
const
  NETRuntimeMinVersion = '9.0.0';
const
  NETRuntimeDownload = 'https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.0-windows-x64-installer';
const
  NETRuntimeFilename = 'windowsdesktop-runtime-9.0.0-win-x64.exe';

var
  DownloadPage: TDownloadWizardPage;
var
  ResultCode: Integer;

// Log download progress to log file
function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if Progress = ProgressMax then
    Log(Format('Successfully downloaded file to {tmp}: %s', [FileName]));
  Result := True;
end;

// When the wizard form loads
procedure InitializeWizard;
begin
  // Create the download page
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;

// Download and run the .NET Runtime setup
function DownloadNETRuntime(): Boolean;
begin
  DownloadPage.Clear;
  DownloadPage.Add(NETRuntimeDownload, NETRuntimeFilename, '');
  DownloadPage.Show;
  try
    try
      DownloadPage.Download; // This downloads the file to {tmp}
    except
      if DownloadPage.AbortedByUser then
        Log('Aborted by user.')
      else
        SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
        Log(AddPeriod(GetExceptionMessage))
      Result := False;
    end;
    if Exec(ExpandConstant('{tmp}\'+NETRuntimeFilename), '/install /passive /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then begin
      Result := True;
    end
    else begin
      Log(Format('%s installation failed: [Result Code: %d] {tmp}\%s', [NETRuntimeLabel, ResultCode, NETRuntimeFilename]));
      Result := False;
    end;
  finally
    DownloadPage.Hide;
  end;
end;

function VC2022RedistNeedsInstall: Boolean;
var 
  Version: String;
begin
  if RegQueryStringValue(HKEY_LOCAL_MACHINE,
       'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version',
       Version) then
  begin
    // Is the installed version at least 14.40 ? 
    Log('VC Redist Version check : found ' + Version);
    Result := (CompareStr(Version, 'v14.40.33810.00')<0);
  end
  else 
  begin
    // Not even an old version installed
    Result := True;
  end;
  if (Result) then
  begin
    ExtractTemporaryFile('VC_redist.x64.exe');
  end;
end;

// Check if .NET 9.0 Desktop Runtime is installed by looking for the runtime directory
function IsDotNet9Installed(): Boolean;
var
  RuntimePath: String;
begin
  RuntimePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App\9.0.0');
  Result := DirExists(RuntimePath);
  if not Result then begin
    // Also check for any 9.0.x version
    RuntimePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App');
    if DirExists(RuntimePath) then begin
      // Check via dotnet --list-runtimes would be ideal but directory check is sufficient
      Log('Checking for .NET 9.0 Desktop Runtime in: ' + RuntimePath);
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpReady then begin
    if IsDotNet9Installed() then begin
      Log('.NET 9.0 Desktop Runtime is installed.');
      Result := True;
    end else begin
      Log('.NET 9.0 Desktop Runtime is not installed. Downloading.');
      Result := DownloadNETRuntime()
    end;
  end else
    Result := True;
end;
