public class TokenRequirement
{
public string NPCId { get; set; }
public ConnectionType TokenType { get; set; }
public int MinimumCount { get; set; }
}

public class TokenTypeRequirement
{
public ConnectionType TokenType { get; set; }
public int MinimumCount { get; set; }
}

