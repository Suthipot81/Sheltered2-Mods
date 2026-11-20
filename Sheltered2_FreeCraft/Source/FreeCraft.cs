using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.FreeCraft
{
    [BepInPlugin("suthipot.sheltered2.freecraft", "Free Craft", "0.1.0")]
    public sealed class FreeCraftPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                new Harmony("suthipot.sheltered2.freecraft").PatchAll(typeof(FreeCraftPlugin).Assembly);
                Logger.LogInfo("Free Craft v0.1: patched recipe ingredient access and checks.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Free Craft patch failed: " + ex);
            }
        }
    }

    // Recipe instances are used by regular workbenches, crafting stations,
    // and the crafting finalization path (including stored food and water).
    // Returning an EMPTY list, without changing the serialized field, means
    // creation and finalization see zero ingredient requirements/consumption.
    [HarmonyPatch(typeof(CraftRecipeInstance), "get_ingredients")]
    internal static class RecipeIngredientsPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref List<ItemStack> __result)
        {
            __result = new List<ItemStack>();
            return false;
        }
    }

    [HarmonyPatch(typeof(RepairRecipeInstance), "get_ingredients")]
    internal static class RepairIngredientsPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref List<ItemStack> __result)
        {
            __result = new List<ItemStack>();
            return false;
        }
    }

    [HarmonyPatch(typeof(CraftingManager), "CanCraftRecipe")]
    internal static class CanCraftRecipePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
