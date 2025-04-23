
public class Inventory
{
    private string[] Slots;
    public int MaxCapacity { get { return Slots.Length; } }
    public int UsedCapacity { get { return Slots.Count(s => s != string.Empty); } }

    public Inventory(int size)
    {
        Slots = new string[size];
        Clear();
    }

    public void Clear()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = string.Empty;
        }
    }

    public string GetFirstItem()
    {
        foreach (string itemType in Slots)
        {
            if (itemType != string.Empty && ContainsItem(itemType))
            {
                return itemType;
            }
        }
        return string.Empty;
    }

    public int GetCapacity()
    {
        return Slots.Length;
    }

    public void SetItemCount(string item, int count)
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
                break; // Stop if inventory is full
            }
        }

        return addedCount; // Return the number of items successfully added
    }

    // Method to remove multiple items of the same type from the inventory
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
                break; // Stop if no more items of the type are found
            }
        }

        return removedCount; // Return the number of items successfully removed
    }

    // Method to add an item to the inventory
    public bool AddItem(ItemTypes itemTypes)
    {
        return AddItem(itemTypes.ToString());
    }

    // Method to add an item to the inventory
    public bool AddItem(string item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == string.Empty)
            {
                Slots[i] = item;
                return true; // Successfully added
            }
        }
        return false; // Inventory full
    }

    // Method to remove an item from the inventory
    public bool RemoveItem(string item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == item)
            {
                Slots[i] = string.Empty;
                return true; // Successfully removed
            }
        }
        return false; // Item not found
    }

    // Method to check if the inventory contains a specific item
    public bool ContainsItem(string item)
    {
        foreach (string slot in Slots)
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
        foreach (string slot in Slots)
        {
            if (slot == string.Empty)
            {
                emptyCount++;
            }
        }
        return emptyCount;
    }

    // Method to count the number of a given item type in the inventory
    public int GetItemCount(string item)
    {
        int count = 0;
        foreach (string slot in Slots)
        {
            if (slot == item)
            {
                count++;
            }
        }
        return count;
    }

    internal void Apply(object invChange)
    {
        throw new NotImplementedException();
    }
}