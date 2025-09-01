
// Player's current resource state
public class ResourceState
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Food { get; set; }
    public int Attention { get; set; }
    public Dictionary<ConnectionType, int> Tokens { get; set; }
    
    public ResourceState()
    {
        Tokens = new Dictionary<ConnectionType, int>();
    }
    
    public static ResourceState FromPlayerResourceState(PlayerResourceState playerState)
    {
        return new ResourceState
        {
            Coins = playerState.Coins,
            Health = playerState.Health,
            Hunger = 10 - playerState.Stamina, // Map stamina to hunger inversely
            Food = playerState.Stamina, // Map stamina to food
            Attention = playerState.Concentration, // Map concentration to attention
            Tokens = new Dictionary<ConnectionType, int>()
        };
    }
}
