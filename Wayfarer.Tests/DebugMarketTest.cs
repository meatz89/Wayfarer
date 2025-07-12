using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    public class DebugMarketTest
    {
        [Fact]
        public void Debug_MarketManager_NPCs()
        {
            // === SETUP ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            NPCRepository npcRepository = new NPCRepository(gameWorld);
            
            // Create services
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression, npcRepository, locationRepository);

            // DEBUG: Check what NPCs exist
            List<NPC> allNPCs = npcRepository.GetAllNPCs();
            Assert.True(allNPCs.Count > 0, $"Expected NPCs, found {allNPCs.Count}");
            
            // DEBUG: Check NPCs at town_square
            List<NPC> townSquareNPCs = npcRepository.GetNPCsForLocation("town_square");
            Assert.True(townSquareNPCs.Count > 0, $"Expected NPCs at town_square, found {townSquareNPCs.Count}");
            
            // DEBUG: Check NPCs available at town_square during Morning
            List<NPC> availableNPCs = npcRepository.GetNPCsForLocationAndTime("town_square", TimeBlocks.Morning);
            Assert.True(availableNPCs.Count > 0, $"Expected available NPCs at town_square during Morning, found {availableNPCs.Count}");
            
            // DEBUG: Check if any of them provide Trade service
            List<NPC> tradeNPCs = availableNPCs.Where(npc => npc.CanProvideService(ServiceTypes.Trade)).ToList();
            Assert.True(tradeNPCs.Count > 0, $"Expected trade NPCs available at town_square during Morning, found {tradeNPCs.Count}");
            
            // DEBUG: Check what items exist through different repositories
            List<Item> testItems = itemRepository.GetAllItems();
            Assert.True(testItems.Count > 0, $"Test ItemRepository returned {testItems.Count} items");
            
            // Create MarketManager's own ItemRepository to see if there's a difference
            ItemRepository marketItemRepo = new ItemRepository(gameWorld);
            List<Item> marketItems = marketItemRepo.GetAllItems();
            Assert.True(marketItems.Count > 0, $"Market ItemRepository returned {marketItems.Count} items");
            Assert.Equal(testItems.Count, marketItems.Count);
            
            // Check if MarketManager's internal ItemRepository returns the same items
            var marketType = marketManager.GetType();
            var itemRepoField = marketType.GetField("_itemRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ItemRepository internalItemRepo = (ItemRepository)itemRepoField.GetValue(marketManager);
            List<Item> internalItems = internalItemRepo.GetAllItems();
            Assert.True(internalItems.Count > 0, $"MarketManager's internal ItemRepository returned {internalItems.Count} items");
            
            // Finally: Check what GetAvailableItems returns
            List<Item> availableItems = marketManager.GetAvailableItems("town_square");
            Assert.True(availableItems.Count > 0, $"Expected available items at town_square, found {availableItems.Count}. Internal repo has {internalItems.Count} items.");
        }
    }
}