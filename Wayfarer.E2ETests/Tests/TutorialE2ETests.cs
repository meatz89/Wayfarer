using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wayfarer.E2ETests.Infrastructure;
using Wayfarer.E2ETests.Assertions;

namespace Wayfarer.E2ETests.Tests
{
    /// <summary>
    /// E2E tests for the complete tutorial flow.
    /// Tests the real GameFacade with a real GameWorld - no mocks.
    /// </summary>
    public class TutorialE2ETests : E2ETestBase
    {
        public async Task Tutorial_CompleteFlow_SuccessfullyCompletesTourial()
        {
            Console.WriteLine("\n=== Tutorial Complete Flow Test ===\n");
            
            // Arrange - Set up game world in tutorial state
            await SetupAsync(startTutorial: true);
            
            // Assert initial state
            Console.WriteLine("1. Verifying initial tutorial state...");
            TutorialAssertions.AssertTutorialActive(GameWorld);
            TutorialAssertions.AssertTutorialStartState(GameWorld);
            
            // Step 1: Check available actions at starting location
            Console.WriteLine("\n2. Checking available actions at Lower Ward Square...");
            var actions = GetLocationActions();
            Console.WriteLine($"   Found {actions.Count} actions");
            foreach (var action in actions)
            {
                Console.WriteLine($"   - {action.ID}: {action.DisplayName}");
            }
            
            // Should have Talk action for Tam
            var talkToTam = actions.FirstOrDefault(a => a.ID == "talk_tam_beggar");
            if (talkToTam == null)
            {
                throw new AssertionException("Talk to Tam action not found");
            }
            
            // Step 2: Talk to Tam
            Console.WriteLine("\n3. Talking to Tam...");
            bool talkResult = await ExecuteLocationAction("talk_tam_beggar");
            if (!talkResult)
            {
                throw new AssertionException("Failed to talk to Tam");
            }
            
            // Verify conversation started
            var convState = ServiceProvider.GetRequiredService<ConversationStateManager>();
            GameWorldAssertions.AssertConversationActive(convState);
            
            var conversation = GetActiveConversation();
            Console.WriteLine($"   Conversation started with {conversation.Context.TargetNPC.Name}");
            
            // Step 3: Progress through conversation
            Console.WriteLine("\n4. Progressing through tutorial conversation...");
            
            // Get initial conversation state
            var convManager = convState.PendingConversationManager;
            var currentNode = await convManager.GetCurrentNode();
            Console.WriteLine($"   Current node text: {currentNode.Text}");
            
            // Look for accept option
            var choices = await convManager.GetAvailableChoices();
            Console.WriteLine($"   Available choices: {choices.Count}");
            foreach (var choice in choices)
            {
                Console.WriteLine($"   - {choice.ChoiceID}: {choice.NarrativeText}");
            }
            
            // Choose to accept helping Tam
            var acceptChoice = choices.FirstOrDefault(c => 
                c.ChoiceID.Contains("accept") || 
                c.NarrativeText.ToLower().Contains("help"));
            
            if (acceptChoice != null)
            {
                Console.WriteLine($"   Selecting: {acceptChoice.NarrativeText}");
                await convManager.MakeChoice(acceptChoice);
                
                // Continue conversation if needed
                while (!convManager.State.IsConversationComplete)
                {
                    currentNode = await convManager.GetCurrentNode();
                    choices = await convManager.GetAvailableChoices();
                    
                    if (choices.Count > 0)
                    {
                        // Select first available choice to progress
                        Console.WriteLine($"   Continuing with: {choices[0].NarrativeText}");
                        await convManager.MakeChoice(choices[0]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            // Clear conversation from UI state
            convState.ClearPendingConversation();
            
            // Step 4: Verify letter received
            Console.WriteLine("\n5. Verifying tutorial letter received...");
            TutorialAssertions.AssertReceivedTutorialLetter(GameWorld);
            
            var letter = GameWorld.LetterQueueManager.PlayerQueue.Letters[0];
            Console.WriteLine($"   Received letter: {letter.ID}");
            Console.WriteLine($"   Destination: {letter.DestinationLocationId}");
            Console.WriteLine($"   Payment: {letter.Payment} coins");
            Console.WriteLine($"   Deadline: Day {letter.DeadlineDay}");
            
            // Step 5: Travel to destination
            Console.WriteLine("\n6. Traveling to delivery destination...");
            
            // Find route to destination
            var routeRepo = ServiceProvider.GetRequiredService<RouteRepository>();
            var routes = routeRepo.GetRoutesFrom(Player.CurrentLocationId);
            var routeToDestination = routes.FirstOrDefault(r => 
                r.DestinationId == letter.DestinationLocationId);
            
            if (routeToDestination == null)
            {
                throw new AssertionException(
                    $"No route found from {Player.CurrentLocationId} to {letter.DestinationLocationId}");
            }
            
            Console.WriteLine($"   Using route: {routeToDestination.Name}");
            Console.WriteLine($"   Time cost: {routeToDestination.BaseHours} hours");
            
            // Execute travel
            var travelIntent = new TravelIntent 
            { 
                RouteId = routeToDestination.ID 
            };
            bool travelResult = await ExecuteIntent(travelIntent);
            
            if (!travelResult)
            {
                throw new AssertionException("Failed to travel to destination");
            }
            
            // Verify arrival
            GameWorldAssertions.AssertPlayerAt(GameWorld, 
                letter.DestinationLocationId, 
                routeToDestination.ArrivalSpotId);
            
            // Step 6: Deliver the letter
            Console.WriteLine("\n7. Delivering the letter...");
            
            var deliverIntent = new DeliverLetterIntent 
            { 
                LetterId = letter.ID 
            };
            bool deliverResult = await ExecuteIntent(deliverIntent);
            
            if (!deliverResult)
            {
                throw new AssertionException("Failed to deliver letter");
            }
            
            // Verify letter delivered
            GameWorldAssertions.AssertDoesNotHaveLetter(GameWorld, letter.ID);
            GameWorldAssertions.AssertCoins(GameWorld, 5); // Should have payment
            
            // Step 7: Verify tutorial completion
            Console.WriteLine("\n8. Verifying tutorial completion...");
            TutorialAssertions.AssertTutorialComplete(GameWorld);
            
            Console.WriteLine("\n✅ Tutorial completed successfully!");
            Console.WriteLine($"   Final location: {Player.CurrentLocationId}/{Player.CurrentSpotId}");
            Console.WriteLine($"   Coins: {Player.Coins}");
            Console.WriteLine($"   Time: Day {GameWorld.TimeManager.CurrentDay}, Hour {GameWorld.TimeManager.CurrentHour}");
        }
        
        public async Task Tutorial_CanDeclineTamLetter_StillInTutorial()
        {
            Console.WriteLine("\n=== Tutorial Decline Path Test ===\n");
            
            // Arrange
            await SetupAsync(startTutorial: true);
            
            // Talk to Tam
            await ExecuteLocationAction("talk_tam_beggar");
            
            var convState = ServiceProvider.GetRequiredService<ConversationStateManager>();
            var convManager = convState.PendingConversationManager;
            
            // Look for decline option
            var choices = await convManager.GetAvailableChoices();
            var declineChoice = choices.FirstOrDefault(c => 
                c.ChoiceID.Contains("decline") || 
                c.NarrativeText.ToLower().Contains("sorry") ||
                c.NarrativeText.ToLower().Contains("can't"));
            
            if (declineChoice != null)
            {
                Console.WriteLine($"Selecting decline: {declineChoice.NarrativeText}");
                await convManager.MakeChoice(declineChoice);
            }
            
            convState.ClearPendingConversation();
            
            // Assert still in tutorial with no letter
            TutorialAssertions.AssertTutorialActive(GameWorld);
            GameWorldAssertions.AssertQueueSize(GameWorld, 0);
            
            Console.WriteLine("✅ Can decline tutorial letter and remain in tutorial");
        }
    }
}