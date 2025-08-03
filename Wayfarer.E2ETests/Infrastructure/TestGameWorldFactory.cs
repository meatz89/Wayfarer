using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wayfarer.E2ETests.Infrastructure
{
    /// <summary>
    /// Factory for creating fully initialized GameWorld instances for testing.
    /// Uses the real initialization pipeline to ensure test accuracy.
    /// </summary>
    public static class TestGameWorldFactory
    {
        public static async Task<GameWorld> CreateTestGameWorld(bool startTutorial = true)
        {
            Console.WriteLine("[TEST] Creating test GameWorld...");
            
            // Create GameWorld instance
            GameWorld gameWorld = new GameWorld();
            
            // Create initialization context with test content path
            string contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "Templates");
            InitializationContext context = new InitializationContext(contentPath, gameWorld);
            
            // Run initialization phases in order
            InitializationPhase[] phases = new InitializationPhase[]
            {
                new Phase1_Locations(),
                new Phase2_Routes(), 
                new Phase3_NPCDependents(),
                new Phase4_Conversations(),
                new Phase4_Narratives(),
                new Phase5_Items(),
                new Phase6_LetterEffects(),
                new Phase7_LetterSystem(),
                new Phase8_MarketSystem(),
                new Phase9_Obligations(),
                new Phase10_ContentValidation()
            };
            
            foreach (InitializationPhase phase in phases)
            {
                Console.WriteLine($"[TEST] Running {phase.Name}...");
                phase.Execute(context);
            }
            
            // Initialize repositories with loaded content
            InitializeRepositories(context);
            
            // Create and initialize player
            Player player = CreateTestPlayer(startTutorial);
            gameWorld.SetPlayer(player);
            
            // Initialize game state
            gameWorld.LetterQueueManager.Initialize(gameWorld);
            gameWorld.MorningActivitiesManager.Initialize(gameWorld);
            gameWorld.TimeManager.Initialize(new TimeState { CurrentDay = 1, CurrentHour = 8 });
            
            if (startTutorial)
            {
                // Set tutorial state
                gameWorld.TutorialState = new TutorialState 
                { 
                    IsActive = true,
                    CurrentStage = TutorialStage.Introduction
                };
            }
            
            Console.WriteLine("[TEST] GameWorld created successfully");
            return gameWorld;
        }
        
        private static void InitializeRepositories(InitializationContext context)
        {
            // Initialize LocationRepository
            LocationRepository locationRepo = ServiceLocator.GetService<LocationRepository>();
            if (context.SharedData.TryGetValue("Locations", out object locationsObj))
            {
                locationRepo.Initialize((List<Location>)locationsObj);
            }
            
            // Initialize RouteRepository
            RouteRepository routeRepo = ServiceLocator.GetService<RouteRepository>();
            if (context.SharedData.TryGetValue("Routes", out object routesObj))
            {
                routeRepo.Initialize((List<Route>)routesObj);
            }
            
            // Initialize NPCRepository
            NPCRepository npcRepo = ServiceLocator.GetService<NPCRepository>();
            if (context.SharedData.TryGetValue("NPCs", out object npcsObj))
            {
                npcRepo.Initialize((List<NPC>)npcsObj);
            }
            
            // Initialize ConversationRepository
            ConversationRepository convRepo = ServiceLocator.GetService<ConversationRepository>();
            if (context.SharedData.TryGetValue("Conversations", out object convsObj) &&
                context.SharedData.TryGetValue("NpcDialogues", out object dialoguesObj))
            {
                convRepo.Initialize(
                    (Dictionary<string, ConversationDefinition>)convsObj,
                    (Dictionary<string, string>)dialoguesObj
                );
            }
            
            // Initialize LetterTemplateRepository
            LetterTemplateRepository letterRepo = ServiceLocator.GetService<LetterTemplateRepository>();
            if (context.SharedData.TryGetValue("LetterTemplates", out object templatesObj))
            {
                letterRepo.Initialize((List<LetterTemplate>)templatesObj);
            }
            
            // Initialize ItemRepository
            ItemRepository itemRepo = ServiceLocator.GetService<ItemRepository>();
            if (context.SharedData.TryGetValue("Items", out object itemsObj))
            {
                itemRepo.Initialize((List<Item>)itemsObj);
            }
            
            // Initialize MarketDataRepository
            MarketDataRepository marketRepo = ServiceLocator.GetService<MarketDataRepository>();
            if (context.SharedData.TryGetValue("MarketData", out object marketObj))
            {
                marketRepo.Initialize((List<MarketData>)marketObj);
            }
        }
        
        private static Player CreateTestPlayer(bool inTutorial)
        {
            return new Player
            {
                Name = "Test Courier",
                CurrentLocationId = "lower_ward",
                CurrentSpotId = "lower_ward_square",
                Stamina = 10,
                MaxStamina = 10,
                Coins = inTutorial ? 0 : 10,
                TutorialActive = inTutorial,
                FocusPoints = 10,
                MaxFocusPoints = 10,
                Inventory = new List<InventoryItem>(),
                WornSeals = new SealSet(),
                Leverage = new LeverageInfo()
            };
        }
    }
}