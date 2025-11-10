public class Inventory
{
// Weight-based inventory system: 10 weight capacity, items have 1-6 weight
private const int BASE_WEIGHT_CAPACITY = 10;
private readonly List<string> _items;

public int MaxWeight { get; private set; }

public Inventory(int maxWeight = BASE_WEIGHT_CAPACITY)
{
    MaxWeight = maxWeight;
    _items = new List<string>();
}

public List<string> GetAllItems()
{
    return _items.ToList();
}

public List<string> GetItemIds()
{
    return _items.Distinct().ToList();
}

public void Clear()
{
    _items.Clear();
}

public int GetCapacity()
{
    return MaxWeight;
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
    // Weight check happens in CanAddItem - this just adds to collection
    _items.Add(item);
    return true;
}

public bool RemoveItem(string item)
{
    return _items.Remove(item);
}

public bool HasItem(string item)
{
    return _items.Contains(item);
}

public bool HasFreeWeight(ItemRepository itemRepository)
{
    return GetUsedWeight(itemRepository) < MaxWeight;
}

public int GetItemCount(string item)
{
    return _items.Count(i => i == item);
}

/// <summary>
/// Calculate the total weight currently used
/// </summary>
public int GetUsedWeight(ItemRepository itemRepository)
{
    int totalWeight = 0;

    foreach (string itemId in _items)
    {
        Item item = itemRepository.GetItemById(itemId);
        if (item != null)
        {
            totalWeight += item.GetWeight();
        }
    }

    return totalWeight;
}

/// <summary>
/// Get the maximum weight capacity (base capacity + transport bonuses)
/// </summary>
public int GetMaxWeight(TravelMethods? currentTransport)
{
    // Base inventory: 10 weight as per documentation
    int baseWeight = MaxWeight;

    // Add transport bonuses (converted from slot bonuses to weight)
    if (currentTransport.HasValue)
    {
        switch (currentTransport.Value)
        {
            case TravelMethods.Cart:
                baseWeight += 6; // Cart adds significant capacity
                break;
            case TravelMethods.Carriage:
                baseWeight += 3; // Carriage adds modest capacity
                break;
                // Walking, Horseback, Boat use base capacity
        }
    }

    return baseWeight;
}

/// <summary>
/// Add item with weight checking
/// </summary>
public bool AddItemWithWeightCheck(Item item, ItemRepository itemRepository, TravelMethods? currentTransport)
{
    return AddItem(item.Id);
}

public bool IsFull(ItemRepository itemRepository)
{
    return GetUsedWeight(itemRepository) >= MaxWeight;
}

public void Remove(Item item)
{
    RemoveItem(item.Id);
}

internal bool CanAddItem(Item? item, ItemRepository itemRepository)
{
    return true;
}
}