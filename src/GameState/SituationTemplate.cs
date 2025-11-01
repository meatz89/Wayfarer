/// <summary>
/// Template for Situation - immutable archetype embedded in SceneTemplate
/// Defines one narrative moment with 2-4 choice archetypes
/// NOT a standalone entity - always embedded in SceneTemplate
/// At spawn time, instantiated to Situation with placeholders replaced and embedded in Scene
/// </summary>
public class SituationTemplate
{
    /// <summary>
    /// Unique identifier for this SituationTemplate within its SceneTemplate
    /// Used to track which template spawned which Situation instance
    /// Used by SituationSpawnRules to reference transitions
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Semantic type marking narrative weight (Normal vs Crisis)
    /// Crisis situations are typically final situations in a Scene that test player preparation
    /// with higher stat requirements or expensive alternatives
    /// Defaults to Normal if not specified in JSON
    /// </summary>
    public SituationType Type { get; init; } = SituationType.Normal;

    /// <summary>
    /// Narrative template with placeholders
    /// Example: "As you approach {LocationName}, {NPCName} steps forward nervously..."
    /// Placeholders replaced at spawn time:
    /// - {NPCName} → actual NPC name
    /// - {LocationName} → actual location name
    /// - {PlayerName} → player character name
    /// - {VenueName} → venue name if applicable
    /// Enables one template to generate varied narrative based on placement
    /// </summary>
    public string NarrativeTemplate { get; init; }

    /// <summary>
    /// Choice templates for this Situation (2-4 enforced by Sir Brante pattern)
    /// Each ChoiceTemplate defines one action option with requirements, costs, rewards
    /// At spawn time, instantiated to Choice instances with placeholders replaced
    /// EMBEDDED - ChoiceTemplates stored inline, not referenced by ID
    /// </summary>
    public List<ChoiceTemplate> ChoiceTemplates { get; init; } = new List<ChoiceTemplate>();

    /// <summary>
    /// Priority for display ordering when multiple Situations available
    /// Higher priority Situations appear first in UI
    /// Default: 0 (normal priority)
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Optional narrative hints for AI generation
    /// If NarrativeTemplate is null/empty, AI generates narrative using these hints
    /// Provides tone, theme, context, style guidance
    /// </summary>
    public NarrativeHints NarrativeHints { get; init; }

    /// <summary>
    /// Rewards applied automatically when Situation activates (no player input)
    /// Only used when SceneTemplate.Archetype = AutoAdvance
    /// Scene displays narrative then immediately applies these rewards
    /// Used for narrative transitions (sleep, travel, forced progression)
    /// </summary>
    public ChoiceReward AutoProgressRewards { get; init; }
}
