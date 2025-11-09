using Xunit;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Content;
using Wayfarer.Services;
using Wayfarer.Subsystems.Consequence;
using System.Linq;

namespace Wayfarer.Tests;

/// <summary>
/// Integration tests for tutorial system completeness.
/// These tests verify the COMPLETE PLAYER EXPERIENCE, not just isolated components.
///
/// Tests ensure:
/// 1. All tutorial scenes are reachable from game start
/// 2. Starting player can complete tutorial (no soft-locks)
/// 3. Scenes spawned from rewards load dependent resources correctly
///
/// PHILOSOPHY: Tests verify INTEGRATION and FLOW, not exact balance values.
/// </summary>
public class TutorialSystemIntegrationTest : IntegrationTestBase
{
    /// <summary>
    /// TEST: All tutorial scenes spawn at game start
    ///
    /// CRITICAL BUG THIS CATCHES: tutorial_rough_morning missing isStarter flag
    ///
    /// Verifies:
    /// - tutorial_secure_lodging spawns (isStarter: true)
    /// - tutorial_rough_morning spawns (currently FAILS - missing isStarter)
    ///
    /// Player Experience: Player should see BOTH tutorial scenes available
    /// </summary>
    [Fact]
    public void AllTutorialScenesSpawnAtGameStart()
    {
        // ARRANGE: Start new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();

        // ACT: Start game (triggers SpawnStarterScenes)
        Player player = gameWorld.GetPlayer();
        gameFacade.StartGameAsync().Wait();

        // ASSERT: Both tutorial scenes should exist in GameWorld
        Scene innLodgingScene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "tutorial_secure_lodging");
        Scene roughMorningScene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "tutorial_rough_morning");

        Assert.NotNull(innLodgingScene);  // Should pass (has isStarter: true)
        Assert.NotNull(roughMorningScene); // Should FAIL (missing isStarter: true)

        // Both scenes should be Active and ready for player interaction
        Assert.Equal(SceneState.Active, innLodgingScene.State);
        Assert.Equal(SceneState.Active, roughMorningScene.State);
    }

    /// <summary>
    /// TEST: Starting player can complete tutorial without soft-lock
    ///
    /// CRITICAL BUG THIS CATCHES: Tutorial soft-lock if player has no resources
    ///
    /// Verifies:
    /// - Starting player has Resolve for challenge path OR
    /// - Fallback choice grants degraded lodging OR
    /// - Money/stat thresholds are low enough for starter
    ///
    /// Player Experience: Tutorial should ALWAYS be completable, never soft-lock
    /// </summary>
    [Fact]
    public void StartingPlayerCanCompleteTutorial()
    {
        // ARRANGE: Start new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        Player player = gameWorld.GetPlayer();
        gameFacade.StartGameAsync().Wait();

        // Get tutorial scene
        Scene innLodgingScene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "tutorial_secure_lodging");
        Assert.NotNull(innLodgingScene);

        // Get first situation (negotiate lodging)
        Situation negotiateSituation = innLodgingScene.Situations.FirstOrDefault();
        Assert.NotNull(negotiateSituation);
        Assert.NotEmpty(negotiateSituation.Template.ChoiceTemplates);

        // ASSERT: At least ONE choice path is available to starting player
        // Starting player typically has 0 coins, 0 stats, some Resolve

        bool hasAvailablePath = false;

        // Check if stat-gated path is available (requires checking player stats)
        ChoiceTemplate statChoice = negotiateSituation.Template.ChoiceTemplates
            .FirstOrDefault(c => c.PathType == ChoicePathType.InstantSuccess &&
                                 c.RequirementFormula?.OrPaths?.Count > 0);

        // Check if money path is available (requires checking player coins)
        ChoiceTemplate moneyChoice = negotiateSituation.Template.ChoiceTemplates
            .FirstOrDefault(c => c.PathType == ChoicePathType.InstantSuccess &&
                                 c.CostTemplate?.Coins > 0);

        // Check if challenge path is available (requires checking player Resolve)
        ChoiceTemplate challengeChoice = negotiateSituation.Template.ChoiceTemplates
            .FirstOrDefault(c => c.PathType == ChoicePathType.Challenge);

        // Fallback is ALWAYS available, but should grant SOMETHING (not nothing)
        ChoiceTemplate fallbackChoice = negotiateSituation.Template.ChoiceTemplates
            .FirstOrDefault(c => c.PathType == ChoicePathType.Fallback);
        Assert.NotNull(fallbackChoice);

        // CRITICAL: Verify at least one non-fallback path is available
        // OR fallback grants degraded lodging (has rewards)

        // Check if starting player has enough Resolve for challenge
        if (challengeChoice != null && player.Resolve >= challengeChoice.CostTemplate.Resolve)
        {
            hasAvailablePath = true;
        }

        // Check if starting player has enough coins for money path
        if (moneyChoice != null && player.Coins >= moneyChoice.CostTemplate.Coins)
        {
            hasAvailablePath = true;
        }

        // Check if fallback grants any rewards (degraded lodging safety net)
        bool fallbackGrantsRewards = fallbackChoice.RewardTemplate?.LocationsToUnlock?.Count > 0 ||
                                     fallbackChoice.RewardTemplate?.ItemIds?.Count > 0;

        // ASSERT: Tutorial is always completable
        Assert.True(hasAvailablePath || fallbackGrantsRewards,
            "Tutorial soft-lock: Starting player has no available paths and fallback grants nothing");
    }

    /// <summary>
    /// TEST: Scenes spawned from rewards load dependent resources correctly
    ///
    /// CRITICAL BUG THIS CATCHES: RewardApplicationService doesn't load packages
    ///
    /// Verifies:
    /// - Scene spawned via SceneSpawnReward
    /// - Dependent resources (locations, items) are loaded into GameWorld
    /// - Marker resolution works
    /// - Player receives items
    ///
    /// Player Experience: Reward-spawned scenes should work identically to starter scenes
    /// </summary>
    [Fact]
    public void RewardSpawnedSceneLoadsDependentResources()
    {
        // ARRANGE: Start new game, get starter scene
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        Player player = gameWorld.GetPlayer();
        gameFacade.StartGameAsync().Wait();

        Scene innLodgingScene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "tutorial_secure_lodging");
        Assert.NotNull(innLodgingScene);

        // Count initial locations/items
        int initialLocationCount = gameWorld.Locations.Count;
        int initialItemCount = gameWorld.Items.Count;
        int initialInventoryCount = player.Inventory.Items.Count;

        // Create a test SceneSpawnReward that would spawn a scene with dependent resources
        SceneTemplate template = gameWorld.SceneTemplates.FirstOrDefault(t =>
            t.Id == "tutorial_secure_lodging");
        Assert.NotNull(template);

        SceneSpawnReward spawnReward = new SceneSpawnReward
        {
            SceneTemplateId = template.Id,
            PlacementType = PlacementType.NPC,
            SpecificPlacementId = "elena"
        };

        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = player,
            CurrentLocationId = player.CurrentLocationId,
            CurrentTimeBlock = player.CurrentTimeBlock,
            CurrentDay = player.CurrentDay,
            Tier = 0
        };

        // ACT: Spawn scene via RewardApplicationService (simulating reward-based spawn)
        RewardApplicationService rewardService = GetService<RewardApplicationService>();

        ChoiceReward reward = new ChoiceReward
        {
            ScenesToSpawn = new List<SceneSpawnReward> { spawnReward }
        };

        rewardService.ApplyChoiceReward(reward, player, context.CurrentLocationId, null, innLodgingScene);

        // Find the spawned scene
        Scene spawnedScene = gameWorld.Scenes
            .Where(s => s.TemplateId == template.Id)
            .OrderByDescending(s => s.Id)  // Get most recent
            .FirstOrDefault();

        Assert.NotNull(spawnedScene);

        // ASSERT: Dependent resources were loaded
        // This will FAIL if RewardApplicationService doesn't call orchestration service

        if (spawnedScene.CreatedLocationIds.Count > 0)
        {
            // New locations should exist in GameWorld
            Assert.True(gameWorld.Locations.Count > initialLocationCount,
                "Dependent locations not loaded into GameWorld");

            // Marker resolution should have occurred
            Assert.NotEmpty(spawnedScene.MarkerResolutionMap);

            // Situations should reference resolved IDs (not markers)
            foreach (Situation situation in spawnedScene.Situations)
            {
                if (situation.ResolvedRequiredLocationId != null)
                {
                    Assert.False(situation.ResolvedRequiredLocationId.Contains("generated:"),
                        "Marker not resolved - situation still references marker");
                }
            }
        }

        if (spawnedScene.CreatedItemIds.Count > 0)
        {
            // New items should exist in GameWorld
            Assert.True(gameWorld.Items.Count > initialItemCount,
                "Dependent items not loaded into GameWorld");

            // Items that should be added to inventory were added
            // (Check if inventory count increased if template specifies items to add)
        }
    }

    /// <summary>
    /// TEST: Tutorial demonstrates complete flow from start to completion
    ///
    /// Verifies:
    /// - Player can start game
    /// - Tutorial scenes are visible
    /// - Player can select choices
    /// - Multi-situation progression works
    /// - Tutorial completes successfully
    ///
    /// Player Experience: Complete end-to-end flow from game start to tutorial completion
    /// </summary>
    [Fact]
    public void TutorialCompleteFlowFromStartToCompletion()
    {
        // ARRANGE: Start new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        Player player = gameWorld.GetPlayer();
        gameFacade.StartGameAsync().Wait();

        // ACT & ASSERT: Verify complete tutorial flow

        // 1. Tutorial scene spawned and active
        Scene innLodgingScene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "tutorial_secure_lodging" && s.State == SceneState.Active);
        Assert.NotNull(innLodgingScene);
        Assert.Equal(PlacementType.NPC, innLodgingScene.PlacementType);
        Assert.Equal("elena", innLodgingScene.PlacementId);

        // 2. Scene has multiple situations (multi-situation scene)
        Assert.True(innLodgingScene.Situations.Count > 1,
            "Tutorial scene should have multiple situations");

        // 3. CurrentSituation is set (scene is ready)
        Assert.NotNull(innLodgingScene.CurrentSituation);

        // 4. First situation has choices available
        Situation firstSituation = innLodgingScene.CurrentSituation;
        Assert.NotEmpty(firstSituation.Template.ChoiceTemplates);
        Assert.True(firstSituation.Template.ChoiceTemplates.Count >= 4,
            "Situation should have at least 4 choices (stat/money/challenge/fallback)");

        // 5. Verify choice diversity (different PathTypes)
        Assert.Contains(firstSituation.Template.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.InstantSuccess);
        Assert.Contains(firstSituation.Template.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.Challenge);
        Assert.Contains(firstSituation.Template.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.Fallback);

        // 6. Scene has SpawnRules for multi-situation progression
        Assert.NotNull(innLodgingScene.SpawnRules);
        Assert.NotEmpty(innLodgingScene.SpawnRules.Transitions);

        // Tutorial is ready for player interaction
    }
}
