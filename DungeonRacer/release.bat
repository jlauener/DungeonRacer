@ECHO OFF
SET app_name=DungeonRacer
SET build_folder=bin\DesktopGL\AnyCPU\Release
SET release_folder=release

IF EXIST %release_folder% (
  ECHO you are about to remove previous release!
  RMDIR %release_folder% /s
)

MKDIR %release_folder%

REM Windows build

COPY %build_folder%\MonoGame.Framework.dll %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup
MKDIR %release_folder%\x86
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\x86\SDL2.dll %release_folder%\x86
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\x86\soft_oal.dll %release_folder%\x86
IF %errorlevel% neq 0 GOTO :cleanup
MKDIR %release_folder%\x64
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\x64\SDL2.dll %release_folder%\x64
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\x64\soft_oal.dll %release_folder%\x64
IF %errorlevel% neq 0 GOTO :cleanup

COPY readme.txt %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup

COPY %build_folder%\MonoPunk.dll %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup

COPY %build_folder%\%app_name%.exe %release_folder%\%app_name%.exe
IF %errorlevel% neq 0 GOTO :cleanup

XCOPY %build_folder%\Content %release_folder%\Content /D /E /C /R /I /K /Y 
IF %errorlevel% neq 0 GOTO :cleanup

ECHO Released successful!
PAUSE
EXIT

:cleanup
IF EXIST %release_folder% RMDIR %release_folder% /S /Q

:error
ECHO Released failed with error #%errorlevel%.
PAUSE