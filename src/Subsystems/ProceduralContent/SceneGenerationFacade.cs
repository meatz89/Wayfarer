
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
/// HIGHLANDER COMPLIANT: ONE SceneArchetypeCatalog for ALL scene types
/// No routing logic - all 13 archetypes handled by single catalogue
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
    /// Generate scene structure from archetype type with entity context
    ///
    /// Flow:
    /// 1. Extract properties from entities into GenerationContext
    /// 2. Call SceneArchetypeCatalog.Generate() directly (ONE catalogue for ALL archetypes)
    /// 3. Return generated SceneArchetypeDefinition
    ///
    /// Called at parse time (or via dynamic package generation) with entities from GameWorld
    /// HIGHLANDER: Accept NPC and Location objects, not string IDs
    ///
    /// RhythmPattern is AUTHORED (not derived) - comes from SceneTemplate.
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    public SceneArchetypeDefinition GenerateSceneFromArchetype(
        SceneArchetypeType archetypeType,
        NPC contextNPC,
        Location contextLocation,
        int? mainStorySequence = null,
        RhythmPattern rhythm = RhythmPattern.Mixed)
    {
        Player contextPlayer = _gameWorld.GetPlayer();

        GenerationContext context = GenerationContext.FromEntities(contextNPC, contextLocation, contextPlayer, mainStorySequence, rhythm);

        // HIGHLANDER: ONE catalogue handles ALL 13 archetypes (service + narrative)
        SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(archetypeType, context);

        return definition;
    }

    /// <summary>
    /// Generate scene structure from archetype ID string (backward compatibility)
    /// Parses string to enum and delegates to strongly-typed method
    /// </summary>
    public SceneArchetypeDefinition GenerateSceneFromArchetype(
        string archetypeId,
        NPC contextNPC,
        Location contextLocation,
        int? mainStorySequence = null)
    {
        if (string.IsNullOrEmpty(archetypeId))
            throw new ArgumentException("archetypeId cannot be null or empty", nameof(archetypeId));

        if (!Enum.TryParse<SceneArchetypeType>(archetypeId, true, out SceneArchetypeType archetypeType))
        {
            throw new InvalidOperationException(
                $"Unknown scene archetype: '{archetypeId}'. " +
                $"Valid archetypes: {string.Join(", ", Enum.GetNames<SceneArchetypeType>())}");
        }

        return GenerateSceneFromArchetype(archetypeType, contextNPC, contextLocation, mainStorySequence);
    }

    /// <summary>
    /// Validate scene template completeness and correctness
    ///
    /// Wraps SceneTemplateValidator.Validate() (pure validation logic)
    ///
    /// Called after generation, before storing template in GameWorld
    /// </summary>
    public SceneValidationResult ValidateTemplate(SceneTemplate template)
    {
        return SceneTemplateValidator.Validate(template);
    }

    /// <summary>
    /// Get list of available scene archetype types
    ///
    /// Returns all SceneArchetypeType enum values
    /// Used for developer tools, debug visualization, error messages
    /// </summary>
    public List<SceneArchetypeType> GetAvailableArchetypes()
    {
        return Enum.GetValues<SceneArchetypeType>().ToList();
    }
}
