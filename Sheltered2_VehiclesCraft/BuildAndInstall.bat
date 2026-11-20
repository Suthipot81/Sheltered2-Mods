@echo off
setlocal EnableExtensions
cd /d "%~dp0"
set "GAME=%~1"
if "%GAME%"=="" if exist "%~dp0Sheltered2_Data\Managed\Assembly-CSharp.dll" set "GAME=%~dp0"
if "%GAME%"=="" (
  echo Sheltered 2 game folder is required.
  echo Steam - Sheltered 2 - Manage - Browse local files.
  echo Drag the GAME FOLDER into this window and press Enter.
  set /p "GAME=Game folder: "
)
set "GAME=%GAME:"=%"
if "%GAME%"=="" goto missing
if not exist "%GAME%\Sheltered2_Data\Managed\Assembly-CSharp.dll" goto missing
if not exist "%GAME%\BepInEx\core\BepInEx.dll" goto missingbep
if not exist "%GAME%\BepInEx\core\0Harmony.dll" goto missingbep
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" goto missingcsc
set "NETSTANDARD="
if exist "%GAME%\Sheltered2_Data\Managed\netstandard.dll" set "NETSTANDARD=%GAME%\Sheltered2_Data\Managed\netstandard.dll"
if not defined NETSTANDARD if exist "%GAME%\Sheltered2_Data\Managed\Facades\netstandard.dll" set "NETSTANDARD=%GAME%\Sheltered2_Data\Managed\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%GAME%\BepInEx\core\netstandard.dll" set "NETSTANDARD=%GAME%\BepInEx\core\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\Facades\netstandard.dll"
if not defined NETSTANDARD goto missingnetstandard
if not exist "Built" mkdir "Built"
echo.
echo Building Vehicles Craft v0.1.1 ...
"%CSC%" /nologo /target:library /optimize+ /platform:anycpu /out:"%~dp0Built\Sheltered2.VehiclesCraft.dll" /reference:"%GAME%\BepInEx\core\BepInEx.dll" /reference:"%GAME%\BepInEx\core\0Harmony.dll" /reference:"%NETSTANDARD%" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp.dll" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp-firstpass.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.CoreModule.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.UI.dll" "%~dp0Source\VehiclesCraft.cs"
if errorlevel 1 goto compilefail
if not exist "%~dp0Built\Sheltered2.VehiclesCraft.dll" goto compilefail
if not exist "%GAME%\BepInEx\plugins\VehiclesCraft" mkdir "%GAME%\BepInEx\plugins\VehiclesCraft"
copy /Y "%~dp0Built\Sheltered2.VehiclesCraft.dll" "%GAME%\BepInEx\plugins\VehiclesCraft\Sheltered2.VehiclesCraft.dll" > nul
if errorlevel 1 goto copyfail
echo.
echo SUCCESS: Vehicles Craft v0.1.1 installed.
echo Location: %GAME%\BepInEx\plugins\VehiclesCraft\Sheltered2.VehiclesCraft.dll
echo Launch the game, then inspect %GAME%\BepInEx\LogOutput.log.
pause
exit /b 0
:missing
echo ERROR: Could not find Sheltered2_Data\Managed\Assembly-CSharp.dll in:
echo %GAME%
goto fail
:missingbep
echo ERROR: BepInEx 5.x Unity Mono x64 is required at %GAME%\BepInEx\core.
goto fail
:missingnetstandard
echo ERROR: netstandard.dll not found. Install .NET Framework 4.8 Developer Pack or locate the game's netstandard facade.
goto fail
:missingcsc
echo ERROR: Windows .NET Framework C# compiler csc.exe was not found.
goto fail
:compilefail
echo ERROR: Compilation failed. Send the compiler messages for troubleshooting.
goto fail
:copyfail
echo ERROR: Could not copy Sheltered2.VehiclesCraft.dll. Close the game and check write access.
:fail
pause
exit /b 1
