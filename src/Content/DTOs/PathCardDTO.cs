using System.Collections.Generic;

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
    public bool HasEncounter { get; set; } = false;
    public bool IsOneTime { get; set; } = false;
    
    // Requirements (visible when face-up)
    public int CoinRequirement { get; set; } = 0;
    public string PermitRequirement { get; set; }
    
    // Effects
    public int TravelTimeMinutes { get; set; }
    public int HungerEffect { get; set; } = 0;
    public string OneTimeReward { get; set; }
    public string NarrativeText { get; set; }
}

/// <summary>
/// DTO for encounter cards during travel
/// </summary>
public class EncounterCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string EncounterType { get; set; }  // "immediate" or "choice"
    public string NarrativeText { get; set; }
    
    // Immediate effect properties
    public int TimeEffect { get; set; } = 0;
    public int CoinEffect { get; set; } = 0;
    public int StaminaEffect { get; set; } = 0;
    public bool ForceReturn { get; set; } = false;
    
    // Choice-based encounter
    public List<EncounterChoice> Choices { get; set; }
}

/// <summary>
/// Represents a choice in an encounter card
/// </summary>
public class EncounterChoice
{
    public string Text { get; set; }
    public int CoinCost { get; set; } = 0;
    public int TimeCost { get; set; } = 0;
    public int StaminaCost { get; set; } = 0;
    public int HungerReduction { get; set; } = 0;
    public bool RequiresPermit { get; set; } = false;
    public bool ForceReturn { get; set; } = false;
}