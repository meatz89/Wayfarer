
public class Inventory
{
    private ItemTypes[] Slots;

    public int MaxCapacity { get { return Slots.Length; } }
    public int UsedCapacity { get { return Slots.Count(s => s != ItemTypes.None); } }

    public Inventory(int size)
    {
        Slots = new ItemTypes[size];
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = ItemTypes.None; // Initialize slots as empty
        }
    }

    public ItemTypes GetFirstItem()
    {
        foreach (ItemTypes itemType in Enum.GetValues(typeof(ItemTypes)))
        {
            if (itemType != ItemTypes.None && ContainsItem(itemType))
            {
                return itemType;
            }
        }
        return ItemTypes.None;
    }

    public int GetCapacityFor(ItemTypes item)
    {
        return Slots.Length;
    }

    public void SetItemCount(ItemTypes item, int count)
    {
        int currentCount = GetItemCount(item);

        if (count > currentCount) // Adding items
        {
            while (currentCount < count && AddItem(item))
            {
                currentCount++;
            }
        }
        else if (count < currentCount) // Removing items
        {
            while (currentCount > count && RemoveItem(item))
            {
                currentCount--;
            }
        }
    }

    // Method to add multiple items of the same type to the inventory
    public int AddItems(ItemTypes resource, int count)
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
                break; // Stop if inventory is full
            }
        }

        return addedCount; // Return the number of items successfully added
    }

    // Method to remove multiple items of the same type from the inventory
    public int RemoveItems(ItemTypes resource, int count)
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
                break; // Stop if no more items of the type are found
            }
        }

        return removedCount; // Return the number of items successfully removed
    }

    // Method to add an item to the inventory
    public bool AddItem(ItemTypes item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == ItemTypes.None)
            {
                Slots[i] = item;
                return true; // Successfully added
            }
        }
        return false; // Inventory full
    }

    // Method to remove an item from the inventory
    public bool RemoveItem(ItemTypes item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == item)
            {
                Slots[i] = ItemTypes.None;
                return true; // Successfully removed
            }
        }
        return false; // Item not found
    }

    // Method to check if the inventory contains a specific item
    public bool ContainsItem(ItemTypes item)
    {
        foreach (ItemTypes slot in Slots)
        {
            if (slot == item)
            {
                return true;
            }
        }
        return false;
    }

    // Method to check how many empty slots are left
    public int GetEmptySlots()
    {
        int emptyCount = 0;
        foreach (ItemTypes slot in Slots)
        {
            if (slot == ItemTypes.None)
            {
                emptyCount++;
            }
        }
        return emptyCount;
    }

    // Method to count the number of a given item type in the inventory
    public int GetItemCount(ItemTypes item)
    {
        int count = 0;
        foreach (ItemTypes slot in Slots)
        {
            if (slot == item)
            {
                count++;
            }
        }
        return count;
    }
    public bool HasItemOfType(ItemTypes weapon)
    {
        return true;
    }

    public List<Item> GetItemsOfType(ItemTypes itemType)
    {
        return new List<Item>();
    }

    public void RemoveItem(Item item)
    {

    }
}