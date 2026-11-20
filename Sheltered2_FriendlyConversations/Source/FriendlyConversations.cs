using System;
using BepInEx;
using HarmonyLib;

namespace Suthipot.Sheltered2.FriendlyConversations
{
    [BepInPlugin("suthipot.sheltered2.friendlyconversations", "Friendly Conversations", "0.1.0")]
    public sealed class FriendlyConversationsPlugin : BaseUnityPlugin
    {
        internal static bool FriendlyOutcomes = true;
        internal static bool FriendlyReactions = true;
        internal static bool SuperPositive = false;

        private void Awake()
        {
            try
            {
                FriendlyOutcomes = Config.Bind("Conversations", "FriendlyOutcomes", true,
                    "Replace negative/neutral outcomes of ordinary shelter conversations with positive outcomes.").Value;

                FriendlyReactions = Config.Bind("Conversations", "FriendlyReactions", true,
                    "Show friendly topic reactions (Like) during ordinary shelter conversations.").Value;

                SuperPositive = Config.Bind("Conversations", "SuperPositive", false,
                    "If true, ordinary conversations are always SuperPositive. If false, Positive.").Value;

                new Harmony("suthipot.sheltered2.friendlyconversations")
                    .PatchAll(typeof(FriendlyConversationsPlugin).Assembly);
                Logger.LogInfo("Friendly Conversations v0.1 patched. Outcomes=" + FriendlyOutcomes +
                    ", Reactions=" + FriendlyReactions + ", SuperPositive=" + SuperPositive);
            }
            catch (Exception ex)
            {
                Logger.LogError("Friendly Conversations startup patch failed: " + ex);
            }
        }
    }

    internal static class FriendlyRules
    {
        internal static MemberAI.ConversationOutcome PositiveOutcome
        {
            get
            {
                return FriendlyConversationsPlugin.SuperPositive
                    ? MemberAI.ConversationOutcome.SuperPositive
                    : MemberAI.ConversationOutcome.Positive;
            }
        }

        internal static bool ShouldConvert(MemberAI.ConversationOutcome result)
        {
            // The None value denotes no actual conversation; keep it unchanged.
            return FriendlyConversationsPlugin.FriendlyOutcomes &&
                   result != MemberAI.ConversationOutcome.None;
        }
    }

    // The routine that chooses the outcome of a normal MemberAI conversation.
    // This occurs before the Job_Conversation is created and controls its
    // relationship outcome and visible response animation/icon.
    [HarmonyPatch(typeof(MemberAI), "DetermineConversationOutcome")]
    internal static class PositiveConversationOutcomePatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref MemberAI.ConversationOutcome __result)
        {
            if (FriendlyRules.ShouldConvert(__result))
                __result = FriendlyRules.PositiveOutcome;
        }
    }

    // While chatting in the shelter, normal topic responses are Like instead
    // of Hate/Dislike. This avoids negative conversation response icons.
    [HarmonyPatch(typeof(MemberAI), "DetermineConversationReaction")]
    internal static class FriendlyTopicReactionPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref MemberAI.ConversationTopicReaction __result)
        {
            if (FriendlyConversationsPlugin.FriendlyReactions)
                __result = MemberAI.ConversationTopicReaction.Like;
        }
    }

    // Safety net for conversations already queued or saved before the mod
    // was installed: convert the stored outcome immediately before vanilla
    // FinishJob updates relationships. This does not patch other jobs or
    // Breakdown_Altercation; those remain under their normal game rules.
    [HarmonyPatch(typeof(Job_Conversation), "FinishJob")]
    internal static class FriendlyConversationFinishPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref MemberAI.ConversationOutcome ___outcome)
        {
            if (FriendlyRules.ShouldConvert(___outcome))
                ___outcome = FriendlyRules.PositiveOutcome;
        }
    }
}
