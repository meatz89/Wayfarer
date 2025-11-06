using Wayfarer.Content;
using Wayfarer.Content.Catalogues;
using Wayfarer.Content.Generators;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Services;
using Wayfarer.Subsystems.ProceduralContent;
using Xunit;

namespace Wayfarer.Tests.Project;

/// <summary>
/// Integration test using REAL facades with TEST GameWorld
/// Validates: Stateless facades + Test JSON = Clean integration testing
/// </summary>
public class LodgingSceneIntegrationTest
{
    [Fact]
    public void ServiceWithLocationAccess_CompleteFlow_WithRealFacades()
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
            NpcLocationId = inn.Id,
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

        SpawnConditionsEvaluator spawnEvaluator = new SpawnConditionsEvaluator(gameWorld);
        SceneNarrativeService narrativeService = new SceneNarrativeService(gameWorld);
        MarkerResolutionService markerService = new MarkerResolutionService();
        SceneInstantiator instantiator = new SceneInstantiator(
            gameWorld,
            spawnEvaluator,
            narrativeService,
            markerService);

        SceneInstanceFacade sceneInstanceFacade = new SceneInstanceFacade(instantiator, gameWorld);

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

        Scene provisionalScene = sceneInstanceFacade.CreateProvisionalScene(template, spawnReward, spawnContext);

        Assert.NotNull(provisionalScene);
        Assert.Equal(SceneState.Provisional, provisionalScene.State);
        Assert.Equal(template.Id, provisionalScene.TemplateId);

        (Scene finalizedScene, DependentResourceSpecs specs) = sceneInstanceFacade.FinalizeScene(provisionalScene.Id, spawnContext);

        Assert.NotNull(finalizedScene);
        Assert.Equal(SceneState.Active, finalizedScene.State);
        Assert.NotEmpty(finalizedScene.SituationIds);
        Assert.Equal(4, finalizedScene.SituationIds.Count);

        Assert.NotNull(specs);
        Assert.True(specs.HasResources);
        Assert.Single(specs.Locations);
        Assert.Single(specs.Items);

        LocationDTO privateRoomDto = specs.Locations[0];
        Assert.Contains("private_room", privateRoomDto.Id);
        Assert.Contains(innkeeper.Name, privateRoomDto.Name);
        Assert.Equal("test_inn_venue", privateRoomDto.VenueId);

        ItemDTO roomKeyDto = specs.Items[0];
        Assert.Contains("room_key", roomKeyDto.Id);

        Assert.NotEmpty(finalizedScene.MarkerResolutionMap);
        Assert.True(finalizedScene.MarkerResolutionMap.ContainsKey("generated:private_room"));
        Assert.True(finalizedScene.MarkerResolutionMap.ContainsKey("generated:room_key"));

        List<Situation> situations = gameWorld.Situations
            .Where(s => finalizedScene.SituationIds.Contains(s.Id))
            .ToList();

        Assert.Equal(4, situations.Count);

        Situation negotiateSituation = situations.FirstOrDefault(s => s.Template.Id == "secure_lodging_negotiate");
        Assert.NotNull(negotiateSituation);
        Assert.NotEmpty(negotiateSituation.Template.ChoiceTemplates);

        Situation accessSituation = situations.FirstOrDefault(s => s.Template.Id == "secure_lodging_access");
        Assert.NotNull(accessSituation);
        Assert.NotNull(accessSituation.ResolvedRequiredLocationId);
        Assert.NotEqual("generated:private_room", accessSituation.ResolvedRequiredLocationId);

        Assert.NotEmpty(finalizedScene.CreatedLocationIds);
    }
}
