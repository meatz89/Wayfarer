using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Test specifically for tutorial conversation functionality
/// </summary>
public class TutorialConversationTest
{
    private IServiceProvider _provider;
    private GameWorld _gameWorld;
    private GameWorldManager _gameWorldManager;
    private NarrativeManager _narrativeManager;
    private ConversationStateManager _conversationStateManager;
    private CommandExecutor _commandExecutor;
    private CommandDiscoveryService _commandDiscovery;
    private GameFacade _gameFacade;
    
    public static async Task<int> Main(string[] args)
    {
        var test = new TutorialConversationTest();
        return await test.RunConversationTest();
    }
    
    private async Task<int> RunConversationTest()
    {
        Console.WriteLine("=== TUTORIAL CONVERSATION TEST ===\n");
        
        try
        {
            // Initialize services
            if (!InitializeServices())
            {
                Console.WriteLine("✗ FAIL: Service initialization failed!");
                return 1;
            }
            
            // Start game to trigger tutorial
            Console.WriteLine("Starting game...");
            await _gameWorldManager.StartGame();
            
            // Rest to progress tutorial to conversation stage
            Console.WriteLine("\n=== STEP 1: Rest to progress tutorial ===");
            if (!await RestToProgressTutorial())
            {
                Console.WriteLine("✗ FAIL: Could not progress tutorial!");
                return 1;
            }
            
            // Move to location with NPCs
            Console.WriteLine("\n=== STEP 2: Move to location with NPCs ===");
            if (!await MoveToNPCLocation())
            {
                Console.WriteLine("✗ FAIL: Could not move to NPC location!");
                return 1;
            }
            
            // Test conversation initiation
            Console.WriteLine("\n=== STEP 3: Test conversation with NPC ===");
            if (!await TestConversation())
            {
                Console.WriteLine("✗ FAIL: Conversation test failed!");
                return 1;
            }
            
            Console.WriteLine("\n✓ ALL TESTS PASSED - Tutorial conversations working!");
            return 0;
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
            _conversationStateManager = _provider.GetRequiredService<ConversationStateManager>();
            _commandExecutor = _provider.GetRequiredService<CommandExecutor>();
            _commandDiscovery = _provider.GetRequiredService<CommandDiscoveryService>();
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
    
    private async Task<bool> RestToProgressTutorial()
    {
        try
        {
            // Check current tutorial step
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Console.WriteLine($"Current tutorial step: {currentStep?.Id} - {currentStep?.Name}");
            
            // Rest to progress from day1_wake
            if (currentStep?.Id == "day1_wake")
            {
                Console.WriteLine("Executing rest command...");
                
                // Find rest action in available commands
                var locationActions = _gameFacade.GetLocationActions();
                var restAction = locationActions.ActionGroups
                    .SelectMany(g => g.Actions)
                    .FirstOrDefault(a => a.Description.ToLower().Contains("rest"));
                
                if (restAction == null)
                {
                    Console.WriteLine("Rest action not available");
                    return false;
                }
                
                var success = await _gameFacade.ExecuteLocationActionAsync(restAction.Id);
                
                if (!success)
                {
                    Console.WriteLine("Rest failed");
                    return false;
                }
                
                Console.WriteLine("✓ Rest successful");
                
                // Check new step
                currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
                Console.WriteLine($"New tutorial step: {currentStep?.Id} - {currentStep?.Name}");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rest failed: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> MoveToNPCLocation()
    {
        try
        {
            // Get current location
            var player = _gameWorld.GetPlayer();
            Console.WriteLine($"Current location: {player.CurrentLocation?.Name}, spot: {player.CurrentLocationSpot?.Name}");
            
            // Move to Lower Ward Square where NPCs are
            if (player.CurrentLocationSpot?.SpotID != "lower_ward_square")
            {
                Console.WriteLine("Moving to Lower Ward Square...");
                
                // Use LocationActionsUIService to execute move command
                var locationActions = _gameFacade.GetLocationActions();
                var moveAction = locationActions.ActionGroups
                    .SelectMany(g => g.Actions)
                    .FirstOrDefault(a => a.Id == "move_lower_ward_square");
                
                if (moveAction != null)
                {
                    var success = await _gameFacade.ExecuteLocationActionAsync(moveAction.Id);
                    if (!success)
                    {
                        Console.WriteLine("Move failed");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Move action not found");
                    return false;
                }
                
                // Verify location
                player = _gameWorld.GetPlayer();
                Console.WriteLine($"✓ Moved to: {player.CurrentLocation?.Name}, spot: {player.CurrentLocationSpot?.Name}");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move failed: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestConversation()
    {
        try
        {
            // Check available NPCs
            var locationActions = _gameFacade.GetLocationActions();
            var conversationActions = locationActions.ActionGroups
                .FirstOrDefault(g => g.ActionType == "Social")?.Actions
                .Where(a => a.Id.StartsWith("talk_"))
                .ToList() ?? new List<ActionOptionViewModel>();
            
            Console.WriteLine($"Available conversation actions: {conversationActions.Count}");
            foreach (var action in conversationActions)
            {
                Console.WriteLine($"  - {action.Id}: {action.Description}");
            }
            
            // Find Tam or Elena
            var targetAction = conversationActions.FirstOrDefault(a => 
                a.Id == "talk_tam_beggar" || a.Id == "talk_elena_scribe");
            
            if (targetAction == null)
            {
                Console.WriteLine("No suitable NPC found for conversation");
                return false;
            }
            
            Console.WriteLine($"\nInitiating conversation with: {targetAction.Description}");
            
            // Execute conversation command
            var success = await _gameFacade.ExecuteLocationActionAsync(targetAction.Id);
            Console.WriteLine($"Command execution result: {success}");
            
            // Check if conversation was set
            var pendingConversation = _conversationStateManager.ConversationPending;
            Console.WriteLine($"Conversation pending in state manager: {pendingConversation}");
            
            // Check through facade
            var currentConversation = _gameFacade.GetCurrentConversation();
            Console.WriteLine($"Current conversation from facade: {currentConversation != null}");
            
            if (currentConversation != null)
            {
                Console.WriteLine($"  NPC: {currentConversation.NpcName}");
                Console.WriteLine($"  Topic: {currentConversation.ConversationTopic}");
                Console.WriteLine($"  Has text: {!string.IsNullOrEmpty(currentConversation.CurrentText)}");
                Console.WriteLine($"  Choices: {currentConversation.Choices?.Count ?? 0}");
                
                Console.WriteLine("\n✓ Conversation initiated successfully!");
                return true;
            }
            else
            {
                Console.WriteLine("\n✗ Conversation not detected!");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Conversation test failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return false;
        }
    }
}