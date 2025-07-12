using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    public class SimpleMarketManagerTest
    {
        [Fact]
        public void MarketManager_ItemRepository_Returns_Items()
        {
            // Create test world with standard setup
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(100))
                .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            NPCRepository npcRepository = new NPCRepository(gameWorld);
            
            // Verify items exist in repository
            List<Item> repoItems = itemRepository.GetAllItems();
            Assert.True(repoItems.Count > 0, $"ItemRepository should have items, found {repoItems.Count}");
            
            // Create MarketManager with the same repositories
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                contractRepository, itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, 
                contractProgression, npcRepository, locationRepository);
            
            // Debug: Check TimeManager state
            TimeBlocks currentTime = gameWorld.TimeManager.GetCurrentTimeBlock();
            Assert.Equal(TimeBlocks.Morning, currentTime);
            
            // Debug: Check NPC availability manually
            List<NPC> npcsAtLocation = npcRepository.GetNPCsForLocationAndTime("town_square", currentTime);
            Assert.True(npcsAtLocation.Count > 0, $"No NPCs available at town_square during {currentTime}");
            
            List<NPC> tradeNPCs = npcsAtLocation.Where(npc => npc.CanProvideService(ServiceTypes.Trade)).ToList();
            Assert.True(tradeNPCs.Count > 0, $"No trade NPCs available at town_square during {currentTime}");
            
            // Debug: Check market availability status
            string marketStatus = marketManager.GetMarketAvailabilityStatus("town_square");
            Assert.Contains("Open", marketStatus);
            
            // Debug: Check individual item pricing
            Item testItem = repoItems.First();
            int buyPrice = marketManager.GetItemPrice("town_square", testItem.Id, true);
            Assert.True(buyPrice > 0, $"Item {testItem.Id} should have positive buy price, got {buyPrice}");
            
            // Debug: Manually replicate GetAvailableItems logic exactly
            List<Item> manualItems = new List<Item>();
            
            // This should be illegal, but I need to test it for debugging
            var marketType = marketManager.GetType();
            var itemRepoField = marketType.GetField("_itemRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ItemRepository internalRepo = (ItemRepository)itemRepoField.GetValue(marketManager);
            List<Item> internalItems = internalRepo.GetAllItems();
            
            Assert.Equal(repoItems.Count, internalItems.Count);
            
            foreach (Item baseItem in internalItems)
            {
                var getDynamicPricingMethod = marketType.GetMethod("GetDynamicPricing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                dynamic pricing = getDynamicPricingMethod.Invoke(marketManager, new object[] { "town_square", baseItem.Id });
                
                if (pricing.IsAvailable)
                {
                    var createItemMethod = marketType.GetMethod("CreateItemWithLocationPricing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Item createdItem = (Item)createItemMethod.Invoke(marketManager, new object[] { baseItem.Id, "town_square" });
                    manualItems.Add(createdItem);
                }
            }
            
            Assert.True(manualItems.Count > 0, $"Manual replication should find available items, found {manualItems.Count}");
            
            // Call GetAvailableItems
            List<Item> availableItems = marketManager.GetAvailableItems("town_square");
            
            // This should not be empty!
            Assert.True(availableItems.Count > 0, 
                $"GetAvailableItems returned {availableItems.Count} items, manual replication found {manualItems.Count}");
        }
    }
}