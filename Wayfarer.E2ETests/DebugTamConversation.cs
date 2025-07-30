using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public class DebugTamConversation
{
    private IServiceProvider _services;
    private IGameFacade _gameFacade;
    private ConversationStateManager _conversationStateManager;
    private LocationActionsUIService _locationActionsService;
    private GameWorld _gameWorld;
    private CommandDiscoveryService _commandDiscovery;
    private LocationRepository _locationRepository;
    private CommandExecutor _commandExecutor;
    
    public static async Task<int> Main(string[] args)
    {
        var test = new DebugTamConversation();
        await test.RunTest();
        return 0;
    }
    
    private async Task RunTest()
    {
        Console.WriteLine("=== DEBUG TAM CONVERSATION TEST ===\n");
        
        // Initialize services
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
        _services = services.BuildServiceProvider();
        _gameFacade = _services.GetRequiredService<IGameFacade>();
        _conversationStateManager = _services.GetRequiredService<ConversationStateManager>();
        _locationActionsService = _services.GetRequiredService<LocationActionsUIService>();
        _gameWorld = _services.GetRequiredService<GameWorld>();
        _commandDiscovery = _services.GetRequiredService<CommandDiscoveryService>();
        _locationRepository = _services.GetRequiredService<LocationRepository>();
        _commandExecutor = _services.GetRequiredService<CommandExecutor>();
        
        try
        {
            // Start game
            Console.WriteLine("Starting game...");
            await _gameFacade.StartGameAsync();
            
            // Progress through tutorial to meet Tam
            Console.WriteLine("\nProgressing through tutorial to meet Tam...");
            
            // 1. Check current location
            var currentLocation = _locationRepository.GetCurrentLocation();
            var currentSpot = _locationRepository.GetCurrentLocationSpot();
            Console.WriteLine($"Current location: {currentLocation?.Id}, spot: {currentSpot?.SpotID}");
            
            // 2. Find movement command to lower_ward_square
            var commands = _commandDiscovery.DiscoverCommands(_gameWorld);
            Console.WriteLine($"\nDiscovered {commands.AllCommands.Count} commands:");
            foreach (var cmd in commands.AllCommands)
            {
                Console.WriteLine($"  - {cmd.UniqueId}: {cmd.DisplayName} (Available: {cmd.IsAvailable})");
            }
            
            // Find travel command to lower_ward_square
            var travelCommand = commands.AllCommands.FirstOrDefault(c => 
                c.UniqueId.Contains("travel") && c.UniqueId.Contains("lower_ward_square"));
            
            if (travelCommand != null)
            {
                Console.WriteLine($"\nExecuting travel command: {travelCommand.UniqueId}");
                var result = await _commandExecutor.ExecuteAsync(travelCommand.Command);
                Console.WriteLine($"Travel result: {result.IsSuccess} - {result.Message}");
            }
            else
            {
                Console.WriteLine("\n✗ No travel command found to lower_ward_square!");
            }
            
            // Verify we moved
            currentSpot = _locationRepository.GetCurrentLocationSpot();
            Console.WriteLine($"\nAfter travel - Current spot: {currentSpot?.SpotID}");;
            
            // Re-discover commands at new location
            Console.WriteLine("\nChecking for Tam at current location...");
            var discovery = _commandDiscovery.DiscoverCommands(_gameWorld);
            
            Console.WriteLine($"Total commands discovered: {discovery.AllCommands.Count}");
            foreach (var cmd in discovery.AllCommands)
            {
                Console.WriteLine($"  - {cmd.UniqueId}: {cmd.DisplayName} (Available: {cmd.IsAvailable})");
                if (cmd.Command is ConverseCommand converseCmd)
                {
                    Console.WriteLine($"    -> ConverseCommand NPC: {converseCmd.NpcId}");
                }
            }
            
            // Find Tam conversation
            var tamAction = discovery.AllCommands.FirstOrDefault(c => 
                c.UniqueId == "talk_tam_beggar" || 
                c.DisplayName.Contains("Tam"));
            
            if (tamAction == null)
            {
                Console.WriteLine("\n✗ FAIL: Tam conversation action not found!");
                return;
            }
            
            Console.WriteLine($"\n✓ Found Tam action: {tamAction.UniqueId} - {tamAction.DisplayName}");
            
            // Execute conversation
            Console.WriteLine("\nExecuting conversation with Tam...");
            Console.WriteLine($"  Command ID: {tamAction.UniqueId}");
            Console.WriteLine($"  Command type: {tamAction.Command.GetType().Name}");
            
            // Check conversation state before
            Console.WriteLine("\nConversation state BEFORE execution:");
            Console.WriteLine($"  ConversationPending: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"  PendingConversationManager: {_conversationStateManager.PendingConversationManager != null}");
            
            // Execute via LocationActionsUIService to match UI flow
            var success = await _locationActionsService.ExecuteActionAsync(tamAction.UniqueId);
            Console.WriteLine($"\nExecution result: {success}");
            
            // Check conversation state after
            Console.WriteLine("\nConversation state AFTER execution:");
            Console.WriteLine($"  ConversationPending: {_conversationStateManager.ConversationPending}");
            Console.WriteLine($"  PendingConversationManager: {_conversationStateManager.PendingConversationManager != null}");
            
            // Check via facade
            var currentConversation = _gameFacade.GetCurrentConversation();
            Console.WriteLine($"\nFacade GetCurrentConversation: {currentConversation != null}");
            if (currentConversation != null)
            {
                Console.WriteLine($"  NPC: {currentConversation.NpcName}");
                Console.WriteLine($"  Topic: {currentConversation.ConversationTopic}");
                Console.WriteLine($"  Has text: {!string.IsNullOrEmpty(currentConversation.CurrentText)}");
                Console.WriteLine($"  Choices: {currentConversation.Choices?.Count ?? 0}");
            }
            
            // Check if MainGameplayView would detect it
            if (_conversationStateManager.ConversationPending && currentConversation != null)
            {
                Console.WriteLine("\n✓ SUCCESS: Conversation initiated properly!");
                Console.WriteLine("MainGameplayView.PollGameState() should navigate to ConversationScreen");
            }
            else
            {
                Console.WriteLine("\n✗ FAIL: Conversation not properly initiated!");
                Console.WriteLine("MainGameplayView.PollGameState() will NOT navigate to ConversationScreen");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    
    private async Task<ActionOptionViewModel[]> GetLocationActions()
    {
        var locationActions = _gameFacade.GetLocationActions();
        return locationActions.ActionGroups
            .SelectMany(g => g.Actions)
            .ToArray();
    }
}