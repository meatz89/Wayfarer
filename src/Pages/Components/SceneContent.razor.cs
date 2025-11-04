using Microsoft.AspNetCore.Components;
using Wayfarer.GameState.Enums;
using Wayfarer.Subsystems.Scene;
using Wayfarer.Subsystems.Consequence;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Scene screen component for Modal Scenes (Sir Brante forced moments).
    /// Full-screen takeover showing scene narrative with 2-4 choices.
    /// Handles both Cascade (auto-advance) and Breathe (return to location) progression modes.
    /// </summary>
    public class SceneContentBase : ComponentBase
    {
        [Parameter] public ModalSceneContext Context { get; set; }
        [Parameter] public EventCallback OnSceneEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected SceneFacade SceneFacade { get; set; }
        [Inject] protected RewardApplicationService RewardApplicationService { get; set; }
        [Inject] protected SituationCompletionHandler SituationCompletionHandler { get; set; }

        protected Scene Scene { get; set; }
        protected Situation CurrentSituation { get; set; }
        protected List<ActionCardViewModel> Choices { get; set; } = new List<ActionCardViewModel>();

        protected override async Task OnParametersSetAsync()
        {
            if (Context != null && Context.IsValid)
            {
                Scene = Context.Scene;
                CurrentSituation = Context.CurrentSituation;

                // Get choices for current situation
                LoadChoices();
            }

            await base.OnParametersSetAsync();
        }

        private void LoadChoices()
        {
            if (CurrentSituation?.Template?.ChoiceTemplates == null)
                return;

            Choices.Clear();

            Player player = GameFacade.GetPlayer();

            foreach (ChoiceTemplate choiceTemplate in CurrentSituation.Template.ChoiceTemplates)
            {
                // Check requirements
                bool requirementsMet = true;
                string lockReason = "";

                // Validate RequirementFormula
                if (choiceTemplate.RequirementFormula != null && choiceTemplate.RequirementFormula.OrPaths.Count > 0)
                {
                    requirementsMet = choiceTemplate.RequirementFormula.IsAnySatisfied(player, GameWorld);
                    if (!requirementsMet)
                    {
                        lockReason = "Requirements not met";
                    }
                }

                // Validate costs (only if requirements are met)
                if (requirementsMet && choiceTemplate.CostTemplate != null)
                {
                    if (player.Resolve < choiceTemplate.CostTemplate.Resolve)
                    {
                        requirementsMet = false;
                        lockReason = $"Not enough Resolve (need {choiceTemplate.CostTemplate.Resolve}, have {player.Resolve})";
                    }
                    else if (player.Coins < choiceTemplate.CostTemplate.Coins)
                    {
                        requirementsMet = false;
                        lockReason = $"Not enough Coins (need {choiceTemplate.CostTemplate.Coins}, have {player.Coins})";
                    }
                    else if (player.Health < choiceTemplate.CostTemplate.Health)
                    {
                        requirementsMet = false;
                        lockReason = $"Not enough Health (need {choiceTemplate.CostTemplate.Health}, have {player.Health})";
                    }
                    else if (player.Stamina < choiceTemplate.CostTemplate.Stamina)
                    {
                        requirementsMet = false;
                        lockReason = $"Not enough Stamina (need {choiceTemplate.CostTemplate.Stamina}, have {player.Stamina})";
                    }
                    else if (player.Focus < choiceTemplate.CostTemplate.Focus)
                    {
                        requirementsMet = false;
                        lockReason = $"Not enough Focus (need {choiceTemplate.CostTemplate.Focus}, have {player.Focus})";
                    }
                    else if (player.Hunger + choiceTemplate.CostTemplate.Hunger > player.MaxHunger)
                    {
                        requirementsMet = false;
                        lockReason = $"Too hungry to continue (current {player.Hunger}, action adds {choiceTemplate.CostTemplate.Hunger})";
                    }
                }

                ActionCardViewModel choice = new ActionCardViewModel
                {
                    Id = choiceTemplate.Id,
                    Name = choiceTemplate.ActionTextTemplate,
                    Description = "",
                    RequirementsMet = requirementsMet,
                    LockReason = lockReason,
                    ResolveCost = choiceTemplate.CostTemplate?.Resolve ?? 0,
                    CoinsCost = choiceTemplate.CostTemplate?.Coins ?? 0,
                    TimeSegments = choiceTemplate.CostTemplate?.TimeSegments ?? 0
                };

                Choices.Add(choice);
            }
        }

        /// <summary>
        /// Handle player selecting a choice in the modal scene.
        /// Validates requirements, applies costs, executes rewards, handles progression mode.
        /// </summary>
        protected async Task HandleChoiceSelected(ActionCardViewModel choice)
        {
            if (choice == null || !choice.RequirementsMet)
                return;

            // Find the matching ChoiceTemplate
            ChoiceTemplate choiceTemplate = CurrentSituation.Template.ChoiceTemplates
                .FirstOrDefault(ct => ct.Id == choice.Id);

            if (choiceTemplate == null)
                return;

            Player player = GameFacade.GetPlayer();

            // DEFENSIVE CHECK: Re-validate requirements before execution
            if (choiceTemplate.RequirementFormula != null && choiceTemplate.RequirementFormula.OrPaths.Count > 0)
            {
                bool requirementsMet = choiceTemplate.RequirementFormula.IsAnySatisfied(player, GameWorld);
                if (!requirementsMet)
                    return; // Requirements not met - should never happen if UI is correct
            }

            // Validate costs before applying
            if (choiceTemplate.CostTemplate != null)
            {
                if (player.Resolve < choiceTemplate.CostTemplate.Resolve ||
                    player.Coins < choiceTemplate.CostTemplate.Coins ||
                    player.Health < choiceTemplate.CostTemplate.Health ||
                    player.Stamina < choiceTemplate.CostTemplate.Stamina ||
                    player.Focus < choiceTemplate.CostTemplate.Focus ||
                    player.Hunger + choiceTemplate.CostTemplate.Hunger > player.MaxHunger)
                {
                    return; // Cannot afford costs - should never happen if UI is correct
                }

                // Apply costs
                player.Coins -= choiceTemplate.CostTemplate.Coins;
                player.Resolve -= choiceTemplate.CostTemplate.Resolve;
                player.Health -= choiceTemplate.CostTemplate.Health;
                player.Stamina -= choiceTemplate.CostTemplate.Stamina;
                player.Focus -= choiceTemplate.CostTemplate.Focus;
                player.Hunger += choiceTemplate.CostTemplate.Hunger;

                // Note: TimeSegments handled by RewardApplicationService (time advancement)
            }

            // Apply choice rewards via RewardApplicationService
            if (choiceTemplate.RewardTemplate != null)
            {
                RewardApplicationService.ApplyChoiceReward(choiceTemplate.RewardTemplate, CurrentSituation);
            }

            // MULTI-SITUATION SCENE: Complete situation and advance
            // Scene entity owns state machine via Scene.AdvanceToNextSituation()
            // UI queries new current situation after completion
            SituationCompletionHandler.CompleteSituation(CurrentSituation);

            // Re-query current situation after scene advancement
            string newSituationId = Scene.CurrentSituationId;
            Situation newSituation = GameWorld.Situations.FirstOrDefault(s => s.Id == newSituationId);

            if (newSituation != null && newSituation.Id != CurrentSituation.Id)
            {
                // Scene advanced to new situation - reload and display
                CurrentSituation = newSituation;
                LoadChoices();
                StateHasChanged();
            }
            else
            {
                // Scene complete - return to location
                await OnSceneEnd.InvokeAsync();
            }
        }
    }
}
