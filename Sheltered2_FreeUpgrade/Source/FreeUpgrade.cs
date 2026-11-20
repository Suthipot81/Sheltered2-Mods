using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.FreeUpgrade
{
    [BepInPlugin("suthipot.sheltered2.freeupgrade", "Free Upgrade", "0.1.0")]
    public sealed class FreeUpgradePlugin : BaseUnityPlugin
    {
        internal static Action<string> WriteInfo;

        private void Awake()
        {
            WriteInfo = message => Logger.LogInfo(message);
            try
            {
                new Harmony("suthipot.sheltered2.freeupgrade").PatchAll(typeof(FreeUpgradePlugin).Assembly);
                Logger.LogInfo("Free Upgrade v0.1 loaded. Target upgrades: OxygenFilter, WaterFilter, WaterPurifier, WeatherVane.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Free Upgrade patch failed: " + ex);
            }
        }
    }

    // The upgrade UI checks UpgradeObject.LevelInfo.items directly, not only the
    // CraftRecipeInstance.ingredients getter used by the existing Free Craft mod.
    // This patch changes costs ONLY on the four requested upgradeable objects.
    // Both UpgradePanel.UpdateSelection and UpgradeObject.BuildRecipeForUpgrade
    // obtain the same LevelInfo, so the UI and actual upgrade will see zero costs.
    [HarmonyPatch(typeof(UpgradeObject), "GetLevelInfo")]
    internal static class TargetedUpgradeCostPatch
    {
        private static readonly FieldInfo OwnerField =
            AccessTools.Field(typeof(UpgradeObject), "obj");
        private static readonly HashSet<string> LoggedTypes = new HashSet<string>();

        [HarmonyPostfix]
        private static void Postfix(UpgradeObject __instance, ref UpgradeObject.LevelInfo __result)
        {
            if (__instance == null || __result == null)
                return;

            Object_Base owner = null;
            if (OwnerField != null)
                owner = OwnerField.GetValue(__instance) as Object_Base;

            // Fallback if the private owner field has not been initialized yet.
            if (owner == null)
                owner = __instance.GetComponentInParent<Object_Base>();

            if (owner == null || !IsTarget(owner))
                return;

            // Preserve the existing recipe and all upgrade side effects. Only
            // replace its ingredient requirements; do NOT bypass completion.
            if (__result.items == null || __result.items.Count == 0)
                return;

            __result.items = new List<ItemStack>();

            string typeName = owner.GetType().Name;
            if (LoggedTypes.Add(typeName) && FreeUpgradePlugin.WriteInfo != null)
                FreeUpgradePlugin.WriteInfo("Free Upgrade applied to " + typeName + " (no upgrade materials required).");
        }

        private static bool IsTarget(Object_Base owner)
        {
            return owner is Object_OxygenFilter
                || owner is Object_WaterFilter
                || owner is Object_WaterPurifier
                || owner is Object_WeatherVane;
        }
    }
}
