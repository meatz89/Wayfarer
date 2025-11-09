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
/// Integration tests for tutorial inn_lodging scene
///
/// Tests FLOW and INTEGRATION, not exact balance values:
/// - Scene structure (3 situations with correct transitions)
/// - Choice structure (4 choices with correct PathTypes)
/// - Resource lifecycle (created, used, cleaned up)
/// - Player experience flow (can complete scene end-to-end)
///
/// Does NOT test exact numerical values (those are balance, will change)
/// </summary>
public class TutorialInnLodgingIntegrationTest
{
    [Fact]
    public void InnLodging_GeneratesCorrectStructure()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        GenerationContext context = GenerationContext.FromEntities(
            tier: 0,
            npc: elena,
            location: commonRoom,
            player: player);

        // ACT
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(
            "inn_lodging",
            tier: 0,
            context);

        // ASSERT STRUCTURE
        Assert.Equal(3, definition.SituationTemplates.Count);
        Assert.Equal(SpawnPattern.Linear, definition.SpawnRules.Pattern);
        Assert.Single(definition.DependentLocations);
        Assert.Single(definition.DependentItems);

        SituationTemplate negotiate = definition.SituationTemplates[0];
        SituationTemplate rest = definition.SituationTemplates[1];
        SituationTemplate depart = definition.SituationTemplates[2];

        // SITUATION 1: Negotiate has 4 choices with correct PathTypes
        Assert.Equal(4, negotiate.ChoiceTemplates.Count);
        Assert.Contains(negotiate.ChoiceTemplates, c => c.PathType == ChoicePathType.InstantSuccess && c.CostTemplate?.Coins > 0);  // Money path
        Assert.Contains(negotiate.ChoiceTemplates, c => c.PathType == ChoicePathType.InstantSuccess && c.RequirementFormula != null);  // Stat path
        Assert.Contains(negotiate.ChoiceTemplates, c => c.PathType == ChoicePathType.Challenge);  // Challenge path
        Assert.Contains(negotiate.ChoiceTemplates, c => c.PathType == ChoicePathType.Fallback);  // Fallback path

        // Success paths unlock room and grant key
        foreach (ChoiceTemplate choice in negotiate.ChoiceTemplates.Where(c => c.PathType != ChoicePathType.Fallback))
        {
            bool grantsAccess = (choice.RewardTemplate?.LocationsToUnlock?.Contains("generated:private_room") ?? false) ||
                               (choice.OnSuccessReward?.LocationsToUnlock?.Contains("generated:private_room") ?? false);
            Assert.True(grantsAccess, $"Choice {choice.Id} should grant room access");
        }

        // SITUATION 2: Rest uses generated room
        Assert.Equal("generated:private_room", rest.RequiredLocationId);
        Assert.Null(rest.RequiredNpcId);
        Assert.Equal(4, rest.ChoiceTemplates.Count);

        // SITUATION 3: Depart cleans up resources
        Assert.Equal("generated:private_room", depart.RequiredLocationId);
        Assert.Equal(2, depart.ChoiceTemplates.Count);

        foreach (ChoiceTemplate choice in depart.ChoiceTemplates)
        {
            Assert.Contains("generated:room_key", choice.RewardTemplate.ItemsToRemove);
            Assert.Contains("generated:private_room", choice.RewardTemplate.LocationsToLock);
        }

        // TRANSITIONS: negotiate → rest → depart
        Assert.Equal(2, definition.SpawnRules.Transitions.Count);
        Assert.Equal(negotiate.Id, definition.SpawnRules.InitialSituationId);
    }

    [Fact]
    public void InnLodging_CategoricalPropertiesApplied()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        // ACT
        GenerationContext context = GenerationContext.FromEntities(
            tier: 0,
            npc: elena,
            location: commonRoom,
            player: player);

        // ASSERT: Categorical properties derived (not exact values, just that derivation happens)
        Assert.NotEqual(default(Quality), context.Quality);
        Assert.NotEqual(default(EnvironmentQuality), context.Environment);
        Assert.NotEqual(default(NPCDemeanor), context.NpcDemeanor);
        Assert.NotEqual(default(PowerDynamic), context.Power);
        Assert.Equal(PersonalityType.MERCANTILE, context.NpcPersonality);
    }

    [Fact]
    public void Tutorial_UsesGeneralArchetype()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");

        // ACT
        SceneTemplate tutorialScene = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "tutorial_secure_lodging");

        // ASSERT: Tutorial uses general reusable archetype
        Assert.NotNull(tutorialScene);
        Assert.Equal(3, tutorialScene.SituationTemplates.Count);
        Assert.Equal(SpawnPattern.Linear, tutorialScene.SpawnRules.Pattern);
        Assert.True(tutorialScene.IsStarter);
        Assert.Equal(PresentationMode.Modal, tutorialScene.PresentationMode);
    }

    [Fact]
    public void InnLodging_CompletePlayerFlow()
    {
        // ARRANGE: Full game world with facades
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        Player player = gameWorld.GetPlayer();
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");

        GenerationContext context = GenerationContext.FromEntities(
            tier: 0,
            npc: elena,
            location: commonRoom,
            player: player);

        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(
            "inn_lodging",
            tier: 0,
            context);

        // Create real facades
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
            SpecificPlacementId = elena.ID
        };

        SceneSpawnContext spawnContext = new SceneSpawnContext
        {
            Player = player,
            CurrentLocation = commonRoom,
            CurrentNPC = elena,
            CurrentSituation = null
        };

        // ACT: Create and finalize scene
        Scene provisionalScene = sceneInstanceFacade.CreateProvisionalScene(template, spawnReward, spawnContext);
        SceneFinalizationResult result = sceneInstanceFacade.FinalizeScene(provisionalScene.Id, spawnContext);
        Scene finalizedScene = result.Scene;
        DependentResourceSpecs specs = result.DependentSpecs;

        // ASSERT: Complete flow integration
        Assert.Equal(SceneState.Active, finalizedScene.State);
        Assert.Equal(3, finalizedScene.SituationIds.Count);

        // Dependent resources created
        Assert.True(specs.HasResources);
        Assert.Single(specs.Locations);
        Assert.Single(specs.Items);

        // Marker resolution works
        Assert.True(finalizedScene.MarkerResolutionMap.ContainsKey("generated:private_room"));
        Assert.True(finalizedScene.MarkerResolutionMap.ContainsKey("generated:room_key"));

        // Situations exist and reference resolved IDs
        List<Situation> situations = gameWorld.Situations
            .Where(s => finalizedScene.SituationIds.Contains(s.Id))
            .ToList();

        Assert.Equal(3, situations.Count);

        // Verify situation flow (negotiate at NPC location, rest at private room, depart at private room)
        Situation negotiateSit = situations.FirstOrDefault(s => s.Template.Id.Contains("negotiate"));
        Situation restSit = situations.FirstOrDefault(s => s.Template.Id.Contains("rest"));
        Situation departSit = situations.FirstOrDefault(s => s.Template.Id.Contains("depart"));

        Assert.NotNull(negotiateSit);
        Assert.NotNull(restSit);
        Assert.NotNull(departSit);

        // Rest and depart require resolved private room (not marker)
        Assert.NotNull(restSit.ResolvedRequiredLocationId);
        Assert.NotEqual("generated:private_room", restSit.ResolvedRequiredLocationId);  // Should be resolved to actual ID
        Assert.NotNull(departSit.ResolvedRequiredLocationId);
        Assert.Equal(restSit.ResolvedRequiredLocationId, departSit.ResolvedRequiredLocationId);  // Same room
    }
}
