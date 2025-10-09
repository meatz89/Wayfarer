using System.Collections.Generic;

/// <summary>
/// Starting conditions for a game - reuses existing PlayerInitialConfig
/// </summary>
public class PackageStartingConditions
{
    /// <summary>
    /// Player initial configuration - reuses existing PlayerInitialConfig from GameWorld.cs
    /// </summary>
    public PlayerInitialConfig PlayerConfig { get; set; }

    /// <summary>
    /// Starting Venue spot ID
    /// </summary>
    public string StartingSpotId { get; set; }

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