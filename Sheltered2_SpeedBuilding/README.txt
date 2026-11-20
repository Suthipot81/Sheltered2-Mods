Sheltered 2 - Speed Building v0.3 (CORRECT 6x Speed)
====================================================

WHAT WAS WRONG IN v0.1 / v0.2
The game's GetSpeedMulti() returns a BONUS / PENALTY, not total speed.
Game progress per frame uses: (1.0f + bonus) * Time.deltaTime.
The older mods multiplied the BONUS by x3 or x6 and could make the
progress run BACKWARDS, particularly with leaders or negative traits.

v0.3 changes the returned bonus to:
  NEW_BONUS = 6 * max(0.10, 1 + ORIGINAL_BONUS) - 1
The result is always positive progress and targets true 6x total speed
under normal conditions (except when a negative base would otherwise stall).

The patch targets Stairwell and room construction / excavation for:
Dirt, Wood (Plaster), Stone, Tile. Garage and Holding Cell are excluded.
All other installed mods, including Strong Leader, are left untouched.

INSTALL / UPDATE
1. Close Sheltered 2, back up saves and existing SpeedBuilding DLL.
2. Extract ZIP to a NEW folder (do not put this ZIP in BepInEx/plugins).
3. Run BuildAndInstall.bat and enter the game folder:
   E:\SteamLibrary\steamapps\common\Sheltered 2
4. On SUCCESS it replaces:
   BepInEx\plugins\SpeedBuilding\Sheltered2.SpeedBuilding.dll
5. Ensure there is no SECOND copy of SpeedBuilding.dll elsewhere in plugins.
6. Restart the game. Use NEW Dirt Room jobs with your Leader and normal member.

Config retained from v0.2:
BepInEx\config\suthipot.sheltered2.speedbuilding.cfg
[Building]
SpeedMultiplier = 6

IMPORTANT: If you manually changed SpeedMultiplier in the old config,
that value will be retained. Use 6 for the intended test.

Expected tests (record build time with same member, same light and same task):
- Vanilla Dirt Room 65 seconds -> mod x6 around 11 seconds of active building.
- Vanilla Dirt Room 120 seconds -> mod x6 around 20 seconds.
Walking, idle, setup, pauses and game-speed differences are excluded.

BepInEx\LogOutput.log should contain:
"Speed Building v0.3 active; corrected total speed multiplier=6"
"Excavation speed patch applied: ..."
If no excavation patch applied line appears while digging, check config and logs.

ROLLBACK: remove the SpeedBuilding DLL or restore the v0.2 DLL backup.
This is a SOURCE BUILD KIT. Windows must compile it through BuildAndInstall.bat.
It has been statically checked against the provided game assembly, but NOT
compiled in Windows or run in Sheltered 2 yet. Treat as beta/test build.
