using Xunit;

public class ArbitrageCalculatorTests
{
    // ========== TEST INFRASTRUCTURE ==========

    private GameWorld CreateGameWorld()
    {
        GameWorld world = new GameWorld();
        world.Locations = new List<Location>();
        world.MarketPriceModifiers = new List<MarketPriceModifier>();
        world.Items = new List<Item>();

        // Initialize hex map with 10x10 grid
        world.WorldHexGrid = new HexMap
        {
            Width = 10,
            Height = 10,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 10; q++)
        {
            for (int r = 0; r < 10; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = TerrainType.Plains,
                    DangerLevel = 0
                };
                world.WorldHexGrid.Hexes.Add(hex);
            }
        }
        world.WorldHexGrid.BuildLookup();

        // Initialize player via GameWorld's GetPlayer() method
        Player player = world.GetPlayer();
        player.Name = "Test Player";
        player.Coins = 100;
        player.Inventory = new Inventory();
        player.CurrentPosition = new AxialCoordinates(0, 0);

        return world;
    }

    private void SetPlayerLocationHex(GameWorld world, Location location)
    {
        // Set hex at player position to reference the location
        Player player = world.GetPlayer();
        Hex playerHex = world.WorldHexGrid.GetHex(player.CurrentPosition);
        if (playerHex != null)
        {
            playerHex.Location = location;
        }
    }

    private PriceManager CreatePriceManager(GameWorld world)
    {
        ItemRepository itemRepo = new ItemRepository(world);
        MarketStateTracker tracker = new MarketStateTracker(world, itemRepo);
        return new PriceManager(world, itemRepo, tracker);
    }

    private ItemRepository CreateItemRepository(GameWorld world)
    {
        return new ItemRepository(world);
    }

    private ArbitrageCalculator CreateCalculator(GameWorld world, PriceManager priceManager, ItemRepository itemRepo)
    {
        return new ArbitrageCalculator(world, priceManager, itemRepo);
    }

    private Location CreateLocation(string name, int q, int r)
    {
        return new Location(name)
        {
            HexPosition = new AxialCoordinates(q, r),
            Capabilities = LocationCapability.Market
        };
    }

    private void SetPrice(GameWorld world, Item item, Location location, int? buyPrice, int? sellPrice)
    {
        MarketPriceModifier modifier = new MarketPriceModifier
        {
            Item = item,
            Location = location,
            BuyPriceOverride = buyPrice,
            SellPriceOverride = sellPrice
        };
        world.MarketPriceModifiers.Add(modifier);
    }

    // ========== FindBestOpening TESTS (12 tests) ==========

    [Fact]
    public void FindBestOpening_NullItem_ThrowsArgumentNullException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => calculator.FindBestOpening(null));
    }

    [Fact]
    public void FindBestOpening_NoLocations_ReturnsNull()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.Locations = new List<Location>(); // Empty
        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestOpening_ItemNotAvailableAnywhere_ReturnsNull()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc1 = CreateLocation("Market", 0, 0);
        world.Locations.Add(loc1);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Set prices to -1 (not available)
        SetPrice(world, item, loc1, -1, -1);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestOpening_SingleProfitableOpportunity_ReturnsIt()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Cheap Market", 0, 0);
        Location sellLoc = CreateLocation("Expensive Market", 5, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy for 10, sell for 30, distance = 5, travel cost = 10, net profit = 10
        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc, 35, 30);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(item, result.Item);
        Assert.Equal(buyLoc, result.BuyLocation);
        Assert.Equal(sellLoc, result.SellLocation);
        Assert.Equal(10, result.BuyPrice);
        Assert.Equal(30, result.SellPrice);
        Assert.Equal(20, result.GrossProfit);
        Assert.Equal(10, result.TravelCost); // 5 distance * 2
        Assert.Equal(10, result.NetProfit);
        Assert.True(result.IsCurrentlyProfitable);
    }

    [Fact]
    public void FindBestOpening_MultipleOpportunities_ReturnsHighestNetProfit()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc1 = CreateLocation("Near City", 2, 0); // Distance 2
        Location sellLoc2 = CreateLocation("Far City", 10, 0); // Distance 10 - need to adjust since our grid is 10x10
        // Actually distance 10 goes beyond, let me use 9
        sellLoc2 = CreateLocation("Far City", 9, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc1);
        world.Locations.Add(sellLoc2);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy for 10
        // Sell1: 25 - 10 - 4 = 11 net profit
        // Sell2: 50 - 10 - 18 = 22 net profit (BEST)
        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc1, 30, 25);
        SetPrice(world, item, sellLoc2, 55, 50);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sellLoc2, result.SellLocation);
        Assert.Equal(22, result.NetProfit);
    }

    [Fact]
    public void FindBestOpening_ConsidersTravelCostInNetProfit()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("Distant City", 7, 0); // Distance 7
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 10, Sell 50, Gross 40, Travel 14 (7 * 2), Net 26
        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc, 55, 50);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40, result.GrossProfit);
        Assert.Equal(14, result.TravelCost);
        Assert.Equal(26, result.NetProfit);
    }

    [Fact]
    public void FindBestOpening_SameLocationBuySell_Excluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc = CreateLocation("Market", 0, 0);
        world.Locations.Add(loc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Even with profitable prices, same location excluded
        SetPrice(world, item, loc, 10, 50);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestOpening_NegativeGrossProfit_Excluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Expensive Market", 0, 0);
        Location sellLoc = CreateLocation("Cheap Market", 5, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Both directions unprofitable:
        // Buy at buyLoc (50), sell at sellLoc (10) = -40 gross profit
        // Buy at sellLoc (60), sell at buyLoc (20) = -40 gross profit
        SetPrice(world, item, buyLoc, 50, 20);
        SetPrice(world, item, sellLoc, 60, 10);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestOpening_ZeroGrossProfit_Excluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market1", 0, 0);
        Location sellLoc = CreateLocation("Market2", 5, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 20, Sell 20 = 0 gross profit
        SetPrice(world, item, buyLoc, 20, 15);
        SetPrice(world, item, sellLoc, 25, 20);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestOpening_DistanceCalculationUsedCorrectly()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Origin", 0, 0);
        Location sellLoc = CreateLocation("Destination", 3, 4); // Distance = 7 in hex grid
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc, 40, 35);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.DistanceBetweenLocations);
        Assert.Equal(14, result.TravelCost); // 7 * 2
    }

    [Fact]
    public void FindBestOpening_ProfitMarginCalculatedCorrectly()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 20, Sell 30, Travel 4, Net 6, Margin = 6/20 = 0.3
        SetPrice(world, item, buyLoc, 20, 15);
        SetPrice(world, item, sellLoc, 35, 30);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.3f, result.ProfitMargin, precision: 2);
    }

    [Fact]
    public void FindBestOpening_OpeningDescriptionGenerated()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = new ItemBuilder().WithName("Silk").Build();
        world.Items.Add(item);

        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc, 40, 35);

        // Act
        ArbitrageCalculator.ArbitrageOpening result = calculator.FindBestOpening(item);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.OpeningDescription);
        Assert.Contains("Silk", result.OpeningDescription);
        Assert.Contains("Market", result.OpeningDescription);
        Assert.Contains("City", result.OpeningDescription);
    }

    // ========== FindAllOpportunities TESTS (5 tests) ==========

    [Fact]
    public void FindAllOpportunities_NoItems_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc1 = CreateLocation("Market1", 0, 0);
        Location loc2 = CreateLocation("Market2", 5, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);
        world.Items = new List<Item>(); // Empty

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAllOpportunities();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindAllOpportunities_NoProfitableOpportunities_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc1 = CreateLocation("Market1", 0, 0);
        Location loc2 = CreateLocation("Market2", 5, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);

        Item item1 = new ItemBuilder().WithName("Item1").Build();
        Item item2 = new ItemBuilder().WithName("Item2").Build();
        world.Items.Add(item1);
        world.Items.Add(item2);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // All prices same = no arbitrage
        SetPrice(world, item1, loc1, 10, 10);
        SetPrice(world, item1, loc2, 10, 10);
        SetPrice(world, item2, loc1, 20, 20);
        SetPrice(world, item2, loc2, 20, 20);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAllOpportunities();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindAllOpportunities_MultipleItemsWithOpportunities_ReturnsAllSortedByNetProfit()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        Item item1 = new ItemBuilder().WithName("Item1").Build();
        Item item2 = new ItemBuilder().WithName("Item2").Build();
        Item item3 = new ItemBuilder().WithName("Item3").Build();
        world.Items.Add(item1);
        world.Items.Add(item2);
        world.Items.Add(item3);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Item1: Net profit 6 (20 - 10 - 4)
        SetPrice(world, item1, buyLoc, 10, 5);
        SetPrice(world, item1, sellLoc, 25, 20);

        // Item2: Net profit 16 (30 - 10 - 4) - HIGHEST
        SetPrice(world, item2, buyLoc, 10, 5);
        SetPrice(world, item2, sellLoc, 35, 30);

        // Item3: Net profit 11 (25 - 10 - 4)
        SetPrice(world, item3, buyLoc, 10, 5);
        SetPrice(world, item3, sellLoc, 30, 25);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAllOpportunities();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(item2, result[0].Item); // Highest profit first
        Assert.Equal(16, result[0].NetProfit);
        Assert.Equal(item3, result[1].Item);
        Assert.Equal(11, result[1].NetProfit);
        Assert.Equal(item1, result[2].Item);
        Assert.Equal(6, result[2].NetProfit);
    }

    [Fact]
    public void FindAllOpportunities_UnprofitableOpportunitiesExcluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("Far City", 9, 0); // Distance 9, cost 18
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        Item profitable = new ItemBuilder().WithName("Profitable").Build();
        Item unprofitable = new ItemBuilder().WithName("Unprofitable").Build();
        world.Items.Add(profitable);
        world.Items.Add(unprofitable);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Profitable: 60 - 10 - 18 = 32 net
        SetPrice(world, profitable, buyLoc, 10, 5);
        SetPrice(world, profitable, sellLoc, 65, 60);

        // Unprofitable: 30 - 10 - 18 = 2 net (but let's make it actually negative)
        SetPrice(world, unprofitable, buyLoc, 20, 15);
        SetPrice(world, unprofitable, sellLoc, 30, 25); // 25 - 20 - 18 = -13 net

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAllOpportunities();

        // Assert
        Assert.Single(result);
        Assert.Equal(profitable, result[0].Item);
    }

    [Fact]
    public void FindAllOpportunities_NullOpeningFromFindBestOpening_Excluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc = CreateLocation("Market", 0, 0);
        world.Locations.Add(loc);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Only one location = no arbitrage possible
        SetPrice(world, item, loc, 10, 5);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAllOpportunities();

        // Assert
        Assert.Empty(result);
    }

    // ========== CalculateProfit TESTS (6 tests) ==========

    [Fact]
    public void CalculateProfit_ValidTrade_ReturnsCorrectProfit()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 3, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 15, Sell 30, Distance 3, Travel 6, Profit = 30 - 15 - 6 = 9
        SetPrice(world, item, buyLoc, 15, 10);
        SetPrice(world, item, sellLoc, 35, 30);

        // Act
        int result = calculator.CalculateProfit(item, buyLoc, sellLoc);

        // Assert
        Assert.Equal(9, result);
    }

    [Fact]
    public void CalculateProfit_ItemNotAvailableForBuy_ReturnsNegativeOne()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 3, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy not available
        SetPrice(world, item, buyLoc, -1, 10);
        SetPrice(world, item, sellLoc, 35, 30);

        // Act
        int result = calculator.CalculateProfit(item, buyLoc, sellLoc);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void CalculateProfit_ItemNotAvailableForSell_ReturnsNegativeOne()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 3, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Sell not available
        SetPrice(world, item, buyLoc, 15, 10);
        SetPrice(world, item, sellLoc, 35, -1);

        // Act
        int result = calculator.CalculateProfit(item, buyLoc, sellLoc);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void CalculateProfit_TravelCostIncludedInCalculation()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("Far City", 9, 0); // Distance 9, cost 18
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 10, Sell 50, Travel 18, Profit = 50 - 10 - 18 = 22
        SetPrice(world, item, buyLoc, 10, 5);
        SetPrice(world, item, sellLoc, 55, 50);

        // Act
        int result = calculator.CalculateProfit(item, buyLoc, sellLoc);

        // Assert
        Assert.Equal(22, result);
    }

    [Fact]
    public void CalculateProfit_SameLocation_ZeroDistanceZeroTravelCost()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location loc = CreateLocation("Market", 0, 0);
        world.Locations.Add(loc);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();

        // Buy 10, Sell 30, Distance 0, Travel 0, Profit = 30 - 10 - 0 = 20
        SetPrice(world, item, loc, 10, 30);

        // Act
        int result = calculator.CalculateProfit(item, loc, loc);

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void CalculateProfit_NullLocations_ReturnsNegativeOne()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);
        Item item = ItemBuilder.Simple();
        Location validLoc = CreateLocation("Market", 0, 0);

        // Act & Assert - Null buyLocation
        int result1 = calculator.CalculateProfit(item, null, validLoc);
        Assert.Equal(-1, result1);

        // Act & Assert - Null sellLocation
        int result2 = calculator.CalculateProfit(item, validLoc, null);
        Assert.Equal(-1, result2);
    }

    // ========== FindAffordableOpportunities TESTS (4 tests) ==========

    [Fact]
    public void FindAffordableOpportunities_PlayerHasEnoughForAll_ReturnsAll()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 1000; // Rich player

        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        Item cheap = new ItemBuilder().WithName("Cheap").Build();
        Item expensive = new ItemBuilder().WithName("Expensive").Build();
        world.Items.Add(cheap);
        world.Items.Add(expensive);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Both affordable
        SetPrice(world, cheap, buyLoc, 10, 5);
        SetPrice(world, cheap, sellLoc, 30, 25);
        SetPrice(world, expensive, buyLoc, 100, 95);
        SetPrice(world, expensive, sellLoc, 130, 125);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAffordableOpportunities();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FindAffordableOpportunities_PlayerCanAffordSome_ReturnsFilteredList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 50; // Limited funds

        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        Item affordable = new ItemBuilder().WithName("Affordable").Build();
        Item tooExpensive = new ItemBuilder().WithName("TooExpensive").Build();
        world.Items.Add(affordable);
        world.Items.Add(tooExpensive);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Only affordable item should be returned
        SetPrice(world, affordable, buyLoc, 30, 25);
        SetPrice(world, affordable, sellLoc, 50, 45);
        SetPrice(world, tooExpensive, buyLoc, 100, 95);
        SetPrice(world, tooExpensive, sellLoc, 130, 125);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAffordableOpportunities();

        // Assert
        Assert.Single(result);
        Assert.Equal(affordable, result[0].Item);
    }

    [Fact]
    public void FindAffordableOpportunities_PlayerCanAffordNothing_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 5; // Very poor

        Location buyLoc = CreateLocation("Market", 0, 0);
        Location sellLoc = CreateLocation("City", 2, 0);
        world.Locations.Add(buyLoc);
        world.Locations.Add(sellLoc);

        Item expensive = new ItemBuilder().WithName("Expensive").Build();
        world.Items.Add(expensive);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        SetPrice(world, expensive, buyLoc, 100, 95);
        SetPrice(world, expensive, sellLoc, 130, 125);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAffordableOpportunities();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindAffordableOpportunities_NoOpportunities_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 1000;

        Location loc = CreateLocation("Market", 0, 0);
        world.Locations.Add(loc);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Only one location = no opportunities
        SetPrice(world, item, loc, 10, 5);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindAffordableOpportunities();

        // Assert
        Assert.Empty(result);
    }

    // ========== FindOpportunitiesFromCurrentLocation TESTS (6 tests) ==========

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_NoCurrentLocation_ThrowsInvalidOperationException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        // Player at (0,0) but no hex has location set

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => calculator.FindOpportunitiesFromCurrentLocation());
    }

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_NoItems_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        world.Locations.Add(currentLoc);
        SetPlayerLocationHex(world, currentLoc);
        world.Items = new List<Item>(); // Empty

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesFromCurrentLocation();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_ItemsAvailableButNoProfitableSells_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location otherLoc = CreateLocation("Other", 5, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(otherLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Buy here for 10, sell elsewhere for 10 (after travel cost, unprofitable)
        SetPrice(world, item, currentLoc, 10, 5);
        SetPrice(world, item, otherLoc, 15, 10); // 10 - 10 - 10 = -10 net

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesFromCurrentLocation();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_MultipleProfitableOpportunities_ReturnsAllSortedByNetProfit()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location nearCity = CreateLocation("Near", 2, 0);
        Location farCity = CreateLocation("Far", 5, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(nearCity);
        world.Locations.Add(farCity);
        SetPlayerLocationHex(world, currentLoc);

        Item item1 = new ItemBuilder().WithName("Item1").Build();
        Item item2 = new ItemBuilder().WithName("Item2").Build();
        world.Items.Add(item1);
        world.Items.Add(item2);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Item1 to Near: 25 - 10 - 4 = 11 net
        SetPrice(world, item1, currentLoc, 10, 5);
        SetPrice(world, item1, nearCity, 30, 25);

        // Item2 to Far: 40 - 10 - 10 = 20 net (BEST)
        SetPrice(world, item2, currentLoc, 10, 5);
        SetPrice(world, item2, farCity, 45, 40);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesFromCurrentLocation();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(item2, result[0].Item); // Highest first
        Assert.Equal(20, result[0].NetProfit);
        Assert.Equal(item1, result[1].Item);
        Assert.Equal(11, result[1].NetProfit);
    }

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_SameLocationExcluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        world.Locations.Add(currentLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Only current location available
        SetPrice(world, item, currentLoc, 10, 50);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesFromCurrentLocation();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOpportunitiesFromCurrentLocation_UnprofitableOpportunitiesExcluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location profitableLoc = CreateLocation("Profitable", 2, 0);
        Location unprofitableLoc = CreateLocation("Unprofitable", 9, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(profitableLoc);
        world.Locations.Add(unprofitableLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item profitable = new ItemBuilder().WithName("Profitable").Build();
        Item unprofitable = new ItemBuilder().WithName("Unprofitable").Build();
        world.Items.Add(profitable);
        world.Items.Add(unprofitable);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Profitable: 30 - 10 - 4 = 16 net
        SetPrice(world, profitable, currentLoc, 10, 5);
        SetPrice(world, profitable, profitableLoc, 35, 30);

        // Unprofitable: 25 - 20 - 18 = -13 net
        SetPrice(world, unprofitable, currentLoc, 20, 15);
        SetPrice(world, unprofitable, unprofitableLoc, 30, 25);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesFromCurrentLocation();

        // Assert
        Assert.Single(result);
        Assert.Equal(profitable, result[0].Item);
    }

    // ========== FindOpportunitiesForInventory TESTS (5 tests) ==========

    [Fact]
    public void FindOpportunitiesForInventory_NoCurrentLocation_ThrowsInvalidOperationException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        // Player at (0,0) but no hex has location set

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => calculator.FindOpportunitiesForInventory());
    }

    [Fact]
    public void FindOpportunitiesForInventory_EmptyInventory_ReturnsEmptyList()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        world.Locations.Add(currentLoc);
        SetPlayerLocationHex(world, currentLoc);

        // Empty inventory
        world.GetPlayer().Inventory = new Inventory();

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesForInventory();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOpportunitiesForInventory_ItemsWithBetterSellPricesElsewhere_Included()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location betterLoc = CreateLocation("Better", 3, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(betterLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item inventoryItem = new ItemBuilder().WithName("InventoryItem").Build();
        world.GetPlayer().Inventory.AddItems(inventoryItem, 1);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Current sell: 10, Better sell: 30, Travel: 6, Net: 14
        SetPrice(world, inventoryItem, currentLoc, 15, 10);
        SetPrice(world, inventoryItem, betterLoc, 35, 30);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesForInventory();

        // Assert
        Assert.Single(result);
        Assert.Equal(inventoryItem, result[0].Item);
        Assert.Equal(currentLoc, result[0].BuyLocation);
        Assert.Equal(betterLoc, result[0].SellLocation);
        Assert.Equal(14, result[0].NetProfit);
    }

    [Fact]
    public void FindOpportunitiesForInventory_ItemsWithWorseSellPricesElsewhere_Excluded()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location worseLoc = CreateLocation("Worse", 3, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(worseLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item inventoryItem = new ItemBuilder().WithName("InventoryItem").Build();
        world.GetPlayer().Inventory.AddItems(inventoryItem, 1);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Current sell: 30, Worse sell: 20
        SetPrice(world, inventoryItem, currentLoc, 35, 30);
        SetPrice(world, inventoryItem, worseLoc, 25, 20);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesForInventory();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindOpportunitiesForInventory_RequiredCapitalIsZero_AlreadyOwnItem()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        Location currentLoc = CreateLocation("Current", 0, 0);
        Location betterLoc = CreateLocation("Better", 2, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(betterLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item inventoryItem = new ItemBuilder().WithName("InventoryItem").Build();
        world.GetPlayer().Inventory.AddItems(inventoryItem, 1);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        SetPrice(world, inventoryItem, currentLoc, 15, 10);
        SetPrice(world, inventoryItem, betterLoc, 30, 25);

        // Act
        List<ArbitrageCalculator.ArbitrageOpening> result = calculator.FindOpportunitiesForInventory();

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].RequiredCapital); // Already own it
    }

    // ========== PlanOptimalRoute TESTS (7 tests) ==========

    [Fact]
    public void PlanOptimalRoute_NoCurrentLocation_ThrowsInvalidOperationException()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        // Player at (0,0) but no hex has location set

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => calculator.PlanOptimalRoute());
    }

    [Fact]
    public void PlanOptimalRoute_NoAffordableOpportunities_ReturnsRouteWithZeroTrades()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 5; // Too poor

        Location currentLoc = CreateLocation("Current", 0, 0);
        Location otherLoc = CreateLocation("Other", 5, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(otherLoc);
        SetPlayerLocationHex(world, currentLoc);

        Item expensive = new ItemBuilder().WithName("Expensive").Build();
        world.Items.Add(expensive);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        SetPrice(world, expensive, currentLoc, 100, 95);
        SetPrice(world, expensive, otherLoc, 130, 125);

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute();

        // Assert
        Assert.Empty(result.Trades);
        Assert.Equal(0, result.TotalProfit);
        Assert.Single(result.LocationSequence); // Just current location
        Assert.Equal(currentLoc, result.LocationSequence[0]);
    }

    [Fact]
    public void PlanOptimalRoute_SingleStopRoute_ReturnsBestOpportunity()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 100;

        Location currentLoc = CreateLocation("Current", 0, 0);
        Location destination = CreateLocation("Destination", 2, 0);
        world.Locations.Add(currentLoc);
        world.Locations.Add(destination);
        SetPlayerLocationHex(world, currentLoc);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Profitable trade
        SetPrice(world, item, currentLoc, 20, 15);
        SetPrice(world, item, destination, 40, 35);

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute(maxStops: 1);

        // Assert
        Assert.Single(result.Trades);
        Assert.Equal(item, result.Trades[0].Item);
        Assert.Equal(currentLoc, result.Trades[0].BuyLocation);
        Assert.Equal(destination, result.Trades[0].SellLocation);
        Assert.Equal(2, result.LocationSequence.Count);
        Assert.Equal(destination, result.LocationSequence[1]);
    }

    [Fact]
    public void PlanOptimalRoute_MultiStopRoute_ChainsOpportunities()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 200;

        Location loc1 = CreateLocation("Location1", 0, 0);
        Location loc2 = CreateLocation("Location2", 2, 0);
        Location loc3 = CreateLocation("Location3", 4, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);
        world.Locations.Add(loc3);
        SetPlayerLocationHex(world, loc1);

        Item item1 = new ItemBuilder().WithName("Item1").WithBuyPrice(0).WithSellPrice(0).Build();
        Item item2 = new ItemBuilder().WithName("Item2").WithBuyPrice(0).WithSellPrice(0).Build();
        world.Items.Add(item1);
        world.Items.Add(item2);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Trade 1: loc1 -> loc2
        SetPrice(world, item1, loc1, 20, 15);
        SetPrice(world, item1, loc2, 40, 35);

        // Trade 2: loc2 -> loc3
        SetPrice(world, item2, loc2, 30, 25);
        SetPrice(world, item2, loc3, 55, 50);

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute(maxStops: 2);

        // Assert
        Assert.Equal(2, result.Trades.Count);
        Assert.Equal(3, result.LocationSequence.Count);
        Assert.Equal(loc1.Name, result.LocationSequence[0].Name);
        Assert.Equal(loc2.Name, result.LocationSequence[1].Name);
        Assert.Equal(loc3.Name, result.LocationSequence[2].Name);
    }

    [Fact]
    public void PlanOptimalRoute_MaxStopsParameter_LimitsRouteLength()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 1000; // Rich

        Location loc1 = CreateLocation("Location1", 0, 0);
        Location loc2 = CreateLocation("Location2", 1, 0);
        Location loc3 = CreateLocation("Location3", 2, 0);
        Location loc4 = CreateLocation("Location4", 3, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);
        world.Locations.Add(loc3);
        world.Locations.Add(loc4);
        SetPlayerLocationHex(world, loc1);

        Item item = ItemBuilder.Simple();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Many profitable trades available
        SetPrice(world, item, loc1, 10, 5);
        SetPrice(world, item, loc2, 30, 25);
        SetPrice(world, item, loc3, 50, 45);
        SetPrice(world, item, loc4, 70, 65);

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute(maxStops: 2);

        // Assert
        Assert.True(result.Trades.Count <= 2); // Respects maxStops
    }

    [Fact]
    public void PlanOptimalRoute_CapitalUpdatedCorrectlyAfterEachTrade()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 50; // Limited starting capital

        Location loc1 = CreateLocation("Location1", 0, 0);
        Location loc2 = CreateLocation("Location2", 2, 0);
        Location loc3 = CreateLocation("Location3", 4, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);
        world.Locations.Add(loc3);
        SetPlayerLocationHex(world, loc1);

        Item cheap = new ItemBuilder().WithName("Cheap").WithBuyPrice(0).WithSellPrice(0).Build();
        Item expensive = new ItemBuilder().WithName("Expensive").WithBuyPrice(0).WithSellPrice(0).Build();
        world.Items.Add(cheap);
        world.Items.Add(expensive);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        // Trade 1: Affordable initially (20 cost)
        SetPrice(world, cheap, loc1, 20, 15);
        SetPrice(world, cheap, loc2, 40, 35); // Profit 11

        // Trade 2: Only affordable after trade 1 completes (need 40)
        SetPrice(world, expensive, loc2, 40, 35);
        SetPrice(world, expensive, loc3, 70, 65); // Would need capital from trade 1

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute(maxStops: 2);

        // Assert
        // Should execute trade 1, gain capital, potentially execute trade 2
        Assert.NotEmpty(result.Trades);
        Assert.Equal(cheap.Name, result.Trades[0].Item.Name);
    }

    [Fact]
    public void PlanOptimalRoute_RouteDescriptionGenerated()
    {
        // Arrange
        GameWorld world = CreateGameWorld();
        world.GetPlayer().Coins = 100;

        Location loc1 = CreateLocation("Market", 0, 0);
        Location loc2 = CreateLocation("City", 2, 0);
        world.Locations.Add(loc1);
        world.Locations.Add(loc2);
        SetPlayerLocationHex(world, loc1);

        Item item = new ItemBuilder().WithName("Silk").Build();
        world.Items.Add(item);

        PriceManager priceManager = CreatePriceManager(world);
        ItemRepository itemRepo = CreateItemRepository(world);
        ArbitrageCalculator calculator = CreateCalculator(world, priceManager, itemRepo);

        SetPrice(world, item, loc1, 20, 15);
        SetPrice(world, item, loc2, 40, 35);

        // Act
        ArbitrageCalculator.TradeRoute result = calculator.PlanOptimalRoute();

        // Assert
        Assert.NotNull(result.RouteDescription);
        Assert.NotEmpty(result.RouteDescription);
    }
}
