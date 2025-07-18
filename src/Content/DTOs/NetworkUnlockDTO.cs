using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing network unlock data from JSON.
/// Maps to the structure in progression_unlocks.json.
/// </summary>
public class NetworkUnlockDTO
{
    public string Id { get; set; }
    public string Type { get; set; } = "npc_network";
    public string UnlockerNpcId { get; set; }
    public int TokensRequired { get; set; }
    public string UnlockDescription { get; set; }
    public List<NetworkUnlockTargetDTO> Unlocks { get; set; } = new List<NetworkUnlockTargetDTO>();
}

/// <summary>
/// An NPC that can be unlocked through network progression
/// </summary>
public class NetworkUnlockTargetDTO
{
    public string NpcId { get; set; }
    public string IntroductionText { get; set; }
}