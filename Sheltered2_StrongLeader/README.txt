Strong Leader v0.3 — Plague + Infection Immunity (Sheltered 2 / BepInEx 5)
===============================================================================
This is a Build Kit (C# source + BuildAndInstall.bat), NOT a precompiled DLL.
It is an UPDATE to the existing Strong Leader plugin, NOT an additional mod.
Plugin ID and DLL install path are unchanged from v0.2.

NEW in v0.3 (PLAYER LEADER ONLY)
- Blocks Plague illness activation and Plague damage ticks for the leader.
- Blocks the separate Infection illness for the leader.
- If either disease is already active on a LIVING leader when loading a save,
  it will be cured as the game processes the leader (not a resurrection).
- Other shelter members and enemy NPCs retain the game's normal disease rules.
- Other illness types (e.g. radiation, bleeding, food poisoning) retain their
  vanilla behavior; this is NOT universal protection from scripted death.

UNCHANGED from v0.2
- No food, water, toilet, or shower needs (leader only).
- No sleep/fatigue needs, compatible with Perpetual Energy trait.
- HP may be damaged but normal shelter/combat HP should not fall below 100.
- Stress, travel energy, combat stamina and non-leaders remain unchanged.

CONFIG (automatically created / extended on the first launch)
BepInEx/config/suthipot.sheltered2.strongleader.cfg
[Leader]
NoFoodWaterToiletShower = true
NoEnergyLoss = true
EnableHealthFloor = true
MinimumHealth = 100
ImmuneToPlague = true
ImmuneToInfection = true

INSTALL
1. Close Sheltered 2. Back up your saves and the previous Strong Leader DLL.
2. Extract this ZIP. Run BuildAndInstall.bat.
3. Use the game folder, for example:
   E:\SteamLibrary\steamapps\common\Sheltered 2
4. Ensure the script prints SUCCESS: Strong Leader v0.3 installed.
5. It overwrites the existing plugin at:
   BepInEx/plugins/StrongLeader/Sheltered2.StrongLeader.dll
6. Restart the game and load your saved game.

TEST PLAN
1. While the Leader is healthy, expose only the Leader to Plague risk and
   observe that no Plague condition appears or HP loss occurs from Plague.
2. Repeat with a save in which the Leader already has an active Plague.
   The active status should clear on the next game update.
3. Confirm an ordinary member can still contract Plague and Infection.
4. Confirm basic needs, sleep and minimum 100 HP from v0.2 still work.
5. If BepInEx reports patch failures, send BepInEx/LogOutput.log.

CAUTIONS
- Do not test using your only save; make a backup or an expendable test save.
- A member who is already dead will NOT be revived by this plugin.
- The mod is intended to intercept vanilla illness logic. Other scripted death
  paths, including mods and quests that bypass Illness_Base, are not guaranteed.
- The source and target names have been statically cross-checked with the
  supplied Assembly-CSharp.dll, but no Windows compilation or gameplay test
  was possible in this environment.

ROLLBACK
Close game, replace v0.3 DLL with your backed-up v0.2 DLL and restart.
