using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

class MinimalTutorialTest
{
    static void Main()
    {
        Console.WriteLine("=== MINIMAL TUTORIAL TEST ===\n");
        
        try
        {
            // Setup minimal services
            var services = new ServiceCollection();
            services.AddLogging();
            
            // Add configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "useMemory", "false" },
                    { "processStateChanges", "true" },
                    { "DefaultAIProvider", "Ollama" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            
            // Register only the services we need
            services.ConfigureServices();
            
            var provider = services.BuildServiceProvider();
            
            // Get services
            var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
            var narrativeManager = provider.GetRequiredService<NarrativeManager>();
            var flagService = provider.GetRequiredService<FlagService>();
            
            Console.WriteLine("Starting game...");
            gameWorldManager.StartGame().GetAwaiter().GetResult();
            
            // Check tutorial
            Console.WriteLine($"\nTutorial Started: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"Active Narratives: {string.Join(", ", narrativeManager.GetActiveNarratives())}");
            
            if (narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
            {
                Console.WriteLine("\n✓ Tutorial is active!");
                var step = narrativeManager.GetCurrentStep("wayfarer_tutorial");
                if (step != null)
                {
                    Console.WriteLine($"Current Step: {step.Name}");
                    Console.WriteLine($"Guidance: {step.GuidanceText}");
                }
            }
            else
            {
                Console.WriteLine("\n✗ Tutorial is NOT active!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}