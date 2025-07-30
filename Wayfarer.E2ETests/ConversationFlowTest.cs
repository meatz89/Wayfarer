using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.E2ETests
{
    public class ConversationFlowTest
    {
        public static async Task RunTest()
        {
            Console.WriteLine("=== CONVERSATION FLOW TEST ===\n");
            
            var services = new ServiceCollection();
            ServiceConfiguration.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            
            var gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            var gameFacade = serviceProvider.GetRequiredService<IGameFacade>();
            var locationActionsService = serviceProvider.GetRequiredService<LocationActionsUIService>();
            var conversationStateManager = serviceProvider.GetRequiredService<ConversationStateManager>();
            var timeManager = serviceProvider.GetRequiredService<ITimeManager>();
            var narrativeManager = serviceProvider.GetRequiredService<NarrativeManager>();
            
            // Start game
            await gameWorldManager.StartGame();
            
            // Progress tutorial to where Tam is visible
            Console.WriteLine("=== Step 1: Progress tutorial to show Tam ===");
            
            // Complete rest action to progress tutorial
            var restActions = gameFacade.GetLocationActions();
            var restAction = restActions.ActionGroups
                .SelectMany(g => g.Actions)
                .FirstOrDefault(a => a.Description.Contains("Rest"));
            
            if (restAction != null)
            {
                Console.WriteLine($"Executing rest action: {restAction.Id}");
                await gameFacade.ExecuteLocationActionAsync(restAction.Id);
            }
            
            // Note: TimeManager doesn't have AdvanceTimeByHours - we need to progress tutorial instead
            
            // Check if Tam is now visible
            Console.WriteLine("\n=== Step 2: Check for Tam's availability ===");
            var locationActions = gameFacade.GetLocationActions();
            
            Console.WriteLine($"Total action groups: {locationActions.ActionGroups.Count}");
            foreach (var group in locationActions.ActionGroups)
            {
                Console.WriteLine($"  {group.ActionType}: {group.Actions.Count} actions");
                foreach (var action in group.Actions)
                {
                    Console.WriteLine($"    - {action.Id}: {action.Description}");
                }
            }
            
            // Find talk with Tam action
            var talkWithTam = locationActions.ActionGroups
                .SelectMany(g => g.Actions)
                .FirstOrDefault(a => a.Id == "talk_tam_beggar" || a.Description.Contains("Tam"));
            
            if (talkWithTam == null)
            {
                Console.WriteLine("ERROR: Talk with Tam action not found!");
                return;
            }
            
            Console.WriteLine($"\nFound action: {talkWithTam.Id} - {talkWithTam.Description}");
            
            // Execute the talk action
            Console.WriteLine("\n=== Step 3: Execute Talk with Tam ===");
            Console.WriteLine($"ConversationPending before: {conversationStateManager.ConversationPending}");
            
            bool success = await gameFacade.ExecuteLocationActionAsync(talkWithTam.Id);
            Console.WriteLine($"ExecuteLocationActionAsync result: {success}");
            
            // Check conversation state
            Console.WriteLine($"\nConversationPending after: {conversationStateManager.ConversationPending}");
            Console.WriteLine($"PendingConversationManager null? {conversationStateManager.PendingConversationManager == null}");
            
            // Get conversation through facade
            var conversation = gameFacade.GetCurrentConversation();
            Console.WriteLine($"\nGetCurrentConversation returned null? {conversation == null}");
            
            if (conversation != null)
            {
                Console.WriteLine($"Conversation NPC: {conversation.NpcName}");
                Console.WriteLine($"Conversation text: {conversation.CurrentText?.Substring(0, Math.Min(100, conversation.CurrentText?.Length ?? 0))}...");
                Console.WriteLine($"Choices count: {conversation.Choices?.Count ?? 0}");
                Console.WriteLine($"IsComplete: {conversation.IsComplete}");
            }
            
            Console.WriteLine("\n=== TEST COMPLETE ===");
        }
    }
}