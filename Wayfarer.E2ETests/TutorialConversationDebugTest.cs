using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Debug test to diagnose why conversations don't show during tutorial
/// </summary>
public class TutorialConversationDebugTest
{
    private IServiceProvider _provider;
    private GameWorld _gameWorld;
    private GameWorldManager _gameWorldManager;
    private NarrativeManager _narrativeManager;
    private CommandDiscoveryService _commandDiscovery;
    private CommandExecutor _commandExecutor;
    private ConversationStateManager _conversationStateManager;
    private MessageSystem _messageSystem;
    private LocationActionsUIService _locationActionsService;
    private GameFacade _gameFacade;
    
    public static async Task<int> Main(string[] args)
    {
        var test = new TutorialConversationDebugTest();
        return await test.RunDebugTest();
    }
    
    private async Task<int> RunDebugTest()
    {
        Console.WriteLine("=== TUTORIAL CONVERSATION DEBUG TEST ===\n");
        
        try
        {
            // Initialize services
            if (!InitializeServices())
            {
                Console.WriteLine("✗ FAIL: Service initialization failed!");
                return 1;
            }
            
            // Start the game
            Console.WriteLine("\n1. Starting game...");
            await _gameWorldManager.StartGame();
            
            // Check tutorial is active
            Console.WriteLine("\n2. Checking tutorial state...");
            bool tutorialActive = _narrativeManager.IsNarrativeActive("wayfarer_tutorial");
            Console.WriteLine($"   Tutorial active: {tutorialActive}");
            if (!tutorialActive)
            {
                Console.WriteLine("✗ FAIL: Tutorial did not auto-start!");
                return 1;
            }
            
            // Check current step
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Console.WriteLine($"   Current step: {currentStep?.Id} - {currentStep?.Name}");
            Console.WriteLine($"   Allowed actions: {string.Join(", ", currentStep?.AllowedActions ?? new List<string>())}");
            
            // Discover available commands
            Console.WriteLine("\n3. Discovering commands...");
            var discovery = _commandDiscovery.DiscoverCommands(_gameWorld);
            Console.WriteLine($"   Total commands found: {discovery.AllCommands.Count}");
            
            // Find talk commands
            var talkCommands = discovery.AllCommands.Where(c => c.UniqueId.StartsWith("talk_")).ToList();
            Console.WriteLine($"\n4. Talk commands found: {talkCommands.Count}");
            foreach (var cmd in talkCommands)
            {
                Console.WriteLine($"   - {cmd.UniqueId}: {cmd.DisplayName} (Available: {cmd.IsAvailable})");
                if (!cmd.IsAvailable)
                {
                    Console.WriteLine($"     Reason: {cmd.UnavailableReason}");
                }
            }
            
            // Check if we have a talk command for patron_intermediary
            var patronTalkCommand = talkCommands.FirstOrDefault(c => c.UniqueId.Contains("patron_intermediary"));
            if (patronTalkCommand == null)
            {
                Console.WriteLine("\n✗ FAIL: No talk command found for patron_intermediary!");
                return 1;
            }
            
            // Execute the talk command
            Console.WriteLine($"\n5. Executing command: {patronTalkCommand.UniqueId}");
            Console.WriteLine("   ConversationStateManager before execute:");
            Console.WriteLine($"   - ConversationPending: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"   - PendingConversationManager: {_conversationStateManager.PendingConversationManager}");
            
            // Execute through LocationActionsUIService
            bool success = await _locationActionsService.ExecuteActionAsync(patronTalkCommand.UniqueId);
            Console.WriteLine($"   Execute result: {success}");
            
            // Check conversation state after execution
            Console.WriteLine("\n6. Checking conversation state after execute:");
            Console.WriteLine($"   - ConversationPending: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"   - PendingConversationManager: {_conversationStateManager.PendingConversationManager}");
            
            if (_conversationStateManager.PendingConversationManager != null)
            {
                var conv = _conversationStateManager.PendingConversationManager;
                Console.WriteLine($"   - NPC: {conv.Context.TargetNPC.Name}");
                Console.WriteLine($"   - Topic: {conv.Context.ConversationTopic}");
                Console.WriteLine($"   - State: {conv.State?.CurrentNarrative?.Substring(0, Math.Min(50, conv.State?.CurrentNarrative?.Length ?? 0))}...");
            }
            
            // Check GameFacade
            Console.WriteLine("\n7. Checking GameFacade conversation state:");
            var facadeConversation = _gameFacade.GetCurrentConversation();
            if (facadeConversation != null)
            {
                Console.WriteLine($"   - NPC: {facadeConversation.NpcName}");
                Console.WriteLine($"   - Text: {facadeConversation.CurrentText?.Substring(0, Math.Min(50, facadeConversation.CurrentText?.Length ?? 0))}...");
                Console.WriteLine($"   - Choices: {facadeConversation.Choices?.Count ?? 0}");
            }
            else
            {
                Console.WriteLine("   - No conversation from GameFacade!");
            }
            
            // Check system messages
            Console.WriteLine("\n8. System messages:");
            var messages = _gameFacade.GetSystemMessages();
            foreach (var msg in messages)
            {
                Console.WriteLine($"   - [{msg.Type}] {msg.Message}");
            }
            
            // Summary
            Console.WriteLine("\n=== SUMMARY ===");
            if (_conversationStateManager.ConversationPending && facadeConversation != null)
            {
                Console.WriteLine("✓ Conversation successfully initiated!");
                Console.WriteLine("  The conversation should now be visible in the UI.");
                return 0;
            }
            else
            {
                Console.WriteLine("✗ FAIL: Conversation not properly set up!");
                if (!_conversationStateManager.ConversationPending)
                {
                    Console.WriteLine("  - ConversationStateManager not marked as pending");
                }
                if (facadeConversation == null)
                {
                    Console.WriteLine("  - GameFacade not returning conversation");
                }
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ CRITICAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
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
            _gameWorld = _provider.GetRequiredService<GameWorld>();
            _gameWorldManager = _provider.GetRequiredService<GameWorldManager>();
            _narrativeManager = _provider.GetRequiredService<NarrativeManager>();
            _commandDiscovery = _provider.GetRequiredService<CommandDiscoveryService>();
            _commandExecutor = _provider.GetRequiredService<CommandExecutor>();
            _conversationStateManager = _provider.GetRequiredService<ConversationStateManager>();
            _messageSystem = _provider.GetRequiredService<MessageSystem>();
            _locationActionsService = _provider.GetRequiredService<LocationActionsUIService>();
            _gameFacade = _provider.GetRequiredService<GameFacade>();
            
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