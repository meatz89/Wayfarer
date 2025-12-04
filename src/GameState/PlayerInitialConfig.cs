/// <summary>
/// Entry for resource tracking with explicit type and amount.
/// INLINED from CollectionEntries.cs per HIGHLANDER principle (keep class with its primary consumer)
/// </summary>
public class ResourceEntry
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
}

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

    // Scene-Situation Architecture (Sir Brante integration)
    public int? Resolve { get; set; }
    public PlayerScalesDTO Scales { get; set; }
    public List<ActiveStateDTO> ActiveStates { get; set; }
    public List<PlayerAchievementDTO> EarnedAchievements { get; set; }
    public List<string> CompletedSituationIds { get; set; }
}
