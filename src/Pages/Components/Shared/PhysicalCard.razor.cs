using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components.Shared
{
    /// <summary>
    /// Physical (Obstacle/Challenge) tactical card component
    /// ONLY knows about PhysicalCard - parallel to SocialCard and MentalCard
    /// THREE PARALLEL SYSTEMS: Each card type has its own component with NO coupling
    /// </summary>
    public class PhysicalCardBase : ComponentBase
    {
        [Parameter] public CardInstance Card { get; set; }
        [Parameter] public bool IsSelected { get; set; }
        [Parameter] public bool IsSelectable { get; set; } = true;
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
            if (Card?.PhysicalCardTemplate == null) return "";
            return Card.PhysicalCardTemplate.Category.ToString().ToLower();
        }

        protected string GetCardCategoryName()
        {
            if (Card?.PhysicalCardTemplate == null) return "";
            return Card.PhysicalCardTemplate.Category.ToString();
        }

        protected int GetCardDepth()
        {
            if (Card?.PhysicalCardTemplate == null) return 1;
            return Card.PhysicalCardTemplate.Depth;
        }

        protected string GetCardName()
        {
            if (Card?.PhysicalCardTemplate == null) return "";
            return Card.PhysicalCardTemplate.Name;
        }

        protected int GetCardPositionCost()
        {
            if (Card?.PhysicalCardTemplate == null) return 0;
            return Card.PhysicalCardTemplate.PositionCost;
        }

        protected string GetCardDescription()
        {
            if (Card?.PhysicalCardTemplate == null) return "";
            return Card.PhysicalCardTemplate.Description ?? "";
        }

        protected bool HasEquipmentRequirement()
        {
            if (Card?.PhysicalCardTemplate == null) return false;
            return Card.PhysicalCardTemplate.EquipmentCategory != EquipmentCategory.None;
        }

        protected string GetEquipmentRequirement()
        {
            if (!HasEquipmentRequirement()) return "";
            return $"Requires: {Card.PhysicalCardTemplate.EquipmentCategory}";
        }

        protected bool HasCardCost()
        {
            if (Card?.PhysicalCardTemplate == null) return false;
            return Card.PhysicalCardTemplate.StaminaCost > 0
                || Card.PhysicalCardTemplate.DirectHealthCost > 0
                || Card.PhysicalCardTemplate.CoinCost > 0;
        }

        protected string GetCardCost()
        {
            if (!HasCardCost()) return "";

            var costs = new System.Collections.Generic.List<string>();
            if (Card.PhysicalCardTemplate.StaminaCost > 0) costs.Add($"Stamina -{Card.PhysicalCardTemplate.StaminaCost}");
            if (Card.PhysicalCardTemplate.DirectHealthCost > 0) costs.Add($"Health -{Card.PhysicalCardTemplate.DirectHealthCost}");
            if (Card.PhysicalCardTemplate.CoinCost > 0) costs.Add($"Coins -{Card.PhysicalCardTemplate.CoinCost}");

            return string.Join(", ", costs);
        }

        protected string GetCardEffectDescription()
        {
            if (Card?.PhysicalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // PROJECTION PRINCIPLE: Get effect projection from resolver (Execute as default)
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(Card, Session, player, PhysicalActionType.Execute);

            return projection.EffectDescription ?? "";
        }
    }
}
