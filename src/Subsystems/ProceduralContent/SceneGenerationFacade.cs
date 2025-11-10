
/// <summary>
/// FACADE: Clean boundary between game code and scene generation subsystem
///
/// PURPOSE:
/// - Isolates game code from catalogue internals
/// - Provides clean interface for scene generation
/// - Wraps validation pipeline
/// - Exposes available archetypes
///
/// USAGE:
/// - Called ONLY from SceneTemplateParser at parse time
/// - NEVER called at runtime (generation is parse-time only)
///
/// TESTABILITY:
/// - Facade itself is integration layer (thin wrapper)
/// - Generation tested via catalogue directly (isolated pure functions)
/// - Validation tested via validator directly (isolated pure logic)
/// </summary>
public class SceneGenerationFacade
{
private readonly GameWorld _gameWorld;

public SceneGenerationFacade(GameWorld gameWorld)
{
    _gameWorld = gameWorld;
}

/// <summary>
/// Generate scene structure from archetype ID with entity context
///
/// Flow:
/// 1. Extract properties from entities into GenerationContext
/// 2. Route to appropriate catalogue based on archetype ID
///    - A-story archetypes → AStorySceneArchetypeCatalog
///    - Standard archetypes → SceneArchetypeCatalog
/// 3. Return generated SceneArchetypeDefinition
///
/// Called at parse time (or via dynamic package generation) with entities from GameWorld
/// </summary>
public SceneArchetypeDefinition GenerateSceneFromArchetype(
    string archetypeId,
    int tier,
    string npcId,
    string locationId,
    int? mainStorySequence = null)
{
    NPC contextNPC = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
    Location contextLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
    Player contextPlayer = _gameWorld.GetPlayer();

    GenerationContext context = GenerationContext.FromEntities(tier, contextNPC, contextLocation, contextPlayer, mainStorySequence);

    // Route to appropriate catalogue based on archetype category
    SceneArchetypeDefinition definition;

    // A-story archetypes: investigation/social/confrontation/discovery/crisis patterns
    if (IsAStoryArchetype(archetypeId))
    {
        definition = AStorySceneArchetypeCatalog.Generate(archetypeId, tier, context);
    }
    else
    {
        // Standard service/consequence archetypes
        definition = SceneArchetypeCatalog.Generate(archetypeId, tier, context);
    }

    return definition;
}

/// <summary>
/// Check if archetype ID is an A-story archetype
/// A-story archetypes: investigation, social, confrontation, discovery, crisis patterns
/// Standard archetypes: service-based patterns (inn_lodging, consequence_reflection, etc.)
/// </summary>
private bool IsAStoryArchetype(string archetypeId)
{
    List<string> aStoryArchetypes = new List<string>
    {
        // Investigation
        "investigate_location", "gather_testimony", "discover_artifact", "uncover_conspiracy",
        // Social
        "meet_order_member", "gain_trust", "social_infiltration",
        // Confrontation
        "seek_audience", "confront_antagonist", "challenge_authority", "expose_corruption",
        // Crisis/Decision
        "urgent_decision", "moral_crossroads", "sacrifice_choice", "reveal_truth"
    };

    return aStoryArchetypes.Contains(archetypeId?.ToLowerInvariant());
}

/// <summary>
/// Validate scene template completeness and correctness
///
/// Wraps SceneTemplateValidator.Validate() (pure validation logic)
///
/// Called after generation, before storing template in GameWorld
/// </summary>
public Wayfarer.Content.Validation.ValidationResult ValidateTemplate(SceneTemplate template)
{
    return SceneTemplateValidator.Validate(template);
}

/// <summary>
/// Get list of available scene archetype IDs
///
/// Hardcoded list of supported archetypes
/// Used for developer tools, debug visualization, error messages
/// </summary>
public List<string> GetAvailableArchetypes()
{
    return new List<string>
    {
        "inn_lodging",
        "consequence_reflection"
    };
}
}
