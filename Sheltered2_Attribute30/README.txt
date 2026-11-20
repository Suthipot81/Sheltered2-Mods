Sheltered 2 - 30 Attribute Points v0.2.0 (BepInEx 5 Build Kit)

Purpose:
 - At new-game character creation, change starting REMAINING stat points from 999 (prior mod) to 30.
 - Preserve vanilla caps for each individual stat.
 - Allow the character editor to close with unspent points (as in the working v0.1 mod).
 - Does NOT modify Leader Traits.

Requirements:
 - Sheltered 2, Steam for Windows
 - BepInEx 5, Unity Mono 64-bit installed in game folder
 - netstandard.dll in the game's Managed folder, BepInEx core or .NET Framework 4.7+ Reference Assemblies
 - Windows C# compiler: csc.exe (.NET Framework)

Install:
 1. Back up saves and close Sheltered 2.
 2. Double-click BuildAndInstall.bat.
 3. Enter the GAME FOLDER (for example E:\SteamLibrary\steamapps\common\Sheltered 2).
 4. On success, the DLL is installed to BepInEx\plugins\AttributePoints\Sheltered2.Attribute30.dll.
 5. The installer renames the previous Attribute999 DLL in that folder to .dll.disabled, without deleting it.
 6. IMPORTANT: if you installed the old Attribute999 DLL anywhere else under plugins, disable that copy yourself to prevent both versions loading.
 7. Start a NEW GAME and check the Character Attributes page.

Known limitations:
 - The UI budget is 30 points, but the game's per-stat maxima remain unchanged.
 - Build and in-game execution have NOT been verified by this authoring environment.
 - See BepInEx\LogOutput.log for load / patch errors.
