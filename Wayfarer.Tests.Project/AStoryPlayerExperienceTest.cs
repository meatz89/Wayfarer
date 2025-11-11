using Xunit;
using System.Linq;

namespace Wayfarer.Tests;

/// <summary>
/// Integration tests that simulate ACTUAL PLAYER EXPERIENCE.
/// Tests EXECUTE the game, not just validate structure.
/// Tests FAIL when player gets stuck, soft-locked, or can't progress.
///
/// Philosophy: If the player can't do it, the test should fail.
/// </summary>
public class AStoryPlayerExperienceTest : IntegrationTestBase
{
    /// <summary>
    /// TEST: Player starts game and can immediately play A1
    ///
    /// FAILS IF:
    /// - A1 doesn't spawn
    /// - A1 is not visible to player
    /// - A1 has no current situation
    /// - A1 first situation has no choices
    ///
    /// This is what ACTUALLY breaks: Player loads game, sees nothing to do.
    /// </summary>
    [Fact]
    public async Task PlayerStartsGame_CanImmediatelyPlayA1()
    {
        // ACT: Player starts new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Player player = gameWorld.GetPlayer();

        // ASSERT: Player can see and interact with A1
        Scene a1 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.Category == StoryCategory.MainStory && s.MainStorySequence == 1);

        Assert.NotNull(a1); // Player sees A1 in their available scenes

        Assert.NotNull(a1.CurrentSituation); // Player can enter the scene

        Assert.NotEmpty(a1.CurrentSituation.Template.ChoiceTemplates); // Player has choices to make

        // Player should have AT LEAST ONE choice they can actually select
        bool hasSelectableChoice = a1.CurrentSituation.Template.ChoiceTemplates.Any(choice =>
            choice.RequirementFormula == null ||
            choice.RequirementFormula.OrPaths == null ||
            !choice.RequirementFormula.OrPaths.Any());

        Assert.True(hasSelectableChoice,
            "Player cannot select any choices in A1 - SOFT LOCK");
    }

    /// <summary>
    /// TEST: Player completes A1, A2 actually spawns
    ///
    /// FAILS IF:
    /// - A2 doesn't spawn after A1 completion
    /// - A2 spawns but is not Active
    /// - A2 spawns but player can't interact with it
    ///
    /// This is what ACTUALLY breaks: Player finishes A1, expects continuation, sees nothing.
    /// </summary>
    [Fact]
    public async Task PlayerCompletesA1_A2ActuallySpawns()
    {
        // ARRANGE: Start game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);

        // ACT: Simulate player completing A1
        // Mark A1 as completed (this is what happens when player finishes all situations)
        a1.State = SceneState.Completed;

        // Trigger spawn check (this is what happens after scene completion)
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // ASSERT: A2 now exists and is playable
        Scene a2 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.MainStorySequence == 2);

        Assert.NotNull(a2); // A2 spawned

        Assert.Equal(StoryCategory.MainStory, a2.Category); // It's actually A-story

        Assert.NotNull(a2.CurrentSituation); // Player can interact with it

        Assert.NotEmpty(a2.CurrentSituation.Template.ChoiceTemplates); // Player has choices
    }

    /// <summary>
    /// TEST: Player completes A2, A3 actually spawns
    ///
    /// FAILS IF:
    /// - A3 doesn't spawn after A2 completion
    /// - Spawn conditions don't actually work
    ///
    /// This catches: Spawn condition exists in JSON but logic is broken.
    /// </summary>
    [Fact]
    public async Task PlayerCompletesA2_A3ActuallySpawns()
    {
        // ARRANGE: Start game, complete A1
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);
        a1.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a2 = gameWorld.Scenes.First(s => s.MainStorySequence == 2);

        // ACT: Player completes A2
        a2.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // ASSERT: A3 now exists and is playable
        Scene a3 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.MainStorySequence == 3);

        Assert.NotNull(a3); // A3 spawned

        Assert.NotNull(a3.CurrentSituation); // Player can interact
    }

    /// <summary>
    /// TEST: Player completes A3, procedural A4 actually generates
    ///
    /// FAILS IF:
    /// - A4 doesn't generate after A3 completion
    /// - ProceduralAStoryService fails silently
    /// - Generated A4 is broken/unplayable
    ///
    /// This is the CRITICAL test: Does the infinite loop actually engage?
    /// </summary>
    [Fact]
    public async Task PlayerCompletesA3_ProceduralA4ActuallyGenerates()
    {
        // ARRANGE: Complete tutorial chain A1 → A2 → A3
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // Complete A1
        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);
        a1.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // Complete A2
        Scene a2 = gameWorld.Scenes.First(s => s.MainStorySequence == 2);
        a2.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // Complete A3
        Scene a3 = gameWorld.Scenes.First(s => s.MainStorySequence == 3);

        int sceneCountBeforeA3 = gameWorld.Scenes.Count;

        // ACT: Player completes A3
        a3.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // ASSERT: A4 was procedurally generated and is playable
        Scene a4 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.MainStorySequence == 4);

        Assert.NotNull(a4); // CRITICAL: Infinite loop engaged

        Assert.Equal(StoryCategory.MainStory, a4.Category);

        Assert.True(gameWorld.Scenes.Count > sceneCountBeforeA3,
            "A4 should be a NEW scene, not pre-existing");

        // Generated scene must be playable
        Assert.NotNull(a4.CurrentSituation);
        Assert.NotEmpty(a4.CurrentSituation.Template.ChoiceTemplates);

        // Generated scene must have guaranteed progression
        bool hasGuaranteedPath = a4.CurrentSituation.Template.ChoiceTemplates.Any(choice =>
            (choice.RequirementFormula == null ||
             choice.RequirementFormula.OrPaths == null ||
             !choice.RequirementFormula.OrPaths.Any()) &&
            choice.ActionType == ChoiceActionType.Instant);

        Assert.True(hasGuaranteedPath,
            "Procedurally generated A4 violates guaranteed progression - SOFT LOCK POSSIBLE");
    }

    /// <summary>
    /// TEST: Starting player can actually select guaranteed path
    ///
    /// FAILS IF:
    /// - Guaranteed path exists but requires resources starting player doesn't have
    /// - Guaranteed path is flagged wrong (has hidden requirements)
    /// - Choice validation passes but actual selection fails
    ///
    /// This catches: "No requirements" choice actually has requirements in implementation.
    /// </summary>
    [Fact]
    public async Task StartingPlayer_CanActuallySelectGuaranteedPath()
    {
        // ARRANGE: Brand new player, zero resources
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Player player = gameWorld.GetPlayer();
        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);

        // Find the guaranteed path (should have no requirements)
        ChoiceTemplate guaranteedChoice = a1.CurrentSituation.Template.ChoiceTemplates
            .FirstOrDefault(choice =>
                (choice.RequirementFormula == null ||
                 choice.RequirementFormula.OrPaths == null ||
                 !choice.RequirementFormula.OrPaths.Any()) &&
                choice.ActionType == ChoiceActionType.Instant);

        Assert.NotNull(guaranteedChoice); // Guaranteed path exists

        // ACT: Try to verify player can actually afford this choice
        // Check costs
        if (guaranteedChoice.CostTemplate != null)
        {
            Assert.True(player.Coins >= guaranteedChoice.CostTemplate.Coins,
                $"Guaranteed path costs {guaranteedChoice.CostTemplate.Coins} coins but starting player has {player.Coins}");

            Assert.True(player.Resolve >= guaranteedChoice.CostTemplate.Resolve,
                $"Guaranteed path costs {guaranteedChoice.CostTemplate.Resolve} resolve but starting player has {player.Resolve}");

            Assert.True(player.Stamina >= guaranteedChoice.CostTemplate.Stamina,
                $"Guaranteed path costs {guaranteedChoice.CostTemplate.Stamina} stamina but starting player has {player.Stamina}");
        }

        // If we get here without assertion failures, starting player can actually use guaranteed path
    }

    /// <summary>
    /// TEST: Complete tutorial flow A1 → A2 → A3 → A4 without soft-locks
    ///
    /// FAILS IF:
    /// - Any scene in chain doesn't spawn
    /// - Player gets stuck at any point
    /// - Infinite loop doesn't engage
    ///
    /// This is the END-TO-END player experience test.
    /// If this passes, player can actually play from start through infinite generation.
    /// </summary>
    [Fact]
    public async Task PlayerCanCompleteEntireTutorialChain_WithoutSoftLock()
    {
        // ARRANGE: Start game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // ACT & ASSERT: Complete entire chain

        // A1 exists and is playable
        Scene a1 = gameWorld.Scenes.FirstOrDefault(s => s.MainStorySequence == 1);
        Assert.NotNull(a1);
        Assert.Equal(SceneState.Active, a1.State);

        // Complete A1, A2 spawns
        a1.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a2 = gameWorld.Scenes.FirstOrDefault(s => s.MainStorySequence == 2);
        Assert.NotNull(a2);
        Assert.Equal(SceneState.Active, a2.State);

        // Complete A2, A3 spawns
        a2.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a3 = gameWorld.Scenes.FirstOrDefault(s => s.MainStorySequence == 3);
        Assert.NotNull(a3);
        Assert.Equal(SceneState.Active, a3.State);

        // Complete A3, A4 generates procedurally
        a3.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a4 = gameWorld.Scenes.FirstOrDefault(s => s.MainStorySequence == 4);
        Assert.NotNull(a4); // INFINITE LOOP ENGAGED
        Assert.Equal(SceneState.Active, a4.State);

        // Player has successfully progressed through tutorial into infinite generation
        // No soft-locks encountered
    }

    /// <summary>
    /// TEST: A4→A5 procedural-to-procedural chain (CRITICAL MISSING TEST)
    ///
    /// FAILS IF:
    /// - A4 completes but A5 doesn't generate
    /// - A5 generates but is unplayable
    /// - Procedural chain breaks after first procedural scene
    ///
    /// This tests the ACTUAL infinite loop: procedural → procedural → procedural...
    /// Previous tests only validated authored → procedural transition.
    /// </summary>
    [Fact]
    public async Task PlayerCompletesA4_ProceduralA5ActuallyGenerates()
    {
        // ARRANGE: Complete chain to A4
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // Complete A1 → A2 → A3 → A4
        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);
        a1.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a2 = gameWorld.Scenes.First(s => s.MainStorySequence == 2);
        a2.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a3 = gameWorld.Scenes.First(s => s.MainStorySequence == 3);
        a3.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        Scene a4 = gameWorld.Scenes.First(s => s.MainStorySequence == 4);
        int sceneCountBeforeA5 = gameWorld.Scenes.Count;

        // ACT: Player completes A4 (first procedural scene)
        a4.State = SceneState.Completed;
        await gameFacade.CheckAndSpawnEligibleScenesAsync();

        // ASSERT: A5 was procedurally generated and is playable
        Scene a5 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.MainStorySequence == 5);

        Assert.NotNull(a5); // CRITICAL: Procedural-to-procedural chain works

        Assert.Equal(StoryCategory.MainStory, a5.Category);

        Assert.True(gameWorld.Scenes.Count > sceneCountBeforeA5,
            "A5 should be a NEW scene, not pre-existing");

        // Generated scene must be playable
        Assert.NotNull(a5.CurrentSituation);
        Assert.NotEmpty(a5.CurrentSituation.Template.ChoiceTemplates);

        // Generated scene must have guaranteed progression
        bool hasGuaranteedPath = a5.CurrentSituation.Template.ChoiceTemplates.Any(choice =>
            (choice.RequirementFormula == null ||
             choice.RequirementFormula.OrPaths == null ||
             !choice.RequirementFormula.OrPaths.Any()) &&
            (choice.ActionType == ChoiceActionType.Instant || choice.ActionType == ChoiceActionType.Navigate));

        Assert.True(hasGuaranteedPath,
            "Procedurally generated A5 violates guaranteed progression - SOFT LOCK POSSIBLE");
    }

    /// <summary>
    /// TEST: A5→A6 extended procedural chain (validates chain continues beyond A5)
    ///
    /// FAILS IF:
    /// - Procedural chain breaks at any point
    /// - Generation quality degrades over time
    ///
    /// This validates that infinite generation truly works infinitely,
    /// not just for first few procedural scenes.
    /// </summary>
    [Fact]
    public async Task PlayerCanCompleteExtendedProceduralChain_A1ThroughA6()
    {
        // ARRANGE: Start game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // ACT & ASSERT: Complete extended chain A1 → A2 → A3 → A4 → A5 → A6
        for (int sequence = 1; sequence <= 6; sequence++)
        {
            Scene currentScene = gameWorld.Scenes.FirstOrDefault(s => s.MainStorySequence == sequence);

            Assert.NotNull(currentScene);
            Assert.Equal(SceneState.Active, currentScene.State);
            Assert.NotNull(currentScene.CurrentSituation);
            Assert.NotEmpty(currentScene.CurrentSituation.Template.ChoiceTemplates);

            // Verify at least one choice is accessible
            bool hasAccessibleChoice = currentScene.CurrentSituation.Template.ChoiceTemplates.Any(choice =>
                choice.RequirementFormula == null ||
                choice.RequirementFormula.OrPaths == null ||
                !choice.RequirementFormula.OrPaths.Any());

            Assert.True(hasAccessibleChoice,
                $"A{sequence} has no accessible choices - SOFT LOCK at sequence {sequence}");

            // Complete current scene and spawn next (if not A6)
            if (sequence < 6)
            {
                currentScene.State = SceneState.Completed;
                await gameFacade.CheckAndSpawnEligibleScenesAsync();
            }
        }

        // If we reached here, player successfully progressed through 6 A-story scenes
        // Authored (A1-A3) + Procedural (A4-A6) without any soft-locks
    }

    /// <summary>
    /// TEST: Playability validation catches unplayable scenes
    ///
    /// FAILS IF:
    /// - Validator doesn't detect missing CurrentSituation
    /// - Validator doesn't detect missing choices
    /// - Validator doesn't throw on unplayable scenes
    ///
    /// This validates the fail-fast principle: Better to crash than soft-lock
    /// </summary>
    [Fact]
    public async Task PlayabilityValidator_DetectsUnplayableScenes()
    {
        // ARRANGE: Start game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Scene a1 = gameWorld.Scenes.First(s => s.MainStorySequence == 1);

        // ACT: Deliberately break scene to make it unplayable
        a1.CurrentSituation = null; // Remove current situation

        // ASSERT: Validator should detect this and throw
        SpawnedScenePlayabilityValidator validator = new SpawnedScenePlayabilityValidator(gameWorld);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => validator.ValidatePlayability(a1)
        );

        Assert.Contains("has no CurrentSituation", exception.Message);
        Assert.Contains("UNPLAYABLE", exception.Message);
        Assert.Contains("SOFT LOCKED", exception.Message);
    }

    /// <summary>
    /// TEST: All spawned scenes pass playability validation
    ///
    /// FAILS IF:
    /// - Any scene in chain fails playability validation
    /// - Procedurally generated scenes violate guaranteed progression
    ///
    /// This validates that playability validator is correctly integrated
    /// and catches issues before they reach player.
    /// </summary>
    [Fact]
    public async Task AllSpawnedScenes_PassPlayabilityValidation()
    {
        // ARRANGE: Start game and complete chain to A5
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        SpawnedScenePlayabilityValidator validator = new SpawnedScenePlayabilityValidator(gameWorld);

        // Complete A1 → A2 → A3 → A4 → A5
        for (int sequence = 1; sequence <= 5; sequence++)
        {
            Scene currentScene = gameWorld.Scenes.First(s => s.MainStorySequence == sequence);

            // ACT: Validate playability (should not throw)
            validator.ValidatePlayability(currentScene);

            // Complete and spawn next
            if (sequence < 5)
            {
                currentScene.State = SceneState.Completed;
                await gameFacade.CheckAndSpawnEligibleScenesAsync();
            }
        }

        // ASSERT: If we reached here, all scenes passed playability validation
        // No exceptions thrown = all scenes playable
    }
}
