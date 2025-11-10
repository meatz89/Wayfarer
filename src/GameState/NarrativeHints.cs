/// <summary>
/// Hints for AI narrative generation
/// Provides context and tone guidance without prescribing exact text
/// </summary>
public class NarrativeHints
{
/// <summary>
/// Tone of the situation narrative
/// Examples: "urgent", "casual", "tense", "mysterious", "humorous"
/// </summary>
public string Tone { get; set; }

/// <summary>
/// Theme or context category
/// Examples: "work", "investigation", "relationship", "combat", "negotiation"
/// </summary>
public string Theme { get; set; }

/// <summary>
/// Contextual details for AI to weave in
/// Examples: "references broken equipment", "mentions weather", "hints at danger"
/// </summary>
public string Context { get; set; }

/// <summary>
/// Narrative style guideline
/// Examples: "formulaic_work", "dramatic_reveal", "subtle_foreshadowing"
/// </summary>
public string Style { get; set; }
}
