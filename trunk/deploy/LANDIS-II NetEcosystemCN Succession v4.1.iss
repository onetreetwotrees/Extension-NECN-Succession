#define PackageName      "NECN Succession"
#define PackageNameLong  "NECN Succession Extension"
#define Version          "4.1"
#define ReleaseType      "official"
#define ReleaseNumber    "4"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

; #include "J:\Scheller\LANDIS-II\deploy\package (Setup section) v6.0.iss"
#include "package (Setup section) v6.0.iss"
#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6"

[Files]
; Auxiliary libs
Source: ..\src\bin\Debug\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Cohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.LeafBiomassCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Succession.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Metadata.dll; DestDir: {#ExtDir}; Flags:replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Climate.dll; DestDir: {#ExtDir}; Flags: replacesameversion

; Century Succession
Source: ..\src\bin\Debug\Landis.Extension.Succession.NetEcosystemCarbonNitrogen.dll; DestDir: {#ExtDir}; Flags: replacesameversion

; Supporting documents
Source: docs\LANDIS-II Net Ecosystem CN Succession v4.1 User Guide.pdf; DestDir: {#AppDir}\docs
Source: docs\LANDIS-II Climate Library v1.0 User Guide.pdf; DestDir: {#AppDir}\docs
Source: docs\NECN-calibrate-log-metadata.csv; DestDir: {#AppDir}\docs
;; Source: docs\Century-prob-establish-log-metadata.csv; DestDir: {#AppDir}\docs
Source: examples\*.bat; DestDir: {#AppDir}\examples\NECN-succession
Source: examples\*.txt; DestDir: {#AppDir}\examples\NECN-succession
Source: examples\*.csv; DestDir: {#AppDir}\examples\NECN-succession
Source: examples\single_cell_3.img; DestDir: {#AppDir}\examples\NECN-succession
Source: examples\ecoregions.gis; DestDir: {#AppDir}\examples\NECN-succession
Source: examples\initial-communities.gis; DestDir: {#AppDir}\examples\NECN-succession

#define NECNSucc "NetEcosystemCN Succession 4.1.txt"
Source: {#NECNSucc}; DestDir: {#LandisPlugInDir}

;;[InstallDelete]
;;Type: files; Name: "{#AppDir}\bin\Landis.Library.Climate.dll"

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""NECN Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#NECNSucc}"" "; WorkingDir: {#LandisPlugInDir}

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
