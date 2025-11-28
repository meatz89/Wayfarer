
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
    /// NO ID STRINGS - uses boolean flags and sequence-based lookup
    /// Perfect information shown from SceneTemplate metadata
    /// </summary>
    private async Task FinalizeSceneSpawns(ChoiceReward reward, Situation currentSituation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
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
                    Console.WriteLine($"[FinalizeSceneSpawns] No authored template for sequence {currentSequence + 1}, generating procedurally");
                    AStoryContext aStoryContext = _proceduralAStoryService.GetOrInitializeContext(player);
                    await _proceduralAStoryService.GenerateNextATemplate(currentSequence + 1, aStoryContext);

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
    /// HIGHLANDER: Single source of truth for applying ALL costs and rewards.
    /// Consequence is the unified class: negative values = costs, positive values = rewards.
    /// Clamping: Health/Stamina/Focus floor at 0, cap at Max. Hunger floor at 0, cap at Max.
    /// Sir Brante pattern: Resolve CAN go negative (willpower debt).
    /// </summary>
    public async Task ApplyConsequence(Consequence consequence, Situation currentSituation)
    {
        Player player = _gameWorld.GetPlayer();

        // STEP 1: Apply COSTS (negative values) with consistent clamping
        // Coins: No floor - can technically go negative (debt scenario)
        if (consequence.Coins < 0)
            player.Coins += consequence.Coins;  // Negative, so this subtracts

        // Sir Brante Willpower Pattern: Resolve CAN go negative
        if (consequence.Resolve < 0)
            player.Resolve += consequence.Resolve;  // Negative, so this subtracts

        // Health/Stamina/Focus: Floor at 0
        if (consequence.Health < 0)
            player.Health = Math.Max(0, player.Health + consequence.Health);

        if (consequence.Stamina < 0)
            player.Stamina = Math.Max(0, player.Stamina + consequence.Stamina);

        if (consequence.Focus < 0)
            player.Focus = Math.Max(0, player.Focus + consequence.Focus);

        // Hunger increase is a COST (positive hunger = bad), handled in rewards section

        // STEP 2: Apply REWARDS (positive values) via ApplyChoiceReward
        ChoiceReward legacyReward = new ChoiceReward
        {
            Coins = consequence.Coins > 0 ? consequence.Coins : 0,
            Resolve = consequence.Resolve > 0 ? consequence.Resolve : 0,
            Health = consequence.Health > 0 ? consequence.Health : 0,
            Stamina = consequence.Stamina > 0 ? consequence.Stamina : 0,
            Focus = consequence.Focus > 0 ? consequence.Focus : 0,
            Hunger = consequence.Hunger,  // Hunger uses Clamp in ApplyChoiceReward
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
