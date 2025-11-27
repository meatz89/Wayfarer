
/// <summary>
/// Centralized service for applying ChoiceReward consequences
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
    /// Apply all components of a ChoiceReward
    ///
    /// PROCEDURAL CONTENT TRACING NOTE:
    /// Choice execution recording should happen in the CALLER before invoking this method:
    ///   1. choiceNode = tracer.RecordChoiceExecution(choiceTemplate, situationNodeId, actionText, metRequirements)
    ///   2. tracer.PushChoiceContext(choiceNode.NodeId)
    ///   3. await ApplyChoiceReward(reward, situation)  // Scenes spawned here auto-link to choice
    ///   4. tracer.PopChoiceContext()
    /// This ensures spawned scenes link correctly to the choice that triggered them.
    /// </summary>
    public async Task ApplyChoiceReward(ChoiceReward reward, Situation currentSituation)
    {
        // ZERO NULL TOLERANCE: reward must never be null (architectural guarantee from caller)
        Player player = _gameWorld.GetPlayer();

        // Apply FullRecovery if flagged (overrides individual resource rewards)
        if (reward.FullRecovery)
        {
            player.Health = player.MaxHealth;
            player.Stamina = player.MaxStamina;
            player.Focus = player.MaxFocus;
            player.Hunger = 0; // Full recovery resets hunger to 0
        }
        else
        {
            // Apply basic resource rewards (existing)
            if (reward.Coins != 0)
                player.Coins += reward.Coins;

            if (reward.Resolve != 0)
                player.Resolve += reward.Resolve;

            // Apply tutorial resource rewards (NEW)
            if (reward.Health != 0)
                player.Health = Math.Clamp(player.Health + reward.Health, 0, player.MaxHealth);

            if (reward.Stamina != 0)
                player.Stamina = Math.Clamp(player.Stamina + reward.Stamina, 0, player.MaxStamina);

            if (reward.Focus != 0)
                player.Focus = Math.Clamp(player.Focus + reward.Focus, 0, player.MaxFocus);

            if (reward.Hunger != 0)
                player.Hunger = Math.Clamp(player.Hunger + reward.Hunger, 0, player.MaxHunger);

            // Apply stat rewards (Sir Brante pattern: direct grants, no XP system)
            if (reward.Insight != 0)
                player.Insight += reward.Insight;

            if (reward.Rapport != 0)
                player.Rapport += reward.Rapport;

            if (reward.Authority != 0)
                player.Authority += reward.Authority;

            if (reward.Diplomacy != 0)
                player.Diplomacy += reward.Diplomacy;

            if (reward.Cunning != 0)
                player.Cunning += reward.Cunning;
        }

        // Apply consequences (bonds, scales, states)
        if (reward.BondChanges.Count > 0 || reward.ScaleShifts.Count > 0 || reward.StateApplications.Count > 0)
        {
            _consequenceFacade.ApplyConsequences(reward.BondChanges, reward.ScaleShifts, reward.StateApplications);
        }

        // Apply achievements
        foreach (Achievement achievement in reward.Achievements)
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

        // Markers deleted in 5-system architecture - entity IDs are concrete, no resolution needed

        // Apply item grants (runtime resolved items from JSON templates)
        foreach (Item item in reward.Items)
        {
            player.Inventory.Add(item);
        }

        // Apply item removals (Multi-Situation Scene Pattern: cleanup phase)
        // Runtime resolved items (from JSON templates)
        foreach (Item item in reward.ItemsToRemove)
        {
            player.RemoveItem(item);
        }

        // Apply time advancement (NEW - for tutorial Night Rest scene)
        if (reward.AdvanceToBlock.HasValue)
        {
            if (reward.AdvanceToDay == DayAdvancement.NextDay)
            {
                // Advance to next day at specified block
                AdvanceToNextDayAtBlock(reward.AdvanceToBlock.Value);
            }
            else
            {
                // Advance to block within current day
                AdvanceToBlock(reward.AdvanceToBlock.Value);
            }
        }
        else if (reward.TimeSegments > 0)
        {
            // Normal segment advancement
            _timeFacade.AdvanceSegments(reward.TimeSegments);
        }

        // Finalize scene spawns
        await FinalizeSceneSpawns(reward, currentSituation);
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
    /// NO provisional scenes - spawns directly as Active
    /// Perfect information shown from SceneTemplate metadata
    /// </summary>
    private async Task FinalizeSceneSpawns(ChoiceReward reward, Situation currentSituation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
        {
            // Lookup by sequence number (NOT by ID string) for A-story scenes
            // This allows authored scenes to have ANY ID format (a1_secure_lodging, a2_morning, etc.)
            // and procedural scenes to use pattern-based IDs (a_story_11, a_story_12, etc.)
            SceneTemplate template = null;

            // Extract sequence number from ID pattern
            if (sceneSpawn.SceneTemplateId.StartsWith("a_story_"))
            {
                string sequenceStr = sceneSpawn.SceneTemplateId.Replace("a_story_", "");
                if (int.TryParse(sequenceStr, out int sequence))
                {
                    // Find by mainStorySequence (works for both authored and procedural scenes)
                    template = _gameWorld.SceneTemplates
                        .FirstOrDefault(t => t.MainStorySequence.HasValue && t.MainStorySequence.Value == sequence);

                    // Generate procedurally if not found (on-demand generation)
                    if (template == null)
                    {
                        AStoryContext aStoryContext = _proceduralAStoryService.GetOrInitializeContext(player);
                        await _proceduralAStoryService.GenerateNextATemplate(sequence, aStoryContext);

                        // ZERO NULL TOLERANCE: Template must exist after generation (assert with First())
                        template = _gameWorld.SceneTemplates
                            .First(t => t.MainStorySequence.HasValue && t.MainStorySequence.Value == sequence);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Invalid A-story template ID format: '{sceneSpawn.SceneTemplateId}'. " +
                        $"Expected format 'a_story_<number>' (e.g., 'a_story_4'). " +
                        $"Check SceneSpawnReward configuration in choice rewards.");
                }
            }
            else
            {
                // Non-A-story scene - lookup by ID as normal
                // ZERO NULL TOLERANCE: Template must exist (will throw if not found)
                template = _gameWorld.SceneTemplates
                    .First(t => t.Id == sceneSpawn.SceneTemplateId);
            }

            // Resolve placement context (ARCHITECTURAL CHANGE: Direct property access)
            // Context properties are nullable - not all situations have Route/Location/NPC
            // SceneSpawnContext documents these as nullable per domain model
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
            }
        }

        // NO CLEANUP NEEDED: Provisional scenes don't exist in HIGHLANDER flow
    }

    /// <summary>
    /// Apply Consequence to player state (unified costs/rewards pattern)
    /// Wraps ApplyChoiceReward for Consequence parameter type
    /// Used by ChoiceTemplate.Consequence (new unified pattern)
    /// </summary>
    public async Task ApplyConsequence(Consequence consequence, Situation currentSituation)
    {
        // Consequence and ChoiceReward are semantically equivalent (costs + rewards)
        // Consequence uses negative values for costs, ChoiceReward uses separate Cost/Reward objects
        // This adapter bridges the gap until full refactoring

        // For now, delegate to ApplyChoiceReward by converting Consequence → ChoiceReward
        // TODO: Refactor all callers to use Consequence directly (HIGHLANDER)
        ChoiceReward legacyReward = new ChoiceReward
        {
            Coins = consequence.Coins > 0 ? consequence.Coins : 0,
            Resolve = consequence.Resolve > 0 ? consequence.Resolve : 0,
            Health = consequence.Health > 0 ? consequence.Health : 0,
            Stamina = consequence.Stamina > 0 ? consequence.Stamina : 0,
            Focus = consequence.Focus > 0 ? consequence.Focus : 0,
            Hunger = consequence.Hunger,
            FullRecovery = consequence.FullRecovery,
            Insight = consequence.Insight,
            Rapport = consequence.Rapport,
            Authority = consequence.Authority,
            Diplomacy = consequence.Diplomacy,
            Cunning = consequence.Cunning,
            BondChanges = consequence.BondChanges,
            ScaleShifts = consequence.ScaleShifts,
            StateApplications = consequence.StateApplications,
            Achievements = consequence.Achievements,
            Items = consequence.Items,
            ItemsToRemove = consequence.ItemsToRemove,
            ScenesToSpawn = consequence.ScenesToSpawn
        };

        await ApplyChoiceReward(legacyReward, currentSituation);
    }

}
