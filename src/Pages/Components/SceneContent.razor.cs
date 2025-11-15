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
            Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Id = {CurrentSituation.Id}");
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
            Console.WriteLine($"[SceneContent.LoadChoices] ERROR - Situation '{CurrentSituation.Id}' has empty ChoiceTemplates (soft-lock risk)");
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
                // Marker resolution deleted - all entities reference by concrete IDs now
                Dictionary<string, string> markerMap = new Dictionary<string, string>();
                requirementsMet = choiceTemplate.RequirementFormula.IsAnySatisfied(player, GameWorld, markerMap);
                requirementPaths = GetRequirementGaps(choiceTemplate.RequirementFormula, player, markerMap);

                if (!requirementsMet && requirementPaths.Count > 0)
                {
                    lockReason = string.Join(" OR ", requirementPaths.Select(p =>
                        string.Join(" AND ", p.MissingRequirements)));
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

            // Map relationship consequences (BondChanges) with current/final values
            List<BondChangeVM> bondChanges = new List<BondChangeVM>();
            if (reward?.BondChanges != null)
            {
                foreach (BondChange bondChange in reward.BondChanges)
                {
                    NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == bondChange.NpcId);
                    int currentBond = GetTotalBond(player, bondChange.NpcId);
                    int finalBond = currentBond + bondChange.Delta;

                    bondChanges.Add(new BondChangeVM
                    {
                        NpcName = npc?.Name ?? bondChange.NpcId,
                        Delta = bondChange.Delta,
                        Reason = bondChange.Reason ?? "",
                        CurrentBond = currentBond,
                        FinalBond = finalBond
                    });
                }
            }

            // Map reputation consequences (ScaleShifts) with current/final values
            List<ScaleShiftVM> scaleShifts = new List<ScaleShiftVM>();
            if (reward?.ScaleShifts != null)
            {
                foreach (ScaleShift scaleShift in reward.ScaleShifts)
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
            // LocationsToUnlock DELETED - new architecture uses query-based accessibility via GrantsLocationAccess property

            // Map scene spawns to display names
            List<string> scenesUnlocked = new List<string>();
            if (reward?.ScenesToSpawn != null)
            {
                foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
                {
                    // Scene spawning uses categorical PlacementFilter now (no context binding needed)
                    // SceneTemplate.PlacementFilter defines categories → EntityResolver finds/creates at spawn time
                    // Perfect information: Show template ID (player sees which scene will spawn)
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

                // Final values after this choice (Sir Brante-style Perfect Information)
                FinalCoins = player.Coins - coinsCost + coinsReward,
                FinalResolve = player.Resolve - resolveCost + resolveReward,
                FinalHealth = fullRecovery ? player.MaxHealth : (player.Health - healthCost + healthReward),
                FinalStamina = fullRecovery ? player.MaxStamina : (player.Stamina - staminaCost + staminaReward),
                FinalFocus = fullRecovery ? player.MaxFocus : (player.Focus - focusCost + focusReward),
                FinalHunger = fullRecovery ? 0 : (player.Hunger + hungerCost + hungerChange),

                // Affordability check - separate from requirements
                // Requirements = prerequisites (stats, relationships, items)
                // Affordability = resource availability (coins, resolve, stamina, focus, health)
                IsAffordable = player.Coins >= coinsCost &&
                              player.Resolve >= resolveCost &&
                              player.Health >= healthCost &&
                              player.Stamina >= staminaCost &&
                              player.Focus >= focusCost,

                // Current player resources (for Razor display)
                CurrentCoins = player.Coins,
                CurrentResolve = player.Resolve,
                CurrentHealth = player.Health,
                CurrentStamina = player.Stamina,
                CurrentFocus = player.Focus,
                CurrentHunger = player.Hunger,

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

    private int GetTotalBond(Player player, string npcId)
    {
        NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.NpcId == npcId);
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

    private string FormatBondGap(string npcId, int threshold, Player player)
    {
        int current = GetTotalBond(player, npcId);
        NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        string npcName = npc?.Name ?? npcId;
        return $"Bond {threshold}+ with {npcName} (now {current})";
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
            "BondStrength" => FormatBondGap(req.Context, req.Threshold, player),
            "Scale" => FormatScaleGap(req.Context, req.Threshold, player),
            "PlayerStat" => FormatPlayerStatGap(req.Context, req.Threshold, player),
            "Resolve" => $"Resolve {req.Threshold}+ (now {player.Resolve})",
            "Coins" => $"Coins {req.Threshold}+ (now {player.Coins})",
            "CompletedSituations" => $"{req.Threshold}+ Completed Situations (now {player.CompletedSituationIds.Count})",
            "Achievement" => req.Threshold > 0 ? $"Need Achievement: {req.Context}" : $"Must NOT have Achievement: {req.Context}",
            "State" => req.Threshold > 0 ? $"Need State: {req.Context}" : $"Must NOT have State: {req.Context}",
            "HasItem" => req.Threshold > 0 ? $"Need Item: {req.Context}" : $"Must NOT have Item: {req.Context}",
            _ => "Unknown requirement"
        };
    }

    private List<RequirementPathVM> GetRequirementGaps(CompoundRequirement compoundReq, Player player, Dictionary<string, string> markerMap)
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

                if (!req.IsSatisfied(player, GameWorld, markerMap))
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

            // Apply costs immediately (for both instant and challenge actions)
            player.Coins -= choiceTemplate.CostTemplate.Coins;
            player.Resolve -= choiceTemplate.CostTemplate.Resolve;
            player.Health -= choiceTemplate.CostTemplate.Health;
            player.Stamina -= choiceTemplate.CostTemplate.Stamina;
            player.Focus -= choiceTemplate.CostTemplate.Focus;
            player.Hunger += choiceTemplate.CostTemplate.Hunger;

            // Note: TimeSegments handled by RewardApplicationService (time advancement)
        }

        // TRANSITION TRACKING: Set LastChoiceId for OnChoice transitions
        CurrentSituation.LastChoiceId = choiceTemplate.Id;

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
                    RequestId = CurrentSituation.Id,
                    NPC = npc // CurrentSituation.Npc may be null if situation is location-only
                };
                GameWorld.PendingSocialContext = new SocialChallengeContext
                {
                    CompletionReward = choiceTemplate.OnSuccessReward,
                    FailureReward = choiceTemplate.OnFailureReward,
                    SituationId = CurrentSituation.Id
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Mental)
            {
                GameWorld.CurrentMentalSituationId = CurrentSituation.Id;
                GameWorld.PendingMentalContext = new MentalChallengeContext
                {
                    CompletionReward = choiceTemplate.OnSuccessReward,
                    FailureReward = choiceTemplate.OnFailureReward,
                    SituationId = CurrentSituation.Id
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Physical)
            {
                GameWorld.CurrentPhysicalSituationId = CurrentSituation.Id;
                GameWorld.PendingPhysicalContext = new PhysicalChallengeContext
                {
                    CompletionReward = choiceTemplate.OnSuccessReward,
                    FailureReward = choiceTemplate.OnFailureReward,
                    SituationId = CurrentSituation.Id
                };
            }

            // Close scene modal - challenge screen will open
            // When challenge completes, facade calls CompleteSituation → advances scene
            // GameScreen will detect scene state and reopen modal if needed
            await OnSceneEnd.InvokeAsync();
            return;
        }

        // INSTANT PATH: Apply rewards immediately
        if (choiceTemplate.RewardTemplate != null)
        {
            await RewardApplicationService.ApplyChoiceReward(choiceTemplate.RewardTemplate, CurrentSituation);
        }

        // FALLBACK PATH: Non-advancing choice (situation stays active and repeatable)
        if (choiceTemplate.PathType == ChoicePathType.Fallback)
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] FALLBACK choice - situation remains active");
            // Rewards already applied above
            // Situation NOT completed - player can retry or leave and return
            // Reload choices to show updated state (costs may have changed player resources)
            LoadChoices();
            StateHasChanged();
            return;
        }

        // ADVANCING PATH: Complete situation and advance scene
        // Scene entity owns state machine via Scene.AdvanceToNextSituation()
        // UI queries new current situation after completion
        Console.WriteLine($"[SceneContent.HandleChoiceSelected] Completing situation '{CurrentSituation.Id}'");
        await SituationCompletionHandler.CompleteSituation(CurrentSituation);

        // CONTEXT-AWARE ROUTING: Query routing decision from completed situation
        SceneRoutingDecision routingDecision = CurrentSituation.RoutingDecision;
        Console.WriteLine($"[SceneContent.HandleChoiceSelected] RoutingDecision: {routingDecision}");

        if (routingDecision == SceneRoutingDecision.ContinueInScene)
        {
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] CONTINUE IN SCENE - reloading modal");
            // Same context (location + NPC) - seamless cascade to next situation
            Situation nextSituation = Scene.CurrentSituation;

            if (nextSituation != null)
            {
                Console.WriteLine($"[SceneContent.HandleChoiceSelected] Next situation: '{nextSituation.Id}'");
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
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] EXIT TO WORLD - closing modal");
            // Different context (location or NPC changed) - player must navigate
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
