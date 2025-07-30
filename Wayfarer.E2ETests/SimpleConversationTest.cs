using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public class SimpleConversationTest
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== SIMPLE CONVERSATION TEST ===\n");
        
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
        var conversationStateManager = serviceProvider.GetRequiredService<ConversationStateManager>();
        var commandExecutor = serviceProvider.GetRequiredService<CommandExecutor>();
        var conversationFactory = serviceProvider.GetRequiredService<ConversationFactory>();
        var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
        var messageSystem = serviceProvider.GetRequiredService<MessageSystem>();
        var gameFacade = serviceProvider.GetRequiredService<IGameFacade>();
        var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
        
        // Start game
        await gameWorldManager.StartGame();
        Console.WriteLine("Game started");
        
        // Check player location
        var player = gameWorld.GetPlayer();
        Console.WriteLine($"\nPlayer location: {player.CurrentLocation?.Name} ({player.CurrentLocation?.Id})");
        Console.WriteLine($"Player spot: {player.CurrentLocationSpot?.Name} ({player.CurrentLocationSpot?.SpotID})");
        Console.WriteLine($"Current time: {gameWorld.CurrentTimeBlock}");
        
        // Get Tam
        var tam = npcRepository.GetById("tam_beggar");
        Console.WriteLine($"\nFound Tam: {tam?.Name}");
        if (tam != null)
        {
            Console.WriteLine($"Is Tam available at current spot/time? {tam.IsAvailableAtTime(player.CurrentLocationSpot?.SpotID, gameWorld.CurrentTimeBlock)}");
        }
        
        // Create and execute converse command directly
        var converseCommand = new ConverseCommand(
            "tam_beggar",
            conversationFactory,
            npcRepository,
            messageSystem,
            conversationStateManager
        );
        
        // First check if command can execute
        var canExecute = converseCommand.CanExecute(gameWorld);
        Console.WriteLine($"\nCanExecute result: {canExecute.IsValid} - {canExecute.FailureReason}");
        
        if (canExecute.IsValid)
        {
            Console.WriteLine("\nExecuting ConverseCommand...");
            var result = await commandExecutor.ExecuteAsync(converseCommand);
            Console.WriteLine($"Command result: {result.IsSuccess} - {result.Message}");
        }
        
        // Check conversation state
        Console.WriteLine($"\nConversationPending: {conversationStateManager.ConversationPending}");
        Console.WriteLine($"PendingConversationManager null? {conversationStateManager.PendingConversationManager == null}");
        
        // Try to get conversation through facade
        var conversation = gameFacade.GetCurrentConversation();
        Console.WriteLine($"\nGetCurrentConversation null? {conversation == null}");
        if (conversation != null)
        {
            Console.WriteLine($"NPC Name: {conversation.NpcName}");
            Console.WriteLine($"NPC ID: {conversation.NpcId}");
            Console.WriteLine($"Text: {conversation.CurrentText?.Substring(0, Math.Min(100, conversation.CurrentText?.Length ?? 0))}");
            Console.WriteLine($"Choices: {conversation.Choices?.Count ?? 0}");
        }
        
        return 0;
    }
}