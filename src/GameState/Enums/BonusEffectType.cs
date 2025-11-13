/// <summary>
/// Effect type for player stat level bonuses.
/// Categorical effect types instead of string-based switching.
/// Parse-time translation from BonusDTO.Effect string to enum.
/// </summary>
public enum BonusEffectType
{
    /// <summary>
    /// No special effect
    /// </summary>
    None,

    /// <summary>
    /// Grants Thought card persistence (cards don't expire)
    /// </summary>
    GainsThoughtPersistence
}
