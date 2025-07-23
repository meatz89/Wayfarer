using System;
using System.Threading.Tasks;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// Command to add or remove items from the player's inventory.
/// Supports undo by reversing the inventory operation.
/// </summary>
public class ModifyInventoryCommand : BaseGameCommand
{
    public enum InventoryOperation
    {
        Add,
        Remove
    }
    
    private readonly string _itemId;
    private readonly InventoryOperation _operation;
    private readonly string _reason;
    private bool _executed;
    
    public override string Description => _operation == InventoryOperation.Add 
        ? $"Add {_itemId} to inventory ({_reason})"
        : $"Remove {_itemId} from inventory ({_reason})";
    
    public ModifyInventoryCommand(string itemId, InventoryOperation operation, string reason = "transaction")
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));
            
        _itemId = itemId;
        _operation = operation;
        _reason = reason;
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        var inventory = player.Inventory;
        
        switch (_operation)
        {
            case InventoryOperation.Add:
                if (inventory.IsFull())
                {
                    return CommandValidationResult.Failure(
                        $"Inventory is full ({inventory.CurrentItemCount}/{inventory.Size} items)",
                        canBeRemedied: true,
                        remediationHint: "Drop or sell items to make space"
                    );
                }
                break;
                
            case InventoryOperation.Remove:
                if (!inventory.HasItem(_itemId))
                {
                    return CommandValidationResult.Failure(
                        $"Item '{_itemId}' not found in inventory",
                        canBeRemedied: false
                    );
                }
                break;
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
        var inventory = player.Inventory;
        
        switch (_operation)
        {
            case InventoryOperation.Add:
                inventory.AddItem(_itemId);
                break;
                
            case InventoryOperation.Remove:
                inventory.RemoveItem(_itemId);
                break;
        }
        
        _executed = true;
        
        // Add to event log
        var action = _operation == InventoryOperation.Add ? "Added" : "Removed";
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"{action} {_itemId} ({_reason})",
            Type = MessageType.Inventory,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.FromResult(CommandResult.SuccessResult(
            $"Successfully {action.ToLower()} {_itemId}",
            new 
            { 
                ItemId = _itemId,
                Operation = _operation.ToString(),
                InventoryCount = inventory.CurrentItemCount,
                InventoryCapacity = inventory.Size
            }
        ));
    }
    
    public override Task UndoAsync(GameWorld gameWorld)
    {
        if (!_executed)
            throw new InvalidOperationException("Cannot undo command that hasn't been executed");
            
        var player = gameWorld.GetPlayer();
        var inventory = player.Inventory;
        
        // Reverse the operation
        switch (_operation)
        {
            case InventoryOperation.Add:
                // Original was add, so undo by removing
                if (!inventory.HasItem(_itemId))
                    throw new InvalidOperationException($"Cannot undo: item '{_itemId}' no longer in inventory");
                inventory.RemoveItem(_itemId);
                break;
                
            case InventoryOperation.Remove:
                // Original was remove, so undo by adding
                if (inventory.IsFull())
                    throw new InvalidOperationException("Cannot undo: inventory is now full");
                inventory.AddItem(_itemId);
                break;
        }
        
        _executed = false;
        
        // Add to event log
        var action = _operation == InventoryOperation.Add ? "Removed" : "Re-added";
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = $"{action} {_itemId} (undo of {_reason})",
            Type = MessageType.Inventory,
            Timestamp = DateTime.UtcNow
        });
        
        return Task.CompletedTask;
    }
}