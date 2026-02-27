@echo off
REM Build AWJ Split Screen mod
pushd "%~dp0"
dotnet build AWJ_SplitScreen_MelonLoader.sln -c Release
set ERR=%ERRORLEVEL%
popd
exit /B %ERR%
