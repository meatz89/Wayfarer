/// <summary>
/// Strongly-typed result for player stats parsing operations.
/// Replaces tuple usage in PlayerStatParser.ParseStatsPackage().
/// </summary>
public record PlayerStatsParseResult
{
    /// <summary>
    /// List of parsed player stat definitions
    /// </summary>
    public List<PlayerStatDefinition> StatDefinitions { get; init; }

    /// <summary>
    /// Parsed stat progression rules
    /// </summary>
    public StatProgression Progression { get; init; }

    /// <summary>
    /// Create player stats parse result
    /// </summary>
    public PlayerStatsParseResult(List<PlayerStatDefinition> statDefinitions, StatProgression progression)
    {
        StatDefinitions = statDefinitions ?? new List<PlayerStatDefinition>();
        Progression = progression;
    }
}
