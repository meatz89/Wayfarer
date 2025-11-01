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

    /// <summary>
    /// AI generation guidance
    /// Provides tone, theme, context for dynamic narrative generation
    /// </summary>
    public NarrativeHintsDTO NarrativeHints { get; set; }

    /// <summary>
    /// Rewards applied automatically when Situation activates (AutoAdvance archetype)
    /// No player choices required - scene displays narrative then applies rewards
    /// Used for sleep, travel, forced narrative progression
    /// </summary>
    public ChoiceRewardDTO AutoProgressRewards { get; set; }
}
