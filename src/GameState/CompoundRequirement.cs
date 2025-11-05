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
    /// Self-contained pattern: markerMap resolves "generated:{templateId}" to actual IDs in requirements
    /// </summary>
    public bool IsAnySatisfied(Player player, GameWorld gameWorld, Dictionary<string, string> markerMap = null)
    {
        if (OrPaths == null || OrPaths.Count == 0)
            return true; // No requirements means always unlocked

        foreach (OrPath path in OrPaths)
        {
            if (path.IsSatisfied(player, gameWorld, markerMap))
                return true; // Found a satisfied path
        }

        return false; // No path satisfied
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
    /// Self-contained pattern: markerMap passed to requirement evaluation for marker resolution
    /// </summary>
    public bool IsSatisfied(Player player, GameWorld gameWorld, Dictionary<string, string> markerMap = null)
    {
        if (NumericRequirements == null || NumericRequirements.Count == 0)
            return true; // No requirements means path is satisfied

        foreach (NumericRequirement req in NumericRequirements)
        {
            if (!req.IsSatisfied(player, gameWorld, markerMap))
                return false; // Found an unsatisfied requirement
        }

        return true; // All requirements satisfied
    }
}
