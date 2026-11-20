Sheltered 2 -- Free Craft v0.1 (experimental)
====================================================
THIS IS A SOURCE + LOCAL BUILD PACK, NOT A PRECOMPILED DLL.
The mod becomes a DLL after you run BuildAndInstall.bat on Windows.

PREREQUISITES
- Sheltered 2 for Windows/Steam (Unity Mono; game files required).
- BepInEx 5 for Unity Mono x64 installed in game folder.
- Windows .NET Framework 4.x C# compiler (csc.exe) normally at
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
- BACK UP YOUR SAVE DATA first. All mods are experimental.

INSTALL
1. Close Sheltered 2.
2. Extract this ZIP anywhere.
3. Double-click BuildAndInstall.bat.
4. When asked for the game folder, paste the path opened by Steam >
   Sheltered 2 > Manage > Browse local files.
5. Script compiles Source\FreeCraft.cs and copies Sheltered2.FreeCraft.dll to
   <game>\BepInEx\plugins\FreeCraft\
6. Start game. Check BepInEx\LogOutput.log for 'Free Craft v0.1'.

UNINSTALL
Close game and delete
<game>\BepInEx\plugins\FreeCraft\Sheltered2.FreeCraft.dll

Notes:
- Independent plugin ID: suthipot.sheltered2.freecraft
- NOT tested inside the game. Check the game's BepInEx log.
- Do not use the Sheltered 1 SMM loader for Sheltered 2.
- Intended for workbench, food/cooking and other recipes routed
  through CraftRecipeInstance. Some unusual recipes may need more patches.
- Empty recipe ingredients are returned at runtime, not saved to recipe data.
- Food consumed by characters and water consumed during normal play remain
  untouched. Repair recipes are also made free.


Build Kit v0.1.1 hotfix (2026-11-16):
- Added netstandard.dll 2.0 facade reference to fix CS0012 while compiling on Windows.
- Mod C# source and plugin behavior are unchanged from v0.1.
- If the script reports netstandard.dll missing, check the Managed/Facades folder
  or install Microsoft .NET Framework 4.8 Developer Pack for its reference facades.
- Actual Windows compilation and in-game testing are not yet verified.
