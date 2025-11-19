/// <summary>
/// HIGHLANDER: Object references ONLY - stores Item objects, not IDs
/// Weight-based inventory system: 10 weight capacity, items have 1-6 weight
/// </summary>
public class Inventory
{
    private const int BASE_WEIGHT_CAPACITY = 10;
    private readonly List<Item> _items = new List<Item>();

    public int MaxWeight { get; private set; }

    public Inventory(int maxWeight = BASE_WEIGHT_CAPACITY)
    {
        MaxWeight = maxWeight;
    }

    public List<Item> GetAllItems()
    {
        return _items.ToList();
    }

    public void Clear()
    {
        _items.Clear();
    }

    public int GetCapacity()
    {
        return MaxWeight;
    }

    public void SetItemCount(Item item, int count)
    {
        int currentCount = Count(item);

        if (count > currentCount)
        {
            for (int i = currentCount; i < count; i++)
            {
                _items.Add(item);
            }
        }
        else if (count < currentCount)
        {
            for (int i = currentCount; i > count; i--)
            {
                _items.Remove(item);
            }
        }
    }

    public void Add(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public int AddItems(Item item, int count)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        for (int i = 0; i < count; i++)
        {
            _items.Add(item);
        }
        return count;
    }

    public int RemoveItems(Item item, int count)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        int removedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (_items.Remove(item))
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

    public bool Remove(Item item)
    {
        return _items.Remove(item);
    }

    public bool Contains(Item item)
    {
        return _items.Contains(item);
    }

    public int Count(Item item)
    {
        return _items.Count(i => i == item);
    }

    public bool HasFreeWeight()
    {
        return GetUsedWeight() < MaxWeight;
    }

    /// <summary>
    /// Calculate the total weight currently used
    /// </summary>
    public int GetUsedWeight()
    {
        int totalWeight = 0;
        foreach (Item item in _items)
        {
            totalWeight += item.GetWeight();
        }
        return totalWeight;
    }

    /// <summary>
    /// Get the maximum weight capacity (base capacity + transport bonuses)
    /// </summary>
    public int GetMaxWeight(TravelMethods? currentTransport)
    {
        int baseWeight = MaxWeight;

        if (currentTransport.HasValue)
        {
            switch (currentTransport.Value)
            {
                case TravelMethods.Cart:
                    baseWeight += 6;
                    break;
                case TravelMethods.Carriage:
                    baseWeight += 3;
                    break;
            }
        }

        return baseWeight;
    }

    public bool AddItemWithWeightCheck(Item item, TravelMethods? currentTransport)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        int maxCapacity = GetMaxWeight(currentTransport);
        if (GetUsedWeight() + item.GetWeight() <= maxCapacity)
        {
            _items.Add(item);
            return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return GetUsedWeight() >= MaxWeight;
    }

    public bool CanAddItem(Item item)
    {
        if (item == null) return false;
        return GetUsedWeight() + item.GetWeight() <= MaxWeight;
    }
}