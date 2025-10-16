using System.Collections.Generic;

/// <summary>
/// DTO for player initial configuration from JSON
/// Uses categorical properties (verisimilitude) that parser translates to concrete values
/// </summary>
public class PlayerInitialConfigDTO
{
    /// <summary>
    /// Starting coins
    /// </summary>
    public int? Coins { get; set; }

    /// <summary>
    /// Constitution level - translates to Health/MaxHealth
    /// Values: "Weak", "Average", "Strong"
    /// </summary>
    public string Constitution { get; set; }

    /// <summary>
    /// Endurance level - translates to Stamina/MaxStamina
    /// Values: "Weak", "Average", "Strong"
    /// </summary>
    public string Endurance { get; set; }

    /// <summary>
    /// Mental capacity level - translates to Focus/MaxFocus
    /// Values: "Weak", "Average", "Strong"
    /// </summary>
    public string MentalCapacity { get; set; }

    /// <summary>
    /// Starting hunger level
    /// </summary>
    public int? Hunger { get; set; }

    /// <summary>
    /// Maximum hunger capacity
    /// </summary>
    public int? MaxHunger { get; set; }

    /// <summary>
    /// Satchel weight capacity
    /// </summary>
    public int? SatchelCapacity { get; set; }

    /// <summary>
    /// Initial satchel weight
    /// </summary>
    public int? SatchelWeight { get; set; }

    /// <summary>
    /// Personality type
    /// </summary>
    public string Personality { get; set; }

    /// <summary>
    /// Character archetype
    /// </summary>
    public string Archetype { get; set; }

    /// <summary>
    /// Initial items in inventory
    /// </summary>
    public List<ResourceEntry> InitialItems { get; set; }
}
