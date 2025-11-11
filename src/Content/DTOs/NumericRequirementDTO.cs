/// <summary>
/// DTO for Numeric Requirements - individual threshold checks
/// Used within CompoundRequirement OR paths
/// </summary>
public class NumericRequirementDTO
{
    /// <summary>
    /// Type of requirement
    /// Values: "BondStrength", "Scale", "Resolve", "Coins", "CompletedSituations", "Achievement", "State"
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Context for the requirement (depends on Type)
    /// - For BondStrength: NPC ID
    /// - For Scale: Scale name ("Morality", "Lawfulness", etc.)
    /// - For Achievement: Achievement ID
    /// - For State: State type ("Trusted", "Celebrated", etc.)
    /// - For others: null or unused
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// Threshold value that must be met
    /// - For BondStrength: minimum bond strength (0-30)
    /// - For Scale: minimum scale value (-10 to +10)
    /// - For Resolve: minimum resolve (0-30)
    /// - For Coins: minimum coins
    /// - For CompletedSituations: count of completed situations
    /// - For Achievement: 1 = must have, 0 = must not have
    /// - For State: 1 = must have state active, 0 = must not have state
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>
    /// Display label for this requirement (for UI)
    /// Example: "Bond 15+ with Martha", "Morality +8", "Have 500 coins"
    /// </summary>
    public string Label { get; set; }
}
