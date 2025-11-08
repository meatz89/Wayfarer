using Microsoft.AspNetCore.Components;

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
            // Observation decay system has been removed
            return false;
        }

        protected string GetCardStatClass()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            // Situation cards use special styling
            if (Card.CardType == CardTypes.Situation)
                return "situation";

            // Conversation cards use BoundStat for styling
            if (Card.SocialCardTemplate.BoundStat == null) return "";
            return Card.SocialCardTemplate.BoundStat.Value.ToString().ToLower();
        }

        protected string GetCardStatName()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            if (Card.SocialCardTemplate.BoundStat == null) return "";

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
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            if (Card.SocialCardTemplate.Depth == null) return 1;
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
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            return Card.SocialCardTemplate.Title;
        }

        protected int GetCardInitiativeCost()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            return Card.SocialCardTemplate.InitiativeCost;
        }

        protected string GetCardDialogue()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            // First check for AI-generated narrative
            if (CardNarratives != null)
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
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");

            return Card.SocialCardTemplate.RequiredStat.HasValue
                && Card.SocialCardTemplate.RequiredStatements > 0;
        }

        protected string GetCardRequirement()
        {
            if (!HasCardRequirement())
                return "";

            if (Session == null)
                throw new InvalidOperationException("Session is required when card has requirements");

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
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.SocialCardTemplate == null)
                throw new InvalidOperationException("SocialCardTemplate is required");
            if (Session == null)
                throw new InvalidOperationException("Session is required for effect projection");

            // PROJECTION PRINCIPLE: Get effect projection from resolver
            CardEffectResult projection = EffectResolver.ProcessSuccessEffect(Card, Session);

            // Use EffectOnlyDescription for card display (excludes Initiative generation)
            return projection.EffectOnlyDescription?.Replace(", +", " +").Replace("Promise made, ", "") ?? "";
        }
    }
}
