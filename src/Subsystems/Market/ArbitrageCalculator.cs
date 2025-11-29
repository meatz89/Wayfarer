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
    /// HIGHLANDER: Object references only, no string identifiers
    /// </summary>
    public class ArbitrageOpening
    {
        public Item Item { get; set; }
        public Location BuyLocation { get; set; }
        public int BuyPrice { get; set; }
        public Location SellLocation { get; set; }
        public int SellPrice { get; set; }
        public int GrossProfit { get; set; }
        public int TravelCost { get; set; }
        public int NetProfit { get; set; }
        public int ProfitMarginBasisPoints { get; set; }
        public int RequiredCapital { get; set; }
        public bool IsCurrentlyProfitable { get; set; }
        public string OpeningDescription { get; set; }
        public int DistanceBetweenLocations { get; set; }
        public int ProfitPerDistance { get; set; }
    }

    /// <summary>
    /// Represents a complete trade route with multiple stops
    /// HIGHLANDER: Object references only, no string identifiers
    /// </summary>
    public class TradeRoute
    {
        public List<Location> LocationSequence { get; set; } = new List<Location>();
        public List<ArbitrageOpening> Trades { get; set; } = new List<ArbitrageOpening>();
        public int TotalProfit { get; set; }
        public int RequiredCapital { get; set; }
        public int TotalDistance { get; set; }
        public int AverageProfitMarginBasisPoints { get; set; }
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
            int buyPrice = _priceManager.GetBuyPrice(item, buyLocation);
            if (buyPrice <= 0) continue; // Item not available for purchase here

            foreach (Location sellLocation in locations)
            {
                if (buyLocation == sellLocation) continue; // Skip same location

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation);
                if (sellPrice <= 0) continue; // Can't sell here

                int grossProfit = sellPrice - buyPrice;
                if (grossProfit <= 0) continue; // Not profitable

                // Calculate travel cost
                int distance = CalculateDistance(buyLocation, sellLocation);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = grossProfit - travelCost;

                if (netProfit > highestProfit)
                {
                    highestProfit = netProfit;
                    bestOpening = new ArbitrageOpening
                    {
                        // HIGHLANDER: Object references only
                        Item = item,
                        BuyLocation = buyLocation,
                        BuyPrice = buyPrice,
                        SellLocation = sellLocation,
                        SellPrice = sellPrice,
                        GrossProfit = grossProfit,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMarginBasisPoints = buyPrice > 0 ? netProfit * 10000 / buyPrice : 0,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = netProfit > 0,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? netProfit / distance : netProfit,
                        OpeningDescription = GenerateOpeningDescription(item, buyLocation, sellLocation, netProfit)
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
    /// HIGHLANDER: Accept objects instead of string IDs
    /// </summary>
    public int CalculateProfit(Item item, Location buyLocation, Location sellLocation)
    {
        int buyPrice = _priceManager.GetBuyPrice(item, buyLocation);
        int sellPrice = _priceManager.GetSellPrice(item, sellLocation);

        if (buyPrice <= 0 || sellPrice <= 0) return -1;

        int distance = CalculateDistance(buyLocation, sellLocation);
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
    /// HIGHLANDER: Use Location objects, not string IDs
    /// </summary>
    public List<ArbitrageOpening> FindOpportunitiesFromCurrentLocation()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();
        List<Item> allItems = _itemRepository.GetAllItems();

        foreach (Item item in allItems)
        {
            int buyPrice = _priceManager.GetBuyPrice(item, currentLocation);
            if (buyPrice <= 0) continue;

            List<Location> locations = _gameWorld.Locations;
            foreach (Location sellLocation in locations)
            {
                if (sellLocation == currentLocation) continue;

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation);
                if (sellPrice <= 0) continue;

                int distance = CalculateDistance(currentLocation, sellLocation);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = sellPrice - buyPrice - travelCost;

                if (netProfit > 0)
                {
                    // HIGHLANDER: Object references only
                    opportunities.Add(new ArbitrageOpening
                    {
                        Item = item,
                        BuyLocation = currentLocation,
                        BuyPrice = buyPrice,
                        SellLocation = sellLocation,
                        SellPrice = sellPrice,
                        GrossProfit = sellPrice - buyPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMarginBasisPoints = buyPrice > 0 ? netProfit * 10000 / buyPrice : 0,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? netProfit / distance : netProfit,
                        OpeningDescription = GenerateOpeningDescription(item, currentLocation, sellLocation, netProfit)
                    });
                }
            }
        }

        return opportunities.OrderByDescending(o => o.NetProfit).ToList();
    }

    /// <summary>
    /// Find opportunities for items the player already owns
    /// HIGHLANDER: Use Location objects, not string IDs
    /// </summary>
    public List<ArbitrageOpening> FindOpportunitiesForInventory()
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();

        foreach (Item item in player.Inventory.GetAllItems())
        {
            int currentSellPrice = _priceManager.GetSellPrice(item, currentLocation);
            List<Location> locations = _gameWorld.Locations;

            foreach (Location sellLocation in locations)
            {
                if (sellLocation == currentLocation) continue;

                int otherSellPrice = _priceManager.GetSellPrice(item, sellLocation);
                if (otherSellPrice <= currentSellPrice) continue;

                int distance = CalculateDistance(currentLocation, sellLocation);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = otherSellPrice - currentSellPrice - travelCost;

                if (netProfit > 0)
                {
                    opportunities.Add(new ArbitrageOpening
                    {
                        Item = item,
                        BuyLocation = currentLocation,
                        BuyPrice = currentSellPrice, // What we could sell for here
                        SellLocation = sellLocation,
                        SellPrice = otherSellPrice,
                        GrossProfit = otherSellPrice - currentSellPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMarginBasisPoints = currentSellPrice > 0 ? netProfit * 10000 / currentSellPrice : 0,
                        RequiredCapital = 0, // Already own the item
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? netProfit / distance : netProfit,
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
    /// HIGHLANDER: Track Location objects throughout route planning, not strings
    /// </summary>
    public TradeRoute PlanOptimalRoute(int maxStops = 3)
    {
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        if (currentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        int availableCapital = player.Coins;

        TradeRoute bestRoute = new TradeRoute();
        bestRoute.LocationSequence.Add(currentLocation);

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
            bestRoute.LocationSequence.Add(bestOpp.SellLocation);

            // Update capital and profit
            currentCapital = currentCapital - bestOpp.BuyPrice + bestOpp.SellPrice - bestOpp.TravelCost;
            totalProfit += bestOpp.NetProfit;
            currentLocation = bestOpp.SellLocation;
        }

        bestRoute.TotalProfit = totalProfit;
        bestRoute.RequiredCapital = availableCapital;
        bestRoute.TotalDistance = bestRoute.Trades.Sum(t => t.DistanceBetweenLocations);
        bestRoute.AverageProfitMarginBasisPoints = bestRoute.Trades.Count > 0
            ? (int)bestRoute.Trades.Average(t => t.ProfitMarginBasisPoints)
            : 0;
        bestRoute.RouteDescription = GenerateRouteDescription(bestRoute);

        return bestRoute;
    }

    /// <summary>
    /// Find all opportunities from a specific location
    /// HIGHLANDER: Accept Location object, not string ID
    /// </summary>
    private List<ArbitrageOpening> FindAllOpportunitiesFrom(Location fromLocation)
    {
        List<ArbitrageOpening> opportunities = new List<ArbitrageOpening>();
        List<Item> allItems = _itemRepository.GetAllItems();
        List<Location> locations = _gameWorld.Locations;

        foreach (Item item in allItems)
        {
            int buyPrice = _priceManager.GetBuyPrice(item, fromLocation);
            if (buyPrice <= 0) continue;

            foreach (Location sellLocation in locations)
            {
                if (sellLocation == fromLocation) continue;

                int sellPrice = _priceManager.GetSellPrice(item, sellLocation);
                if (sellPrice <= buyPrice) continue;

                int distance = CalculateDistance(fromLocation, sellLocation);
                int travelCost = CalculateTravelCost(distance);
                int netProfit = sellPrice - buyPrice - travelCost;

                if (netProfit > 0)
                {
                    // HIGHLANDER: Object references only
                    opportunities.Add(new ArbitrageOpening
                    {
                        Item = item,
                        BuyLocation = fromLocation,
                        BuyPrice = buyPrice,
                        SellLocation = sellLocation,
                        SellPrice = sellPrice,
                        GrossProfit = sellPrice - buyPrice,
                        TravelCost = travelCost,
                        NetProfit = netProfit,
                        ProfitMarginBasisPoints = buyPrice > 0 ? netProfit * 10000 / buyPrice : 0,
                        RequiredCapital = buyPrice,
                        IsCurrentlyProfitable = true,
                        DistanceBetweenLocations = distance,
                        ProfitPerDistance = distance > 0 ? netProfit / distance : netProfit
                    });
                }
            }
        }

        return opportunities;
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Calculate distance between two locations using hex coordinates
    /// HIGHLANDER: Accept Location objects, not string IDs
    /// </summary>
    private int CalculateDistance(Location fromLocation, Location toLocation)
    {
        if (fromLocation == toLocation) return 0;

        if (fromLocation == null)
            throw new ArgumentNullException(nameof(fromLocation));

        if (toLocation == null)
            throw new ArgumentNullException(nameof(toLocation));

        // Verify locations have hex positions
        if (fromLocation.HexPosition == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location '{fromLocation.Name}' missing HexPosition");

        if (toLocation.HexPosition == null)
            throw new InvalidOperationException($"[ArbitrageCalculator] Location '{toLocation.Name}' missing HexPosition");

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
    /// HIGHLANDER: Accept objects, extract .Name only for display
    /// </summary>
    private string GenerateOpeningDescription(Item item, Location buyLocation, Location sellLocation, int profit)
    {
        if (profit > 20)
            return $"Excellent opening: Buy {item.Name} in {buyLocation.Name} and sell in {sellLocation.Name} for {profit} coin profit!";
        else if (profit > 10)
            return $"Good trade: {item.Name} from {buyLocation.Name} to {sellLocation.Name} yields {profit} coins";
        else
            return $"Modest profit: {item.Name} trade between {buyLocation.Name} and {sellLocation.Name} for {profit} coins";
    }

    /// <summary>
    /// Generate route description
    /// HIGHLANDER: Extract .Name only for display from object properties
    /// </summary>
    private string GenerateRouteDescription(TradeRoute route)
    {
        if (route.Trades.Count == 0)
            return "No profitable trades found";

        List<string> trades = route.Trades.Select(t =>
            $"{t.Item.Name} ({t.BuyLocation.Name} → {t.SellLocation.Name})"
        ).ToList();

        return $"Trade route with {route.TotalProfit} total profit: " + string.Join(" → ", trades);
    }
}
