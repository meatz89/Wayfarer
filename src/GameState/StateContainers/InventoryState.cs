using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


/// <summary>
/// Immutable state container for inventory data.
/// All modifications must go through operations/commands.
/// </summary>
public sealed class InventoryState
{
    public ImmutableList<string> ItemSlots { get; }
    public int Capacity { get; }

    public InventoryState(int capacity)
    {
        Capacity = capacity;
        List<string> slots = new List<string>();
        for (int i = 0; i < capacity; i++)
        {
            slots.Add(string.Empty);
        }
        ItemSlots = slots.ToImmutableList();
    }

    private InventoryState(ImmutableList<string> itemSlots, int capacity)
    {
        ItemSlots = itemSlots;
        Capacity = capacity;
    }

    /// <summary>
    /// Gets the count of used slots.
    /// </summary>
    public int UsedCapacity => ItemSlots.Count(s => !string.IsNullOrEmpty(s));

    /// <summary>
    /// Gets all non-empty items.
    /// </summary>
    public IEnumerable<string> GetAllItems()
    {
        return ItemSlots.Where(s => !string.IsNullOrEmpty(s));
    }

    /// <summary>
    /// Checks if inventory contains an item.
    /// </summary>
    public bool HasItem(string item)
    {
        return ItemSlots.Contains(item);
    }

    /// <summary>
    /// Gets the count of a specific item.
    /// </summary>
    public int GetItemCount(string item)
    {
        return ItemSlots.Count(s => s == item);
    }

    /// <summary>
    /// Checks if there's a free slot.
    /// </summary>
    public bool HasFreeSlot()
    {
        return ItemSlots.Any(s => string.IsNullOrEmpty(s));
    }

    /// <summary>
    /// Creates a new InventoryState with an item added.
    /// Returns null if no free slot available.
    /// </summary>
    public InventoryState WithAddedItem(string item)
    {
        int firstEmptyIndex = ItemSlots.FindIndex(s => string.IsNullOrEmpty(s));
        if (firstEmptyIndex == -1) return null;

        return new InventoryState(ItemSlots.SetItem(firstEmptyIndex, item), Capacity);
    }

    /// <summary>
    /// Creates a new InventoryState with an item removed.
    /// Returns null if item not found.
    /// </summary>
    public InventoryState WithRemovedItem(string item)
    {
        int itemIndex = ItemSlots.FindIndex(s => s == item);
        if (itemIndex == -1) return null;

        return new InventoryState(ItemSlots.SetItem(itemIndex, string.Empty), Capacity);
    }

    /// <summary>
    /// Creates a new InventoryState with all slots cleared.
    /// </summary>
    public InventoryState WithClearedSlots()
    {
        List<string> emptySlots = new List<string>();
        for (int i = 0; i < Capacity; i++)
        {
            emptySlots.Add(string.Empty);
        }
        return new InventoryState(emptySlots.ToImmutableList(), Capacity);
    }

    /// <summary>
    /// Creates a new InventoryState with multiple items added.
    /// Returns the new state and the number of items actually added.
    /// </summary>
    public (InventoryState newState, int addedCount) WithAddedItems(string item, int count)
    {
        InventoryState currentState = this;
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            InventoryState newState = currentState.WithAddedItem(item);
            if (newState == null) break;

            currentState = newState;
            addedCount++;
        }

        return (currentState, addedCount);
    }

    /// <summary>
    /// Creates a new InventoryState with multiple items removed.
    /// Returns the new state and the number of items actually removed.
    /// </summary>
    public (InventoryState newState, int removedCount) WithRemovedItems(string item, int count)
    {
        InventoryState currentState = this;
        int removedCount = 0;

        for (int i = 0; i < count; i++)
        {
            InventoryState newState = currentState.WithRemovedItem(item);
            if (newState == null) break;

            currentState = newState;
            removedCount++;
        }

        return (currentState, removedCount);
    }

    /// <summary>
    /// Creates an InventoryState from a mutable Inventory object.
    /// </summary>
    public static InventoryState FromInventory(Inventory inventory)
    {
        return new InventoryState(inventory.ItemSlots.ToImmutableList(), inventory.Size);
    }

    /// <summary>
    /// Calculate the total number of slots currently used, considering item sizes
    /// </summary>
    public int GetUsedSlots(ItemRepository itemRepository)
    {
        int usedSlots = 0;

        foreach (string itemId in ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = itemRepository.GetItemById(itemId);
                if (item != null)
                {
                    usedSlots += item.GetRequiredSlots();
                }
                else
                {
                    // Fallback for items not found in repository
                    usedSlots += 1;
                }
            }
        }

        return usedSlots;
    }

    /// <summary>
    /// Check if there's enough space to add an item considering its size
    /// </summary>
    public bool CanAddItem(Item item, ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        if (item == null) return false;

        int requiredSlots = item.GetRequiredSlots();
        int usedSlots = GetUsedSlots(itemRepository);
        int maxSlots = GetMaxSlots(itemRepository, currentTransport);

        return (usedSlots + requiredSlots) <= maxSlots;
    }

    /// <summary>
    /// Get the maximum number of slots available (base capacity + transport bonuses)
    /// </summary>
    public int GetMaxSlots(ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        // Base inventory: 5 slots as specified in UserStories.md
        int baseSlots = 5;

        // Add transport bonuses
        if (currentTransport.HasValue)
        {
            switch (currentTransport.Value)
            {
                case TravelMethods.Cart:
                    baseSlots += 2; // Cart adds 2 slots but blocks mountain routes
                    break;
                case TravelMethods.Carriage:
                    baseSlots += 1; // Carriage adds modest storage
                    break;
                    // Walking, Horseback, Boat use base capacity
            }
        }

        return baseSlots;
    }

    /// <summary>
    /// Get available slot space
    /// </summary>
    public int GetAvailableSlots(ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        return GetMaxSlots(itemRepository, currentTransport) - GetUsedSlots(itemRepository);
    }
}