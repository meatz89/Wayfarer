
/// <summary>
/// Determines player readiness for different archetype intensities based on Resolve level.
/// Used by ProceduralAStoryService to filter archetype selection.
///
/// READINESS LEVELS:
/// - Exhausted (Resolve less than 3): Only Peaceful archetypes safe
/// - Normal (Resolve 3-15): Peaceful, Building, Testing archetypes safe
/// - Capable (Resolve greater than 15): All archetypes including Crisis safe
///
/// HIGHLANDER: Single source of truth for player readiness determination.
/// </summary>
public class PlayerReadinessService
{
    /// <summary>
    /// Threshold below which player is considered exhausted.
    /// Only Peaceful archetypes should be offered.
    /// </summary>
    public const int ExhaustedThreshold = 3;

    /// <summary>
    /// Threshold above which player is considered capable.
    /// All archetypes including Crisis can be offered.
    /// </summary>
    public const int CapableThreshold = 15;

    /// <summary>
    /// Get the maximum archetype intensity appropriate for the player's current state.
    /// Archetypes with intensity at or below this level are safe to offer.
    /// </summary>
    public ArchetypeIntensity GetMaxSafeIntensity(Player player)
    {
        if (player.Resolve < ExhaustedThreshold)
        {
            return ArchetypeIntensity.Peaceful;
        }

        if (player.Resolve > CapableThreshold)
        {
            return ArchetypeIntensity.Crisis;
        }

        return ArchetypeIntensity.Testing;
    }

    /// <summary>
    /// Check if a specific archetype intensity is safe for the player.
    /// </summary>
    public bool IsIntensitySafe(Player player, ArchetypeIntensity intensity)
    {
        ArchetypeIntensity maxSafe = GetMaxSafeIntensity(player);
        return intensity <= maxSafe;
    }

    /// <summary>
    /// Get all safe intensity levels for the player.
    /// Used for filtering archetype lists.
    /// </summary>
    public List<ArchetypeIntensity> GetSafeIntensities(Player player)
    {
        ArchetypeIntensity maxSafe = GetMaxSafeIntensity(player);
        List<ArchetypeIntensity> safeIntensities = new List<ArchetypeIntensity>();

        if (maxSafe >= ArchetypeIntensity.Peaceful)
            safeIntensities.Add(ArchetypeIntensity.Peaceful);

        if (maxSafe >= ArchetypeIntensity.Building)
            safeIntensities.Add(ArchetypeIntensity.Building);

        if (maxSafe >= ArchetypeIntensity.Testing)
            safeIntensities.Add(ArchetypeIntensity.Testing);

        if (maxSafe >= ArchetypeIntensity.Crisis)
            safeIntensities.Add(ArchetypeIntensity.Crisis);

        return safeIntensities;
    }

    /// <summary>
    /// Get a descriptive readiness state for UI display.
    /// </summary>
    public PlayerReadinessState GetReadinessState(Player player)
    {
        if (player.Resolve < ExhaustedThreshold)
        {
            return PlayerReadinessState.Exhausted;
        }

        if (player.Resolve > CapableThreshold)
        {
            return PlayerReadinessState.Capable;
        }

        return PlayerReadinessState.Normal;
    }
}

/// <summary>
/// Player readiness state for UI and filtering purposes.
/// </summary>
public enum PlayerReadinessState
{
    /// <summary>
    /// Player is exhausted (Resolve less than 3).
    /// Only peaceful archetypes should be offered.
    /// </summary>
    Exhausted,

    /// <summary>
    /// Player is in normal state (Resolve 3-15).
    /// Peaceful, Building, and Testing archetypes safe.
    /// </summary>
    Normal,

    /// <summary>
    /// Player is capable (Resolve greater than 15).
    /// All archetype intensities are safe including Crisis.
    /// </summary>
    Capable
}
