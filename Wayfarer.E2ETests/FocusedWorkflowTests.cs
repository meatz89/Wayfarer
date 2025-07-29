using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Focused tests for specific game workflows that are critical to gameplay.
/// These tests can be run individually for debugging specific features.
/// </summary>
public class FocusedWorkflowTests
{
    private IServiceProvider _provider;
    private IGameFacade _gameFacade;
    
    public async Task RunLetterDeliveryWorkflow()
    {
        Console.WriteLine("=== LETTER DELIVERY WORKFLOW TEST ===\n");
        
        Initialize();
        await _gameFacade.StartGameAsync();
        
        // Skip to Dawn to access letter board
        await AdvanceTimeToBlock(TimeBlocks.Dawn);
        
        // Get letter board
        var letterBoard = _gameFacade.GetLetterBoard();
        if (letterBoard.Offers == null || !letterBoard.Offers.Any())
        {
            Console.WriteLine("✗ No letters available on board");
            return;
        }
        
        // Accept first available letter
        var offer = letterBoard.Offers.First();
        Console.WriteLine($"Accepting letter from {offer.SenderName} to {offer.RecipientName}");
        
        var success = await _gameFacade.AcceptLetterOfferAsync(offer.Id);
        if (!success)
        {
            Console.WriteLine("✗ Failed to accept letter");
            return;
        }
        
        // Check letter queue
        var queue = _gameFacade.GetLetterQueue();
        var letterInQueue = queue.QueueSlots.FirstOrDefault(p => p.Letter != null);
        
        if (letterInQueue == null)
        {
            Console.WriteLine("✗ Letter not in queue after accepting");
            return;
        }
        
        Console.WriteLine($"✓ Letter in position {letterInQueue.Position}");
        
        // If not in position 1, skip it there
        if (letterInQueue.Position != 1)
        {
            success = await _gameFacade.SkipLetterAsync(letterInQueue.Position);
            if (!success)
            {
                Console.WriteLine("✗ Failed to skip letter to position 1");
                return;
            }
            Console.WriteLine("✓ Skipped letter to position 1");
        }
        
        // Travel to destination if needed
        var currentLocation = _gameFacade.GetCurrentLocation().location;
        // Note: We can't directly check destination from offer, would need to find recipient location
        // For now, we'll skip the travel part of this test
        Console.WriteLine("Travel logic would go here");
        
        if (false) // Placeholder
        {
            var destinations = _gameFacade.GetTravelDestinations();
            var targetDest = destinations.FirstOrDefault();
            
            if (targetDest != null && targetDest.CanTravel)
            {
                var routes = _gameFacade.GetRoutesToDestination(targetDest.LocationId);
                var route = routes.FirstOrDefault(r => r.CanTravel);
                
                if (route != null)
                {
                    success = await _gameFacade.TravelToDestinationAsync(targetDest.LocationId, route.RouteId);
                    if (success)
                    {
                        Console.WriteLine($"✓ Traveled to {targetDest.LocationName}");
                    }
                    else
                    {
                        Console.WriteLine("✗ Travel failed");
                        return;
                    }
                }
            }
        }
        
        // Deliver the letter
        queue = _gameFacade.GetLetterQueue();
        letterInQueue = queue.QueueSlots.FirstOrDefault(p => p.Position == 1 && p.Letter != null);
        
        if (letterInQueue != null && letterInQueue.CanDeliver)
        {
            success = await _gameFacade.DeliverLetterAsync(letterInQueue.Letter.Id);
            if (success)
            {
                Console.WriteLine("✓ Letter delivered successfully!");
                
                // Check rewards
                var player = _gameFacade.GetPlayer();
                Console.WriteLine($"Player now has {player.Coins} coins");
            }
            else
            {
                Console.WriteLine("✗ Failed to deliver letter");
            }
        }
        else
        {
            Console.WriteLine("✗ Letter cannot be delivered (wrong location?)");
        }
    }
    
    public async Task RunStaminaCollapseWorkflow()
    {
        Console.WriteLine("=== STAMINA COLLAPSE WORKFLOW TEST ===\n");
        
        Initialize();
        await _gameFacade.StartGameAsync();
        
        var player = _gameFacade.GetPlayer();
        Console.WriteLine($"Starting stamina: {player.Stamina}/{player.MaxStamina}");
        
        // Find actions that cost stamina
        while (player.Stamina > 0)
        {
            var actions = _gameFacade.GetLocationActions();
            var staminaAction = actions.ActionGroups
                ?.SelectMany(g => g.Actions)
                ?.Where(a => a.StaminaCost > 0 && a.IsAvailable)
                ?.OrderByDescending(a => a.StaminaCost)
                ?.FirstOrDefault();
                
            if (staminaAction == null)
            {
                Console.WriteLine("No stamina-consuming actions available");
                break;
            }
            
            Console.WriteLine($"Executing {staminaAction.Description} (costs {staminaAction.StaminaCost} stamina)");
            await _gameFacade.ExecuteLocationActionAsync(staminaAction.Id);
            
            player = _gameFacade.GetPlayer();
            Console.WriteLine($"Stamina now: {player.Stamina}/{player.MaxStamina}");
            
            if (player.Stamina == 0)
            {
                Console.WriteLine("✓ Stamina depleted!");
                
                // Check system messages for collapse notification
                var messages = _gameFacade.GetSystemMessages();
                var collapseMsg = messages.FirstOrDefault(m => 
                    m.Message.Contains("collapse") || 
                    m.Message.Contains("exhausted"));
                    
                if (collapseMsg != null)
                {
                    Console.WriteLine($"✓ Collapse message: {collapseMsg.Message}");
                }
                
                // Check if we were moved to a rest location
                var (location, spot) = _gameFacade.GetCurrentLocation();
                Console.WriteLine($"Current location after collapse: {location.Name} - {spot?.Name}");
                
                // Check if time advanced
                var timeInfo = _gameFacade.GetTimeInfo();
                Console.WriteLine($"Time after collapse: {timeInfo.timeBlock}");
                
                break;
            }
        }
    }
    
    public async Task RunTokenPurgeWorkflow()
    {
        Console.WriteLine("=== TOKEN PURGE WORKFLOW TEST ===\n");
        
        Initialize();
        await _gameFacade.StartGameAsync();
        
        // First, we need to earn some tokens
        Console.WriteLine("Earning tokens by working...");
        
        var actions = _gameFacade.GetLocationActions();
        var workAction = actions.ActionGroups
            ?.SelectMany(g => g.Actions)
            ?.FirstOrDefault(a => a.Id.StartsWith("work_") && a.IsAvailable);
            
        if (workAction != null)
        {
            // Work multiple times to earn tokens
            for (int i = 0; i < 3; i++)
            {
                await _gameFacade.ExecuteLocationActionAsync(workAction.Id);
                Console.WriteLine($"✓ Completed work action {i + 1}");
            }
        }
        
        // Check tokens earned
        var relationships = _gameFacade.GetNPCRelationships();
        var npcWithTokens = relationships.FirstOrDefault(r => 
            r.TokensByType != null && r.TokensByType.Sum(kvp => kvp.Value) > 0);
            
        if (npcWithTokens == null)
        {
            Console.WriteLine("✗ No tokens earned");
            return;
        }
        
        Console.WriteLine($"✓ Earned tokens with {npcWithTokens.NPCName}:");
        foreach (var (tokenType, count) in npcWithTokens.TokensByType)
        {
            Console.WriteLine($"  {tokenType}: {count}");
        }
        
        // Fill letter queue if needed
        await FillLetterQueue();
        
        // Try to purge bottom letter
        var queue = _gameFacade.GetLetterQueue();
        var bottomLetter = queue.QueueSlots
            .Where(p => p.Letter != null)
            .OrderByDescending(p => p.Position)
            .FirstOrDefault();
            
        if (bottomLetter == null)
        {
            Console.WriteLine("✗ No letters in queue to purge");
            return;
        }
        
        Console.WriteLine($"Attempting to purge letter in position {bottomLetter.Position}");
        
        // Create token selection
        var tokenSelection = new Dictionary<string, int>();
        foreach (var (tokenType, count) in npcWithTokens.TokensByType)
        {
            if (count > 0)
            {
                tokenSelection[tokenType.ToString()] = Math.Min(count, 3); // Use up to 3 of each type
            }
        }
        
        var success = await _gameFacade.LetterQueuePurgeAsync(tokenSelection);
        
        if (success)
        {
            Console.WriteLine("✓ Letter purged successfully!");
            
            // Check updated token counts
            relationships = _gameFacade.GetNPCRelationships();
            npcWithTokens = relationships.FirstOrDefault(r => r.NPCId == npcWithTokens.NPCId);
            
            Console.WriteLine("Remaining tokens:");
            foreach (var (tokenType, count) in npcWithTokens.TokensByType)
            {
                Console.WriteLine($"  {tokenType}: {count}");
            }
        }
        else
        {
            Console.WriteLine("✗ Failed to purge letter");
        }
    }
    
    // Helper methods
    
    private void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "useMemory", "true" },
                { "processStateChanges", "true" },
                { "DefaultAIProvider", "Ollama" }
            })
            .Build();
            
        services.AddSingleton<IConfiguration>(configuration);
        services.ConfigureServices();
        
        _provider = services.BuildServiceProvider();
        _gameFacade = _provider.GetRequiredService<IGameFacade>();
    }
    
    private async Task AdvanceTimeToBlock(TimeBlocks targetBlock)
    {
        var timeInfo = _gameFacade.GetTimeInfo();
        
        while (timeInfo.timeBlock != targetBlock)
        {
            // Find and execute a time-passing action
            var actions = _gameFacade.GetLocationActions();
            var timeAction = actions.ActionGroups
                ?.SelectMany(g => g.Actions)
                ?.FirstOrDefault(a => a.TimeCost > 0 && a.IsAvailable);
                
            if (timeAction != null)
            {
                await _gameFacade.ExecuteLocationActionAsync(timeAction.Id);
            }
            else
            {
                // If no actions available, try to advance day
                await _gameFacade.AdvanceToNextDayAsync();
            }
            
            timeInfo = _gameFacade.GetTimeInfo();
        }
        
        Console.WriteLine($"✓ Advanced time to {targetBlock}");
    }
    
    private async Task FillLetterQueue()
    {
        var queue = _gameFacade.GetLetterQueue();
        var emptySlots = queue.Status.MaxCapacity - queue.QueueSlots.Count(p => p.Letter != null);
        
        if (emptySlots == 0)
        {
            Console.WriteLine("Letter queue already full");
            return;
        }
        
        // Try to get to Dawn for letter board
        await AdvanceTimeToBlock(TimeBlocks.Dawn);
        
        var letterBoard = _gameFacade.GetLetterBoard();
        if (letterBoard.Offers != null)
        {
            foreach (var offer in letterBoard.Offers.Take(emptySlots))
            {
                await _gameFacade.AcceptLetterOfferAsync(offer.Id);
                Console.WriteLine($"✓ Accepted letter from {offer.SenderName}");
            }
        }
    }
    
    // Main entry point for running specific workflows
    public static async Task<int> Main(string[] args)
    {
        var tests = new FocusedWorkflowTests();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run -- Program=FocusedWorkflowTests [workflow]");
            Console.WriteLine("\nAvailable workflows:");
            Console.WriteLine("  letter-delivery  - Test complete letter delivery workflow");
            Console.WriteLine("  stamina-collapse - Test stamina depletion and collapse");
            Console.WriteLine("  token-purge      - Test earning tokens and purging letters");
            return 1;
        }
        
        try
        {
            switch (args[0])
            {
                case "letter-delivery":
                    await tests.RunLetterDeliveryWorkflow();
                    break;
                case "stamina-collapse":
                    await tests.RunStaminaCollapseWorkflow();
                    break;
                case "token-purge":
                    await tests.RunTokenPurgeWorkflow();
                    break;
                default:
                    Console.WriteLine($"Unknown workflow: {args[0]}");
                    return 1;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}