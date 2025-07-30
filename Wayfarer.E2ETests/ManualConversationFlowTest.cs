using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Manual test that simulates the exact flow when clicking "Talk with Tam" button
/// </summary>
public class ManualConversationFlowTest
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== MANUAL CONVERSATION FLOW TEST ===\n");
        Console.WriteLine("This test simulates clicking 'Talk with Tam' button in the UI\n");
        
        // Set up services
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
        var serviceProvider = services.BuildServiceProvider();
        
        var gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
        var gameFacade = serviceProvider.GetRequiredService<IGameFacade>();
        var narrativeManager = serviceProvider.GetRequiredService<NarrativeManager>();
        
        // Start game
        await gameWorldManager.StartGame();
        Console.WriteLine("Game started - tutorial should be active\n");
        
        // Progress tutorial to make Tam visible
        Console.WriteLine("Step 1: Execute rest action to progress tutorial...");
        var restActions = gameFacade.GetLocationActions();
        var restAction = restActions.ActionGroups
            .SelectMany(g => g.Actions)
            .FirstOrDefault(a => a.Description.Contains("Rest"));
        
        if (restAction != null)
        {
            Console.WriteLine($"  Found rest action: {restAction.Id}");
            await gameFacade.ExecuteLocationActionAsync(restAction.Id);
            Console.WriteLine("  Rest completed\n");
        }
        
        // Now check for Tam
        Console.WriteLine("Step 2: Look for Talk with Tam action...");
        var locationActions = gameFacade.GetLocationActions();
        var talkWithTam = locationActions.ActionGroups
            .SelectMany(g => g.Actions)
            .FirstOrDefault(a => a.Id.Contains("tam") || a.Description.Contains("Tam"));
        
        if (talkWithTam == null)
        {
            Console.WriteLine("  ERROR: Talk with Tam action not found!");
            Console.WriteLine("  Available actions:");
            foreach (var group in locationActions.ActionGroups)
            {
                foreach (var action in group.Actions)
                {
                    Console.WriteLine($"    - {action.Id}: {action.Description}");
                }
            }
            return 1;
        }
        
        Console.WriteLine($"  Found action: {talkWithTam.Id} - {talkWithTam.Description}\n");
        
        // Execute the action (simulating button click)
        Console.WriteLine("Step 3: Execute Talk with Tam action (simulating button click)...");
        bool success = await gameFacade.ExecuteLocationActionAsync(talkWithTam.Id);
        Console.WriteLine($"  ExecuteLocationActionAsync result: {success}\n");
        
        // Check if conversation is pending
        Console.WriteLine("Step 4: Check conversation state...");
        var conversation = gameFacade.GetCurrentConversation();
        
        if (conversation == null)
        {
            Console.WriteLine("  ERROR: No conversation found!");
            return 1;
        }
        
        Console.WriteLine($"  Conversation started with: {conversation.NpcName}");
        Console.WriteLine($"  NPC ID: {conversation.NpcId}");
        Console.WriteLine($"  Text: {conversation.CurrentText?.Substring(0, Math.Min(100, conversation.CurrentText?.Length ?? 0))}...");
        Console.WriteLine($"  Choices: {conversation.Choices?.Count ?? 0}");
        Console.WriteLine($"  IsComplete: {conversation.IsComplete}");
        
        Console.WriteLine("\n✓ SUCCESS: Conversation flow working correctly!");
        Console.WriteLine("The UI would now navigate to ConversationScreen and show the conversation.");
        
        return 0;
    }
}