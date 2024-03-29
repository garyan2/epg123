@ECHO OFF
CLS

SETLOCAL ENABLEEXTENSIONS
SET COMPILER=C:\Program Files (x86)\Inno Setup 5\iscc.exe
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
CHOICE /c yn /m "Do you wish to sign the installer"
IF ERRORLEVEL==2 "%COMPILER%" /Qp "epg123InnoSetupScript.iss"
IF ERRORLEVEL==1 "%COMPILER%" /Qp "epg123InnoSetupScript.iss" /DSIGN_INSTALLER

ECHO Copying release files to portable folder ...
COPY /Y "%RELEASE%\epg123.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123_gui.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123_gui.exe.config" "%PORTABLE%"
COPY /Y "%RELEASE%\hdhr2mxf.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\Newtonsoft.Json.dll" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Client.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Client.exe.config" "%PORTABLE%"
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

SET /P VER=Enter version number for zip files:

ECHO Creating zip file for installation package ...
DEL "%BASE%\epg123Setup_v%VER%.zip"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%SETUP%', '%BASE%\epg123Setup_v%VER%.zip'); }"

ECHO Creating zip file for portable package ...
DEL "%BASE%\epg123_v%VER%.zip"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%PORTABLE%', '%BASE%\epg123_v%VER%.zip'); }"

