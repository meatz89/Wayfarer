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
/// Tests INTEGRATION and PLAYER EXPERIENCE:
/// - Right templates chosen for right contexts
/// - Content appears at correct places
/// - Choices are displayed and executable
/// - Effects are processed correctly
/// - Full player flow works end-to-end
///
/// Does NOT test structural details or exact numbers
/// </summary>
public class TutorialInnLodgingIntegrationTest
{
    [Fact]
    public void Tutorial_ChoosesInnLodgingArchetype()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");

        // ACT
        SceneTemplate tutorialScene = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "tutorial_secure_lodging");

        // ASSERT: Tutorial references inn_lodging archetype (reusable pattern)
        Assert.NotNull(tutorialScene);
        Assert.NotEmpty(tutorialScene.SituationTemplates);
        Assert.NotNull(tutorialScene.SpawnRules);
        Assert.True(tutorialScene.IsStarter);
        Assert.Equal(PresentationMode.Modal, tutorialScene.PresentationMode);
    }

    [Fact]
    public void InnLodging_GeneratesMultiSituationSequence()
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

        // ASSERT: Multi-situation sequence generated (not single-beat)
        Assert.NotEmpty(definition.SituationTemplates);
        Assert.True(definition.SituationTemplates.Count > 1, "Inn lodging should be multi-situation sequence");

        // Linear progression pattern
        Assert.Equal(SpawnPattern.Linear, definition.SpawnRules.Pattern);
        Assert.NotNull(definition.SpawnRules.InitialSituationId);
        Assert.NotEmpty(definition.SpawnRules.Transitions);

        // Generates dependent resources for lodging
        Assert.NotEmpty(definition.DependentLocations);
        Assert.NotEmpty(definition.DependentItems);
    }

    [Fact]
    public void InnLodging_FirstSituation_HasMultipleChoicePaths()
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

        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 0, context);

        // ACT
        SituationTemplate firstSituation = definition.SituationTemplates
            .FirstOrDefault(s => s.Id == definition.SpawnRules.InitialSituationId);

        // ASSERT: Multiple choice paths available
        Assert.NotNull(firstSituation);
        Assert.NotEmpty(firstSituation.ChoiceTemplates);

        // Has money path (costs coins)
        Assert.Contains(firstSituation.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.InstantSuccess &&
            c.CostTemplate?.Coins > 0);

        // Has stat-gated path (requires check)
        Assert.Contains(firstSituation.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.InstantSuccess &&
            c.RequirementFormula != null);

        // Has challenge path (tactical gameplay)
        Assert.Contains(firstSituation.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.Challenge);

        // Has fallback path (always available)
        Assert.Contains(firstSituation.ChoiceTemplates, c =>
            c.PathType == ChoicePathType.Fallback);
    }

    [Fact]
    public void InnLodging_SuccessPaths_GrantRoomAccess()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        GenerationContext context = GenerationContext.FromEntities(tier: 0, elena, commonRoom, player);
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 0, context);
        SituationTemplate firstSituation = definition.SituationTemplates
            .FirstOrDefault(s => s.Id == definition.SpawnRules.InitialSituationId);

        // ACT & ASSERT: Success paths grant access to generated room
        foreach (ChoiceTemplate choice in firstSituation.ChoiceTemplates)
        {
            if (choice.PathType == ChoicePathType.Fallback)
                continue;  // Fallback doesn't grant access

            bool grantsRoomAccess =
                (choice.RewardTemplate?.LocationsToUnlock?.Any(id => id.Contains("room")) ?? false) ||
                (choice.OnSuccessReward?.LocationsToUnlock?.Any(id => id.Contains("room")) ?? false);

            Assert.True(grantsRoomAccess,
                $"Success path {choice.PathType} should grant room access");
        }
    }

    [Fact]
    public void InnLodging_SubsequentSituations_UseGeneratedRoom()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        GenerationContext context = GenerationContext.FromEntities(tier: 0, elena, commonRoom, player);
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 0, context);

        SituationTemplate firstSituation = definition.SituationTemplates
            .FirstOrDefault(s => s.Id == definition.SpawnRules.InitialSituationId);

        // ACT: Find situations after first
        List<SituationTemplate> subsequentSituations = definition.SituationTemplates
            .Where(s => s.Id != firstSituation.Id)
            .ToList();

        // ASSERT: Subsequent situations require generated room
        Assert.NotEmpty(subsequentSituations);

        foreach (SituationTemplate situation in subsequentSituations)
        {
            bool requiresGeneratedRoom =
                situation.RequiredLocationId?.Contains("generated:") ?? false;

            Assert.True(requiresGeneratedRoom,
                $"Situation {situation.Id} should require generated room");
        }
    }

    [Fact]
    public void InnLodging_FinalSituation_CleansUpResources()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        GenerationContext context = GenerationContext.FromEntities(tier: 0, elena, commonRoom, player);
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 0, context);

        // ACT: Find final situation (no outgoing transitions)
        SituationTemplate finalSituation = definition.SituationTemplates
            .FirstOrDefault(s => !definition.SpawnRules.Transitions.Any(t => t.SourceSituationId == s.Id));

        // ASSERT: Final situation cleans up generated resources
        Assert.NotNull(finalSituation);
        Assert.NotEmpty(finalSituation.ChoiceTemplates);

        foreach (ChoiceTemplate choice in finalSituation.ChoiceTemplates)
        {
            bool removesItems = choice.RewardTemplate?.ItemsToRemove?.Any() ?? false;
            bool locksLocations = choice.RewardTemplate?.LocationsToLock?.Any() ?? false;

            Assert.True(removesItems || locksLocations,
                $"Final situation choice should clean up resources");
        }
    }

    [Fact]
    public void InnLodging_CategoricalPropertiesApply()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");
        Player player = gameWorld.GetPlayer();

        // ACT
        GenerationContext context = GenerationContext.FromEntities(tier: 0, elena, commonRoom, player);

        // ASSERT: Categorical properties derived from entities
        Assert.NotEqual(default(Quality), context.Quality);
        Assert.NotEqual(default(EnvironmentQuality), context.Environment);
        Assert.NotEqual(default(NPCDemeanor), context.NpcDemeanor);
        Assert.NotEqual(default(PowerDynamic), context.Power);
        Assert.Equal(PersonalityType.MERCANTILE, context.NpcPersonality);
    }

    [Fact]
    public void InnLodging_CompleteIntegrationFlow()
    {
        // ARRANGE: Full game world with real facades
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        Player player = gameWorld.GetPlayer();
        NPC elena = gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
        Location commonRoom = gameWorld.Locations.FirstOrDefault(l => l.Id == "common_room");

        GenerationContext context = GenerationContext.FromEntities(tier: 0, elena, commonRoom, player);
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 0, context);

        SpawnConditionsEvaluator spawnEvaluator = new SpawnConditionsEvaluator(gameWorld);
        SceneNarrativeService narrativeService = new SceneNarrativeService(gameWorld);
        MarkerResolutionService markerService = new MarkerResolutionService();
        SceneInstantiator instantiator = new SceneInstantiator(gameWorld, spawnEvaluator, narrativeService, markerService);
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

        // ASSERT: Complete integration succeeds
        Assert.Equal(SceneState.Active, finalizedScene.State);
        Assert.NotEmpty(finalizedScene.Situations);

        // Dependent resources generated
        Assert.True(specs.HasResources);
        Assert.NotEmpty(specs.Locations);
        Assert.NotEmpty(specs.Items);

        // Markers resolved to actual IDs
        Assert.NotEmpty(finalizedScene.MarkerResolutionMap);
        Assert.All(finalizedScene.MarkerResolutionMap.Values, resolvedId =>
            Assert.False(resolvedId.Contains("generated:"), "Markers should be resolved to actual IDs"));

        // Situations created and can be queried
        List<Situation> situations = finalizedScene.Situations;

        Assert.NotEmpty(situations);
        Assert.All(situations, situation =>
        {
            Assert.NotNull(situation.Template);
            Assert.NotEmpty(situation.Template.ChoiceTemplates);
        });

        // Situations reference resolved location IDs (not markers)
        Assert.All(situations.Where(s => s.ResolvedRequiredLocationId != null), situation =>
            Assert.False(situation.ResolvedRequiredLocationId.Contains("generated:"),
                "Situation should use resolved location ID, not marker"));
    }

    [Fact]
    public void ConsequenceReflection_LocationOnly_WorksWithoutNPC()
    {
        // ARRANGE: Location-only scene (no NPC)
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        Player player = gameWorld.GetPlayer();
        Location fountainPlaza = gameWorld.Locations.FirstOrDefault(l => l.Id == "fountain_plaza");

        Assert.NotNull(fountainPlaza);

        // ACT: Generate scene with NO NPC (location-only)
        GenerationContext context = GenerationContext.FromEntities(
            tier: 0,
            npc: null,  // NO NPC - location-only scene
            location: fountainPlaza,
            player: player);

        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(
            "consequence_reflection",
            tier: 0,
            context);

        // ASSERT: Scene generated successfully
        Assert.NotNull(definition);
        Assert.NotEmpty(definition.SituationTemplates);

        SituationTemplate situation = definition.SituationTemplates[0];

        // RequiredLocationId uses base location (fountain_plaza)
        Assert.Equal(fountainPlaza.Id, situation.RequiredLocationId);

        // No NPC required (solo reflection)
        Assert.Null(situation.RequiredNpcId);

        // Choices exist (universal scaling applied even without NPC)
        Assert.NotEmpty(situation.ChoiceTemplates);

        // Context has location properties derived
        Assert.NotNull(context.LocationId);
        Assert.Equal(fountainPlaza.Id, context.LocationId);
        Assert.Null(context.NpcId);

        // Categorical properties derived from location only
        Assert.NotEqual(default(Quality), context.Quality);
        Assert.NotEqual(default(EnvironmentQuality), context.Environment);

        // NPC-derived properties use defaults
        Assert.Equal(NPCDemeanor.Neutral, context.NpcDemeanor);
        Assert.Equal(PowerDynamic.Equal, context.Power);
        Assert.Equal(EmotionalTone.Cold, context.Tone);
    }

    [Fact]
    public void Tutorial_BothScenesUseCorrectArchetypes()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");

        // ACT
        SceneTemplate innLodgingScene = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "tutorial_secure_lodging");
        SceneTemplate consequenceScene = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "tutorial_rough_morning");

        // ASSERT: Both tutorial scenes exist and use correct archetypes
        Assert.NotNull(innLodgingScene);
        Assert.NotNull(consequenceScene);

        // inn_lodging = multi-situation with NPC
        Assert.NotEmpty(innLodgingScene.SituationTemplates);
        Assert.True(innLodgingScene.SituationTemplates.Count > 1);

        // consequence_reflection = single-situation without NPC
        Assert.NotEmpty(consequenceScene.SituationTemplates);
        Assert.Equal(SpawnPattern.Standalone, consequenceScene.SpawnRules.Pattern);
    }
}
