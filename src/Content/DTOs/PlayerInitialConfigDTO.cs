/// <summary>
/// DTO for player initial configuration from JSON
/// Uses categorical properties (verisimilitude) that parser translates to concrete values
/// </summary>
public class PlayerInitialConfigDTO
{
/// <summary>
/// Progression fields
/// </summary>
public int? Level { get; set; }
public int? CurrentXP { get; set; }
public int? XPToNextLevel { get; set; }

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

/// <summary>
/// Direct resource values (override categorical if provided)
/// Used for tutorial where player starts damaged (4/6 Health)
/// </summary>
public int? Health { get; set; }
public int? MaxHealth { get; set; }
public int? MinHealth { get; set; }
public int? StaminaPoints { get; set; }
public int? MaxStamina { get; set; }
public int? Focus { get; set; }
public int? MaxFocus { get; set; }

/// <summary>
/// Scene-Situation Architecture additions (Sir Brante inspired)
/// </summary>

/// <summary>
/// Resolve - universal consumable resource (0-30, similar to Willpower)
/// Used to unlock situations and make difficult choices
/// </summary>
public int? Resolve { get; set; }

/// <summary>
/// Player Scales - 6 moral/behavioral axes (-10 to +10 each)
/// Strongly-typed nested object (NOT list or dictionary)
/// </summary>
public PlayerScalesDTO Scales { get; set; }

/// <summary>
/// Active States - temporary conditions currently affecting player
/// List of active state instances with segment-based duration tracking
/// </summary>
public List<ActiveStateDTO> ActiveStates { get; set; } = new List<ActiveStateDTO>();

/// <summary>
/// Earned Achievements - milestone markers player has achieved
/// List of achievement instances with segment-based earned time
/// </summary>
public List<PlayerAchievementDTO> EarnedAchievements { get; set; } = new List<PlayerAchievementDTO>();

/// <summary>
/// Completed Situation IDs - tracking which situations player has finished
/// Used for spawn rules and requirement checking
/// </summary>
public List<string> CompletedSituationIds { get; set; } = new List<string>();
}
