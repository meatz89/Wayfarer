using System.Threading.Tasks;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// Command to spend coins from the player's purse.
/// Supports undo by restoring the spent coins.
/// </summary>
public class SpendCoinsCommand : BaseGameCommand
{
    private readonly int _amount;
    private readonly string _reason;
    private int _previousCoins;
    private bool _executed;
    
    public override string Description => $"Spend {_amount} coins for {_reason}";
    
    public SpendCoinsCommand(int amount, string reason = "purchase")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
            
        _amount = amount;
        _reason = reason;
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        
        if (player.Coins < _amount)
        {
            return CommandValidationResult.Failure(
                $"Insufficient coins: need {_amount}, have {player.Coins}",
                canBeRemedied: true,
                remediationHint: $"Earn {_amount - player.Coins} more coins"
            );
        }
        
        return CommandValidationResult.Success();
    }
    
    public override Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        var validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.FailureResult(validation.FailureReason));
        }
        
        var player = gameWorld.GetPlayer();
        
        // Store state for undo
        _previousCoins = player.Coins;
        
        // Modify state
        player.ModifyCoins(-_amount);
        _executed = true;
        
        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"Spent {_amount} coins on {_reason}",
            Type = MessageType.Resource,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.FromResult(CommandResult.SuccessResult(
            $"Successfully spent {_amount} coins",
            new { RemainingCoins = player.Coins }
        ));
    }
    
    public override Task UndoAsync(GameWorld gameWorld)
    {
        if (!_executed)
            throw new InvalidOperationException("Cannot undo command that hasn't been executed");
            
        var player = gameWorld.GetPlayer();
        
        // Restore previous state
        player.Coins = _previousCoins;
        _executed = false;
        
        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"Refunded {_amount} coins from {_reason}",
            Type = MessageType.Resource,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.CompletedTask;
    }
}