using Microsoft.AspNetCore.Components;

/// <summary>
/// Scene screen component for Modal Scenes (Sir Brante forced moments).
/// Full-screen takeover showing scene narrative with 2-4 choices.
/// Handles both Cascade (continue in scene) and Breathe (return to location) progression modes.
/// </summary>
public class SceneContentBase : ComponentBase, IDisposable
{
    [Parameter] public SceneContext Context { get; set; }
    [Parameter] public EventCallback OnSceneEnd { get; set; }
    [CascadingParameter] public GameScreenBase GameScreen { get; set; }

    [Inject] protected GameOrchestrator GameOrchestrator { get; set; }
    [Inject] protected GameWorld GameWorld { get; set; }
    [Inject] protected SceneFacade SceneFacade { get; set; }
    [Inject] protected SituationFacade SituationFacade { get; set; }
    [Inject] protected RewardApplicationService RewardApplicationService { get; set; }
    [Inject] protected SituationCompletionHandler SituationCompletionHandler { get; set; }
    [Inject] protected NarrativeStreamingService NarrativeStreamingService { get; set; }
    [Inject] protected ProceduralContentTracer ProceduralTracer { get; set; }

    protected Scene Scene { get; set; }
    protected Situation CurrentSituation { get; set; }
    protected List<ActionCardViewModel> Choices { get; set; } = new List<ActionCardViewModel>();

    // Typewriter streaming state
    protected string StreamedDescription { get; set; } = "";
    protected bool IsStreamingComplete { get; set; } = false;
    private CancellationTokenSource _streamingCts;

    // PROGRESSIVE LOADING: Separate states for description and choices
    protected bool IsActivatingSituation { get; set; } = false;
    protected bool IsGeneratingChoices { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        if (Context != null && Context.IsValid)
        {
            Scene = Context.Scene;
            CurrentSituation = Context.CurrentSituation;

            // PROGRESSIVE LOADING: Phase 1 - Get description immediately
            await ActivateSituationDescriptionAsync();

            // Start typewriter NOW - player has something to read while choices generate
            await StartDescriptionStreaming();

            // PROGRESSIVE LOADING: Phase 2 - Generate choices in background (fire-and-forget)
            _ = GenerateChoicesInBackgroundAsync();
        }

        await base.OnParametersSetAsync();
    }

    /// <summary>
    /// PROGRESSIVE LOADING - PHASE 1: Generate situation description immediately.
    /// Player sees description with typewriter effect while choices load in background.
    /// </summary>
    private async Task ActivateSituationDescriptionAsync()
    {
        if (CurrentSituation == null)
            return;

        // Show loading state
        IsActivatingSituation = true;
        Choices.Clear();
        StateHasChanged();

        try
        {
            Console.WriteLine($"[SceneContent] Activating situation description '{CurrentSituation.Name}'");
            await SituationFacade.ActivateSituationDescriptionAsync(CurrentSituation);
            Console.WriteLine($"[SceneContent] Situation description ready for '{CurrentSituation.Name}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SceneContent] ERROR activating situation description: {ex.Message}");
        }
        finally
        {
            IsActivatingSituation = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// PROGRESSIVE LOADING - PHASE 2: Generate choice labels in background.
    /// Runs AFTER description shown - player reads description while this generates.
    /// Fire-and-forget pattern with UI update when complete.
    /// </summary>
    private async Task GenerateChoicesInBackgroundAsync()
    {
        if (CurrentSituation == null)
            return;

        // Show choices loading state
        IsGeneratingChoices = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            Console.WriteLine($"[SceneContent] Generating choice labels for '{CurrentSituation.Name}' in background");
            await SituationFacade.GenerateChoiceLabelsAsync(CurrentSituation);
            Console.WriteLine($"[SceneContent] Choice labels ready for '{CurrentSituation.Name}' - {CurrentSituation.Choices?.Count ?? 0} choices");

            // Load choices into UI
            LoadChoices();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SceneContent] ERROR generating choice labels: {ex.Message}");
            // Fallback: Load choices with existing/fallback labels
            LoadChoices();
        }
        finally
        {
            IsGeneratingChoices = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task StartDescriptionStreaming()
    {
        // Cancel any existing streaming
        _streamingCts?.Cancel();
        _streamingCts = new CancellationTokenSource();

        // Reset streaming state
        StreamedDescription = "";
        IsStreamingComplete = false;
        StateHasChanged();

        if (string.IsNullOrWhiteSpace(CurrentSituation?.Description))
        {
            IsStreamingComplete = true;
            return;
        }

        // Stream the description with typewriter effect
        await foreach (NarrativeChunk chunk in NarrativeStreamingService.StreamNarrativeAsync(
            CurrentSituation.Description,
            _streamingCts.Token))
        {
            if (_streamingCts.Token.IsCancellationRequested)
                break;

            StreamedDescription += (StreamedDescription.Length > 0 ? " " : "") + chunk.Text;
            IsStreamingComplete = chunk.IsComplete;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        _streamingCts?.Cancel();
        _streamingCts?.Dispose();
    }

    private void LoadChoices()
    {
        Console.WriteLine($"[SceneContent.LoadChoices] ENTER - CurrentSituation null? {CurrentSituation == null}");
        if (CurrentSituation != null)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Name = {CurrentSituation.Name}");
            Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Choices null? {CurrentSituation.Choices == null}");
            if (CurrentSituation.Choices != null)
            {
                Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Choices.Count = {CurrentSituation.Choices.Count}");
            }
        }

        if (CurrentSituation?.Choices == null)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] EARLY RETURN - CurrentSituation/Choices is null");
            return;
        }

        if (CurrentSituation.Choices.Count == 0)
        {
            Console.WriteLine($"[SceneContent.LoadChoices] ERROR - Situation '{CurrentSituation.Name}' has empty Choices (soft-lock risk)");
            return;
        }

        Choices.Clear();

        Player player = GameOrchestrator.GetPlayer();

        // Template/Instance pattern: Choice instances have pre-scaled values from Pass 2B
        // NO runtime scaling needed - values calculated once at activation time
        // Perfect Information: Player sees AND receives pre-calculated costs (arc42 §8.26)

        foreach (Choice choice in CurrentSituation.Choices)
        {
            // TEMPLATE/INSTANCE: Use pre-scaled values from Choice entity (calculated at activation)
            // ScaledRequirement and ScaledConsequence used for BOTH display AND execution
            ChoiceTemplate choiceTemplate = choice.Template;
            CompoundRequirement scaledRequirement = choice.ScaledRequirement;
            Consequence scaledConsequence = choice.ScaledConsequence;

            // Check requirements using SCALED values (Perfect Information compliance)
            bool requirementsMet = true;
            string lockReason = "";

            // Validate RequirementFormula and build requirement gaps
            List<RequirementPathVM> requirementPaths = new List<RequirementPathVM>();
            if (scaledRequirement != null && scaledRequirement.OrPaths.Count > 0)
            {
                requirementsMet = scaledRequirement.IsAnySatisfied(player, GameWorld);
                requirementPaths = GetRequirementGaps(scaledRequirement, player);

                if (!requirementsMet && requirementPaths.Count > 0)
                {
                    lockReason = string.Join(" OR ", requirementPaths.Select(p =>
                        string.Join(" AND ", p.MissingRequirements)));
                }
            }

            // Map ALL costs/rewards from SCALED Consequence (Perfect Information)
            // Consequence uses NEGATIVE VALUES for costs, POSITIVE VALUES for rewards
            Consequence consequence = scaledConsequence ?? Consequence.None();

            // Extract costs (negative values, convert to positive for display)
            int resolveCost = consequence.Resolve < 0 ? -consequence.Resolve : 0;
            int coinsCost = consequence.Coins < 0 ? -consequence.Coins : 0;
            int healthCost = consequence.Health < 0 ? -consequence.Health : 0;
            int staminaCost = consequence.Stamina < 0 ? -consequence.Stamina : 0;
            int focusCost = consequence.Focus < 0 ? -consequence.Focus : 0;
            int hungerCost = consequence.Hunger > 0 ? consequence.Hunger : 0;
            int timeSegments = consequence.TimeSegments;

            // HIGHLANDER: Validate resource availability via OrPath
            // Caller builds OrPath directly - CompoundRequirement stays domain-agnostic
            // Sir Brante pattern: Resolve uses GATE (>= 0), others use AFFORDABILITY (>= cost)
            if (requirementsMet)
            {
                OrPath resourcePath = new OrPath { Label = "Resource Requirements" };
                if (consequence.Resolve < 0) resourcePath.ResolveRequired = 0;  // Gate pattern
                if (consequence.Coins < 0) resourcePath.CoinsRequired = -consequence.Coins;
                if (consequence.Health < 0) resourcePath.HealthRequired = -consequence.Health;
                if (consequence.Stamina < 0) resourcePath.StaminaRequired = -consequence.Stamina;
                if (consequence.Focus < 0) resourcePath.FocusRequired = -consequence.Focus;
                if (consequence.Hunger > 0) resourcePath.HungerCapacityRequired = consequence.Hunger;

                if (!resourcePath.IsSatisfied(player, GameWorld))
                {
                    requirementsMet = false;
                    PathProjection projection = resourcePath.GetProjection(player, GameWorld);
                    List<string> missing = projection.Requirements
                        .Where(r => !r.IsSatisfied)
                        .Select(r => $"{r.Label} (have {r.CurrentValue})")
                        .ToList();
                    lockReason = string.Join(", ", missing);
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

            ActionCardViewModel choiceVM = new ActionCardViewModel
            {
                SourceTemplate = choiceTemplate, // HIGHLANDER: Store template reference for execution
                Name = choice.Label, // AI-GENERATED LABEL from lazy activation (regenerated each entry)
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

                // HIGHLANDER: RequirementsMet now covers BOTH stat requirements AND resource affordability
                // See arc42/08 §8.20 for unified resource availability pattern
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

            Choices.Add(choiceVM);
        }
    }

    private int GetTotalBond(Player player, NPC npc)
    {
        // HIGHLANDER: Tokens stored directly on NPC
        return npc.GetTotalTokens();
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

    private List<RequirementPathVM> GetRequirementGaps(CompoundRequirement compoundReq, Player player)
    {
        List<RequirementPathVM> paths = new List<RequirementPathVM>();

        if (compoundReq == null || compoundReq.OrPaths.Count == 0)
            return paths;

        foreach (OrPath orPath in compoundReq.OrPaths)
        {
            PathProjection projection = orPath.GetProjection(player, GameWorld);

            List<string> requirements = projection.Requirements.Select(r => r.Label).ToList();
            List<string> missingRequirements = projection.MissingRequirements.Select(r => r.Label).ToList();

            paths.Add(new RequirementPathVM
            {
                Requirements = requirements,
                PathSatisfied = projection.IsSatisfied,
                MissingRequirements = missingRequirements
            });
        }

        return paths;
    }

    /// <summary>
    /// Handle player selecting a choice in the modal scene.
    /// Validates requirements, applies costs, executes rewards, handles progression mode.
    /// TWO-PHASE SCALING: Uses SCALED values for BOTH display AND execution (Perfect Information).
    /// </summary>
    protected async Task HandleChoiceSelected(ActionCardViewModel choice)
    {
        // HIGHLANDER: RequirementsMet now covers BOTH stat requirements AND resource affordability
        // See arc42/08 §8.20 for unified resource availability pattern
        if (choice == null || !choice.RequirementsMet)
            return;

        // HIGHLANDER: Use direct object reference from ViewModel
        ChoiceTemplate choiceTemplate = choice.SourceTemplate;

        if (choiceTemplate == null)
            return;

        Player player = GameOrchestrator.GetPlayer();

        // TWO-PHASE SCALING: Derive RuntimeScalingContext from situation entities
        // MUST match LoadChoices() scaling - display = execution (arc42 §8.26)
        RuntimeScalingContext scalingContext = RuntimeScalingContext.FromEntities(
            CurrentSituation.Npc,
            CurrentSituation.Location,
            player);

        // TWO-PHASE SCALING: Apply entity-derived adjustments for execution
        CompoundRequirement scaledRequirement = scalingContext.ApplyToRequirement(choiceTemplate.RequirementFormula);
        Consequence scaledConsequence = scalingContext.ApplyToConsequence(choiceTemplate.Consequence);

        // DEFENSIVE CHECK: Re-validate authored requirements using SCALED values
        if (scaledRequirement != null && scaledRequirement.OrPaths.Count > 0)
        {
            bool requirementsMet = scaledRequirement.IsAnySatisfied(player, GameWorld);
            if (!requirementsMet)
                return; // Requirements not met - should never happen if UI is correct
        }

        // HIGHLANDER: Re-validate resource availability via OrPath using SCALED consequence
        // Caller builds OrPath directly - CompoundRequirement stays domain-agnostic
        Consequence consequence = scaledConsequence ?? Consequence.None();
        OrPath resourcePath = new OrPath { Label = "Resource Requirements" };
        if (consequence.Resolve < 0) resourcePath.ResolveRequired = 0;  // Gate pattern
        if (consequence.Coins < 0) resourcePath.CoinsRequired = -consequence.Coins;
        if (consequence.Health < 0) resourcePath.HealthRequired = -consequence.Health;
        if (consequence.Stamina < 0) resourcePath.StaminaRequired = -consequence.Stamina;
        if (consequence.Focus < 0) resourcePath.FocusRequired = -consequence.Focus;
        if (consequence.Hunger > 0) resourcePath.HungerCapacityRequired = consequence.Hunger;
        if (!resourcePath.IsSatisfied(player, GameWorld))
        {
            return; // Cannot afford costs - should never happen if UI is correct
        }

        // TWO PILLARS: Apply costs via RewardApplicationService.ApplyConsequence()
        // Consequence uses NEGATIVE VALUES for costs: Coins = -5 means pay 5 coins
        // Resolve CAN go negative - that's the Sir Brante willpower consequence
        Consequence costConsequence = new Consequence
        {
            Coins = consequence.Coins < 0 ? consequence.Coins : 0,
            Resolve = consequence.Resolve < 0 ? consequence.Resolve : 0,
            Health = consequence.Health < 0 ? consequence.Health : 0,
            Stamina = consequence.Stamina < 0 ? consequence.Stamina : 0,
            Focus = consequence.Focus < 0 ? consequence.Focus : 0,
            Hunger = consequence.Hunger > 0 ? consequence.Hunger : 0
        };
        await RewardApplicationService.ApplyConsequence(costConsequence, CurrentSituation);

        // TRANSITION TRACKING: Set LastChoice for OnChoice transitions
        CurrentSituation.LastChoice = choiceTemplate;

        // PROCEDURAL CONTENT TRACING: Record choice execution for ALL paths (instant + challenge)
        // UNIFIED ARCHITECTURE: Choice is a choice, whether instant or challenge
        SituationSpawnNode situationNode = ProceduralTracer.GetNodeForSituation(CurrentSituation);
        ChoiceExecutionNode choiceNode = ProceduralTracer.RecordChoiceExecution(
            choiceTemplate,
            situationNode,
            choiceTemplate.ActionTextTemplate,
            playerMetRequirements: true // Reached this point only if requirements met
        );

        // ROUTE BY ACTION TYPE: StartChallenge vs Instant
        if (choiceTemplate.ActionType == ChoiceActionType.StartChallenge)
        {
            // CHALLENGE PATH: Route to tactical challenge subsystem
            // Challenge facades will call SituationCompletionHandler.CompleteSituation() when complete
            // OnSuccessReward applied by challenge system, NOT here
            Console.WriteLine($"[SceneContent.HandleChoiceSelected] Routing to {choiceTemplate.ChallengeType} challenge");

            // TWO-PHASE SCALING: Pre-scale challenge rewards for Perfect Information
            // Challenge facades apply these pre-scaled consequences directly
            Consequence scaledSuccessReward = scalingContext.ApplyToConsequence(choiceTemplate.OnSuccessConsequence);
            Consequence scaledFailureReward = scalingContext.ApplyToConsequence(choiceTemplate.OnFailureConsequence);

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
                    CompletionReward = scaledSuccessReward, // TWO-PHASE SCALING: Pre-scaled
                    FailureReward = scaledFailureReward,    // TWO-PHASE SCALING: Pre-scaled
                    ChoiceExecution = choiceNode // Store for later use
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Mental)
            {
                GameWorld.PendingMentalContext = new MentalChallengeContext
                {
                    Situation = CurrentSituation, // Object reference, NO ID
                    CompletionReward = scaledSuccessReward, // TWO-PHASE SCALING: Pre-scaled
                    FailureReward = scaledFailureReward,    // TWO-PHASE SCALING: Pre-scaled
                    ChoiceExecution = choiceNode // Store for later use
                };
            }
            else if (choiceTemplate.ChallengeType == TacticalSystemType.Physical)
            {
                GameWorld.PendingPhysicalContext = new PhysicalChallengeContext
                {
                    Situation = CurrentSituation, // Object reference, NO ID
                    CompletionReward = scaledSuccessReward, // TWO-PHASE SCALING: Pre-scaled
                    FailureReward = scaledFailureReward,    // TWO-PHASE SCALING: Pre-scaled
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

        // INSTANT PATH: Apply consequences immediately using SCALED values
        // TWO-PHASE SCALING: scaledConsequence has entity-derived cost adjustments (arc42 §8.26)
        Console.WriteLine($"[HandleChoiceSelected.DEBUG] Choice: {choiceTemplate.Id}");
        Console.WriteLine($"[HandleChoiceSelected.DEBUG] Consequence null? {scaledConsequence == null}");
        if (scaledConsequence != null)
        {
            Console.WriteLine($"[HandleChoiceSelected.DEBUG] ScenesToSpawn.Count = {scaledConsequence.ScenesToSpawn.Count}");
            foreach (SceneSpawnReward spawn in scaledConsequence.ScenesToSpawn)
            {
                Console.WriteLine($"[HandleChoiceSelected.DEBUG]   SpawnNextMainStoryScene = {spawn.SpawnNextMainStoryScene}");
            }
            // PROCEDURAL CONTENT TRACING: Push context for instant consequence application
            if (choiceNode != null)
            {
                ProceduralTracer.PushChoiceContext(choiceNode);
            }

            try
            {
                // TWO-PHASE SCALING: Apply SCALED consequence (costs adjusted for entity context)
                await RewardApplicationService.ApplyConsequence(scaledConsequence, CurrentSituation);
            }
            finally
            {
                // ALWAYS pop context (even on exception)
                if (choiceNode != null)
                {
                    ProceduralTracer.PopChoiceContext();
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
                // PROGRESSIVE LOADING: Show description immediately, generate choices in background
                CurrentSituation = nextSituation;
                await ActivateSituationDescriptionAsync();
                await StartDescriptionStreaming();
                _ = GenerateChoicesInBackgroundAsync();
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
