using Xunit;

namespace Wayfarer.Tests.Subsystems.Consequence;

/// <summary>
/// Tests for RewardApplicationService - on-demand template generation
/// Critical flow: Scene spawn reward → template lookup → procedural generation if missing
/// </summary>
public class RewardApplicationServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task ApplyChoiceReward_GeneratesProceduralTemplate_WhenMissing()
    {
        // Arrange
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();

        // Start game
        await gameFacade.StartGameAsync();
        Player player = gameWorld.GetPlayer();

        // Create choice reward that spawns A4 (doesn't exist yet)
        ChoiceReward reward = new ChoiceReward
        {
            ScenesToSpawn = new List<SceneSpawnReward>
            {
                new SceneSpawnReward { SceneTemplateId = "a_story_4" }
            }
        };

        // Create mock situation for context (NO Name - entity identified by object reference)
        Situation mockSituation = new Situation
        {
            Type = SituationType.Normal
        };

        // Act - Apply reward (should trigger procedural generation)
        RewardApplicationService rewardService = GetService<RewardApplicationService>();
        await rewardService.ApplyChoiceReward(reward, mockSituation);

        // Assert - Template should be generated
        SceneTemplate generatedTemplate = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 4);

        Assert.NotNull(generatedTemplate);
        Assert.Equal("a_story_4", generatedTemplate.Id);
        Assert.Equal(4, generatedTemplate.MainStorySequence);
        Assert.Equal(StoryCategory.MainStory, generatedTemplate.Category);
    }

    [Fact]
    public async Task ApplyChoiceReward_UsesExistingTemplate_WhenExists()
    {
        // Arrange
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();

        await gameFacade.StartGameAsync();

        // A1 template exists (authored)
        ChoiceReward reward = new ChoiceReward
        {
            ScenesToSpawn = new List<SceneSpawnReward>
            {
                new SceneSpawnReward { SceneTemplateId = "a_story_1" }
            }
        };

        Situation mockSituation = new Situation
        {
            Type = SituationType.Normal
        };

        // Get initial template count
        int initialTemplateCount = gameWorld.SceneTemplates.Count;

        // Act
        RewardApplicationService rewardService = GetService<RewardApplicationService>();
        await rewardService.ApplyChoiceReward(reward, mockSituation);

        // Assert - No new template generated (used existing)
        Assert.Equal(initialTemplateCount, gameWorld.SceneTemplates.Count);
    }

    [Fact]
    public async Task ApplyChoiceReward_LooksBySequenceNumber_NotIdString()
    {
        // Arrange
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();

        await gameFacade.StartGameAsync();

        // A2 exists with custom ID "a2_morning" but mainStorySequence = 2
        // Spawning "a_story_2" should find it by sequence, not ID
        ChoiceReward reward = new ChoiceReward
        {
            ScenesToSpawn = new List<SceneSpawnReward>
            {
                new SceneSpawnReward { SceneTemplateId = "a_story_2" }
            }
        };

        Situation mockSituation = new Situation { Type = SituationType.Normal };

        int initialTemplateCount = gameWorld.SceneTemplates.Count;

        // Act
        RewardApplicationService rewardService = GetService<RewardApplicationService>();
        await rewardService.ApplyChoiceReward(reward, mockSituation);

        // Assert - Found existing A2 by sequence (didn't generate new one)
        Assert.Equal(initialTemplateCount, gameWorld.SceneTemplates.Count);

        // Verify A2 was found
        SceneTemplate a2 = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == 2);
        Assert.NotNull(a2);
        Assert.Equal("a2_morning", a2.Id); // Custom ID preserved
    }
}
