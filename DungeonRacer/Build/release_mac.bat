REM MacOS build
REM @ECHO OFF

SET app_name_source=DungeonRacer
SET app_name_dest=SDR
SET root_folder=..
SET build_folder=%root_folder%\bin\DesktopGL\AnyCPU\Release
SET release_root=release_mac
SET release_folder=%release_root%\SDR.app\Contents\MacOS

IF EXIST %release_root% (
  ECHO you are about to remove previous release!
  RMDIR %release_root% /s
)

MKDIR %release_root%

XCOPY %app_name_dest%.app %release_root%\%app_name_dest%.app /D /E /C /R /I /K /Y 
IF %errorlevel% neq 0 GOTO :cleanup

COPY %build_folder%\MonoGame.Framework.dll %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup
MKDIR %release_folder%\x86
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\libSDL2-2.0.0.dylib %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\libopenal.1.dylib %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\MonoGame.Framework.dll.config %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup
COPY %build_folder%\MonoPunk.dll %release_folder%
IF %errorlevel% neq 0 GOTO :cleanup

COPY %root_folder%\readme.txt %release_root%
IF %errorlevel% neq 0 GOTO :cleanup

COPY macify.sh %release_root%
IF %errorlevel% neq 0 GOTO :cleanup

COPY %build_folder%\%app_name_source%.exe %release_folder%\%app_name_dest%.exe
IF %errorlevel% neq 0 GOTO :cleanup

XCOPY %build_folder%\Content %release_folder%\Content /D /E /C /R /I /K /Y 
IF %errorlevel% neq 0 GOTO :cleanup

ECHO Released successful!
PAUSE
EXIT

:cleanup
IF EXIST %release_root% RMDIR %release_root% /S /Q

:error
ECHO Released failed with error #%errorlevel%.
PAUSE