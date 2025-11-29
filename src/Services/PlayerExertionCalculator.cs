/// <summary>
/// Calculates player exertion state from current Health/Stamina
/// Used by tactical systems to apply dynamic cost modifiers
/// </summary>
public class PlayerExertionCalculator
{
    /// <summary>
    /// Calculate exertion state from player's current resources
    /// </summary>
    public PlayerExertionState CalculateExertion(Player player)
    {
        return new PlayerExertionState
        {
            Physical = CalculatePhysicalExertion(player),
            Mental = CalculateMentalExertion(player),
            CurrentRisk = CalculateRiskLevel(player)
        };
    }

    /// <summary>
    /// Physical exertion based on Stamina percentage
    /// </summary>
    private PlayerExertionLevel CalculatePhysicalExertion(Player player)
    {
        int maxStamina = player.MaxStamina;
        if (maxStamina == 0) return PlayerExertionLevel.Normal;

        int staminaPercent = player.Stamina * 100 / maxStamina;

        return staminaPercent switch
        {
            > 80 => PlayerExertionLevel.Fresh,
            > 50 => PlayerExertionLevel.Normal,
            > 30 => PlayerExertionLevel.Fatigued,
            > 10 => PlayerExertionLevel.Exhausted,
            _ => PlayerExertionLevel.Desperate
        };
    }

    /// <summary>
    /// Mental exertion based on Health percentage
    /// (Using Health as proxy for mental fatigue until we have dedicated mental resource)
    /// </summary>
    private PlayerExertionLevel CalculateMentalExertion(Player player)
    {
        int maxHealth = player.MaxHealth;
        if (maxHealth == 0) return PlayerExertionLevel.Normal;

        int healthPercent = player.Health * 100 / maxHealth;

        return healthPercent switch
        {
            > 80 => PlayerExertionLevel.Fresh,
            > 50 => PlayerExertionLevel.Normal,
            > 30 => PlayerExertionLevel.Fatigued,
            > 10 => PlayerExertionLevel.Exhausted,
            _ => PlayerExertionLevel.Desperate
        };
    }

    /// <summary>
    /// Risk level based on combined Health + Stamina state
    /// Lower resources = higher risk
    /// </summary>
    private EnvironmentalRiskLevel CalculateRiskLevel(Player player)
    {
        int maxStamina = player.MaxStamina;
        int maxHealth = player.MaxHealth;

        if (maxStamina == 0 || maxHealth == 0) return EnvironmentalRiskLevel.Moderate;

        // Average percentage of both resources
        int avgPercent = (player.Stamina * 100 / maxStamina + player.Health * 100 / maxHealth) / 2;

        return avgPercent switch
        {
            > 80 => EnvironmentalRiskLevel.Minimal,
            > 60 => EnvironmentalRiskLevel.Low,
            > 40 => EnvironmentalRiskLevel.Moderate,
            > 20 => EnvironmentalRiskLevel.High,
            _ => EnvironmentalRiskLevel.Extreme
        };
    }
}
