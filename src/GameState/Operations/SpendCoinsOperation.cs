
/// <summary>
/// Operation to spend coins from the player's wallet
/// </summary>
public class SpendCoinsOperation : IGameOperation
{
    private readonly int _amount;

    public SpendCoinsOperation(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        _amount = amount;
    }

    public string Description => $"Spend {_amount} coins";

    public bool CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return player.Coins >= _amount;
    }

    public void Execute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        player.Coins -= _amount;
    }
}