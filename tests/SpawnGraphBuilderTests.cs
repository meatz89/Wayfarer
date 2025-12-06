using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// COMPREHENSIVE TESTS: SpawnGraphBuilder
/// Tests graph construction from ProceduralContentTracer data
/// Verifies correct node/edge generation for all entity types
/// Tests both dagre layout path and fallback layout
/// </summary>
public class SpawnGraphBuilderTests
{
    // ==================== BUILD GRAPH - EMPTY/NULL CASES ====================

    [Fact]
    public async Task BuildGraphAsync_WithNullTracer_ReturnsFailure()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = new Mock<IJSRuntime>();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();

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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No tracer data", result.Message);
    }

    // ==================== BUILD GRAPH - FALLBACK LAYOUT ====================

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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithSingleScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("fallback", result.Message.ToLower());
    }

    // ==================== NODE CREATION - SCENE NODES ====================

    [Fact]
    public async Task BuildGraphAsync_SingleScene_CreatesSceneNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleScenes(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int sceneNodeCount = diagram.Nodes.Count(n => n is SceneNodeModel);
        Assert.Equal(3, sceneNodeCount);
    }

    // ==================== NODE CREATION - SITUATION NODES ====================

    [Fact]
    public async Task BuildGraphAsync_SceneWithSituation_CreatesSituationNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleSituations(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int situationNodeCount = diagram.Nodes.Count(n => n is SituationNodeModel);
        Assert.Equal(3, situationNodeCount);
    }

    // ==================== NODE CREATION - CHOICE NODES ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithChoice_CreatesChoiceNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithMultipleChoices(3);

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int choiceNodeCount = diagram.Nodes.Count(n => n is ChoiceNodeModel);
        Assert.Equal(3, choiceNodeCount);
    }

    // ==================== NODE CREATION - ENTITY NODES ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithLocation_CreatesEntityNode()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithAllEntities();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int entityNodeCount = diagram.Nodes.Count(n => n is EntityNodeModel);
        Assert.Equal(3, entityNodeCount); // Location, NPC, Route

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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithDuplicateEntities();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        // Two situations referencing same location should create only one entity node
        int locationEntityCount = diagram.Nodes.OfType<EntityNodeModel>()
            .Count(e => e.EntityType == SpawnGraphEntityType.Location);
        Assert.Equal(1, locationEntityCount);
    }

    // ==================== LINK CREATION - HIERARCHY LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SceneWithSituation_CreatesHierarchyLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithSceneAndSituation();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.LinkCount > 0);

        SpawnGraphLinkModel hierarchyLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.Hierarchy);
        Assert.NotNull(hierarchyLink);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithChoice_CreatesHierarchyLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        // Should have Scene→Situation and Situation→Choice hierarchy links
        int hierarchyLinkCount = diagram.Links.OfType<SpawnGraphLinkModel>()
            .Count(l => l.LinkType == SpawnGraphLinkType.Hierarchy);
        Assert.Equal(2, hierarchyLinkCount);
    }

    // ==================== LINK CREATION - SPAWN LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_ChoiceSpawnsScene_CreatesSpawnSceneLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoiceSpawningScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel spawnLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.SpawnScene);
        Assert.NotNull(spawnLink);
    }

    [Fact]
    public async Task BuildGraphAsync_ChoiceSpawnsSituation_CreatesSpawnSituationLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoiceSpawningSituation();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel spawnLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.SpawnSituation);
        Assert.NotNull(spawnLink);
    }

    // ==================== LINK CREATION - ENTITY LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithLocation_CreatesEntityLocationLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithLocationEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityLocation);
        Assert.NotNull(entityLink);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithNPC_CreatesEntityNpcLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithNPCEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityNpc);
        Assert.NotNull(entityLink);
    }

    [Fact]
    public async Task BuildGraphAsync_SituationWithRoute_CreatesEntityRouteLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithRouteEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityRoute);
        Assert.NotNull(entityLink);
    }

    // ==================== LINK CREATION - PARENT SITUATION LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithParentSituation_CreatesSpawnSituationLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCascadingSituations();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        // Parent situation to child situation should create SpawnSituation link
        SpawnGraphLinkModel cascadeLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.SpawnSituation);
        Assert.NotNull(cascadeLink);
    }

    // ==================== COMPLEX GRAPH SCENARIOS ====================

    [Fact]
    public async Task BuildGraphAsync_CompleteHierarchy_CreatesCorrectNodeCount()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCompleteHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        // Expected: 1 scene, 2 situations, 2 choices, 2 entities (Location + NPC)
        Assert.Equal(7, result.NodeCount);

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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCompleteHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        // Expected links:
        // - Scene→Situation1 (hierarchy)
        // - Scene→Situation2 (hierarchy)
        // - Situation1→Choice1 (hierarchy)
        // - Situation2→Choice2 (hierarchy)
        // - Situation1→Location (entity)
        // - Situation2→NPC (entity)
        Assert.True(result.LinkCount >= 6);
    }

    [Fact]
    public async Task BuildGraphAsync_DeepHierarchy_HandlesMultipleLevels()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithDeepHierarchy();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.NodeCount >= 5); // Scene, multiple situations at different depths
    }

    // ==================== FALLBACK LAYOUT POSITIONING ====================

    [Fact]
    public async Task BuildGraphAsync_FallbackLayout_PositionsNodesInColumns()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);

        SceneNodeModel sceneNode = diagram.Nodes.OfType<SceneNodeModel>().First();
        SituationNodeModel situationNode = diagram.Nodes.OfType<SituationNodeModel>().First();
        ChoiceNodeModel choiceNode = diagram.Nodes.OfType<ChoiceNodeModel>().First();

        // Fallback layout places: Scene (column 0), Situation (column 1), Choice (column 2)
        Assert.True(sceneNode.Position.X < situationNode.Position.X);
        Assert.True(situationNode.Position.X < choiceNode.Position.X);
    }

    [Fact]
    public async Task BuildGraphAsync_FallbackLayout_CalculatesGraphDimensions()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
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
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(diagram.Nodes.Count, result.NodeCount);
        Assert.Equal(diagram.Links.Count, result.LinkCount);
    }

    [Fact]
    public async Task BuildGraphAsync_ClearsDiagramBeforeBuilding()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();

        // Add some existing nodes
        diagram.Nodes.Add(new NodeModel(new Point(0, 0)));
        diagram.Nodes.Add(new NodeModel(new Point(100, 100)));

        ProceduralContentTracer tracer = CreateTracerWithSingleScene();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.NodeCount); // Only the scene node, pre-existing cleared
    }

    // ==================== ENTITY RESOLUTION METADATA ====================

    [Fact]
    public async Task BuildGraphAsync_EntityWithResolutionMetadata_PassesToLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = CreateTestDiagram();
        ProceduralContentTracer tracer = CreateTracerWithEntityResolution();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityLocation);
        Assert.NotNull(entityLink);
        // Link should have label from resolution metadata
        Assert.NotNull(entityLink.Label);
    }

    // ==================== HELPER METHODS ====================

    private BlazorDiagram CreateTestDiagram()
    {
        return new BlazorDiagram();
    }

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
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
            new EntityResolutionContext());

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
            tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
                new EntityResolutionContext());
        }

        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithChoice()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
            new EntityResolutionContext());

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
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
            new EntityResolutionContext());

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
            LocationResolution = new EntityResolutionMetadata
            {
                Outcome = EntityResolutionOutcome.Discovered
            }
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
            NpcResolution = new EntityResolutionMetadata
            {
                Outcome = EntityResolutionOutcome.Discovered
            }
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
            RouteResolution = new EntityResolutionMetadata
            {
                Outcome = EntityResolutionOutcome.RouteDestination
            }
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

        // Two situations referencing the same location
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

    private ProceduralContentTracer CreateTracerWithChoiceSpawningScene()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
            new EntityResolutionContext());

        ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate("choice_1", "Start quest");
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choiceTemplate, sitNode, "Start quest", true);

        // Push choice context and spawn a scene
        tracer.PushChoiceContext(choiceNode);
        Scene spawnedScene = CreateTestScene("scene_2", "Spawned Scene");
        tracer.RecordSceneSpawn(spawnedScene, "scene_2", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Morning);
        tracer.PopChoiceContext();

        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithChoiceSpawningSituation()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry,
            new EntityResolutionContext());

        ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate("choice_1", "Investigate");
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choiceTemplate, sitNode, "Investigate", true);

        // Record spawned situation linked to choice
        Situation spawnedSituation = CreateTestSituation("sit_2", "Spawned Situation");
        choiceNode.SpawnedSituations.Add(new SituationSpawnNode { Name = "Spawned Situation" });
        tracer.AllSituationNodes.Add(choiceNode.SpawnedSituations[0]);

        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithCascadingSituations()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        Situation parentSituation = CreateTestSituation("sit_1", "Parent Situation");
        SituationSpawnNode parentSitNode = tracer.RecordSituationSpawn(parentSituation, sceneNode,
            SituationSpawnTriggerType.SceneEntry, new EntityResolutionContext());

        // Push parent situation context for cascade
        tracer.PushSituationContext(parentSitNode);

        Situation childSituation = CreateTestSituation("sit_2", "Child Situation");
        tracer.RecordSituationSpawn(childSituation, sceneNode, SituationSpawnTriggerType.Cascade,
            new EntityResolutionContext());

        tracer.PopSituationContext();

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
            SituationSpawnTriggerType.SceneEntry, new EntityResolutionContext());

        // Level 2 situation (child of Level 1)
        tracer.PushSituationContext(sitNode1);
        Situation sit2 = CreateTestSituation("sit_2", "Level 2");
        SituationSpawnNode sitNode2 = tracer.RecordSituationSpawn(sit2, sceneNode,
            SituationSpawnTriggerType.Cascade, new EntityResolutionContext());

        // Level 3 situation (child of Level 2)
        tracer.PushSituationContext(sitNode2);
        Situation sit3 = CreateTestSituation("sit_3", "Level 3");
        tracer.RecordSituationSpawn(sit3, sceneNode,
            SituationSpawnTriggerType.Cascade, new EntityResolutionContext());
        tracer.PopSituationContext();

        tracer.PopSituationContext();

        return tracer;
    }

    private ProceduralContentTracer CreateTracerWithEntityResolution()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        Location location = CreateTestLocation("Market Square");
        Situation situation = CreateTestSituationWithLocation("sit_1", "Test Situation", location);

        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = new EntityResolutionMetadata
            {
                Outcome = EntityResolutionOutcome.Discovered,
                Filter = new PlacementFilterSnapshot
                {
                    Purpose = LocationPurpose.Commerce,
                    Privacy = LocationPrivacy.Public
                }
            }
        };
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, resolutionContext);

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
        return new Location
        {
            Name = name,
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public
        };
    }

    private NPC CreateTestNPC(string name)
    {
        return new NPC
        {
            Name = name,
            Profession = Profession.Merchant
        };
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
