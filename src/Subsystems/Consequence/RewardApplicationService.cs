
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

    public RewardApplicationService(
        GameWorld gameWorld,
        ConsequenceFacade consequenceFacade,
        TimeFacade timeFacade,
        SceneInstantiator sceneInstantiator,
        ProceduralAStoryService proceduralAStoryService,
        PackageLoader packageLoader)
    {
        _gameWorld = gameWorld;
        _consequenceFacade = consequenceFacade;
        _timeFacade = timeFacade;
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _proceduralAStoryService = proceduralAStoryService ?? throw new ArgumentNullException(nameof(proceduralAStoryService));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
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
            // CONTEXT INJECTION (arc42 §8.28): Ensure RhythmPattern is SET on spawn reward
            // This is the CREATION point - compute and set if not already authored
            EnsureRhythmPatternSet(sceneSpawn);

            SceneTemplate template;

            if (sceneSpawn.SpawnNextMainStoryScene)
            {
                // MAINSTORY SEQUENCING - NO ID STRINGS
                int currentSequence = currentSituation?.ParentScene?.MainStorySequence ?? player.CurrentMainStorySequence;
                Console.WriteLine($"[FinalizeSceneSpawns] SpawnNextMainStoryScene=true, CurrentSequence={currentSequence}");

                // Try to find authored template for next sequence
                template = _gameWorld.GetNextMainStoryTemplate(currentSequence);

                if (template == null)
                {
                    // No authored template exists - generate procedurally
                    // CONTEXT INJECTION: spawn reward now has RhythmPattern SET (not derived)
                    Console.WriteLine($"[FinalizeSceneSpawns] No authored template for sequence {currentSequence + 1}, generating procedurally");
                    AStoryContext aStoryContext = _proceduralAStoryService.GetOrInitializeContext(player);

                    // Build selection inputs from spawn reward - RhythmPattern is GUARANTEED set
                    List<string> recentCategories = GetRecentCategories();
                    List<string> recentArchetypes = GetRecentArchetypes();
                    SceneSelectionInputs selectionInputs = sceneSpawn.BuildSelectionInputs(recentCategories, recentArchetypes);

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
                // NON-MAINSTORY: Direct template reference
                template = sceneSpawn.Template;
                Console.WriteLine($"[FinalizeSceneSpawns] Using direct template reference: '{template.Id}'");
            }
            else
            {
                throw new InvalidOperationException(
                    "SceneSpawnReward must have either SpawnNextMainStoryScene=true OR Template set.");
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

            // HIGHLANDER FLOW: Spawn scene directly
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
    /// Ensure RhythmPattern is SET on spawn reward.
    /// CONTEXT INJECTION (arc42 §8.28): This is the CREATION point for context.
    /// If authored, already set. If not, compute from history and SET it.
    /// After this call, RhythmPatternContext is GUARANTEED to be set.
    /// </summary>
    private void EnsureRhythmPatternSet(SceneSpawnReward sceneSpawn)
    {
        if (sceneSpawn.RhythmPatternContext.HasValue)
        {
            return; // Already set (authored)
        }

        // Compute RhythmPattern from intensity history and SET it
        sceneSpawn.RhythmPatternContext = ComputeRhythmPatternFromHistory();
        Console.WriteLine($"[EnsureRhythmPatternSet] Computed and SET RhythmPattern={sceneSpawn.RhythmPatternContext}");
    }

    /// <summary>
    /// Get recent categories for anti-repetition.
    /// </summary>
    private List<string> GetRecentCategories()
    {
        return _gameWorld.Scenes
            .Where(s => s.Category == StoryCategory.MainStory &&
                        s.State == SceneState.Completed &&
                        s.Template != null)
            .OrderByDescending(s => s.MainStorySequence)
            .Take(2)
            .Select(s => ArchetypeCategorySelector.MapArchetypeToCategory(s.Template.SceneArchetype))
            .ToList();
    }

    /// <summary>
    /// Get recent archetypes for anti-repetition.
    /// </summary>
    private List<string> GetRecentArchetypes()
    {
        return _gameWorld.Scenes
            .Where(s => s.Category == StoryCategory.MainStory &&
                        s.State == SceneState.Completed &&
                        s.Template != null)
            .OrderByDescending(s => s.MainStorySequence)
            .Take(3)
            .Select(s => s.Template.SceneArchetype.ToString())
            .ToList();
    }

    /// <summary>
    /// Compute RhythmPattern from completed scene intensity history.
    /// Called at CREATION time to SET on spawn reward.
    /// </summary>
    private RhythmPattern ComputeRhythmPatternFromHistory()
    {
        List<Scene> completedScenes = _gameWorld.Scenes
            .Where(s => s.Category == StoryCategory.MainStory &&
                        s.MainStorySequence.HasValue &&
                        s.State == SceneState.Completed)
            .OrderBy(s => s.MainStorySequence)
            .ToList();

        if (!completedScenes.Any())
        {
            return RhythmPattern.Building; // Default for game start
        }

        // Extract intensity and rhythm history
        List<ArchetypeIntensity> intensityHistory = new List<ArchetypeIntensity>();
        List<RhythmPattern> rhythmHistory = new List<RhythmPattern>();

        foreach (Scene scene in completedScenes)
        {
            ArchetypeIntensity sceneIntensity = ArchetypeIntensity.Standard;
            if (scene.Situations.Any())
            {
                sceneIntensity = scene.Situations.Max(s => s.Intensity);
            }
            intensityHistory.Add(sceneIntensity);

            if (scene.Template != null)
            {
                rhythmHistory.Add(scene.Template.RhythmPattern);
            }
        }

        // Just after Crisis rhythm → Mixed (recovery phase)
        if (rhythmHistory.Any() && rhythmHistory.Last() == RhythmPattern.Crisis)
        {
            return RhythmPattern.Mixed;
        }

        // Recent window for intensity counts
        List<ArchetypeIntensity> recent = intensityHistory.TakeLast(5).ToList();
        int demandingCount = recent.Count(i => i == ArchetypeIntensity.Demanding);
        int recoveryCount = recent.Count(i => i == ArchetypeIntensity.Recovery);

        // Intensity heavy (more Demanding than Recovery) → needs Mixed
        if (demandingCount > recoveryCount)
        {
            return RhythmPattern.Mixed;
        }

        // Calculate scenes since last Demanding
        int scenesSinceDemanding = 0;
        for (int i = intensityHistory.Count - 1; i >= 0; i--)
        {
            if (intensityHistory[i] == ArchetypeIntensity.Demanding)
            {
                break;
            }
            scenesSinceDemanding++;
        }

        // Long time since Demanding → time for Crisis
        if (scenesSinceDemanding >= 4)
        {
            return RhythmPattern.Crisis;
        }

        // Default: Building
        return RhythmPattern.Building;
    }

}
