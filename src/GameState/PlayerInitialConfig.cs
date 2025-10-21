
// Player initial configuration data
public class PlayerInitialConfig
{
    // Progression
    public int? Level { get; set; }
    public int? CurrentXP { get; set; }
    public int? XPToNextLevel { get; set; }

    // Resources
    public int? Coins { get; set; }
    public int? StaminaPoints { get; set; }
    public int? MaxStamina { get; set; }
    public int? Health { get; set; }
    public int? MaxHealth { get; set; }
    public int? MinHealth { get; set; }
    public int? Hunger { get; set; }
    public int? MaxHunger { get; set; }
    public int? Focus { get; set; }
    public int? MaxFocus { get; set; }

    // Equipment
    public int? SatchelCapacity { get; set; }
    public int? SatchelWeight { get; set; }

    // Identity
    public string Personality { get; set; }
    public string Archetype { get; set; }

    // Starting items
    public List<ResourceEntry> InitialItems { get; set; }
}
