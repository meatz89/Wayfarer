using Xunit;

namespace Wayfarer.Tests.Project.Content;

/// <summary>
/// Tests for hierarchical placement filter resolution and categorical dimension matching
/// Validates CSS-style inheritance: effectiveFilter = situationFilter ?? sceneBaseFilter
/// Tests multi-dimensional categorical matching for entity resolution
/// </summary>
public class HierarchicalPlacementTests
{
    /// <summary>
    /// Test CSS-style fallback: situation with no filter inherits scene base filter
    /// </summary>
    [Fact]
    public void SituationInheritsSceneBaseLocationFilter_WhenSituationHasNoOverride()
    {
        // ARRANGE: Scene has base location filter for common_room
        PlacementFilterDTO sceneBaseLocationFilter = new PlacementFilterDTO
        {
            PlacementType = "Location",
            LocationId = "common_room"
        };

        SituationDTO situationDto = new SituationDTO
        {
            Id = "test_situation",
            TemplateId = "test_template",
            LocationFilter = null // No override - should inherit scene base
        };

        SceneDTO sceneDto = new SceneDTO
        {
            Id = "test_scene",
            TemplateId = "test_template",
            State = "Active",
            LocationFilter = sceneBaseLocationFilter,
            Situations = new List<SituationDTO> { situationDto }
        };

        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // ACT: Parse scene with hierarchical resolution
        Scene scene = SceneParser.ConvertDTOToScene(sceneDto, gameWorld, entityResolver);

        // ASSERT: Situation inherited scene's base location filter
        Situation situation = scene.Situations[0];
        Assert.NotNull(situation.Location);
        Assert.Equal("common_room", situation.Location.Id);
    }

    /// <summary>
    /// Test CSS-style override: situation with own filter overrides scene base filter
    /// </summary>
    [Fact]
    public void SituationOverridesSceneBaseLocationFilter_WhenSituationHasOwnFilter()
    {
        // ARRANGE: Scene has base filter for common_room, situation overrides to private_room
        PlacementFilterDTO sceneBaseLocationFilter = new PlacementFilterDTO
        {
            PlacementType = "Location",
            LocationId = "common_room"
        };

        PlacementFilterDTO situationLocationFilter = new PlacementFilterDTO
        {
            PlacementType = "Location",
            LocationId = "fountain_plaza"
        };

        SituationDTO situationDto = new SituationDTO
        {
            Id = "test_situation",
            TemplateId = "test_template",
            LocationFilter = situationLocationFilter // Override scene base
        };

        SceneDTO sceneDto = new SceneDTO
        {
            Id = "test_scene",
            TemplateId = "test_template",
            State = "Active",
            LocationFilter = sceneBaseLocationFilter,
            Situations = new List<SituationDTO> { situationDto }
        };

        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // ACT
        Scene scene = SceneParser.ConvertDTOToScene(sceneDto, gameWorld, entityResolver);

        // ASSERT: Situation used its own filter, not scene base
        Situation situation = scene.Situations[0];
        Assert.NotNull(situation.Location);
        Assert.Equal("fountain_plaza", situation.Location.Id);
    }

    /// <summary>
    /// Test multi-dimensional NPC matching: all specified dimensions must match
    /// </summary>
    [Fact]
    public void NPCMatching_AllCategoricalDimensions_MustMatch()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // Filter requires: Notable + Facilitator + Informed
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            SocialStandings = new List<NPCSocialStanding> { NPCSocialStanding.Notable },
            StoryRoles = new List<NPCStoryRole> { NPCStoryRole.Facilitator },
            KnowledgeLevels = new List<NPCKnowledgeLevel> { NPCKnowledgeLevel.Informed }
        };

        // ACT: Find NPCs matching all three dimensions
        NPC resolvedNpc = entityResolver.FindOrCreateNPC(filter);

        // ASSERT: Should find Elena (Notable + Facilitator + Informed)
        Assert.NotNull(resolvedNpc);
        Assert.Equal("elena", resolvedNpc.ID);
        Assert.Equal(NPCSocialStanding.Notable, resolvedNpc.SocialStanding);
        Assert.Equal(NPCStoryRole.Facilitator, resolvedNpc.StoryRole);
        Assert.Equal(NPCKnowledgeLevel.Informed, resolvedNpc.KnowledgeLevel);
    }

    /// <summary>
    /// Test multi-dimensional NPC matching: partial match should fail
    /// </summary>
    [Fact]
    public void NPCMatching_PartialDimensionMatch_ShouldFail()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // Filter requires: Notable + Obstacle + Expert
        // Elena is Notable + Facilitator + Informed (wrong on 2 dimensions)
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            SocialStandings = new List<NPCSocialStanding> { NPCSocialStanding.Notable },
            StoryRoles = new List<NPCStoryRole> { NPCStoryRole.Obstacle }, // Elena is Facilitator
            KnowledgeLevels = new List<NPCKnowledgeLevel> { NPCKnowledgeLevel.Expert } // Elena is Informed
        };

        // ACT: Try to find NPC
        NPC resolvedNpc = entityResolver.FindOrCreateNPC(filter);

        // ASSERT: Should generate new NPC (no existing match)
        Assert.NotNull(resolvedNpc);
        Assert.NotEqual("elena", resolvedNpc.ID);
        Assert.Contains("generated:", resolvedNpc.ID);
    }

    /// <summary>
    /// Test location matching with multiple categorical dimensions
    /// </summary>
    [Fact]
    public void LocationMatching_MultipleDimensions_AllMustMatch()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // Filter requires: SemiPublic + Safe + Moderate + Dwelling
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            PrivacyLevels = new List<LocationPrivacy> { LocationPrivacy.SemiPublic },
            SafetyLevels = new List<LocationSafety> { LocationSafety.Safe },
            ActivityLevels = new List<LocationActivity> { LocationActivity.Moderate },
            Purposes = new List<LocationPurpose> { LocationPurpose.Dwelling }
        };

        // ACT
        Location resolvedLocation = entityResolver.FindOrCreateLocation(filter);

        // ASSERT: Should find common_room (SemiPublic + Safe + Moderate + Dwelling)
        Assert.NotNull(resolvedLocation);
        Assert.Equal("common_room", resolvedLocation.Id);
        Assert.Equal(LocationPrivacy.SemiPublic, resolvedLocation.Privacy);
        Assert.Equal(LocationSafety.Safe, resolvedLocation.Safety);
        Assert.Equal(LocationActivity.Moderate, resolvedLocation.Activity);
        Assert.Equal(LocationPurpose.Dwelling, resolvedLocation.Purpose);
    }

    /// <summary>
    /// Test OR logic within single dimension: match ANY value in list
    /// </summary>
    [Fact]
    public void CategoricalMatching_OrLogicWithinDimension_MatchAnyValue()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // Filter accepts multiple values in one dimension: Notable OR Authority
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            SocialStandings = new List<NPCSocialStanding>
            {
                NPCSocialStanding.Notable,
                NPCSocialStanding.Authority
            },
            StoryRoles = new List<NPCStoryRole> { NPCStoryRole.Facilitator },
            KnowledgeLevels = new List<NPCKnowledgeLevel> { NPCKnowledgeLevel.Informed }
        };

        // ACT
        NPC resolvedNpc = entityResolver.FindOrCreateNPC(filter);

        // ASSERT: Elena matches (Notable is in the list)
        Assert.NotNull(resolvedNpc);
        Assert.Equal("elena", resolvedNpc.ID);
    }

    /// <summary>
    /// Test empty filter list: should match all entities (no restriction)
    /// </summary>
    [Fact]
    public void EmptyDimensionList_MatchesAll_NoRestriction()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // Filter with empty lists = no restrictions on those dimensions
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.NPC,
            SocialStandings = new List<NPCSocialStanding>(), // Empty = no restriction
            StoryRoles = new List<NPCStoryRole>(), // Empty = no restriction
            KnowledgeLevels = new List<NPCKnowledgeLevel>() // Empty = no restriction
        };

        // ACT
        NPC resolvedNpc = entityResolver.FindOrCreateNPC(filter);

        // ASSERT: Should match any NPC (no dimensional restrictions)
        Assert.NotNull(resolvedNpc);
    }

    /// <summary>
    /// Test null filter: should use existing entities or generate
    /// </summary>
    [Fact]
    public void NullPlacementFilter_ShouldReturnNull()
    {
        // ARRANGE
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // ACT
        NPC resolvedNpc = entityResolver.FindOrCreateNPC(null);

        // ASSERT
        Assert.Null(resolvedNpc);
    }

    /// <summary>
    /// Test scene context comparison: same location + NPC = ContinueInScene
    /// </summary>
    [Fact]
    public void ContextComparison_SameLocationAndNpc_ReturnsContinueInScene()
    {
        // ARRANGE: Two situations at same location with same NPC
        Situation situation1 = new Situation
        {
            Id = "sit1",
            Location = new Location { Id = "common_room", Name = "Common Room" },
            Npc = new NPC { ID = "elena", Name = "Elena" }
        };

        Situation situation2 = new Situation
        {
            Id = "sit2",
            Location = new Location { Id = "common_room", Name = "Common Room" },
            Npc = new NPC { ID = "elena", Name = "Elena" }
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            Situations = new List<Situation> { situation1, situation2 }
        };

        // ACT
        SceneRoutingDecision decision = scene.CompareContexts(situation1, situation2);

        // ASSERT: Same context = continue in scene (cascade)
        Assert.Equal(SceneRoutingDecision.ContinueInScene, decision);
    }

    /// <summary>
    /// Test scene context comparison: different location = ExitToWorld
    /// </summary>
    [Fact]
    public void ContextComparison_DifferentLocation_ReturnsExitToWorld()
    {
        // ARRANGE: Two situations at different locations
        Situation situation1 = new Situation
        {
            Id = "sit1",
            Location = new Location { Id = "common_room", Name = "Common Room" },
            Npc = new NPC { ID = "elena", Name = "Elena" }
        };

        Situation situation2 = new Situation
        {
            Id = "sit2",
            Location = new Location { Id = "fountain_plaza", Name = "Fountain Plaza" },
            Npc = new NPC { ID = "elena", Name = "Elena" }
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            Situations = new List<Situation> { situation1, situation2 }
        };

        // ACT
        SceneRoutingDecision decision = scene.CompareContexts(situation1, situation2);

        // ASSERT: Different location = exit to world (breathe)
        Assert.Equal(SceneRoutingDecision.ExitToWorld, decision);
    }

    /// <summary>
    /// Test scene context comparison: different NPC = ExitToWorld
    /// </summary>
    [Fact]
    public void ContextComparison_DifferentNpc_ReturnsExitToWorld()
    {
        // ARRANGE: Two situations with different NPCs
        Situation situation1 = new Situation
        {
            Id = "sit1",
            Location = new Location { Id = "common_room", Name = "Common Room" },
            Npc = new NPC { ID = "elena", Name = "Elena" }
        };

        Situation situation2 = new Situation
        {
            Id = "sit2",
            Location = new Location { Id = "common_room", Name = "Common Room" },
            Npc = new NPC { ID = "thomas", Name = "Thomas" }
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            Situations = new List<Situation> { situation1, situation2 }
        };

        // ACT
        SceneRoutingDecision decision = scene.CompareContexts(situation1, situation2);

        // ASSERT: Different NPC = exit to world
        Assert.Equal(SceneRoutingDecision.ExitToWorld, decision);
    }

    /// <summary>
    /// Test scene resumption: Active scene with matching context should resume
    /// </summary>
    [Fact]
    public void SceneResumption_ActiveSceneAtMatchingContext_ShouldResume()
    {
        // ARRANGE: Active scene waiting at fountain_plaza
        Location fountainPlaza = new Location { Id = "fountain_plaza", Name = "Fountain Plaza" };

        SituationTemplate template = new SituationTemplate
        {
            Id = "test_template"
        };

        Situation currentSituation = new Situation
        {
            Id = "current_sit",
            Template = template,
            Location = fountainPlaza,
            Npc = null
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            State = SceneState.Active,
            CurrentSituation = currentSituation
        };

        // ACT: Check if scene should resume at fountain_plaza
        bool shouldResume = scene.ShouldResumeAtContext("fountain_plaza", null);

        // ASSERT
        Assert.True(shouldResume);
    }

    /// <summary>
    /// Test scene resumption: wrong location should not resume
    /// </summary>
    [Fact]
    public void SceneResumption_WrongLocation_ShouldNotResume()
    {
        // ARRANGE: Active scene waiting at fountain_plaza
        Location fountainPlaza = new Location { Id = "fountain_plaza", Name = "Fountain Plaza" };

        SituationTemplate template = new SituationTemplate
        {
            Id = "test_template"
        };

        Situation currentSituation = new Situation
        {
            Id = "current_sit",
            Template = template,
            Location = fountainPlaza,
            Npc = null
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            State = SceneState.Active,
            CurrentSituation = currentSituation
        };

        // ACT: Player is at common_room, not fountain_plaza
        bool shouldResume = scene.ShouldResumeAtContext("common_room", null);

        // ASSERT
        Assert.False(shouldResume);
    }

    /// <summary>
    /// Test scene resumption: Completed scene should not resume
    /// </summary>
    [Fact]
    public void SceneResumption_CompletedScene_ShouldNotResume()
    {
        // ARRANGE: Completed scene
        Location fountainPlaza = new Location { Id = "fountain_plaza", Name = "Fountain Plaza" };

        SituationTemplate template = new SituationTemplate
        {
            Id = "test_template"
        };

        Situation currentSituation = new Situation
        {
            Id = "current_sit",
            Template = template,
            Location = fountainPlaza,
            Npc = null
        };

        Scene scene = new Scene
        {
            Id = "test_scene",
            State = SceneState.Completed, // Completed, not Active
            CurrentSituation = currentSituation
        };

        // ACT
        bool shouldResume = scene.ShouldResumeAtContext("fountain_plaza", null);

        // ASSERT: Completed scenes never resume
        Assert.False(shouldResume);
    }

    /// <summary>
    /// Test hierarchical NPC filter resolution with null scene base
    /// </summary>
    [Fact]
    public void HierarchicalResolution_NullSceneBase_UseSituationFilter()
    {
        // ARRANGE: Scene has NO base NPC filter, situation provides one
        PlacementFilterDTO situationNpcFilter = new PlacementFilterDTO
        {
            PlacementType = "NPC",
            NpcId = "elena"
        };

        SituationDTO situationDto = new SituationDTO
        {
            Id = "test_situation",
            TemplateId = "test_template",
            NpcFilter = situationNpcFilter
        };

        SceneDTO sceneDto = new SceneDTO
        {
            Id = "test_scene",
            TemplateId = "test_template",
            State = "Active",
            NpcFilter = null, // No scene base
            Situations = new List<SituationDTO> { situationDto }
        };

        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // ACT
        Scene scene = SceneParser.ConvertDTOToScene(sceneDto, gameWorld, entityResolver);

        // ASSERT: Situation used its own filter
        Situation situation = scene.Situations[0];
        Assert.NotNull(situation.Npc);
        Assert.Equal("elena", situation.Npc.ID);
    }

    /// <summary>
    /// Test hierarchical resolution with both scene base and situation override null
    /// </summary>
    [Fact]
    public void HierarchicalResolution_BothNull_NoEntityResolved()
    {
        // ARRANGE: Both scene base and situation filter are null
        SituationDTO situationDto = new SituationDTO
        {
            Id = "test_situation",
            TemplateId = "test_template",
            NpcFilter = null
        };

        SceneDTO sceneDto = new SceneDTO
        {
            Id = "test_scene",
            TemplateId = "test_template",
            State = "Active",
            NpcFilter = null,
            Situations = new List<SituationDTO> { situationDto }
        };

        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        EntityResolver entityResolver = new EntityResolver(gameWorld);

        // ACT
        Scene scene = SceneParser.ConvertDTOToScene(sceneDto, gameWorld, entityResolver);

        // ASSERT: No NPC resolved
        Situation situation = scene.Situations[0];
        Assert.Null(situation.Npc);
    }
}
