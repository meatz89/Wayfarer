
// Player initial configuration data
public class PlayerInitialConfig
{
    public int? Coins { get; set; }
    public int? StaminaPoints { get; set; }
    public int? MaxStamina { get; set; }
    public int? Health { get; set; }
    public int? MaxHealth { get; set; }
    public int? Hunger { get; set; }
    public int? MaxHunger { get; set; }
    public int? SatchelCapacity { get; set; }
    public int? SatchelWeight { get; set; }
    public string Personality { get; set; }
    public string Archetype { get; set; }
    public List<ResourceEntry> InitialItems { get; set; }
}
