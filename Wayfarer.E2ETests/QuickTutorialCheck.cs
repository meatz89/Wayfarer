using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QuickTutorialCheck
{
    public static async Task Main()
    {
        Console.WriteLine("=== QUICK TUTORIAL CHECK ===\n");
        
        try
        {
            // Minimal setup
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
            
            var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
            var narrativeManager = provider.GetRequiredService<NarrativeManager>();
            var flagService = provider.GetRequiredService<FlagService>();
            
            Console.WriteLine("Starting game...");
            await gameWorldManager.StartGame();
            
            // Quick check
            bool tutorialActive = narrativeManager.IsNarrativeActive("wayfarer_tutorial");
            bool tutorialStarted = flagService.HasFlag(FlagService.TUTORIAL_STARTED);
            
            Console.WriteLine($"\nTutorial Active: {tutorialActive}");
            Console.WriteLine($"Tutorial Started Flag: {tutorialStarted}");
            
            if (tutorialActive)
            {
                var step = narrativeManager.GetCurrentStep("wayfarer_tutorial");
                Console.WriteLine($"Current Step: {step?.Name ?? "none"}");
                Console.WriteLine("\n✓ TUTORIAL IS WORKING!");
            }
            else
            {
                Console.WriteLine("\n✗ TUTORIAL NOT ACTIVE!");
                
                // Check why
                bool tutorialComplete = flagService.HasFlag(FlagService.TUTORIAL_COMPLETE);
                Console.WriteLine($"Tutorial Complete Flag: {tutorialComplete}");
                
                if (!tutorialComplete)
                {
                    Console.WriteLine("\nThe tutorial should have started but didn't.");
                    Console.WriteLine("This means InitializeTutorialIfNeeded is not working correctly.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
        }
    }
}