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

        float staminaPercent = (float)player.Stamina / maxStamina;

        return staminaPercent switch
        {
            > 0.8f => PlayerExertionLevel.Fresh,
            > 0.5f => PlayerExertionLevel.Normal,
            > 0.3f => PlayerExertionLevel.Fatigued,
            > 0.1f => PlayerExertionLevel.Exhausted,
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

        float healthPercent = (float)player.Health / maxHealth;

        return healthPercent switch
        {
            > 0.8f => PlayerExertionLevel.Fresh,
            > 0.5f => PlayerExertionLevel.Normal,
            > 0.3f => PlayerExertionLevel.Fatigued,
            > 0.1f => PlayerExertionLevel.Exhausted,
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
        float avgPercent = ((float)player.Stamina / maxStamina + (float)player.Health / maxHealth) / 2;

        return avgPercent switch
        {
            > 0.8f => EnvironmentalRiskLevel.Minimal,
            > 0.6f => EnvironmentalRiskLevel.Low,
            > 0.4f => EnvironmentalRiskLevel.Moderate,
            > 0.2f => EnvironmentalRiskLevel.High,
            _ => EnvironmentalRiskLevel.Extreme
        };
    }
}
