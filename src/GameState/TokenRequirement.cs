/// <summary>
/// HIGHLANDER: Store NPC object reference, not string ID
/// </summary>
public class TokenRequirement
{
    public NPC Npc { get; set; }
    public ConnectionType TokenType { get; set; }
    public int MinimumCount { get; set; }
}

public class TokenTypeRequirement
{
    public ConnectionType TokenType { get; set; }
    public int MinimumCount { get; set; }
}

