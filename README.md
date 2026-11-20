# Sheltered 2 Mods

A collection of gameplay enhancement mods for **Sheltered 2 (Steam / Windows)** developed using **C#, BepInEx 5, and Harmony**.

This repository provides the source code, build scripts, documentation, and compiled DLLs for my Sheltered 2 mods.

Each mod is independent and can be installed separately.

## Available Mods

| Mod | Description |
|---|---|
| [Attribute Points 30](https://www.nexusmods.com/sheltered2/mods/25) | Increases starting character attribute points to 30. |
| [Free Craft](https://www.nexusmods.com/sheltered2/mods/26) | Craft items without consuming resources, including workbenches and cooking stations. |
| [Free Upgrade](https://www.nexusmods.com/sheltered2/mods/27) | Upgrade selected shelter equipment without requiring materials. |
| [Friendly Conversations](https://www.nexusmods.com/sheltered2/mods/28) | Encourages positive conversations and improves relationships between shelter members. |
| [Infinite Durability](https://www.nexusmods.com/sheltered2/mods/29) | Prevents durability loss for furniture, equipment, weapons, and tools, excluding shelter entrance doors. |
| [Infinite Generator Fuel](https://www.nexusmods.com/sheltered2/mods/30) | Prevents generator fuel consumption and increases maximum power output to 200. |
| [Leader Trait Points 999](https://www.nexusmods.com/sheltered2/mods/31) | Provides 999 positive Leader Trait points during character creation. |
| [Speed Building X6](https://www.nexusmods.com/sheltered2/mods/32) | Increases shelter room and stairwell construction speed by 6 times. |
| [Strong Leader](https://www.nexusmods.com/sheltered2/mods/33) | Removes basic survival needs and fatigue for the Leader, adds Plague and Infection immunity, and maintains a minimum of 100 HP under supported damage conditions. |
| [Vehicles Craft](https://www.nexusmods.com/sheltered2/mods/34) | Unlocks vehicle part crafting and adds experimental vehicle chassis recipes. |

## Requirements

- Sheltered 2 (Steam / Windows)
- BepInEx 5 (Unity Mono x64)
- Windows .NET Framework C# Compiler (`csc.exe`)
- `netstandard.dll` reference assembly, available from the game or compatible .NET Framework developer tools

**BepInEx Download:**

https://github.com/BepInEx/BepInEx/releases

## Installation Instructions

1. Back up your Sheltered 2 save files.
2. Download BepInEx 5 (Unity Mono x64).
3. Extract BepInEx into the Sheltered 2 installation directory.
4. Launch the game once, then close it to initialize BepInEx.
5. Download and extract the desired mod.
6. Run `BuildAndInstall.bat` from the extracted mod folder.
7. Enter the full Sheltered 2 installation directory when prompted.
8. Wait for the compilation and installation process to finish.
9. Confirm that the command window displays a `SUCCESS` message.
10. Launch Sheltered 2 and verify that the mod works correctly.

**Example game directory:**

`E:\SteamLibrary\steamapps\common\Sheltered 2`

Successfully built plugins are installed under:

`Sheltered 2\BepInEx\plugins\`

## Source Code and Build Information

Each mod has its own folder containing the relevant files:

- `Source/` — C# source code
- `BuildAndInstall.bat` — Windows compilation and installation script
- `README.txt` — Mod-specific information and instructions
- `Built/` — Compiled plugin DLL
- `.jpg` images — Screenshots or mod previews

The batch installer uses the local .NET Framework C# compiler and references assemblies from the installed game and BepInEx.

The resulting plugin DLL is copied into the appropriate BepInEx plugins directory.

All source code and build scripts are publicly available in this repository for inspection and review.

## Troubleshooting

If a mod fails to compile or load:

1. Verify that BepInEx 5 is installed correctly.
2. Confirm that the entered game directory is correct.
3. Check the compiler error messages in the batch window.
4. Verify that the required .NET Framework reference assemblies are available.
5. Review `BepInEx/LogOutput.log` for plugin loading errors.

## Uninstallation

1. Close Sheltered 2.
2. Open the `BepInEx/plugins` directory.
3. Locate the folder containing the mod you want to uninstall.
4. Remove the corresponding mod folder.
5. Restart the game.

## Important Notes

- Always back up your save files before installing or updating mods.
- These mods are intended for the Steam version of Sheltered 2 on Windows.
- Some gameplay changes may behave differently depending on your game version and save progress.
- Experimental functionality may require additional testing.
- Review batch scripts before executing them.
- These are unofficial modifications and are not affiliated with the game's developers or publisher.

## Author

**Suthipot81**

GitHub: https://github.com/Suthipot81

Nexus Mods: https://www.nexusmods.com/sheltered2

Thank you for using my Sheltered 2 mods!
