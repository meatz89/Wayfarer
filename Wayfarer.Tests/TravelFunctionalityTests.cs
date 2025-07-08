using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests travel functionality - player should move between locations when travel actions are executed
    /// </summary>
    public class TravelFunctionalityTests
    {
        private IServiceProvider CreateEconomicServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            
            // Add configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"DefaultAIProvider", "None"} // Disable AI
                })
                .Build();
            services.AddSingleton(configuration);
            services.AddLogging();
            
            // Use the test service configuration
            services.ConfigureTestServices();
            
            return services.BuildServiceProvider();
        }

        private (GameWorld gameWorld, GameWorldManager gameWorldManager) InitializeGameForTravel()
        {
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            
            // Initialize location system
            Location startLocation = locationSystem.Initialize().Result;
            
            // Character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);
            
            // Start game
            gameWorldManager.StartGame();
            
            return (gameWorld, gameWorldManager);
        }

        [Fact]
        public void Travel_FromDustyFlagonToTownSquare_ShouldChangePlayerLocation()
        {
            // Arrange
            var (gameWorld, gameWorldManager) = InitializeGameForTravel();
            Player player = gameWorld.GetPlayer();
            
            // Verify starting location
            Assert.Equal("dusty_flagon", player.CurrentLocation.Id);
            Assert.Equal("dusty_flagon", gameWorld.WorldState.CurrentLocation.Id);
            
            // Get available routes to town_square
            List<RouteOption> routes = gameWorldManager.GetAvailableRoutes("dusty_flagon", "town_square");
            Assert.True(routes.Count > 0, "Should have at least one route from dusty_flagon to town_square");
            
            RouteOption route = routes[0];
            
            // Act
            gameWorldManager.Travel(route).Wait();
            
            // Assert
            Assert.Equal("town_square", player.CurrentLocation.Id);
            Assert.Equal("town_square", gameWorld.WorldState.CurrentLocation.Id);
            Assert.NotEqual("dusty_flagon", player.CurrentLocation.Id);
        }

        [Fact]
        public void Travel_ShouldConsumeStaminaAndCoinResources()
        {
            // Arrange
            var (gameWorld, gameWorldManager) = InitializeGameForTravel();
            Player player = gameWorld.GetPlayer();
            
            int initialStamina = player.Stamina;
            int initialCoins = player.Coins;
            
            // Get route to town_square
            List<RouteOption> routes = gameWorldManager.GetAvailableRoutes("dusty_flagon", "town_square");
            RouteOption route = routes[0];
            
            // Act
            gameWorldManager.Travel(route).Wait();
            
            // Assert
            Assert.True(player.Stamina <= initialStamina, "Travel should consume stamina");
            Assert.True(player.Coins <= initialCoins, "Travel should consume coins (or stay same if free)");
        }

        [Fact]
        public void Travel_WithInsufficientResources_ShouldNotChangeLocation()
        {
            // Arrange
            var (gameWorld, gameWorldManager) = InitializeGameForTravel();
            Player player = gameWorld.GetPlayer();
            
            // Drain player resources
            player.Stamina = 0;
            player.Coins = 0;
            
            string originalLocation = player.CurrentLocation.Id;
            
            // Get route that requires resources
            List<RouteOption> routes = gameWorldManager.GetAvailableRoutes("dusty_flagon", "town_square");
            RouteOption route = routes[0];
            
            // Act
            gameWorldManager.Travel(route).Wait();
            
            // Assert - should still be at original location
            Assert.Equal(originalLocation, player.CurrentLocation.Id);
            Assert.Equal(originalLocation, gameWorld.WorldState.CurrentLocation.Id);
        }

        [Fact]
        public void GetAvailableRoutes_ShouldReturnValidRoutes()
        {
            // Arrange
            var (gameWorld, gameWorldManager) = InitializeGameForTravel();
            
            // Act
            List<RouteOption> routes = gameWorldManager.GetAvailableRoutes("dusty_flagon", "town_square");
            
            // Assert
            Assert.NotNull(routes);
            Assert.True(routes.Count > 0, "Should have routes between known locations");
            
            foreach (RouteOption route in routes)
            {
                Assert.Equal("dusty_flagon", route.Origin);
                Assert.Equal("town_square", route.Destination);
                Assert.NotNull(route.Id);
                Assert.NotNull(route.Name);
            }
        }
    }
}