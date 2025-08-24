@echo off
setlocal

set TAR_FOLDER=publish\wsl
set TAR_FILE=%TAR_FOLDER%\ubuntu2204.tar
set DISTRO_NAME=mcportal-test
set IMPORT_PATH=%TAR_FOLDER%\%DISTRO_NAME%

call Resources\Windows\publish-linux.bat

if exist "%TAR_FILE%" GOTO startup

echo Exporting distribution...
if not exist "%TAR_FOLDER%" mkdir "%TAR_FOLDER%"
echo Exporting Ubuntu-22.04 to %TAR_FILE% (this may take a while)...
wsl --export Ubuntu-22.04 "%TAR_FILE%"

:startup

echo Unregistering previous distribution...
wsl --unregister %DISTRO_NAME% > nul 2>&1

echo Importing distribution...
wsl --import %DISTRO_NAME% "%IMPORT_PATH%" "%TAR_FILE%" --version 2

echo Initializing distribution...
wsl -d %DISTRO_NAME% -- sudo bash Resources/Linux/init-portal.sh

echo Launching distribution for test...
wsl -d %DISTRO_NAME%

echo Tearing down distribution...
wsl --unregister %DISTRO_NAME%

echo Sandbox complete.
pause

endlocal
