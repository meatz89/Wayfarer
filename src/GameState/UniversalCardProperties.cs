/// <summary>
/// Universal card properties that apply across all three tactical systems
/// These properties affect card behavior regardless of Social/Mental/Physical context
/// </summary>

/// <summary>
/// RiskLevel determines base danger probability regardless of system
/// Safe: near-zero failure consequences
/// Cautious: low risk with minor consequences
/// Risky: moderate probability of negative outcomes
/// Dangerous: high probability of significant consequences
/// </summary>
public enum RiskLevel
{
Safe,
Cautious,
Risky,
Dangerous
}

/// <summary>
/// Visibility determines how noticeable the action is regardless of system
/// Affects Exposure in Mental, Doubt in Social, enemy awareness in Physical
/// </summary>
public enum Visibility
{
Subtle,      // Minimal exposure/attention
Moderate,    // Normal visibility
Obvious,     // Clearly noticeable
Loud         // Significant exposure/attention
}

/// <summary>
/// ExertionLevel determines stamina cost regardless of system
/// Even Social and Mental cards have exertion - stress and concentration drain stamina
/// </summary>
public enum ExertionLevel
{
Minimal,     // Nearly no stamina cost
Light,       // Small stamina cost
Moderate,    // Moderate stamina cost
Heavy,       // Significant stamina cost, restricted when low stamina
Extreme      // Extreme stamina cost, only available with high stamina
}

/// <summary>
/// MethodType categorizes the approach being taken, orthogonal to system-specific categories
/// Determines which knowledge facts provide bonuses
/// </summary>
public enum MethodType
{
Direct,      // Straightforward, obvious approach
Analytical,  // Systematic, logical approach
Intuitive,   // Instinctive, feeling-based approach
Negotiated,  // Bargaining, compromise approach
Deceptive    // Indirect, misleading approach
}

/// <summary>
/// Equipment categories provided by items and required by cards
/// Equipment items provide one or more categories
/// Cards requiring equipment only appear in deck when player has matching equipment
/// </summary>
public enum EquipmentCategory
{
None,           // No equipment required
Climbing,       // Rope, grappling hook
Mechanical,     // Tools for mechanisms, locks
Documentation,  // Writing materials, references
Illumination,   // Lanterns, torches
Force,          // Crowbars, battering tools
Precision,      // Fine tools, instruments
Medical,        // Bandages, medicines
Securing        // Rope, chains, binding materials
}
