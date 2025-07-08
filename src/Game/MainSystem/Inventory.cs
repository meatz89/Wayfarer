public class Inventory
{
    public string[] ItemSlots;
    public int Size { get { return ItemSlots.Length; } }
    public int UsedCapacity
    {
        get
        {
            return ItemSlots.Count(s =>
    {
        return s != string.Empty;
    });
        }
    }

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

    public void Apply(object invChange)
    {
        throw new NotImplementedException();
    }

}