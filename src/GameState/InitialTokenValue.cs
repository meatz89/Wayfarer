/// <summary>
/// Initial token value for NPC (strongly-typed, no Dictionary)
/// HIGHLANDER: Enum type, not string ID
/// </summary>
public class InitialTokenValue
{
    public ConnectionType TokenType { get; set; }
    public int Value { get; set; }
}
