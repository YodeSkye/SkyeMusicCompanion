[Setup]
AppName=Skye Music Companion
AppVersion=1.0
AppVerName=Skye Music Companion v1.0
AppPublisher=Skye
DefaultDirName={autopf}\Skye\Skye Music Companion
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=SkyeMusicCompanionSetup

[Files]
Source: "C:\Users\YodeS\Dev\SkyeMusicCompanion\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu shortcut (root, no subfolder)
Name: "{commonprograms}\Skye Music Companion"; Filename: "{app}\SkyeMusicCompanion.exe"

; Optional desktop shortcut
Name: "{commondesktop}\Skye Music Companion"; Filename: "{app}\SkyeMusicCompanion.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"