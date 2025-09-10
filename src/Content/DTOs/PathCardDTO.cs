using System.Collections.Generic;

/// <summary>
/// DTO for path card data from JSON packages
/// </summary>
public class PathCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int StaminaCost { get; set; }
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
    public string Effect { get; set; }
    public string NarrativeText { get; set; }
}