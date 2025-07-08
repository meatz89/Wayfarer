using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Test-driven development for Route Condition Variations feature.
    /// These tests validate that route availability changes based on time, weather, and other conditions,
    /// creating strategic depth without automated optimization suggestions.
    /// 
    /// User Story: Route Condition Variations
    /// Priority: MEDIUM - Strategic depth through changing conditions
    /// Game Design Goal: Add adaptation challenges and information gathering gameplay
    /// </summary>
    public class RouteConditionVariationsTests
    {
        /// <summary>
        /// Test that routes have time-of-day restrictions
        /// Acceptance Criteria: Some routes only available during specific time windows
        /// </summary>
        [Fact]
        public void Route_Should_Respect_TimeOfDay_Restrictions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            // Create a route only available during morning
            RouteOption morningRoute = CreateTestRoute("morning_ferry", TimeBlocks.Morning);
            AddRouteToLocation(gameWorld, "town_square", "dusty_flagon", morningRoute);
            
            // Act & Assert - Available during morning
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Morning;
            List<RouteOption> morningRoutes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");
            Assert.Contains(morningRoute, morningRoutes);
            
            // Act & Assert - Not available during afternoon
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Afternoon;
            List<RouteOption> afternoonRoutes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");
            Assert.DoesNotContain(morningRoute, afternoonRoutes);
        }

        /// <summary>
        /// Test that routes have weather-based modifications
        /// Acceptance Criteria: Weather conditions affect route costs and availability
        /// </summary>
        [Fact]
        public void Route_Should_Modify_Costs_Based_On_Weather()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            RouteOption route = CreateTestRoute("forest_path", baseCost: 5, baseStamina: 2);
            route.WeatherModifications = new Dictionary<WeatherCondition, RouteModification>
            {
                { WeatherCondition.Rain, new RouteModification { StaminaCostModifier = 1, CoinCostModifier = 0 } },
                { WeatherCondition.Snow, new RouteModification { StaminaCostModifier = 2, CoinCostModifier = 1 } }
            };
            
            // Act & Assert - Clear weather (no modification)
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;
            int clearStamina = travelManager.CalculateStaminaCost(route);
            Assert.Equal(2, clearStamina);
            
            // Act & Assert - Rain increases stamina cost
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Rain;
            int rainStamina = travelManager.CalculateStaminaCost(route);
            Assert.Equal(3, rainStamina); // 2 + 1
            
            // Act & Assert - Snow increases both stamina and coin cost
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Snow;
            int snowStamina = travelManager.CalculateStaminaCost(route);
            int snowCoinCost = travelManager.CalculateCoinCost(route);
            Assert.Equal(4, snowStamina); // 2 + 2
            Assert.Equal(6, snowCoinCost); // 5 + 1
        }

        /// <summary>
        /// Test that routes have seasonal availability
        /// Acceptance Criteria: Some routes only available during specific seasons
        /// </summary>
        // REMOVED: Seasonal availability test
        // Seasons are not part of the game - timeframe is only days/weeks, not months/seasons
        [Fact]
        public void Route_Should_Be_Available_Without_Seasonal_Restrictions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            // All routes should be available without seasonal restrictions
            RouteOption testRoute = CreateTestRoute("test_route");
            AddRouteToLocation(gameWorld, "town_square", "destination", testRoute);
            
            // Act & Assert - Route should always be available
            List<RouteOption> routes = travelManager.GetAvailableRoutes("town_square", "destination");
            Assert.Contains(testRoute, routes);
        }

        /// <summary>
        /// Test that routes can be temporarily blocked by events
        /// Acceptance Criteria: Dynamic events can temporarily block routes
        /// </summary>
        [Fact]
        public void Route_Should_Be_Blocked_By_Temporary_Events()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            RouteOption bridgeRoute = CreateTestRoute("stone_bridge");
            AddRouteToLocation(gameWorld, "town_square", "northern_village", bridgeRoute);
            
            // Initially available
            List<RouteOption> initialRoutes = travelManager.GetAvailableRoutes("town_square", "northern_village");
            Assert.Contains(bridgeRoute, initialRoutes);
            
            // Act - Bridge collapses (temporary event)
            gameWorld.WorldState.AddTemporaryRouteBlock("stone_bridge", 3); // Blocked for 3 days
            
            // Assert - Route blocked
            List<RouteOption> blockedRoutes = travelManager.GetAvailableRoutes("town_square", "northern_village");
            Assert.DoesNotContain(bridgeRoute, blockedRoutes);
            
            // Act - Time passes (3 days later)
            gameWorld.WorldState.CurrentDay += 3;
            
            // Assert - Route available again
            List<RouteOption> recoveredRoutes = travelManager.GetAvailableRoutes("town_square", "northern_village");
            Assert.Contains(bridgeRoute, recoveredRoutes);
        }

        /// <summary>
        /// Test that routes unlock through repeated usage
        /// Acceptance Criteria: Using routes reveals new path variations
        /// </summary>
        [Fact]
        public void Route_Should_Unlock_Variants_Through_Usage()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            RouteOption basicRoute = CreateTestRoute("forest_path");
            RouteOption hiddenRoute = CreateTestRoute("hidden_shortcut");
            hiddenRoute.IsDiscovered = false;
            hiddenRoute.UnlockCondition = new RouteUnlockCondition
            {
                RequiredRouteUsage = "forest_path",
                RequiredUsageCount = 3
            };
            
            AddRouteToLocation(gameWorld, "town_square", "forest_village", basicRoute);
            AddRouteToLocation(gameWorld, "town_square", "forest_village", hiddenRoute);
            
            // Initially only basic route available
            List<RouteOption> initialRoutes = travelManager.GetAvailableRoutes("town_square", "forest_village");
            Assert.Contains(basicRoute, initialRoutes);
            Assert.DoesNotContain(hiddenRoute, initialRoutes);
            
            // Act - Use basic route 3 times
            for (int i = 0; i < 3; i++)
            {
                travelManager.RecordRouteUsage("forest_path");
            }
            
            // Assert - Hidden route now discovered
            List<RouteOption> discoveredRoutes = travelManager.GetAvailableRoutes("town_square", "forest_village");
            Assert.Contains(basicRoute, discoveredRoutes);
            Assert.Contains(hiddenRoute, discoveredRoutes);
        }

        /// <summary>
        /// Test that route conditions affect player decision-making
        /// Acceptance Criteria: Players must adapt strategies based on changing conditions
        /// </summary>
        [Fact]
        public void Route_Conditions_Should_Create_Strategic_Decisions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            // Fast but weather-dependent route
            RouteOption fastRoute = CreateTestRoute("river_crossing", baseCost: 10, baseStamina: 1, timeBlocks: 1);
            fastRoute.WeatherModifications = new Dictionary<WeatherCondition, RouteModification>
            {
                { WeatherCondition.Rain, new RouteModification { StaminaCostModifier = 5 } } // Dangerous in rain
            };
            
            // Slow but reliable route
            RouteOption reliableRoute = CreateTestRoute("safe_road", baseCost: 5, baseStamina: 3, timeBlocks: 2);
            
            AddRouteToLocation(gameWorld, "town_square", "destination", fastRoute);
            AddRouteToLocation(gameWorld, "town_square", "destination", reliableRoute);
            
            // Act & Assert - Clear weather: fast route is better
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;
            List<RouteOption> clearRoutes = travelManager.GetAvailableRoutes("town_square", "destination");
            Assert.Contains(fastRoute, clearRoutes);
            Assert.Contains(reliableRoute, clearRoutes);
            
            int fastCostClear = travelManager.CalculateStaminaCost(fastRoute);
            int reliableCostClear = travelManager.CalculateStaminaCost(reliableRoute);
            Assert.True(fastCostClear < reliableCostClear); // Fast route is more efficient
            
            // Act & Assert - Rain: reliable route becomes better
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Rain;
            int fastCostRain = travelManager.CalculateStaminaCost(fastRoute);
            int reliableCostRain = travelManager.CalculateStaminaCost(reliableRoute);
            Assert.True(fastCostRain > reliableCostRain); // Reliable route is now more efficient
        }

        /// <summary>
        /// Test that route information is NOT automatically displayed
        /// Acceptance Criteria: Players must manually check conditions (no automated suggestions)
        /// </summary>
        [Fact]
        public void Route_System_Should_Not_Provide_Automated_Suggestions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            
            // Act - Get available routes
            List<RouteOption> routes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");
            
            // Assert - No automated optimization methods should exist
            Assert.False(HasMethod(travelManager, "GetOptimalRoute"));
            Assert.False(HasMethod(travelManager, "GetBestRouteForWeather"));
            Assert.False(HasMethod(travelManager, "GetRouteRecommendation"));
            Assert.False(HasMethod(travelManager, "GetRouteComparisonData"));
            
            // Players must manually inspect each route's properties
            foreach (RouteOption route in routes)
            {
                // These properties should be available for manual inspection
                Assert.NotNull(route.Name);
                Assert.True(route.BaseCoinCost >= 0);
                Assert.True(route.BaseStaminaCost >= 0);
                Assert.True(route.TimeBlockCost >= 0);
                
                // But no automated analysis should be provided
                Assert.False(HasProperty(route, "EfficiencyScore"));
                Assert.False(HasProperty(route, "Recommendation"));
                Assert.False(HasProperty(route, "OptimizationSuggestion"));
            }
        }

        /// <summary>
        /// Test that route discovery creates gameplay depth
        /// Acceptance Criteria: Players learn route patterns through experience
        /// </summary>
        [Fact]
        public void Route_Discovery_Should_Create_Learning_Gameplay()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            Player player = gameWorld.GetPlayer();
            
            RouteOption route = CreateTestRoute("forest_path");
            route.WeatherModifications = new Dictionary<WeatherCondition, RouteModification>
            {
                { WeatherCondition.Rain, new RouteModification { StaminaCostModifier = 2 } }
            };
            
            // Act - First time using route in rain (player doesn't know about penalty)
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Rain;
            int initialStamina = player.Stamina;
            int expectedCost = travelManager.CalculateStaminaCost(route);
            
            // Assert - Player experiences the actual cost (learning through gameplay)
            Assert.True(expectedCost > route.BaseStaminaCost);
            
            // Game should NOT tell player "this route is more expensive in rain"
            // Player must learn this through experience and remember it
            Assert.False(HasProperty(route, "WeatherWarning"));
            Assert.False(HasProperty(route, "ConditionAlert"));
            Assert.False(HasProperty(route, "CostExplanation"));
        }

        #region Helper Methods

        private GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            
            gameWorld.WorldState.CurrentDay = 1;
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Morning;
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;
            
            // Create and register the starting location
            Location startLocation = new Location("town_square", "Town Square");
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            locationRepository.AddLocation(startLocation);
            
            // Set player location - player must always be at exactly one location
            gameWorld.SetCurrentLocation(startLocation);
            
            return gameWorld;
        }

        private TravelManager CreateTravelManager(GameWorld gameWorld)
        {
            // This would normally be injected, but for testing we create a minimal version
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            
            return new TravelManager(
                gameWorld,
                locationSystem,
                actionRepository,
                locationRepository,
                actionFactory,
                itemRepository
            );
        }

        private RouteOption CreateTestRoute(string name, TimeBlocks? availableTime = null, int baseCost = 5, int baseStamina = 2, int timeBlocks = 1)
        {
            return new RouteOption
            {
                Id = name,
                Name = name,
                Origin = "town_square",
                Destination = "dusty_flagon",
                Method = TravelMethods.Walking,
                BaseCoinCost = baseCost,
                BaseStaminaCost = baseStamina,
                TimeBlockCost = timeBlocks,
                DepartureTime = availableTime,
                IsDiscovered = true
            };
        }

        private void AddRouteToLocation(GameWorld gameWorld, string fromLocationId, string toLocationId, RouteOption route)
        {
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            
            // Create test locations if they don't exist
            Location fromLocation = locationRepository.GetLocation(fromLocationId);
            if (fromLocation == null)
            {
                fromLocation = new Location(fromLocationId, "Test Location");
                locationRepository.AddLocation(fromLocation);
            }
            
            Location toLocation = locationRepository.GetLocation(toLocationId);
            if (toLocation == null)
            {
                toLocation = new Location(toLocationId, "Test Destination");
                locationRepository.AddLocation(toLocation);
            }
            
            // Add route connection
            LocationConnection connection = fromLocation.Connections.FirstOrDefault(c => c.DestinationLocationId == toLocationId);
            if (connection == null)
            {
                connection = new LocationConnection
                {
                    DestinationLocationId = toLocationId,
                    RouteOptions = new List<RouteOption>()
                };
                fromLocation.Connections.Add(connection);
            }
            connection.RouteOptions.Add(route);
        }

        private bool HasMethod(object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName) != null;
        }

        private bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        #endregion
    }

    // Note: WeatherCondition, RouteModification, and RouteUnlockCondition
    // are now defined in the main RouteOption.cs file
    // (Season removed - game timeframe is only days/weeks, not months/seasons)
}