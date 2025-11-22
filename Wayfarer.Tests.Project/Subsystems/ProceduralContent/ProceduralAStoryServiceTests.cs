using Xunit;

namespace Wayfarer.Tests.Subsystems.ProceduralContent;

/// <summary>
/// Tests for ProceduralAStoryService - infinite A-story generation
/// Critical algorithms: archetype rotation, tier calculation, anti-repetition
/// </summary>
public class ProceduralAStoryServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task GenerateNextATemplate_Sequence4_CreatesTemplateWithCorrectSequence()
    {
        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        int sequence = 4;

        // Act
        string generatedTemplateId = await service.GenerateNextATemplate(sequence, context);

        // Assert
        Assert.NotNull(generatedTemplateId);
        Assert.Equal("a_story_4", generatedTemplateId);

        // Verify template exists in GameWorld
        GameWorld gameWorld = GetGameWorld();
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.Equal(sequence, template.MainStorySequence);
        Assert.Equal("MainStory", template.Category);
    }

    [Fact]
    public async Task GenerateNextATemplate_Sequence11_CreatesTemplateWithCorrectSequence()
    {
        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();
        int sequence = 11;

        // Act
        string generatedTemplateId = await service.GenerateNextATemplate(sequence, context);

        // Assert
        Assert.Equal("a_story_11", generatedTemplateId);

        GameWorld gameWorld = GetGameWorld();
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.Equal(sequence, template.MainStorySequence);
    }

    [Fact]
    public async Task GenerateNextATemplate_MultipleSequences_CreatesDistinctTemplates()
    {
        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();

        // Act - Generate A4, A5, A6
        await service.GenerateNextATemplate(4, context);
        await service.GenerateNextATemplate(5, context);
        await service.GenerateNextATemplate(6, context);

        // Assert
        GameWorld gameWorld = GetGameWorld();
        SceneTemplate a4 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 4);
        SceneTemplate a5 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 5);
        SceneTemplate a6 = gameWorld.SceneTemplates.FirstOrDefault(t => t.MainStorySequence == 6);

        Assert.NotNull(a4);
        Assert.NotNull(a5);
        Assert.NotNull(a6);

        // Verify distinct IDs
        Assert.NotEqual(a4.Id, a5.Id);
        Assert.NotEqual(a5.Id, a6.Id);
        Assert.NotEqual(a4.Id, a6.Id);
    }

    [Theory]
    [InlineData(1, 1)]  // Sequence 1 = Tier 1 (Personal)
    [InlineData(15, 1)] // Sequence 15 = Tier 1
    [InlineData(30, 1)] // Sequence 30 = Tier 1 (boundary)
    [InlineData(31, 2)] // Sequence 31 = Tier 2 (Local)
    [InlineData(40, 2)] // Sequence 40 = Tier 2
    [InlineData(50, 2)] // Sequence 50 = Tier 2 (boundary)
    [InlineData(51, 3)] // Sequence 51 = Tier 3 (Regional)
    [InlineData(100, 3)] // Sequence 100 = Tier 3
    public async Task GenerateNextATemplate_CalculatesTierCorrectly(int sequence, int expectedTier)
    {
        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();

        // Act
        await service.GenerateNextATemplate(sequence, context);

        // Assert
        GameWorld gameWorld = GetGameWorld();
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.Equal(expectedTier, template.Tier);
    }

    [Theory]
    [InlineData(1, 0)]  // (1-1) % 4 = 0 = Investigation
    [InlineData(2, 1)]  // (2-1) % 4 = 1 = Social
    [InlineData(3, 2)]  // (3-1) % 4 = 2 = Confrontation
    [InlineData(4, 3)]  // (4-1) % 4 = 3 = Crisis
    [InlineData(5, 0)]  // (5-1) % 4 = 0 = Investigation (cycle repeats)
    [InlineData(11, 2)] // (11-1) % 4 = 2 = Confrontation
    public async Task GenerateNextATemplate_RotatesArchetypesCorrectly(int sequence, int expectedCyclePosition)
    {
        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();

        // Map cycle position to expected archetype category
        string expectedCategoryPattern = expectedCyclePosition switch
        {
            0 => "investigate", // Investigation archetypes contain "investigate"
            1 => "meet",        // Social archetypes contain "meet" or "gain"
            2 => "confront",    // Confrontation archetypes contain "confront"
            3 => "crisis",      // Crisis archetypes contain "crisis"
            _ => throw new InvalidOperationException()
        };

        // Act
        await service.GenerateNextATemplate(sequence, context);

        // Assert
        GameWorld gameWorld = GetGameWorld();
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.NotNull(template.SceneArchetypeId);

        // Verify archetype matches expected category
        Assert.Contains(expectedCategoryPattern, template.SceneArchetypeId.ToLowerInvariant());
    }

    [Fact]
    public void AStoryContext_IsArchetypeRecent_ReturnsTrueForRecentArchetypes()
    {
        // Arrange
        AStoryContext context = new AStoryContext();
        context.RecentArchetypeIds.Add("investigate_location");
        context.RecentArchetypeIds.Add("meet_order_member");
        context.RecentArchetypeIds.Add("confront_antagonist");

        // Act & Assert
        Assert.True(context.IsArchetypeRecent("investigate_location"));
        Assert.True(context.IsArchetypeRecent("meet_order_member"));
        Assert.False(context.IsArchetypeRecent("gather_testimony"));
    }

    [Fact]
    public void AStoryContext_CalculatedTier_ReturnsCorrectTiers()
    {
        // Arrange & Act & Assert
        AStoryContext context1 = new AStoryContext { CurrentSequence = 15 };
        Assert.Equal(1, context1.CalculatedTier);

        AStoryContext context2 = new AStoryContext { CurrentSequence = 40 };
        Assert.Equal(2, context2.CalculatedTier);

        AStoryContext context3 = new AStoryContext { CurrentSequence = 75 };
        Assert.Equal(3, context3.CalculatedTier);
    }

    [Fact]
    public async Task SelectArchetype_AllArchetypesRecent_FallbackToAnyArchetype()
    {
        // EDGE CASE COVERAGE: Tests fallback when all candidate archetypes are recent
        // Corresponds to SelectArchetype line 128-131

        // Arrange
        ProceduralAStoryService service = GetService<ProceduralAStoryService>();
        AStoryContext context = AStoryContext.InitializeForProceduralGeneration();

        // Mark all Investigation archetypes as recent (sequence 1 uses Investigation)
        context.RecentArchetypeIds.Add("investigate_location");
        context.RecentArchetypeIds.Add("gather_testimony");
        context.RecentArchetypeIds.Add("seek_audience");

        int sequence = 1; // Investigation category (cycle position 0)

        // Act - Should fallback to ANY Investigation archetype (not filter by recent)
        await service.GenerateNextATemplate(sequence, context);

        // Assert - Template created successfully despite all archetypes being recent
        GameWorld gameWorld = GetGameWorld();
        SceneTemplate template = gameWorld.SceneTemplates
            .FirstOrDefault(t => t.MainStorySequence == sequence);

        Assert.NotNull(template);
        Assert.Contains("investigate", template.SceneArchetypeId.ToLowerInvariant());
    }
}
