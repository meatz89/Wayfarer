using System;
using System.Linq;
using Wayfarer.Game.MainSystem;
using Wayfarer.Tests;

class DebugContractCompletion
{
    static void Main()
    {
        // Create test scenario
        TestScenarioBuilder scenario = new TestScenarioBuilder()
            .WithPlayer(p => p
                .StartAt("town_square")
                .WithCoins(20)
                .WithStamina(10));

        GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
        
        // Get contract
        ContractRepository contracts = new ContractRepository(gameWorld);
        Contract herbContract = contracts.GetContract("herb_delivery");
        
        if (herbContract == null)
        {
            Console.WriteLine("Contract not found!");
            return;
        }
        
        Console.WriteLine($"Contract ID: {herbContract.Id}");
        Console.WriteLine($"Contract Steps: {herbContract.CompletionSteps.Count}");
        
        foreach (var step in herbContract.CompletionSteps)
        {
            Console.WriteLine($"Step: {step.Id} - {step.GetType().Name} - Required: {step.IsRequired} - Completed: {step.IsCompleted}");
        }
        
        // Activate contract
        contracts.AddActiveContract(herbContract);
        
        // Create progression service
        ContractProgressionService progression = new ContractProgressionService(
            contracts, 
            new ItemRepository(gameWorld), 
            new LocationRepository(gameWorld), 
            gameWorld);
        
        // Simulate market transaction
        Player player = gameWorld.GetPlayer();
        Console.WriteLine($"Player location: {player.CurrentLocation?.Id}");
        
        // Check if transaction step completes
        progression.CheckMarketProgression("herbs", "town_square", TransactionType.Sell, 1, 5, player);
        
        Console.WriteLine("After market progression:");
        foreach (var step in herbContract.CompletionSteps)
        {
            Console.WriteLine($"Step: {step.Id} - Completed: {step.IsCompleted}");
        }
        
        Console.WriteLine($"Contract fully completed: {herbContract.IsFullyCompleted()}");
        Console.WriteLine($"Contract status: {herbContract.IsCompleted}");
        
        ContractCompletionResult status = contracts.GetContractStatus("herb_delivery");
        Console.WriteLine($"Status: {status.Status}");
        Console.WriteLine($"Completed steps: {status.CompletedSteps.Count}");
    }
}