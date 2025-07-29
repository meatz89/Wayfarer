using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Test to verify that conversations properly trigger screen switching in MainGameplayView
/// </summary>
public class TestConversationScreenSwitch
{
    private IServiceProvider _provider;
    private IGameFacade _gameFacade;
    private ConversationStateManager _conversationStateManager;
    
    public static async Task<int> Main(string[] args)
    {
        // Check if this test was specifically requested
        if (args.Length > 0 && args[0] != "TestConversationScreenSwitch")
        {
            return 0; // Skip this test
        }
        
        var test = new TestConversationScreenSwitch();
        return await test.RunTest();
    }
    
    private async Task<int> RunTest()
    {
        Console.WriteLine("=== CONVERSATION SCREEN SWITCH TEST ===\n");

        // Initialize services
        if (!InitializeServices())
        {
            Console.WriteLine("✗ FAIL: Service initialization failed!");
            return 1;
        }

        // Start game
        await _gameFacade.StartGameAsync();
        Console.WriteLine("Game started successfully\n");
        // Move to location with NPCs
        Console.WriteLine("=== TEST 1: Moving to Lower Ward Square ===");
        await _gameFacade.ExecuteLocationActionAsync("move_lower_ward_square");
        
        var locationActions = _gameFacade.GetLocationActions();
        Console.WriteLine($"Current location: {locationActions.LocationName}");
        
        // Find conversation actions
        var conversationActions = locationActions.ActionGroups
            .FirstOrDefault(g => g.ActionType == "Social")?.Actions
            .Where(a => a.Id.StartsWith("talk_"))
            .ToList() ?? new List<ActionOptionViewModel>();
            
        Console.WriteLine($"Available NPCs to talk to: {conversationActions.Count}");
        foreach (var action in conversationActions)
        {
            Console.WriteLine($"  - {action.Id}: {action.Description}");
        }

        if (conversationActions.Any())
        {
            Console.WriteLine("\n=== TEST 2: Starting Conversation ===");
            
            // Check initial state
            Console.WriteLine($"ConversationPending before: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"PendingConversationManager before: {_conversationStateManager.PendingConversationManager != null}");
            
            // Execute conversation command
            var firstNpc = conversationActions.First();
            Console.WriteLine($"\nExecuting action: {firstNpc.Id}");
            
            var success = await _gameFacade.ExecuteLocationActionAsync(firstNpc.Id);
            Console.WriteLine($"Action execution result: {success}");
            
            // Check conversation state after execution
            Console.WriteLine($"\nConversationPending after: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"PendingConversationManager after: {_conversationStateManager.PendingConversationManager != null}");
            
            // Check if GameFacade can retrieve the conversation
            var currentConversation = _gameFacade.GetCurrentConversation();
            Console.WriteLine($"GetCurrentConversation returns: {currentConversation != null}");
            
            if (currentConversation != null)
            {
                Console.WriteLine($"  NPC: {currentConversation.NpcName}");
                Console.WriteLine($"  IsComplete: {currentConversation.IsComplete}");
                Console.WriteLine($"  Text: {currentConversation.CurrentText?.Substring(0, Math.Min(50, currentConversation.CurrentText.Length))}...");
                
                Console.WriteLine("\n✓ PASS: Conversation properly set up for screen switch!");
            }
            else
            {
                Console.WriteLine("\n✗ FAIL: Conversation not available through GameFacade!");
            }
        }
        else
        {
            Console.WriteLine("\n✗ FAIL: No NPCs available to test conversation with!");
        }
        
        Console.WriteLine("\n=== TEST COMPLETE ===");
        return conversationActions.Any() && _conversationStateManager.ConversationPending ? 0 : 1;
    }
    
    private bool InitializeServices()
    {
        try
        {
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
            _provider = services.BuildServiceProvider();
            
            // Get required services
            _gameFacade = _provider.GetRequiredService<IGameFacade>();
            _conversationStateManager = _provider.GetRequiredService<ConversationStateManager>();
            
            Console.WriteLine("✓ All services initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Service initialization failed: {ex.Message}");
            return false;
        }
    }
}