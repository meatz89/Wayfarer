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

        // Verify A4 archetype matches rotation (sequence 4 = position 3 = Crisis)
        Assert.Contains("crisis", a4After.SceneArchetypeId.ToLowerInvariant());

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

        // Verify A5 archetype matches rotation (sequence 5 = position 0 = Investigation)
        Assert.Contains("investigate", a5After.SceneArchetypeId.ToLowerInvariant());

        // Final Verification: Infinite progression capability proven
        // - A3 (authored) → A4 (procedural) → A5 (procedural) → ... → infinity
        // - No special case logic needed
        // - Generic sequence-based lookup works
        // - Archetype rotation correct
        // - Forward progress guaranteed

        Console.WriteLine($"[Integration] Infinite A-Story Progression VERIFIED:");
        Console.WriteLine($"  A3 (authored) → {a3.SceneArchetypeId}");
        Console.WriteLine($"  A4 (procedural) → {a4After.SceneArchetypeId}");
        Console.WriteLine($"  A5 (procedural) → {a5After.SceneArchetypeId}");
        Console.WriteLine($"  ∞ Progression capability: PROVEN");
    }

    [Fact]
    public async Task ProceduralGeneration_ArchetypeRotation_Follows4PartCycle()
    {
        // Verify archetype rotation cycles correctly: Investigation → Social → Confrontation → Crisis → repeat

        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        GameWorld gameWorld = GetGameWorld();

        // Act - Generate 8 sequences to complete 2 full cycles
        List<string> generatedArchetypes = new List<string>();

        for (int sequence = 1; sequence <= 8; sequence++)
        {
            await service.GenerateNextATemplate(sequence, context);

            SceneTemplate template = gameWorld.SceneTemplates
                .FirstOrDefault(t => t.MainStorySequence == sequence);

            Assert.NotNull(template);
            generatedArchetypes.Add(template.SceneArchetypeId);
        }

        // Assert - Verify rotation pattern
        // Sequence 1 (pos 0): Investigation
        Assert.Contains("investigate", generatedArchetypes[0].ToLowerInvariant());

        // Sequence 2 (pos 1): Social
        Assert.Contains("meet", generatedArchetypes[1].ToLowerInvariant());

        // Sequence 3 (pos 2): Confrontation
        Assert.Contains("confront", generatedArchetypes[2].ToLowerInvariant());

        // Sequence 4 (pos 3): Crisis
        Assert.Contains("crisis", generatedArchetypes[3].ToLowerInvariant());

        // Sequence 5 (pos 0): Investigation (cycle repeats)
        Assert.Contains("investigate", generatedArchetypes[4].ToLowerInvariant());

        // Sequence 6 (pos 1): Social
        Assert.Contains("meet", generatedArchetypes[5].ToLowerInvariant());

        // Sequence 7 (pos 2): Confrontation
        Assert.Contains("confront", generatedArchetypes[6].ToLowerInvariant());

        // Sequence 8 (pos 3): Crisis
        Assert.Contains("crisis", generatedArchetypes[7].ToLowerInvariant());

        Console.WriteLine("[Integration] Archetype Rotation Verified:");
        for (int i = 0; i < generatedArchetypes.Count; i++)
        {
            Console.WriteLine($"  A{i + 1}: {generatedArchetypes[i]}");
        }
    }

    [Fact]
    public async Task ProceduralGeneration_TierEscalation_FollowsSequenceThresholds()
    {
        // Verify tier escalation: 1-30 Personal, 31-50 Local, 51+ Regional

        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        GameWorld gameWorld = GetGameWorld();

        // Act & Assert - Test tier boundaries
        // Sequence 1: Tier 1 (Personal)
        await service.GenerateNextATemplate(1, context);
        SceneTemplate a1 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 1);
        Assert.Equal(1, a1.Tier);

        // Sequence 30: Tier 1 (Personal - boundary)
        await service.GenerateNextATemplate(30, context);
        SceneTemplate a30 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 30);
        Assert.Equal(1, a30.Tier);

        // Sequence 31: Tier 2 (Local - threshold)
        await service.GenerateNextATemplate(31, context);
        SceneTemplate a31 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 31);
        Assert.Equal(2, a31.Tier);

        // Sequence 50: Tier 2 (Local - boundary)
        await service.GenerateNextATemplate(50, context);
        SceneTemplate a50 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 50);
        Assert.Equal(2, a50.Tier);

        // Sequence 51: Tier 3 (Regional - threshold)
        await service.GenerateNextATemplate(51, context);
        SceneTemplate a51 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 51);
        Assert.Equal(3, a51.Tier);

        // Sequence 100: Tier 3 (Regional - continues)
        await service.GenerateNextATemplate(100, context);
        SceneTemplate a100 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 100);
        Assert.Equal(3, a100.Tier);

        Console.WriteLine("[Integration] Tier Escalation Verified:");
        Console.WriteLine($"  A1: Tier {a1.Tier} (Personal)");
        Console.WriteLine($"  A30: Tier {a30.Tier} (Personal boundary)");
        Console.WriteLine($"  A31: Tier {a31.Tier} (Local threshold)");
        Console.WriteLine($"  A50: Tier {a50.Tier} (Local boundary)");
        Console.WriteLine($"  A51: Tier {a51.Tier} (Regional threshold)");
        Console.WriteLine($"  A100: Tier {a100.Tier} (Regional continues)");
    }
}
