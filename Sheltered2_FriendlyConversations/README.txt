Sheltered 2 - Friendly Conversations v0.1.0 (BepInEx 5 BUILD KIT)

PURPOSE
 - Members chatting normally inside the shelter have a POSITIVE outcome.
 - Their displayed topic reaction is LIKE, instead of Dislike/Hate.
 - Relationship changes from Job_Conversation should not turn negative.
 - Already-queued conversations are also forced positive when completed.
 - Does not alter NPC encounter dialogue, player choices, faction relations,
   or separate Mental Breakdown/Altercation events.
 - Does not rewrite localized dialogue text (if any): it changes the
   underlying conversation outcome, reaction icons, and relationship logic.

CONFIG
 After the first successful launch, edit:
   BepInEx\config\suthipot.sheltered2.friendlyconversations.cfg
 [Conversations]
 FriendlyOutcomes = true
 FriendlyReactions = true
 SuperPositive = false

 Set SuperPositive = true if you want larger positive relationship gains.
 Restart game after changing settings.

INSTALLATION
 1. Close the game and back up the save.
 2. Extract the ZIP to any folder on Windows.
 3. Run BuildAndInstall.bat (Windows .NET Framework compiler required).
 4. Enter the Sheltered 2 GAME FOLDER when prompted, e.g.:
      E:\SteamLibrary\steamapps\common\Sheltered 2
 5. The script checks BepInEx 5, game DLLs, and netstandard.dll and installs:
      BepInEx\plugins\FriendlyConversations\Sheltered2.FriendlyConversations.dll
 6. Launch game and allow two shelter members to talk.
 7. Observe response icons and relationship values before/after conversation.
 8. Check BepInEx\LogOutput.log for "Friendly Conversations v0.1 patched".

UNINSTALL
 Close the game and delete only:
   BepInEx\plugins\FriendlyConversations\Sheltered2.FriendlyConversations.dll

LIMITATIONS
 - This package is C# SOURCE + A WINDOWS BUILD/INSTALL SCRIPT.
 - It is not a precompiled DLL and has not been tested live inside Sheltered 2.
 - Harmony signatures and enum values were inspected against your uploaded
   Assembly-CSharp.dll. Validate the in-game behavior on a backed-up save.
 - If any conversation shows a negative reaction or drops relationship,
   send BepInEx\LogOutput.log and screenshots of the before/after values.
