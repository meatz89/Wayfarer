using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// SPAWNGRAPH LINK CREATION TESTS
/// Tests that SpawnGraphBuilder correctly creates links for all relationship types
/// </summary>
public class SpawnGraphBuilderLinkTests
{
    // ==================== HIERARCHY LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SceneWithSituation_CreatesHierarchyLink()
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
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoice();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        int hierarchyLinkCount = diagram.Links.OfType<SpawnGraphLinkModel>()
            .Count(l => l.LinkType == SpawnGraphLinkType.Hierarchy);
        Assert.Equal(2, hierarchyLinkCount); // Scene→Situation and Situation→Choice
    }

    // ==================== SPAWN LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_ChoiceSpawnsScene_CreatesSpawnSceneLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
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
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithChoiceSpawningSituation();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel spawnLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.SpawnSituation);
        Assert.NotNull(spawnLink);
    }

    // ==================== ENTITY LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithLocation_CreatesEntityLocationLink()
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
        BlazorDiagram diagram = new BlazorDiagram();
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
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithRouteEntity();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityRoute);
        Assert.NotNull(entityLink);
    }

    // ==================== PARENT SITUATION (CASCADE) LINKS ====================

    [Fact]
    public async Task BuildGraphAsync_SituationWithParentSituation_CreatesSpawnSituationLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithCascadingSituations();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel cascadeLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.SpawnSituation);
        Assert.NotNull(cascadeLink);
    }

    // ==================== ENTITY RESOLUTION METADATA ====================

    [Fact]
    public async Task BuildGraphAsync_EntityWithResolutionMetadata_PassesToLink()
    {
        // Arrange
        Mock<IJSRuntime> jsRuntime = CreateMockJSRuntimeWithNullLayout();
        SpawnGraphBuilder builder = new SpawnGraphBuilder(jsRuntime.Object);
        BlazorDiagram diagram = new BlazorDiagram();
        ProceduralContentTracer tracer = CreateTracerWithEntityResolution();

        // Act
        SpawnGraphBuildResult result = await builder.BuildGraphAsync(tracer, diagram);

        // Assert
        Assert.True(result.Success);
        SpawnGraphLinkModel entityLink = diagram.Links.OfType<SpawnGraphLinkModel>()
            .FirstOrDefault(l => l.LinkType == SpawnGraphLinkType.EntityLocation);
        Assert.NotNull(entityLink);
        Assert.NotNull(entityLink.Label);
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

    private ProceduralContentTracer CreateTracerWithSceneAndSituation()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
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

    private ProceduralContentTracer CreateTracerWithChoiceSpawningScene()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation situation = CreateTestSituation("sit_1", "Test Situation");
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate("choice_1", "Start quest");
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choiceTemplate, sitNode, "Start quest", true);
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
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        ChoiceTemplate choiceTemplate = CreateTestChoiceTemplate("choice_1", "Investigate");
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choiceTemplate, sitNode, "Investigate", true);
        choiceNode.SpawnedSituations.Add(new SituationSpawnNode { Name = "Spawned Situation" });
        tracer.AllSituationNodes.Add(choiceNode.SpawnedSituations[0]);
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

    private ProceduralContentTracer CreateTracerWithCascadingSituations()
    {
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene("scene_1", "Test Scene");
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "scene_1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        Situation parentSituation = CreateTestSituation("sit_1", "Parent Situation");
        SituationSpawnNode parentSitNode = tracer.RecordSituationSpawn(parentSituation, sceneNode, SituationSpawnTriggerType.SceneEntry, EntityResolutionContext.Empty());
        tracer.PushSituationContext(parentSitNode);
        Situation childSituation = CreateTestSituation("sit_2", "Child Situation");
        tracer.RecordSituationSpawn(childSituation, sceneNode, SituationSpawnTriggerType.Cascade, EntityResolutionContext.Empty());
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
                Filter = new PlacementFilterSnapshot { Purpose = LocationPurpose.Commerce, Privacy = LocationPrivacy.Public }
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
