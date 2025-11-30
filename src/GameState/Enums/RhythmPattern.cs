
/// <summary>
/// Sir Brante-style narrative rhythm classification for scenes.
/// Determines choice generation pattern and consequence polarity.
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern).
///
/// CATEGORICAL PROPERTY: Authored in SceneTemplate JSON, passed through GenerationContext,
/// used by archetypes to change choice generation without hardcoded sequence checks.
/// </summary>
public enum RhythmPattern
{
    /// <summary>
    /// All choices lead to positive outcomes.
    /// - Stat-gated path: GRANTS stats instead of requiring them
    /// - Money path: Minimal cost or gain
    /// - Challenge path: Low stakes exploration
    /// - Fallback: Different flavor of positive, no penalty
    /// Used for: Character formation, tutorial introduction, recovery periods
    /// </summary>
    Building,

    /// <summary>
    /// All choices about damage mitigation.
    /// - Stat-gated path: AVOIDS penalty if requirement met
    /// - Money path: Pay to avoid penalty
    /// - Challenge path: Risky attempt to avoid penalty
    /// - Fallback: TAKES the penalty (health/stamina/coin loss)
    /// Used for: Crisis escalation, high stakes moments, dramatic tension
    /// </summary>
    Crisis,

    /// <summary>
    /// Standard trade-off gameplay.
    /// - Stat-gated path: Best outcome if requirement met
    /// - Money path: Guaranteed but costly
    /// - Challenge path: Variable outcome with risk
    /// - Fallback: Poor outcome but always available
    /// Used for: Normal gameplay loop, most procedural content
    /// </summary>
    Mixed
}
