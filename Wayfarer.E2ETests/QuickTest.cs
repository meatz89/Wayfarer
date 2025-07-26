using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class QuickTest
{
    public static void Main()
    {
        Console.WriteLine("=== QUICK SERVICE AVAILABILITY TEST ===\n");

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
            
            // Get location actions
            var viewModel = locationActionsService.GetLocationActionsViewModel();
            
            Console.WriteLine("\n=== CHECKING SERVICE AVAILABILITY ===");
            
            // Check for closed services
            bool foundClosedServices = false;
            foreach (var group in viewModel.ActionGroups)
            {
                foreach (var action in group.Actions)
                {
                    if (action.IsServiceClosed)
                    {
                        foundClosedServices = true;
                        Console.WriteLine($"\n🔒 CLOSED SERVICE: {action.Description}");
                        Console.WriteLine($"   Next Available: {action.NextAvailableTime}");
                        if (!string.IsNullOrEmpty(action.ServiceSchedule))
                        {
                            Console.WriteLine($"   Schedule: {action.ServiceSchedule}");
                        }
                    }
                }
            }
            
            if (!foundClosedServices)
            {
                Console.WriteLine("\nNo closed services found at current time.");
            }
            
            // Advance time to see changes
            Console.WriteLine("\n=== ADVANCING TIME TO EVENING ===");
            while (timeManager.GetCurrentTimeBlock() != TimeBlocks.Evening)
            {
                timeManager.AdvanceTime(1);
            }
            
            viewModel = locationActionsService.GetLocationActionsViewModel();
            
            foundClosedServices = false;
            foreach (var group in viewModel.ActionGroups)
            {
                foreach (var action in group.Actions)
                {
                    if (action.IsServiceClosed)
                    {
                        foundClosedServices = true;
                        Console.WriteLine($"\n🔒 CLOSED SERVICE: {action.Description}");
                        Console.WriteLine($"   Next Available: {action.NextAvailableTime}");
                        if (!string.IsNullOrEmpty(action.ServiceSchedule))
                        {
                            Console.WriteLine($"   Schedule: {action.ServiceSchedule}");
                        }
                    }
                }
            }
            
            if (!foundClosedServices)
            {
                Console.WriteLine("\nNo closed services found at Evening.");
            }
            
            Console.WriteLine("\n✅ Service availability feature is working!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}