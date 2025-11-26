/// <summary>
/// DTO for SituationTemplate - individual persistent narrative context archetype
/// Embedded within SceneTemplate, defines a single situation with 2-4 choices
/// Maps to SituationTemplate domain entity
/// </summary>
public class SituationTemplateDTO
{
    /// <summary>
    /// Unique identifier within parent SceneTemplate
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Semantic type marking narrative weight: "Normal" or "Crisis"
    /// Crisis situations test player preparation with high stat requirements
    /// Defaults to "Normal" if not specified in JSON
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Optional archetype for procedural choice generation.
    /// Strongly-typed enum - compile-time validated, no runtime string matching.
    /// If specified: Parser generates 4 ChoiceTemplates from archetype at parse time.
    /// If null: Use explicit ChoiceTemplates from JSON (hand-authored behavior).
    /// </summary>
    public SituationArchetypeType? ArchetypeId { get; set; }

    /// <summary>
    /// Narrative description template with {placeholders}
    /// Example: "{NPCName} asks for your help investigating {LocationName}"
    /// </summary>
    public string NarrativeTemplate { get; set; }

    /// <summary>
    /// Embedded choice templates (composition)
    /// Sir Brante pattern: Each Situation has exactly 2-4 Choices
    /// </summary>
    public List<ChoiceTemplateDTO> ChoiceTemplates { get; set; } = new List<ChoiceTemplateDTO>();

    /// <summary>
    /// Display ordering when multiple situations available
    /// Lower values shown first
    /// </summary>
    public int Priority { get; set; } = 0;

    // ==================== EXPLICIT PLACEMENT FILTERS ====================
    // Each situation must specify explicit filters - NO inheritance from scene
    // null = no entity needed (solo situation, not route-based, etc.)
    // PlacementProximity.SameLocation = use/find at spawn context location

    /// <summary>
    /// Location filter for this situation - EXPLICIT, not inherited
    /// Required for all situations (use Proximity = SameLocation for same spawn location)
    /// </summary>
    public PlacementFilterDTO LocationFilter { get; set; }

    /// <summary>
    /// NPC filter for this situation - EXPLICIT, not inherited
    /// null = solo situation (no NPC needed)
    /// </summary>
    public PlacementFilterDTO NpcFilter { get; set; }

    /// <summary>
    /// Route filter for this situation - EXPLICIT, not inherited
    /// null = not a route-based situation
    /// </summary>
    public PlacementFilterDTO RouteFilter { get; set; }

    /// <summary>
    /// AI generation guidance
    /// Provides tone, theme, context for dynamic narrative generation
    /// </summary>
    public NarrativeHintsDTO NarrativeHints { get; set; }

}
