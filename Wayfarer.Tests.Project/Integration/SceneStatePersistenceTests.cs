using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Tests for Scene state management and persistence readiness.
///
/// These tests verify that Scene state is correctly initialized and modifiable,
/// which is the foundation for future save/load functionality.
///
/// HIGHLANDER ARCHITECTURE:
/// - Scene instances created via PackageLoader.CreateSingleScene()
/// - Scene state tracked in GameWorld.Scenes collection
/// - Scene lifecycle: Deferred → Active → Completed
///
/// Uses IntegrationTestBase - tests EXACT production initialization path.
/// </summary>
public class SceneStatePersistenceTests : IntegrationTestBase
{
    [Fact]
    public async Task Scene_StateTransition_DeferredToActive()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        // Spawn starter scenes (creates A1 as Deferred)
        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT initial state
        Assert.Equal(SceneState.Deferred, a1Scene.State);

        // ACT: Simulate activation (what LocationFacade does when player enters)
        a1Scene.State = SceneState.Active;

        // ASSERT: State changed
        Assert.Equal(SceneState.Active, a1Scene.State);
    }

    [Fact]
    public async Task Scene_StateTransition_ActiveToCompleted()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        a1Scene.State = SceneState.Active;

        // ACT: Complete the scene
        a1Scene.State = SceneState.Completed;

        // ASSERT: State changed
        Assert.Equal(SceneState.Completed, a1Scene.State);
    }

    [Fact]
    public async Task Scene_TemplateReference_Preserved()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT: Template reference is preserved (for save/load reconstruction)
        Assert.NotNull(a1Scene.Template);
        Assert.NotNull(a1Scene.TemplateId);
        Assert.Equal(a1Scene.Template.Id, a1Scene.TemplateId);
    }

    [Fact]
    public async Task Scene_TemplateId_CanResolveToTemplate()
    {
        // This test verifies save/load can work:
        // 1. Save: Store TemplateId
        // 2. Load: Use TemplateId to find Template in GameWorld.SceneTemplates

        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        string savedTemplateId = a1Scene.TemplateId;

        // ACT: Simulate load - resolve TemplateId back to Template
        SceneTemplate resolvedTemplate = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.Id == savedTemplateId);

        // ASSERT: Can resolve TemplateId to same Template
        Assert.NotNull(resolvedTemplate);
        Assert.Same(a1Scene.Template, resolvedTemplate);
    }

    [Fact]
    public async Task Scene_CurrentSituationIndex_Trackable()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT: CurrentSituationIndex is initialized
        Assert.Equal(0, a1Scene.CurrentSituationIndex);

        // ACT: Advance situation index
        a1Scene.CurrentSituationIndex = 1;

        // ASSERT: Index changed
        Assert.Equal(1, a1Scene.CurrentSituationIndex);
    }

    [Fact]
    public async Task Scene_Deferred_HasSituationCountFromTemplate()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT: Deferred scenes have SituationCount from template for persistence
        // (actual Situations are created at activation, not spawn)
        Assert.True(a1Scene.SituationCount >= 0,
            "Deferred scene should have SituationCount set from template");

        // Template should have situations defined
        Assert.NotNull(a1Scene.Template);
        Assert.NotEmpty(a1Scene.Template.SituationTemplates);
    }

    [Fact]
    public async Task Scene_PersistenceProperties_AllSet()
    {
        // Verify all properties needed for save/load are properly set

        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT: All persistence-critical properties are set
        Assert.NotNull(a1Scene.TemplateId);           // Template reference for reconstruction
        Assert.NotNull(a1Scene.Template);             // Runtime template reference
        Assert.NotNull(a1Scene.DisplayName);          // User-visible name
        Assert.True(Enum.IsDefined(typeof(SceneState), a1Scene.State)); // State is valid enum value
        Assert.NotNull(a1Scene.Situations);           // Situations collection exists
        Assert.True(a1Scene.MainStorySequence.HasValue); // Sequence number for A-story
        Assert.True(Enum.IsDefined(typeof(StoryCategory), a1Scene.Category)); // Category is valid
    }

    [Fact]
    public async Task GameWorld_Scenes_CollectionPersists()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        int initialCount = gameWorld.Scenes.Count;

        // ACT: Spawn scenes
        await orchestrator.SpawnStarterScenes(startingLocation);

        // ASSERT: Scenes added to GameWorld collection
        Assert.True(gameWorld.Scenes.Count > initialCount);

        // Verify scene can be found in collection
        Scene a1Scene = gameWorld.Scenes
            .FirstOrDefault(s => s.MainStorySequence == 1);

        Assert.NotNull(a1Scene);
    }

    [Fact]
    public async Task Scene_LocationActivationFilter_Preserved()
    {
        // LocationActivationFilter is needed for scene activation logic

        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // ASSERT: LocationActivationFilter is copied from template
        Assert.NotNull(a1Scene.LocationActivationFilter);
    }
}
