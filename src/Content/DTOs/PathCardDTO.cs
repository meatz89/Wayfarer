/// <summary>
/// DTO for path card data from JSON packages
/// </summary>
public class PathCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int StaminaCost { get; set; }
    public bool StartsRevealed { get; set; } = false;  // Whether card is face-up at game start
    public bool IsHidden { get; set; } = false;
    public int ExplorationThreshold { get; set; } = 0;  // Route exploration cubes required to reveal (0=always visible, 10=extremely hidden) - replaces RoutePath.HiddenUntilExploration
    public bool IsOneTime { get; set; } = false;

    // Requirements (visible when face-up)
    public int CoinRequirement { get; set; } = 0;
    public string PermitRequirement { get; set; }

    // Effects
    public int TravelTimeSegments { get; set; }
    public int HungerEffect { get; set; } = 0;
    public string OneTimeReward { get; set; }
    public string NarrativeText { get; set; }

    // Additional Mechanical Effects
    public int StaminaRestore { get; set; } = 0; // For REST actions
    public int HealthEffect { get; set; } = 0; // Positive for healing, negative for damage
    public int CoinReward { get; set; } = 0; // Coins gained from this path
    public bool ForceReturn { get; set; } = false; // Dead-end paths that force return

    // Token Gains
    public Dictionary<string, int> TokenGains { get; set; } // e.g., {"Diplomacy": 1, "Status": 2}

    // Path Revelations
    public List<string> RevealsPaths { get; set; } // List of path IDs to reveal when played

    // Stat Requirements - minimum stat levels required to use this path
    public Dictionary<string, int> StatRequirements { get; set; } // e.g., {"insight": 2, "cunning": 3}

    // Core Loop: Optional scene on this path (references GameWorld.Scenes)
    // Player can preview scene and see equipment applicability before committing
    public string SceneId { get; set; }
}