public class Inventory
{
    private ResourceTypes[] Slots;

    public int MaxCapacity { get { return Slots.Length; } }
    public int UsedCapacity { get { return Slots.Count(s => s != ResourceTypes.None); } }

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
            while (currentCount < count && AddResource(item))
            {
                currentCount++;
            }
        }
        else if (count < currentCount) // Removing items
        {
            while (currentCount > count && RemoveResource(item))
            {
                currentCount--;
            }
        }
    }

    // Method to add multiple items of the same type to the inventory
    public int AddResources(ResourceTypes resource, int count)
    {
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (AddResource(resource))
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
    public int RemoveResources(ResourceTypes resource, int count)
    {
        int removedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (RemoveResource(resource))
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
    private bool AddResource(ResourceTypes item)
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
    public bool RemoveResource(ResourceTypes item)
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


    public void AddItem(ItemTypes itemName)
    {

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