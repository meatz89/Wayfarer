/// <summary>
/// Strongly-typed representation of a token change operation.
/// Replaces tuple usage in GameRuleEngine.TokenChanges.
/// </summary>
public record TokenChange
{
    /// <summary>
    /// The NPC ID receiving the token change
    /// </summary>
    public string NpcId { get; init; }

    /// <summary>
    /// The type of token being changed
    /// </summary>
    public ConnectionType TokenType { get; init; }

    /// <summary>
    /// The amount of change (positive for gains, negative for losses)
    /// </summary>
    public int Amount { get; init; }

    /// <summary>
    /// Create a token change
    /// </summary>
    public TokenChange(string npcId, ConnectionType tokenType, int amount)
    {
        NpcId = npcId;
        TokenType = tokenType;
        Amount = amount;
    }
}
