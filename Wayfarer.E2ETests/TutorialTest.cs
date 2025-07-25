using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

public class TutorialTest
{
    public static void Main()
    {
        Console.WriteLine("=== TUTORIAL SYSTEM TEST ===\n");
        
        try
        {
            // 1. Setup services
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
            
            // 2. Get required services
            var gameWorld = provider.GetRequiredService<GameWorld>();
            var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
            var narrativeManager = provider.GetRequiredService<NarrativeManager>();
            var flagService = provider.GetRequiredService<FlagService>();
            var commandDiscovery = provider.GetRequiredService<CommandDiscoveryService>();
            
            Console.WriteLine("✓ Services created successfully\n");
            
            // 3. Start the game (which should auto-start tutorial)
            Console.WriteLine("Starting game...");
            gameWorldManager.StartGame().Wait();
            Console.WriteLine("✓ Game started\n");
            
            // 4. Check tutorial state
            Console.WriteLine("=== TUTORIAL STATE CHECK ===");
            Console.WriteLine($"Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            
            var activeNarratives = narrativeManager.GetActiveNarratives();
            Console.WriteLine($"Active narratives: {string.Join(", ", activeNarratives)}");
            
            if (narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
            {
                Console.WriteLine("✓ Tutorial is active!");
                
                var currentStep = narrativeManager.GetCurrentStep("wayfarer_tutorial");
                if (currentStep != null)
                {
                    Console.WriteLine($"\nCurrent tutorial step:");
                    Console.WriteLine($"  ID: {currentStep.Id}");
                    Console.WriteLine($"  Name: {currentStep.Name}");
                    Console.WriteLine($"  Description: {currentStep.Description}");
                    Console.WriteLine($"  Guidance: {currentStep.GuidanceText}");
                    Console.WriteLine($"  Allowed Actions: {string.Join(", ", currentStep.AllowedActions)}");
                    Console.WriteLine($"  Visible NPCs: {string.Join(", ", currentStep.VisibleNPCs)}");
                }
            }
            else
            {
                Console.WriteLine("✗ Tutorial is NOT active!");
            }
            
            // 5. Check command filtering
            Console.WriteLine("\n=== COMMAND FILTERING CHECK ===");
            var discoveryResult = commandDiscovery.DiscoverCommands(gameWorld);
            Console.WriteLine($"Total commands discovered: {discoveryResult.AllCommands.Count}");
            
            foreach (var category in discoveryResult.CommandsByCategory)
            {
                Console.WriteLine($"\n{category.Key} commands:");
                foreach (var cmd in category.Value)
                {
                    Console.WriteLine($"  - {cmd.DisplayName}");
                }
            }
            
            // 6. Check player state
            var player = gameWorld.GetPlayer();
            Console.WriteLine($"\n=== PLAYER STATE ===");
            Console.WriteLine($"Location: {player.CurrentLocation?.Name} ({player.CurrentLocation?.Id})");
            Console.WriteLine($"Spot: {player.CurrentLocationSpot?.Name} ({player.CurrentLocationSpot?.SpotID})");
            Console.WriteLine($"Stamina: {player.Stamina}/{player.MaxStamina}");
            Console.WriteLine($"Coins: {player.Coins}");
            
            Console.WriteLine("\n✓ Tutorial test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}