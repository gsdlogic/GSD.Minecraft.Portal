@echo off
setlocal

set SOLUTION=GSD.Minecraft.Portal\GSD.Minecraft.Portal.sln
set PROJECT=GSD.Minecraft.Portal\Source\GSD.Minecraft.Portal\GSD.Minecraft.Portal.csproj
set OUTPUT=publish\linux

echo Cleaning previous publish...
if exist %OUTPUT% rmdir /s /q %OUTPUT%

echo Publishing project...
dotnet publish %PROJECT% -c Release -o %OUTPUT% -r linux-x64 --self-contained true

echo Done. Output in %OUTPUT%
endlocal
