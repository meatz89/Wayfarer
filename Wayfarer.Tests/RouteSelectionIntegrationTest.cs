using System.Collections.Generic;
using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Route selection tests using the superior test pattern.
    /// Tests route functionality using TestGameWorldInitializer and real GameWorld objects.
    /// Follows game design principles - no automated optimization!
    /// </summary>
    public class RouteSelectionIntegrationTest
    {
        [Fact]
        public void TravelManager_GetAvailableRoutes_Should_Return_Valid_Routes()
        {
            // === SETUP WITH NEW TEST PATTERN ===
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p
                    .StartAt("dusty_flagon")
                    .WithCoins(100)
                    .WithStamina(10))
                .WithTimeState(t => t
                    .Day(1)
                    .TimeBlock(TimeBlocks.Morning));
            
            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            
            // Create repositories using new pattern
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            
            // Create services with proper dependencies
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

            // Create test locations with routes
            Location townSquare = new Location("town_square", "Town Square");
            Location dustyFlagon = new Location("dusty_flagon", "Dusty Flagon");

            // Add route options
            RouteOption walkRoute = new RouteOption
            {
                Id = "walk_route",
                Name = "Walk",
                Origin = "town_square",
                Destination = "dusty_flagon",
                BaseCoinCost = 0,
                BaseStaminaCost = 2,
                TimeBlockCost = 1,
                IsDiscovered = true,
                Method = TravelMethods.Walking
            };

            RouteOption cartRoute = new RouteOption
            {
                Id = "cart_route",
                Name = "Cart",
                Origin = "town_square",
                Destination = "dusty_flagon",
                BaseCoinCost = 5,
                BaseStaminaCost = 1,
                TimeBlockCost = 1,
                IsDiscovered = true,
                Method = TravelMethods.Carriage
            };

            townSquare.Connections.Add(new LocationConnection
            {
                DestinationLocationId = "dusty_flagon",
                RouteOptions = new List<RouteOption> { walkRoute, cartRoute }
            });

            locationRepository.AddLocation(townSquare);
            locationRepository.AddLocation(dustyFlagon);

            ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
            TravelManager travelManager = new TravelManager(gameWorld, locationSystem, actionRepository, locationRepository, actionFactory, itemRepository, contractProgression);

            // Act
            List<RouteOption> availableRoutes = travelManager.GetAvailableRoutes("town_square", "dusty_flagon");

            // Assert
            Assert.NotNull(availableRoutes);
            Assert.Equal(2, availableRoutes.Count);

            // Verify each route has basic properties (no automated analysis)
            foreach (RouteOption route in availableRoutes)
            {
                Assert.NotNull(route.Name);
                Assert.True(route.BaseCoinCost >= 0);
                Assert.True(route.BaseStaminaCost >= 0);
                Assert.True(route.TimeBlockCost >= 0);
                Assert.True(route.IsDiscovered);

                // Verify player can manually check costs
                int coinCost = travelManager.CalculateCoinCost(route);
                int staminaCost = travelManager.CalculateStaminaCost(route);
                bool canAfford = travelManager.CanTravel(route);

                Assert.True(coinCost >= 0);
                Assert.True(staminaCost >= 0);
                Assert.True(canAfford); // Player should be able to afford these test routes
            }
        }

        [Fact]
        public void TravelManager_WeatherModifications_Should_Affect_Costs()
        {
            // Arrange
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            player.Coins = 10; // Low coin count to avoid coin weight affecting stamina
            player.Stamina = 10;
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;

            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

            // Create test locations with weather-sensitive route
            Location townSquare = new Location("town_square", "Town Square");
            Location dustyFlagon = new Location("dusty_flagon", "Dusty Flagon");

            RouteOption weatherRoute = new RouteOption
            {
                Id = "weather_route",
                Name = "Forest Path",
                Origin = "town_square",
                Destination = "dusty_flagon",
                BaseCoinCost = 2,
                BaseStaminaCost = 3,
                TimeBlockCost = 1,
                IsDiscovered = true,
                Method = TravelMethods.Walking,
                WeatherModifications = new Dictionary<WeatherCondition, RouteModification>
                {
                    { WeatherCondition.Rain, new RouteModification { StaminaCostModifier = 2 } }
                }
            };

            townSquare.Connections.Add(new LocationConnection
            {
                DestinationLocationId = "dusty_flagon",
                RouteOptions = new List<RouteOption> { weatherRoute }
            });

            locationRepository.AddLocation(townSquare);
            locationRepository.AddLocation(dustyFlagon);

            ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
            TravelManager travelManager = new TravelManager(gameWorld, locationSystem, actionRepository, locationRepository, actionFactory, itemRepository, contractProgression);

            // Act & Assert - Clear weather
            gameWorld.CurrentWeather = WeatherCondition.Clear;
            int clearWeatherCost = travelManager.CalculateStaminaCost(weatherRoute);
            Assert.Equal(3, clearWeatherCost); // Base cost

            // Act & Assert - Rainy weather
            gameWorld.CurrentWeather = WeatherCondition.Rain;
            int rainyWeatherCost = travelManager.CalculateStaminaCost(weatherRoute);
            Assert.Equal(5, rainyWeatherCost); // Base cost + 2 penalty

            // Verify no automated warnings are provided
            Assert.False(HasProperty(weatherRoute, "WeatherWarning"));
            Assert.False(HasProperty(weatherRoute, "CostPrediction"));
            Assert.False(HasProperty(weatherRoute, "OptimalWeatherAdvice"));
        }

        private bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }
    }
}