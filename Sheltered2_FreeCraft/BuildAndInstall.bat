@echo off
setlocal EnableExtensions
cd /d "%~dp0"
set "GAME=%~1"
if "%GAME%"=="" if exist "%~dp0Sheltered2_Data\Managed\Assembly-CSharp.dll" set "GAME=%~dp0"
if "%GAME%"=="" (
  echo Sheltered 2 game folder is required.
  echo Steam Library - Sheltered 2 - Manage - Browse local files.
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
rem Unity's game assemblies reference the netstandard 2.0 facade.
rem Framework csc.exe cannot resolve these types unless netstandard.dll is referenced.
set "NETSTANDARD="
if exist "%GAME%\Sheltered2_Data\Managed\netstandard.dll" set "NETSTANDARD=%GAME%\Sheltered2_Data\Managed\netstandard.dll"
if not defined NETSTANDARD if exist "%GAME%\Sheltered2_Data\Managed\Facades\netstandard.dll" set "NETSTANDARD=%GAME%\Sheltered2_Data\Managed\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%GAME%\BepInEx\core\netstandard.dll" set "NETSTANDARD=%GAME%\BepInEx\core\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\Facades\netstandard.dll"
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7\Facades\netstandard.dll"
if not defined NETSTANDARD goto missingnetstandard
echo Using netstandard.dll: %NETSTANDARD%
if not exist "Built" mkdir "Built"
echo.
echo Building Free Craft ...
"%CSC%" /nologo /target:library /optimize+ /platform:anycpu /out:"%~dp0Built\Sheltered2.FreeCraft.dll" /reference:"%GAME%\BepInEx\core\BepInEx.dll" /reference:"%GAME%\BepInEx\core\0Harmony.dll" /reference:"%NETSTANDARD%" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp.dll" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp-firstpass.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.CoreModule.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.UI.dll" "%~dp0Source\FreeCraft.cs"
if errorlevel 1 goto compilefail
if not exist "%~dp0Built\Sheltered2.FreeCraft.dll" goto compilefail
if not exist "%GAME%\BepInEx\plugins\FreeCraft" mkdir "%GAME%\BepInEx\plugins\FreeCraft"
copy /Y "%~dp0Built\Sheltered2.FreeCraft.dll" "%GAME%\BepInEx\plugins\FreeCraft\Sheltered2.FreeCraft.dll" > nul
if errorlevel 1 goto copyfail
echo.
echo SUCCESS: Free Craft installed.
echo Location: %GAME%\BepInEx\plugins\FreeCraft\Sheltered2.FreeCraft.dll
echo Check %GAME%\BepInEx\LogOutput.log after launch.
pause
exit /b 0
:missing
echo ERROR: Could not find Sheltered2_Data\Managed\Assembly-CSharp.dll in:
echo %GAME%
goto fail
:missingbep
echo ERROR: Install BepInEx 5.x (Unity Mono x64) into the game folder first.
echo Expected: %GAME%\BepInEx\core\BepInEx.dll
goto fail

:missingnetstandard
echo ERROR: netstandard.dll was not found.
echo Sheltered 2 game folder: %GAME%
echo Look in: %GAME%\Sheltered2_Data\Managed
echo       and: %GAME%\Sheltered2_Data\Managed\Facades
echo Or install the Microsoft .NET Framework 4.8 Developer Pack (includes reference facades).
echo Do not copy random DLLs from third-party download websites.
goto fail
:missingcsc
echo ERROR: Windows .NET Framework C# compiler csc.exe not found.
echo Install .NET Framework developer tools or build with Visual Studio.
goto fail
:compilefail
echo ERROR: Compile failed. Copy the compiler messages to the assistant.
goto fail
:copyfail
echo ERROR: Could not copy the DLL. Close the game and check write access.
:fail
pause
exit /b 1
