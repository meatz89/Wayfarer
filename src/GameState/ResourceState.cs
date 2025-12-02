/// <summary>
/// Player's current resource state for UI display.
/// DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum values.
/// </summary>
public class ResourceState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Stamina { get; set; }

    // EXPLICIT TOKEN PROPERTIES - ConnectionType is fixed enum, use direct properties
    public int TrustTokens { get; set; }
    public int DiplomacyTokens { get; set; }
    public int StatusTokens { get; set; }
    public int ShadowTokens { get; set; }

    // Helper for enum-based access when needed
    public int GetTokens(ConnectionType type) => type switch
    {
        ConnectionType.Trust => TrustTokens,
        ConnectionType.Diplomacy => DiplomacyTokens,
        ConnectionType.Status => StatusTokens,
        ConnectionType.Shadow => ShadowTokens,
        _ => 0
    };

    public static ResourceState FromPlayer(Player player)
    {
        return new ResourceState
        {
            Coins = player.Coins,
            Health = player.Health,
            Hunger = player.Hunger,
            Stamina = player.Stamina
        };
    }

    public static ResourceState FromPlayerResourceState(PlayerResourceState playerState)
    {
        return new ResourceState
        {
            Coins = playerState.Coins,
            Health = playerState.Health,
            Hunger = playerState.Hunger,
            Stamina = playerState.Stamina
        };
    }
}
