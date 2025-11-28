/// <summary>
/// Projection of requirement satisfaction status for Perfect Information display.
/// Shows which OR-paths are satisfied and which requirements are missing.
/// </summary>
public class RequirementProjection
{
    /// <summary>Whether any requirements exist at all</summary>
    public bool HasRequirements { get; init; }

    /// <summary>Whether at least one OR-path is fully satisfied</summary>
    public bool IsSatisfied { get; init; }

    /// <summary>Detailed status for each OR-path</summary>
    public List<PathProjection> Paths { get; init; } = new List<PathProjection>();

    /// <summary>Create a projection indicating no requirements (always satisfied)</summary>
    public static RequirementProjection NoRequirements() => new RequirementProjection
    {
        HasRequirements = false,
        IsSatisfied = true,
        Paths = new List<PathProjection>()
    };
}

/// <summary>
/// Projection of a single OR-path's satisfaction status.
/// All requirements within a path use AND logic (all must be met).
/// </summary>
public class PathProjection
{
    /// <summary>Display label for this unlock path</summary>
    public string Label { get; init; }

    /// <summary>Whether all requirements in this path are satisfied</summary>
    public bool IsSatisfied { get; init; }

    /// <summary>Status of each individual requirement in this path</summary>
    public List<RequirementStatus> Requirements { get; init; } = new List<RequirementStatus>();

    /// <summary>Only the unsatisfied requirements (for UI display of gaps)</summary>
    public List<RequirementStatus> MissingRequirements => Requirements.Where(r => !r.IsSatisfied).ToList();
}

/// <summary>
/// Status of a single requirement check.
/// Includes current value, required value, and gap for UI display.
/// Uses explicit Label instead of NumericRequirement reference.
/// </summary>
public class RequirementStatus
{
    /// <summary>Display label for this requirement (e.g., "Insight 3+", "Resolve 0+")</summary>
    public string Label { get; init; }

    /// <summary>Whether this requirement is satisfied</summary>
    public bool IsSatisfied { get; init; }

    /// <summary>Player's current value for this requirement type</summary>
    public int CurrentValue { get; init; }

    /// <summary>Value needed to satisfy this requirement</summary>
    public int RequiredValue { get; init; }

    /// <summary>Gap between current and required (positive = need more)</summary>
    public int Gap => RequiredValue - CurrentValue;
}
