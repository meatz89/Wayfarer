using System;

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
        if (dto == null) return null;

        PlayerInitialConfig config = new PlayerInitialConfig();

        // Direct passthrough for non-categorical values
        config.Coins = dto.Coins;
        config.Hunger = dto.Hunger;
        config.MaxHunger = dto.MaxHunger;
        config.SatchelCapacity = dto.SatchelCapacity;
        config.SatchelWeight = dto.SatchelWeight;
        config.Personality = dto.Personality;
        config.Archetype = dto.Archetype;
        config.InitialItems = dto.InitialItems;

        // Translate categorical properties to concrete values
        int health = TranslateResourceLevel(dto.Constitution);
        config.Health = health;
        config.MaxHealth = health;

        int stamina = TranslateResourceLevel(dto.Endurance);
        config.StaminaPoints = stamina;
        config.MaxStamina = stamina;

        // Focus is a new resource - parse mentalCapacity
        // Note: Player entity doesn't have Focus yet, but will in Phase 3
        // For now, we can store it but Player.ApplyInitialConfiguration won't use it
        // This is forward-compatible with upcoming Focus implementation

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
