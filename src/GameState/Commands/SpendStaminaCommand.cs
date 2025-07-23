using System;
using System.Threading.Tasks;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// Command to spend stamina from the player.
/// Supports undo by restoring the spent stamina.
/// </summary>
public class SpendStaminaCommand : BaseGameCommand
{
    private readonly int _amount;
    private readonly string _activity;
    private int _previousStamina;
    private bool _executed;
    
    public override string Description => $"Spend {_amount} stamina on {_activity}";
    
    public SpendStaminaCommand(int amount, string activity = "activity")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
            
        _amount = amount;
        _activity = activity;
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        
        if (player.Stamina < _amount)
        {
            return CommandValidationResult.Failure(
                $"Insufficient stamina: need {_amount}, have {player.Stamina}",
                canBeRemedied: true,
                remediationHint: "Rest at an inn or consume food to restore stamina"
            );
        }
        
        // Check for dangerous activities requiring minimum stamina
        if (_activity.Contains("dangerous", StringComparison.OrdinalIgnoreCase) && player.Stamina < 4)
        {
            return CommandValidationResult.Failure(
                "Too exhausted for dangerous activities (need at least 4 stamina)",
                canBeRemedied: true,
                remediationHint: "Rest to restore stamina to at least 4"
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
        _previousStamina = player.Stamina;
        
        // Modify state
        player.ModifyStamina(-_amount);
        _executed = true;
        
        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"Spent {_amount} stamina on {_activity}",
            Type = MessageType.Resource,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.FromResult(CommandResult.SuccessResult(
            $"Successfully spent {_amount} stamina",
            new { RemainingStamina = player.Stamina }
        ));
    }
    
    public override Task UndoAsync(GameWorld gameWorld)
    {
        if (!_executed)
            throw new InvalidOperationException("Cannot undo command that hasn't been executed");
            
        var player = gameWorld.GetPlayer();
        
        // Restore previous state
        player.SetNewStamina(_previousStamina);
        _executed = false;
        
        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"Restored {_amount} stamina from {_activity}",
            Type = MessageType.Resource,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.CompletedTask;
    }
}