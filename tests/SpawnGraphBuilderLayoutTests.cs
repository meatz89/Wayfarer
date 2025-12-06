using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// SPAWNGRAPH LAYOUT AND COMPLEX SCENARIO TESTS
/// Tests fallback layout positioning and complex graph hierarchies
/// </summary>
public class SpawnGraphBuilderLayoutTests
{
    // ==================== COMPLEX GRAPH SCENARIOS ====================

    [Fact]
    public async Task BuildGraphAsync_CompleteHierarchy_CreatesCorrectNodeCount()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCompleteHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(7, result.NodeCount); // 1 scene, 2 situations, 2 choices, 2 entities

        Assert.Equal(1, diagram.Nodes.Count(n => n is SceneNodeModel));
        Assert.Equal(2, diagram.Nodes.Count(n => n is SituationNodeModel));
        Assert.Equal(2, diagram.Nodes.Count(n => n is ChoiceNodeModel));
        Assert.Equal(2, diagram.Nodes.Count(n => n is EntityNodeModel));
    }

    [Fact]
    public async Task BuildGraphAsync_CompleteHierarchy_CreatesCorrectLinkCount()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCompleteHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.LinkCount >= 6);
    }

    [Fact]
    public async Task BuildGraphAsync_DeepHierarchy_HandlesMultipleLevels()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithDeepHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.NodeCount >= 4); // Scene + multiple situations at different depths
    }

    // ==================== FALLBACK LAYOUT POSITIONING ====================

    [Fact]
    public async Task BuildGraphAsync_FallbackLayout_PositionsNodesInColumns()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);

        SceneNodeModel sceneNode = diagram.Nodes.OfType<SceneNodeModel>().First();
        SituationNodeModel situationNode = diagram.Nodes.OfType<SituationNodeModel>().First();
        ChoiceNodeModel choiceNode = diagram.Nodes.OfType<ChoiceNodeModel>().First();

        Assert.True(sceneNode.Position.X < situationNode.Position.X);
        Assert.True(situationNode.Position.X < choiceNode.Position.X);
    }

    [Fact]
    public async Task BuildGraphAsync_FallbackLayout_CalculatesGraphDimensions()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCompleteHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.GraphWidth > 0);
        Assert.True(result.GraphHeight > 0);
    }

    // ==================== GRAPH RESULT PROPERTIES ====================

    [Fact]
    public async Task BuildGraphAsync_Success_ReturnsCorrectCounts()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(diagram.Nodes.Count, result.NodeCount);
        Assert.Equal(diagram.Links.Count, result.LinkCount);
    }

    [Fact]
    public async Task BuildGraphAsync_FallbackLayout_EntityNodesInFourthColumn()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithLocationEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);

        SceneNodeModel sceneNode = diagram.Nodes.OfType<SceneNodeModel>().First();
        SituationNodeModel situationNode = diagram.Nodes.OfType<SituationNodeModel>().First();
        EntityNodeModel entityNode = diagram.Nodes.OfType<EntityNodeModel>().First();

        // Entity nodes should be in the rightmost column
        Assert.True(entityNode.Position.X > situationNode.Position.X);
    }

    [Fact]
    public async Task BuildGraphAsync_MultipleRowsFallback_SpacesVertically()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleScenes(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);

        List<SceneNodeModel> sceneNodes = diagram.Nodes.OfType<SceneNodeModel>().ToList();
        Assert.Equal(3, sceneNodes.Count);

        // Each scene should be at different Y positions
        List<double> yPositions = sceneNodes.Select(n => n.Position.Y).Distinct().ToList();
        Assert.Equal(3, yPositions.Count);
    }

    // ==================== HELPER METHODS ====================

    private Mock<IJSRuntime> CreateMockJSRuntimeWithNullLayout()
    {
        Mock<IJSRuntime> jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(js => js.InvokeAsync<DagreLayoutResult>(
            It.IsAny<string>(),
            It.IsAny<object[]>()))
            .ReturnsAsync((DagreLayoutResult)null);
        return jsRuntime;
    }

    private ProceduralContentTracer CreateTracerWithChoice()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate("choice_1", "Accept the offer");
        tracer.RecordChoiceExecution(choiceTemplate, sitNode, "Accept the offer", true);
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithLocationEntity()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Location location = CreateTestLocation("Market Square");
        Situation situation = CreateTestSituationWithLocation("sit_1", "Test Situation", location);
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered }
        };
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithMultipleScenes(int count)
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        for (int i = 0; i < count; i++)
        {
            Scene scene = CreateTestScene($"scene_{i}", $"Test Scene {i}");
            tracer.RecordSceneSpawn(scene, $"scene_{i}", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        }
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithCompleteHierarchy()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Situation 1 with Location
        Location location = CreateTestLocation("Market Square");
        Situation situation1 = CreateTestSituationWithLocation("sit_1", "Situation 1", location);
        SituationSpawnNode sitNode1 = tracer.RecordSituationSpawn(situation1, sceneNode,
            SituationSpawnTriggerType.SceneEntry, new EntityResolutionContext
            {
                LocationResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered }
            });

        // Situation 2 with NPC
        NPC npc = CreateTestNPC("Merchant");
        Situation situation2 = CreateTestSituationWithNPC("sit_2", "Situation 2", npc);
        SituationSpawnNode sitNode2 = tracer.RecordSituationSpawn(situation2, sceneNode,
            SituationSpawnTriggerType.SceneEntry, new EntityResolutionContext
            {
                NpcResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered }
            });

        // Choice in each situation
        ChoiceTemplate choice1 = CreateTestChoiceTemplate("choice_1", "Choice 1");
        tracer.RecordChoiceExecution(choice1, sitNode1, "Choice 1", true);

        ChoiceTemplate choice2 = CreateTestChoiceTemplate("choice_2", "Choice 2");
        tracer.RecordChoiceExecution(choice2, sitNode2, "Choice 2", true);

        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithDeepHierarchy()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Level 1 situation
        Situation sit1 = CreateTestSituation("sit_1", "Level 1");
        SituationSpawnNode sitNode1 = tracer.RecordSituationSpawn(sit1, sceneNode,
            SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());

        // Level 2 situation (child of Level 1)
        tracer.PushSituationContext(sitNode1);
        Situation sit2 = CreateTestSituation("sit_2", "Level 2");
        SituationSpawnNode sitNode2 = tracer.RecordSituationSpawn(sit2, sceneNode,
            SituationSpawnTriggerType.Cascade, EntityResolutionContext.Empty());

        // Level 3 situation (child of Level 2)
        tracer.PushSituationContext(sitNode2);
        Situation sit3 = CreateTestSituation("sit_3", "Level 3");
        tracer.RecordSituationSpawn(sit3, sceneNode,
            SituationSpawnTriggerType.Cascade, EntityResolutionContext.Empty());
        tracer.PopSituationContext();

        tracer.PopSituationContext();

        return tracer;
    }

    // ==================== ENTITY FACTORY HELPERS ====================

    private Scene CreateTestScene(string templateId, string displayName)
    {
        return new Scene
        {
            TemplateId = templateId,
            Template = new SceneTemplate { DisplayNameTemplate = displayName },
            Category = StoryCategory.Encounter,
            State = SceneState.Active,
            SituationCount = 0
        };
    }

    private Situation CreateTestSituation(string templateId, string name)
    {
        return new Situation
        {
            TemplateId = templateId,
            Name = name,
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            InteractionType = SituationInteractionType.Instant
        };
    }

    private Situation CreateTestSituationWithLocation(string templateId, string name, Location location)
    {
        return new Situation
        {
            TemplateId = templateId,
            Name = name,
            Location = location,
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            InteractionType = SituationInteractionType.Instant
        };
    }

    private Situation CreateTestSituationWithNPC(string templateId, string name, NPC npc)
    {
        return new Situation
        {
            TemplateId = templateId,
            Name = name,
            Npc = npc,
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            InteractionType = SituationInteractionType.Instant
        };
    }

    private Location CreateTestLocation(string name)
    {
        return new Location { Name = name, Purpose = LocationPurpose.Commerce, Privacy = LocationPrivacy.Public };
    }

    private NPC CreateTestNPC(string name)
    {
        return new NPC { Name = name, Profession = Profession.Merchant };
    }

    private ChoiceTemplate CreateTestChoiceTemplate(string id, string actionText)
    {
        return new ChoiceTemplate
        {
            Id = id,
            ActionTextTemplate = actionText,
            PathType = ChoicePathType.InstantSuccess,
            ActionType = ChoiceActionType.Instant
        };
    }
}
