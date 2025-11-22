
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
    private readonly SceneInstanceFacade _sceneInstanceFacade;
    private readonly DependentResourceOrchestrationService _dependentResourceOrchestrationService;
    private readonly ProceduralAStoryService _proceduralAStoryService;

    public RewardApplicationService(
        GameWorld gameWorld,
        ConsequenceFacade consequenceFacade,
        TimeFacade timeFacade,
        SceneInstanceFacade sceneInstanceFacade,
        DependentResourceOrchestrationService dependentResourceOrchestrationService,
        ProceduralAStoryService proceduralAStoryService)
    {
        _gameWorld = gameWorld;
        _consequenceFacade = consequenceFacade;
        _timeFacade = timeFacade;
        _sceneInstanceFacade = sceneInstanceFacade;
        _dependentResourceOrchestrationService = dependentResourceOrchestrationService ?? throw new ArgumentNullException(nameof(dependentResourceOrchestrationService));
        _proceduralAStoryService = proceduralAStoryService ?? throw new ArgumentNullException(nameof(proceduralAStoryService));
    }

    /// <summary>
    /// Apply all components of a ChoiceReward
    /// </summary>
    public async Task ApplyChoiceReward(ChoiceReward reward, Situation currentSituation)
    {
        if (reward == null)
            return;

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
            // Get template (or generate on-demand if procedural A-story)
            SceneTemplate template = _gameWorld.SceneTemplates
                .FirstOrDefault(t => t.Id == sceneSpawn.SceneTemplateId);

            if (template == null)
            {
                // On-demand template generation for procedural A-story
                // Pattern: Template IDs like "a_story_sequence_4", "a_story_sequence_11", etc.
                if (sceneSpawn.SceneTemplateId.StartsWith("a_story_sequence_"))
                {
                    string sequenceStr = sceneSpawn.SceneTemplateId.Replace("a_story_sequence_", "");
                    if (int.TryParse(sequenceStr, out int sequence))
                    {
                        Console.WriteLine($"[RewardApplicationService] A-story template '{sceneSpawn.SceneTemplateId}' not found - generating procedurally");

                        // Get or initialize A-story context
                        AStoryContext aStoryContext = _proceduralAStoryService.GetOrInitializeContext(player);

                        // Generate template procedurally (HIGHLANDER: DTO → JSON → PackageLoader → Template)
                        string generatedTemplateId = await _proceduralAStoryService.GenerateNextATemplate(sequence, aStoryContext);

                        Console.WriteLine($"[RewardApplicationService] Generated A-story template: {generatedTemplateId}");

                        // Retrieve generated template
                        template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == generatedTemplateId);

                        if (template == null)
                        {
                            Console.WriteLine($"[RewardApplicationService] FATAL: Generated template '{generatedTemplateId}' not found in GameWorld after generation");
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[RewardApplicationService] Invalid A-story sequence number in template ID: '{sceneSpawn.SceneTemplateId}'");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"[RewardApplicationService] SceneTemplate '{sceneSpawn.SceneTemplateId}' not found (not an A-story pattern)");
                    continue;
                }
            }

            // Resolve placement context (ARCHITECTURAL CHANGE: Direct property access)
            RouteOption currentRoute = currentSituation?.Route;
            Location currentLocation = currentSituation?.Location;
            NPC currentNPC = currentSituation?.Npc;

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
            Scene scene = await _sceneInstanceFacade.SpawnScene(template, sceneSpawn, context);

            if (scene != null)
            {
                // ADR-007: Use TemplateId for logging (no Id property)
                Console.WriteLine($"[RewardApplicationService] Spawned scene '{scene.DisplayName}' ({scene.TemplateId})");
            }
        }

        // NO CLEANUP NEEDED: Provisional scenes don't exist in HIGHLANDER flow
    }

}
