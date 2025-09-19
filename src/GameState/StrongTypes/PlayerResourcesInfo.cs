public class PlayerResourcesInfo
{
    public int Coins { get; init; }
    public int Health { get; init; }
    public int Hunger { get; init; }

    public PlayerResourcesInfo(int coins, int health, int hunger)
    {
        Coins = coins;
        Health = health;
        Hunger = hunger;
    }
}