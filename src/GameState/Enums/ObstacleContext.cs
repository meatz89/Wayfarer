namespace Wayfarer.GameState.Enums;

/// <summary>
/// Strongly-typed contexts for obstacles and equipment matching.
/// Equipment with matching contexts can reduce obstacle intensity.
/// Source: refinement-core-gameplay-loop.md lines 425-427
/// </summary>
public enum ObstacleContext
{
    // Physical contexts - environmental challenges requiring physical capability
    Climbing,
    Water,
    Strength,
    Precision,
    Endurance,
    Combat,
    Height,
    Cold,
    Navigation,
    Securing,

    // Mental contexts - investigation and observation challenges
    Darkness,
    Mechanical,
    Spatial,
    Deduction,
    Search,
    Pattern,
    Memory,
    Code,

    // Social contexts - interpersonal and relationship challenges
    Authority,
    Deception,
    Persuasion,
    Intimidation,
    Empathy,
    Negotiation,
    Etiquette
}
