/// <summary>
/// Parser for player initial configuration
/// Translates categorical properties (verisimilitude) to concrete mechanical values
/// </summary>
public static class PlayerInitialConfigParser
{
    /// <summary>
    /// Parse PlayerInitialConfigDTO to PlayerInitialConfig
    /// Translates categorical resource levels to concrete 6-point scale values
    /// </summary>
    public static PlayerInitialConfig Parse(PlayerInitialConfigDTO dto)
    {
        if (dto == null)
            throw new InvalidOperationException("PlayerInitialConfigDTO cannot be null");

        // Validate required fields for progression
        if (!dto.Level.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'level'");
        if (!dto.CurrentXP.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'currentXP'");
        if (!dto.XPToNextLevel.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'xpToNextLevel'");

        // Validate required fields for resources
        if (!dto.Coins.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'coins'");
        if (!dto.Hunger.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'hunger'");
        if (!dto.MaxHunger.HasValue)
            throw new InvalidOperationException("PlayerConfig missing required field 'maxHunger'");

        // Validate health - must have EITHER categorical OR direct values
        bool hasHealthDirect = dto.Health.HasValue && dto.MaxHealth.HasValue;
        bool hasHealthCategorical = !string.IsNullOrEmpty(dto.Constitution);
        if (!hasHealthDirect && !hasHealthCategorical)
            throw new InvalidOperationException("PlayerConfig missing health configuration (provide either 'constitution' OR both 'health' and 'maxHealth')");

        // Validate stamina - must have EITHER categorical OR direct values
        bool hasStaminaDirect = dto.StaminaPoints.HasValue && dto.MaxStamina.HasValue;
        bool hasStaminaCategorical = !string.IsNullOrEmpty(dto.Endurance);
        if (!hasStaminaDirect && !hasStaminaCategorical)
            throw new InvalidOperationException("PlayerConfig missing stamina configuration (provide either 'endurance' OR both 'staminaPoints' and 'maxStamina')");

        // Validate focus - must have EITHER categorical OR direct values
        bool hasFocusDirect = dto.Focus.HasValue && dto.MaxFocus.HasValue;
        bool hasFocusCategorical = !string.IsNullOrEmpty(dto.MentalCapacity);
        if (!hasFocusDirect && !hasFocusCategorical)
            throw new InvalidOperationException("PlayerConfig missing focus configuration (provide either 'mentalCapacity' OR both 'focus' and 'maxFocus')");

        PlayerInitialConfig config = new PlayerInitialConfig();

        // Direct passthrough for non-categorical values
        config.Level = dto.Level;
        config.CurrentXP = dto.CurrentXP;
        config.XPToNextLevel = dto.XPToNextLevel;
        config.Coins = dto.Coins;
        config.Hunger = dto.Hunger;
        config.MaxHunger = dto.MaxHunger;
        config.MinHealth = dto.MinHealth;
        config.SatchelCapacity = dto.SatchelCapacity;
        config.SatchelWeight = dto.SatchelWeight;
        config.Personality = dto.Personality;
        config.Archetype = dto.Archetype;
        config.InitialItems = dto.InitialItems;

        // Scene-Situation Architecture properties (direct passthrough from DTO)
        config.Resolve = dto.Resolve;
        config.Scales = dto.Scales;
        config.ActiveStates = dto.ActiveStates;
        config.EarnedAchievements = dto.EarnedAchievements;
        config.CompletedSituationIds = dto.CompletedSituationIds;

        // Use direct values if provided, otherwise translate categorical
        if (dto.Health.HasValue && dto.MaxHealth.HasValue)
        {
            config.Health = dto.Health.Value;
            config.MaxHealth = dto.MaxHealth.Value;
        }
        else if (!string.IsNullOrEmpty(dto.Constitution))
        {
            int health = TranslateResourceLevel(dto.Constitution);
            config.Health = health;
            config.MaxHealth = health;
        }

        if (dto.StaminaPoints.HasValue && dto.MaxStamina.HasValue)
        {
            config.StaminaPoints = dto.StaminaPoints.Value;
            config.MaxStamina = dto.MaxStamina.Value;
        }
        else if (!string.IsNullOrEmpty(dto.Endurance))
        {
            int stamina = TranslateResourceLevel(dto.Endurance);
            config.StaminaPoints = stamina;
            config.MaxStamina = stamina;
        }

        // Focus - new resource for Phase 3
        if (dto.Focus.HasValue && dto.MaxFocus.HasValue)
        {
            config.Focus = dto.Focus.Value;
            config.MaxFocus = dto.MaxFocus.Value;
        }
        else if (!string.IsNullOrEmpty(dto.MentalCapacity))
        {
            int focus = TranslateResourceLevel(dto.MentalCapacity);
            config.Focus = focus;
            config.MaxFocus = focus;
        }

        return config;
    }

    /// <summary>
    /// Translate categorical resource level to concrete 6-point scale value
    /// "Weak" → 4
    /// "Average" → 6 (baseline)
    /// "Strong" → 8
    /// null or unknown → 6 (default to Average)
    /// </summary>
    private static int TranslateResourceLevel(string level)
    {
        if (string.IsNullOrEmpty(level))
        {
            return 6; // Default to Average
        }

        return level.ToLower() switch
        {
            "weak" => 4,
            "average" => 6,
            "strong" => 8,
            _ => 6 // Unknown values default to Average
        };
    }
}
