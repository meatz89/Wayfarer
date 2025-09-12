
// Player's current resource state for UI display
public class ResourceState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Stamina { get; set; }
    public int Attention { get; set; }
    public Dictionary<ConnectionType, int> Tokens { get; set; }

    public ResourceState()
    {
        Tokens = new Dictionary<ConnectionType, int>();
    }

    public static ResourceState FromPlayer(Player player)
    {
        return new ResourceState
        {
            Coins = player.Coins,
            Health = player.Health,
            Hunger = player.Hunger,
            Stamina = player.Stamina,
            Attention = player.Attention,
            Tokens = new Dictionary<ConnectionType, int>()
        };
    }
    
    public static ResourceState FromPlayerResourceState(PlayerResourceState playerState)
    {
        return new ResourceState
        {
            Coins = playerState.Coins,
            Health = playerState.Health,
            Hunger = playerState.Hunger,
            Stamina = playerState.Stamina,
            Attention = playerState.Attention,
            Tokens = new Dictionary<ConnectionType, int>()
        };
    }
}
