using System;
using System.Threading.Tasks;


/// <summary>
/// Command to add or remove items from the player's inventory.
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

    public ModifyInventoryCommand(string itemId, InventoryOperation operation, string reason = "transaction")
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("Item ID cannot be empty", nameof(itemId));

        _itemId = itemId;
        _operation = operation;
        _reason = reason;

        Description = operation == InventoryOperation.Add
            ? $"Add {_itemId} to inventory ({_reason})"
            : $"Remove {_itemId} from inventory ({_reason})";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;

        switch (_operation)
        {
            case InventoryOperation.Add:
                if (inventory.IsFull())
                {
                    return CommandValidationResult.Failure(
                        $"Inventory is full ({inventory.UsedCapacity}/{inventory.Size} items)",
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
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.Failure(validation.FailureReason));
        }

        Player player = gameWorld.GetPlayer();
        Inventory inventory = player.Inventory;

        switch (_operation)
        {
            case InventoryOperation.Add:
                inventory.AddItem(_itemId);
                break;

            case InventoryOperation.Remove:
                inventory.RemoveItem(_itemId);
                break;
        }

        // Add to event log
        string action = _operation == InventoryOperation.Add ? "Added" : "Removed";
        gameWorld.SystemMessages.Add(new SystemMessage($"{action} {_itemId} ({_reason})"));

        return Task.FromResult(CommandResult.Success(
            $"Successfully {action.ToLower()} {_itemId}",
            new
            {
                ItemId = _itemId,
                Operation = _operation.ToString(),
                InventoryCount = inventory.UsedCapacity,
                InventoryCapacity = inventory.Size
            }
        ));
    }

}