using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components.Shared
{
    /// <summary>
    /// Physical (Scene/Challenge) tactical card component
    /// ONLY knows about PhysicalCard - parallel to SocialCard and MentalCard
    /// THREE PARALLEL SYSTEMS: Each card type has its own component with NO coupling
    /// </summary>
    public class PhysicalCardBase : ComponentBase
    {
        [Parameter] public CardInstance Card { get; set; }
        [Parameter] public bool IsSelected { get; set; }
        [Parameter] public bool IsSelectable { get; set; } = true;
        [Parameter] public bool IsLocked { get; set; }
        [Parameter] public EventCallback OnCardClick { get; set; }
        [Parameter] public RenderFragment SystemSpecificBadges { get; set; }

        // PHYSICAL SYSTEM ONLY: PhysicalSession, PhysicalEffectResolver, GameFacade
        [Inject] protected PhysicalEffectResolver EffectResolver { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Parameter] public PhysicalSession Session { get; set; }

        protected async Task HandleClick()
        {
            await OnCardClick.InvokeAsync();
        }

        protected string GetCardCategoryClass()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.Category.ToString().ToLower();
        }

        protected string GetCardCategoryName()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.Category.ToString();
        }

        protected int GetCardDepth()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.Depth;
        }

        protected string GetCardName()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.Name;
        }

        protected int GetCardExertionCost()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.ExertionCost;
        }

        protected string GetCardDescription()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.Description ?? "";
        }

        protected bool HasEquipmentRequirement()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.EquipmentCategory != EquipmentCategory.None;
        }

        protected string GetEquipmentRequirement()
        {
            if (!HasEquipmentRequirement())
                return "";

            return $"Requires: {Card.PhysicalCardTemplate.EquipmentCategory}";
        }

        protected bool HasCardCost()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");

            return Card.PhysicalCardTemplate.StaminaCost > 0
                || Card.PhysicalCardTemplate.DirectHealthCost > 0
                || Card.PhysicalCardTemplate.CoinCost > 0;
        }

        protected string GetCardCost()
        {
            if (!HasCardCost())
                return "";

            List<string> costs = new System.Collections.Generic.List<string>();
            if (Card.PhysicalCardTemplate.StaminaCost > 0) costs.Add($"Stamina -{Card.PhysicalCardTemplate.StaminaCost}");
            if (Card.PhysicalCardTemplate.DirectHealthCost > 0) costs.Add($"Health -{Card.PhysicalCardTemplate.DirectHealthCost}");
            if (Card.PhysicalCardTemplate.CoinCost > 0) costs.Add($"Coins -{Card.PhysicalCardTemplate.CoinCost}");

            return string.Join(", ", costs);
        }

        protected string GetCardEffectDescription()
        {
            if (Card == null)
                throw new InvalidOperationException("Card parameter is required");
            if (Card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("PhysicalCardTemplate is required");
            if (Session == null)
                throw new InvalidOperationException("Session is required for effect projection");
            if (GameFacade == null)
                throw new InvalidOperationException("GameFacade is required for effect projection");

            Player player = GameFacade.GetPlayer();

            // PROJECTION PRINCIPLE: Get effect projection from resolver (Execute as default)
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(Card, Session, player, PhysicalActionType.Execute);

            return projection.EffectDescription ?? "";
        }
    }
}
