/// <summary>
/// Represents player's physical and mental exertion state
/// Used to dynamically modify tactical card costs based on player fatigue
/// </summary>
public class PlayerExertionState
{
public PlayerExertionLevel Physical { get; set; } = PlayerExertionLevel.Normal;
public PlayerExertionLevel Mental { get; set; } = PlayerExertionLevel.Normal;
public EnvironmentalRiskLevel CurrentRisk { get; set; } = EnvironmentalRiskLevel.Moderate;

/// <summary>
/// Calculate dynamic cost modifier for Physical cards
/// Low stamina increases Position cost
/// </summary>
public int GetPhysicalCostModifier()
{
    return Physical switch
    {
        PlayerExertionLevel.Fresh => -1,      // Reduced cost when well-rested
        PlayerExertionLevel.Normal => 0,      // No modifier
        PlayerExertionLevel.Fatigued => +1,   // Increased cost when tired
        PlayerExertionLevel.Exhausted => +2,  // Major cost increase
        PlayerExertionLevel.Desperate => +3,  // Severe cost increase
        _ => 0
    };
}

/// <summary>
/// Calculate dynamic cost modifier for Mental cards
/// High fatigue increases Attention cost
/// </summary>
public int GetMentalCostModifier()
{
    return Mental switch
    {
        PlayerExertionLevel.Fresh => -1,      // Reduced cost when sharp
        PlayerExertionLevel.Normal => 0,      // No modifier
        PlayerExertionLevel.Fatigued => +1,   // Increased cost when tired
        PlayerExertionLevel.Exhausted => +2,  // Major cost increase
        PlayerExertionLevel.Desperate => +3,  // Severe cost increase
        _ => 0
    };
}

/// <summary>
/// Calculate dynamic risk modifier for danger/exposure
/// Higher risk level increases consequences
/// </summary>
public int GetRiskModifier()
{
    return CurrentRisk switch
    {
        EnvironmentalRiskLevel.Minimal => -1,
        EnvironmentalRiskLevel.Low => 0,
        EnvironmentalRiskLevel.Moderate => +1,
        EnvironmentalRiskLevel.High => +2,
        EnvironmentalRiskLevel.Extreme => +3,
        _ => 0
    };
}
}

/// <summary>
/// Player exertion level based on resource state (NOT card property)
/// </summary>
public enum PlayerExertionLevel
{
Fresh,      // >80% resources
Normal,     // 50-80% resources
Fatigued,   // 30-50% resources
Exhausted,  // 10-30% resources
Desperate   // <10% resources
}

/// <summary>
/// Environmental risk level for tactical engagement (NOT card property)
/// Affects danger/exposure consequences based on environment
/// </summary>
public enum EnvironmentalRiskLevel
{
Minimal,    // Safe environment
Low,        // Minor threats
Moderate,   // Balanced risk
High,       // Dangerous situation
Extreme     // Life-threatening
}
