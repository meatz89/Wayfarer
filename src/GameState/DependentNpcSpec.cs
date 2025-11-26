/// <summary>
/// Specification for generating a dependent NPC as part of self-contained scene
/// Scene defines these specs at authoring time, PackageLoader.CreateSingleNpc generates NPC at runtime
/// Direct creation path - no JSON serialization
/// </summary>
public class DependentNpcSpec
{
    /// <summary>
    /// Template identifier for this NPC specification
    /// Used for tracking/debugging
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// NPC name (generic, descriptive)
    /// Example: "Innkeeper", "Guard", "Merchant"
    /// When AI narrative added, it will regenerate with full context
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// NPC description (generic, descriptive)
    /// Example: "A friendly innkeeper who runs the establishment."
    /// When AI narrative added, it will regenerate with full context
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// NPC role in the scene
    /// Examples: "Merchant", "Guard", "Scholar", "Noble"
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// NPC tier (1-5)
    /// Determines stat ranges and difficulty
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// NPC level within tier
    /// Fine-grained progression within tier
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Personality type for conversation generation
    /// Examples: "Friendly", "Suspicious", "Neutral", "Hostile"
    /// </summary>
    public string PersonalityType { get; set; } = "Neutral";
}
