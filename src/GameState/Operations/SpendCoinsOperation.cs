namespace Wayfarer.GameState.Operations;

/// <summary>
/// Operation to spend coins from the player's wallet
/// </summary>
public class SpendCoinsOperation : IGameOperation
{
    private readonly int _amount;
    private int _previousCoins;
    
    public SpendCoinsOperation(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
            
        _amount = amount;
    }
    
    public string Description => $"Spend {_amount} coins";
    
    public bool CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        return player.Coins >= _amount;
    }
    
    public void Execute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        _previousCoins = player.Coins;
        player.Coins -= _amount;
    }
    
    public void Rollback(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        player.Coins = _previousCoins;
    }
}