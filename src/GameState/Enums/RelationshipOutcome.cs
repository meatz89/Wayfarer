
/// <summary>
/// Social impact of how scene was resolved
/// Provides semantic context for AI narrative generation and future situation availability
/// </summary>
public enum RelationshipOutcome
{
/// <summary>
/// Made enemies, damaged relationships
/// Typically from Violence resolution
/// </summary>
Hostile,

/// <summary>
/// No relationship established or maintained
/// Typically from Bypass, Stealth, or simple Resolution
/// </summary>
Neutral,

/// <summary>
/// Built positive relationship
/// Typically from Diplomacy or successful Transform
/// </summary>
Friendly,

/// <summary>
/// Deep alliance formed, strong bond
/// Typically from deep Transform investment
/// </summary>
Allied,

/// <summary>
/// Favors owed or owing, future expectations
/// Typically from negotiated Transform with strings attached
/// </summary>
Obligated
}
