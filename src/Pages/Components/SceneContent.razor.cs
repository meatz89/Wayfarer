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

                // Map ALL costs (Perfect Information)
                int resolveCost = choiceTemplate.CostTemplate?.Resolve ?? 0;
                int coinsCost = choiceTemplate.CostTemplate?.Coins ?? 0;
                int timeSegments = choiceTemplate.CostTemplate?.TimeSegments ?? 0;
                int healthCost = choiceTemplate.CostTemplate?.Health ?? 0;
                int staminaCost = choiceTemplate.CostTemplate?.Stamina ?? 0;
                int focusCost = choiceTemplate.CostTemplate?.Focus ?? 0;
                int hungerCost = choiceTemplate.CostTemplate?.Hunger ?? 0;

                // Map ALL rewards (Perfect Information)
                ChoiceReward reward = choiceTemplate.RewardTemplate;
                int coinsReward = reward?.Coins ?? 0;
                int resolveReward = reward?.Resolve ?? 0;
                int healthReward = reward?.Health ?? 0;
                int staminaReward = reward?.Stamina ?? 0;
                int focusReward = reward?.Focus ?? 0;
                int hungerChange = reward?.Hunger ?? 0;
                bool fullRecovery = reward?.FullRecovery ?? false;

                // Map relationship consequences (BondChanges)
                List<BondChangeVM> bondChanges = new List<BondChangeVM>();
                if (reward?.BondChanges != null)
                {
                    foreach (BondChange bondChange in reward.BondChanges)
                    {
                        NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == bondChange.NpcId);
                        bondChanges.Add(new BondChangeVM
                        {
                            NpcName = npc?.Name ?? bondChange.NpcId,
                            Delta = bondChange.Delta,
                            Reason = bondChange.Reason ?? ""
                        });
                    }
                }

                // Map reputation consequences (ScaleShifts)
                List<ScaleShiftVM> scaleShifts = new List<ScaleShiftVM>();
                if (reward?.ScaleShifts != null)
                {
                    foreach (ScaleShift scaleShift in reward.ScaleShifts)
                    {
                        scaleShifts.Add(new ScaleShiftVM
                        {
                            ScaleName = scaleShift.ScaleType.ToString(),
                            Delta = scaleShift.Delta,
                            Reason = scaleShift.Reason ?? ""
                        });
                    }
                }

                // Map condition consequences (StateApplications)
                List<StateApplicationVM> stateApplications = new List<StateApplicationVM>();
                if (reward?.StateApplications != null)
                {
                    foreach (StateApplication stateApp in reward.StateApplications)
                    {
                        stateApplications.Add(new StateApplicationVM
                        {
                            StateName = stateApp.StateType.ToString(),
                            Apply = stateApp.Apply,
                            Reason = stateApp.Reason ?? ""
                        });
                    }
                }

                // Map progression unlocks
                List<string> achievementsGranted = reward?.AchievementIds ?? new List<string>();
                List<string> itemsGranted = reward?.ItemIds ?? new List<string>();
                List<string> locationsUnlocked = reward?.LocationsToUnlock ?? new List<string>();

                // Map scene spawns to display names
                List<string> scenesUnlocked = new List<string>();
                if (reward?.ScenesToSpawn != null)
                {
                    foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
                    {
                        scenesUnlocked.Add(sceneSpawn.SceneTemplateId);
                    }
                }

                ActionCardViewModel choice = new ActionCardViewModel
                {
                    Id = choiceTemplate.Id,
                    Name = choiceTemplate.ActionTextTemplate,
                    Description = "",
                    RequirementsMet = requirementsMet,
                    LockReason = lockReason,

                    // All costs
                    ResolveCost = resolveCost,
                    CoinsCost = coinsCost,
                    TimeSegments = timeSegments,
                    HealthCost = healthCost,
                    StaminaCost = staminaCost,
                    FocusCost = focusCost,
                    HungerCost = hungerCost,

                    // All rewards
                    CoinsReward = coinsReward,
                    ResolveReward = resolveReward,
                    HealthReward = healthReward,
                    StaminaReward = staminaReward,
                    FocusReward = focusReward,
                    HungerChange = hungerChange,
                    FullRecovery = fullRecovery,

                    // All consequences
                    BondChanges = bondChanges,
                    ScaleShifts = scaleShifts,
                    StateApplications = stateApplications,

                    // All progression unlocks
                    AchievementsGranted = achievementsGranted,
                    ItemsGranted = itemsGranted,
                    LocationsUnlocked = locationsUnlocked,
                    ScenesUnlocked = scenesUnlocked
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
