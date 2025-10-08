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
        [Parameter] public SocialSession Session { get; set; }

        protected async Task HandleClick()
        {
            await OnCardClick.InvokeAsync();
        }

        protected bool IsObservationExpired()
        {
            return Card.SocialCardTemplate.IsGoalCard == CardType.Observation &&
                   Card.Context?.ObservationDecayState == nameof(ObservationDecayState.Expired);
        }

        protected string GetCardStatClass()
        {
            // Request cards use CardType for styling
            if (Card?.SocialCardTemplate?.IsGoalCard == CardType.Request)
                return Card.SocialCardTemplate.IsGoalCard.ToString().ToLower();

            // Conversation cards use BoundStat for styling
            if (Card?.SocialCardTemplate?.BoundStat == null) return "";
            return Card.SocialCardTemplate.BoundStat.Value.ToString().ToLower();
        }

        protected string GetCardStatName()
        {
            if (Card?.SocialCardTemplate?.BoundStat == null) return "";

            return Card.SocialCardTemplate.BoundStat.Value switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Diplomacy => "Diplomacy",
                PlayerStatType.Cunning => "Cunning",
                _ => Card.SocialCardTemplate.BoundStat.Value.ToString()
            };
        }

        protected int GetCardDepth()
        {
            if (Card?.SocialCardTemplate?.Depth == null) return 1;
            return (int)Card.SocialCardTemplate.Depth;
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
            return Card.SocialCardTemplate.Title;
        }

        protected int GetCardInitiativeCost()
        {
            if (Card?.SocialCardTemplate == null) return 0;
            return Card.SocialCardTemplate.InitiativeCost;
        }

        protected string GetCardDialogue()
        {
            // First check for AI-generated narrative
            if (CardNarratives != null && Card != null)
            {
                CardNarrative cardNarrative = CardNarratives.FirstOrDefault(cn => cn.CardId == Card.SocialCardTemplate.Id);
                if (cardNarrative != null && !string.IsNullOrEmpty(cardNarrative.NarrativeText))
                    return cardNarrative.NarrativeText;
            }

            // Use DialogueText property - this is what the player says
            return Card.SocialCardTemplate.DialogueText ?? "";
        }

        protected bool HasCardRequirement()
        {
            if (Card?.SocialCardTemplate == null) return false;

            return Card.SocialCardTemplate.RequiredStat.HasValue
                && Card.SocialCardTemplate.RequiredStatements > 0;
        }

        protected string GetCardRequirement()
        {
            if (!HasCardRequirement() || Session == null) return "";

            PlayerStatType reqStat = Card.SocialCardTemplate.RequiredStat.Value;
            int reqCount = Card.SocialCardTemplate.RequiredStatements;
            int currentCount = Session.GetStatementCount(reqStat);

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
            if (Card?.SocialCardTemplate == null) return "";
            if (Session == null) return "";

            // PROJECTION PRINCIPLE: Get effect projection from resolver
            CardEffectResult projection = EffectResolver.ProcessSuccessEffect(Card, Session);

            // Use EffectOnlyDescription for card display (excludes Initiative generation)
            return projection.EffectOnlyDescription?.Replace(", +", " +").Replace("Promise made, ", "") ?? "";
        }
    }
}
