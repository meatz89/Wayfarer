using Wayfarer.Content;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Services;

namespace Wayfarer.Subsystems.Consequence;

/// <summary>
/// Centralized service for applying ChoiceReward consequences
/// Used by GameFacade (instant actions), challenge facades (on completion), and SceneFacade (AutoAdvance)
/// Handles: resources, bonds, scales, states, achievements, items, scene spawning, time advancement
/// Tutorial system relies on this for reward application after challenges complete
/// </summary>
public class RewardApplicationService
{
    private readonly GameWorld _gameWorld;
    private readonly ConsequenceFacade _consequenceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly SceneInstantiator _sceneInstantiator;

    public RewardApplicationService(
        GameWorld gameWorld,
        ConsequenceFacade consequenceFacade,
        TimeFacade timeFacade,
        SceneInstantiator sceneInstantiator)
    {
        _gameWorld = gameWorld;
        _consequenceFacade = consequenceFacade;
        _timeFacade = timeFacade;
        _sceneInstantiator = sceneInstantiator;
    }

    /// <summary>
    /// Apply all components of a ChoiceReward
    /// </summary>
    public void ApplyChoiceReward(ChoiceReward reward, Situation currentSituation)
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
        }

        // Apply consequences (bonds, scales, states)
        if (reward.BondChanges.Count > 0 || reward.ScaleShifts.Count > 0 || reward.StateApplications.Count > 0)
        {
            _consequenceFacade.ApplyConsequences(reward.BondChanges, reward.ScaleShifts, reward.StateApplications);
        }

        // Apply achievements
        foreach (string achievementId in reward.AchievementIds)
        {
            // Check if achievement already earned
            if (!player.EarnedAchievements.Any(a => a.AchievementId == achievementId))
            {
                player.EarnedAchievements.Add(new PlayerAchievement
                {
                    AchievementId = achievementId,
                    EarnedDay = _gameWorld.CurrentDay,
                    EarnedTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                    EarnedSegment = _timeFacade.GetCurrentSegment()
                });
            }
        }

        // Apply items
        foreach (string itemId in reward.ItemIds)
        {
            // Add to inventory (stub - future inventory system)
            // TODO: Implement when inventory system exists
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
        FinalizeSceneSpawns(reward, currentSituation);
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
    /// Finalize provisional Scenes created during action generation
    /// Provisional Scenes created eagerly (perfect information), now finalize when action selected
    /// </summary>
    private void FinalizeSceneSpawns(ChoiceReward reward, Situation currentSituation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SceneSpawnReward sceneSpawn in reward.ScenesToSpawn)
        {
            // Resolve Route if needed (Situation only has RouteId, not object reference)
            RouteOption currentRoute = null;
            if (!string.IsNullOrEmpty(currentSituation?.PlacementRouteId))
            {
                currentRoute = _gameWorld.Routes.FirstOrDefault(r => r.Id == currentSituation.PlacementRouteId);
            }

            // Build spawn context from Situation placement
            SceneSpawnContext context = new SceneSpawnContext
            {
                Player = player,
                CurrentSituation = currentSituation,
                CurrentLocation = currentSituation?.PlacementLocation?.Venue,
                CurrentNPC = currentSituation?.PlacementNpc,
                CurrentRoute = currentRoute
            };

            // Find provisional Scene matching this template and placement
            // Provisional Scene was created during action generation (eager creation for perfect information)
            global::Scene provisionalScene = _gameWorld.ProvisionalScenes.Values
                .FirstOrDefault(s => s.TemplateId == sceneSpawn.SceneTemplateId);

            if (provisionalScene != null)
            {
                // Finalize: Move from provisional to active storage
                _sceneInstantiator.FinalizeScene(provisionalScene.Id, context);
            }
            else
            {
                // Fallback: Create and immediately finalize if provisional wasn't created
                // This handles non-template-based Situations (old architecture compatibility)
                SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == sceneSpawn.SceneTemplateId);
                if (template != null)
                {
                    global::Scene scene = _sceneInstantiator.CreateProvisionalScene(template, sceneSpawn, context);
                    _sceneInstantiator.FinalizeScene(scene.Id, context);
                }
            }
        }

        // CLEANUP: Delete provisional Scenes from non-selected actions in this Situation
        // After finalization, selected action's Scenes are in GameWorld.Scenes (no longer provisional)
        // Delete remaining provisional Scenes from same Situation to prevent accumulation
        if (currentSituation != null)
        {
            List<string> unselectedProvisionalScenes = _gameWorld.ProvisionalScenes.Values
                .Where(s => s.SourceSituationId == currentSituation.Id)
                .Select(s => s.Id)
                .ToList();

            foreach (string? sceneId in unselectedProvisionalScenes)
            {
                _sceneInstantiator.DeleteProvisionalScene(sceneId);
            }
        }
    }
}
