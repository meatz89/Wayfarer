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
            while (currentCount > count && RemoveItem(item))
            {
                currentCount--;
            }
        }
    }


    // Method to add an item to the inventory
    public bool AddItem(ResourceTypes item)
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
    public bool RemoveItem(ResourceTypes item)
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
        foreach (var slot in Slots)
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
        foreach (var slot in Slots)
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
        foreach (var slot in Slots)
        {
            if (slot == item)
            {
                count++;
            }
        }
        return count;
    }

    // Method to display inventory contents
    public void DisplayInventory()
    {
        Console.WriteLine("Inventory Contents:");
        for (int i = 0; i < Slots.Length; i++)
        {
            Console.WriteLine($"Slot {i + 1}: {Slots[i]}");
        }
    }
}