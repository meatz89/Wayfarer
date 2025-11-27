using Microsoft.AspNetCore.Components;

/// <summary>
/// Scene screen component for Modal Scenes (Sir Brante forced moments).
/// Full-screen takeover showing scene narrative with 2-4 choices.
/// Handles both Cascade (continue in scene) and Breathe (return to location) progression modes.
/// </summary>
public class SceneContentBase : ComponentBase
{
    [Parameter] public SceneContext Context { get; set; }
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
        Console.WriteLine($"[SceneContent.LoadChoices] ENTER - CurrentSituation null? {CurrentSituation == null}");
        if (CurrentSituation != null)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Name = {CurrentSituation.Name}");
            Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Template null? {CurrentSituation.Template == null}");
            if (CurrentSituation.Template != null)
            {
                Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Template.ChoiceTemplates null? {CurrentSituation.Template.ChoiceTemplates == null}");
                if (CurrentSituation.Template.ChoiceTemplates != null)
                {
                    Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Template.ChoiceTemplates.Count = {CurrentSituation.Template.ChoiceTemplates.Count}");
                }
            }
        }

        if (CurrentSituation?.Template?.ChoiceTemplates == null)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] EARLY RETURN - CurrentSituation/Template/ChoiceTemplates is null");
            return;
        }

        if (CurrentSituation.Template.ChoiceTemplates.Count == 0)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] ERROR - Situation '{CurrentSituation.Name}' has empty ChoiceTemplates (soft-lock risk)");
            return;
        }

        Choices.Clear();

        Player player = GameFacade.GetPlayer();

        foreach (ChoiceTemplate choiceTemplate in CurrentSituation.Template.ChoiceTemplates)
        {
            // Check requirements
            bool requirementsMet = true;
            string lockReason = "";

            // Validate RequirementFormula and build requirement gaps
            List<RequirementPathVM> requirementPaths = new List<RequirementPathVM>();
            if (choiceTemplate.RequirementFormula != null && choiceTemplate.RequirementFormula.OrPaths.Count > 0)
            {
                requirementsMet = choiceTemplate.RequirementFormula.IsAnySatisfied(player, GameWorld);
                requirementPaths = GetRequirementGaps(choiceTemplate.RequirementFormula, player);

                if (!requirementsMet && requirementPaths.Count > 0)
                {
                    lockReason = string.Join(" OR ", requirementPaths.Select(p =>
                        string.Join(" AND ", p.MissingRequirements)));
                }
            }

            // Map ALL costs/rewards from unified Consequence (Perfect Information)
            // Consequence uses NEGATIVE VALUES for costs, POSITIVE VALUES for rewards
            Consequence consequence = choiceTemplate.Consequence ?? Consequence.None();

            // Extract costs (negative values, convert to positive for display)
            int resolveCost = consequence.Resolve < 0 ? -consequence.Resolve : 0;
            int coinsCost = consequence.Coins < 0 ? -consequence.Coins : 0;
            int healthCost = consequence.Health < 0 ? -consequence.Health : 0;
            int staminaCost = consequence.Stamina < 0 ? -consequence.Stamina : 0;
            int focusCost = consequence.Focus < 0 ? -consequence.Focus : 0;
            int hungerCost = consequence.Hunger > 0 ? consequence.Hunger : 0;
            int timeSegments = consequence.TimeSegments;

            // Validate costs (only if requirements are met)
            if (requirementsMet && consequence != null)
            {
                if (player.Resolve < resolveCost)
                {
                    requirementsMet = false;
                    lockReason = $"Not enough Resolve (need {resolveCost}, have {player.Resolve})";
                }
                else if (player.Coins < coinsCost)
                {
                    requirementsMet = false;
                    lockReason = $"Not enough Coins (need {coinsCost}, have {player.Coins})";
                }
                else if (player.Health < healthCost)
                {
                    requirementsMet = false;
                    lockReason = $"Not enough Health (need {healthCost}, have {player.Health})";
                }
                else if (player.Stamina < staminaCost)
                {
                    requirementsMet = false;
                    lockReason = $"Not enough Stamina (need {staminaCost}, have {player.Stamina})";
                }
                else if (player.Focus < focusCost)
                {
                    requirementsMet = false;
                    lockReason = $"Not enough Focus (need {focusCost}, have {player.Focus})";
                }
                else if (player.Hunger + hungerCost > player.MaxHunger)
                {
                    requirementsMet = false;
                    lockReason = $"Too hungry to continue (current {player.Hunger}, action adds {hungerCost})";
                }
            }

            // Extract rewards (positive values)
            int coinsReward = consequence.Coins > 0 ? consequence.Coins : 0;
            int resolveReward = consequence.Resolve > 0 ? consequence.Resolve : 0;
            int healthReward = consequence.Health > 0 ? consequence.Health : 0;
            int staminaReward = consequence.Stamina > 0 ? consequence.Stamina : 0;
            int focusReward = consequence.Focus > 0 ? consequence.Focus : 0;
            int hungerChange = consequence.Hunger < 0 ? consequence.Hunger : 0; // Negative hunger is good (less hungry)
            bool fullRecovery = consequence.FullRecovery;

            // Map Five Stats rewards (Sir Brante pattern: direct grants)
            int insightReward = consequence.Insight;
            int rapportReward = consequence.Rapport;
            int authorityReward = consequence.Authority;
            int diplomacyReward = consequence.Diplomacy;
            int cunningReward = consequence.Cunning;

            // Map relationship consequences (BondChanges) with current/final values
            List<BondChangeVM> bondChanges = new List<BondChangeVM>();
            if (consequence.BondChanges != null)
            {
                foreach (BondChange bondChange in consequence.BondChanges)
                {
                    // Direct object reference, NO ID lookup
                    NPC npc = bondChange.Npc;
                    if (npc == null)
                    {
                        Console.WriteLine("[SceneContent.LoadChoices] WARNING: BondChange has null NPC reference");
                        continue;
                    }

                    int currentBond = GetTotalBond(player, npc);
                    int finalBond = currentBond + bondChange.Delta;

                    bondChanges.Add(new BondChangeVM
                    {
                        NpcName = npc.Name,
                        Delta = bondChange.Delta,
                        Reason = bondChange.Reason ?? "",
                        CurrentBond = currentBond,
                        FinalBond = finalBond
                    });
                }
            }

            // Map reputation consequences (ScaleShifts) with current/final values
            List<ScaleShiftVM> scaleShifts = new List<ScaleShiftVM>();
            if (consequence.ScaleShifts != null)
            {
                foreach (ScaleShift scaleShift in consequence.ScaleShifts)
                {
                    string scaleName = scaleShift.ScaleType.ToString();
                    int currentScale = GetScaleValue(player, scaleName);
                    int finalScale = currentScale + scaleShift.Delta;

                    scaleShifts.Add(new ScaleShiftVM
                    {
                        ScaleName = scaleName,
                        Delta = scaleShift.Delta,
                        Reason = scaleShift.Reason ?? "",
                        CurrentScale = currentScale,
                        FinalScale = finalScale
                    });
                }
            }

            // Map condition consequences (StateApplications)
            List<StateApplicationVM> stateApplications = new List<StateApplicationVM>();
            if (consequence.StateApplications != null)
            {
                foreach (StateApplication stateApp in consequence.StateApplications)
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
            // HIGHLANDER: consequence uses Achievement and Item objects, extract Names for display
            List<string> achievementsGranted = consequence.Achievements?.Select(a => a.Name).ToList() ?? new List<string>();
            List<string> itemsGranted = consequence.Items?.Select(i => i.Name).ToList() ?? new List<string>();
            // LocationsToUnlock DELETED - new architecture uses dual-model accessibility (situation presence grants access)

            // Map scene spawns to display names
            List<string> scenesUnlocked = new List<string>();
            if (consequence.ScenesToSpawn != null)
            {
                foreach (SceneSpawnReward sceneSpawn in consequence.ScenesToSpawn)
                {
                    // Scene spawning uses categorical PlacementFilter now (no context binding needed)
                    // SceneTemplate.PlacementFilter defines categories → EntityResolver finds/creates at spawn time
                    // Perfect information: Show what scene will spawn
                    if (sceneSpawn.SpawnNextMainStoryScene)
                        scenesUnlocked.Add("Next Main Story Scene");
                    else if (sceneSpawn.Template != null)
                        scenesUnlocked.Add(sceneSpawn.Template.Id);
                }
            }

            ActionCardViewModel choice = new ActionCardViewModel
            {
                SourceTemplate = choiceTemplate, // HIGHLANDER: Store template reference for execution
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

                // Five Stats rewards
                InsightReward = insightReward,
                RapportReward = rapportReward,
                AuthorityReward = authorityReward,
                DiplomacyReward = diplomacyReward,
                CunningReward = cunningReward,

                // Final values after this choice (Sir Brante-style Perfect Information)
                FinalCoins = player.Coins - coinsCost + coinsReward,
                FinalResolve = player.Resolve - resolveCost + resolveReward,
                FinalHealth = fullRecovery ? player.MaxHealth : (player.Health - healthCost + healthReward),
                FinalStamina = fullRecovery ? player.MaxStamina : (player.Stamina - staminaCost + staminaReward),
                FinalFocus = fullRecovery ? player.MaxFocus : (player.Focus - focusCost + focusReward),
                FinalHunger = fullRecovery ? 0 : (player.Hunger + hungerCost + hungerChange),

                // Final stat values after this choice
                FinalInsight = player.Insight + insightReward,
                FinalRapport = player.Rapport + rapportReward,
                FinalAuthority = player.Authority + authorityReward,
                FinalDiplomacy = player.Diplomacy + diplomacyReward,
                FinalCunning = player.Cunning + cunningReward,

                // Affordability check - separate from requirements
                // Requirements = prerequisites (stats, relationships, items)
                // Affordability = resource availability (coins, resolve, stamina, focus, health)
                IsAffordable = consequence.IsAffordable(player),
                HasAnyConsequences = consequence.HasAnyEffect(),

                // Current player resources (for Razor display)
                CurrentCoins = player.Coins,
                CurrentResolve = player.Resolve,
                CurrentHealth = player.Health,
                CurrentStamina = player.Stamina,
                CurrentFocus = player.Focus,
                CurrentHunger = player.Hunger,

                // Current player stats (for Sir Brante display)
                CurrentInsight = player.Insight,
                CurrentRapport = player.Rapport,
                CurrentAuthority = player.Authority,
                CurrentDiplomacy = player.Diplomacy,
                CurrentCunning = player.Cunning,

                // All consequences
                BondChanges = bondChanges,
                ScaleShifts = scaleShifts,
                StateApplications = stateApplications,

                // All progression unlocks
                AchievementsGranted = achievementsGranted,
                ItemsGranted = itemsGranted,
                // LocationsUnlocked DELETED - new architecture uses query-based accessibility
                ScenesUnlocked = scenesUnlocked,

                // Requirement gaps (Perfect Information for locked choices)
                RequirementPaths = requirementPaths
            };

            Choices.Add(choice);
        }
    }

    private int GetTotalBond(Player player, NPC npc)
    {
        // HIGHLANDER: Compare NPC objects directly, not string IDs
        NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.Npc == npc);
        if (entry == null) return 0;
        return entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
    }

    private int GetScaleValue(Player player, string scaleName)
    {
        return scaleName switch
        {
            "Morality" => player.Scales.Morality,
            "Lawfulness" => player.Scales.Lawfulness,
            "Method" => player.Scales.Method,
            "Caution" => player.Scales.Caution,
            "Transparency" => player.Scales.Transparency,
            "Fame" => player.Scales.Fame,
            _ => 0
        };
    }

    private int GetPlayerStatLevel(Player player, string statName)
    {
        if (!Enum.TryParse<PlayerStatType>(statName, ignoreCase: true, out PlayerStatType statType))
            return 0;
        return statType switch
        {
            PlayerStatType.Insight => player.Insight,
            PlayerStatType.Rapport => player.Rapport,
            PlayerStatType.Authority => player.Authority,
            PlayerStatType.Diplomacy => player.Diplomacy,
            PlayerStatType.Cunning => player.Cunning,
            _ => 0
        };
    }

    private string FormatBondGap(NPC npc, int threshold, Player player)
    {
        int current = GetTotalBond(player, npc);
        return $"Bond {threshold}+ with {npc.Name} (now {current})";
    }

    private string FormatScaleGap(string scaleName, int threshold, Player player)
    {
        int current = GetScaleValue(player, scaleName);
        if (threshold >= 0)
            return $"{scaleName} {threshold}+ (now {current})";
        else
            return $"{scaleName} {threshold}- (now {current})";
    }

    private string FormatPlayerStatGap(string statName, int threshold, Player player)
    {
        int current = GetPlayerStatLevel(player, statName);
        return $"{statName} {threshold}+ (now {current})";
    }

    private string FormatRequirementGap(NumericRequirement req, Player player)
    {
        return req.Type switch
        {
            "BondStrength" => FormatBondGapFromId(req.Context, req.Threshold, player),
            "Scale" => FormatScaleGap(req.Context, req.Threshold, player),
            "PlayerStat" => FormatPlayerStatGap(req.Context, req.Threshold, player),
            "Resolve" => $"Resolve {req.Threshold}+ (now {player.Resolve})",
            "Coins" => $"Coins {req.Threshold}+ (now {player.Coins})",
            "CompletedSituations" => $"{req.Threshold}+ Completed Situations (now {player.CompletedSituations.Count})",
            "Achievement" => req.Threshold > 0 ? $"Need Achievement: {req.Context}" : $"Must NOT have Achievement: {req.Context}",
            "State" => req.Threshold > 0 ? $"Need State: {req.Context}" : $"Must NOT have State: {req.Context}",
            "HasItem" => req.Threshold > 0 ? $"Need Item: {req.Context}" : $"Must NOT have Item: {req.Context}",
            _ => "Unknown requirement"
        };
    }

    private string FormatBondGapFromId(string npcId, int threshold, Player player)
    {
        NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.Name == npcId);
        if (npc == null)
        {
            return $"Bond {threshold}+ with {npcId} (NPC not found)";
        }
        return FormatBondGap(npc, threshold, player);
    }

    private List<RequirementPathVM> GetRequirementGaps(CompoundRequirement compoundReq, Player player)
    {
        List<RequirementPathVM> paths = new List<RequirementPathVM>();

        if (compoundReq == null || compoundReq.OrPaths.Count == 0)
            return paths;

        foreach (OrPath orPath in compoundReq.OrPaths)
        {
            List<string> requirements = new List<string>();
            List<string> missingRequirements = new List<string>();
            bool pathSatisfied = true;

            foreach (NumericRequirement req in orPath.NumericRequirements)
            {
                string formattedReq = FormatRequirementGap(req, player);
                requirements.Add(formattedReq);

                if (!req.IsSatisfied(player, GameWorld))
                {
                    missingRequirements.Add(formattedReq);
                    pathSatisfied = false;
                }
            }

            paths.Add(new RequirementPathVM
            {
                Requirements = requirements,
                PathSatisfied = pathSatisfied,
                MissingRequirements = missingRequirements
            });
        }

        return paths;
    }

    /// <summary>
    /// Handle player selecting a choice in the modal scene.
    /// Validates requirements, applies costs, executes rewards, handles progression mode.
    /// </summary>
    protected async Task HandleChoiceSelected(ActionCardViewModel choice)
    {
        if (choice == null || !choice.RequirementsMet || !choice.IsAffordable)
            return;

        // HIGHLANDER: Use direct object reference from ViewModel
        ChoiceTemplate choiceTemplate = choice.SourceTemplate;

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
        // Consequence uses NEGATIVE VALUES for costs: Coins = -5 means pay 5 coins
        if (choiceTemplate.Consequence != null)
        {
            int resolveCost = choiceTemplate.Consequence.Resolve < 0 ? -choiceTemplate.Consequence.Resolve : 0;
            int coinsCost = choiceTemplate.Consequence.Coins < 0 ? -choiceTemplate.Consequence.Coins : 0;
            int healthCost = choiceTemplate.Consequence.Health < 0 ? -choiceTemplate.Consequence.Health : 0;
            int staminaCost = choiceTemplate.Consequence.Stamina < 0 ? -choiceTemplate.Consequence.Stamina : 0;
            int focusCost = choiceTemplate.Consequence.Focus < 0 ? -choiceTemplate.Consequence.Focus : 0;
            int hungerCost = choiceTemplate.Consequence.Hunger > 0 ? choiceTemplate.Consequence.Hunger : 0;

            if (player.Resolve < resolveCost ||
                player.Coins < coinsCost ||
                player.Health < healthCost ||
                player.Stamina < staminaCost ||
                player.Focus < focusCost ||
                player.Hunger + hungerCost > player.MaxHunger)
            {
                return; // Cannot afford costs - should never happen if UI is correct
            }

            // Apply costs immediately (for both instant and challenge actions)
            player.Coins -= coinsCost;
            player.Resolve -= resolveCost;
            player.Health -= healthCost;
            player.Stamina -= staminaCost;
            player.Focus -= focusCost;
            player.Hunger += hungerCost;

            // Note: TimeSegments handled by RewardApplicationService (time advancement)
        }

        // TRANSITION TRACKING: Set LastChoice for OnChoice transitions
        CurrentSituation.LastChoice = choiceTemplate;

        // PROCEDURAL CONTENT TRACING: Record choice execution for ALL paths (instant + challenge)
        // UNIFIED ARCHITECTURE: Choice is a choice, whether instant or challenge
        ChoiceExecutionNode choiceNode = null;
        if (GameWorld.ProceduralTracer != null && GameWorld.ProceduralTracer.IsEnabled)
        {
            SituationSpawnNode situationNode = GameWorld.ProceduralTracer.GetNodeForSituation(CurrentSituation);
            choiceNode = GameWorld.ProceduralTracer.RecordChoiceExecution(
                choiceTemplate,
                situationNode,
                choiceTemplate.ActionTextTemplate,
                playerMetRequirements: true // Reached this point only if requirements met
            );
        }

        // ROUTE BY ACTION TYPE: StartChallenge vs Instant
        if (choiceTemplate.ActionType == ChoiceActionType.StartChallenge)
        {
            // CHALLENGE PATH: Route to tactical challenge subsystem
            // Challenge facades will call SituationCompletionHandler.CompleteSituation() when complete
            // OnSuccessReward applied by challenge system, NOT here
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] Routing to {choiceTemplate.ChallengeType} challenge");

            // Store challenge context for resumption
            if (choiceTemplate.ChallengeType == TacticalSystemType.Social)
            {
                // HIERARCHICAL PLACEMENT: Get NPC from CurrentSituation (situation owns placement)
                NPC npc = CurrentSituation?.Npc;
                GameWorld.CurrentSocialSession = new SocialSession
                {
                    NPC = npc // CurrentSituation.Npc may be null if situation is location-only
                };
                GameWorld.PendingSocialContext = new SocialChallengeContext
                {
                    Situation = CurrentSituation, // Object reference, NO ID
                    CompletionReward = choiceTemplate.OnSuccessConsequence,
                    FailureReward = choiceTemplate.OnFailureConsequence,
                    ChoiceExecution = choiceNode // Store for later use
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Mental)
            {
                GameWorld.PendingMentalContext = new MentalChallengeContext
                {
                    Situation = CurrentSituation, // Object reference, NO ID
                    CompletionReward = choiceTemplate.OnSuccessConsequence,
                    FailureReward = choiceTemplate.OnFailureConsequence,
                    ChoiceExecution = choiceNode // Store for later use
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Physical)
            {
                GameWorld.PendingPhysicalContext = new PhysicalChallengeContext
                {
                    Situation = CurrentSituation, // Object reference, NO ID
                    CompletionReward = choiceTemplate.OnSuccessConsequence,
                    FailureReward = choiceTemplate.OnFailureConsequence,
                    ChoiceExecution = choiceNode // Store for later use
                };
            }

            // Close scene modal - challenge screen will open
            // When challenge completes, facade calls CompleteSituation → advances scene
            // GameScreen will detect scene state and reopen modal if needed
            await OnSceneEnd.InvokeAsync();
            return;
        }

        // FALLBACK PATH: Player declines - no rewards, exit to location, situation remains available
        if (choiceTemplate.PathType == ChoicePathType.Fallback)
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] FALLBACK choice - exiting to location, no rewards applied");
            await OnSceneEnd.InvokeAsync();
            return;
        }

        // INSTANT PATH: Apply consequences immediately
        if (choiceTemplate.Consequence != null)
        {
            // PROCEDURAL CONTENT TRACING: Push context for instant consequence application
            if (GameWorld.ProceduralTracer != null && GameWorld.ProceduralTracer.IsEnabled && choiceNode != null)
            {
                GameWorld.ProceduralTracer.PushChoiceContext(choiceNode);
            }

            try
            {
                await RewardApplicationService.ApplyConsequence(choiceTemplate.Consequence, CurrentSituation);
            }
            finally
            {
                // ALWAYS pop context (even on exception)
                if (GameWorld.ProceduralTracer != null && GameWorld.ProceduralTracer.IsEnabled && choiceNode != null)
                {
                    GameWorld.ProceduralTracer.PopChoiceContext();
                }
            }
        }

        // ADVANCING PATH: Complete situation and advance scene
        // Scene entity owns state machine via Scene.AdvanceToNextSituation()
        // UI queries new current situation after completion
        Console.WriteLine($"[SceneContent.HandleChoiceSelected] Completing situation '{CurrentSituation.Name}'");
        await SituationCompletionHandler.CompleteSituation(CurrentSituation);

        // CONTEXT-AWARE ROUTING: Query routing decision from completed situation
        SceneRoutingDecision routingDecision = CurrentSituation.RoutingDecision;
        Console.WriteLine($"[SceneContent.HandleChoiceSelected] RoutingDecision: {routingDecision}, ProgressionMode: {Scene.ProgressionMode}");

        // ROUTING DECISION IS AUTHORITATIVE: ExitToWorld = context changed, player must navigate
        // ProgressionMode only affects UI pacing (how fast same-context transitions appear)
        // ExitToWorld ALWAYS exits - it's a hard constraint, not a UI preference
        bool shouldContinueInScene = routingDecision == SceneRoutingDecision.ContinueInScene;

        if (shouldContinueInScene)
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] CONTINUE IN SCENE - reloading modal (Cascade={Scene.ProgressionMode == ProgressionMode.Cascade})");
            // Same context OR Cascade mode - seamless cascade to next situation
            Situation nextSituation = Scene.CurrentSituation;

            if (nextSituation != null)
            {
                Console.WriteLine($"[SceneContent.HandleChoiceSelected] Next situation: '{nextSituation.Name}'");
                // Reload modal with next situation - no exit to world
                // Situations are fully instantiated during FinalizeScene, no on-demand instantiation needed
                CurrentSituation = nextSituation;
                LoadChoices();
                StateHasChanged();
            }
            else
            {
                // Safety fallback - scene should have marked complete
                Console.WriteLine($"[SceneContent.HandleChoiceSelected] Next situation is NULL - closing modal");
                await OnSceneEnd.InvokeAsync();
            }
        }
        else if (routingDecision == SceneRoutingDecision.ExitToWorld)
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] EXIT TO WORLD (Breathe mode) - closing modal");
            // Different context AND Breathe mode - player must navigate
            // Scene persists with updated CurrentSituationId
            // Will resume when player navigates to required context
            await OnSceneEnd.InvokeAsync();
        }
        else // SceneRoutingDecision.SceneComplete
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] SCENE COMPLETE - closing modal");
            // Scene complete - no more situations
            await OnSceneEnd.InvokeAsync();
        }
    }
}
