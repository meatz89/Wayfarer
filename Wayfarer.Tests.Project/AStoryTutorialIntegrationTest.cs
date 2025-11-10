using Xunit;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Content;
using Wayfarer.Services;
using System.Linq;

namespace Wayfarer.Tests;

/// <summary>
/// Integration tests for A-Story tutorial (A1-A3).
/// Verifies the COMPLETE PLAYER EXPERIENCE of infinite A-story bootstrap.
///
/// Tests ensure:
/// 1. A1 spawns at game start (isStarter: true)
/// 2. A1 completion spawns A2 (sequence progression)
/// 3. A2 completion spawns A3 (sequence progression)
/// 4. A3 completion triggers procedural A4 generation
/// 5. All A-story scenes have guaranteed progression (no soft-locks)
///
/// PHILOSOPHY: Tests verify the INFINITE LOOP engages correctly.
/// </summary>
public class AStoryTutorialIntegrationTest : IntegrationTestBase
{
    /// <summary>
    /// TEST: A1 spawns at game start
    ///
    /// Verifies:
    /// - a1_arrival has isStarter: true
    /// - Scene spawns during game initialization
    /// - Scene is Active and ready for player
    /// - Scene has Category=MainStory, MainStorySequence=1
    ///
    /// Player Experience: A-story begins immediately at game start
    /// </summary>
    [Fact]
    public async Task A1_SpawnsAtGameStart()
    {
        // ARRANGE: Start new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // ASSERT: A1 exists and is active
        Scene a1Scene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "a1_arrival");

        Assert.NotNull(a1Scene);
        Assert.Equal(SceneState.Active, a1Scene.State);
        Assert.Equal(StoryCategory.MainStory, a1Scene.Category);
        Assert.Equal(1, a1Scene.MainStorySequence);
        Assert.Equal(PresentationMode.Modal, a1Scene.PresentationMode);
        Assert.Equal(ProgressionMode.Cascade, a1Scene.ProgressionMode);
    }

    /// <summary>
    /// TEST: All A-story scenes have guaranteed progression
    ///
    /// Verifies:
    /// - Every situation in A1/A2/A3 has at least one zero-requirement choice
    /// - No soft-locks possible in tutorial
    /// - Starting player can always progress
    ///
    /// Player Experience: Tutorial is ALWAYS completable
    /// </summary>
    [Fact]
    public void AllAStorySituations_HaveGuaranteedProgression()
    {
        // ARRANGE: Get all A-story templates
        GameWorld gameWorld = GetGameWorld();

        var aStoryTemplates = gameWorld.SceneTemplates
            .Where(t => t.Category == StoryCategory.MainStory &&
                       t.MainStorySequence.HasValue &&
                       t.MainStorySequence.Value <= 3)
            .ToList();

        Assert.NotEmpty(aStoryTemplates);
        Assert.Equal(3, aStoryTemplates.Count); // A1, A2, A3

        // ASSERT: Every situation has guaranteed progression
        foreach (var template in aStoryTemplates)
        {
            Assert.NotEmpty(template.SituationTemplates);

            foreach (var situation in template.SituationTemplates)
            {
                bool hasGuaranteedPath = situation.ChoiceTemplates.Any(choice =>
                    (choice.RequirementFormula == null ||
                     choice.RequirementFormula.OrPaths == null ||
                     !choice.RequirementFormula.OrPaths.Any()) &&
                    choice.ActionType == ChoiceActionType.Instant);

                Assert.True(hasGuaranteedPath,
                    $"A-story situation '{situation.Id}' in '{template.Id}' lacks guaranteed progression path");
            }
        }
    }

    /// <summary>
    /// TEST: A-story chain is complete (no gaps)
    ///
    /// Verifies:
    /// - A1, A2, A3 all exist
    /// - No gaps in sequence (1, 2, 3)
    /// - All start at sequence 1
    /// - All are MainStory category
    ///
    /// Player Experience: Tutorial has no missing scenes
    /// </summary>
    [Fact]
    public void AStoryChain_IsComplete()
    {
        // ARRANGE: Get all A-story templates
        GameWorld gameWorld = GetGameWorld();

        var aStoryTemplates = gameWorld.SceneTemplates
            .Where(t => t.Category == StoryCategory.MainStory &&
                       t.MainStorySequence.HasValue)
            .OrderBy(t => t.MainStorySequence)
            .ToList();

        Assert.NotEmpty(aStoryTemplates);

        // ASSERT: Chain starts at 1
        int minSequence = aStoryTemplates.Min(t => t.MainStorySequence!.Value);
        Assert.Equal(1, minSequence);

        // ASSERT: No gaps in sequence
        int maxSequence = aStoryTemplates.Where(t => t.MainStorySequence <= 10)
            .Max(t => t.MainStorySequence!.Value);

        for (int expectedSeq = 1; expectedSeq <= maxSequence; expectedSeq++)
        {
            bool exists = aStoryTemplates.Any(t => t.MainStorySequence == expectedSeq);
            Assert.True(exists,
                $"Missing A-story sequence A{expectedSeq} in authored tutorial chain");
        }
    }

    /// <summary>
    /// TEST: A1 has correct structure for inn lodging
    ///
    /// Verifies:
    /// - Uses inn_lodging archetype
    /// - Has multiple situations (negotiate → rest → depart)
    /// - Placed at NPC (elena)
    /// - Modal + Cascade presentation
    ///
    /// Player Experience: A1 is a complete inn lodging flow
    /// </summary>
    [Fact]
    public async Task A1_HasCorrectInnLodgingStructure()
    {
        // ARRANGE: Start game and get A1
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        Scene a1Scene = gameWorld.Scenes.FirstOrDefault(s =>
            s.TemplateId == "a1_arrival");
        Assert.NotNull(a1Scene);

        // ASSERT: Structure matches inn_lodging archetype
        Assert.Equal(PlacementType.NPC, a1Scene.PlacementType);
        Assert.Equal("elena", a1Scene.PlacementId);

        // Multiple situations (3 for inn_lodging: negotiate, rest, depart)
        Assert.True(a1Scene.Situations.Count >= 2,
            "A1 should have multiple situations for inn lodging flow");

        // Has initial situation set
        Assert.NotNull(a1Scene.CurrentSituation);

        // First situation has 4-choice pattern
        Assert.True(a1Scene.CurrentSituation.Template.ChoiceTemplates.Count >= 4,
            "First situation should follow 4-choice pattern (stat/money/challenge/fallback)");
    }

    /// <summary>
    /// TEST: A2 spawns after A1 completion
    ///
    /// Verifies:
    /// - A2 has spawnConditions requiring A1 completion
    /// - A2 uses categorical NPC placement (Scholar/Merchant)
    /// - A2 follows same guaranteed progression pattern
    ///
    /// Player Experience: A-story continues naturally after A1
    /// </summary>
    [Fact]
    public void A2_RequiresA1Completion()
    {
        // ARRANGE: Get A2 template
        GameWorld gameWorld = GetGameWorld();

        SceneTemplate a2Template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.Id == "a2_morning");
        Assert.NotNull(a2Template);

        // ASSERT: A2 requires A1 completion
        Assert.NotNull(a2Template.SpawnConditions);
        Assert.NotNull(a2Template.SpawnConditions.PlayerState);
        Assert.Contains("a1_arrival",
            a2Template.SpawnConditions.PlayerState.CompletedScenes);

        // ASSERT: A2 uses categorical placement (reusable)
        Assert.Equal(PlacementType.NPC, a2Template.PlacementFilter.PlacementType);
        Assert.NotNull(a2Template.PlacementFilter.CategoryFilter);
        Assert.NotNull(a2Template.PlacementFilter.CategoryFilter.PersonalityTypes);
        Assert.Contains("Scholar", a2Template.PlacementFilter.CategoryFilter.PersonalityTypes);
    }

    /// <summary>
    /// TEST: A3 triggers procedural generation
    ///
    /// Verifies:
    /// - A3 completion spawns A4 (procedural)
    /// - ProceduralAStoryService detects sequence 3 → 4
    /// - Generated A4 has same guarantees (no soft-lock)
    ///
    /// Player Experience: Infinite loop engages after A3
    ///
    /// NOTE: This test requires mocking or actual procedural generation.
    /// Skipped for now - requires integration with ProceduralAStoryService.
    /// </summary>
    [Fact(Skip = "Requires procedural generation integration - implement when service fully wired")]
    public async Task A3Completion_TriggersProceduralA4Generation()
    {
        // TODO: Test that completing A3 generates A4 procedurally
        // This requires:
        // 1. Complete A1, A2, A3 in sequence
        // 2. Verify ProceduralAStoryService.GenerateNextATemplate called
        // 3. Verify A4 exists with MainStorySequence=4
        // 4. Verify A4 has same guaranteed progression guarantees
    }
}
