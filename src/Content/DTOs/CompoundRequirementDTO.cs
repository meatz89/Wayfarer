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
/// Uses Explicit Property Principle: each requirement type has its own named property
/// instead of generic string-based Type/Context routing.
/// See arc42/08_crosscutting_concepts.md §8.19
/// </summary>
public class OrPathDTO
{
    /// <summary>
    /// Display label for this unlock path (for UI)
    /// Example: "High Bond with Martha", "Complete Investigation", "Achieve Moral Standing"
    /// </summary>
    public string Label { get; set; }

    // ============================================
    // STAT REQUIREMENTS - explicit property per stat
    // ============================================
    public int? InsightRequired { get; set; }
    public int? RapportRequired { get; set; }
    public int? AuthorityRequired { get; set; }
    public int? DiplomacyRequired { get; set; }
    public int? CunningRequired { get; set; }

    // ============================================
    // RESOURCE REQUIREMENTS
    // ============================================
    public int? ResolveRequired { get; set; }
    public int? CoinsRequired { get; set; }

    // ============================================
    // PROGRESSION REQUIREMENTS
    // ============================================
    public int? SituationCountRequired { get; set; }

    // ============================================
    // RELATIONSHIP REQUIREMENTS (NPC name for lookup)
    // ============================================
    public string BondNpcName { get; set; }
    public int? BondStrengthRequired { get; set; }

    // ============================================
    // SCALE REQUIREMENTS (ScaleType string for parsing)
    // ============================================
    public string ScaleTypeName { get; set; }
    public int? ScaleValueRequired { get; set; }

    // ============================================
    // BOOLEAN REQUIREMENTS (names for lookup)
    // ============================================
    public string RequiredAchievementName { get; set; }
    public string RequiredStateName { get; set; }
    public string RequiredItemName { get; set; }
}
