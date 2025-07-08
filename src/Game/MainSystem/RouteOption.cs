
public class RouteOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public TravelMethods Method { get; set; }
    public int BaseCoinCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TimeBlockCost { get; set; }
    public TimeBlocks? DepartureTime { get; set; }
    public bool IsDiscovered { get; set; } = true;
    public List<string> RequiredRouteTypes { get; set; } = new List<string>();
    public int MaxItemCapacity { get; set; } = 3;
    public string Description { get; set; }

    public bool CanTravel(ItemRepository itemRepository, Player player, int totalWeight)
    {
        // Check if player has enough coins
        if (player.Coins < BaseCoinCost) return false;

        // Calculate adjusted stamina cost based on weight
        int adjustedStaminaCost = CalculateWeightAdjustedStaminaCost(totalWeight);

        // Check if player has enough stamina
        if (player.Stamina < adjustedStaminaCost) return false;

        // Check inventory size against transport capacity
        int itemCount = player.Inventory.ItemSlots.Count(i => i != null);
        if (itemCount > MaxItemCapacity) return false;

        // Check for required route types
        foreach (string requiredType in RequiredRouteTypes)
        {
            bool hasEnablingItem = false;
            foreach (string itemName in player.Inventory.ItemSlots)
            {
                if (itemName != null)
                {
                    Item item = itemRepository.GetItemByName(itemName);
                    if (item != null && item.EnabledRouteTypes.Contains(requiredType))
                    {
                        hasEnablingItem = true;
                        break;
                    }
                }
            }

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

    public int CalculateWeightAdjustedStaminaCost(int totalWeight)
    {
        int adjustedStaminaCost = BaseStaminaCost;

        // Apply weight penalties
        if (totalWeight >= 4 && totalWeight <= 6)
        {
            adjustedStaminaCost += 1;
        }
        else if (totalWeight >= 7)
        {
            adjustedStaminaCost += 2;
        }

        return adjustedStaminaCost;
    }

    internal EncounterContext GetEncounter(int seed)
    {
        return null;
    }
}

