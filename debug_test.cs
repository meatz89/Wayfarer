using Xunit;
using Wayfarer.Game.MainSystem;

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
        Assert.NotEmpty(allNPCs); // This should pass
        
        // DEBUG: Check NPCs at town_square
        List<NPC> townSquareNPCs = npcRepository.GetNPCsForLocation("town_square");
        Assert.NotEmpty(townSquareNPCs); // This should pass
        
        // DEBUG: Check NPCs available at town_square during Morning
        List<NPC> availableNPCs = npcRepository.GetNPCsForLocationAndTime("town_square", TimeBlocks.Morning);
        Assert.NotEmpty(availableNPCs); // This might fail
        
        // DEBUG: Check if any of them provide Trade service
        List<NPC> tradeNPCs = availableNPCs.Where(npc => npc.CanProvideService(ServiceTypes.Trade)).ToList();
        Assert.NotEmpty(tradeNPCs); // This might fail
        
        // DEBUG: Check what items exist
        List<Item> allItems = itemRepository.GetAllItems();
        Assert.NotEmpty(allItems); // This should pass
        
        // Finally: Check what GetAvailableItems returns
        List<Item> availableItems = marketManager.GetAvailableItems("town_square");
        Assert.NotEmpty(availableItems); // This is failing
    }
}