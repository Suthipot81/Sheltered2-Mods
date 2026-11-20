using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace Suthipot.Sheltered2.StrongLeader
{
    [BepInPlugin("suthipot.sheltered2.strongleader", "Strong Leader", "0.3.0")]
    public sealed class StrongLeaderPlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool> PreventBasicNeeds;
        internal static ConfigEntry<bool> PreventEnergyLoss;
        internal static ConfigEntry<bool> ProtectLeaderHealth;
        internal static ConfigEntry<int> MinimumHealth;
        internal static ConfigEntry<bool> ImmunityToPlague;
        internal static ConfigEntry<bool> ImmunityToInfection;

        private void Awake()
        {
            try
            {
                PreventBasicNeeds = Config.Bind("Leader", "NoFoodWaterToiletShower", true,
                    "Keep hunger, thirst, toilet and dirtiness needs at zero for the PLAYER LEADER only. Does not affect fatigue or stress.");
                PreventEnergyLoss = Config.Bind("Leader", "NoEnergyLoss", true,
                    "Keep the player leader fatigue/need-for-sleep meter at zero. Works alongside the Perpetual Energy trait. Does not change combat stamina or expedition party energy.");
                ProtectLeaderHealth = Config.Bind("Leader", "EnableHealthFloor", true,
                    "Allow the player leader to take damage, but do not let normal shelter/combat HP loss reduce health below MinimumHealth.");
                MinimumHealth = Config.Bind("Leader", "MinimumHealth", 100,
                    "Minimum HP for the player leader. Default: 100. Requires restart after editing.");

                ImmunityToPlague = Config.Bind("Leader", "ImmuneToPlague", true,
                    "Prevent the PLAYER LEADER from contracting plague, and cure an existing plague status. Does not affect other members.");
                ImmunityToInfection = Config.Bind("Leader", "ImmuneToInfection", true,
                    "Prevent the PLAYER LEADER from contracting the separate Infection illness, and cure an existing infection. Does not affect other members.");

                if (MinimumHealth.Value < 1 || MinimumHealth.Value > 100000)
                {
                    Logger.LogWarning("Invalid MinimumHealth; falling back to 100.");
                    MinimumHealth.Value = 100;
                }

                new Harmony("suthipot.sheltered2.strongleader")
                    .PatchAll(typeof(StrongLeaderPlugin).Assembly);
                Logger.LogInfo("Strong Leader v0.3 active. Needs bypass=" + PreventBasicNeeds.Value +
                               ", no fatigue=" + PreventEnergyLoss.Value + ", health floor=" + (ProtectLeaderHealth.Value ? MinimumHealth.Value.ToString() : "off") +
                               ", plague immune=" + ImmunityToPlague.Value + ", infection immune=" + ImmunityToInfection.Value +
                               ". Player leader only.");
            }
            catch (Exception exception)
            {
                Logger.LogError("Strong Leader Harmony patch failed: " + exception);
            }
        }
    }

    internal static class LeaderGuard
    {
        private static readonly FieldInfo NeedsOwner = AccessTools.Field(typeof(NeedsStats), "m_member");
        private static readonly FieldInfo Hunger = AccessTools.Field(typeof(NeedsStats), "hunger");
        private static readonly FieldInfo Thirst = AccessTools.Field(typeof(NeedsStats), "thirst");
        private static readonly FieldInfo Toilet = AccessTools.Field(typeof(NeedsStats), "toilet");
        private static readonly FieldInfo Dirtiness = AccessTools.Field(typeof(NeedsStats), "dirtiness");
        private static readonly FieldInfo Fatigue = AccessTools.Field(typeof(NeedsStats), "fatigue");
        private static readonly FieldInfo CurrentHp = AccessTools.Field(typeof(BaseCharacter), "m_health");
        private static readonly FieldInfo MaxHp = AccessTools.Field(typeof(BaseCharacter), "m_maxHealth");
        // Illness_Base stores the owning Member in this protected field.
        private static readonly FieldInfo IllnessOwner = AccessTools.Field(typeof(Illness_Base), "m_member");
        private static readonly FieldInfo PlagueBuildUp = AccessTools.Field(typeof(Illness_Plague), "m_currentPlagueBuildUp");

        internal static bool IsPlayerLeader(Member member)
        {
            if (member == null) return false;
            MemberManager manager = MemberManager.instance;
            return manager != null && member.GetID == manager.LeaderId;
        }

        internal static int HpFloor
        {
            get { return StrongLeaderPlugin.MinimumHealth != null ? StrongLeaderPlugin.MinimumHealth.Value : 100; }
        }

        internal static bool NeedsEnabled
        {
            get { return StrongLeaderPlugin.PreventBasicNeeds != null && StrongLeaderPlugin.PreventBasicNeeds.Value; }
        }

        internal static bool EnergyEnabled
        {
            get { return StrongLeaderPlugin.PreventEnergyLoss != null && StrongLeaderPlugin.PreventEnergyLoss.Value; }
        }

        internal static bool HealthEnabled
        {
            get { return StrongLeaderPlugin.ProtectLeaderHealth != null && StrongLeaderPlugin.ProtectLeaderHealth.Value; }
        }

        internal static bool PlagueEnabled
        {
            get { return StrongLeaderPlugin.ImmunityToPlague != null && StrongLeaderPlugin.ImmunityToPlague.Value; }
        }

        internal static bool InfectionEnabled
        {
            get { return StrongLeaderPlugin.ImmunityToInfection != null && StrongLeaderPlugin.ImmunityToInfection.Value; }
        }

        internal static bool IsProtectedIllness(Illness_Base illness)
        {
            if (illness == null || IllnessOwner == null) return false;
            bool isPlague = illness is Illness_Plague;
            bool isInfection = illness is Illness_Infection;
            if ((!isPlague || !PlagueEnabled) && (!isInfection || !InfectionEnabled)) return false;
            Member owner = IllnessOwner.GetValue(illness) as Member;
            return IsPlayerLeader(owner);
        }

        // Also clear an existing disease loaded from a save, rather than simply
        // blocking future infections. Do NOT cure illnesses on other characters.
        internal static void CureProtectedIllnesses(Member member)
        {
            if (!IsPlayerLeader(member)) return;
            IllnessState illnesses = member.illness;
            if (illnesses == null) return;
            if (PlagueEnabled && illnesses.plague != null && illnesses.plague.isActive)
            {
                illnesses.plague.SetActive(false);
                // The vanilla cure method does not clear accumulated plague severity.
                if (PlagueBuildUp != null) PlagueBuildUp.SetValue(illnesses.plague, 0f);
            }
            if (InfectionEnabled && illnesses.infection != null && illnesses.infection.isActive)
                illnesses.infection.SetActive(false);
        }

        private static void ResetNeed(NeedsStats needs, FieldInfo field)
        {
            if (field == null) return;
            NeedsStat stat = field.GetValue(needs) as NeedsStat;
            if (stat != null && stat.Value > 0f)
                stat.Set(0f);
        }

        internal static void ResetNeeds(NeedsStats needs, Member knownMember)
        {
            if (needs == null || (!NeedsEnabled && !EnergyEnabled)) return;
            Member member = knownMember;
            if (member == null && NeedsOwner != null)
                member = NeedsOwner.GetValue(needs) as Member;
            if (!IsPlayerLeader(member)) return;
            // Game's need values are deficiency meters: 0 = no unmet need.
            // Never touch fatigue/stress, loyalty, personality, or non-leader members.
            if (NeedsEnabled)
            {
                ResetNeed(needs, Hunger);
                ResetNeed(needs, Thirst);
                ResetNeed(needs, Toilet);
                ResetNeed(needs, Dirtiness);
            }
            // Fatigue is the Sheltered 2 sleep/energy NEED. A lower value is better.
            // Perpetual Energy itself also prevents sleep; this independent guard
            // does not patch LeaderTraitsManager and only writes when > 0.
            if (EnergyEnabled)
                ResetNeed(needs, Fatigue);
        }

        internal static void ResetNeeds(NeedsStats needs)
        {
            ResetNeeds(needs, null);
        }

        internal static void EnforceShelterHp(Member member)
        {
            if (!HealthEnabled || !IsPlayerLeader(member) || CurrentHp == null || MaxHp == null) return;
            int hp = member.health;
            if (hp <= 0) return; // Never resurrect a member who was already dead.
            int min = HpFloor;
            if (member.maxHealth < min)
                MaxHp.SetValue(member, min);
            if (hp < min)
                CurrentHp.SetValue(member, min);
        }

        internal static bool IsLeaderEncounter(EncounterMember member)
        {
            if (member == null) return false;
            PartyMember partyMember = member.partyMember;
            return partyMember != null && IsPlayerLeader(partyMember.GetMember);
        }
    }

    [HarmonyPatch(typeof(NeedsStats), "Initialize")]
    internal static class InitialNeedsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NeedsStats __instance)
        {
            LeaderGuard.ResetNeeds(__instance);
        }
    }

    [HarmonyPatch(typeof(NeedsStats), "UpdateStats")]
    internal static class TickNeedsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(NeedsStats __instance) { LeaderGuard.ResetNeeds(__instance); }

        [HarmonyPostfix]
        private static void Postfix(NeedsStats __instance) { LeaderGuard.ResetNeeds(__instance); }
    }

    [HarmonyPatch(typeof(NeedsStats), "UpdateNeedsOnReturnToShelter")]
    internal static class ExpeditionReturnNeedsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(NeedsStats __instance) { LeaderGuard.ResetNeeds(__instance); }

        [HarmonyPostfix]
        private static void Postfix(NeedsStats __instance) { LeaderGuard.ResetNeeds(__instance); }
    }

    // Expeditions increment fatigue/hunger/thirst separately from NeedsStats ticks.
    // Apply the same leader-only guard when the party completes an exploration tick.
    [HarmonyPatch(typeof(PartyMember), "UpdateNeeds")]
    internal static class ExpeditionNeedsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(PartyMember __instance)
        {
            if (__instance == null) return;
            Member member = __instance.GetMember;
            if (LeaderGuard.IsPlayerLeader(member))
                LeaderGuard.ResetNeeds(member.needs, member);
        }
    }

    [HarmonyPatch(typeof(Member), "Update")]
    internal static class LeaderFrameGuardPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Member __instance)
        {
            if (!LeaderGuard.IsPlayerLeader(__instance)) return;
            LeaderGuard.ResetNeeds(__instance.needs, __instance);
            LeaderGuard.CureProtectedIllnesses(__instance);
            LeaderGuard.EnforceShelterHp(__instance);
        }
    }

    // Vanilla Member.Damage delegates to BaseCharacter.Damage.
    // Reduce only the incoming amount that would take the leader below 100 HP;
    // normal damage, damage type, combat animations and floating text still run.
    [HarmonyPatch(typeof(BaseCharacter), "Damage")]
    internal static class ShelterDamageFloorPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BaseCharacter __instance, ref int __0)
        {
            if (!LeaderGuard.HealthEnabled || __0 <= 0) return;
            Member member = __instance as Member;
            if (!LeaderGuard.IsPlayerLeader(member)) return;
            int allowed = Math.Max(0, member.health - LeaderGuard.HpFloor);
            if (__0 > allowed) __0 = allowed;
        }
    }

    // Combat clones the party member into EncounterMember and applies damage
    // through set_Health(int). It runs its death check *inside* the setter,
    // so the incoming value must be clamped BEFORE calling vanilla code.
    // Other party members and all NPCs retain their normal damage/death rules.
    [HarmonyPatch(typeof(EncounterMember), "set_Health")]
    internal static class CombatDamageFloorPatch
    {
        [HarmonyPrefix]
        private static void Prefix(EncounterMember __instance, ref int __0)
        {
            if (!LeaderGuard.HealthEnabled || !LeaderGuard.IsLeaderEncounter(__instance)) return;
            int floor = LeaderGuard.HpFloor;
            if (__0 < floor) __0 = floor;
        }
    }


// Vanilla Illness_Base.SetActive(true) is the central activation path used by
// IllnessState, plague exposure checks and infection events. Refuse activation
// only when the illness belongs to the player leader and is protected.
[HarmonyPatch(typeof(Illness_Base), "SetActive")]
internal static class LeaderIllnessActivationPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Illness_Base __instance, bool __0)
    {
        if (!__0) return true; // Never prevent normal cures.
        return !LeaderGuard.IsProtectedIllness(__instance);
    }
}

// Plague can deal a lethal hit from inside UpdateIllness once build-up reaches
// its maximum. Do not let that tick execute for a protected leader. This also
// cures plague that was already active in a saved game before the mod upgrade.
[HarmonyPatch(typeof(Illness_Plague), "UpdateIllness")]
internal static class LeaderPlagueTickPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Illness_Plague __instance)
    {
        if (!LeaderGuard.IsProtectedIllness(__instance)) return true;
        if (__instance.isActive) __instance.SetActive(false);
        return false;
    }
}

// The game has a second disease, Illness_Infection, independent of Plague.
// Keep it disabled for the leader without touching other illnesses.
[HarmonyPatch(typeof(Illness_Infection), "UpdateIllness")]
internal static class LeaderInfectionTickPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Illness_Infection __instance)
    {
        if (!LeaderGuard.IsProtectedIllness(__instance)) return true;
        if (__instance.isActive) __instance.SetActive(false);
        return false;
    }
}
}
