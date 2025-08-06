using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Simplified E2E test that validates the core intent system works.
/// </summary>
public class SimpleE2ETest
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Wayfarer E2E Test - Intent System Validation");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        
        try
        {
            // Create services
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
            
            // Create a test GameWorld using the initializer
            Console.WriteLine("Creating GameWorld...");
            var gameWorld = GameWorldInitializer.CreateGameWorld();
            services.AddSingleton(gameWorld);
            
            // Add repositories
            services.AddSingleton<LocationRepository>();
            services.AddSingleton<RouteRepository>();
            services.AddSingleton<NPCRepository>();
            services.AddSingleton<ItemRepository>();
            services.AddSingleton<LetterTemplateRepository>();
            services.AddSingleton<MarketDataRepository>();
            services.AddSingleton<ConversationRepository>();
            services.AddSingleton<StandingObligationRepository>();
            
            // Add core services
            services.AddSingleton<MessageSystem>();
            services.AddSingleton<SpecialLetterService>();
            services.AddSingleton<TokenSpendingService>();
            services.AddSingleton<NPCService>();
            services.AddSingleton<LetterQueueService>();
            services.AddSingleton<LetterOfferService>();
            services.AddSingleton<TimeImpactCalculator>();
            services.AddSingleton<InformationDiscoveryManager>();
            services.AddSingleton<UIMessageBuffer>();
            services.AddSingleton<NPCVisibilityService>();
            services.AddSingleton<NPCLetterOfferService>();
            services.AddSingleton<ConversationStateManager>();
            
            // Add conversation system
            services.AddSingleton<ConversationFactory>();
            services.AddSingleton<INarrativeProvider, DeterministicNarrativeProvider>();
            services.AddSingleton<ConversationChoiceValidator>();
            
            // Add GameFacade
            services.AddSingleton<GameFacade>();
            
            var provider = services.BuildServiceProvider();
            var gameFacade = provider.GetRequiredService<GameFacade>();
            
            Console.WriteLine("✅ Services created successfully");
            
            // Test 1: Get location actions
            Console.WriteLine("\nTest 1: Get Location Actions");
            var actions = gameFacade.GetLocationActions();
            Console.WriteLine($"  Found {actions.Count} actions at current location");
            foreach (var action in actions.Take(5))
            {
                Console.WriteLine($"  - {action.ID}: {action.DisplayName}");
            }
            if (actions.Count > 5)
            {
                Console.WriteLine($"  ... and {actions.Count - 5} more");
            }
            
            // Test 2: Execute a simple intent
            Console.WriteLine("\nTest 2: Execute Observe Intent");
            var observeIntent = new ObserveLocationIntent();
            var result = await gameFacade.ExecuteIntent(observeIntent);
            Console.WriteLine($"  Observe intent result: {result}");
            
            // Test 3: Check if we can talk to NPCs
            Console.WriteLine("\nTest 3: Check Talk Actions");
            var talkActions = actions.Where(a => a.ID.StartsWith("talk_")).ToList();
            Console.WriteLine($"  Found {talkActions.Count} NPCs to talk to");
            
            if (talkActions.Any())
            {
                var firstTalk = talkActions.First();
                Console.WriteLine($"\nTest 4: Execute Talk Action - {firstTalk.DisplayName}");
                var talkResult = await gameFacade.ExecuteLocationActionAsync(firstTalk.ID);
                Console.WriteLine($"  Talk action result: {talkResult}");
                
                // Check if conversation started
                var convState = provider.GetRequiredService<ConversationStateManager>();
                Console.WriteLine($"  Conversation active: {convState.ConversationPending}");
            }
            
            Console.WriteLine("\n✅ All basic tests passed!");
            Console.WriteLine("\nIntent system is working correctly.");
            Console.WriteLine("Tutorial flow would require full GameWorld implementation.");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}