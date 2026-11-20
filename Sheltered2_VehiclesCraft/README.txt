Sheltered 2 - Vehicles Craft v0.1.1 (experimental)
BepInEx 5 / Steam Windows x64. Build kit (NOT precompiled DLL).

Functionality:
- Unlock vanilla Los Muertos Mechanic-related Workbench recipes early.
- Finds vehicle part recipes and allows crafting without being allied
  with Los Muertos: Tyre, Alternator, Car Battery, Starter Motor,
  Clutch Cable, Spark Plug, Fan Belt, Distributor.
- When the game has the matching item definition but no recipe,
  injects a simple Metal-cost Workbench recipe.
- OPTIONAL / EXPERIMENTAL: Adds recipes for Bicycle Frame,
  Motorcycle Chassis and Pickup Truck Chassis (if corresponding
  game item definitions exist). These normally come from scavenging.
- Does not edit Los Muertos reputation or saved alliance status.
- Cooperates with your existing Free Craft by keeping CraftRecipeInstance
  ingredient requirements in the regular game pipeline.

Config after first launch:
  BepInEx\config\suthipot.sheltered2.vehiclescraft.cfg
  [Vehicle Parts]
  AddMissingPartRecipes = true
  [Vehicle Chassis]
  EnableChassisRecipes = true
  [Workbench]
  ShowAtTier1 = true

Custom fallback recipes consume Metal by default. If vanilla vehicle part
recipes exist, their original ingredient costs are preserved. The chassis
recipes use 10, 18 and 30 Metal respectively (Bicycle, Motorcycle, Pickup).
Your Free Craft mod may suppress that material consumption.

Installation:
1. Close game and back up save.
2. Confirm BepInEx 5 (Unity Mono x64) installed.
3. Double-click BuildAndInstall.bat and enter your Sheltered 2 game folder.
4. After successful build, start game and open Workbench.
5. For chassis: craft ONE first, check Inventory, then try Garage.

Uninstall:
  Delete BepInEx\plugins\VehiclesCraft\Sheltered2.VehiclesCraft.dll.
  Avoid unloading this mod while active custom recipes are in a job queue.

IMPORTANT: The 8 standard recipes and the 3 chassis recipes have different
levels of certainty. The game DLL contains the recipe/vehicle infrastructure,
but actual recipe assets and ItemDef dictionary keys are game resources not
provided here, so recipe availability/quality must be verified in-game.
This is a test build, not a verified guarantee of all 11 outputs.


v0.1.1 COMPILER HOTFIX:
- Fixed ItemStack ingredient construction for the Sheltered 2 API:
  the constructor expects (string itemKey, ItemStack.Quality, int count, ItemStack.SpawnType).
- Renamed internal Info helper to LogInfo to remove CS0108 warning.
- This is a compile fix, not a confirmed in-game crafting behavior fix.
- Back up saves before testing; test one vehicle-part craft first.
