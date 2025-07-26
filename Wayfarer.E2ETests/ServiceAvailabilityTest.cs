using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ServiceAvailabilityTest
{
    public static async Task Main()
    {
        Console.WriteLine("=== SERVICE AVAILABILITY TEST ===\n");

        // Create service collection and configure services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
        // Add IConfiguration (required by GameWorldManager and AIProvider)
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "useMemory", "false" },
                { "processStateChanges", "true" },
                { "DefaultAIProvider", "Ollama" }
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        services.ConfigureServices();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Get required services
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var timeManager = serviceProvider.GetRequiredService<ITimeManager>();
            var locationActionsService = serviceProvider.GetRequiredService<LocationActionsUIService>();
            
            Console.WriteLine($"Current time: {timeManager.GetCurrentTimeBlock()}");
            Console.WriteLine($"Current location: {gameWorld.GetPlayer().CurrentLocationSpot?.Name ?? "Unknown"}");
            Console.WriteLine();
            
            // Get location actions
            var viewModel = locationActionsService.GetLocationActionsViewModel();
            
            Console.WriteLine("=== LOCATION ACTIONS ===");
            foreach (var group in viewModel.ActionGroups)
            {
                Console.WriteLine($"\n{group.ActionType} Actions:");
                foreach (var action in group.Actions)
                {
                    if (action.IsServiceClosed)
                    {
                        Console.WriteLine($"  🔒 {action.Description}");
                        Console.WriteLine($"     {action.NextAvailableTime}");
                        if (!string.IsNullOrEmpty(action.ServiceSchedule))
                        {
                            Console.WriteLine($"     Schedule: {action.ServiceSchedule}");
                        }
                        foreach (var reason in action.UnavailableReasons)
                        {
                            Console.WriteLine($"     Reason: {reason}");
                        }
                    }
                    else if (action.IsAvailable)
                    {
                        Console.WriteLine($"  ✅ {action.Description}");
                        if (!string.IsNullOrEmpty(action.RewardsDescription))
                        {
                            Console.WriteLine($"     Rewards: {action.RewardsDescription}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ {action.Description}");
                        foreach (var reason in action.UnavailableReasons)
                        {
                            Console.WriteLine($"     Reason: {reason}");
                        }
                    }
                }
            }
            
            // Test different times to see availability changes
            Console.WriteLine("\n\n=== TESTING DIFFERENT TIME BLOCKS ===");
            
            var timeBlocks = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
            
            foreach (var timeBlock in timeBlocks)
            {
                // Advance to specific time block
                while (timeManager.GetCurrentTimeBlock() != timeBlock)
                {
                    timeManager.AdvanceTime(1);
                }
                
                Console.WriteLine($"\n--- At {timeBlock} ---");
                
                viewModel = locationActionsService.GetLocationActionsViewModel();
                
                // Check for Letter Board
                var letterBoardClosed = viewModel.ActionGroups
                    .SelectMany(g => g.Actions)
                    .FirstOrDefault(a => a.Id == "letter_board_closed");
                    
                if (letterBoardClosed != null)
                {
                    Console.WriteLine($"Letter Board: CLOSED - {letterBoardClosed.NextAvailableTime}");
                }
                else
                {
                    Console.WriteLine("Letter Board: AVAILABLE");
                }
                
                // Check for Market
                var marketClosed = viewModel.ActionGroups
                    .SelectMany(g => g.Actions)
                    .FirstOrDefault(a => a.Id == "market_closed");
                    
                if (marketClosed != null)
                {
                    Console.WriteLine($"Market: CLOSED - {marketClosed.NextAvailableTime}");
                }
                else
                {
                    var browseMarket = viewModel.ActionGroups
                        .SelectMany(g => g.Actions)
                        .FirstOrDefault(a => a.Description.Contains("Browse market"));
                    
                    if (browseMarket != null)
                    {
                        Console.WriteLine("Market: OPEN");
                    }
                    else
                    {
                        Console.WriteLine("Market: No market at this location");
                    }
                }
                
                // Count available NPCs
                var npcActions = viewModel.ActionGroups
                    .SelectMany(g => g.Actions)
                    .Where(a => !string.IsNullOrEmpty(a.NPCName) && !a.IsServiceClosed)
                    .Select(a => a.NPCName)
                    .Distinct()
                    .Count();
                    
                var missingNPCs = viewModel.ActionGroups
                    .SelectMany(g => g.Actions)
                    .Where(a => a.IsServiceClosed && !string.IsNullOrEmpty(a.NPCName))
                    .ToList();
                
                Console.WriteLine($"Available NPCs: {npcActions}");
                if (missingNPCs.Any())
                {
                    Console.WriteLine($"Missing NPCs: {missingNPCs.Count}");
                    foreach (var npc in missingNPCs.Take(3))
                    {
                        Console.WriteLine($"  - {npc.NPCName}: {npc.NextAvailableTime}");
                    }
                }
            }
            
            Console.WriteLine("\n✅ Service availability test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed with error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}