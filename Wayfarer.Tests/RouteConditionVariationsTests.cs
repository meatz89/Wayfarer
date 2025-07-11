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
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
            List<RouteOption> morningRoutes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");
            Assert.Contains(morningRoute, morningRoutes);

            // Act & Assert - Not available during afternoon
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Afternoon;
            List<RouteOption> afternoonRoutes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");
            Assert.DoesNotContain(morningRoute, afternoonRoutes);
        }

        /// <summary>
        /// Test that weather interacts with terrain to create logical blocking/access conditions
        /// Acceptance Criteria: Weather-terrain interactions block access based on logical requirements, not arbitrary modifiers
        /// </summary>
        [Fact]
        public void Route_Should_Create_Logical_Access_Conditions_Based_On_Weather()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);

            // Create exposed weather route that requires weather protection in bad conditions
            RouteOption exposedRoute = CreateTestRoute("mountain_pass", baseCost: 5, baseStamina: 2);
            exposedRoute.TerrainCategories = new List<TerrainCategory> { TerrainCategory.Exposed_Weather };

            // Act & Assert - Clear weather allows access
            RouteAccessResult clearResult = exposedRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);
            Assert.True(clearResult.IsAllowed);
            Assert.Empty(clearResult.BlockingReason);

            // Act & Assert - Rain blocks access without weather protection
            RouteAccessResult rainResult = exposedRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Rain);
            Assert.False(rainResult.IsAllowed);
            Assert.Contains("weather protection", rainResult.BlockingReason.ToLower());

            // Act & Assert - Snow blocks access without weather protection  
            RouteAccessResult snowResult = exposedRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Snow);
            Assert.False(snowResult.IsAllowed);
            Assert.Contains("weather protection", snowResult.BlockingReason.ToLower());

            // Act & Assert - With weather protection, rain allows access
            Item weatherGear = new Item
            {
                Id = "weather_cloak",
                Name = "Weather Cloak",
                Categories = new List<EquipmentCategory> { EquipmentCategory.Weather_Protection },
                LocationId = "town_square",
                SpotId = "marketplace",
                Weight = 1,
                BuyPrice = 10,
                SellPrice = 5,
                InventorySlots = 1
            };
            itemRepository.AddItem(weatherGear);
            player.Inventory.AddItem("Weather Cloak");

            RouteAccessResult publicRainResult = exposedRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Rain);
            Assert.True(publicRainResult.IsAllowed);
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
        /// Test that route conditions create logical access patterns and strategic decisions
        /// Acceptance Criteria: Players must choose equipment and routes based on logical terrain-weather interactions
        /// </summary>
        [Fact]
        public void Route_Conditions_Should_Create_Strategic_Decisions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);

            // Fast but weather-sensitive route (blocked in bad weather without equipment)
            RouteOption fastRoute = CreateTestRoute("river_crossing", baseCost: 10, baseStamina: 1, timeBlocks: 1);
            fastRoute.TerrainCategories = new List<TerrainCategory> { TerrainCategory.Exposed_Weather };

            // Slow but reliable route (always accessible)
            RouteOption reliableRoute = CreateTestRoute("safe_road", baseCost: 5, baseStamina: 3, timeBlocks: 2);

            AddRouteToLocation(gameWorld, "town_square", "destination", fastRoute);
            AddRouteToLocation(gameWorld, "town_square", "destination", reliableRoute);

            // Act & Assert - Clear weather: both routes accessible
            List<RouteOption> clearRoutes = travelManager.GetAvailableRoutes("town_square", "destination");
            Assert.Contains(fastRoute, clearRoutes);
            Assert.Contains(reliableRoute, clearRoutes);

            // Act & Assert - Rain: fast route blocked without equipment, reliable route still accessible
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Rain;
            List<RouteOption> rainRoutes = travelManager.GetAvailableRoutes("town_square", "destination");
            Assert.DoesNotContain(fastRoute, rainRoutes); // Blocked by weather
            Assert.Contains(reliableRoute, rainRoutes); // Still accessible

            // Act & Assert - With weather gear, fast route becomes accessible again
            Item weatherGear = new Item
            {
                Id = "rain_cloak",
                Name = "Rain Cloak",
                Categories = new List<EquipmentCategory> { EquipmentCategory.Weather_Protection },
                LocationId = "town_square",
                SpotId = "marketplace",
                Weight = 1,
                BuyPrice = 15,
                SellPrice = 7,
                InventorySlots = 1
            };
            itemRepository.AddItem(weatherGear);
            player.Inventory.AddItem("Rain Cloak");

            List<RouteOption> equippedRainRoutes = travelManager.GetAvailableRoutes("town_square", "destination");
            Assert.Contains(fastRoute, equippedRainRoutes); // Now accessible with equipment
            Assert.Contains(reliableRoute, equippedRainRoutes); // Reliable route still accessible
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
        /// Test that route discovery creates gameplay depth through logical system interactions
        /// Acceptance Criteria: Players learn terrain-weather-equipment relationships through exploration
        /// </summary>
        [Fact]
        public void Route_Discovery_Should_Create_Learning_Gameplay()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);

            // Create wilderness route that has logical interactions with weather and equipment
            RouteOption wildernessRoute = CreateTestRoute("forest_path");
            wildernessRoute.TerrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain };

            // Act - Player can access route in clear weather
            RouteAccessResult clearResult = wildernessRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);
            Assert.True(clearResult.IsAllowed);

            // Act - Player discovers route becomes blocked in fog without navigation tools
            RouteAccessResult fogResult = wildernessRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Fog);
            Assert.False(fogResult.IsAllowed);
            Assert.Contains("wilderness", fogResult.BlockingReason.ToLower());

            // Act - Player learns navigation tools enable access in dangerous conditions
            Item compass = new Item
            {
                Id = "compass",
                Name = "Compass",
                Categories = new List<EquipmentCategory> { EquipmentCategory.Navigation_Tools },
                LocationId = "town_square",
                SpotId = "marketplace",
                Weight = 1,
                BuyPrice = 12,
                SellPrice = 6,
                InventorySlots = 1
            };
            itemRepository.AddItem(compass);
            player.Inventory.AddItem("Compass");

            RouteAccessResult equippedFogResult = wildernessRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Fog);
            Assert.True(equippedFogResult.IsAllowed);

            // Game should NOT tell player "buy navigation tools for wilderness routes"
            // Player must discover these relationships through exploration
            Assert.False(HasProperty(wildernessRoute, "EquipmentSuggestion"));
            Assert.False(HasProperty(wildernessRoute, "WeatherWarning"));
            Assert.False(HasProperty(wildernessRoute, "TerrainTips"));
        }

        #region Helper Methods

        private GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);

            gameWorld.WorldState.CurrentDay = 1;
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;

            // Create and register the starting location
            Location startLocation = new Location("town_square", "Town Square");
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            locationRepository.AddLocation(startLocation);

            // Set player location - player must always be at exactly one location
            gameWorld.WorldState.SetCurrentLocation(startLocation, null);

            return gameWorld;
        }

        private TravelManager CreateTravelManager(GameWorld gameWorld)
        {
            // This would normally be injected, but for testing we create a minimal version
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, new ItemRepository(gameWorld));
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