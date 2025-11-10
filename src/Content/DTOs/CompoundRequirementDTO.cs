/// <summary>
/// DTO for Compound Requirements - OR-based unlocking system
/// Multiple paths to unlock the same situation
/// At least ONE OrPath must be satisfied to unlock
/// </summary>
public class CompoundRequirementDTO
{
/// <summary>
/// List of OR paths - player needs to satisfy at least ONE complete path
/// Each path contains multiple AND requirements (all must be met within that path)
/// </summary>
public List<OrPathDTO> OrPaths { get; set; } = new List<OrPathDTO>();
}

/// <summary>
/// Single OR path - all requirements in this path must be met (AND logic within path)
/// </summary>
public class OrPathDTO
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
public List<NumericRequirementDTO> NumericRequirements { get; set; } = new List<NumericRequirementDTO>();
}
