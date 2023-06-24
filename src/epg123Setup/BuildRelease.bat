@ECHO OFF
CLS

REM Enable command extensions
SETLOCAL ENABLEEXTENSIONS
SET VER="1.8.0.4"
SET COMPILER=C:\Program Files (x86)\Inno Setup 5\compil32.exe
SET BASE=..\..\bin\output
SET PORTABLE=%BASE%\portable
SET SETUP=%BASE%\setup
SET RELEASE=..\..\bin\Release

ECHO Clearing last build contents ...
RD /S /Q "%PORTABLE%"
MD "%PORTABLE%"
RD /S /Q "%SETUP%"
MD "%SETUP%"

ECHO Creating installation package ...
DEL "%BASE%\epg123Setup_v%VER%.zip"
"%COMPILER%" /cc "epg123InnoSetupScript.iss"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%SETUP%', '%BASE%\epg123Setup_v%VER%.zip'); }"

ECHO Copying files to release folders ...
COPY /Y "%RELEASE%\epg123.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123_gui.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123_gui.exe.config" "%PORTABLE%"

REM COPY /Y "%RELEASE%\epg123Server.exe" "%PORTABLE%"

COPY /Y "%RELEASE%\hdhr2mxf.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\plutotv.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\stirrtv.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\Newtonsoft.Json.dll" "%PORTABLE%"

COPY /Y "%RELEASE%\epg123Client.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Client.exe.config" "%PORTABLE%"
REM COPY /Y "%RELEASE%\epgTray.exe" "%PORTABLE%"
REM COPY /Y "%RELEASE%\epgTray.exe.config" "%PORTABLE%"

COPY /Y "%RELEASE%\epg123Transfer.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Transfer.exe.config" "%PORTABLE%"

COPY /Y "%RELEASE%\logViewer.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\logViewer.exe.config" "%PORTABLE%"

COPY /Y "%RELEASE%\GaRyan2.Github.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\GaRyan2.MxfXmltvTools.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\GaRyan2.SchedulesDirect.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\GaRyan2.Tmdb.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\GaRyan2.Utilities.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\GaRyan2.WmcUtilities.dll" "%PORTABLE%"

COPY /Y "docs\license.rtf" "%PORTABLE%"

REM COPY /Y "docs\customLineup.xml.example" "%PORTABLE%"

ECHO Zipping up portable files ...
DEL "%BASE%\epg123_v%VER%.zip"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%PORTABLE%', '%BASE%\epg123_v%VER%.zip'); }"
