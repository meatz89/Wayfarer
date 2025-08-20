using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState.Constants;
using Wayfarer.Content;

namespace Wayfarer.GameState.Operations
{
    /// <summary>
/// Handles all inventory state operations in an immutable, validated manner.
/// All inventory changes must go through this class.
/// </summary>
public static class InventoryOperations
{
    /// <summary>
    /// Attempts to add an item to the inventory.
    /// </summary>
    public static InventoryOperationResult AddItem(InventoryState inventory, string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryOperationResult.Failure("Item ID cannot be empty");

        if (!inventory.HasFreeSlot())
            return InventoryOperationResult.Failure($"Inventory is full ({inventory.UsedCapacity}/{inventory.Capacity} items)");

        InventoryState newInventory = inventory.WithAddedItem(itemId);
        if (newInventory == null)
            return InventoryOperationResult.Failure("Failed to add item to inventory");

        return InventoryOperationResult.Success(newInventory, $"Added {itemId} to inventory");
    }

    /// <summary>
    /// Attempts to remove an item from the inventory.
    /// </summary>
    public static InventoryOperationResult RemoveItem(InventoryState inventory, string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryOperationResult.Failure("Item ID cannot be empty");

        if (!inventory.HasItem(itemId))
            return InventoryOperationResult.Failure($"Item '{itemId}' not found in inventory");

        InventoryState newInventory = inventory.WithRemovedItem(itemId);
        if (newInventory == null)
            return InventoryOperationResult.Failure("Failed to remove item from inventory");

        return InventoryOperationResult.Success(newInventory, $"Removed {itemId} from inventory");
    }

    /// <summary>
    /// Attempts to add multiple items to the inventory.
    /// </summary>
    public static InventoryOperationResult AddItems(InventoryState inventory, string itemId, int count)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryOperationResult.Failure("Item ID cannot be empty");

        if (count <= 0)
            return InventoryOperationResult.Failure("Count must be positive");

        (InventoryState newInventory, int addedCount) = inventory.WithAddedItems(itemId, count);

        if (addedCount == 0)
            return InventoryOperationResult.Failure($"Inventory is full ({inventory.UsedCapacity}/{inventory.Capacity} items)");

        if (addedCount < count)
            return InventoryOperationResult.PartialSuccess(newInventory,
                $"Added {addedCount} of {count} {itemId} items (inventory full)", addedCount);

        return InventoryOperationResult.Success(newInventory, $"Added {count} {itemId} items to inventory");
    }

    /// <summary>
    /// Attempts to remove multiple items from the inventory.
    /// </summary>
    public static InventoryOperationResult RemoveItems(InventoryState inventory, string itemId, int count)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryOperationResult.Failure("Item ID cannot be empty");

        if (count <= 0)
            return InventoryOperationResult.Failure("Count must be positive");

        int currentCount = inventory.GetItemCount(itemId);
        if (currentCount == 0)
            return InventoryOperationResult.Failure($"Item '{itemId}' not found in inventory");

        (InventoryState newInventory, int removedCount) = inventory.WithRemovedItems(itemId, count);

        if (removedCount < count)
            return InventoryOperationResult.PartialSuccess(newInventory,
                $"Removed {removedCount} of {count} {itemId} items (only had {currentCount})", removedCount);

        return InventoryOperationResult.Success(newInventory, $"Removed {count} {itemId} items from inventory");
    }

    /// <summary>
    /// Sets the exact count of an item in the inventory.
    /// </summary>
    public static InventoryOperationResult SetItemCount(InventoryState inventory, string itemId, int targetCount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return InventoryOperationResult.Failure("Item ID cannot be empty");

        if (targetCount < 0)
            return InventoryOperationResult.Failure("Target count cannot be negative");

        int currentCount = inventory.GetItemCount(itemId);

        if (currentCount == targetCount)
            return InventoryOperationResult.Success(inventory, "Item count already at target");

        if (targetCount > currentCount)
        {
            // Need to add items
            return AddItems(inventory, itemId, targetCount - currentCount);
        }
        else
        {
            // Need to remove items
            return RemoveItems(inventory, itemId, currentCount - targetCount);
        }
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public static InventoryOperationResult ClearInventory(InventoryState inventory)
    {
        InventoryState newInventory = inventory.WithClearedSlots();
        return InventoryOperationResult.Success(newInventory, "Cleared all items from inventory");
    }

    /// <summary>
    /// Validates if an item can be added considering size and transport.
    /// </summary>
    public static bool CanAddItemWithSize(InventoryState inventory, Item item, ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        return inventory.CanAddItem(item, itemRepository, currentTransport);
    }

    /// <summary>
    /// Attempts to add an item considering its size requirements.
    /// </summary>
    public static InventoryOperationResult AddItemWithSizeCheck(InventoryState inventory, Item item, ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        if (item == null)
            return InventoryOperationResult.Failure("Item cannot be null");

        if (!inventory.CanAddItem(item, itemRepository, currentTransport))
        {
            int usedSlots = inventory.GetUsedSlots(itemRepository);
            int maxSlots = inventory.GetMaxSlots(itemRepository, currentTransport);
            return InventoryOperationResult.Failure(
                $"Not enough space for {item.Name} (requires {item.GetRequiredSlots()} slots, " +
                $"have {maxSlots - usedSlots} of {maxSlots} available)");
        }

        return AddItem(inventory, item.Id);
    }
}

/// <summary>
/// Result of an inventory operation.
/// </summary>
public class InventoryOperationResult
{
    public bool IsSuccess { get; }
    public bool IsPartialSuccess { get; }
    public string Message { get; }
    public InventoryState NewInventory { get; }
    public int? AffectedCount { get; }

    private InventoryOperationResult(bool success, bool partialSuccess, string message, InventoryState newInventory, int? affectedCount = null)
    {
        IsSuccess = success;
        IsPartialSuccess = partialSuccess;
        Message = message;
        NewInventory = newInventory;
        AffectedCount = affectedCount;
    }

    public static InventoryOperationResult Failure(string message)
    {
        return new InventoryOperationResult(false, false, message, null);
    }

    public static InventoryOperationResult Success(InventoryState newInventory, string message)
    {
        return new InventoryOperationResult(true, false, message, newInventory);
    }

    public static InventoryOperationResult PartialSuccess(InventoryState newInventory, string message, int affectedCount)
    {
        return new InventoryOperationResult(false, true, message, newInventory, affectedCount);
    }
}
}