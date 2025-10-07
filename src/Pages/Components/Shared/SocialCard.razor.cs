using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components.Shared
{
    /// <summary>
    /// Social (Conversation) tactical card component
    /// ONLY knows about ConversationCard - parallel to MentalCard and PhysicalCard
    /// THREE PARALLEL SYSTEMS: Each card type has its own component with NO coupling
    /// </summary>
    public class SocialCardBase : ComponentBase
    {
        [Parameter] public CardInstance Card { get; set; }
        [Parameter] public bool IsSelected { get; set; }
        [Parameter] public bool IsSelectable { get; set; } = true;
        [Parameter] public EventCallback OnCardClick { get; set; }
        [Parameter] public RenderFragment SystemSpecificBadges { get; set; }
        [Parameter] public List<CardNarrative> CardNarratives { get; set; }

        // SOCIAL SYSTEM ONLY: ConversationSession and CategoricalEffectResolver
        [Inject] protected SocialEffectResolver EffectResolver { get; set; }
        [Parameter] public SocialChallengeSession Session { get; set; }

        protected async Task HandleClick()
        {
            await OnCardClick.InvokeAsync();
        }

        protected bool IsObservationExpired()
        {
            return Card.ConversationCardTemplate.CardType == CardType.Observation &&
                   Card.Context?.ObservationDecayState == nameof(ObservationDecayState.Expired);
        }

        protected string GetCardStatClass()
        {
            // Request cards use CardType for styling
            if (Card?.ConversationCardTemplate?.CardType == CardType.Request)
                return Card.ConversationCardTemplate.CardType.ToString().ToLower();

            // Conversation cards use BoundStat for styling
            if (Card?.ConversationCardTemplate?.BoundStat == null) return "";
            return Card.ConversationCardTemplate.BoundStat.Value.ToString().ToLower();
        }

        protected string GetCardStatName()
        {
            if (Card?.ConversationCardTemplate?.BoundStat == null) return "";

            return Card.ConversationCardTemplate.BoundStat.Value switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Diplomacy => "Diplomacy",
                PlayerStatType.Cunning => "Cunning",
                _ => Card.ConversationCardTemplate.BoundStat.Value.ToString()
            };
        }

        protected int GetCardDepth()
        {
            if (Card?.ConversationCardTemplate?.Depth == null) return 1;
            return (int)Card.ConversationCardTemplate.Depth;
        }

        protected string GetTraitClass(CardTrait trait)
        {
            return $"trait-{trait.ToString().ToLower()}";
        }

        protected string GetTraitDisplayName(CardTrait trait)
        {
            return trait switch
            {
                CardTrait.SuppressSpeakCadence => "Persistent Effect",
                _ => trait.ToString()
            };
        }

        protected string GetTraitTooltip(CardTrait trait)
        {
            return trait switch
            {
                CardTrait.SuppressSpeakCadence => "This card's Cadence effect is not reduced by playing it (SPEAK action doesn't counter the effect)",
                _ => trait.ToString()
            };
        }

        protected string GetCardName()
        {
            return Card.ConversationCardTemplate.Title;
        }

        protected int GetCardInitiativeCost()
        {
            if (Card?.ConversationCardTemplate == null) return 0;
            return Card.ConversationCardTemplate.InitiativeCost;
        }

        protected string GetCardDialogue()
        {
            // First check for AI-generated narrative
            if (CardNarratives != null && Card != null)
            {
                CardNarrative cardNarrative = CardNarratives.FirstOrDefault(cn => cn.CardId == Card.ConversationCardTemplate.Id);
                if (cardNarrative != null && !string.IsNullOrEmpty(cardNarrative.NarrativeText))
                    return cardNarrative.NarrativeText;
            }

            // Use DialogueText property - this is what the player says
            return Card.ConversationCardTemplate.DialogueText ?? "";
        }

        protected bool HasCardRequirement()
        {
            if (Card?.ConversationCardTemplate == null) return false;

            return Card.ConversationCardTemplate.RequiredStat.HasValue
                && Card.ConversationCardTemplate.RequiredStatements > 0;
        }

        protected string GetCardRequirement()
        {
            if (!HasCardRequirement() || Session == null) return "";

            var reqStat = Card.ConversationCardTemplate.RequiredStat.Value;
            var reqCount = Card.ConversationCardTemplate.RequiredStatements;
            var currentCount = Session.GetStatementCount(reqStat);

            bool requirementMet = currentCount >= reqCount;
            string status = requirementMet ? "✓" : "✗";

            return $"{status} Requires {reqCount} {reqStat} Statements (Current: {currentCount})";
        }

        protected bool HasCardCost()
        {
            // Strategic costs shown in effect description
            return false;
        }

        protected string GetCardCost()
        {
            return "";
        }

        protected string GetCardEffectDescription()
        {
            if (Card?.ConversationCardTemplate == null) return "";
            if (Session == null) return "";

            // PROJECTION PRINCIPLE: Get effect projection from resolver
            CardEffectResult projection = EffectResolver.ProcessSuccessEffect(Card, Session);

            // Use EffectOnlyDescription for card display (excludes Initiative generation)
            return projection.EffectOnlyDescription?.Replace(", +", " +").Replace("Promise made, ", "") ?? "";
        }
    }
}
