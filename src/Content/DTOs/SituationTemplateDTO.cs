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
    /// Optional archetype ID for procedural choice generation
    /// If specified: Parser generates 4 ChoiceTemplates from archetype at parse time
    /// Valid values: "confrontation", "negotiation", "investigation", "social_maneuvering", "crisis"
    /// If null/empty: Use explicit ChoiceTemplates from JSON (existing hand-authored behavior)
    /// BACKWARD COMPATIBLE: Existing content without archetypeId continues working unchanged
    /// </summary>
    public string ArchetypeId { get; set; }

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

    // ==================== HIERARCHICAL PLACEMENT (OVERRIDE FILTERS) ====================
    // CSS-style inheritance: SituationTemplate can OVERRIDE SceneTemplate base filters
    // Resolution: effectiveFilter = situationFilter ?? sceneBaseFilter
    // All nullable - null means "inherit from scene base"

    /// <summary>
    /// Location filter override for this specific situation
    /// null = inherit from SceneTemplate.BaseLocationFilter (CSS-style fallback)
    /// Non-null = override scene base for this situation only
    /// Enables multi-location scenes: "Negotiate" at Common Room, "Rest" at Private Room
    /// </summary>
    public PlacementFilterDTO LocationFilter { get; set; }

    /// <summary>
    /// NPC filter override for this specific situation
    /// null = inherit from SceneTemplate.BaseNpcFilter (CSS-style fallback)
    /// Non-null = override scene base for this situation only
    /// Example: Scene has Innkeeper base, but "Depart" situation has null (no NPC)
    /// </summary>
    public PlacementFilterDTO NpcFilter { get; set; }

    /// <summary>
    /// Route filter override for this specific situation
    /// null = inherit from SceneTemplate.BaseRouteFilter (CSS-style fallback)
    /// Non-null = override scene base for this situation only
    /// Rarely used - most situations don't involve routes
    /// </summary>
    public PlacementFilterDTO RouteFilter { get; set; }

    /// <summary>
    /// AI generation guidance
    /// Provides tone, theme, context for dynamic narrative generation
    /// </summary>
    public NarrativeHintsDTO NarrativeHints { get; set; }
}
