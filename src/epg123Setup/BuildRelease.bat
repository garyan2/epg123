@ECHO OFF
CLS

REM Enable command extensions
SETLOCAL ENABLEEXTENSIONS
SET VER="1.3.9.30"
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

ECHO Copying files to release folders ...
COPY /Y "%RELEASE%\epg123.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\hdhr2mxf.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\Newtonsoft.Json.dll" "%PORTABLE%"

COPY /Y "%RELEASE%\epg123Client.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Client.exe.config" "%PORTABLE%"

COPY /Y "%RELEASE%\epg123Transfer.exe" "%PORTABLE%"
COPY /Y "%RELEASE%\epg123Transfer.exe.config" "%PORTABLE%"

COPY /Y "docs\epg123_Guide.pdf" "%PORTABLE%"

COPY /Y "docs\license.rtf" "%PORTABLE%"

COPY /Y "docs\customLineup.xml.example" "%PORTABLE%"

ECHO Zipping up portable files ...
DEL "%BASE%\epg123_v%VER%.zip"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%PORTABLE%', '%BASE%\epg123_v%VER%.zip'); }"

ECHO Creating installation package ...
DEL "%BASE%\epg123Setup_v%VER%.zip"
"%COMPILER%" /cc "epg123InnoSetupScript.iss"
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('%SETUP%', '%BASE%\epg123Setup_v%VER%.zip'); }"
