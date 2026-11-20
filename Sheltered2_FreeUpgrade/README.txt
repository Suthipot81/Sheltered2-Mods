Sheltered 2 - Free Upgrade v0.1 (Steam / Windows / BepInEx 5)
==============================================================

Standalone mod ID: suthipot.sheltered2.freeupgrade

TARGETS (only these four):
- Object_OxygenFilter: Air Filter
- Object_WaterFilter: Rain Water Filter
- Object_WeatherVane: Weather Vane
- Object_WaterPurifier: Water Purifier

WHAT IT DOES:
- Removes material requirements from upgrade levels of the four objects above.
- Enables their Upgrade button without inventory materials.
- The normal upgrade job, upgrade effects, progress and time remain unchanged.
- Does not alter the cost of normal Crafting recipes or other upgradeable objects.
- Compatible by design with the existing Free Craft and Infinite Durability mods.

TECHNICAL NOTES:
- UpgradePanel.UpdateSelection checks UpgradeObject.LevelInfo.items (the original costs).
- UpgradeObject.BuildRecipeForUpgrade also uses the same LevelInfo.items.
- A targeted Harmony postfix on UpgradeObject.GetLevelInfo replaces that list with an empty one
  for the four requested Object_Base subclasses. Other upgrade lists are untouched.
- No source game DLL is edited. On disabling the mod and restarting, original requirements return.

INSTALL:
1. Back up your save. Close Sheltered 2.
2. Make sure BepInEx 5 is already installed and working.
3. Extract this ZIP to its own folder.
4. Run BuildAndInstall.bat and enter your Sheltered 2 GAME folder, for example:
   E:\SteamLibrary\steamapps\common\Sheltered 2
5. On SUCCESS, the DLL will be at:
   <game>\BepInEx\plugins\FreeUpgrade\Sheltered2.FreeUpgrade.dll
6. Start the game, open one of these 4 object's Upgrade screens.
7. Confirm materials are not required, upgrade works, and resource totals do not drop.

TROUBLESHOOTING:
- If BuildAndInstall.bat shows a compiler error, send the full error output.
- If the mod loads but costs remain, send BepInEx\LogOutput.log.
  Look for 'Free Upgrade v0.1 loaded' and 'Free Upgrade applied to ...'.
- If the Upgrade panel was already open, close and reopen it after loading the mod.

REMOVE:
- Close game, delete BepInEx\plugins\FreeUpgrade\Sheltered2.FreeUpgrade.dll.
- Reopen game. Upgrade costs should return to game defaults.

STATUS:
- Target types, field and methods checked against your uploaded Assembly-CSharp.dll.
- Not compiled on your Windows installation or runtime-tested inside the game.
