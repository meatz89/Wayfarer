/// <summary>
/// Starting conditions for a game - uses DTO for JSON deserialization
/// </summary>
public class PackageStartingConditions
{
/// <summary>
/// Player initial configuration DTO - contains categorical properties from JSON
/// Parser translates to concrete PlayerInitialConfig domain entity
/// </summary>
public PlayerInitialConfigDTO PlayerConfig { get; set; }

/// <summary>
/// Starting Venue location ID
/// </summary>
public string StartingSpotId { get; set; }

/// <summary>
/// Starting day (default: 1)
/// </summary>
public int? StartingDay { get; set; }

/// <summary>
/// Starting time block (Morning/Midday/Afternoon/Evening)
/// </summary>
public string StartingTimeBlock { get; set; }

/// <summary>
/// Starting segment WITHIN the time block (1-4, relative to block start)
/// CRITICAL: This is NOT the absolute segment of day - it's position within the time block.
/// Example: Evening segment 1 = first segment of Evening (13th segment of full day)
/// Example: Midday segment 3 = third segment of Midday (7th segment of full day)
/// </summary>
public int? StartingSegment { get; set; }

/// <summary>
/// Initial obligations in queue
/// </summary>
public List<StandingObligationDTO> StartingObligations { get; set; }

/// <summary>
/// Initial token relationships with NPCs
/// Dictionary key is NPC ID, value is token counts
/// </summary>
public Dictionary<string, NPCTokenRelationship> StartingTokens { get; set; }
}

/// <summary>
/// Token relationship with an NPC
/// </summary>
public class NPCTokenRelationship
{
public int Trust { get; set; }
public int Diplomacy { get; set; }
public int Status { get; set; }
public int Shadow { get; set; }
}