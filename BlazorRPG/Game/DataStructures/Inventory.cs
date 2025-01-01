public class Inventory
{
    private ResourceTypes[] Slots;

    public Inventory(int size)
    {
        Slots = new ResourceTypes[size];
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = ResourceTypes.None; // Initialize slots as empty
        }
    }

    public int GetCapacityFor(ResourceTypes item)
    {
        return Slots.Length;
    }

    public void SetItemCount(ResourceTypes item, int count)
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
            while (currentCount > count && RemoveItems(item))
            {
                currentCount--;
            }
        }
    }

    // Method to add multiple items of the same type to the inventory
    public int AddItems(ResourceTypes item, int count)
    {
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (AddItem(item))
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
    public int RemoveItems(ResourceTypes item, int count)
    {
        int removedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (RemoveItems(item))
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
    private bool AddItem(ResourceTypes item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == ResourceTypes.None)
            {
                Slots[i] = item;
                return true; // Successfully added
            }
        }
        return false; // Inventory full
    }

    // Method to remove an item from the inventory
    private bool RemoveItems(ResourceTypes item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == item)
            {
                Slots[i] = ResourceTypes.None;
                return true; // Successfully removed
            }
        }
        return false; // Item not found
    }

    // Method to check if the inventory contains a specific item
    public bool ContainsItem(ResourceTypes item)
    {
        foreach (ResourceTypes slot in Slots)
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
        foreach (ResourceTypes slot in Slots)
        {
            if (slot == ResourceTypes.None)
            {
                emptyCount++;
            }
        }
        return emptyCount;
    }

    // Method to count the number of a given item type in the inventory
    public int GetItemCount(ResourceTypes item)
    {
        int count = 0;
        foreach (ResourceTypes slot in Slots)
        {
            if (slot == item)
            {
                count++;
            }
        }
        return count;
    }
}