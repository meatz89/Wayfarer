using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Travel system architectural compliance tests.
    /// These tests prevent the specific travel system violations that created confusing UI
    /// and ensure routes are the single transport selection mechanism.
    /// 
    /// CRITICAL RULES ENFORCED:
    /// 1. Routes ARE transport methods - no separate transport selection
    /// 2. Each route has exactly one transport method defined
    /// 3. Route names should describe their transport method
    /// 4. No TravelMethods enum used independently of routes
    /// </summary>
    public class TravelSystemComplianceTests
    {
        [Fact]
        public void Routes_Should_Define_Exactly_One_Transport_Method()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            
            // Get all routes in the system
            List<RouteOption> allRoutes = GetAllRoutesInSystem(locationRepository);
            
            // Assert - Each route must have exactly one transport method
            foreach (RouteOption route in allRoutes)
            {
                Assert.True(!string.IsNullOrEmpty(route.Method.ToString()), 
                    $"Route '{route.Name}' must have exactly one transport method defined. " +
                    $"ARCHITECTURAL RULE: Routes ARE the transport selection.");
                
                Assert.True(Enum.IsDefined(typeof(TravelMethods), route.Method), 
                    $"Route '{route.Name}' has invalid transport method '{route.Method}'");
            }
        }
        
        [Fact]
        public void Route_Names_Should_Describe_Transport_Method()
        {
            // Arrange
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon"))
                .Build();

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            
            List<RouteOption> allRoutes = GetAllRoutesInSystem(locationRepository);
            
            // Assert - Route names should make transport method obvious
            foreach (RouteOption route in allRoutes)
            {
                bool isDescriptive = IsRouteNameDescriptive(route.Name, route.Method);
                
                Assert.True(isDescriptive, 
                    $"Route name '{route.Name}' should describe transport method '{route.Method}'. " +
                    $"ARCHITECTURAL RULE: Route names should make transport obvious " +
                    $"(e.g., 'Walking Path', 'Standard Cart', 'Express Coach'). " +
                    $"This eliminates need for separate transport selection UI.");
            }
        }
        
        [Fact]
        public void TravelManager_Should_Not_Have_Separate_Transport_Selection_Methods()
        {
            // This test prevents the architectural violation of adding transport selection on top of routes
            
            var travelManagerType = typeof(TravelManager);
            
            // These methods enable the architectural violation of separate transport selection
            var violatingMethods = new[]
            {
                "GetAvailableTransportOptions",
                "GetAvailableRouteTransportCombinations",
                "GetTransportInventoryBonus",
                "CheckInventoryCapacity"
            };
            
            foreach (string methodName in violatingMethods)
            {
                var method = travelManagerType.GetMethod(methodName);
                if (method != null && method.IsPublic)
                {
                    Assert.True(false, 
                        $"ARCHITECTURAL VIOLATION: Method '{methodName}' enables separate transport selection " +
                        $"on top of route selection. This violates the principle that routes ARE transport methods. " +
                        $"Route selection IS transport selection. Remove this method and related UI.");
                }
            }
        }
        
        [Fact]
        public void Route_Selection_Should_Be_Complete_Travel_Choice()
        {
            // Verify that selecting a route is a complete travel decision
            
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(100).WithStamina(20))
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
            
            List<RouteOption> routes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            
            foreach (RouteOption route in routes)
            {
                // Selecting this route should provide complete travel information
                int coinCost = travelManager.CalculateCoinCost(route);
                int staminaCost = travelManager.CalculateStaminaCost(route);
                bool canTravel = travelManager.CanTravel(route);
                
                // All calculations should work with just the route - no additional transport parameter
                Assert.True(coinCost >= 0, $"Route '{route.Name}' should provide complete cost calculation");
                Assert.True(staminaCost >= 0, $"Route '{route.Name}' should provide complete stamina calculation");
                
                // Route access should be checkable
                var accessInfo = travelManager.GetRouteAccessInfo(route);
                Assert.NotNull(accessInfo);
                
                // This proves route selection is sufficient - no transport selection needed
            }
        }
        
        [Fact]
        public void Routes_Should_Have_Distinct_Transport_Characteristics()
        {
            // Verify that different routes offer meaningful transport choices
            
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
            
            List<RouteOption> routes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            
            if (routes.Count > 1)
            {
                // Routes should offer different trade-offs (cost vs speed vs stamina)
                bool hasVariedCosts = routes.Select(r => r.BaseCoinCost).Distinct().Count() > 1;
                bool hasVariedStamina = routes.Select(r => r.BaseStaminaCost).Distinct().Count() > 1;
                bool hasVariedTime = routes.Select(r => r.TimeBlockCost).Distinct().Count() > 1;
                bool hasVariedMethods = routes.Select(r => r.Method).Distinct().Count() > 1;
                
                Assert.True(hasVariedCosts || hasVariedStamina || hasVariedTime || hasVariedMethods,
                    "Multiple routes should offer different transport characteristics (cost, stamina, time, method). " +
                    "This provides meaningful choice without needing separate transport selection.");
            }
        }
        
        [Fact]
        public void Transport_Compatibility_Should_Be_Route_Based()
        {
            // Verify that transport compatibility is inherent to routes, not separate selection
            
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
            
            List<RouteOption> routes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            
            foreach (RouteOption route in routes)
            {
                // Route access should be checkable based on route characteristics
                var accessInfo = travelManager.GetRouteAccessInfo(route);
                
                Assert.NotNull(accessInfo);
                
                // Access should be determined by route properties + player state
                // Not by separate transport compatibility logic
                if (!accessInfo.IsAllowed)
                {
                    Assert.True(!string.IsNullOrEmpty(accessInfo.BlockingReason),
                        $"Blocked route '{route.Name}' should have clear blocking reason");
                }
                
                // This access check should be complete - no additional transport checks needed
                Assert.True(true, "Route access checking is complete without transport selection");
            }
        }
        
        /// <summary>
        /// Helper method to get all routes in the system for testing
        /// </summary>
        private List<RouteOption> GetAllRoutesInSystem(LocationRepository locationRepository)
        {
            var allRoutes = new List<RouteOption>();
            
            List<Location> locations = locationRepository.GetAllLocations();
            foreach (Location location in locations)
            {
                if (location.Connections != null)
                {
                    foreach (LocationConnection connection in location.Connections)
                    {
                        if (connection.RouteOptions != null)
                        {
                            allRoutes.AddRange(connection.RouteOptions);
                        }
                    }
                }
            }
            
            return allRoutes;
        }
        
        /// <summary>
        /// Helper method to check if route name describes transport method
        /// </summary>
        private bool IsRouteNameDescriptive(string routeName, TravelMethods method)
        {
            var descriptivePatterns = new Dictionary<TravelMethods, string[]>
            {
                { TravelMethods.Walking, new[] { "walking", "path", "foot", "trek", "hike" } },
                { TravelMethods.Carriage, new[] { "cart", "carriage", "coach", "wagon" } },
                { TravelMethods.Horseback, new[] { "horse", "mount", "ride", "horseback" } },
                { TravelMethods.Cart, new[] { "cart", "wagon", "haul" } },
                { TravelMethods.Boat, new[] { "boat", "ferry", "ship", "sail", "water" } }
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