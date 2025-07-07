
public class RouteOption
{
    public string Name { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TimeBlockCost { get; set; }
    public TimeBlocks? DepartureTime { get; set; }
    public bool IsDiscovered { get; set; } = true;
    public List<string> RequiredRouteTypes { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; } = 3;
    public Location Origin { get; internal set; }
    public Location Destination { get; internal set; }
    public TravelMethods Method { get; set; }

    public bool CanTravel(ItemRepository itemRepository, Player player)
    {
        // Check if player has enough coins
        if (player.Coins < BaseCoinCost) return false;

        // Check weight-adjusted stamina cost
        int adjustedStaminaCost = BaseStaminaCost;
        int totalWeight = player.CalculateTotalWeight();

        if (totalWeight >= 4 && totalWeight <= 6)
        {
            adjustedStaminaCost += 1;
        }
        else if (totalWeight >= 7)
        {
            adjustedStaminaCost += 2;
        }

        if (player.Stamina < adjustedStaminaCost) return false;

        // Check inventory size against transport capacity
        int itemCount = player.Inventory.ItemSlots.Count(i => i != null);
        if (itemCount > MaxItemCapacity) return false;

        // Check for required route types
        foreach (string requiredType in RequiredRouteTypes)
        {
            bool hasEnablingItem = player.Inventory.ItemSlots
                .Select(itemId => itemRepository.GetItemByName(itemId))
                .Any(item => item != null && item.EnabledRouteTypes.Contains(requiredType));

            if (!hasEnablingItem) return false;
        }

        return true;
    }

    public int GetActualStaminaCost()
    {
        return BaseStaminaCost;
    }

    public int GetActualTimeCost()
    {
        return TimeBlockCost;
    }

    internal EncounterContext GetEncounter(int seed)
    {
        throw new NotImplementedException();
    }
}

