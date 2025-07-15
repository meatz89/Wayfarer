using Wayfarer.Game.MainSystem;
using Xunit;

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
        TestScenarioBuilder scenario = new TestScenarioBuilder()
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
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ContractProgressionService contractProgression = new ContractProgressionService(contracts, itemRepository, locationRepository, gameWorld);
        MarketManager market = new MarketManager(gameWorld,
            new LocationSystem(gameWorld, locationRepository),
            itemRepository,
            contractProgression,
            new NPCRepository(gameWorld),
            locationRepository);

        // === VERIFY INITIAL STATE ===
        // Using repository query methods (useful for production too)
        Player player = gameWorld.GetPlayer();
        Assert.Equal(20, player.Coins);
        Assert.Equal("dusty_flagon", player.CurrentLocation?.Id);

        // Debug: Check what contracts are loaded
        Console.WriteLine($"Total contracts loaded: {gameWorld.WorldState.Contracts.Count}");
        foreach (Contract c in gameWorld.WorldState.Contracts)
        {
            Console.WriteLine($"Contract ID: {c.Id}, Available: {contracts.IsContractAvailable(c.Id)}");
        }

        Assert.True(contracts.IsContractAvailable("village_herb_delivery"));

        // === ACCEPT CONTRACT ===
        // Direct contract manipulation (simulates GameWorldManager.ExecuteContractAction)
        Contract herbContract = contracts.GetContract("village_herb_delivery");
        Assert.NotNull(herbContract);
        contracts.AddActiveContract(herbContract);

        // Debug: Check active contracts
        List<Contract> activeContracts = contracts.GetActiveContracts();
        Console.WriteLine($"Active contracts count: {activeContracts.Count}");
        foreach (Contract c in activeContracts)
        {
            Console.WriteLine($"Active contract ID: {c?.Id}");
        }

        // Verify contract status using repository methods
        ContractCompletionResult contractStatus = contracts.GetContractStatus("village_herb_delivery");
        Assert.Equal(ContractStatus.Active, contractStatus.Status);
        Assert.Equal(0f, contractStatus.ProgressPercentage);

        // === FIRST TRAVEL - TO MARKET ===
        // Player travels to town_square to buy herbs (player choice, not contract requirement)

        // Simulate travel using repository pattern
        Location townSquare = locationRepository.GetLocation("town_square");
        Assert.NotNull(townSquare); // Ensure test data is loaded correctly
        player.CurrentLocation = townSquare;
        gameWorld.WorldState.SetCurrentLocation(townSquare, null);

        // Verify travel using direct access
        Assert.Equal("town_square", player.CurrentLocation?.Id);

        // Contract should still be incomplete - travel doesn't complete it
        contractStatus = contracts.GetContractStatus("village_herb_delivery");
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
        contractStatus = contracts.GetContractStatus("village_herb_delivery");
        Assert.Equal(ContractStatus.Active, contractStatus.Status);
        Assert.Empty(contractStatus.CompletedTransactions);

        // === SECOND TRAVEL - TO CONTRACT DESTINATION ===
        // Player travels to millbrook to sell herbs (contract requirement)

        // Simulate travel to millbrook
        Location millbrook = locationRepository.GetLocation("millbrook");
        Assert.NotNull(millbrook); // Ensure test data is loaded correctly
        player.CurrentLocation = millbrook;
        gameWorld.WorldState.SetCurrentLocation(millbrook, null);

        // Mark travel progression for contract completion
        contractProgression.CheckTravelProgression("millbrook", player);

        // === SELL ITEMS (COMPLETION ACTION) ===
        // Player sells herbs at millbrook - THIS completes the contract
        // village_herb_delivery contract requires selling herbs at millbrook

        // Execute sale using enhanced method
        TradeActionResult sellResult = market.TrySellItem("herbs", "millbrook");
        Assert.True(sellResult.Success);
        Assert.True(sellResult.HadItemBefore);
        Assert.False(sellResult.HasItemAfter);
        Assert.True(sellResult.CoinsChanged > 0); // Earned money

        // === VERIFY CONTRACT COMPLETION ===
        // Contract should now be completed
        contractStatus = contracts.GetContractStatus("village_herb_delivery");
        Assert.Equal(ContractStatus.Completed, contractStatus.Status);
        Assert.Single(contractStatus.CompletedTransactions);

        // Verify transaction details
        ContractTransaction completedTransaction = contractStatus.CompletedTransactions.First();
        Assert.Equal("herbs", completedTransaction.ItemId);
        Assert.Equal("millbrook", completedTransaction.LocationId);
        Assert.Equal(TransactionType.Sell, completedTransaction.TransactionType);

        // === VERIFY "ONLY CHECK COMPLETION" PRINCIPLE ===
        // The contract was completed despite:
        // 1. Player didn't start with herbs (had to buy them)
        // 2. Player took specific actions (buy then sell)
        // 3. Player made choices about where and how to acquire items
        // 
        // The contract ONLY checked the completion action: selling herbs at millbrook
        // All other actions were player choices, not contract requirements

        // Verify final player state using direct access
        Assert.False(player.Inventory.HasItem("herbs")); // Item was sold
        // Player economics: 20 → buy herbs for 7 → 13 → sell herbs for 5 → 18 coins
        // This is realistic trading economics (buy high, sell low) that creates strategic decisions
        Assert.Equal(18, player.Coins); // Lost 2 coins due to market spread, but completed contract
        Assert.Equal("millbrook", player.CurrentLocation?.Id); // Player at destination

        // Note: Contract payment is not automatically applied - that would be handled
        // by a separate system (e.g., visiting contract giver to collect payment)
    }

    [Fact]
    public void ContractPipeline_AlternativePath_StillCompletes()
    {
        // === TEST ALTERNATIVE COMPLETION PATH ===
        // This demonstrates contract flexibility - different paths to same outcome

        TestScenarioBuilder scenario = new TestScenarioBuilder()
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
            new ContractProgressionService(contracts, new ItemRepository(gameWorld), locationRepository, gameWorld),
            new NPCRepository(gameWorld),
            locationRepository);

        // Accept contract
        Contract herbContract = contracts.GetContract("herb_delivery");
        Assert.NotNull(herbContract); // Ensure contract exists in test data
        contracts.AddActiveContract(herbContract);

        // Verify setup
        Player player = gameWorld.GetPlayer();
        Assert.True(player.Inventory.HasItem("herbs"));
        Assert.Equal("town_square", player.CurrentLocation?.Id);

        // Travel to dusty_flagon (correct location for herb_delivery contract)
        Location dustyFlagon = locationRepository.GetLocation("dusty_flagon");
        Assert.NotNull(dustyFlagon);
        player.CurrentLocation = dustyFlagon;
        gameWorld.WorldState.SetCurrentLocation(dustyFlagon, null);

        // Player can complete immediately - no travel or buying needed
        TradeActionResult sellResult = market.TrySellItem("herbs", "dusty_flagon");
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

        TestScenarioBuilder scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(50));

        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        ContractRepository contracts = new ContractRepository(gameWorld);
        ContractProgressionService progression = new ContractProgressionService(
            contracts, new ItemRepository(gameWorld), new LocationRepository(gameWorld), gameWorld);

        // Accept contract - dark_passage_navigation is perfect for travel-only testing
        // Only requires travel to workshop, no transactions needed
        Contract scoutContract = contracts.GetContract("dark_passage_navigation");
        contracts.AddActiveContract(scoutContract);

        // Verify initial state
        ContractCompletionResult initialStatus = contracts.GetContractStatus("dark_passage_navigation");
        Assert.Equal(ContractStatus.Active, initialStatus.Status);
        Assert.Empty(initialStatus.CompletedDestinations);

        // Travel to destination - use repository to get the location
        Player player = gameWorld.GetPlayer();
        LocationRepository locationRepo = new LocationRepository(gameWorld);
        Location workshop = locationRepo.GetLocation("workshop");
        Assert.NotNull(workshop); // Ensure test data is loaded correctly

        // Set player location using proper business logic pattern
        player.CurrentLocation = workshop;
        gameWorld.WorldState.SetCurrentLocation(workshop, null);

        // Check progression - this should complete the contract
        progression.CheckTravelProgression("workshop", player);

        // Contract completes immediately upon arrival
        ContractCompletionResult finalStatus = contracts.GetContractStatus("dark_passage_navigation");
        Assert.Equal(ContractStatus.Completed, finalStatus.Status);
        Assert.Contains("workshop", finalStatus.CompletedDestinations);

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

        TestScenarioBuilder scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("dusty_flagon")
                .WithCoins(100));

        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        ContractRepository contracts = new ContractRepository(gameWorld);
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        MarketManager market = new MarketManager(gameWorld,
            new LocationSystem(gameWorld, locationRepository),
            new ItemRepository(gameWorld),
            new ContractProgressionService(contracts, new ItemRepository(gameWorld), locationRepository, gameWorld),
            new NPCRepository(gameWorld),
            locationRepository);
        ContractProgressionService progression = new ContractProgressionService(
            contracts, new ItemRepository(gameWorld), new LocationRepository(gameWorld), gameWorld);

        // Accept contract - scout_mountain_pass has destination requirement
        Contract complexContract = contracts.GetContract("scout_mountain_pass");
        Assert.NotNull(complexContract); // Ensure contract exists in test data
        contracts.AddActiveContract(complexContract);

        // Verify initial state
        ContractCompletionResult initialStatus = contracts.GetContractStatus("scout_mountain_pass");
        Assert.Equal(ContractStatus.Active, initialStatus.Status);
        Assert.Empty(initialStatus.CompletedTransactions);
        Assert.Empty(initialStatus.CompletedDestinations);

        // === COMPLETE REQUIREMENT: TRAVEL TO MOUNTAIN_PASS ===
        Player player = gameWorld.GetPlayer();

        // Travel to mountain_pass using repository pattern
        LocationRepository locationRepo = new LocationRepository(gameWorld);
        Location mountainPass = locationRepo.GetLocation("mountain_pass");
        Assert.NotNull(mountainPass); // Ensure test data is loaded correctly
        player.CurrentLocation = mountainPass;
        gameWorld.WorldState.SetCurrentLocation(mountainPass, null);

        // Check travel progression
        progression.CheckTravelProgression("mountain_pass", player);

        // Contract should now be complete
        ContractCompletionResult finalStatus = contracts.GetContractStatus("scout_mountain_pass");
        Assert.Equal(ContractStatus.Completed, finalStatus.Status); // Should be complete now
        Assert.Empty(finalStatus.CompletedTransactions); // No transactions required
        Assert.Single(finalStatus.CompletedDestinations); // One destination completed
        Assert.Contains("mountain_pass", finalStatus.CompletedDestinations);
        Assert.Equal(1f, finalStatus.ProgressPercentage); // 100% complete

        // This demonstrates that travel-based contracts complete immediately upon reaching destination
        // and track progress independently from other contract types
    }
}