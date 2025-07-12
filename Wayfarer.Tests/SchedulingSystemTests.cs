using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests for the Period-Based Activity Planning scheduling system.
    /// Validates NPC availability, market schedules, transport departure times, and time pressure mechanics.
    /// </summary>
    public class SchedulingSystemTests
    {
        [Fact]
        public void NPC_Schedule_Should_Restrict_Market_Availability()
        {
            // === SETUP ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("town_square")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Night));  // NPCs not available at night

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Manually add NPC for scheduling test
            var trader = new NPC
            {
                ID = "trader_elena",
                Name = "Elena the Trader",
                Location = "town_square",
                Profession = Professions.Merchant,
                AvailabilitySchedule = Schedule.Market_Days,
                ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade }
            };
            
            // Add NPC to world state using proper method
            gameWorld.WorldState.AddCharacter(trader);
            
            // Create managers
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            NPCRepository npcRepository = new NPCRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractProgressionService contractProgression = new ContractProgressionService(
                new ContractRepository(gameWorld), itemRepository, locationRepository);
            
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression, npcRepository);

            // === TEST NIGHT TIME (Market Closed) ===
            string nightStatus = marketManager.GetMarketAvailabilityStatus("town_square");
            Assert.Contains("opens at", nightStatus.ToLower());  // Market should be closed with next opening time
            
            // === CHANGE TO MORNING (Market Open) ===
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
            string morningStatus = marketManager.GetMarketAvailabilityStatus("town_square");
            Assert.Contains("open", morningStatus.ToLower());  // Market should be open
        }
        
        [Fact]
        public void Transport_Departure_Times_Should_Restrict_Route_Availability()
        {
            // === SETUP ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("dusty_flagon")
                    .WithCoins(100))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Afternoon));  // Express coach departs in Morning only

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Manually add route with departure time for testing
            var expressRoute = new RouteOption
            {
                Id = "express_coach",
                Name = "Express Coach",
                Origin = "dusty_flagon",
                Destination = "town_square",
                Method = TravelMethods.Carriage,
                DepartureTime = TimeBlocks.Morning,
                BaseCoinCost = 8,
                BaseStaminaCost = 0,
                TimeBlockCost = 1,
                MaxItemCapacity = 10,
                TerrainCategories = new List<TerrainCategory>()
            };
            
            // Add route to location connections
            var dustyFlagon = gameWorld.WorldState.locations.First(l => l.Id == "dusty_flagon");
            var connection = dustyFlagon.Connections.FirstOrDefault(c => c.DestinationLocationId == "town_square");
            if (connection == null)
            {
                connection = new LocationConnection
                {
                    DestinationLocationId = "town_square",
                    RouteOptions = new List<RouteOption>()
                };
                dustyFlagon.Connections.Add(connection);
            }
            connection.RouteOptions.Add(expressRoute);
            
            // Create managers
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            TransportCompatibilityValidator transportValidator = new TransportCompatibilityValidator(itemRepository);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);
            TravelManager travelManager = new TravelManager(gameWorld, 
                new LocationSystem(gameWorld, locationRepository),
                actionRepository,
                locationRepository,
                actionFactory,
                itemRepository,
                new ContractProgressionService(contractRepository, itemRepository, locationRepository),
                transportValidator);

            // === TEST AFTERNOON (Express Coach Not Available) ===
            List<RouteOption> afternoonRoutes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            bool expressAvailable = afternoonRoutes.Any(r => r.Id == "express_coach");
            Assert.False(expressAvailable, "Express coach should not be available in afternoon");
            
            // === CHANGE TO MORNING (Express Coach Available) ===
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
            List<RouteOption> morningRoutes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            bool expressAvailableMorning = morningRoutes.Any(r => r.Id == "express_coach");
            Assert.True(expressAvailableMorning, "Express coach should be available in morning");
        }
        
        [Fact]
        public void Time_Blocks_Should_Create_Daily_Activity_Pressure()
        {
            // === SETUP ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("dusty_flagon")
                    .WithCoins(100)
                    .WithStamina(10))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning)
                    .UsedTimeBlocks(0));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

            // === TEST INITIAL STATE ===
            TimeManager timeManager = gameWorld.TimeManager;
            Assert.Equal(5, timeManager.RemainingTimeBlocks);  // All time blocks available
            
            // === CONSUME TIME BLOCKS ===
            timeManager.ConsumeTimeBlock(2);
            Assert.Equal(3, timeManager.RemainingTimeBlocks);  // 3 remaining after using 2
            
            // === TEST TIME PRESSURE ===
            timeManager.ConsumeTimeBlock(3);
            Assert.Equal(0, timeManager.RemainingTimeBlocks);  // No time blocks remaining
            
            // === TEST TIME PRESSURE VALIDATION ===
            // When all time blocks are consumed, further actions should be blocked
            // This creates the strategic pressure that players must manage their time
            Assert.Throws<InvalidOperationException>(() => timeManager.ConsumeTimeBlock(1));
        }
    }
}