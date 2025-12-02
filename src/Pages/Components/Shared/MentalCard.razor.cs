using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components.Shared
{
    /// <summary>
    /// Mental (Obligation) tactical card component
    /// ONLY knows about MentalCard - parallel to SocialCard and PhysicalCard
    /// THREE PARALLEL SYSTEMS: Each card type has its own component with NO coupling
    /// </summary>
    public class MentalCardBase : ComponentBase
    {
        [Parameter] public CardInstance Card { get; set; }
        [Parameter] public bool IsSelected { get; set; }
        [Parameter] public bool IsSelectable { get; set; } = true;
        [Parameter] public EventCallback OnCardClick { get; set; }
        [Parameter] public RenderFragment SystemSpecificBadges { get; set; }

        // MENTAL SYSTEM ONLY: MentalSession, MentalEffectResolver, GameOrchestrator
        [Inject] protected MentalEffectResolver EffectResolver { get; set; }
        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }
        [Parameter] public MentalSession Session { get; set; }

        protected async Task HandleClick()
        {
            await OnCardClick.InvokeAsync();
        }

        protected string GetCardCategoryClass()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.Category.ToString().ToLower();
        }

        protected string GetCardCategoryName()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.Category.ToString();
        }

        protected int GetCardDepth()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.Depth;
        }

        protected string GetCardName()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.Name;
        }

        protected int GetCardAttentionCost()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.AttentionCost;
        }

        protected string GetCardDescription()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.Description ?? "";
        }

        protected bool HasEquipmentRequirement()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            return Card.MentalCardTemplate.EquipmentCategory != EquipmentCategory.None;
        }

        protected string GetEquipmentRequirement()
        {
            if (!HasEquipmentRequirement())
                return "";

            return $"Requires: {Card.MentalCardTemplate.EquipmentCategory}";
        }

        protected bool HasCardCost()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");

            // NOTE: Mental cards have NO health/stamina costs - only CoinCost (rare)
            return Card.MentalCardTemplate.CoinCost > 0;
        }

        protected string GetCardCost()
        {
            if (!HasCardCost())
                return "";

            List<string> costs = new System.Collections.Generic.List<string>();
            if (Card.MentalCardTemplate.CoinCost > 0) costs.Add($"Coins -{Card.MentalCardTemplate.CoinCost}");

            return string.Join(", ", costs);
        }

        protected string GetCardEffectDescription()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.MentalCardTemplate == null)
                throw new InvalidOperationException("MentalCardTemplate is required");
            if (Session == null)
                throw new InvalidOperationException("Session is required for effect projection");
            if (GameOrchestrator == null)
                throw new InvalidOperationException("GameOrchestrator is required for effect projection");

            Player player = GameOrchestrator.GetPlayer();

            // PROJECTION PRINCIPLE: Get effect projection from resolver (Act as default)
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(Card, Session, player, MentalActionType.Act);

            return projection.EffectDescription ?? "";
        }
    }
}
