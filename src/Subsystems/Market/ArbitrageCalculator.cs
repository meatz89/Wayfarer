/// <summary>
/// Calculates arbitrage opportunities between locations.
/// Identifies profitable trade routes and provides trading insights.
/// </summary>
public class ArbitrageCalculator
{
    private readonly GameWorld _gameWorld;
    private readonly PriceManager _priceManager;
    private readonly ItemRepository _itemRepository;

    public ArbitrageCalculator(
        GameWorld gameWorld,
        PriceManager priceManager,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _priceManager = priceManager;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Represents an arbitrage opening for trading an item between locations
    /// </summary>
    public class ArbitrageOpening
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string BuyLocationId { get; set; }
        public string BuyLocationName { get; set; }
        public int BuyPrice { get; set; }
        public string SellLocationId { get; set; }
        public string SellLocationName { get; set; }
        public int SellPrice { get; set; }
        public int GrossProfit { get; set; }
        public int TravelCost { get; set; }
        public int NetProfit { get; set; }
        public float ProfitMargin { get; set; } // Percentage return on investment
        public int RequiredCapital { get; set; }
        public bool IsCurrentlyProfitable { get; set; }
        public string OpeningDescription { get; set; }
        public int DistanceBetweenLocations { get; set; }
        public float ProfitPerDistance { get; set; }
    }

    /// <summary>
    /// Represents a complete trade route with multiple stops
    /// </summary>
    public class TradeRoute
    {
        public List<string> LocationSequence { get; set; } = new List<string>();
        public List<ArbitrageOpening> Trades { get; set; } = new List<ArbitrageOpening>();
        public int TotalProfit { get; set; }
        public int RequiredCapital { get; set; }
        public int TotalDistance { get; set; }
        public float AverageProfitMargin { get; set; }
        public string RouteDescription { get; set; }
    }

    // ========== ARBITRAGE CALCULATION ==========

    /// <summary>
    /// Find the best arbitrage opening for a specific item
    /// PHASE 6D: Accept Item object instead of ID
    /// </summary>
    public ArbitrageOpening FindBestOpening(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        List<Location> locations = _gameWorld.Locations;
        ArbitrageOpening bestOpening = null;
        int highestProfit = 0;

        // Compare all Location pairs
        foreach (Location buyLocation in locations)
        {
            int buyPrice = _priceManager.GetBuyPrice(item, buyLocation.Name);
            if (buyPrice <= 0) continue; // Item not available for purchase here

            foreach (Location sellLocation in locations)
            {
                if (buyLocation == sellLocation) continue; // Skip same location

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation.Name);
                if (sellPrice <= 0) continue; // Can't sell here

                int grossProfit = sellPrice - buyPrice;
                if (grossProfit <= 0) continue; // Not profitable

                // Calculate travel cost
                int distance = CalculateDistance(buyLocation.Name, sellLocation.Name);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = grossProfit - travelCost;

                if (netProfit > highestProfit)
                {
                    highestProfit = netProfit;
                    bestOpening = new ArbitrageOpening
                    {
                        // ADR-007: Use Name instead of deleted Id
                        ItemId = item.Name,
                        ItemName = item.Name,
                        BuyLocationId = buyLocation.Name,
                        BuyLocationName = buyLocation.Name,
                        BuyPrice = buyPrice,
                        SellLocationId = sellLocation.Name,
                        SellLocationName = sellLocation.Name,
                        SellPrice = sellPrice,
                        GrossProfit = grossProfit,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMargin = (float)netProfit / buyPrice,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = netProfit > 0,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                        OpeningDescription = GenerateOpeningDescription(item.Name, buyLocation.Name, sellLocation.Name, netProfit)
                    };
                }
            }
        }

        return bestOpening;
    }

    /// <summary>
    /// Find all profitable arbitrage opportunities
    /// </summary>
    public List<ArbitrageOpening> FindAllOpportunities()
    {
        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item item in allItems)
        {
            ArbitrageOpening opening = FindBestOpening(item);
            if (opening != null && opening.IsCurrentlyProfitable)
            {
                opportunities.Add(opening);
            }
        }

        // Sort by net profit descending
        return opportunities.OrderByDescending(o => o.NetProfit).ToList();
    }

    /// <summary>
    /// Calculate profit for a specific trade between two locations
    /// </summary>
    public int CalculateProfit(string itemId, string buyLocationId, string sellLocationId)
    {
        int buyPrice = _priceManager.GetBuyPrice(itemId, buyLocationId);
        int sellPrice = _priceManager.GetSellPrice(itemId, sellLocationId);

        if (buyPrice <= 0 || sellPrice <= 0) return -1;

        int distance = CalculateDistance(buyLocationId, sellLocationId);
        int travelCost = CalculateTravelCost(distance);

        return sellPrice - buyPrice - travelCost;
    }

    // ========== ADVANCED ARBITRAGE ANALYSIS ==========

    /// <summary>
    /// Find arbitrage opportunities the player can afford
    /// </summary>
    public List<ArbitrageOpening> FindAffordableOpportunities()
    {
        Player player = _gameWorld.GetPlayer();
        int availableCoins = player.Coins;

        return FindAllOpportunities()
            .Where(o => o.RequiredCapital <= availableCoins)
            .ToList();
    }

    /// <summary>
    /// Find arbitrage opportunities from the player's current location
    /// </summary>
    public List<ArbitrageOpening> FindOpportunitiesFromCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        string currentLocationId = currentLocation.Name;
        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item item in allItems)
        {
            int buyPrice = _priceManager.GetBuyPrice(item, currentLocationId);
            if (buyPrice <= 0) continue;

            List<Location> locations = _gameWorld.Locations;
            foreach (Location sellLocation in locations)
            {
                if (sellLocation.Name == currentLocationId) continue;

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation.Name);
                if (sellPrice <= 0) continue;

                int distance = CalculateDistance(currentLocationId, sellLocation.Name);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = sellPrice - buyPrice - travelCost;

                if (netProfit > 0)
                {
                    // ADR-007: Use Name instead of deleted Id
                    opportunities.Add(new ArbitrageOpening
                    {
                        ItemId = item.Name,
                        ItemName = item.Name,
                        BuyLocationId = currentLocationId,
                        BuyLocationName = currentLocation.Name,
                        BuyPrice = buyPrice,
                        SellLocationId = sellLocation.Name,
                        SellLocationName = sellLocation.Name,
                        SellPrice = sellPrice,
                        GrossProfit = sellPrice - buyPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMargin = (float)netProfit / buyPrice,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                        OpeningDescription = GenerateOpeningDescription(item.Name, currentLocation.Name, sellLocation.Name, netProfit)
                    });
                }
            }
        }

        return opportunities.OrderByDescending(o => o.NetProfit).ToList();
    }

    /// <summary>
    /// Find opportunities for items the player already owns
    /// </summary>
    public List<ArbitrageOpening> FindOpportunitiesForInventory()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        string currentLocationId = currentLocation.Name;
        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();

        foreach (string itemId in player.Inventory.GetItemIds())
        {
            Item item = _itemRepository.GetItemById(itemId);
            if (item == null)
                throw new InvalidOperationException($"Item not found in repository: {itemId}");

            int currentSellPrice = _priceManager.GetSellPrice(itemId, currentLocationId);
            List<Location> locations = _gameWorld.Locations;

            foreach (Location sellLocation in locations)
            {
                if (sellLocation.Name == currentLocationId) continue;

                int otherSellPrice = _priceManager.GetSellPrice(itemId, sellLocation.Name);
                if (otherSellPrice <= currentSellPrice) continue;

                int distance = CalculateDistance(currentLocationId, sellLocation.Name);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = otherSellPrice - currentSellPrice - travelCost;

                if (netProfit > 0)
                {
                    opportunities.Add(new ArbitrageOpening
                    {
                        ItemId = itemId,
                        ItemName = item.Name,
                        BuyLocationId = currentLocationId,
                        BuyLocationName = currentLocation.Name,
                        BuyPrice = currentSellPrice, // What we could sell for here
                        SellLocationId = sellLocation.Name,
                        SellLocationName = sellLocation.Name,
                        SellPrice = otherSellPrice,
                        GrossProfit = otherSellPrice - currentSellPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMargin = currentSellPrice > 0 ? (float)netProfit / currentSellPrice : 0,
                        RequiredCapital = 0, // Already own the item
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit,
                        OpeningDescription = $"Sell {item.Name} in {sellLocation.Name} for {netProfit} coin profit"
                    });
                }
            }
        }

        return opportunities.OrderByDescending(o => o.NetProfit).ToList();
    }

    // ========== TRADE ROUTE PLANNING ==========

    /// <summary>
    /// Plan an optimal trade route visiting multiple locations
    /// </summary>
    public TradeRoute PlanOptimalRoute(int maxStops = 3)
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocationEntity = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocationEntity == null)
            throw new InvalidOperationException("Player has no current location");

        string startLocation = currentLocationEntity.Name;
        int availableCapital = player.Coins;

        TradeRoute bestRoute = new TradeRoute();
        bestRoute.LocationSequence.Add(startLocation);

        string currentLocation = startLocation;
        int currentCapital = availableCapital;
        int totalProfit = 0;

        for (int stop = 0; stop < maxStops; stop++)
        {
            // Find best opening from current Location with available capital
            List<ArbitrageOpening> opportunities = FindAllOpportunitiesFrom(currentLocation)
                .Where(o => o.RequiredCapital <= currentCapital)
                .OrderByDescending(o => o.NetProfit)
                .ToList();

            if (opportunities.Count == 0) break;

            ArbitrageOpening bestOpp = opportunities.First();
            bestRoute.Trades.Add(bestOpp);
            bestRoute.LocationSequence.Add(bestOpp.SellLocationId);

            // Update capital and profit
            currentCapital = currentCapital - bestOpp.BuyPrice + bestOpp.SellPrice - bestOpp.TravelCost;
            totalProfit += bestOpp.NetProfit;
            currentLocation = bestOpp.SellLocationId;
        }

        bestRoute.TotalProfit = totalProfit;
        bestRoute.RequiredCapital = availableCapital;
        bestRoute.TotalDistance = bestRoute.Trades.Sum(t => t.DistanceBetweenLocations);
        bestRoute.AverageProfitMargin = bestRoute.Trades.Count > 0
            ? bestRoute.Trades.Average(t => t.ProfitMargin)
            : 0;
        bestRoute.RouteDescription = GenerateRouteDescription(bestRoute);

        return bestRoute;
    }

    /// <summary>
    /// Find all opportunities from a specific location
    /// </summary>
    private List<ArbitrageOpening> FindAllOpportunitiesFrom(string fromLocationId)
    {
        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();
        List<Item> allItems = _itemRepository.GetAllItems();
        List<Location> locations = _gameWorld.Locations;
        Location fromLocation = _gameWorld.GetLocation(fromLocationId);

        foreach (Item item in allItems)
        {
            int buyPrice = _priceManager.GetBuyPrice(item, fromLocationId);
            if (buyPrice <= 0) continue;

            foreach (Location sellLocation in locations)
            {
                if (sellLocation.Name == fromLocationId) continue;

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation.Name);
                if (sellPrice <= buyPrice) continue;

                int distance = CalculateDistance(fromLocationId, sellLocation.Name);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = sellPrice - buyPrice - travelCost;

                if (netProfit > 0)
                {
                    // ADR-007: Use Name instead of deleted Id
                    opportunities.Add(new ArbitrageOpening
                    {
                        ItemId = item.Name,
                        ItemName = item.Name,
                        BuyLocationId = fromLocationId,
                        BuyLocationName = fromLocation.Name,
                        BuyPrice = buyPrice,
                        SellLocationId = sellLocation.Name,
                        SellLocationName = sellLocation.Name,
                        SellPrice = sellPrice,
                        GrossProfit = sellPrice - buyPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMargin = (float)netProfit / buyPrice,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? (float)netProfit / distance : netProfit
                    });
                }
            }
        }

        return opportunities;
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Calculate distance between two locations using hex coordinates
    /// </summary>
    private int CalculateDistance(string fromLocationId, string toLocationId)
    {
        if (fromLocationId == toLocationId) return 0;

        // Get locations from GameWorld
        Location fromLocation = _gameWorld.GetLocation(fromLocationId);
        Location toLocation = _gameWorld.GetLocation(toLocationId);

        if (fromLocation == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location not found: {fromLocationId}");

        if (toLocation == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location not found: {toLocationId}");

        // Verify locations have hex positions
        if (fromLocation.HexPosition == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location '{fromLocationId}' missing HexPosition");

        if (toLocation.HexPosition == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location '{toLocationId}' missing HexPosition");

        // Calculate hex distance
        return fromLocation.HexPosition.Value.DistanceTo(toLocation.HexPosition.Value);
    }

    /// <summary>
    /// Calculate travel cost based on distance
    /// </summary>
    private int CalculateTravelCost(int distance)
    {
        // Simple linear cost model
        // Could be enhanced with route difficulty, transport costs, etc.
        return distance * 2; // 2 coins per unit distance
    }

    /// <summary>
    /// Generate human-readable opening description
    /// </summary>
    private string GenerateOpeningDescription(string itemName, string buyLocation, string sellLocation, int profit)
    {
        if (profit > 20)
            return $"Excellent opening: Buy {itemName} in {buyLocation} and sell in {sellLocation} for {profit} coin profit!";
        else if (profit > 10)
            return $"Good trade: {itemName} from {buyLocation} to {sellLocation} yields {profit} coins";
        else
            return $"Modest profit: {itemName} trade between {buyLocation} and {sellLocation} for {profit} coins";
    }

    /// <summary>
    /// Generate route description
    /// </summary>
    private string GenerateRouteDescription(TradeRoute route)
    {
        if (route.Trades.Count == 0)
            return "No profitable trades found";

        List<string> trades = route.Trades.Select(t =>
            $"{t.ItemName} ({t.BuyLocationName} → {t.SellLocationName})"
        ).ToList();

        return $"Trade route with {route.TotalProfit} total profit: " + string.Join(" → ", trades);
    }
}
