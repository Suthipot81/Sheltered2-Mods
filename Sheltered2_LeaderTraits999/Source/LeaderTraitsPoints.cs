using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.LeaderTrait999
{
    [BepInPlugin("suthipot.sheltered2.leadertrait999", "999 Leader Trait Points", "0.1.0")]
    public sealed class LeaderTraitPointsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                new Harmony("suthipot.sheltered2.leadertrait999")
                    .PatchAll(typeof(LeaderTraitPointsPlugin).Assembly);
                Logger.LogInfo("999 Leader Trait Points v0.1: initial trait budget patch active.");
            }
            catch (Exception ex)
            {
                Logger.LogError("999 Leader Trait Points patch failed: " + ex);
            }
        }
    }

    // Sheltered 2 initial leader trait customisation screen starts at 5 points.
    // In the vanilla CustomisationPanel_LeaderTraits.Populate IL:
    //   ldarg.0; ldc.i4.5; stfld int32 m_currentPoints
    //   for each already-selected trait: m_currentPoints += trait.m_cost
    // Change ONLY that 5 to +999, leaving negative/positive trait costs
    // and the rest of the system (leader levels, points in the live game) unchanged.
    [HarmonyPatch(typeof(CustomisationPanel_LeaderTraits), "Populate")]
    internal static class StartingLeaderTraitsBudgetPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            FieldInfo pointsField = AccessTools.Field(typeof(CustomisationPanel_LeaderTraits),
                "m_currentPoints");
            if (pointsField == null)
                throw new MissingFieldException("CustomisationPanel_LeaderTraits.m_currentPoints");

            int changed = 0;
            for (int i = 0; i + 1 < code.Count; i++)
            {
                if (code[i].opcode != OpCodes.Ldc_I4_5) continue;
                if (code[i + 1].opcode != OpCodes.Stfld) continue;
                if (!object.Equals(code[i + 1].operand, pointsField)) continue;

                // Modify in place: preserve Harmony labels and exception blocks.
                code[i].opcode = OpCodes.Ldc_I4;
                code[i].operand = 999;
                changed++;
            }
            if (changed != 1)
                throw new InvalidOperationException("Expected exactly one 5-point leader traits initializer; found " + changed);
            return code;
        }
    }
}
