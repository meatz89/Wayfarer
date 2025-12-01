/// <summary>
/// Player's current resource state for UI display.
/// DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary.
/// </summary>
public class ResourceState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Stamina { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<ConnectionTypeTokenEntry> Tokens { get; set; }

    public ResourceState()
    {
        Tokens = new List<ConnectionTypeTokenEntry>();
    }

    public static ResourceState FromPlayer(Player player)
    {
        return new ResourceState
        {
            Coins = player.Coins,
            Health = player.Health,
            Hunger = player.Hunger,
            Stamina = player.Stamina,
            Tokens = new List<ConnectionTypeTokenEntry>()
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
            Tokens = new List<ConnectionTypeTokenEntry>()
        };
    }
}
