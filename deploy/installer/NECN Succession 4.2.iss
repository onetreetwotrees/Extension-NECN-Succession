#define PackageName      "NECN Succession"
#define PackageNameLong  "NECN Succession Extension"
#define Version          "4.2.2"
#define ReleaseType      "official"
#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6"
#define LandisPlugInDir "C:\Program Files\LANDIS-II\plug-ins"

#include "package (Setup section) v6.0.iss"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put a version number in the file name of this .dll
Source: ..\..\src\bin\Debug\Landis.Extension.Succession.NetEcosystemCarbonNitrogen.dll; DestDir: {#ExtDir}; Flags: replacesameversion


; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: ..\..\src\bin\Debug\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.Biomass-v1.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.BiomassCohorts-v2.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.Climate-v1.1.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.Cohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.LeafBiomassCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.Metadata.dll; DestDir: {#ExtDir}; Flags:replacesameversion
Source: ..\..\src\bin\Debug\Landis.Library.Succession-v5.dll; DestDir: {#ExtDir}; Flags: replacesameversion

;Complete example for testing
Source: ..\examples\*.bat; DestDir: {#AppDir}\examples\NECN Succession; Flags: replacesameversion
Source: ..\examples\*.txt; DestDir: {#AppDir}\examples\NECN Succession; Flags: replacesameversion
Source: ..\examples\*.csv; DestDir: {#AppDir}\examples\NECN Succession; Flags: replacesameversion
Source: ..\examples\*.gis; DestDir: {#AppDir}\examples\NECN Succession; Flags: replacesameversion
Source: ..\examples\*.img; DestDir: {#AppDir}\examples\NECN Succession; Flags: replacesameversion


;Supporting files
Source: ..\calibration\*.csv; DestDir: {#AppDir}\examples\NECN Succession\calibration


;LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "NECN Succession 4.2.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""NECN Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]

[Code]
#include "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
    Result := 0;
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
