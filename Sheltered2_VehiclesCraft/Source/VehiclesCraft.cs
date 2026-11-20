using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Suthipot.Sheltered2.VehiclesCraft
{
    [BepInPlugin("suthipot.sheltered2.vehiclescraft", "Vehicles Craft", "0.1.1")]
    public sealed class VehiclesCraftPlugin : BaseUnityPlugin
    {
        internal static bool MakeMissingVehicleRecipes = true;
        internal static bool AddChassisRecipes = true;
        internal static bool UnlockWorkbenchTierOne = true;
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            try
            {
                MakeMissingVehicleRecipes = Config.Bind("Vehicle Parts", "AddMissingPartRecipes", true,
                    "Use a simple metal-based recipe only if an existing game recipe is unavailable.").Value;
                AddChassisRecipes = Config.Bind("Vehicle Chassis", "EnableChassisRecipes", true,
                    "Experimental: make Bicycle Frame, Motorcycle Chassis and Pickup Truck Chassis craftable.").Value;
                UnlockWorkbenchTierOne = Config.Bind("Workbench", "ShowAtTier1", true,
                    "Set selected vehicle recipes to Workbench Tier 1; leave false for vanilla tier requirements.").Value;
                new Harmony("suthipot.sheltered2.vehiclescraft").PatchAll(typeof(VehiclesCraftPlugin).Assembly);
                Logger.LogInfo("Vehicles Craft active. Part/chassis recipes will be checked when crafting is opened.");
            }
            catch (Exception ex) { Logger.LogError("Vehicles Craft startup failed: " + ex); }
        }

        internal static void LogInfo(string s) { if (Log != null) Log.LogInfo(s); }
        internal static void Warn(string s) { if (Log != null) Log.LogWarning(s); }
    }

    internal static class VehiclesCraftRecipes
    {
        private sealed class Target
        {
            internal readonly string Title;
            internal readonly string[] Aliases;
            internal readonly int MetalCost;
            internal readonly bool Chassis;
            internal Target(string title, int metalCost, bool chassis, params string[] aliases)
            {
                Title = title; MetalCost = metalCost; Chassis = chassis; Aliases = aliases;
            }
        }

        private static readonly HashSet<string> MissingItemWarnings = new HashSet<string>();

        private static readonly Target[] Targets = new Target[]
        {
            new Target("Tyre", 2, false, "tyre", "tire"),
            new Target("Alternator", 2, false, "alternator"),
            new Target("Car Battery", 3, false, "carbattery"),
            new Target("Starter Motor", 3, false, "startermotor"),
            new Target("Clutch Cable", 2, false, "clutchcable"),
            new Target("Spark Plug", 1, false, "sparkplug"),
            new Target("Fan Belt", 2, false, "fanbelt"),
            new Target("Distributor", 3, false, "distributor"),
            new Target("Bicycle Frame", 10, true, "bicycleframe", "bicyclechassis", "bikeframe"),
            new Target("Motorcycle Chassis", 18, true, "motorcyclechassis", "motorbikechassis"),
            new Target("Pickup Truck Chassis", 30, true, "pickuptruckchassis", "pickupchassis", "truckchassis")
        };

        private static readonly FieldInfo RecipesField = AccessTools.Field(typeof(CraftingManager), "m_recipes");
        private static readonly FieldInfo UnlockedField = AccessTools.Field(typeof(CraftRecipeInstance), "m_unlocked");
        private static readonly FieldInfo LevelField = AccessTools.Field(typeof(CraftingRecipe), "m_recipeLevel");
        private static readonly FieldInfo PoolsField = AccessTools.Field(typeof(CraftingRecipe), "m_randomIngredientsPool");

        private static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            StringBuilder b = new StringBuilder(input.Length);
            foreach (char ch in input)
                if (char.IsLetterOrDigit(ch)) b.Append(char.ToLowerInvariant(ch));
            return b.ToString();
        }

        private static bool NameMatches(string text, Target target)
        {
            string n = Normalize(text);
            if (n.Length == 0) return false;
            foreach (string alias in target.Aliases)
                if (n == alias || n.EndsWith(alias, StringComparison.Ordinal)) return true;
            return false;
        }

        private static bool Matches(ItemDef item, Target target)
        {
            if (item == null) return false;
            if (NameMatches(item.dictionaryKey, target)) return true;
            try { return NameMatches(item.GetLocalisedName(), target); }
            catch { return false; }
        }

        private static ItemDef FindItem(Target target)
        {
            Dictionary<string, ItemDef> items = ItemManager.items;
            if (items == null) return null;
            foreach (KeyValuePair<string, ItemDef> entry in items)
            {
                if (NameMatches(entry.Key, target) || Matches(entry.Value, target))
                    return entry.Value;
            }
            return null;
        }

        private static ItemDef FindMetal()
        {
            Dictionary<string, ItemDef> items = ItemManager.items;
            if (items == null) return null;
            foreach (KeyValuePair<string, ItemDef> entry in items)
                if (Normalize(entry.Key) == "metal" || Normalize(entry.Value == null ? "" : entry.Value.dictionaryKey) == "metal")
                    return entry.Value;
            return null;
        }

        private static void SetField(object obj, string field, object value)
        {
            FieldInfo fi = AccessTools.Field(obj.GetType(), field);
            if (fi == null) throw new MissingFieldException(obj.GetType().Name, field);
            fi.SetValue(obj, value);
        }

        private static CraftRecipeInstance CreateRecipe(ItemDef output, Target target, ItemDef metal)
        {
            CraftingRecipe def = ScriptableObject.CreateInstance<CraftingRecipe>();
            def.name = "Suthipot.VehiclesCraft." + Normalize(target.Title);
            def.ID = "suthipot.vehiclescraft." + Normalize(target.Title);
            SetField(def, "m_type", CraftingRecipe.OutputType.Item);
            SetField(def, "m_recipeType", CraftingRecipe.RecipeType.Vehicle);
            SetField(def, "m_unlockedByDefault", true);
            SetField(def, "m_factionSkillCheck", FactionAchievementManager.AchievementType.Null);
            SetField(def, "m_baseCraftTime", 20f);
            SetField(def, "m_recipeLevel", 1);
            SetField(def, "m_outputItem", output);
            SetField(def, "m_outputCount", 1);
            SetField(def, "m_ingredients", new List<ItemStack> { new ItemStack(metal.dictionaryKey, default(ItemStack.Quality), target.MetalCost, default(ItemStack.SpawnType)) });
            if (PoolsField != null && PoolsField.FieldType != null)
                PoolsField.SetValue(def, Activator.CreateInstance(PoolsField.FieldType));
            return new CraftRecipeInstance(def);
        }

        private static bool IsManagedVehicleRecipe(CraftingRecipe def)
        {
            if (def == null || def.type != CraftingRecipe.OutputType.Item || def.outputItem == null)
                return false;
            foreach (Target target in Targets)
            {
                if (target.Chassis && !VehiclesCraftPlugin.AddChassisRecipes) continue;
                if (Matches(def.outputItem, target)) return true;
            }
            return false;
        }

        internal static bool ShouldUnlock(CraftRecipeInstance recipe)
        {
            try { return recipe != null && IsManagedVehicleRecipe(recipe.def); }
            catch { return false; }
        }

        internal static void Ensure(CraftingManager manager)
        {
            if (manager == null || RecipesField == null) return;
            List<CraftRecipeInstance> recipes = RecipesField.GetValue(manager) as List<CraftRecipeInstance>;
            if (recipes == null || ItemManager.items == null || ItemManager.items.Count == 0) return;

            ItemDef metal = FindMetal();
            foreach (Target target in Targets)
            {
                if (target.Chassis && !VehiclesCraftPlugin.AddChassisRecipes) continue;
                ItemDef output = FindItem(target);
                if (output == null)
                {
                    if (MissingItemWarnings.Add(target.Title))
                        VehiclesCraftPlugin.Warn("Vehicles Craft: cannot find game item definition for " + target.Title +
                            "; recipe cannot be injected until a matching game ItemDef is available.");
                    continue;
                }

                bool existing = false;
                foreach (CraftRecipeInstance entry in recipes)
                {
                    if (entry == null || entry.def == null || entry.def.outputItem == null) continue;
                    if (entry.def.outputItem == output || Matches(entry.def.outputItem, target))
                    {
                        existing = true;
                        if (UnlockedField != null) UnlockedField.SetValue(entry, true);
                        if (VehiclesCraftPlugin.UnlockWorkbenchTierOne && LevelField != null)
                            LevelField.SetValue(entry.def, 1);
                    }
                }
                if (existing) continue;
                if (!target.Chassis && !VehiclesCraftPlugin.MakeMissingVehicleRecipes) continue;
                if (metal == null) continue;

                CraftRecipeInstance created = CreateRecipe(output, target, metal);
                recipes.Add(created);
                VehiclesCraftPlugin.LogInfo("Vehicles Craft: added recipe for " + target.Title +
                    " (" + target.MetalCost + " Metal).");
            }
        }
    }

    // Prevent the vanilla Los Muertos Mechanic check from hiding vanilla
    // vehicle recipes on the Workbench. Does not mutate faction reputation/save.
    [HarmonyPatch(typeof(FactionAchievementManager.Achievement), "get_Completed")]
    internal static class MechanicUnlockedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(FactionAchievementManager.Achievement __instance, ref bool __result)
        {
            if (__instance != null &&
                __instance.Type == FactionAchievementManager.AchievementType.Mechanic)
                __result = true;
        }
    }

    // Unhide vanilla vehicle recipes even when their per-save m_unlocked flag is false.
    [HarmonyPatch(typeof(CraftRecipeInstance), "get_unlocked")]
    internal static class VehicleRecipeUnlockedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CraftRecipeInstance __instance, ref bool __result)
        {
            if (!__result && VehiclesCraftRecipes.ShouldUnlock(__instance)) __result = true;
        }
    }

    // Lazy injection: ItemManager is guaranteed to have loaded its items by the
    // time the Workbench requests available recipes. No save file is modified.
    [HarmonyPatch(typeof(CraftingManager), "GetAllUnlockedRecipesByType")]
    internal static class EnsureVehicleRecipesPatch
    {
        [HarmonyPrefix]
        private static void Prefix(CraftingManager __instance)
        {
            try { VehiclesCraftRecipes.Ensure(__instance); }
            catch (Exception ex) { VehiclesCraftPlugin.Warn("Recipe setup failed: " + ex); }
        }
    }

    // Ensure the vanilla Workbench's recipe-category filter includes Vehicle.
    [HarmonyPatch(typeof(Object_Base_Crafting), "get_RecipeFilter")]
    internal static class WorkbenchVehicleFilterPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Object_Base_Crafting __instance,
                                    ref List<CraftingRecipe.RecipeType> __result)
        {
            if (__instance is Object_WorkBench && __result != null &&
                !__result.Contains(CraftingRecipe.RecipeType.Vehicle))
                __result.Add(CraftingRecipe.RecipeType.Vehicle);
        }
    }
}
