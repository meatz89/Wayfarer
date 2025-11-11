using Xunit;

namespace Wayfarer.Tests.Project;

/// <summary>
/// Integration test using REAL facades with TEST GameWorld
/// Validates: HIGHLANDER scene spawning flow (JSON → PackageLoader → Parser → Entity)
/// Replaces provisional scene pattern with direct Active scene creation
/// </summary>
public class LodgingSceneIntegrationTest
{
    [Fact]
    public async Task ServiceWithLocationAccess_CompleteFlow_WithRealFacades()
    {
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Test");

        Player player = gameWorld.GetPlayer();
        Assert.NotNull(player);

        List<Location> locations = gameWorld.Locations;
        Assert.NotEmpty(locations);

        Location inn = locations.FirstOrDefault(l => l.Id == "test_inn_common");
        Assert.NotNull(inn);
        Assert.Equal("test_inn_venue", inn.VenueId);

        List<NPC> npcs = gameWorld.NPCs;
        Assert.Single(npcs);

        NPC innkeeper = npcs[0];
        Assert.Equal("test_innkeeper", innkeeper.ID);
        Assert.Equal(PersonalityType.MERCANTILE, innkeeper.PersonalityType);

        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = innkeeper.PersonalityType,
            LocationId = inn.Id,
            NpcId = innkeeper.ID,
            NpcName = innkeeper.Name,
            PlayerCoins = 50
        };

        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.Equal(4, definition.SituationTemplates.Count);
        Assert.Single(definition.DependentLocations);
        Assert.Single(definition.DependentItems);

        // HIGHLANDER FLOW: Construct full dependency chain
        SpawnConditionsEvaluator spawnEvaluator = new SpawnConditionsEvaluator(gameWorld);
        SceneNarrativeService narrativeService = new SceneNarrativeService(gameWorld);
        MarkerResolutionService markerService = new MarkerResolutionService();
        SceneInstantiator instantiator = new SceneInstantiator(
            gameWorld,
            spawnEvaluator,
            narrativeService,
            markerService);

        ContentGenerationFacade contentGenerationFacade = new ContentGenerationFacade();
        SceneGenerationFacade sceneGenerationFacade = new SceneGenerationFacade(gameWorld);
        PackageLoader packageLoader = new PackageLoader(gameWorld, sceneGenerationFacade);
        PackageLoaderFacade packageLoaderFacade = new PackageLoaderFacade(packageLoader);
        HexRouteGenerator hexRouteGenerator = new HexRouteGenerator(gameWorld);
        MessageSystem messageSystem = new MessageSystem(gameWorld);
        TimeModel timeModel = new TimeModel(gameWorld.CurrentDay);
        timeModel.SetInitialState(gameWorld.CurrentDay, gameWorld.CurrentTimeBlock, 1);
        TimeManager timeManager = new TimeManager(timeModel, messageSystem);
        SpawnedScenePlayabilityValidator playabilityValidator = new SpawnedScenePlayabilityValidator(gameWorld);

        SceneInstanceFacade sceneInstanceFacade = new SceneInstanceFacade(
            instantiator,
            contentGenerationFacade,
            packageLoaderFacade,
            hexRouteGenerator,
            timeManager,
            gameWorld,
            playabilityValidator);

        SceneTemplate template = new SceneTemplate
        {
            Id = "test_lodging_scene",
            SituationTemplates = definition.SituationTemplates,
            SpawnRules = definition.SpawnRules,
            DependentLocations = definition.DependentLocations,
            DependentItems = definition.DependentItems,
            SpawnConditions = SpawnConditions.AlwaysEligible
        };

        SceneSpawnReward spawnReward = new SceneSpawnReward
        {
            SceneTemplateId = template.Id,
            PlacementRelation = PlacementRelation.SpecificNPC,
            SpecificPlacementId = innkeeper.ID
        };

        SceneSpawnContext spawnContext = new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = inn,
            CurrentNPC = innkeeper,
            CurrentSituation = null
        };

        // HIGHLANDER FLOW: Single method spawns scene as Active immediately
        Scene spawnedScene = await sceneInstanceFacade.SpawnScene(template, spawnReward, spawnContext);

        // Assert scene spawned successfully
        Assert.NotNull(spawnedScene);
        Assert.Equal(SceneState.Active, spawnedScene.State); // HIGHLANDER: No provisional state
        Assert.Equal(template.Id, spawnedScene.TemplateId);

        // Assert situations were created
        Assert.NotEmpty(spawnedScene.Situations);
        Assert.Equal(4, spawnedScene.Situations.Count);

        // Assert dependent resources were created and added to GameWorld
        Assert.NotEmpty(spawnedScene.CreatedLocationIds);
        Assert.NotEmpty(spawnedScene.CreatedItemIds);
        Assert.Single(spawnedScene.CreatedLocationIds);
        Assert.Single(spawnedScene.CreatedItemIds);

        // Verify dependent location exists in GameWorld
        string createdLocationId = spawnedScene.CreatedLocationIds[0];
        Location privateRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == createdLocationId);
        Assert.NotNull(privateRoom);
        Assert.Contains("private_room", privateRoom.Id);
        Assert.Contains(innkeeper.Name, privateRoom.Name);
        Assert.Equal("test_inn_venue", privateRoom.VenueId);

        // Verify dependent item exists in GameWorld
        string createdItemId = spawnedScene.CreatedItemIds[0];
        Item roomKey = gameWorld.Items.FirstOrDefault(i => i.Id == createdItemId);
        Assert.NotNull(roomKey);
        Assert.Contains("room_key", roomKey.Id);

        // Assert marker resolution map populated
        Assert.NotEmpty(spawnedScene.MarkerResolutionMap);
        Assert.True(spawnedScene.MarkerResolutionMap.ContainsKey("generated:private_room"));
        Assert.True(spawnedScene.MarkerResolutionMap.ContainsKey("generated:room_key"));

        // Verify marker resolution: "generated:private_room" → concrete location ID
        Assert.Equal(createdLocationId, spawnedScene.MarkerResolutionMap["generated:private_room"]);
        Assert.Equal(createdItemId, spawnedScene.MarkerResolutionMap["generated:room_key"]);

        // Assert situations have correct structure
        Situation negotiateSituation = spawnedScene.Situations.FirstOrDefault(s => s.Template.Id == "secure_lodging_negotiate");
        Assert.NotNull(negotiateSituation);
        Assert.NotEmpty(negotiateSituation.Template.ChoiceTemplates);

        Situation accessSituation = spawnedScene.Situations.FirstOrDefault(s => s.Template.Id == "secure_lodging_access");
        Assert.NotNull(accessSituation);
        Assert.NotNull(accessSituation.ResolvedRequiredLocationId);
        Assert.NotEqual("generated:private_room", accessSituation.ResolvedRequiredLocationId); // Should be resolved
        Assert.Equal(createdLocationId, accessSituation.ResolvedRequiredLocationId); // Marker resolved to concrete ID

        // Assert ParentScene relationships set
        foreach (Situation situation in spawnedScene.Situations)
        {
            Assert.NotNull(situation.ParentScene);
            Assert.Equal(spawnedScene.Id, situation.ParentScene.Id);
        }

        // Assert Template relationships set
        foreach (Situation situation in spawnedScene.Situations)
        {
            Assert.NotNull(situation.Template);
            Assert.Contains(situation.Template, template.SituationTemplates);
        }
    }
}
