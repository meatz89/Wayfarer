/// <summary>
/// Compound Requirement - OR-based unlocking system
/// Multiple paths to unlock the same situation
/// Player needs to satisfy at least ONE complete OR path
/// </summary>
public class CompoundRequirement
{
    /// <summary>
    /// List of OR paths - player needs to satisfy at least ONE complete path
    /// Each path contains multiple AND requirements (all must be met within that path)
    /// </summary>
    public List<OrPath> OrPaths { get; set; } = new List<OrPath>();

    /// <summary>
    /// Check if any path is satisfied by current game state
    /// Returns true if at least one complete path's requirements are all met
    /// </summary>
    public bool IsAnySatisfied(Player player, GameWorld gameWorld)
    {
        if (OrPaths == null || OrPaths.Count == 0)
            return true; // No requirements means always unlocked

        foreach (OrPath path in OrPaths)
        {
            if (path.IsSatisfied(player, gameWorld))
                return true; // Found a satisfied path
        }

        return false; // No path satisfied
    }

    /// <summary>
    /// Project which paths are satisfied and which are missing.
    /// Returns detailed status for Perfect Information UI display.
    /// </summary>
    public RequirementProjection GetProjection(Player player, GameWorld gameWorld)
    {
        if (OrPaths == null || OrPaths.Count == 0)
        {
            return RequirementProjection.NoRequirements();
        }

        List<PathProjection> paths = new List<PathProjection>();
        bool anyPathSatisfied = false;

        foreach (OrPath path in OrPaths)
        {
            PathProjection pathProjection = path.GetProjection(player, gameWorld);
            paths.Add(pathProjection);
            if (pathProjection.IsSatisfied)
            {
                anyPathSatisfied = true;
            }
        }

        return new RequirementProjection
        {
            HasRequirements = true,
            IsSatisfied = anyPathSatisfied,
            Paths = paths
        };
    }
}

/// <summary>
/// Single OR path - all requirements in this path must be met (AND logic within path)
/// </summary>
public class OrPath
{
    /// <summary>
    /// Display label for this unlock path (for UI)
    /// Example: "High Bond with Martha", "Complete Investigation", "Achieve Moral Standing"
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// All numeric requirements for this path
    /// ALL must be satisfied for this path to be valid
    /// </summary>
    public List<NumericRequirement> NumericRequirements { get; set; } = new List<NumericRequirement>();

    /// <summary>
    /// Check if this path is satisfied by current game state
    /// Returns true if ALL requirements in this path are met
    /// </summary>
    public bool IsSatisfied(Player player, GameWorld gameWorld)
    {
        if (NumericRequirements == null || NumericRequirements.Count == 0)
            return true; // No requirements means path is satisfied

        foreach (NumericRequirement req in NumericRequirements)
        {
            if (!req.IsSatisfied(player, gameWorld))
                return false; // Found an unsatisfied requirement
        }

        return true; // All requirements satisfied
    }

    /// <summary>
    /// Project the satisfaction status of each requirement in this path.
    /// Returns detailed status including current values and gaps.
    /// </summary>
    public PathProjection GetProjection(Player player, GameWorld gameWorld)
    {
        List<RequirementStatus> requirements = new List<RequirementStatus>();
        bool allSatisfied = true;

        if (NumericRequirements != null)
        {
            foreach (NumericRequirement req in NumericRequirements)
            {
                bool satisfied = req.IsSatisfied(player, gameWorld);
                int currentValue = req.GetCurrentValue(player, gameWorld);

                requirements.Add(new RequirementStatus
                {
                    Requirement = req,
                    IsSatisfied = satisfied,
                    CurrentValue = currentValue,
                    RequiredValue = req.Threshold
                });

                if (!satisfied)
                {
                    allSatisfied = false;
                }
            }
        }

        return new PathProjection
        {
            Label = Label,
            IsSatisfied = allSatisfied,
            Requirements = requirements
        };
    }
}
