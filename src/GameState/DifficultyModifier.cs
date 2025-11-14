/// <summary>
/// Difficulty modifier - reduces challenge difficulty based on player resources
/// No boolean gates: All situations always visible, difficulty varies transparently
/// Graduated benefits: Higher resources = greater difficulty reduction
/// </summary>
public class DifficultyModifier
{
    /// <summary>
    /// Type of resource or property being checked
    /// </summary>
    public ModifierType Type { get; set; }

    /// <summary>
    /// Context for the modifier (if needed)
    /// ARCHITECTURAL CHANGE: Familiarity and ConnectionTokens use situation.Location/Npc directly
    /// - Understanding: null (global resource)
    /// - Mastery: Challenge type ("Combat", "Athletics", etc.)
    /// - Familiarity: NOT USED (uses situation.Location instead)
    /// - ConnectionTokens: NOT USED (uses situation.Npc instead)
    /// - SceneProperty: Property name (always "Intensity" now)
    /// - HasItemCategory: ItemCategory enum value as string ("Light_Source", "Navigation_Tools", etc.)
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// Minimum resource value needed for modifier to apply
    /// - Understanding: 0-10
    /// - Mastery: 0-3
    /// - Familiarity: 0-3
    /// - ConnectionTokens: 0-15
    /// - SceneProperty: Maximum threshold (inverted: lower is better)
    /// - HasItemCategory: Not used (presence check only)
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>
    /// Effect on difficulty when threshold met
    /// Usually negative (reduction), can be positive (increase)
    /// Example: -3 reduces Exposure/Danger/Doubt by 3 points
    /// </summary>
    public int Effect { get; set; }
}

/// <summary>
/// Types of difficulty modifiers
/// NO ID MATCHING: Only mechanical properties and numerical resources
/// All situations always visible, difficulty varies based on player state
/// </summary>
public enum ModifierType
{
    /// <summary>
    /// Global Mental expertise (0-10 scale)
    /// Accumulated through ALL Mental challenges (+1 to +3 based on difficulty)
    /// Never depletes (permanent player growth)
    /// Competition: Multiple obligations need it, limited Focus/Time to accumulate
    /// Context: null (global resource)
    /// Threshold: Minimum Understanding needed (e.g., 2)
    /// Effect: Exposure reduction (e.g., -3)
    /// </summary>
    Understanding,

    /// <summary>
    /// Physical expertise per challenge type (0-3 scale per type)
    /// Accumulated through Physical challenges of specific types
    /// Never depletes (cumulative per-type growth)
    /// Competition: Multiple physical scenes need it, limited Stamina to accumulate
    /// Context: Challenge type ("Combat", "Athletics", "Finesse", etc.)
    /// Threshold: Minimum Mastery needed for that type (e.g., 2)
    /// Effect: Danger reduction (e.g., -3)
    /// </summary>
    Mastery,

    /// <summary>
    /// Location understanding (0-3 scale per location)
    /// Accumulated through Mental challenges at that specific location
    /// Never depletes (cumulative per-location growth)
    /// Competition: Multiple locations need obligation, limited Focus
    /// Context: NOT USED (service uses situation.Location directly)
    /// Threshold: Minimum Familiarity needed (e.g., 2)
    /// Effect: Exposure or Doubt reduction (e.g., -2)
    /// </summary>
    Familiarity,

    /// <summary>
    /// NPC relationship strength (0-15 scale per NPC)
    /// Accumulated through Social challenges with that specific NPC
    /// Can decrease through obligation failures
    /// Competition: Multiple NPCs need relationship building, limited Time
    /// Context: NOT USED (service uses situation.Npc directly)
    /// Threshold: Minimum Connection Tokens needed (e.g., 5)
    /// Effect: Doubt rate reduction (e.g., -4)
    /// </summary>
    ConnectionTokens,

    /// <summary>
    /// Scene property threshold check
    /// Checks if scene intensity meets threshold
    /// Context: Property name (always "Intensity" now)
    /// Threshold: Maximum intensity value (inverted logic: lower is better)
    /// Effect: Difficulty change (can be positive or negative)
    /// Example: If Intensity <= 2, reduce difficulty by 3
    /// Example: If Intensity > 3, increase difficulty by 2
    /// </summary>
    SceneProperty,

    /// <summary>
    /// Equipment category presence (MECHANICAL PROPERTY, NOT ID)
    /// Checks if player has ANY item with this category
    /// Context: ItemCategory enum value as string ("Light_Source", "Navigation_Tools", etc.)
    /// Threshold: Not used (presence check only)
    /// Effect: Difficulty reduction
    /// Example: HasItemCategory("Light_Source") matches: Lantern, Torch, Candle
    /// NEVER matches specific item IDs like "lantern"
    /// This is the CORRECT pattern from RouteOption.cs:305-323
    /// </summary>
    HasItemCategory
}
