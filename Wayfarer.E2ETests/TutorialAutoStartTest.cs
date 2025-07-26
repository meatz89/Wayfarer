using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public class TutorialAutoStartTest
{
    public static void Main()
    {
        Console.WriteLine("=== TUTORIAL AUTO-START TEST ===\n");
        
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
            
            Console.WriteLine("✓ Services created successfully\n");
            
            // Check initial state
            Console.WriteLine("=== BEFORE GAME START ===");
            Console.WriteLine($"Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            Console.WriteLine($"Active narratives: {string.Join(", ", narrativeManager.GetActiveNarratives())}");
            
            // Start the game (which should auto-start tutorial)
            Console.WriteLine("\nStarting game...");
            gameWorldManager.StartGame().Wait();
            Console.WriteLine("✓ Game started\n");
            
            // Check tutorial state after game start
            Console.WriteLine("=== AFTER GAME START ===");
            Console.WriteLine($"Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            Console.WriteLine($"Active narratives: {string.Join(", ", narrativeManager.GetActiveNarratives())}");
            
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
                }
                
                // Check player state
                var player = gameWorld.GetPlayer();
                Console.WriteLine($"\n=== PLAYER STATE ===");
                Console.WriteLine($"Location: {player.CurrentLocation?.Name} ({player.CurrentLocation?.Id})");
                Console.WriteLine($"Spot: {player.CurrentLocationSpot?.Name} ({player.CurrentLocationSpot?.SpotID})");
                Console.WriteLine($"Stamina: {player.Stamina}/{player.MaxStamina}");
                Console.WriteLine($"Coins: {player.Coins}");
            }
            else
            {
                Console.WriteLine("\n✗ TUTORIAL DID NOT AUTO-START!");
                Console.WriteLine("This is a critical failure - new players won't have guidance.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}