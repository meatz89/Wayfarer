using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Unit tests for Dynamic Location-Based Pricing system.
    /// These tests validate the critical location-specific pricing system
    /// that enables arbitrage opportunities and strategic trading gameplay.
    /// 
    /// User Story: Location-Specific Pricing (UserStories.md:173-191)
    /// Priority: HIGH - Core profit mechanism
    /// </summary>
    public class DynamicLocationPricingTests
    {
        /// <summary>
        /// Test that MarketManager correctly initializes location-specific pricing
        /// Acceptance Criteria: Each location has unique item prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Initialize_Location_Specific_Pricing()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act & Assert
            // Verify herbs have different prices at different locations
            int herbsPriceTownSquare = marketManager.GetItemPrice("town_square", "herbs", true);
            int herbsPriceDustyFlagon = marketManager.GetItemPrice("dusty_flagon", "herbs", true);
            
            Assert.NotEqual(herbsPriceTownSquare, herbsPriceDustyFlagon);
            Assert.True(herbsPriceTownSquare > 0);
            Assert.True(herbsPriceDustyFlagon > 0);
        }

        /// <summary>
        /// Test that arbitrage opportunities exist between locations
        /// Acceptance Criteria: Price differences between locations create clear arbitrage opportunities
        /// </summary>
        [Fact]
        public void MarketManager_Should_Create_Arbitrage_Opportunities()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            List<ArbitrageOpportunity> opportunities = marketManager.GetArbitrageOpportunities("town_square", "dusty_flagon");
            
            // Assert
            Assert.NotEmpty(opportunities);
            Assert.True(opportunities.Count > 0);
            
            // Verify at least one profitable opportunity exists
            ArbitrageOpportunity profitableOpportunity = opportunities.FirstOrDefault(o => o.Profit > 0);
            Assert.NotNull(profitableOpportunity);
            Assert.True(profitableOpportunity.SellPrice > profitableOpportunity.BuyPrice);
        }

        /// <summary>
        /// Test that items are available at specific locations with correct pricing
        /// Acceptance Criteria: Each location has 4-6 tradeable goods with unique prices
        /// </summary>
        [Fact]
        public void MarketManager_Should_Return_Available_Items_With_Location_Pricing()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            List<Item> townSquareItems = marketManager.GetAvailableItems("town_square");
            List<Item> dustyFlagonItems = marketManager.GetAvailableItems("dusty_flagon");
            
            // Assert
            Assert.NotEmpty(townSquareItems);
            Assert.NotEmpty(dustyFlagonItems);
            
            // Verify items have location-specific pricing
            Item townSquareHerbs = townSquareItems.FirstOrDefault(i => i.Id == "herbs");
            Item dustyFlagonHerbs = dustyFlagonItems.FirstOrDefault(i => i.Id == "herbs");
            
            if (townSquareHerbs != null && dustyFlagonHerbs != null)
            {
                Assert.NotEqual(townSquareHerbs.BuyPrice, dustyFlagonHerbs.BuyPrice);
            }
        }

        /// <summary>
        /// Test that buy and sell prices are different and create profit margins
        /// Acceptance Criteria: Goods have different buy and sell prices at each location
        /// </summary>
        [Fact]
        public void MarketManager_Should_Have_Different_Buy_And_Sell_Prices()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            int herbsBuyPrice = marketManager.GetItemPrice("town_square", "herbs", true);
            int herbsSellPrice = marketManager.GetItemPrice("town_square", "herbs", false);
            
            // Assert
            Assert.NotEqual(herbsBuyPrice, herbsSellPrice);
            Assert.True(herbsBuyPrice > herbsSellPrice); // Buy price should be higher than sell price
            Assert.True(herbsBuyPrice > 0);
            Assert.True(herbsSellPrice > 0);
        }

        /// <summary>
        /// Test that player can buy items at location-specific prices
        /// Acceptance Criteria: Player can compare prices before making purchases
        /// </summary>
        [Fact]
        public void MarketManager_Should_Allow_Buying_Items_At_Location_Prices()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            Player player = gameWorld.GetPlayer();
            
            // Give player enough money
            player.Coins = 100;
            int initialCoins = player.Coins;
            
            // Act
            bool success = marketManager.BuyItem("herbs", "town_square");
            
            // Assert
            Assert.True(success);
            int expectedPrice = marketManager.GetItemPrice("town_square", "herbs", true);
            Assert.Equal(initialCoins - expectedPrice, player.Coins);
            Assert.True(player.Inventory.HasItem("herbs"));
        }

        /// <summary>
        /// Test that player can sell items at location-specific prices
        /// Acceptance Criteria: Player receives location-specific sell price when selling items
        /// </summary>
        [Fact]
        public void MarketManager_Should_Allow_Selling_Items_At_Location_Prices()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            Player player = gameWorld.GetPlayer();
            
            // Give player item to sell
            player.Inventory.AddItem("herbs");
            int initialCoins = player.Coins;
            
            // Act
            bool success = marketManager.SellItem("herbs", "town_square");
            
            // Assert
            Assert.True(success);
            int expectedPrice = marketManager.GetItemPrice("town_square", "herbs", false);
            Assert.Equal(initialCoins + expectedPrice, player.Coins);
            Assert.False(player.Inventory.HasItem("herbs"));
        }

        /// <summary>
        /// Test that items unavailable at certain locations return correct status
        /// Acceptance Criteria: Some items may not be available at all locations
        /// </summary>
        [Fact]
        public void MarketManager_Should_Handle_Unavailable_Items_At_Locations()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            int price = marketManager.GetItemPrice("dusty_flagon", "rope", true);
            
            // Assert - rope should not be available at dusty_flagon according to implementation plan
            Assert.Equal(-1, price); // -1 indicates item not available
        }

        /// <summary>
        /// Test that profit calculations work correctly for arbitrage opportunities
        /// Acceptance Criteria: Price differentials reward strategic route planning
        /// </summary>
        [Fact]
        public void MarketManager_Should_Calculate_Profit_Margins_Correctly()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            List<ArbitrageOpportunity> opportunities = marketManager.GetArbitrageOpportunities("town_square", "dusty_flagon");
            
            // Assert
            foreach (ArbitrageOpportunity opportunity in opportunities)
            {
                Assert.Equal(opportunity.SellPrice - opportunity.BuyPrice, opportunity.Profit);
                
                float expectedMargin = ((float)(opportunity.SellPrice - opportunity.BuyPrice) / opportunity.BuyPrice) * 100;
                Assert.Equal(expectedMargin, opportunity.ProfitMargin, 1); // 1 decimal precision
            }
        }

        /// <summary>
        /// Test that buying requirements are validated (money and inventory space)
        /// Acceptance Criteria: Player cannot purchase items when lacking money or inventory space
        /// </summary>
        [Fact]
        public void MarketManager_Should_Validate_Purchase_Requirements()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            Player player = gameWorld.GetPlayer();
            
            // Test insufficient money
            player.Coins = 1; // Very low amount
            bool canBuyExpensive = marketManager.CanBuyItem("tools", "town_square");
            Assert.False(canBuyExpensive);
            
            // Test insufficient inventory space
            player.Coins = 100; // Enough money
            // Fill inventory to capacity
            for (int i = 0; i < player.Inventory.Size; i++)
            {
                player.Inventory.AddItem("test_item");
            }
            bool canBuyWhenFull = marketManager.CanBuyItem("herbs", "town_square");
            Assert.False(canBuyWhenFull);
        }

        /// <summary>
        /// Test that arbitrage opportunities are sorted by profitability
        /// Acceptance Criteria: Most profitable opportunities should be listed first
        /// </summary>
        [Fact]
        public void MarketManager_Should_Sort_Arbitrage_By_Profitability()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            LocationSystem locationSystem = CreateTestLocationSystem();
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem);
            
            // Act
            List<ArbitrageOpportunity> opportunities = marketManager.GetArbitrageOpportunities("town_square", "dusty_flagon");
            
            // Assert
            if (opportunities.Count > 1)
            {
                for (int i = 0; i < opportunities.Count - 1; i++)
                {
                    Assert.True(opportunities[i].Profit >= opportunities[i + 1].Profit);
                }
            }
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test game world with standard starting configuration
        /// </summary>
        private static GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            player.Coins = 50; // Starting money for tests
            return gameWorld;
        }

        /// <summary>
        /// Creates a test location system with the required locations
        /// </summary>
        private static LocationSystem CreateTestLocationSystem()
        {
            // Create a mock location repository for testing
            GameWorld gameWorld = new GameWorld();
            LocationRepository locationRepo = new LocationRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepo);
            
            return locationSystem;
        }

        #endregion
    }
}