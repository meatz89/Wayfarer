using Microsoft.AspNetCore.Components;

public partial class TravelSelectionBase : ComponentBase
{
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    // ✅ ARCHITECTURAL COMPLIANCE: Only inject GameWorldManager for actions
    // ItemRepository allowed for read-only UI data binding
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Parameter] public Location CurrentLocation { get; set; }
    [Parameter] public List<Location> Locations { get; set; }
    [Parameter] public EventCallback<string> OnTravel { get; set; }
    [Parameter] public EventCallback<RouteOption> OnTravelRoute { get; set; }

    public bool ShowEnablingItems => GameWorld.GetPlayer().Inventory.ItemSlots
        .Select(name => ItemRepository.GetItemByName(name))
        .Any(item => item != null && item.EnabledRouteTypes.Any());

    protected override void OnInitialized()
    {
        Console.WriteLine("TravelSelection component initialized");
        base.OnInitialized();
    }

    public List<Location> GetTravelableLocations()
    {
        var locations = Locations?.Where(loc => loc.Id != CurrentLocation?.Id &&
                              GameWorldManager.CanTravelTo(loc.Id)).ToList();
        Console.WriteLine($"GetTravelableLocations: Found {locations?.Count ?? 0} locations");
        Console.WriteLine($"Current location: {CurrentLocation?.Id}");
        Console.WriteLine($"All locations: {string.Join(", ", Locations?.Select(l => l.Id) ?? new string[0])}");
        return locations ?? new List<Location>();
    }


    public void ShowRouteOptions(GameWorld gameWorld, string destinationName, string destinationId)
    {
        // ✅ ARCHITECTURAL COMPLIANCE: Route through GameWorldManager gateway
        List<RouteOption> availableRoutes = GameWorldManager.GetAvailableRoutes(CurrentLocation.Id, destinationId);

        Console.WriteLine($"=== Travel to {destinationName} ===");

        if (availableRoutes.Count == 0)
        {
            Console.WriteLine("No routes available at this time.");
            return;
        }

        // ✅ ARCHITECTURAL COMPLIANCE: Use GameWorldManager for weight calculation
        int totalWeight = GameWorldManager.CalculateTotalWeight();
        string weightStatus = totalWeight <= 3 ? "Light load" :
                                (totalWeight <= 6 ? "Medium load (+1 stamina cost)" : "Heavy load (+2 stamina cost)");

        Console.WriteLine($"Current load: {weightStatus} ({totalWeight} weight units)");
        Console.WriteLine();

        Console.WriteLine("How do you want to travel?");

        for (int i = 0; i < availableRoutes.Count; i++)
        {
            RouteOption route = availableRoutes[i];
            string arrivalTime = CalculateArrivalTimeBlock(gameWorld.CurrentTimeBlock, route.TimeBlockCost);

            // Calculate adjusted stamina cost
            int adjustedStaminaCost = route.BaseStaminaCost;
            if (totalWeight >= 4 && totalWeight <= 6)
            {
                adjustedStaminaCost += 1;
            }
            else if (totalWeight >= 7)
            {
                adjustedStaminaCost += 2;
            }

            string staminaCostDisplay = route.BaseStaminaCost == adjustedStaminaCost ?
                $"{adjustedStaminaCost} stamina" :
                $"{adjustedStaminaCost} stamina (base {route.BaseStaminaCost} + weight penalty)";

            string departureInfo = route.DepartureTime.HasValue ? $", departs at {route.DepartureTime}" : "";
            // ✅ ARCHITECTURAL COMPLIANCE: Route through GameWorldManager gateway
            string affordabilityMarker = GameWorldManager.CanTravelRoute(route) ? "" : " (Cannot afford)";

            Console.WriteLine($"[{i + 1}] {route.Name} - {route.BaseCoinCost} coins, {staminaCostDisplay}{departureInfo}, arrives {arrivalTime}{affordabilityMarker}");

            // Show required route types if any
            if (route.RequiredRouteTypes.Count > 0)
            {
                Console.WriteLine($"    Requires: {string.Join(", ", route.RequiredRouteTypes)}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Current resources: {gameWorld.PlayerCoins} coins, {gameWorld.PlayerStamina} stamina");
        Console.WriteLine($"Current time: {gameWorld.CurrentTimeBlock}");

        // Show items that enable routes
        List<string> enablingItems = new List<string>();
        foreach (string itemName in gameWorld.PlayerInventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null && item.EnabledRouteTypes.Count > 0)
                {
                    enablingItems.Add($"{itemName} (enables {string.Join(", ", item.EnabledRouteTypes)})");
                }
            }
        }

        if (enablingItems.Count > 0)
        {
            Console.WriteLine("Items that enable routes:");
            foreach (string itemInfo in enablingItems)
            {
                Console.WriteLine($"- {itemInfo}");
            }
        }
    }

    public string CalculateArrivalTimeBlock(TimeBlocks currentTimeBlock, int timeBlocksToAdvance)
    {
        int finalTimeBlockValue = ((int)currentTimeBlock + timeBlocksToAdvance) % 5;
        int daysLater = ((int)currentTimeBlock + timeBlocksToAdvance) / 5;

        TimeBlocks arrivalTimeBlock = (TimeBlocks)finalTimeBlockValue;

        if (daysLater == 0)
        {
            return arrivalTimeBlock.ToString();
        }
        else if (daysLater == 1)
        {
            return $"Tomorrow {arrivalTimeBlock}";
        }
        else
        {
            return $"{daysLater} days later, {arrivalTimeBlock}";
        }
    }
}
