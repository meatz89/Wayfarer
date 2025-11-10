
/// <summary>
/// Type of consequence when situation succeeds
/// Determines how scene is affected by situation completion
/// </summary>
public enum ConsequenceType
{
/// <summary>
/// Scene permanently overcome, marked Resolved, removed from play
/// Highest costs, often violent, irreversible
/// Sets ResolutionMethod (Violence/Authority typical), RelationshipOutcome (often Hostile/Neutral)
/// </summary>
Resolution,

/// <summary>
/// Player passes, scene persists for world
/// Low cost, quick, reversible, no relationship damage
/// Sets ResolutionMethod (Stealth/Cleverness typical), RelationshipOutcome (Neutral)
/// </summary>
Bypass,

/// <summary>
/// Scene fundamentally changed, all properties set to zero
/// Builds relationships, earns authority, eases future
/// Sets ResolutionMethod (Diplomacy/Authority typical), RelationshipOutcome (Friendly/Allied/Obligated)
/// </summary>
Transform,

/// <summary>
/// Scene properties reduced by specified amounts, other situations may unlock
/// Incremental progress, unlocks approaches, flexible
/// Sets ResolutionMethod (Preparation), RelationshipOutcome (usually Neutral)
/// </summary>
Modify,

/// <summary>
/// Player receives knowledge cards or items, scene unchanged
/// Tactical advantage, informs decisions, low risk
/// No resolution method set (scene not resolved)
/// </summary>
Grant
}
