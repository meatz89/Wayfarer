public class Inventory
{
    public string[] Slots;
    public int Capacity { get { return Slots.Length; } }
    public int UsedCapacity
    {
        get
        {
            return Slots.Count(s =>
    {
        return s != string.Empty;
    });
        }
    }

    public Inventory(int size)
    {
        Slots = new string[size];
        Clear();
    }

    public List<string> GetAllItems()
    {
        return Slots.ToList();
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

    public bool AddItem(ItemTypes itemTypes)
    {
        return AddItem(itemTypes.ToString());
    }

    public bool AddItem(string item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == string.Empty)
            {
                Slots[i] = item;
                return true; 
            }
        }
        return false; 
    }

    public bool RemoveItem(string item)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] == item)
            {
                Slots[i] = string.Empty;
                return true;
            }
        }
        return false;
    }

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

    public void Apply(object invChange)
    {
        throw new NotImplementedException();
    }

}