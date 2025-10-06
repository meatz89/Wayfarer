using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class PhysicalContentBase : ComponentBase
    {
        [CascadingParameter] public GameScreen ParentScreen { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The PhysicalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// Parallel to CategoricalEffectResolver in Conversation system.
        /// </summary>
        [Inject] protected PhysicalEffectResolver EffectResolver { get; set; }

        protected PhysicalSession Session => ParentScreen?.PhysicalSession;
        protected List<CardInstance> Hand => GameFacade?.IsPhysicalSessionActive() == true
            ? GameFacade.GetPhysicalFacade().GetHand()
            : new List<CardInstance>();
        protected CardInstance SelectedCard { get; set; }
        protected string LastNarrative { get; set; } = "";
        protected bool IsProcessing { get; set; } = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        protected int GetCurrentTier()
        {
            if (Session == null || Session.UnlockedTiers == null || !Session.UnlockedTiers.Any())
                return 1;

            return Session.UnlockedTiers.Max();
        }

        protected int GetHandCount()
        {
            return Hand?.Count ?? 0;
        }

        protected void SelectCard(CardInstance card)
        {
            SelectedCard = (SelectedCard == card) ? null : card;
        }

        protected async Task ExecuteAssess()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var result = await GameFacade.ExecuteAssess(SelectedCard);

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhysicalContent] Error in ExecuteAssess: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteExecute()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var result = await GameFacade.ExecuteExecute(SelectedCard);

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhysicalContent] Error in ExecuteExecute: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task EndChallenge()
        {
            if (IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var outcome = GameFacade.EndPhysicalSession();

                if (outcome != null)
                {
                    LastNarrative = outcome.Success
                        ? $"Challenge complete! Progress: {outcome.FinalProgress}, Danger: {outcome.FinalDanger}"
                        : $"Challenge incomplete. Progress: {outcome.FinalProgress}, Danger: {outcome.FinalDanger}";
                }

                await ParentScreen.HandlePhysicalEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhysicalContent] Error in EndChallenge: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get card effect preview for UI display
        /// Uses Execute as default action type for preview (positive action)
        /// Parallel to ConversationContent.GetCardEffect()
        /// </summary>
        protected string GetCardEffect(CardInstance card)
        {
            if (card?.PhysicalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // PROJECTION: Get effect projection using Execute as default action (positive action for preview)
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, PhysicalActionType.Execute);

            return projection.EffectDescription;
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get effect-only description (for card tooltips)
        /// Parallel to ConversationContent.GetCardEffectOnlyDescription()
        /// </summary>
        protected string GetCardEffectOnlyDescription(CardInstance card)
        {
            if (card?.PhysicalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // Use Execute as default action type for preview
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, PhysicalActionType.Execute);
            return projection.EffectDescription ?? "";
        }

    }
}
