public class PlayerResourcesInfo
{
    public int Coins { get; init; }
    public int Health { get; init; }
    public int Hunger { get; init; }
    public int CurrentAttention { get; init; }
    public int MaxAttention { get; init; }

    public PlayerResourcesInfo(int coins, int health, int hunger, int currentAttention, int maxAttention)
    {
        Coins = coins;
        Health = health;
        Hunger = hunger;
        CurrentAttention = currentAttention;
        MaxAttention = maxAttention;
    }
}