
public class Inventory
{
    public string[] ItemSlots;
    public int Size => ItemSlots.Length;
    public int UsedCapacity => ItemSlots.Count(s =>
                                {
                                    return s != string.Empty;
                                });

    public Inventory(int size)
    {
        ItemSlots = new string[size];
        Clear();
    }

    public List<string> GetAllItems()
    {
        return ItemSlots.ToList();
    }

    public void Clear()
    {
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            ItemSlots[i] = string.Empty;
        }
    }

    public string GetFirstItem()
    {
        foreach (string itemType in ItemSlots)
        {
            if (itemType != string.Empty && HasItem(itemType))
            {
                return itemType;
            }
        }
        return string.Empty;
    }

    public int GetCapacity()
    {
        return ItemSlots.Length;
    }

    public void SetItemCount(string item, int count)
    {
        int currentCount = GetItemCount(item);

        if (count > currentCount)
        {
            while (currentCount < count && AddItem(item))
            {
                currentCount++;
            }
        }
        else if (count < currentCount)
        {
            while (currentCount > count && RemoveItem(item))
            {
                currentCount--;
            }
        }
    }

    public void Add(Item item)
    {
        AddItem(item.Id);
    }

    public int AddItems(string resource, int count)
    {
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (AddItem(resource))
            {
                addedCount++;
            }
            else
            {
                break;
            }
        }

        return addedCount;
    }

    public int RemoveItems(string resource, int count)
    {
        int removedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (RemoveItem(resource))
            {
                removedCount++;
            }
            else
            {
                break;
            }
        }

        return removedCount;
    }

    public bool AddItem(Item Item)
    {
        return AddItem(Item.ToString());
    }

    public bool AddItem(string item)
    {
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (ItemSlots[i] == string.Empty)
            {
                ItemSlots[i] = item;
                return true;
            }
        }
        return false;
    }

    public bool RemoveItem(string item)
    {
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (ItemSlots[i] == item)
            {
                ItemSlots[i] = string.Empty;
                return true;
            }
        }
        return false;
    }
    
    public bool TryAddItem(string item)
    {
        return AddItem(item);
    }

    public bool HasItem(string item)
    {
        foreach (string slot in ItemSlots)
        {
            if (slot == item)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasFreeSlot()
    {
        foreach (string slot in ItemSlots)
        {
            if (slot == string.Empty)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemCount(string item)
    {
        int count = 0;
        foreach (string slot in ItemSlots)
        {
            if (slot == item)
            {
                count++;
            }
        }
        return count;
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

    /// <summary>
    /// Add item with size-aware slot checking
    /// </summary>
    public bool AddItemWithSizeCheck(Item item, ItemRepository itemRepository, TravelMethods? currentTransport = null)
    {
        if (!CanAddItem(item, itemRepository, currentTransport))
        {
            return false;
        }

        return AddItem(item.Id);
    }

    public bool IsFull()
    {
        return !HasFreeSlot();
    }

    public void Remove(Item item)
    {
        this.RemoveItem(item.Id);
    }
}