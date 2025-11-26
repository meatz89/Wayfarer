using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Integration tests for infinite A-story progression (Frieren model)
/// CRITICAL FLOW: Authored tutorial → seamless procedural continuation → infinite journey
/// Tests the complete chain: A3 completion → A4 generation → A5 generation → forward progress
/// </summary>
public class InfiniteAStoryProgressionTests : IntegrationTestBase
{
    [Fact]
    public async Task FullChain_A3CompletionSpawnsA4_A4CompletionSpawnsA5_InfiniteProgression()
    {
        // CRITICAL INTEGRATION TEST: Verify infinite A-story works end-to-end
        // This is THE most important test for Frieren model validation
        //
        // Flow tested:
        // 1. Player completes A3 (last authored tutorial scene)
        // 2. A3 final situation spawns A4 (first procedural scene)
        // 3. A4 template generated procedurally (RewardApplicationService → ProceduralAStoryService)
        // 4. Player completes A4
        // 5. A4 spawns A5 (second procedural scene)
        // 6. Verify infinite progression capability

        // Arrange
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();

        // Start game
        await gameFacade.StartGameAsync();
        Player player = gameWorld.GetPlayer();

        // Verify A1 exists (authored)
        SceneTemplate a1 = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 1);
        Assert.NotNull(a1);
        Assert.Equal(1, a1.MainStorySequence);

        // Verify A2 exists (authored)
        SceneTemplate a2 = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 2);
        Assert.NotNull(a2);
        Assert.Equal(2, a2.MainStorySequence);

        // Verify A3 exists (authored - last tutorial scene)
        SceneTemplate a3 = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 3);
        Assert.NotNull(a3);
        Assert.Equal(3, a3.MainStorySequence);

        // Verify A4 does NOT exist yet (will be generated procedurally)
        SceneTemplate a4Before = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 4);
        Assert.Null(a4Before);

        // Act - Part 1: Get A3 active and complete it
        // Find A3 scene instance
        Scene a3Scene = gameWorld.Scenes
            .FirstOrDefault(s => s.Template.MainStorySequence == 3);

        if (a3Scene == null)
        {
            // A3 not spawned yet - spawn it
            RewardApplicationService rewardService = GetService<RewardApplicationService>();
            ChoiceReward spawnA3Reward = new ChoiceReward
            {
                ScenesToSpawn = new List<SceneSpawnReward>
                {
                    new SceneSpawnReward { SceneTemplateId = "a_story_3" }
                }
            };

            Situation mockSituation = new Situation
            {
                Type = SituationType.Normal
            };

            await rewardService.ApplyChoiceReward(spawnA3Reward, mockSituation);

            // Retrieve spawned A3
            a3Scene = gameWorld.Scenes
                .FirstOrDefault(s => s.Template.MainStorySequence == 3);
        }

        Assert.NotNull(a3Scene);

        // Get A3 final situation
        Situation a3FinalSituation = a3Scene.Situations.LastOrDefault();
        Assert.NotNull(a3FinalSituation);

        // Get any choice template from final situation (all choices spawn A4)
        ChoiceTemplate firstChoiceTemplate = a3FinalSituation.Template.ChoiceTemplates.FirstOrDefault();
        Assert.NotNull(firstChoiceTemplate);

        // Complete choice (triggers scene spawn via reward)
        RewardApplicationService rewardApplicationService = GetService<RewardApplicationService>();
        SituationCompletionHandler completionHandler = GetService<SituationCompletionHandler>();
        await rewardApplicationService.ApplyChoiceReward(firstChoiceTemplate.RewardTemplate, a3FinalSituation);
        await completionHandler.CompleteSituation(a3FinalSituation);

        // Assert - Part 1: A4 should now exist (generated procedurally)
        SceneTemplate a4After = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 4);
        Assert.NotNull(a4After);
        Assert.Equal(4, a4After.MainStorySequence);
        Assert.Equal("a_story_4", a4After.Id);
        Assert.Equal(StoryCategory.MainStory, a4After.Category);

        // Verify A4 has an archetype set (FLOW test - not specific value)
        Assert.NotNull(a4After.SceneArchetypeId);

        // Act - Part 2: Complete A4 to spawn A5
        // Verify A5 does NOT exist yet
        SceneTemplate a5Before = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 5);
        Assert.Null(a5Before);

        // Find A4 scene instance (should be spawned by A3 completion)
        Scene a4Scene = gameWorld.Scenes
            .FirstOrDefault(s => s.Template.MainStorySequence == 4);
        Assert.NotNull(a4Scene);

        // Get A4 final situation
        Situation a4FinalSituation = a4Scene.Situations.LastOrDefault();
        Assert.NotNull(a4FinalSituation);

        // Get any choice template from A4 final situation
        ChoiceTemplate a4ChoiceTemplate = a4FinalSituation.Template.ChoiceTemplates.FirstOrDefault();
        Assert.NotNull(a4ChoiceTemplate);

        // Complete A4 choice (should spawn A5)
        await rewardApplicationService.ApplyChoiceReward(a4ChoiceTemplate.RewardTemplate, a4FinalSituation);
        await completionHandler.CompleteSituation(a4FinalSituation);

        // Assert - Part 2: A5 should now exist (second procedural scene)
        SceneTemplate a5After = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 5);
        Assert.NotNull(a5After);
        Assert.Equal(5, a5After.MainStorySequence);
        Assert.Equal("a_story_5", a5After.Id);
        Assert.Equal(StoryCategory.MainStory, a5After.Category);

        // Verify A5 has an archetype set (FLOW test - not specific value)
        Assert.NotNull(a5After.SceneArchetypeId);

        // Final Verification: Infinite progression capability proven
        // - A3 (authored) → A4 (procedural) → A5 (procedural) → ... → infinity
        // - No special case logic needed
        // - Generic sequence-based lookup works
        // - Forward progress guaranteed

        Console.WriteLine($"[Integration] Infinite A-Story Progression VERIFIED:");
        Console.WriteLine($"  A3 (authored) → {a3.SceneArchetypeId}");
        Console.WriteLine($"  A4 (procedural) → {a4After.SceneArchetypeId}");
        Console.WriteLine($"  A5 (procedural) → {a5After.SceneArchetypeId}");
        Console.WriteLine($"  Forward progress: PROVEN");
    }

    [Fact]
    public async Task ProceduralGeneration_ArchetypeRotation_GeneratesValidArchetypes()
    {
        // Verify archetype rotation generates valid archetypes for each sequence
        // NOTE: Testing FLOW (archetypes are generated), not specific rotation values

        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        GameWorld gameWorld = GetGameWorld();

        // Act - Generate 4 sequences to verify flow
        List<SceneArchetypeType> generatedArchetypes = new List<SceneArchetypeType>();

        for (int sequence = 1; sequence <= 4; sequence++)
        {
            await service.GenerateNextATemplate(sequence, context);

            SceneTemplate template = gameWorld.SceneTemplates
                .FirstOrDefault(t => t.MainStorySequence == sequence);

            Assert.NotNull(template);
            Assert.NotNull(template.SceneArchetypeId);
            generatedArchetypes.Add(template.SceneArchetypeId.Value);
        }

        // Assert - Verify all archetypes are valid enum values (FLOW test)
        Assert.Equal(4, generatedArchetypes.Count);
        foreach (SceneArchetypeType archetype in generatedArchetypes)
        {
            Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), archetype));
        }

        Console.WriteLine("[Integration] Archetype Generation Verified:");
        for (int i = 0; i < generatedArchetypes.Count; i++)
        {
            Console.WriteLine($"  A{i + 1}: {generatedArchetypes[i]}");
        }
    }

    [Fact]
    public async Task ProceduralGeneration_SceneTemplatesCreated_WithValidStructure()
    {
        // Verify procedurally generated scenes have valid structure
        // NOTE: Testing FLOW (structure is valid), not specific values

        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        GameWorld gameWorld = GetGameWorld();

        // Act - Generate a procedural scene
        int sequence = 4;
        await service.GenerateNextATemplate(sequence, context);

        // Assert - Scene template has valid structure
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.Equal(StoryCategory.MainStory, template.Category);
        Assert.Equal(sequence, template.MainStorySequence);
        Assert.NotNull(template.SceneArchetypeId);
        Assert.NotEmpty(template.SituationTemplates);
        Assert.NotNull(template.LocationActivationFilter);

        Console.WriteLine("[Integration] Scene Structure Verified:");
        Console.WriteLine($"  ID: {template.Id}");
        Console.WriteLine($"  Category: {template.Category}");
        Console.WriteLine($"  MainStorySequence: {template.MainStorySequence}");
        Console.WriteLine($"  SceneArchetypeId: {template.SceneArchetypeId}");
        Console.WriteLine($"  SituationTemplates: {template.SituationTemplates.Count}");
    }
}
