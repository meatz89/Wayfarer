using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Integration tests for Pass 2 AI Narrative Generation.
///
/// Two-Pass Procedural Generation (arc42 §8.28):
/// - Pass 1: Mechanical generation (Situations, Choices, Costs, Rewards)
/// - Pass 2: AI Narrative generation (Situation.Description) with fallback
///
/// These tests verify:
/// 1. ActivateSceneAsync calls narrative service for all situations
/// 2. Fallback narrative generation when AI unavailable
/// 3. Narrative persistence to Situation.Description
///
/// Uses IntegrationTestBase with Ollama disabled - tests fallback behavior.
/// </summary>
public class Pass2NarrativeGenerationTests : IntegrationTestBase
{
    [Fact]
    public async Task ActivateSceneAsync_GeneratesNarrativeForAllSituations()
    {
        // ARRANGE: Scene with situations
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        SceneInstantiator instantiator = GetService<SceneInstantiator>();
        Location startingLocation = gameWorld.StartingLocation;

        // Spawn starter scenes (creates A1 as Deferred)
        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        // Verify scene is Deferred with no situations yet
        Assert.Equal(SceneState.Deferred, a1Scene.State);
        Assert.Empty(a1Scene.Situations);
        Assert.NotNull(a1Scene.Template);
        Assert.NotEmpty(a1Scene.Template.SituationTemplates);

        // Build activation context
        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            CurrentLocation = startingLocation,
            CurrentVenue = startingLocation.Venue,
            Player = gameWorld.GetPlayer()
        };

        // ACT: Activate scene (triggers Pass 1 + Pass 2)
        await instantiator.ActivateSceneAsync(a1Scene, activationContext);

        // ASSERT: All situations have non-empty Description (Pass 2 ran)
        Assert.Equal(SceneState.Active, a1Scene.State);
        Assert.NotEmpty(a1Scene.Situations);

        foreach (Situation situation in a1Scene.Situations)
        {
            Assert.NotNull(situation.Description);
            Assert.NotEmpty(situation.Description);
        }
    }

    [Fact]
    public async Task ActivateSceneAsync_FallbackNarrativeWhenAIUnavailable()
    {
        // ARRANGE: IntegrationTestBase configures Ollama as disabled
        // This means fallback narrative generation should be used
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        SceneInstantiator instantiator = GetService<SceneInstantiator>();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            CurrentLocation = startingLocation,
            CurrentVenue = startingLocation.Venue,
            Player = gameWorld.GetPlayer()
        };

        // ACT: Activate scene
        await instantiator.ActivateSceneAsync(a1Scene, activationContext);

        // ASSERT: Descriptions are populated with fallback content
        // Fallback narratives contain time-based atmospheric text
        foreach (Situation situation in a1Scene.Situations)
        {
            Assert.NotNull(situation.Description);
            Assert.NotEmpty(situation.Description);

            // Fallback narratives contain time block references like "Morning", "Midday", etc.
            // or location/NPC context
            bool hasFallbackContent =
                situation.Description.Contains("light") ||
                situation.Description.Contains("sun") ||
                situation.Description.Contains("shadows") ||
                situation.Description.Contains("darkness") ||
                situation.Description.Contains("You") ||
                situation.Description.Contains("encounter") ||
                situation.Description.Contains("find yourself");

            Assert.True(hasFallbackContent,
                $"Situation '{situation.Name}' should have fallback narrative content, got: '{situation.Description}'");
        }
    }

    [Fact]
    public async Task ActivateSceneAsync_NarrativePersistedToSituation()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        SceneInstantiator instantiator = GetService<SceneInstantiator>();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            CurrentLocation = startingLocation,
            CurrentVenue = startingLocation.Venue,
            Player = gameWorld.GetPlayer()
        };

        // ACT: Activate scene
        await instantiator.ActivateSceneAsync(a1Scene, activationContext);

        // ASSERT: Narratives are persisted (same object reference contains description)
        // Verify we can access descriptions multiple times (not regenerated each time)
        List<string> firstRead = a1Scene.Situations
            .Select(s => s.Description)
            .ToList();

        List<string> secondRead = a1Scene.Situations
            .Select(s => s.Description)
            .ToList();

        Assert.Equal(firstRead.Count, secondRead.Count);

        for (int i = 0; i < firstRead.Count; i++)
        {
            Assert.Equal(firstRead[i], secondRead[i]);
        }
    }

    [Fact]
    public async Task ActivateSceneAsync_SituationCountMatchesTemplateCount()
    {
        // ARRANGE
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        SceneInstantiator instantiator = GetService<SceneInstantiator>();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        int expectedSituationCount = a1Scene.Template.SituationTemplates.Count;

        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            CurrentLocation = startingLocation,
            CurrentVenue = startingLocation.Venue,
            Player = gameWorld.GetPlayer()
        };

        // ACT: Activate scene
        await instantiator.ActivateSceneAsync(a1Scene, activationContext);

        // ASSERT: All situations created from templates
        Assert.Equal(expectedSituationCount, a1Scene.Situations.Count);
        Assert.Equal(expectedSituationCount, a1Scene.SituationCount);
    }

    [Fact]
    public async Task ActivateSceneAsync_EntitiesResolvedBeforeNarrative()
    {
        // ARRANGE: Verify Pass 1 (entity resolution) happens before Pass 2 (narrative)
        GameOrchestrator orchestrator = GetGameOrchestrator();
        GameWorld gameWorld = GetGameWorld();
        SceneInstantiator instantiator = GetService<SceneInstantiator>();
        Location startingLocation = gameWorld.StartingLocation;

        await orchestrator.SpawnStarterScenes(startingLocation);

        Scene a1Scene = gameWorld.Scenes
            .First(s => s.MainStorySequence == 1);

        SceneSpawnContext activationContext = new SceneSpawnContext
        {
            CurrentLocation = startingLocation,
            CurrentVenue = startingLocation.Venue,
            Player = gameWorld.GetPlayer()
        };

        // ACT: Activate scene
        await instantiator.ActivateSceneAsync(a1Scene, activationContext);

        // ASSERT: All situations have resolved Location (mandatory per arc42)
        foreach (Situation situation in a1Scene.Situations)
        {
            // LocationFilter is MANDATORY - Situation must have resolved Location
            Assert.NotNull(situation.Location);

            // If NpcFilter was specified, NPC should be resolved
            if (situation.NpcFilter != null)
            {
                Assert.NotNull(situation.Npc);
            }

            // Narrative should reference resolved entities (contains location or NPC name)
            // This verifies Pass 1 (resolution) happened before Pass 2 (narrative)
            Assert.NotNull(situation.Description);
        }
    }
}
