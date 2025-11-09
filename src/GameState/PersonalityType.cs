/// <summary>
/// Categorical personality types for NPCs based on medieval social archetypes.
/// These types enable personality-specific conversation cards while preserving
/// the authentic human descriptions in NPC data.
/// </summary>
public enum PersonalityType
{
/// <summary>
/// Family-oriented, faith-driven NPCs with high emotional investment.
/// Examples: "Intelligent and disconnected", "Caring and worried"
/// Cards: Deep personal connection, genuine concern, emotional support
/// </summary>
DEVOTED,

/// <summary>
/// Business-focused NPCs who view interactions through trade lens.
/// Examples: "Businesslike and hurried", "Practical and efficient"
/// Cards: Trade negotiations, mutual benefit, practical solutions
/// </summary>
MERCANTILE,

/// <summary>
/// Status-conscious NPCs concerned with hierarchy and respect.
/// Examples: "Formal and insistent", "Impatient and powerful"
/// Cards: Proper protocol, social standing, formal requests
/// </summary>
PROUD,

/// <summary>
/// Information-focused NPCs who deal in secrets and hidden knowledge.
/// Examples: "Mysterious and observant", "Calculating and discrete"
/// Cards: Reading between lines, sharing information, testing loyalty
/// </summary>
CUNNING,

/// <summary>
/// Duty-bound NPCs with strong moral compass and reliability.
/// Examples: "Stern and dutiful", "Friendly and observant"
/// Cards: Honor-based interactions, plain speaking, respect for duty
/// </summary>
STEADFAST
}