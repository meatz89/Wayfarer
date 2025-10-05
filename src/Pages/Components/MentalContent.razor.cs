using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class MentalContentBase : ComponentBase
    {
        [CascadingParameter] public GameScreen ParentScreen { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        protected MentalSession Session => ParentScreen?.MentalSession;
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

                await ParentScreen.NavigateToScreen(ScreenMode.Location);
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

    }
}
