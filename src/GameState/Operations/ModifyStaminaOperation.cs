namespace Wayfarer.GameState.Operations;

/// <summary>
/// Operation to modify player stamina
/// </summary>
public class ModifyStaminaOperation : IGameOperation
{
    private readonly int _amount;
    private int _previousStamina;
    
    public ModifyStaminaOperation(int amount)
    {
        _amount = amount;
    }
    
    public string Description => _amount >= 0 
        ? $"Gain {_amount} stamina"
        : $"Lose {Math.Abs(_amount)} stamina";
    
    public bool CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        var newStamina = player.Stamina + _amount;
        
        // Can't go below 0 stamina
        return newStamina >= 0;
    }
    
    public void Execute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        _previousStamina = player.Stamina;
        player.Stamina += _amount;
        
        // Clamp to valid range
        player.Stamina = Math.Max(0, Math.Min(player.MaxStamina, player.Stamina));
    }
    
    public void Rollback(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        player.Stamina = _previousStamina;
    }
}