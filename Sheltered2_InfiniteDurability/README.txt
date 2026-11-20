Sheltered 2 - Infinite Durability v0.2 + Workbench Tier 3 Bypass
================================================================

A PATCH FOR THE EXISTING Infinite Durability mod (same plugin ID).
THIS IS A SOURCE + WINDOWS BUILD KIT, NOT A PRECOMPILED DLL.

UNCHANGED BEHAVIOUR FROM THE TESTED v0.1.1
- Furniture and shelter objects normally do not lose integrity.
- Shelter entrance door still takes damage and can be broken by enemies.
- Weapons, equipment, tools, bags, electrical items and vehicle parts
  retain integrity during ordinary use.
- Food spoilage is not modified; no automatic repair to 100%.

NEW IN v0.2: WORKBENCH TIER 3 BYPASS
- Upon completion of the Engineering101 faction achievement (the Workbench
  Tier 2 requirement), the mod treats the SkilledEngineer achievement as
  complete for game checks, without needing to repair 25 items.
- This should enable the Workbench Tier 3 tab AND recipes that check the
  SkilledEngineer achievement, on the next opening of the Workbench panel.
- Other achievements and the Workbench Tier 4 requirement are untouched.
- Does NOT add or fake saved repair progress. Disabling this mod restores
  the original SkilledEngineer unlock requirement.
- Does NOT unlock Tier 3 before Engineering101 is completed.

PREPARATION
1. Back up your save files. Exit Sheltered 2.
2. The old Infinite Durability mod should already be in
   <game>\BepInEx\plugins\InfiniteDurability\
3. BepInEx 5 (Unity Mono x64) is required.

BUILD AND INSTALL
1. Extract ZIP (e.g. Desktop).
2. Run BuildAndInstall.bat.
3. Enter the Sheltered 2 GAME folder when prompted, e.g.
   E:\SteamLibrary\steamapps\common\Sheltered 2
4. The batch compiles the C# source using the installed game's DLLs,
   with the netstandard reference fix already included from v0.1.1.
5. The batch overwrites this DLL in its EXISTING plugin directory:
   <game>\BepInEx\plugins\InfiniteDurability\Sheltered2.InfiniteDurability.dll
6. Open your existing save and then reopen Workbench to inspect Tier 3.

ROLL BACK
- If needed, rebuild the previous v0.1.1 Build Kit from your backup
  to replace the new DLL. Do NOT leave two versions in plugins.
- To completely disable, close the game and move/delete only the Infinite
  Durability DLL from BepInEx\plugins\InfiniteDurability\.

LIMITATIONS
- Tier 3 bypass requires Engineering101 to be marked Completed in game.
- Vanilla Faction Goals UI may still show the original SkilledEngineer
  repair count, since progress is intentionally NOT edited.
- The mod was metadata-checked against the uploaded Assembly-CSharp.dll;
  it has not been compiled on Windows or gameplay-tested here.
- If compile or gameplay fails, share the console output or BepInEx\LogOutput.log.
