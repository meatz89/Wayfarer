using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// SPAWNGRAPH NODE CREATION TESTS
/// Tests that SpawnGraphBuilder correctly creates nodes for all entity types
/// </summary>
public class SpawnGraphBuilderNodeTests
{
    // ==================== BUILD GRAPH - EMPTY/NULL CASES ====================

    [Fact]
    public async Task BuildGraphAsync_WithNullTracer_ReturnsFailure()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = new Mock<IJSRuntime>();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(null, diagram);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No tracer data", result.Message);
    }

    [Fact]
    public async Task BuildGraphAsync_WithEmptyTracer_ReturnsFailure()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = new Mock<IJSRuntime>();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No tracer data", result.Message);
    }

    [Fact]
    public async Task BuildGraphAsync_WithJSFailure_UsesFallbackLayout()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(js => js.InvokeAsync<DagreLayoutResult>(
            It.IsAny<string>(),
            It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("JS interop unavailable"));

        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithSingleScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("fallback", result.Message.ToLower());
    }

    // ==================== SCENE NODE TESTS ====================

    [Fact]
    public async Task BuildGraphAsync_SingleScene_CreatesSceneNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithSingleScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.NodeCount);
        Assert.IsType<SceneNodeModel>(diagram.Nodes[0]);
        SceneNodeModel sceneNode = diagram.Nodes[0] as SceneNodeModel;
        Assert.Equal("Test Scene", sceneNode.DisplayName);
    }

    [Fact]
    public async Task BuildGraphAsync_MultipleScenes_CreatesAllSceneNodes()
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
        int sceneNodeCount = diagram.Nodes.Count(n => n is SceneNodeModel);
        Assert.Equal(3, sceneNodeCount);
    }

    // ==================== SITUATION NODE TESTS ====================

    [Fact]
    public async Task BuildGraphAsync_SceneWithSituation_CreatesSituationNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithSceneAndSituation();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int situationNodeCount = diagram.Nodes.Count(n => n is SituationNodeModel);
        Assert.Equal(1, situationNodeCount);

        SituationNodeModel sitNode = diagram.Nodes.OfType<SituationNodeModel>().First();
        Assert.Equal("Test Situation", sitNode.Name);
    }

    [Fact]
    public async Task BuildGraphAsync_MultipleSituations_CreatesAllSituationNodes()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleSituations(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int situationNodeCount = diagram.Nodes.Count(n => n is SituationNodeModel);
        Assert.Equal(3, situationNodeCount);
    }

    // ==================== CHOICE NODE TESTS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithChoice_CreatesChoiceNode()
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
        int choiceNodeCount = diagram.Nodes.Count(n => n is ChoiceNodeModel);
        Assert.Equal(1, choiceNodeCount);

        ChoiceNodeModel choiceNode = diagram.Nodes.OfType<ChoiceNodeModel>().First();
        Assert.Contains("Accept", choiceNode.ActionText);
    }

    [Fact]
    public async Task BuildGraphAsync_MultipleChoices_CreatesAllChoiceNodes()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleChoices(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int choiceNodeCount = diagram.Nodes.Count(n => n is ChoiceNodeModel);
        Assert.Equal(3, choiceNodeCount);
    }

    // ==================== ENTITY NODE TESTS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithLocation_CreatesEntityNode()
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
        int entityNodeCount = diagram.Nodes.Count(n => n is EntityNodeModel);
        Assert.Equal(1, entityNodeCount);

        EntityNodeModel entityNode = diagram.Nodes.OfType<EntityNodeModel>().First();
        Assert.Equal(SpawnGraphEntityType.Location, entityNode.EntityType);
        Assert.Equal("Market Square", entityNode.EntityName);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithNPC_CreatesNPCEntityNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithNPCEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        EntityNodeModel entityNode = diagram.Nodes.OfType<EntityNodeModel>()
            .FirstOrDefault(e => e.EntityType == SpawnGraphEntityType.NPC);
        Assert.NotNull(entityNode);
        Assert.Equal("Merchant Aldric", entityNode.EntityName);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithRoute_CreatesRouteEntityNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithRouteEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        EntityNodeModel entityNode = diagram.Nodes.OfType<EntityNodeModel>()
            .FirstOrDefault(e => e.EntityType == SpawnGraphEntityType.Route);
        Assert.NotNull(entityNode);
        Assert.Contains("Village", entityNode.EntityName);
        Assert.Contains("City", entityNode.EntityName);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithAllEntities_CreatesAllEntityNodes()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithAllEntities();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int entityNodeCount = diagram.Nodes.Count(n => n is EntityNodeModel);
        Assert.Equal(3, entityNodeCount);

        Assert.True(diagram.Nodes.OfType<EntityNodeModel>().Any(e => e.EntityType == SpawnGraphEntityType.Location));
        Assert.True(diagram.Nodes.OfType<EntityNodeModel>().Any(e => e.EntityType == SpawnGraphEntityType.NPC));
        Assert.True(diagram.Nodes.OfType<EntityNodeModel>().Any(e => e.EntityType == SpawnGraphEntityType.Route));
    }

    [Fact]
    public async Task BuildGraphAsync_DuplicateEntities_CreatesOnlyOneNodeEach()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithDuplicateEntities();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int locationEntityCount = diagram.Nodes.OfType<EntityNodeModel>()
            .Count(e => e.EntityType == SpawnGraphEntityType.Location);
        Assert.Equal(1, locationEntityCount);
    }

    [Fact]
    public async Task BuildGraphAsync_ClearsDiagramBeforeBuilding()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();

        diagram.Nodes.Add(new NodeModel(new Point(0, 0)));
        diagram.Nodes.Add(new NodeModel(new Point(100, 100)));

        ProceduralContentTracer tracer = CreateTracerWithSingleScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.NodeCount);
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

    private ProceduralContentTracer CreateTracerWithSingleScene()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
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

    private ProceduralContentTracer CreateTracerWithSceneAndSituation()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithMultipleSituations(int count)
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        for (int i = 0; i < count; i++)
        {
            Situation situation = CreateTestSituation($"sit_{i}", $"Test Situation {i}");
            tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        }
        return tracer;
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

    private ProceduralContentTracer CreateTracerWithMultipleChoices(int count)
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        for (int i = 0; i < count; i++)
        {
            ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate($"choice_{i}", $"Choice {i}");
            tracer.RecordChoiceExecution(choiceTemplate, sitNode, $"Choice {i}", true);
        }
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

    private ProceduralContentTracer CreateTracerWithNPCEntity()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        NPC npc = CreateTestNPC("Merchant Aldric");
        Situation situation = CreateTestSituationWithNPC("sit_1", "Test Situation", npc);
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            NpcResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered }
        };
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithRouteEntity()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Location origin = CreateTestLocation("Village");
        Location destination = CreateTestLocation("City");
        RouteOption route = CreateTestRoute(origin, destination);
        Situation situation = CreateTestSituationWithRoute("sit_1", "Test Situation", route);
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            RouteResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.RouteDestination }
        };
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithAllEntities()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Location location = CreateTestLocation("Market Square");
        NPC npc = CreateTestNPC("Merchant Aldric");
        Location origin = CreateTestLocation("Village");
        Location destination = CreateTestLocation("City");
        RouteOption route = CreateTestRoute(origin, destination);
        Situation situation = CreateTestSituationWithAllEntities("sit_1", "Test Situation", location, npc, route);
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered },
            NpcResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered },
            RouteResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.RouteDestination }
        };
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithDuplicateEntities()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Location sharedLocation = CreateTestLocation("Market Square");
        Situation situation1 = CreateTestSituationWithLocation("sit_1", "Situation 1", sharedLocation);
        Situation situation2 = CreateTestSituationWithLocation("sit_2", "Situation 2", sharedLocation);
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = new EntityResolutionMetadata { Outcome = EntityResolutionOutcome.Discovered }
        };
        tracer.RecordSituationSpawn(situation1, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
        tracer.RecordSituationSpawn(situation2, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);
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

    private Situation CreateTestSituationWithRoute(string templateId, string name, RouteOption route)
    {
        return new Situation
        {
            TemplateId = templateId,
            Name = name,
            Route = route,
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            InteractionType = SituationInteractionType.Navigation
        };
    }

    private Situation CreateTestSituationWithAllEntities(string templateId, string name, Location location, NPC npc, RouteOption route)
    {
        return new Situation
        {
            TemplateId = templateId,
            Name = name,
            Location = location,
            Npc = npc,
            Route = route,
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

    private RouteOption CreateTestRoute(Location origin, Location destination)
    {
        return new RouteOption
        {
            OriginLocation = origin,
            DestinationLocation = destination,
            Method = TravelMethod.Walk,
            BaseStaminaCost = 2,
            BaseCoinCost = 0
        };
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
