using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.E2ETests.Infrastructure
{
    /// <summary>
    /// Base class for all E2E tests. Provides setup/teardown and helper methods.
    /// </summary>
    public abstract class E2ETestBase : IDisposable
    {
        protected GameWorld GameWorld { get; private set; }
        protected GameFacade GameFacade { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }
        protected Player Player => GameWorld.Player;
        
        /// <summary>
        /// Sets up a fresh GameWorld and services for each test.
        /// </summary>
        protected async Task SetupAsync(bool startTutorial = true)
        {
            // Create test GameWorld
            GameWorld = await TestGameWorldFactory.CreateTestGameWorld(startTutorial);
            
            // Create service provider
            ServiceProvider = TestServiceProvider.CreateServiceProvider(GameWorld);
            
            // Get GameFacade
           GameFacade = ServiceProvider.GetRequiredService<GameFacade>();
            
            Console.WriteLine($"[TEST] Setup complete. Player at {Player.CurrentLocationId}/{Player.CurrentSpotId}");
        }
        
        /// <summary>
        /// Executes an intent and returns the result.
        /// </summary>
        protected async Task<bool> ExecuteIntent(PlayerIntent intent)
        {
            Console.WriteLine($"[TEST] Executing intent: {intent.GetType().Name}");
            bool result = await GameFacade.ExecuteIntent(intent);
            Console.WriteLine($"[TEST] Intent result: {result}");
            return result;
        }
        
        /// <summary>
        /// Gets available actions at the current location.
        /// </summary>
        protected List<LocationAction> GetLocationActions()
        {
            return GameFacade.GetLocationActions();
        }
        
        /// <summary>
        /// Executes a location action by ID.
        /// </summary>
        protected async Task<bool> ExecuteLocationAction(string actionId)
        {
            Console.WriteLine($"[TEST] Executing location action: {actionId}");
            return await GameFacade.ExecuteLocationActionAsync(actionId);
        }
        
        /// <summary>
        /// Advances time by the specified hours.
        /// </summary>
        protected void AdvanceTime(int hours)
        {
            GameWorld.TimeManager.AdvanceTime(hours);
            Console.WriteLine($"[TEST] Advanced time by {hours} hours. Now Day {GameWorld.TimeManager.CurrentDay}, Hour {GameWorld.TimeManager.CurrentHour}");
        }
        
        /// <summary>
        /// Gets an NPC by ID.
        /// </summary>
        protected NPC GetNPC(string npcId)
        {
            NPCRepository npcRepo = ServiceProvider.GetRequiredService<NPCRepository>();
            return npcRepo.GetNPC(npcId);
        }
        
        /// <summary>
        /// Gets a location by ID.
        /// </summary>
        protected Location GetLocation(string locationId)
        {
            LocationRepository locationRepo = ServiceProvider.GetRequiredService<LocationRepository>();
            return locationRepo.GetLocation(locationId);
        }
        
        /// <summary>
        /// Gets the current conversation manager if a conversation is active.
        /// </summary>
        protected ConversationManager GetActiveConversation()
        {
            ConversationStateManager convState = ServiceProvider.GetRequiredService<ConversationStateManager>();
            return convState.PendingConversationManager;
        }
        
        /// <summary>
        /// Cleans up resources after each test.
        /// </summary>
        public void Dispose()
        {
            // Clear static service locator
            ServiceLocator.SetServiceProvider(null);
            
            // Clear any other static state if needed
            GameWorld = null;
           GameFacade = null;
            ServiceProvider = null;
            
            Console.WriteLine("[TEST] Cleanup complete");
        }
    }
}