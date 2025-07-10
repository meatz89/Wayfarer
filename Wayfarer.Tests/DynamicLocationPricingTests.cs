using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Test-driven development for Dynamic Location-Based Pricing System.
    /// These tests validate the core economic simulation mechanic where items have
    /// different buy/sell prices at different locations, creating arbitrage opportunities.
    /// 
    /// User Story: Location-Specific Pricing (:172-195)
    /// Priority: CRITICAL - Core profit mechanism for economic simulation
    /// </summary>
    public class DynamicLocationPricingTests
    {
        /// <summary>
        /// Test that MarketManager initializes with location-specific pricing data
        /// Acceptance Criteria: Each location has 4-6 tradeable goods with unique prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Initialize_Location_Specific_Pricing()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

            // Act & Assert - Test Town Square pricing
            int herbsBuyPrice = marketManager.GetItemPrice("town_square", "herbs", true);
            int herbsSellPrice = marketManager.GetItemPrice("town_square", "herbs", false);

            Assert.True(herbsBuyPrice > 0, "Herbs should have a valid buy price at town_square");
            Assert.True(herbsSellPrice > 0, "Herbs should have a valid sell price at town_square");
            Assert.True(herbsBuyPrice > herbsSellPrice, "Buy price should be higher than sell price");

            // Act & Assert - Test Dusty Flagon pricing (should be different)
            int herbsBuyPriceDusty = marketManager.GetItemPrice("dusty_flagon", "herbs", true);
            int herbsSellPriceDusty = marketManager.GetItemPrice("dusty_flagon", "herbs", false);

            Assert.True(herbsBuyPriceDusty > 0, "Herbs should have a valid buy price at dusty_flagon");
            Assert.True(herbsSellPriceDusty > 0, "Herbs should have a valid sell price at dusty_flagon");

            // Prices should be different between locations (this is what creates arbitrage)
            bool pricesAreDifferent = (herbsBuyPrice != herbsBuyPriceDusty) || (herbsSellPrice != herbsSellPriceDusty);
            Assert.True(pricesAreDifferent, "Prices should differ between locations to create arbitrage opportunities");
        }

        /// <summary>
        /// Test that MarketManager returns available items with location-specific pricing
        /// Acceptance Criteria: Player can see all available items with correct local prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Return_Available_Items_With_Location_Pricing()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

            // Act
            List<Item> availableItems = marketManager.GetAvailableItems("town_square");

            // Assert
            Assert.NotEmpty(availableItems);
            Assert.True(availableItems.Count >= 3, "Town square should have at least 3 tradeable items");

            Item herbsItem = availableItems.FirstOrDefault(i => i.Name == "herbs");
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
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

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
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

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
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            Player player = gameWorld.GetPlayer();
            player.Coins = 100; // Give player enough money

            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

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
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            Player player = gameWorld.GetPlayer();
            player.Coins = 50;
            player.Inventory.AddItem("herbs"); // Give player an item to sell

            LocationSystem locationSystem = CreateTestLocationSystem();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository);

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

        #region Helper Methods

        /// <summary>
        /// Creates a test game world with basic configuration and test items
        /// </summary>
        private static GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);

            // Set up basic world state
            gameWorld.WorldState.CurrentDay = 1;
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;

            // Add test items to world state
            gameWorld.WorldState.Items = new List<Item>
            {
                new Item { Id = "herbs", Name = "herbs", BuyPrice = 5, SellPrice = 3, Weight = 1, Description = "Common herbs" },
                new Item { Id = "tools", Name = "tools", BuyPrice = 15, SellPrice = 12, Weight = 3, Description = "Basic tools" },
                new Item { Id = "rope", Name = "rope", BuyPrice = 8, SellPrice = 6, Weight = 2, Description = "Strong rope" },
                new Item { Id = "food", Name = "food", BuyPrice = 4, SellPrice = 2, Weight = 1, Description = "Fresh provisions" }
            };

            return gameWorld;
        }

        /// <summary>
        /// Creates a test location system with required locations
        /// </summary>
        private static LocationSystem CreateTestLocationSystem()
        {
            GameWorld gameWorld = new GameWorld();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            return new LocationSystem(gameWorld, locationRepository);
        }

        #endregion
    }

}