@echo off
REM Build AWJ Split Screen mod
REM --no-incremental: edited source files can land with an mtime behind the system
REM clock, which makes MSBuild's up-to-date check skip the recompile and print
REM "Build succeeded" while leaving the previous DLL in bin\. Always recompile.
pushd "%~dp0"
dotnet build AWJ_SplitScreen_MelonLoader.sln -c Release --no-incremental
set ERR=%ERRORLEVEL%
popd
exit /B %ERR%
