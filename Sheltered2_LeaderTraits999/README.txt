Sheltered 2 - 999 Leader Trait Points v0.1.0 (BepInEx 5 Build Kit)

Purpose:
 - On NEW GAME creation, when selecting Leader Traits, begin with +999 points instead of +5.
 - Applies only to initial Leader Trait selection in CustomisationPanel_LeaderTraits.Populate.
 - Positive traits should consume points as they do in vanilla. Negative traits are not required.
 - Does NOT change stat Attribute Points, leader level-up rewards or trait categories.
 - This mod does not create 999 distinct traits; only raises the budget.

Requirements:
 - Sheltered 2, Steam for Windows
 - BepInEx 5 Unity Mono 64-bit
 - Windows C# compiler with netstandard.dll (as above)

Install:
 1. Close game and back up save.
 2. Double-click BuildAndInstall.bat.
 3. Enter your game folder.
 4. The DLL is installed to BepInEx\plugins\LeaderTraits999\Sheltered2.LeaderTraits999.dll.
 5. Start a NEW GAME and open the LEADER TRAITS selection screen.
 6. Check the INITIAL points indicator shows +999, and selecting a positive trait decreases it by the vanilla cost.

Notes:
 - Can be enabled alongside 30 Attribute Points (different class and BepInEx GUID).
 - Build and in-game execution have NOT been verified by this authoring environment.
 - See BepInEx\LogOutput.log if the points value does not change.
