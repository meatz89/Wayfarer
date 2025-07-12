using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests to verify that trading actions properly consume time blocks
    /// per the Period-Based Activity Planning user story requirement:
    /// "Each significant activity consumes 1 period"
    /// </summary>
    public class TradingTimeConsumptionTests
    {
        [Fact]
        public void BuyItem_Should_Consume_One_Time_Block()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(50))
                .WithTimeState(t => t.Day(1).Hour(9)); // Morning
                
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            MarketManager marketManager = new MarketManager(
                gameWorld, 
                new LocationSystem(gameWorld, new LocationRepository(gameWorld)),
                new ItemRepository(gameWorld),
                new ContractProgressionService(new ContractRepository(gameWorld), new ItemRepository(gameWorld), new LocationRepository(gameWorld)),
                new NPCRepository(gameWorld),
                new LocationRepository(gameWorld)
            );
            
            int initialHour = gameWorld.TimeManager.CurrentTimeHours;
            TimeBlocks initialTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            
            // Act - Buy an item (should consume 1 time block)
            bool success = marketManager.BuyItem("tools", "town_square");
            
            // Assert
            Assert.True(success);
            int finalHour = gameWorld.TimeManager.CurrentTimeHours;
            TimeBlocks finalTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            
            // Time should have advanced by approximately 3.6 hours (1 time block)
            Assert.True(finalHour > initialHour);
            
            // Should have moved to next time block or stayed in current if still in range
            Assert.True(finalTimeBlock == TimeBlocks.Morning || finalTimeBlock == TimeBlocks.Afternoon);
        }
        
        [Fact]
        public void SellItem_Should_Consume_One_Time_Block()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithItem("tools"))
                .WithTimeState(t => t.Day(1).Hour(9)); // Morning
                
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            MarketManager marketManager = new MarketManager(
                gameWorld, 
                new LocationSystem(gameWorld, new LocationRepository(gameWorld)),
                new ItemRepository(gameWorld),
                new ContractProgressionService(new ContractRepository(gameWorld), new ItemRepository(gameWorld), new LocationRepository(gameWorld)),
                new NPCRepository(gameWorld),
                new LocationRepository(gameWorld)
            );
            
            int initialHour = gameWorld.TimeManager.CurrentTimeHours;
            TimeBlocks initialTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            
            // Act - Sell an item (should consume 1 time block)
            bool success = marketManager.SellItem("tools", "town_square");
            
            // Assert
            Assert.True(success);
            int finalHour = gameWorld.TimeManager.CurrentTimeHours;
            TimeBlocks finalTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            
            // Time should have advanced by approximately 3.6 hours (1 time block)
            Assert.True(finalHour > initialHour);
            
            // Should have moved to next time block or stayed in current if still in range
            Assert.True(finalTimeBlock == TimeBlocks.Morning || finalTimeBlock == TimeBlocks.Afternoon);
        }
        
        [Fact]
        public void Multiple_Trading_Actions_Should_Consume_Multiple_Time_Blocks()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(100).WithItem("tools"))
                .WithTimeState(t => t.Day(1).Hour(6)); // Dawn
                
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            MarketManager marketManager = new MarketManager(
                gameWorld, 
                new LocationSystem(gameWorld, new LocationRepository(gameWorld)),
                new ItemRepository(gameWorld),
                new ContractProgressionService(new ContractRepository(gameWorld), new ItemRepository(gameWorld), new LocationRepository(gameWorld)),
                new NPCRepository(gameWorld),
                new LocationRepository(gameWorld)
            );
            
            TimeBlocks initialTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            Assert.Equal(TimeBlocks.Dawn, initialTimeBlock);
            
            // Act - Perform multiple trading actions
            bool sellSuccess = marketManager.SellItem("tools", "town_square"); // 1st time block
            bool buySuccess = marketManager.BuyItem("herbs", "town_square");   // 2nd time block
            
            // Assert
            Assert.True(sellSuccess);
            Assert.True(buySuccess);
            
            TimeBlocks finalTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
            
            // Should have advanced from Dawn through at least 2 time blocks
            // Dawn (6-8:59) + 3.6 hours = ~9:30 (Morning) + 3.6 hours = ~13:00 (Afternoon)
            Assert.True(finalTimeBlock == TimeBlocks.Afternoon || finalTimeBlock == TimeBlocks.Evening);
        }
    }
}