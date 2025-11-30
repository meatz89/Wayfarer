
/// <summary>
/// HIGHLANDER: Centralized service for applying Consequence (the ONLY class for resource outcomes)
/// Used by GameFacade (instant actions), challenge facades (on completion), and SceneFacade (choice completion)
/// Handles: resources, bonds, scales, states, achievements, items, scene spawning, time advancement
/// Tutorial system relies on this for reward application after challenges complete
/// On-demand template generation: Procedural A-story templates generated when spawning if don't exist yet
/// </summary>
public class RewardApplicationService
{
    private readonly GameWorld _gameWorld;
    private readonly ConsequenceFacade _consequenceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly ProceduralAStoryService _proceduralAStoryService;
    private readonly PackageLoader _packageLoader;
    private readonly PlayerReadinessService _playerReadinessService;

    public RewardApplicationService(
        GameWorld gameWorld,
        ConsequenceFacade consequenceFacade,
        TimeFacade timeFacade,
        SceneInstantiator sceneInstantiator,
        ProceduralAStoryService proceduralAStoryService,
        PackageLoader packageLoader,
        PlayerReadinessService playerReadinessService)
    {
        _gameWorld = gameWorld;
        _consequenceFacade = consequenceFacade;
        _timeFacade = timeFacade;
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _proceduralAStoryService = proceduralAStoryService ?? throw new ArgumentNullException(nameof(proceduralAStoryService));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
        _playerReadinessService = playerReadinessService ?? throw new ArgumentNullException(nameof(playerReadinessService));
    }

    /// <summary>
    /// HIGHLANDER: Apply all components of a Consequence (costs AND rewards)
    /// This is the SINGLE METHOD for ALL resource mutations.
    ///
    /// PROCEDURAL CONTENT TRACING NOTE:
    /// Choice execution recording should happen in the CALLER before invoking this method:
    ///   1. choiceNode = tracer.RecordChoiceExecution(choiceTemplate, situationNodeId, actionText, metRequirements)
    ///   2. tracer.PushChoiceContext(choiceNode.NodeId)
    ///   3. await ApplyConsequence(consequence, situation)  // Scenes spawned here auto-link to choice
    ///   4. tracer.PopChoiceContext()
    /// This ensures spawned scenes link correctly to the choice that triggered them.
    /// </summary>
    public async Task ApplyConsequence(Consequence consequence, Situation currentSituation)
    {
        // ZERO NULL TOLERANCE: consequence must never be null (architectural guarantee from caller)
        Player player = _gameWorld.GetPlayer();

        // Apply FullRecovery if flagged (overrides individual resource rewards)
        if (consequence.FullRecovery)
        {
            player.Health = player.MaxHealth;
            player.Stamina = player.MaxStamina;
            player.Focus = player.MaxFocus;
            player.Hunger = 0; // Full recovery resets hunger to 0
        }
        else
        {
            // HIGHLANDER: Apply ALL resource changes via Consequence (negative = cost, positive = reward)
            if (consequence.Coins != 0)
                player.Coins += consequence.Coins;

            // Sir Brante Willpower Pattern: Resolve CAN go negative
            if (consequence.Resolve != 0)
                player.Resolve += consequence.Resolve;

            // Health/Stamina/Focus: Clamp to 0-Max range
            if (consequence.Health != 0)
                player.Health = Math.Clamp(player.Health + consequence.Health, 0, player.MaxHealth);

            if (consequence.Stamina != 0)
                player.Stamina = Math.Clamp(player.Stamina + consequence.Stamina, 0, player.MaxStamina);

            if (consequence.Focus != 0)
                player.Focus = Math.Clamp(player.Focus + consequence.Focus, 0, player.MaxFocus);

            if (consequence.Hunger != 0)
                player.Hunger = Math.Clamp(player.Hunger + consequence.Hunger, 0, player.MaxHunger);

            // Apply stat changes (Sir Brante pattern: direct grants, no XP system)
            if (consequence.Insight != 0)
                player.Insight += consequence.Insight;

            if (consequence.Rapport != 0)
                player.Rapport += consequence.Rapport;

            if (consequence.Authority != 0)
                player.Authority += consequence.Authority;

            if (consequence.Diplomacy != 0)
                player.Diplomacy += consequence.Diplomacy;

            if (consequence.Cunning != 0)
                player.Cunning += consequence.Cunning;

            // Mental progression: Understanding (0-10 scale, cumulative expertise)
            if (consequence.Understanding != 0)
                player.Understanding = Math.Min(10, player.Understanding + consequence.Understanding);
        }

        // Apply consequences (bonds, scales, states)
        if (consequence.BondChanges.Count > 0 || consequence.ScaleShifts.Count > 0 || consequence.StateApplications.Count > 0)
        {
            _consequenceFacade.ApplyConsequences(consequence.BondChanges, consequence.ScaleShifts, consequence.StateApplications);
        }

        // Apply achievements
        foreach (Achievement achievement in consequence.Achievements)
        {
            // Check if achievement already earned
            // HIGHLANDER: Compare Achievement objects directly
            if (!player.EarnedAchievements.Any(a => a.Achievement == achievement))
            {
                player.EarnedAchievements.Add(new PlayerAchievement
                {
                    Achievement = achievement,
                    EarnedDay = _gameWorld.CurrentDay,
                    EarnedTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                    EarnedSegment = _timeFacade.GetCurrentSegment()
                });
            }
        }

        // Apply item grants (runtime resolved items from JSON templates)
        foreach (Item item in consequence.Items)
        {
            player.Inventory.Add(item);
        }

        // Apply item removals (Multi-Situation Scene Pattern: cleanup phase)
        foreach (Item item in consequence.ItemsToRemove)
        {
            player.RemoveItem(item);
        }

        // Apply time advancement
        if (consequence.AdvanceToBlock.HasValue)
        {
            if (consequence.AdvanceToDay == DayAdvancement.NextDay)
            {
                // Advance to next day at specified block
                AdvanceToNextDayAtBlock(consequence.AdvanceToBlock.Value);
            }
            else
            {
                // Advance to block within current day
                AdvanceToBlock(consequence.AdvanceToBlock.Value);
            }
        }
        else if (consequence.TimeSegments > 0)
        {
            // Normal segment advancement
            _timeFacade.AdvanceSegments(consequence.TimeSegments);
        }

        // Finalize scene spawns
        await FinalizeSceneSpawns(consequence, currentSituation);
    }

    /// <summary>
    /// Advance to specific TimeBlock within current day
    /// </summary>
    private void AdvanceToBlock(TimeBlocks targetBlock)
    {
        TimeBlocks currentBlock = _timeFacade.GetCurrentTimeBlock();

        // Calculate segments needed to reach target block
        int currentBlockIndex = (int)currentBlock;
        int targetBlockIndex = (int)targetBlock;

        if (targetBlockIndex <= currentBlockIndex)
            return; // Already at or past target block

        int segmentsToAdvance = (targetBlockIndex - currentBlockIndex) * 4; // 4 segments per block
        _timeFacade.AdvanceSegments(segmentsToAdvance);
    }

    /// <summary>
    /// Advance to next day at specific TimeBlock
    /// Tutorial Night Rest uses this: Evening → Day 2 Morning
    /// </summary>
    private void AdvanceToNextDayAtBlock(TimeBlocks targetBlock)
    {
        TimeBlocks currentBlock = _timeFacade.GetCurrentTimeBlock();

        // Calculate segments to end of current day
        int currentBlockIndex = (int)currentBlock;
        int segmentsToEndOfDay = (4 - currentBlockIndex) * 4; // Remaining blocks * 4 segments

        // Add segments to reach target block in next day
        int targetBlockIndex = (int)targetBlock;
        int segmentsIntoNextDay = targetBlockIndex * 4;

        _timeFacade.AdvanceSegments(segmentsToEndOfDay + segmentsIntoNextDay);
    }

    /// <summary>
    /// Spawn scenes when action selected (HIGHLANDER FLOW)
    /// NO ID STRINGS - uses boolean flags and sequence-based lookup
    /// Perfect information shown from SceneTemplate metadata
    /// </summary>
    private async Task FinalizeSceneSpawns(Consequence consequence, Situation currentSituation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SceneSpawnReward sceneSpawn in consequence.ScenesToSpawn)
        {
            SceneTemplate template;

            if (sceneSpawn.SpawnNextMainStoryScene)
            {
                // MAINSTORY SEQUENCING - NO ID STRINGS
                // Use the CURRENT SCENE's sequence (the one being completed), not player's tracked progress
                // Player.CurrentMainStorySequence is updated AFTER scene completion by SituationCompletionHandler
                // If we use player's sequence here, we'd spawn the same scene again
                int currentSequence = currentSituation?.ParentScene?.MainStorySequence ?? player.CurrentMainStorySequence;
                Console.WriteLine($"[FinalizeSceneSpawns] SpawnNextMainStoryScene=true, CurrentSequence={currentSequence} (from Scene={currentSituation?.ParentScene?.MainStorySequence}, Player={player.CurrentMainStorySequence})");

                // Try to find authored template for next sequence
                template = _gameWorld.GetNextMainStoryTemplate(currentSequence);

                if (template == null)
                {
                    // No authored template exists - generate procedurally
                    // CONTEXT INJECTION (HIGHLANDER): Build inputs from authored context OR GameWorld
                    Console.WriteLine($"[FinalizeSceneSpawns] No authored template for sequence {currentSequence + 1}, generating procedurally");
                    AStoryContext aStoryContext = _proceduralAStoryService.GetOrInitializeContext(player);

                    // Build selection inputs: authored context if available, else from GameWorld
                    SceneSelectionInputs selectionInputs = BuildSelectionInputs(
                        currentSequence + 1, sceneSpawn, player, currentSituation?.Location);

                    await _proceduralAStoryService.GenerateNextATemplate(
                        currentSequence + 1, aStoryContext, selectionInputs);

                    // ZERO NULL TOLERANCE: Template must exist after generation
                    template = _gameWorld.GetNextMainStoryTemplate(currentSequence);
                    if (template == null)
                    {
                        throw new InvalidOperationException(
                            $"Failed to generate MainStory template for sequence {currentSequence + 1}");
                    }
                }

                Console.WriteLine($"[FinalizeSceneSpawns] Found template '{template.Id}' (MainStorySequence={template.MainStorySequence})");
            }
            else if (sceneSpawn.Template != null)
            {
                // NON-MAINSTORY: Direct template reference (resolved at parse time)
                // NO ID STRINGS - object reference only
                template = sceneSpawn.Template;
                Console.WriteLine($"[FinalizeSceneSpawns] Using direct template reference: '{template.Id}'");
            }
            else
            {
                // Invalid spawn configuration
                throw new InvalidOperationException(
                    "SceneSpawnReward must have either SpawnNextMainStoryScene=true OR Template set. " +
                    "NO ID STRINGS - use boolean flags and object references only.");
            }

            // Resolve placement context
            RouteOption currentRoute = currentSituation!.Route;
            Location currentLocation = currentSituation!.Location;
            NPC currentNPC = currentSituation!.Npc;

            // Build spawn context
            SceneSpawnContext context = new SceneSpawnContext
            {
                Player = player,
                CurrentSituation = currentSituation,
                CurrentLocation = currentLocation,
                CurrentNPC = currentNPC,
                CurrentRoute = currentRoute
            };

            // HIGHLANDER FLOW: Spawn scene directly (JSON → PackageLoader → Parser)
            string packageJson = _sceneInstantiator.CreateDeferredScene(template, sceneSpawn, context);
            if (!string.IsNullOrEmpty(packageJson))
            {
                string packageId = $"scene_{template.Id}_{Guid.NewGuid().ToString("N")}";
                await _packageLoader.LoadDynamicPackageFromJson(packageJson, packageId);
                Console.WriteLine($"[FinalizeSceneSpawns] Created deferred scene package: {packageId}");
            }
        }
    }

    /// <summary>
    /// Build SceneSelectionInputs for procedural generation.
    /// CONTEXT INJECTION (HIGHLANDER): Same code path for authored and procedural.
    /// - If SceneSpawnReward has authored context: use explicit values
    /// - If not: derive from GameWorld state
    /// </summary>
    private SceneSelectionInputs BuildSelectionInputs(
        int sequence,
        SceneSpawnReward sceneSpawn,
        Player player,
        Location currentLocation)
    {
        // Start with base inputs
        SceneSelectionInputs inputs = new SceneSelectionInputs
        {
            Sequence = sequence,
            MaxSafeIntensity = _playerReadinessService.GetMaxSafeIntensity(player)
        };

        // CONTEXT INJECTION: Use authored context if available
        if (sceneSpawn.HasAuthoredContext)
        {
            // Authored path: use explicit values from content
            inputs.TargetCategory = sceneSpawn.TargetCategory;
            inputs.LocationSafety = sceneSpawn.LocationSafetyContext ?? LocationSafety.Safe;
            inputs.LocationPurpose = sceneSpawn.LocationPurposeContext ?? LocationPurpose.Civic;
            inputs.ExcludedCategories = sceneSpawn.ExcludedCategories ?? new List<string>();
        }
        else
        {
            // Procedural path: derive from GameWorld state
            inputs.TargetCategory = null; // Will use rotation logic
            inputs.LocationSafety = currentLocation?.Safety ?? LocationSafety.Safe;
            inputs.LocationPurpose = currentLocation?.Purpose ?? LocationPurpose.Civic;
            inputs.ExcludedCategories = new List<string>();
        }

        return inputs;
    }
}
