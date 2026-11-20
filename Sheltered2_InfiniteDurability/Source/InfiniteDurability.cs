using System;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.InfiniteDurability
{
    [BepInPlugin("suthipot.sheltered2.infinitedurability", "Infinite Durability", "0.2.0")]
    public sealed class InfiniteDurabilityPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                new Harmony("suthipot.sheltered2.infinitedurability").PatchAll(typeof(InfiniteDurabilityPlugin).Assembly);
                Logger.LogInfo("Infinite Durability v0.2: protected objects/items; doors excluded; auto Workbench Tier 3 after Tier 2.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Infinite Durability patch failed: " + ex);
            }
        }
    }

    // Regular wear from use/time. Preserve every integrity update on shelter doors.
    [HarmonyPatch(typeof(Object_Integrity), "UpdateIntegrity")]
    internal static class ObjectWearPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Object_Integrity __instance)
        {
            return __instance is Object_ShelterDoor;
        }
    }

    // One-shot integrity damage. Door breaches and enemy attacks remain available.
    [HarmonyPatch(typeof(Object_Integrity), "Degrade")]
    internal static class ObjectDegradePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Object_Integrity __instance)
        {
            return __instance is Object_ShelterDoor;
        }
    }

    // Dedicated weapon and equipment use methods only reduce durability/hit count.
    [HarmonyPatch(typeof(ItemStack), "UseWeapon")]
    internal static class WeaponUsePatch
    {
        [HarmonyPrefix] private static bool Prefix() { return false; }
    }

    [HarmonyPatch(typeof(ItemStack), "UseEquipment")]
    internal static class EquipmentUsePatch
    {
        [HarmonyPrefix] private static bool Prefix() { return false; }
    }

    // Guard any other reductions on durable inventory items, but preserve food
    // spoilage, medical consumables, and other survival-resource mechanics.
    [HarmonyPatch(typeof(ItemStack), "SetIntegrity")]
    internal static class DurableItemsPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ItemStack __instance, float __0)
        {
            if (__instance == null || __instance.def == null) return true;
            ItemDef.ItemType type = __instance.def.type;
            bool durable = type == ItemDef.ItemType.Weapon ||
                           type == ItemDef.ItemType.Tool ||
                           type == ItemDef.ItemType.Equipment ||
                           type == ItemDef.ItemType.Bag ||
                           type == ItemDef.ItemType.VehiclePart ||
                           type == ItemDef.ItemType.Electrical;
            if (!durable) return true;
            return __0 >= __instance.integrity;
        }
    }
    // Workbench Tier 2 is unlocked by Engineering101.
    // SkilledEngineer normally requires repairing items and gates Workbench Tier 3.
    // Report SkilledEngineer as completed *only while Engineering101 is already completed*.
    // This affects both the level tab and the recipe check, without editing achievement
    // progress, touching the save, or unlocking unrelated crafting stations or Tier 4.
    // Turn this mod off and the vanilla requirement returns immediately.
    [HarmonyPatch(typeof(FactionAchievementManager.Achievement), "get_Completed")]
    internal static class WorkbenchTier3UnlockPatch
    {
        [HarmonyPostfix]
        private static void Postfix(FactionAchievementManager.Achievement __instance, ref bool __result)
        {
            if (__result || __instance == null) return;
            if (__instance.Type != FactionAchievementManager.AchievementType.SkilledEngineer) return;

            FactionAchievementManager manager = FactionAchievementManager.instance;
            if (manager == null || manager.achievements == null) return;

            FactionAchievementManager.Achievement tierTwo;
            if (manager.achievements.TryGetValue(
                    FactionAchievementManager.AchievementType.Engineering101,
                    out tierTwo) &&
                tierTwo != null && tierTwo.Completed)
            {
                __result = true;
            }
        }
    }

}
