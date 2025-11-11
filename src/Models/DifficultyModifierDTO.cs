/// <summary>
/// DTO for difficulty modifiers
/// Maps to DifficultyModifier entity
/// Type stored as string for JSON parsing (converted to enum)
/// </summary>
public class DifficultyModifierDTO
{
    /// <summary>
    /// Modifier type as string (Understanding, Mastery, Familiarity, etc.)
    /// Parsed to ModifierType enum
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Context for modifier (location ID, NPC ID, property name, category name)
    /// Can be null for global resources like Understanding
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// Minimum resource value needed
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>
    /// Effect on difficulty (usually negative for reduction)
    /// </summary>
    public int Effect { get; set; }
}
