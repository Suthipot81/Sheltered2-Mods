using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace Suthipot.Sheltered2.SpeedBuilding
{
    [BepInPlugin("suthipot.sheltered2.speedbuilding", "Speed Building", "0.3.0")]
    public sealed class SpeedBuildingPlugin : BaseUnityPlugin
    {
        internal static float Multiplier = 6f;
        private static SpeedBuildingPlugin _instance;
        private static bool _loggedConstruction;
        private static bool _loggedExcavation;
        private static bool _loggedBadMultiplier;

        // The ORIGINAL GetSpeedMulti returns only the speed bonus or penalty.
        // The game updates work progress as (1.0 + cachedBonus) * Time.deltaTime.
        // Therefore the correct boosted bonus is: speedMultiplier * (1 + bonus) - 1.
        // Simply multiplying the bonus (v0.1/v0.2) could produce negative progress.
        internal static void BoostSpeed(ref float bonus, bool excavating)
        {
            float originalBonus = bonus;
            if (float.IsNaN(originalBonus) || float.IsInfinity(originalBonus))
                originalBonus = 0f;

            float normalSpeed = 1f + originalBonus;
            // Keep progress FORWARD even if a combination of negative traits,
            // darkness and other modifiers makes the base speed zero/negative.
            if (normalSpeed < 0.10f)
            {
                normalSpeed = 0.10f;
                if (!_loggedBadMultiplier && _instance != null)
                {
                    _loggedBadMultiplier = true;
                    _instance.Logger.LogWarning("Very low/negative vanilla construction speed; clamped to a positive minimum.");
                }
            }
            float boostedSpeed = normalSpeed * Multiplier;
            // UpdateInteraction reads (1 + this stored value); never allow negative speed.
            bonus = boostedSpeed - 1f;

            bool alreadyLogged = excavating ? _loggedExcavation : _loggedConstruction;
            if (!alreadyLogged)
            {
                if (excavating) _loggedExcavation = true;
                else _loggedConstruction = true;
                if (_instance != null)
                    _instance.Logger.LogInfo((excavating ? "Excavation" : "Construction") +
                        " speed patch applied: vanilla rate=" + (1f + originalBonus) +
                        ", boosted rate=" + boostedSpeed + " (x" + Multiplier + ").");
            }
        }

        private void Awake()
        {
            try
            {
                _instance = this;
                // Preserve the old config key, but migrate the original 3x preset
                // to 6x ONCE. Later user edits are respected.
                ConfigEntry<float> speed = Config.Bind("Building", "SpeedMultiplier", 6f,
                    "Construction speed for Stairwell and Dirt/Plaster/Stone/Tiled rooms. Default 6x.");
                ConfigEntry<bool> migrated = Config.Bind("Building", "MigratedTo6x", false,
                    "One-time upgrade marker for the original 3x config. Do not edit.");
                if (!migrated.Value)
                {
                    if (Math.Abs(speed.Value - 3f) < 0.0001f)
                    {
                        speed.Value = 6f;
                        Logger.LogInfo("Migrated original SpeedMultiplier preset from 3x to 6x.");
                    }
                    migrated.Value = true;
                }
                Multiplier = speed.Value;
                if (float.IsNaN(Multiplier) || float.IsInfinity(Multiplier) || Multiplier < 0.01f || Multiplier > 100f)
                {
                    Multiplier = 6f;
                    speed.Value = 6f;
                    Logger.LogWarning("Invalid SpeedMultiplier; using 6x.");
                }
                new Harmony("suthipot.sheltered2.speedbuilding").PatchAll(typeof(SpeedBuildingPlugin).Assembly);
                Logger.LogInfo("Speed Building v0.3 active; corrected total speed multiplier=" + Multiplier);
            }
            catch (Exception ex) { Logger.LogError("Speed Building startup failed: " + ex); }
        }

        internal static bool IsTargetRoom(ObjectManager.ObjectType type)
        {
            return type == ObjectManager.ObjectType.Stairwell ||
                   type == ObjectManager.ObjectType.DirtRoom ||
                   type == ObjectManager.ObjectType.PlasterRoom ||
                   type == ObjectManager.ObjectType.StoneRoom ||
                   type == ObjectManager.ObjectType.TileRoom;
        }
    }

    // This method handles building already-placed shelter objects.  Restrict the
    // multiplier to the five room/stairwell types requested by the user.
    [HarmonyPatch(typeof(ObjectInteraction_ConstructObject), "GetSpeedMulti")]
    internal static class ConstructRoomSpeedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ObjectInteraction_ConstructObject __instance, ref float __result)
        {
            if (__instance == null || __instance.obj == null) return;
            if (SpeedBuildingPlugin.IsTargetRoom(__instance.obj.objectType))
                SpeedBuildingPlugin.BoostSpeed(ref __result, false);
        }
    }

    // Excavation and room upgrades run through an Object_Tarp; this is a separate
    // work loop from ConstructObject.  Exclude garages and holding cells.
    [HarmonyPatch(typeof(ObjectInteraction_Excavate), "GetSpeedMulti")]
    internal static class ExcavateRoomSpeedPatch
    {
        private static readonly FieldInfo TarpField =
            AccessTools.Field(typeof(ObjectInteraction_Excavate), "m_tarpObj");

        [HarmonyPostfix]
        private static void Postfix(ObjectInteraction_Excavate __instance, ref float __result)
        {
            if (__instance == null || TarpField == null) return;
            Object_Tarp tarp = TarpField.GetValue(__instance) as Object_Tarp;
            if (tarp == null || tarp.TargetCell == null) return;
            ShelterGridCell cell = tarp.TargetCell;
            if (cell.cellType == ShelterGridCell.CellType.Garage ||
                cell.cellType == ShelterGridCell.CellType.HoldingCell)
                return;
            bool roomType = cell.roomType == ShelterGridCell.RoomType.Dirt ||
                            cell.roomType == ShelterGridCell.RoomType.Wood ||
                            cell.roomType == ShelterGridCell.RoomType.Stone ||
                            cell.roomType == ShelterGridCell.RoomType.Tile;
            if (roomType || cell.cellType == ShelterGridCell.CellType.Stairwell)
                SpeedBuildingPlugin.BoostSpeed(ref __result, true);
        }
    }
}
