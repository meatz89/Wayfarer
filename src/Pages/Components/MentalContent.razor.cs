using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class MentalContentBase : ComponentBase
    {
        [Parameter] public MentalChallengeContext Context { get; set; }
        [Parameter] public EventCallback OnChallengeEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The MentalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// Parallel to CategoricalEffectResolver in Conversation system.
        /// </summary>
        [Inject] protected MentalEffectResolver EffectResolver { get; set; }

        protected MentalSession Session => Context?.Session;
        protected List<CardInstance> Hand => GameFacade?.IsMentalSessionActive() == true
            ? GameFacade.GetMentalFacade().GetHand()
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

        protected async Task ExecuteObserve()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var result = await GameFacade.ExecuteObserve(SelectedCard);

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in ExecuteObserve: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteAct()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var result = await GameFacade.ExecuteAct(SelectedCard);

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in ExecuteAct: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task EndInvestigation()
        {
            if (IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var outcome = GameFacade.EndMentalSession();

                if (outcome != null)
                {
                    LastNarrative = outcome.Success
                        ? $"Investigation complete! Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}"
                        : $"Investigation incomplete. Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}";
                }

                await OnChallengeEnd.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in EndInvestigation: {ex.Message}");
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
        /// Uses Act as default action type for preview (positive action)
        /// Parallel to ConversationContent.GetCardEffect()
        /// </summary>
        protected string GetCardEffect(CardInstance card)
        {
            if (card?.MentalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // PROJECTION: Get effect projection using Act as default action (positive action for preview)
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, MentalActionType.Act);

            return projection.EffectDescription;
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get effect-only description (for card tooltips)
        /// Parallel to ConversationContent.GetCardEffectOnlyDescription()
        /// </summary>
        protected string GetCardEffectOnlyDescription(CardInstance card)
        {
            if (card?.MentalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // Use Act as default action type for preview
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, MentalActionType.Act);
            return projection.EffectDescription ?? "";
        }

    }
}
