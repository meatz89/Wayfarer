/// <summary>
/// Special traits that modify card behavior.
/// Cards can have multiple traits via flags or a list.
/// Extensible enum for future mechanical overrides.
/// </summary>
public enum CardTrait
{
    /// <summary>
    /// No special traits
    /// </summary>
    None = 0,

    /// <summary>
    /// Suppress the +1 Cadence penalty from SPEAK action.
    /// Used by Rapport cards that manage their own Cadence effects.
    /// </summary>
    SuppressSpeakCadence = 1,

    // Future traits can be added here:
    // IgnorePersonalityRules = 2,
    // DoubleStatementCount = 4,
    // BypassInitiativeCost = 8,
    // etc.
}