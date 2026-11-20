using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.Attribute30
{
    [BepInPlugin("suthipot.sheltered2.attribute30", "30 Attribute Points", "0.2.0")]
    public sealed class AttributePointsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                new Harmony("suthipot.sheltered2.attribute30").PatchAll(typeof(AttributePointsPlugin).Assembly);
                Logger.LogInfo("30 Attribute Points v0.2: character editor patches active.");
            }
            catch (Exception ex)
            {
                Logger.LogError("30 Attribute Points patch failed: " + ex);
            }
        }
    }

    internal static class PointsBudget
    {
        private static bool hasInitialSum;
        private static int initialSum;
        private static readonly string[] StatFields = {
            "m_strengthLevel", "m_dexterityLevel", "m_charismaLevel",
            "m_perceptionLevel", "m_intelligenceLevel", "m_fortitudeLevel"
        };

        // Populate() has already written this character's six starting stats
        // by the time it asks for the total pool. Cache the first preset sum
        // so the first screen shows 30 REMAINING points, rather than 30 - stats.
        public static int TotalBudget(Customisation_Stats editor)
        {
            if (!hasInitialSum)
            {
                try
                {
                    FieldInfo attrField = AccessTools.Field(typeof(CustomisationPanel), "attribs");
                    object attrs = attrField == null ? null : attrField.GetValue(editor);
                    if (attrs != null)
                    {
                        int total = 0;
                        for (int i = 0; i < StatFields.Length; ++i)
                        {
                            FieldInfo statField = AccessTools.Field(attrs.GetType(), StatFields[i]);
                            if (statField == null) throw new MissingFieldException(StatFields[i]);
                            total += Convert.ToInt32(statField.GetValue(attrs));
                        }
                        initialSum = total;
                        hasInitialSum = true;
                    }
                }
                catch (Exception) { /* First-preset fallback: still allow a large pool. */ }
            }
            return 30 + initialSum;
        }
    }

    [HarmonyPatch(typeof(Customisation_Stats), "Populate")]
    internal static class PopulatePointsPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            FieldInfo pointsField = AccessTools.Field(typeof(Customisation_Stats), "m_remainingPoints");
            MethodInfo budgetMethod = AccessTools.Method(typeof(PointsBudget), "TotalBudget");
            if (pointsField == null || budgetMethod == null)
                throw new MissingMemberException("Character editor point fields/method not found");

            bool replaced = false;
            for (int i = 0; i < code.Count && !replaced; i++)
            {
                // Vanilla code loads 18 before subtracting 6 initial stat levels.
                // Anchor to the following stfld m_remainingPoints, not any random 24.
                bool is18 = code[i].opcode == OpCodes.Ldc_I4_S &&
                            Convert.ToInt32(code[i].operand) == 18;
                if (!is18) continue;
                for (int j = i + 1; j < code.Count && j < i + 55; j++)
                {
                    if (code[j].opcode == OpCodes.Stfld &&
                        object.Equals(code[j].operand, pointsField))
                    {
                        CodeInstruction first = new CodeInstruction(OpCodes.Ldarg_0);
                        first.labels.AddRange(code[i].labels);
                        first.blocks.AddRange(code[i].blocks);
                        code[i] = first;
                        code.Insert(i + 1, new CodeInstruction(OpCodes.Call, budgetMethod));
                        replaced = true;
                        break;
                    }
                }
            }
            if (!replaced)
                throw new InvalidOperationException("Could not locate the vanilla 18-point budget calculation");
            return code;
        }
    }

    // The game's normal validation forbids leaving the screen with ANY points
    // left over. With spare points and vanilla stat caps this would soft-lock
    // character creation; the body is verified to check only this condition.
    [HarmonyPatch(typeof(CharacterCustomisation), "VerifyPanelCanBeClosed")]
    internal static class CloseCharacterEditorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
