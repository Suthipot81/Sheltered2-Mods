@echo off
setlocal EnableExtensions
cd /d "%~dp0"
set "GAME=%~1"
if "%GAME%"=="" if exist "%~dp0Sheltered2_Data\Managed\Assembly-CSharp.dll" set "GAME=%~dp0"
if "%GAME%"=="" (
  echo Sheltered 2 game folder is required.
  echo Steam Library - Sheltered 2 - Manage - Browse local files.
  echo Example: E:\SteamLibrary\steamapps\common\Sheltered 2
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
if not defined NETSTANDARD if exist "%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7\Facades\netstandard.dll" set "NETSTANDARD=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7\Facades\netstandard.dll"
if not defined NETSTANDARD goto missingnetstandard
if not exist "Built" mkdir "Built"
echo.
echo Building 30 Attribute Points ...
"%CSC%" /nologo /target:library /optimize+ /platform:anycpu /out:"%~dp0Built\Sheltered2.Attribute30.dll" /reference:"%GAME%\BepInEx\core\BepInEx.dll" /reference:"%GAME%\BepInEx\core\0Harmony.dll" /reference:"%NETSTANDARD%" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp.dll" /reference:"%GAME%\Sheltered2_Data\Managed\Assembly-CSharp-firstpass.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.CoreModule.dll" /reference:"%GAME%\Sheltered2_Data\Managed\UnityEngine.UI.dll" "%~dp0Source\AttributePoints.cs"
if errorlevel 1 goto compilefail
if not exist "%~dp0Built\Sheltered2.Attribute30.dll" goto compilefail
set "OLD=%GAME%\BepInEx\plugins\AttributePoints\Sheltered2.Attribute999.dll"
if exist "%OLD%" (
  if exist "%OLD%.disabled" (
    echo ERROR: Old 999 attribute DLL and disabled backup both exist.
    echo Move or rename the old DLL manually, then run this again.
    goto fail
  )
  ren "%OLD%" "Sheltered2.Attribute999.dll.disabled"
  if errorlevel 1 goto oldfail
  echo Disabled old 999 Attribute Points DLL; backup has .dll.disabled suffix.
)

if not exist "%GAME%\BepInEx\plugins\AttributePoints" mkdir "%GAME%\BepInEx\plugins\AttributePoints"
copy /Y "%~dp0Built\Sheltered2.Attribute30.dll" "%GAME%\BepInEx\plugins\AttributePoints\Sheltered2.Attribute30.dll" >nul
if errorlevel 1 goto copyfail
echo.
echo SUCCESS: 30 Attribute Points installed.
echo Location: %GAME%\BepInEx\plugins\AttributePoints\Sheltered2.Attribute30.dll
echo Check %GAME%\BepInEx\LogOutput.log after launch.
pause
exit /b 0
:missing
echo ERROR: Sheltered2_Data\Managed\Assembly-CSharp.dll not found in:
echo %GAME%
goto fail
:missingbep
echo ERROR: Install BepInEx 5.x Unity Mono x64 first.
goto fail
:missingnetstandard
echo ERROR: netstandard.dll not found. Install .NET Framework 4.8 Developer Pack.
goto fail
:missingcsc
echo ERROR: Windows .NET Framework C# compiler csc.exe not found.
goto fail
:compilefail
echo ERROR: Compile failed. Copy compiler messages to ChatGPT.
goto fail
:oldfail
echo ERROR: Could not disable the old 999 Attributes DLL. Close game.
goto fail
:copyfail
echo ERROR: Could not install DLL. Close game and check permissions.
:fail
pause
exit /b 1
