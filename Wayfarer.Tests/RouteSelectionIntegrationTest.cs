using Xunit;
using System.Collections.Generic;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Simple integration test to verify Route Selection Interface functionality.
    /// This test verifies that the new route comparison methods work correctly.
    /// </summary>
    public class RouteSelectionIntegrationTest
    {
        [Fact]
        public void TravelManager_GetRouteComparisonData_Should_Return_Valid_Comparison()
        {
            // Arrange
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Coins = 100;
            player.Stamina = 10;
            gameWorld.CurrentTimeBlock = TimeBlocks.Morning;

            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository();

            // Create test locations with routes
            Location townSquare = new Location("town_square", "Town Square");
            Location dustyFlagon = new Location("dusty_flagon", "Dusty Flagon");

            // Add route options
            RouteOption walkRoute = new RouteOption 
            { 
                Name = "Walk", 
                BaseCoinCost = 0, 
                BaseStaminaCost = 2, 
                TimeBlockCost = 1, 
                IsDiscovered = true,
                Method = TravelMethods.Walking
            };

            RouteOption cartRoute = new RouteOption 
            { 
                Name = "Cart", 
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

            TravelManager travelManager = new TravelManager(gameWorld, null, null, locationRepository, null, itemRepository);

            // Act
            List<RouteComparisonData> comparisonData = travelManager.GetRouteComparisonData("town_square", "dusty_flagon");

            // Assert
            Assert.NotNull(comparisonData);
            Assert.Equal(2, comparisonData.Count);

            // Verify each route has proper comparison data
            foreach (RouteComparisonData comparison in comparisonData)
            {
                Assert.NotNull(comparison.Route);
                Assert.NotNull(comparison.CostBenefitAnalysis);
                Assert.True(comparison.TotalCost >= 0);
                Assert.True(comparison.EfficiencyScore > 0);
                Assert.NotNull(comparison.ArrivalTime);
                Assert.True(comparison.CanAfford);
            }

            // Verify routes are sorted by efficiency (higher is better)
            Assert.True(comparisonData[0].EfficiencyScore >= comparisonData[1].EfficiencyScore);
        }

        [Fact]
        public void TravelManager_GetOptimalRouteRecommendation_Should_Return_Valid_Recommendation()
        {
            // Arrange
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Coins = 100;
            player.Stamina = 10;
            gameWorld.CurrentTimeBlock = TimeBlocks.Morning;

            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository();

            // Create test locations with routes
            Location townSquare = new Location("town_square", "Town Square");
            Location dustyFlagon = new Location("dusty_flagon", "Dusty Flagon");

            RouteOption walkRoute = new RouteOption 
            { 
                Name = "Walk", 
                BaseCoinCost = 0, 
                BaseStaminaCost = 2, 
                TimeBlockCost = 1, 
                IsDiscovered = true,
                Method = TravelMethods.Walking
            };

            townSquare.Connections.Add(new LocationConnection
            {
                DestinationLocationId = "dusty_flagon",
                RouteOptions = new List<RouteOption> { walkRoute }
            });

            locationRepository.AddLocation(townSquare);
            locationRepository.AddLocation(dustyFlagon);

            TravelManager travelManager = new TravelManager(gameWorld, null, null, locationRepository, null, itemRepository);

            // Act
            RouteRecommendation recommendation = travelManager.GetOptimalRouteRecommendation(
                "town_square", "dusty_flagon", OptimizationStrategy.Efficiency);

            // Assert
            Assert.NotNull(recommendation);
            Assert.NotNull(recommendation.RecommendedRoute);
            Assert.NotNull(recommendation.Justification);
            Assert.True(recommendation.EfficiencyScore > 0);
            Assert.Equal(OptimizationStrategy.Efficiency, recommendation.Strategy);
        }
    }
}