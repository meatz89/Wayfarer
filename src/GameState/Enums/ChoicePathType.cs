
/// <summary>
/// UNIVERSAL categorical property: Type of choice path for reward/consequence routing.
///
/// Replaces ID string matching antipattern (.EndsWith("_stat"), .EndsWith("_money")).
/// Scene archetypes route rewards by PathType instead of parsing IDs.
///
/// FORBIDDEN: `if (choice.Id.EndsWith("_stat"))` (CLAUDE.md violation)
/// CORRECT: `if (choice.PathType == ChoicePathType.InstantSuccess)`
///
/// Situation archetypes set this when generating choices.
/// Scene archetypes switch on this for reward enrichment.
/// </summary>
public enum ChoicePathType
{
    /// <summary>
    /// Instant success path: Stat-gated or money-gated choice.
    /// Rewards applied immediately upon selection.
    /// Examples: Pay coins, use rapport/authority/insight stat check
    /// </summary>
    InstantSuccess,

    /// <summary>
    /// Challenge path: Starts tactical challenge (Social/Mental/Physical).
    /// Rewards split between OnSuccessReward and OnFailureReward.
    /// Examples: Negotiate (Social), Work puzzle (Mental), Fight (Physical)
    /// </summary>
    Challenge,

    /// <summary>
    /// Fallback path: Always available, minimal/no rewards.
    /// Player can always choose this (no gating).
    /// Examples: Politely decline, Give up, Submit, Flee
    /// </summary>
    Fallback
}
