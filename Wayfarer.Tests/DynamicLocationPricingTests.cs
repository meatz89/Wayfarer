using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Market trading flow tests demonstrating the new superior test pattern.
    /// Tests market trading functionality using repository-based architecture with declarative scenarios.
    /// 
    /// Validates dynamic location-based pricing and trading workflows using:
    /// - TestGameWorldInitializer for clean setup
    /// - TestScenarioBuilder for declarative test data  
    /// - MarketManager query methods for verification
    /// - Direct GameWorld property access where appropriate
    /// </summary>
    public class MarketTradingFlowTests
    {
        /// <summary>
        /// Test that MarketManager initializes with location-specific pricing data
        /// Acceptance Criteria: Each location has 4-6 tradeable goods with unique prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Initialize_Location_Specific_Pricing()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            // === VERIFY USING ENHANCED MARKETMANAGER QUERY METHODS ===
            // Use the new query methods that provide comprehensive market information
            List<MarketPriceInfo> herbPrices = marketManager.GetItemMarketPrices("herbs");
            
            Assert.NotEmpty(herbPrices);
            
            // Find town square and dusty flagon pricing
            MarketPriceInfo townSquareHerbs = herbPrices.FirstOrDefault(p => p.LocationId == "town_square");
            MarketPriceInfo dustyFlagonHerbs = herbPrices.FirstOrDefault(p => p.LocationId == "dusty_flagon");
            
            Assert.NotNull(townSquareHerbs);
            Assert.NotNull(dustyFlagonHerbs);
            
            // Verify pricing structure
            Assert.True(townSquareHerbs.BuyPrice > 0, "Herbs should have a valid buy price at town_square");
            Assert.True(townSquareHerbs.SellPrice > 0, "Herbs should have a valid sell price at town_square");
            Assert.True(townSquareHerbs.BuyPrice > townSquareHerbs.SellPrice, "Buy price should be higher than sell price");
            
            Assert.True(dustyFlagonHerbs.BuyPrice > 0, "Herbs should have a valid buy price at dusty_flagon");
            Assert.True(dustyFlagonHerbs.SellPrice > 0, "Herbs should have a valid sell price at dusty_flagon");
            
            // Verify arbitrage opportunities exist (prices differ between locations)
            bool pricesAreDifferent = (townSquareHerbs.BuyPrice != dustyFlagonHerbs.BuyPrice) || 
                                     (townSquareHerbs.SellPrice != dustyFlagonHerbs.SellPrice);
            Assert.True(pricesAreDifferent, "Prices should differ between locations to create arbitrage opportunities");
        }

        /// <summary>
        /// Test that MarketManager returns available items with location-specific pricing
        /// Acceptance Criteria: Player can see all available items with correct local prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Return_Available_Items_With_Location_Pricing()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            // Act
            List<Item> availableItems = marketManager.GetAvailableItems("town_square");

            // Assert
            Assert.NotEmpty(availableItems);
            Assert.True(availableItems.Count >= 3, "Town square should have at least 3 tradeable items");

            Item herbsItem = availableItems.FirstOrDefault(i => i.Id == "herbs");
            Assert.NotNull(herbsItem);
            Assert.True(herbsItem.BuyPrice > 0, "Herbs should have valid buy price");
            Assert.True(herbsItem.SellPrice > 0, "Herbs should have valid sell price");
            Assert.True(herbsItem.IsAvailable, "Herbs should be available for trading");
        }

        /// <summary>
        /// Test that MarketManager allows discovering profit opportunities through gameplay
        /// Acceptance Criteria: Players can manually check prices between locations to find profitable trades
        /// </summary>
        [Fact]
        public void MarketManager_Should_Allow_Manual_Profit_Discovery()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            // Act - Player manually checks prices between locations (gameplay behavior)
            int herbsBuyPriceTown = marketManager.GetItemPrice("town_square", "herbs", true);
            int herbsSellPriceDusty = marketManager.GetItemPrice("dusty_flagon", "herbs", false);

            // Assert - Player can discover if this is profitable
            Assert.True(herbsBuyPriceTown > 0, "Should be able to buy herbs at town square");
            Assert.True(herbsSellPriceDusty > 0, "Should be able to sell herbs at dusty flagon");

            // Test player's ability to calculate profit manually
            int potentialProfit = herbsSellPriceDusty - herbsBuyPriceTown;

            // Assert meaningful price differences exist for gameplay
            Assert.True(Math.Abs(potentialProfit) > 0, "Price differences should exist to enable trading strategies");
        }

        /// <summary>
        /// Test that MarketManager has different buy and sell prices (spread)
        /// Acceptance Criteria: Goods have different buy and sell prices at each location
        /// </summary>
        [Fact]
        public void MarketManager_Should_Have_Different_Buy_And_Sell_Prices()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            // Act & Assert for multiple items and locations
            string[] locations = { "town_square", "dusty_flagon" };
            string[] items = { "herbs", "tools", "rope", "food" };

            foreach (string location in locations)
            {
                foreach (string item in items)
                {
                    int buyPrice = marketManager.GetItemPrice(location, item, true);
                    int sellPrice = marketManager.GetItemPrice(location, item, false);

                    if (buyPrice > 0 && sellPrice > 0) // Item is available at this location
                    {
                        Assert.True(buyPrice > sellPrice,
                            $"Buy price ({buyPrice}) should be higher than sell price ({sellPrice}) for {item} at {location}");

                        // Verify reasonable spread (buy price should be 10-200% higher than sell price for trading game)
                        double spread = (double)(buyPrice - sellPrice) / sellPrice;
                        Assert.True(spread >= 0.1 && spread <= 2.0,
                            $"Price spread for {item} at {location} should be reasonable (10-200%), got {spread:P}");
                    }
                }
            }
        }

        /// <summary>
        /// Test that MarketManager allows buying items at location-specific prices
        /// Acceptance Criteria: Player can purchase items using location-specific buy prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Allow_Buying_Items_At_Location_Prices()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);
            Player player = gameWorld.GetPlayer();

            // Act
            string itemToBuy = "herbs";
            string location = "town_square";
            int buyPrice = marketManager.GetItemPrice(location, itemToBuy, true);

            bool canBuy = marketManager.CanBuyItem(itemToBuy, location);
            Assert.True(canBuy, "Player should be able to buy herbs at town square");

            int coinsBefore = player.Coins;
            bool purchaseSuccessful = marketManager.BuyItem(itemToBuy, location);

            // Assert
            Assert.True(purchaseSuccessful, "Purchase should be successful");
            Assert.Equal(coinsBefore - buyPrice, player.Coins);
            Assert.True(player.Inventory.HasItem(itemToBuy), "Player should have the purchased item in inventory");
        }

        /// <summary>
        /// Test that MarketManager allows selling items at location-specific prices
        /// Acceptance Criteria: Player can sell items using location-specific sell prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Allow_Selling_Items_At_Location_Prices()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("dusty_flagon")
                    .WithCoins(50)
                    .WithItem("herbs")) // Give player an item to sell
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);
            Player player = gameWorld.GetPlayer();

            // Act
            string itemToSell = "herbs";
            string location = "dusty_flagon";
            int sellPrice = marketManager.GetItemPrice(location, itemToSell, false);

            int coinsBefore = player.Coins;
            bool saleSuccessful = marketManager.SellItem(itemToSell, location);

            // Assert
            Assert.True(saleSuccessful, "Sale should be successful");
            Assert.Equal(coinsBefore + sellPrice, player.Coins);
            Assert.False(player.Inventory.HasItem(itemToSell), "Player should no longer have the sold item");
        }

    }

}