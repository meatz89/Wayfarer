using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

/// <summary>
/// Integration test demonstrating the complete contract completion pipeline using the new superior test pattern:
/// Game Start -> Accept Contract -> Travel -> Buy -> Travel -> Sell -> Complete Contract
/// 
/// This test validates the "only check completion actions" principle by showing that
/// players can complete contracts through any path they choose, as long as they
/// perform the required completion action.
/// 
/// NEW ARCHITECTURE BENEFITS:
/// - Uses TestGameWorldInitializer for clean, synchronous setup
/// - Uses TestScenarioBuilder for declarative test data
/// - Uses repository query methods for state inspection 
/// - Direct GameWorld property access where appropriate
/// - Same GameWorldManager flow as production
/// - Zero mocks, zero async complexity, zero test pollution
/// </summary>
public class ContractPipelineIntegrationTest
{
    [Fact]
    public void CompleteContractPipeline_GameStartToCompletion_FollowsOnlyCheckCompletionPrinciple()
    {
        // === DECLARATIVE SETUP ===
        // Clear, readable description of what game state we need
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(20)  // Enough for herbs
                .WithStamina(10))
            .WithTimeState(t => t
                .Day(1)
                .TimeBlock(TimeBlocks.Morning));
        
        // === PRODUCTION-IDENTICAL SETUP ===
        // Same GameWorld type as production, same initialization flow
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // Same repositories as production would use
        ContractRepository contracts = new ContractRepository(gameWorld);
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        MarketManager market = new MarketManager(gameWorld, 
            new LocationSystem(gameWorld, locationRepository),
            new ItemRepository(gameWorld),
            new ContractProgressionService(contracts, new ItemRepository(gameWorld), locationRepository),
            new NPCRepository(gameWorld),
            locationRepository);
        
        // === VERIFY INITIAL STATE ===
        // Using repository query methods (useful for production too)
        Player player = gameWorld.GetPlayer();
        Assert.Equal(20, player.Coins);
        Assert.Equal("dusty_flagon", player.CurrentLocation?.Id);
        Assert.True(contracts.IsContractAvailable("herb_delivery"));
        
        // === ACCEPT CONTRACT ===
        // Direct contract manipulation (simulates GameWorldManager.ExecuteContractAction)
        Contract herbContract = contracts.GetContract("herb_delivery");
        Assert.NotNull(herbContract);
        gameWorld.ActiveContracts.Add(herbContract);
        
        // Verify contract status using repository methods
        ContractCompletionResult contractStatus = contracts.GetContractStatus("herb_delivery");
        Assert.Equal(ContractStatus.Active, contractStatus.Status);
        Assert.Equal(0f, contractStatus.ProgressPercentage);
        
        // === FIRST TRAVEL - TO MARKET ===
        // Player travels to town_square to buy herbs (player choice, not contract requirement)
        
        // Simulate travel (direct property access - GameWorld already provides this)
        Location townSquare = gameWorld.WorldState.locations.First(l => l.Id == "town_square");
        player.CurrentLocation = townSquare;
        gameWorld.WorldState.SetCurrentLocation(townSquare, null);
        
        // Verify travel using direct access
        Assert.Equal("town_square", player.CurrentLocation?.Id);
        
        // Contract should still be incomplete - travel doesn't complete it
        contractStatus = contracts.GetContractStatus("herb_delivery");
        Assert.Equal(ContractStatus.Active, contractStatus.Status);
        Assert.Empty(contractStatus.CompletedTransactions);
        
        // === BUY ITEMS ===
        // Player buys herbs (player choice, not contract requirement)
        
        // Use enhanced MarketManager methods for better testing
        List<MarketPriceInfo> herbPrices = market.GetItemMarketPrices("herbs");
        MarketPriceInfo townSquareHerbs = herbPrices.First(p => p.LocationId == "town_square");
        Assert.True(townSquareHerbs.CanBuy);
        Assert.True(player.Coins >= townSquareHerbs.BuyPrice);
        
        // Execute purchase using enhanced method
        TradeActionResult buyResult = market.TryBuyItem("herbs", "town_square");
        Assert.True(buyResult.Success);
        Assert.True(buyResult.HasItemAfter);
        Assert.False(buyResult.HadItemBefore);
        Assert.True(buyResult.CoinsChanged < 0); // Spent money
        
        // Contract should still be incomplete - buying doesn't complete it
        contractStatus = contracts.GetContractStatus("herb_delivery");
        Assert.Equal(ContractStatus.Active, contractStatus.Status);
        Assert.Empty(contractStatus.CompletedTransactions);
        
        // === SELL ITEMS (COMPLETION ACTION) ===
        // Player sells herbs at town_square - THIS completes the contract
        
        // Execute sale using enhanced method
        TradeActionResult sellResult = market.TrySellItem("herbs", "town_square");
        Assert.True(sellResult.Success);
        Assert.True(sellResult.HadItemBefore);
        Assert.False(sellResult.HasItemAfter);
        Assert.True(sellResult.CoinsChanged > 0); // Earned money
        
        // === VERIFY CONTRACT COMPLETION ===
        // Contract should now be completed
        contractStatus = contracts.GetContractStatus("herb_delivery");
        Assert.Equal(ContractStatus.Completed, contractStatus.Status);
        Assert.Single(contractStatus.CompletedTransactions);
        
        // Verify transaction details
        ContractTransaction completedTransaction = contractStatus.CompletedTransactions.First();
        Assert.Equal("herbs", completedTransaction.ItemId);
        Assert.Equal("town_square", completedTransaction.LocationId);
        Assert.Equal(TransactionType.Sell, completedTransaction.TransactionType);
        
        // === VERIFY "ONLY CHECK COMPLETION" PRINCIPLE ===
        // The contract was completed despite:
        // 1. Player didn't start with herbs (had to buy them)
        // 2. Player took specific actions (buy then sell)
        // 3. Player made choices about where and how to acquire items
        // 
        // The contract ONLY checked the completion action: selling herbs at town_square
        // All other actions were player choices, not contract requirements
        
        // Verify final player state using direct access
        Assert.False(player.Inventory.HasItem("herbs")); // Item was sold
        Assert.True(player.Coins < 20); // Lost money on the trade (buy high, sell low at same location)
        Assert.Equal("town_square", player.CurrentLocation?.Id); // Player at destination
        
        // Note: Contract payment is not automatically applied - that would be handled
        // by a separate system (e.g., visiting contract giver to collect payment)
    }

    [Fact]
    public void ContractPipeline_AlternativePath_StillCompletes()
    {
        // === TEST ALTERNATIVE COMPLETION PATH ===
        // This demonstrates contract flexibility - different paths to same outcome
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("town_square")  // Start at destination
                .WithCoins(100)
                .WithItem("herbs"));      // Already has required item
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        ContractRepository contracts = new ContractRepository(gameWorld);
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        MarketManager market = new MarketManager(gameWorld, 
            new LocationSystem(gameWorld, locationRepository),
            new ItemRepository(gameWorld),
            new ContractProgressionService(contracts, new ItemRepository(gameWorld), locationRepository),
            new NPCRepository(gameWorld),
            locationRepository);
        
        // Accept contract
        Contract herbContract = contracts.GetContract("herb_delivery");
        gameWorld.ActiveContracts.Add(herbContract);
        
        // Verify setup
        Player player = gameWorld.GetPlayer();
        Assert.True(player.Inventory.HasItem("herbs"));
        Assert.Equal("town_square", player.CurrentLocation?.Id);
        
        // Player can complete immediately - no travel or buying needed
        TradeActionResult sellResult = market.TrySellItem("herbs", "town_square");
        Assert.True(sellResult.Success);
        
        // Contract completes regardless of how player got the herbs or location
        ContractCompletionResult contractStatus = contracts.GetContractStatus("herb_delivery");
        Assert.Equal(ContractStatus.Completed, contractStatus.Status);
        
        // This demonstrates the principle: contracts don't care about the process,
        // only the completion action
    }

    [Fact]
    public void ContractPipeline_TravelBasedContract_CompletesOnArrival()
    {
        // === TEST TRAVEL-ONLY COMPLETION CONTRACT ===
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(50));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        ContractRepository contracts = new ContractRepository(gameWorld);
        ContractProgressionService progression = new ContractProgressionService(
            contracts, new ItemRepository(gameWorld), new LocationRepository(gameWorld));
        
        // Accept contract
        Contract explorationContract = contracts.GetContract("mountain_exploration");
        gameWorld.ActiveContracts.Add(explorationContract);
        
        // Verify initial state
        ContractCompletionResult initialStatus = contracts.GetContractStatus("mountain_exploration");
        Assert.Equal(ContractStatus.Active, initialStatus.Status);
        Assert.Empty(initialStatus.CompletedDestinations);
        
        // Travel to destination
        Player player = gameWorld.GetPlayer();
        Location mountainSummit = gameWorld.WorldState.locations.First(l => l.Id == "mountain_summit");
        player.CurrentLocation = mountainSummit;
        gameWorld.WorldState.SetCurrentLocation(mountainSummit, null);
        
        // Check progression - this should complete the contract
        progression.CheckTravelProgression("mountain_summit", player);
        
        // Contract completes immediately upon arrival
        ContractCompletionResult finalStatus = contracts.GetContractStatus("mountain_exploration");
        Assert.Equal(ContractStatus.Completed, finalStatus.Status);
        Assert.Contains("mountain_summit", finalStatus.CompletedDestinations);
        
        // This shows travel-based contracts complete on arrival, regardless of:
        // - How the player got there
        // - What equipment they brought  
        // - What preparation they did
        // Only the arrival matters for completion
    }

    [Fact]
    public void ContractPipeline_MultipleRequirements_TrackProgressIndependently()
    {
        // === TEST CONTRACT WITH MULTIPLE COMPLETION REQUIREMENTS ===
        
        var scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(100)
                .WithItem("rare_materials"));
        
        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        ContractRepository contracts = new ContractRepository(gameWorld);
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        MarketManager market = new MarketManager(gameWorld, 
            new LocationSystem(gameWorld, locationRepository),
            new ItemRepository(gameWorld),
            new ContractProgressionService(contracts, new ItemRepository(gameWorld), locationRepository),
            new NPCRepository(gameWorld),
            locationRepository);
        ContractProgressionService progression = new ContractProgressionService(
            contracts, new ItemRepository(gameWorld), new LocationRepository(gameWorld));
        
        // Accept contract
        Contract complexContract = contracts.GetContract("artisan_masterwork");
        gameWorld.ActiveContracts.Add(complexContract);
        
        // Verify initial state
        ContractCompletionResult initialStatus = contracts.GetContractStatus("artisan_masterwork");
        Assert.Equal(ContractStatus.Active, initialStatus.Status);
        Assert.Empty(initialStatus.CompletedTransactions);
        Assert.Empty(initialStatus.CompletedDestinations);
        
        // === COMPLETE FIRST REQUIREMENT: TRANSACTION ===
        Player player = gameWorld.GetPlayer();
        
        // Travel to workshop
        Location workshop = gameWorld.WorldState.locations.First(l => l.Id == "workshop");
        player.CurrentLocation = workshop;
        gameWorld.WorldState.SetCurrentLocation(workshop, null);
        
        // Sell rare materials
        TradeActionResult sellResult = market.TrySellItem("rare_materials", "workshop");
        Assert.True(sellResult.Success);
        
        // Contract partially complete
        ContractCompletionResult partialStatus = contracts.GetContractStatus("artisan_masterwork");
        Assert.Equal(ContractStatus.Active, partialStatus.Status); // Not fully complete yet
        Assert.Single(partialStatus.CompletedTransactions); // Transaction completed
        Assert.Empty(partialStatus.CompletedDestinations); // Destination not yet completed
        Assert.True(partialStatus.ProgressPercentage > 0f && partialStatus.ProgressPercentage < 1f);
        
        // === COMPLETE SECOND REQUIREMENT: DESTINATION ===
        Location mountainSummit = gameWorld.WorldState.locations.First(l => l.Id == "mountain_summit");
        player.CurrentLocation = mountainSummit;
        gameWorld.WorldState.SetCurrentLocation(mountainSummit, null);
        
        // Check progression
        progression.CheckTravelProgression("mountain_summit", player);
        
        // Contract now fully complete
        ContractCompletionResult finalStatus = contracts.GetContractStatus("artisan_masterwork");
        Assert.Equal(ContractStatus.Completed, finalStatus.Status);
        Assert.Single(finalStatus.CompletedTransactions);
        Assert.Single(finalStatus.CompletedDestinations);
        Assert.Equal(1f, finalStatus.ProgressPercentage);
    }
}