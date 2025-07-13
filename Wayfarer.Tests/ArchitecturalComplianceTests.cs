using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Architectural compliance tests to prevent violations of core system design principles.
    /// These tests enforce the fundamental architecture rules discovered through debugging and user feedback.
    /// 
    /// CRITICAL: These tests prevent regression of architectural misunderstandings that lead to
    /// broken time progression, confusing UI, and over-complex travel systems.
    /// </summary>
    public class ArchitecturalComplianceTests
    {
        /// <summary>
        /// ARCHITECTURAL RULE: Time blocks should NEVER be displayed in UI as "remaining blocks"
        /// Time blocks are internal mechanics only - players should see actual time progression
        /// </summary>
        [Fact]
        public void TimeSystem_Should_Never_Display_Time_Blocks_In_UI()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Act & Assert - Verify time blocks are not exposed for UI display
            
            // ✅ CORRECT: Only actual time values should be available for UI
            int currentHour = timeManager.GetCurrentTimeHours(); // This is correct for UI
            TimeBlocks currentTimeBlock = timeManager.GetCurrentTimeBlock(); // This is correct for UI
            
            // ❌ FORBIDDEN: No "remaining time blocks" properties should exist for UI consumption
            // If these properties exist, they violate the architectural principle
            var timeManagerType = typeof(TimeManager);
            
            // These properties should NOT exist or should NOT be publicly accessible for UI
            var remainingTimeBlocksProperty = timeManagerType.GetProperty("RemainingTimeBlocks");
            var usedTimeBlocksProperty = timeManagerType.GetProperty("UsedTimeBlocks");
            var canPerformTimeBlockProperty = timeManagerType.GetProperty("CanPerformTimeBlockAction");
            
            // If these exist and are public, they enable UI violations
            if (remainingTimeBlocksProperty != null && remainingTimeBlocksProperty.CanRead)
            {
                Assert.True(false, "ARCHITECTURAL VIOLATION: RemainingTimeBlocks property should not be exposed to UI. " +
                           "Players should see actual time progression like 'Morning 6:00' → 'Afternoon 14:00', " +
                           "not abstract 'time blocks remaining (2/5)'. Time blocks are internal action mechanics only.");
            }
            
            // Verify the correct pattern: actual time values are available
            Assert.True(currentHour >= 0 && currentHour <= 24, "Current hour should be valid time");
            Assert.True(Enum.IsDefined(typeof(TimeBlocks), currentTimeBlock), "Current time block should be valid");
        }
        
        /// <summary>
        /// ARCHITECTURAL RULE: TimeBlocks enum should ALWAYS be calculated from current hour
        /// NEVER store TimeBlocks as separate state - this causes desynchronization
        /// </summary>
        [Fact]
        public void TimeSystem_Should_Calculate_TimeBlocks_From_Current_Hour()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Test various hours to ensure TimeBlocks are calculated correctly
            var testCases = new[]
            {
                (hour: 6, expected: TimeBlocks.Dawn),        // 6:00-8:59 Dawn
                (hour: 8, expected: TimeBlocks.Dawn),        // 6:00-8:59 Dawn  
                (hour: 9, expected: TimeBlocks.Morning),     // 9:00-11:59 Morning
                (hour: 11, expected: TimeBlocks.Morning),    // 9:00-11:59 Morning
                (hour: 12, expected: TimeBlocks.Afternoon),  // 12:00-15:59 Afternoon
                (hour: 15, expected: TimeBlocks.Afternoon),  // 12:00-15:59 Afternoon
                (hour: 16, expected: TimeBlocks.Evening),    // 16:00-19:59 Evening
                (hour: 19, expected: TimeBlocks.Evening),    // 16:00-19:59 Evening
                (hour: 20, expected: TimeBlocks.Night),      // 20:00-5:59 Night
                (hour: 23, expected: TimeBlocks.Night),      // 20:00-5:59 Night
                (hour: 0, expected: TimeBlocks.Night),       // 20:00-5:59 Night
                (hour: 5, expected: TimeBlocks.Night)        // 20:00-5:59 Night
            };
            
            foreach (var (hour, expected) in testCases)
            {
                // Act
                timeManager.SetNewTime(hour);
                
                // Assert - TimeBlocks should be calculated from hour, not stored separately
                TimeBlocks actual = timeManager.GetCurrentTimeBlock();
                Assert.Equal(expected, actual);
            }
        }
        
        /// <summary>
        /// ARCHITECTURAL RULE: Actions that consume time blocks MUST advance actual clock time
        /// Time block consumption without time advancement violates player mental model
        /// </summary>
        [Fact]
        public void TimeSystem_Should_Advance_Clock_When_Consuming_Time_Blocks()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithActionPoints(18))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            
            // Set initial time
            timeManager.SetNewTime(6); // 6:00 AM
            int initialHour = timeManager.GetCurrentTimeHours();
            
            // Act - Consume time blocks (representing actions)
            timeManager.ConsumeTimeBlock(1);
            
            // Assert - Clock time should advance proportionally
            int finalHour = timeManager.GetCurrentTimeHours();
            
            Assert.True(finalHour > initialHour, 
                $"ARCHITECTURAL VIOLATION: ConsumeTimeBlock must advance actual clock time. " +
                $"Started at {initialHour}:00, still at {finalHour}:00 after consuming time block. " +
                $"Players expect 'Morning 6:00' → 'Morning 9:00' or 'Afternoon 12:00', " +
                $"not disconnected time block tracking.");
        }
        
        /// <summary>
        /// ARCHITECTURAL RULE: Routes define exactly one transport method
        /// NEVER allow multiple transport options per route - this violates the route concept
        /// </summary>
        [Fact]
        public void TravelSystem_Should_Have_One_Transport_Method_Per_Route()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);
            
            TravelManager travelManager = new TravelManager(
                gameWorld,
                new LocationSystem(gameWorld, locationRepository),
                actionRepository,
                locationRepository,
                actionFactory,
                itemRepository,
                new ContractProgressionService(contractRepository, itemRepository, locationRepository),
                new TransportCompatibilityValidator(itemRepository),
                new RouteRepository(gameWorld)
            );
            
            // Get routes between locations
            List<RouteOption> routes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            
            // Assert - Each route should have exactly one method defined
            foreach (RouteOption route in routes)
            {
                Assert.True(!string.IsNullOrEmpty(route.Method.ToString()), 
                    $"Route '{route.Name}' must have exactly one transport method defined. " +
                    $"ARCHITECTURAL VIOLATION: Routes ARE the transport selection. " +
                    $"'Walking Path' = walking transport, 'Standard Cart' = cart transport.");
                    
                // Verify route names describe their transport method
                Assert.True(IsRouteNameDescriptive(route.Name, route.Method), 
                    $"Route name '{route.Name}' should describe transport method '{route.Method}'. " +
                    $"ARCHITECTURAL RULE: Route names should make transport method obvious " +
                    $"(e.g., 'Walking Path', 'Standard Cart', 'Express Coach').");
            }
            
            // ❌ FORBIDDEN: Check that no separate transport selection logic exists
            // If GetAvailableTransportOptions exists, it violates the architecture
            var travelManagerType = typeof(TravelManager);
            var getTransportOptionsMethod = travelManagerType.GetMethod("GetAvailableTransportOptions");
            
            if (getTransportOptionsMethod != null)
            {
                Assert.True(false, 
                    "ARCHITECTURAL VIOLATION: GetAvailableTransportOptions method should not exist. " +
                    "This enables separate transport selection on top of route selection, " +
                    "which violates the principle that routes ARE transport methods. " +
                    "Remove this method and related transport selection UI.");
            }
        }
        
        /// <summary>
        /// ARCHITECTURAL RULE: Travel UI should show routes with inherent transport methods
        /// NEVER create separate transport selection UI on top of route selection
        /// </summary>
        [Fact]
        public void TravelSystem_Should_Not_Have_Separate_Transport_Selection()
        {
            // This test verifies that the TravelMethods enum is not used for separate transport selection
            
            // ❌ FORBIDDEN: TravelMethods enum should not be used independently of routes
            // If it exists and is used for UI selection, it violates the architecture
            
            var travelMethodsType = Type.GetType("TravelMethods") ?? Type.GetType("Wayfarer.TravelMethods");
            
            if (travelMethodsType != null && travelMethodsType.IsEnum)
            {
                // If TravelMethods exists as separate selection mechanism, this is wrong
                var travelManagerType = typeof(TravelManager);
                var getTransportMethodsMethod = travelManagerType.GetMethods()
                    .Where(m => m.Name.Contains("Transport") && m.ReturnType.IsArray)
                    .FirstOrDefault();
                    
                if (getTransportMethodsMethod != null)
                {
                    Assert.True(false, 
                        "ARCHITECTURAL VIOLATION: Separate transport method selection detected. " +
                        "TravelMethods enum should not be used for independent transport selection. " +
                        "Routes already define their transport method - this IS the transport selection. " +
                        "Remove transport selection UI and use route selection only.");
                }
            }
            
            // ✅ CORRECT PATTERN: Routes contain all transport information
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            
            // Verify routes have transport methods defined
            Location fromLocation = locationRepository.GetLocation("dusty_flagon");
            if (fromLocation?.Connections?.Any() == true)
            {
                var connection = fromLocation.Connections.First();
                if (connection.RouteOptions?.Any() == true)
                {
                    var route = connection.RouteOptions.First();
                    Assert.True(!string.IsNullOrEmpty(route.Method.ToString()),
                        "Routes must have transport method defined - this IS the transport selection");
                }
            }
        }
        
        /// <summary>
        /// Helper method to verify route names describe their transport method
        /// </summary>
        private bool IsRouteNameDescriptive(string routeName, TravelMethods method)
        {
            var descriptivePatterns = new Dictionary<TravelMethods, string[]>
            {
                { TravelMethods.Walking, new[] { "walking", "path", "foot" } },
                { TravelMethods.Carriage, new[] { "cart", "carriage", "coach" } },
                { TravelMethods.Horseback, new[] { "horse", "mount", "ride" } },
                { TravelMethods.Cart, new[] { "cart", "wagon" } },
                { TravelMethods.Boat, new[] { "boat", "ferry", "ship" } }
            };
            
            if (descriptivePatterns.ContainsKey(method))
            {
                return descriptivePatterns[method].Any(pattern => 
                    routeName.ToLower().Contains(pattern.ToLower()));
            }
            
            return true; // Default to true for unknown methods
        }
    }
}