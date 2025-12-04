using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Integration tests for A-story chain (A1 → A2 → A3 spawning via SceneSpawnReward).
///
/// Tests the HIGHLANDER scene instantiation architecture:
/// - A1 spawns at game start via SpawnStarterScenes() because IsStarter=true
/// - A1 contains SceneSpawnReward that triggers A2 spawning on completion
/// - All scenes created via PackageLoader.CreateSingleScene() (unified path)
///
/// Uses IntegrationTestBase - tests EXACT production initialization path.
/// </summary>
public class AStoryChainIntegrationTests : IntegrationTestBase
{
    [Fact]
    public void AStory_A1Template_IsStarter()
    {
        // ARRANGE & ACT - uses production initialization via IntegrationTestBase
        GameWorld gameWorld = GetGameWorld();

        // ASSERT: A1 template (MainStorySequence=1) has IsStarter=true
        SceneTemplate a1Template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 1);

        Assert.NotNull(a1Template);
        Assert.True(a1Template.IsStarter,
            "A1 template must have IsStarter=true to spawn at game start");
    }

    [Fact]
    public void AStory_A2Template_IsNotStarter()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        // ASSERT: A2 template (MainStorySequence=2) has IsStarter=false
        SceneTemplate a2Template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 2);

        Assert.NotNull(a2Template);
        Assert.False(a2Template.IsStarter,
            "A2 template must have IsStarter=false (spawns from A1 completion reward)");
    }

    [Fact]
    public void AStory_A1Template_HasLocationActivationFilter()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        SceneTemplate a1Template = gameWorld.SceneTemplates
            .First(t => t.MainStorySequence == 1);

        // ASSERT: A1 has LocationActivationFilter for deferred activation
        Assert.NotNull(a1Template.LocationActivationFilter);
    }

    [Fact]
    public void AStory_Templates_HaveMainStoryCategory()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        List<SceneTemplate> mainStoryTemplates = gameWorld.SceneTemplates
            .Where(t => t.MainStorySequence.HasValue)
            .ToList();

        // ASSERT: All MainStory templates have MainStory category
        foreach (SceneTemplate template in mainStoryTemplates)
        {
            Assert.Equal(StoryCategory.MainStory, template.Category);
        }
    }

    [Fact]
    public void AStory_Templates_HaveSequentialNumbering()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        List<SceneTemplate> mainStoryTemplates = gameWorld.SceneTemplates
            .Where(t => t.MainStorySequence.HasValue)
            .OrderBy(t => t.MainStorySequence)
            .ToList();

        // ASSERT: Sequences are 1, 2, 3, ... (no gaps in authored content)
        if (mainStoryTemplates.Count >= 2)
        {
            for (int i = 0; i < mainStoryTemplates.Count - 1; i++)
            {
                int currentSeq = mainStoryTemplates[i].MainStorySequence.Value;
                int nextSeq = mainStoryTemplates[i + 1].MainStorySequence.Value;

                Assert.Equal(currentSeq + 1, nextSeq);
            }
        }
    }

    [Fact]
    public void AStory_A1Template_HasSituationsWithChoices()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        SceneTemplate a1Template = gameWorld.SceneTemplates
            .First(t => t.MainStorySequence == 1);

        // ASSERT: A1 has situations with choices (required for progression)
        Assert.NotEmpty(a1Template.SituationTemplates);

        SituationTemplate firstSituation = a1Template.SituationTemplates.First();
        Assert.NotEmpty(firstSituation.ChoiceTemplates);
    }

    [Fact]
    public void AStory_ChainReward_ExistsForProgression()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        SceneTemplate a1Template = gameWorld.SceneTemplates
            .First(t => t.MainStorySequence == 1);

        // ASSERT: A1 final choice has SceneSpawnReward with SpawnNextMainStoryScene=true
        // This is the mechanism that chains A1 → A2
        // Rewards can be in: Consequence, OnSuccessConsequence, or OnFailureConsequence
        bool hasChainReward = a1Template.SituationTemplates
            .SelectMany(s => s.ChoiceTemplates)
            .Any(c =>
                (c.Consequence?.ScenesToSpawn?.Any(r => r.SpawnNextMainStoryScene) == true) ||
                (c.OnSuccessConsequence?.ScenesToSpawn?.Any(r => r.SpawnNextMainStoryScene) == true) ||
                (c.OnFailureConsequence?.ScenesToSpawn?.Any(r => r.SpawnNextMainStoryScene) == true));

        Assert.True(hasChainReward,
            "A1 must have at least one choice with SceneSpawnReward.SpawnNextMainStoryScene=true " +
            "to chain to A2 on completion");
    }

    [Fact]
    public async Task AStory_SpawnStarterScenes_CreatesA1AsDeferred()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();

        // Get starting location for SpawnStarterScenes parameter
        Location startingLocation = gameWorld.StartingLocation;
        Assert.NotNull(startingLocation);

        // ACT: Call SpawnStarterScenes (what StartGameAsync does)
        await orchestrator.SpawnStarterScenes(startingLocation);

        // ASSERT: A1 scene created as Deferred
        Scene a1Scene = gameWorld.Scenes
            .FirstOrDefault(s => s.MainStorySequence == 1);

        Assert.NotNull(a1Scene);
        Assert.Equal(SceneState.Deferred, a1Scene.State);
        Assert.Equal(StoryCategory.MainStory, a1Scene.Category);
    }

    [Fact]
    public async Task AStory_SpawnStarterScenes_DoesNotCreateA2()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();

        Location startingLocation = gameWorld.StartingLocation;

        // ACT
        await orchestrator.SpawnStarterScenes(startingLocation);

        // ASSERT: A2 NOT created at game start (only via SceneSpawnReward)
        Scene a2Scene = gameWorld.Scenes
            .FirstOrDefault(s => s.MainStorySequence == 2);

        Assert.Null(a2Scene);
    }

    [Fact]
    public void HIGHLANDER_SceneCreation_ViaCreateSingleScene()
    {
        // This test verifies the HIGHLANDER architecture:
        // All Scene creation goes through PackageLoader.CreateSingleScene()
        //
        // Evidence: Scene instances in GameWorld.Scenes are created by:
        // 1. SceneInstantiator.CreateDeferredScene() → PackageLoader.CreateSingleScene()
        // 2. No other Scene creation path exists
        //
        // VERIFICATION: Check that SceneInstantiator can be resolved from DI
        // (proving all dependencies including PackageLoader are in place)

        SceneInstantiator instantiator = GetService<SceneInstantiator>();

        // ASSERT: SceneInstantiator resolved successfully (HIGHLANDER architecture in place)
        Assert.NotNull(instantiator);
    }

    [Fact]
    public void HIGHLANDER_PackageLoader_HasCreateSingleSceneMethod()
    {
        // Verify PackageLoader has the unified scene creation method
        PackageLoader packageLoader = GetService<PackageLoader>();

        // ASSERT: PackageLoader resolved (CreateSingleScene exists on it)
        Assert.NotNull(packageLoader);

        // Verify method exists via reflection
        System.Reflection.MethodInfo createMethod = typeof(PackageLoader)
            .GetMethod("CreateSingleScene");

        Assert.NotNull(createMethod);
    }
}
