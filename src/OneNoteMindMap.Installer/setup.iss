#ifndef InstallerArch
  #define InstallerArch "x64"
#endif

#if InstallerArch == "x86"
  #define InstallerSuffix "x86"
  #define InstallerDefaultDir "{autopf32}\OneNoteMindMap"
  #define InstallerArchitectures "x86compatible"
  #define ComRoot "HKCR32"
  #define RegAsmPath "{dotnet40}\RegAsm.exe"
  #define ComRegistrationMessage "正在注册 32 位 COM 组件..."
#else
  #define InstallerSuffix "x64"
  #define InstallerDefaultDir "{autopf64}\OneNoteMindMap"
  #define InstallerArchitectures "x64compatible"
  #define ComRoot "HKCR64"
  #define RegAsmPath "{dotnet4064}\RegAsm.exe"
  #define ComRegistrationMessage "正在注册 64 位 COM 组件..."
#endif

#define MyAppName "OneNote 脑图"
#define MyAppVersion "1.5.1"
#define MyAppPublisher "OneNoteMindMap"
#define MyProgId "OneNoteMindMap.Connect"
#define MyGuid "{{A4CEC0EF-4C6C-4CBD-9112-B83545EEADE8}"

[Setup]
AppId={{6AF2A61D-CCB8-459F-AD4B-5B716666690F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={#InstallerDefaultDir}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=OneNoteMindMapSetup-{#MyAppVersion}-{#InstallerSuffix}
ArchitecturesAllowed={#InstallerArchitectures}
#if InstallerArch == "x64"
ArchitecturesInstallIn64BitMode=x64compatible
#endif
PrivilegesRequired=admin
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Files]
Source: "..\OneNoteMindMap.AddIn\bin\Release\OneNoteMindMap.AddIn.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.Core\bin\Release\OneNoteMindMap.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.Editor\bin\Release\OneNoteMindMap.Editor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.AddIn\bin\Release\Extensibility.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.AddIn\bin\Release\office.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.AddIn\bin\Release\Microsoft.Office.Interop.OneNote.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OneNoteMindMap.AddIn\bin\Release\stdole.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
Name: "{group}\OneNote 脑图编辑工具"; Filename: "{app}\OneNoteMindMap.Editor.dll"
Name: "{group}\卸载 OneNote 脑图"; Filename: "{uninstallexe}"

[Run]
Filename: "{#RegAsmPath}"; Parameters: "/codebase ""{app}\OneNoteMindMap.AddIn.dll"""; StatusMsg: "{#ComRegistrationMessage}"; Flags: runhidden waituntilterminated skipifdoesntexist

[UninstallRun]
Filename: "{#RegAsmPath}"; Parameters: "/u ""{app}\OneNoteMindMap.AddIn.dll"""; StatusMsg: "正在注销 COM 组件..."; Flags: runhidden waituntilterminated skipifdoesntexist; RunOnceId: "UnregisterOneNoteMindMap"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Office\OneNote\AddIns\{#MyProgId}"; ValueType: string; ValueName: "Description"; ValueData: "OneNote 思维导图工具"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Office\OneNote\AddIns\{#MyProgId}"; ValueType: string; ValueName: "FriendlyName"; ValueData: "{#MyAppName}"
Root: HKCU; Subkey: "Software\Microsoft\Office\OneNote\AddIns\{#MyProgId}"; ValueType: dword; ValueName: "LoadBehavior"; ValueData: "3"

Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}"; ValueType: string; ValueData: "OneNoteMindMap.AddIn.Connect"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}"; ValueType: string; ValueName: "AppID"; ValueData: "{#MyGuid}"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueData: "mscoree.dll"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Both"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueName: "Class"; ValueData: "OneNoteMindMap.AddIn.Connect"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueName: "Assembly"; ValueData: "OneNoteMindMap.AddIn, Version=1.5.1.0, Culture=neutral, PublicKeyToken=null"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueName: "RuntimeVersion"; ValueData: "v4.0.30319"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32"; ValueType: string; ValueName: "CodeBase"; ValueData: "{app}\OneNoteMindMap.AddIn.dll"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32\1.5.1.0"; ValueType: string; ValueName: "Class"; ValueData: "OneNoteMindMap.AddIn.Connect"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32\1.5.1.0"; ValueType: string; ValueName: "Assembly"; ValueData: "OneNoteMindMap.AddIn, Version=1.5.1.0, Culture=neutral, PublicKeyToken=null"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32\1.5.1.0"; ValueType: string; ValueName: "RuntimeVersion"; ValueData: "v4.0.30319"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\InprocServer32\1.5.1.0"; ValueType: string; ValueName: "CodeBase"; ValueData: "{app}\OneNoteMindMap.AddIn.dll"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\ProgId"; ValueType: string; ValueData: "{#MyProgId}"
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\Programmable"; ValueType: string; ValueData: ""
Root: {#ComRoot}; Subkey: "CLSID\{#MyGuid}\VersionIndependentProgID"; ValueType: string; ValueData: "{#MyProgId}"

Root: {#ComRoot}; Subkey: "AppID\{#MyGuid}"; ValueType: string; ValueData: "OneNoteMindMap.AddIn"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "AppID\{#MyGuid}"; ValueType: string; ValueName: "DllSurrogate"; ValueData: ""
Root: {#ComRoot}; Subkey: "AppID\OneNoteMindMap.AddIn.dll"; ValueType: string; ValueName: "AppID"; ValueData: "{#MyGuid}"; Flags: uninsdeletekey

Root: {#ComRoot}; Subkey: "{#MyProgId}"; ValueType: string; ValueData: "OneNoteMindMap.AddIn.Connect"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "{#MyProgId}\CLSID"; ValueType: string; ValueData: "{#MyGuid}"
Root: {#ComRoot}; Subkey: "{#MyProgId}\CurVer"; ValueType: string; ValueData: "{#MyProgId}.1"
Root: {#ComRoot}; Subkey: "{#MyProgId}.1"; ValueType: string; ValueData: "OneNoteMindMap.AddIn.Connect"; Flags: uninsdeletekey
Root: {#ComRoot}; Subkey: "{#MyProgId}.1\CLSID"; ValueType: string; ValueData: "{#MyGuid}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
const
  DotNet48Release = 528040;

function IsDotNet48Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and
    (Release >= DotNet48Release);
  if (not Result) and IsWin64 then
    Result := RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and
      (Release >= DotNet48Release);
end;

function InitializeSetup: Boolean;
begin
  Result := IsDotNet48Installed;
  if not Result then
    MsgBox('需要先安装 .NET Framework 4.8 才能继续安装 OneNote 脑图。', mbCriticalError, MB_OK);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    RegDeleteKeyIncludingSubkeys(HKEY_CURRENT_USER, 'Software\Microsoft\Office\OneNote\AddIns\OneNoteMindMap.Connect');
    RegDeleteKeyIncludingSubkeys(HKEY_CURRENT_USER, 'Software\Microsoft\Office\OneNote\AddInsData\OneNoteMindMap.Connect');
  end;
end;
