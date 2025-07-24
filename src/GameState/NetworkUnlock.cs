using System.Collections.Generic;

/// <summary>
/// Represents a network unlock progression rule
/// </summary>
public class NetworkUnlock
{
    public string Id { get; set; }
    public string Type { get; set; } = "npc_network";
    public string UnlockerNpcId { get; set; }
    public int TokensRequired { get; set; }
    public string UnlockDescription { get; set; }
    public List<NetworkUnlockTarget> Unlocks { get; set; } = new List<NetworkUnlockTarget>();
}

/// <summary>
/// Represents an NPC that can be unlocked through network progression
/// </summary>
public class NetworkUnlockTarget
{
    public string NpcId { get; set; }
    public string IntroductionText { get; set; }
}
