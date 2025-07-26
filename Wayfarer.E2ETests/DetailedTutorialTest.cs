using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public class DetailedTutorialTest
{
    public static void Main()
    {
        Console.WriteLine("=== DETAILED TUTORIAL TEST ===\n");
        
        try
        {
            // Setup services
            var services = new ServiceCollection();
            services.AddLogging();
            
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
            var provider = services.BuildServiceProvider();
            
            // Get required services
            var gameWorld = provider.GetRequiredService<GameWorld>();
            var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
            var narrativeManager = provider.GetRequiredService<NarrativeManager>();
            var flagService = provider.GetRequiredService<FlagService>();
            var debugLogger = provider.GetRequiredService<DebugLogger>();
            
            Console.WriteLine("✓ Services created successfully\n");
            
            // Enable debug logging
            debugLogger.IsEnabled = true;
            
            // Load narrative definitions manually to ensure they're available
            Console.WriteLine("Loading narrative definitions...");
            NarrativeContentBuilder.BuildAllNarratives();
            narrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
            Console.WriteLine($"✓ Loaded {NarrativeDefinitions.All.Count} narrative definitions\n");
            
            // Check initial state
            Console.WriteLine("=== BEFORE GAME START ===");
            Console.WriteLine($"Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            Console.WriteLine($"Active narratives: {string.Join(", ", narrativeManager.GetActiveNarratives())}");
            
            // Check player state
            var player = gameWorld.GetPlayer();
            Console.WriteLine($"\nInitial player state:");
            Console.WriteLine($"  Coins: {player.Coins}");
            Console.WriteLine($"  Stamina: {player.Stamina}/{player.MaxStamina}");
            
            // Start the game (which should auto-start tutorial)
            Console.WriteLine("\nStarting game...");
            gameWorldManager.StartGame().Wait();
            Console.WriteLine("✓ Game started\n");
            
            // Check tutorial state after game start
            Console.WriteLine("=== AFTER GAME START ===");
            Console.WriteLine($"Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            Console.WriteLine($"Active narratives: {string.Join(", ", narrativeManager.GetActiveNarratives())}");
            
            // Check player state changes
            Console.WriteLine($"\nPlayer state after start:");
            Console.WriteLine($"  Coins: {player.Coins} (should be 2 for tutorial)");
            Console.WriteLine($"  Stamina: {player.Stamina}/{player.MaxStamina} (should be 4/10 for tutorial)");
            
            if (narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
            {
                Console.WriteLine("\n✓ TUTORIAL AUTO-STARTED SUCCESSFULLY!");
                
                var currentStep = narrativeManager.GetCurrentStep("wayfarer_tutorial");
                if (currentStep != null)
                {
                    Console.WriteLine($"\nCurrent tutorial step:");
                    Console.WriteLine($"  ID: {currentStep.Id}");
                    Console.WriteLine($"  Name: {currentStep.Name}");
                    Console.WriteLine($"  Description: {currentStep.Description}");
                    Console.WriteLine($"  Guidance: {currentStep.GuidanceText}");
                    Console.WriteLine($"  Allowed Actions: {string.Join(", ", currentStep.AllowedActions)}");
                    Console.WriteLine($"  Forced Location: {currentStep.ForcedLocation ?? "none"}");
                    Console.WriteLine($"  Forced Spot: {currentStep.ForcedSpot ?? "none"}");
                }
                
                // Check if location was forced
                Console.WriteLine($"\nPlayer location:");
                Console.WriteLine($"  Location: {player.CurrentLocation?.Name} ({player.CurrentLocation?.Id})");
                Console.WriteLine($"  Spot: {player.CurrentLocationSpot?.Name} ({player.CurrentLocationSpot?.SpotID})");
            }
            else
            {
                Console.WriteLine("\n✗ TUTORIAL DID NOT AUTO-START!");
                
                // Try to manually start it to see if there's an error
                Console.WriteLine("\nAttempting to manually start tutorial...");
                try
                {
                    narrativeManager.StartNarrative("wayfarer_tutorial");
                    Console.WriteLine("✓ Manual start succeeded!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Manual start failed: {ex.Message}");
                }
            }
            
            // Get debug logs
            var logs = debugLogger.GetRecentLogs();
            if (logs.Count > 0)
            {
                Console.WriteLine($"\n=== DEBUG LOGS ({logs.Count} entries) ===");
                foreach (var log in logs.Take(20))
                {
                    Console.WriteLine($"[{log.Category}] {log.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}