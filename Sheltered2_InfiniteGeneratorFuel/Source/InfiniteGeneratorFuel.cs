using System;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.InfiniteGeneratorFuel
{
    [BepInPlugin("suthipot.sheltered2.infinitegeneratorfuel", "Infinite Generator Fuel", "0.1.0")]
    public sealed class InfiniteGeneratorFuelPlugin : BaseUnityPlugin
    {
        internal static bool InfiniteFuelEnabled = true;
        internal static float GeneratorOutputPerGameHour = 200f;

        private void Awake()
        {
            try
            {
                InfiniteFuelEnabled = Config.Bind(
                    "Generator", "InfiniteFuel", true,
                    "If true, generator fuel is not consumed by normal electricity production. " +
                    "An initially empty generator still needs at least one fuel can. " +
                    "Tutorial ForceFuelDepletion remains untouched.").Value;

                GeneratorOutputPerGameHour = Config.Bind(
                    "Generator", "OutputPerGameHour", 200f,
                    "Generator maximum output per in-game hour (vanilla is typically 8; mod default 200). " +
                    "Restart the game after changing this setting.").Value;

                if (Single.IsNaN(GeneratorOutputPerGameHour) ||
                    Single.IsInfinity(GeneratorOutputPerGameHour) ||
                    GeneratorOutputPerGameHour < 0.01f)
                {
                    GeneratorOutputPerGameHour = 200f;
                    Logger.LogWarning("Invalid OutputPerGameHour; using 200.");
                }

                new Harmony("suthipot.sheltered2.infinitegeneratorfuel")
                    .PatchAll(typeof(InfiniteGeneratorFuelPlugin).Assembly);

                Logger.LogInfo("Infinite Generator Fuel v0.1 active: InfiniteFuel=" +
                    InfiniteFuelEnabled + ", OutputPerGameHour=" +
                    GeneratorOutputPerGameHour + ".");
            }
            catch (Exception ex)
            {
                Logger.LogError("Infinite Generator Fuel startup patch failed: " + ex);
            }
        }
    }

    // Object_PowerSource.Start calculates m_outputPerSecond based on this field:
    // m_outputPerSecond = m_maxOutputPerGameHour / (TimeManager.secondsInGameDay / 24f).
    // Change the generator-specific input *before* vanilla Start calculates per-tick output.
    // Does not modify batteries, incinerators, or other power sources.
    [HarmonyPatch(typeof(Object_PowerSource), "Start")]
    internal static class GeneratorMaxOutputPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Object_PowerSource __instance,
                                   ref float ___m_maxOutputPerGameHour)
        {
            if (__instance is Object_Generator)
                ___m_maxOutputPerGameHour =
                    InfiniteGeneratorFuelPlugin.GeneratorOutputPerGameHour;
        }
    }

    // Only fake a large fuel reserve within the three energy-calculation calls.
    // The true saved m_currentCapacity is restored immediately afterward.
    // This preserves the original power routing, power usage statistics, state
    // checks, and refueling logic, while avoiding low-fuel output clamping.
    internal static class GeneratorFuelScope
    {
        private const float SimulationReserve = 1000000f;

        internal static void Enter(Object_PowerSourceWithStorage source,
                                   ref float currentCapacity,
                                   out float previousCapacity)
        {
            previousCapacity = float.NaN;  // sentinel = no change
            if (!InfiniteGeneratorFuelPlugin.InfiniteFuelEnabled ||
                !(source is Object_Generator) || currentCapacity <= 0f)
                return;

            previousCapacity = currentCapacity;
            currentCapacity = Math.Max(currentCapacity, SimulationReserve);
        }

        internal static void Exit(ref float currentCapacity, float previousCapacity)
        {
            if (!float.IsNaN(previousCapacity))
                currentCapacity = previousCapacity;
        }
    }

    // 1) Recalculate the tick's available output without fuel-based throttling.
    [HarmonyPatch(typeof(Object_PowerSourceWithStorage), "OnNewTick")]
    internal static class GeneratorFuelTickPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Object_PowerSourceWithStorage __instance,
                                   ref float ___m_currentCapacity,
                                   out float __state)
        {
            GeneratorFuelScope.Enter(__instance, ref ___m_currentCapacity, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(ref float ___m_currentCapacity, float __state)
        {
            GeneratorFuelScope.Exit(ref ___m_currentCapacity, __state);
        }
    }

    // 2) Report the available output without the usual fuel cap.
    [HarmonyPatch(typeof(Object_PowerSourceWithStorage), "GetPower_Tick")]
    internal static class GeneratorFuelAvailabilityPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Object_PowerSourceWithStorage __instance,
                                   ref float ___m_currentCapacity,
                                   out float __state)
        {
            GeneratorFuelScope.Enter(__instance, ref ___m_currentCapacity, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(ref float ___m_currentCapacity, float __state)
        {
            GeneratorFuelScope.Exit(ref ___m_currentCapacity, __state);
        }
    }

    // 3) Deliver the requested power, then restore actual fuel to its original amount.
    // Does not prevent adding fuel, changing generator status, or tutorial depletion.
    [HarmonyPatch(typeof(Object_PowerSourceWithStorage), "RequestPower")]
    internal static class GeneratorFuelConsumptionPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Object_PowerSourceWithStorage __instance,
                                   ref float ___m_currentCapacity,
                                   out float __state)
        {
            GeneratorFuelScope.Enter(__instance, ref ___m_currentCapacity, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(ref float ___m_currentCapacity, float __state)
        {
            GeneratorFuelScope.Exit(ref ___m_currentCapacity, __state);
        }
    }
}
