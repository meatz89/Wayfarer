using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests for the Logical Equipment Requirements for Routes feature.
    /// Validates that routes require appropriate equipment categories based on terrain types,
    /// creating strategic equipment planning decisions for players.
    /// 
    /// User Story: Logical Equipment Requirements (UserStories.md lines 29-49)
    /// </summary>
    public class LogicalEquipmentRequirementsTests
    {
        [Fact]
        public void Mountain_Route_Should_Require_Climbing_Equipment()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(50));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create mountain route with climbing requirement
            RouteOption mountainRoute = new RouteOption
            {
                Id = "mountain_trail",
                Name = "Mountain Trail",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Climbing },
                BaseStaminaCost = 4,
                TimeBlockCost = 2
            };

            // Act - Check route access without climbing equipment
            RouteAccessResult result = mountainRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);

            // Assert
            Assert.False(result.IsAllowed, "Mountain route should be blocked without climbing equipment");
            Assert.Contains("climbing equipment", result.BlockingReason.ToLower());
        }

        [Fact]
        public void Mountain_Route_Should_Allow_Access_With_Climbing_Equipment()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithItem("rope")); // rope has Climbing_Equipment category

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create mountain route with climbing requirement
            RouteOption mountainRoute = new RouteOption
            {
                Id = "mountain_trail",
                Name = "Mountain Trail",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Climbing },
                BaseStaminaCost = 4,
                TimeBlockCost = 2
            };

            // Act - Check route access with climbing equipment
            RouteAccessResult result = mountainRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);

            // Assert
            Assert.True(result.IsAllowed, "Mountain route should be accessible with climbing equipment");
        }

        [Fact]
        public void Dark_Passage_Should_Warn_Without_Light_Source()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create dark passage route
            RouteOption darkRoute = new RouteOption
            {
                Id = "dark_alley",
                Name = "Dark Alley",
                Origin = "town_square",
                Destination = "workshop",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Dark_Passage },
                BaseStaminaCost = 1,
                TimeBlockCost = 1
            };

            // Act - Check route access without light source
            RouteAccessResult result = darkRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);

            // Assert
            Assert.True(result.IsAllowed, "Dark passage should be allowed but with warnings");
            Assert.True(result.Warnings.Count > 0, "Should have warnings about dark passage");
            Assert.Contains(result.Warnings, w => w.ToLower().Contains("light"));
        }

        [Fact]
        public void Wilderness_Route_Should_Be_Blocked_In_Fog_Without_Navigation()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create wilderness route
            RouteOption wildernessRoute = new RouteOption
            {
                Id = "wilderness_path",
                Name = "Wilderness Path",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain },
                BaseStaminaCost = 3,
                TimeBlockCost = 2
            };

            // Act - Check route access in foggy weather without navigation tools
            RouteAccessResult result = wildernessRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Fog);

            // Assert
            Assert.False(result.IsAllowed, "Wilderness route should be blocked in fog without navigation tools");
            Assert.Contains("navigation", result.BlockingReason.ToLower());
        }

        [Fact]
        public void Wilderness_Route_Should_Allow_Access_In_Fog_With_Navigation()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithItem("compass")); // compass has Navigation_Tools category

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create wilderness route
            RouteOption wildernessRoute = new RouteOption
            {
                Id = "wilderness_path",
                Name = "Wilderness Path",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain },
                BaseStaminaCost = 3,
                TimeBlockCost = 2
            };

            // Act - Check route access in foggy weather with navigation tools
            RouteAccessResult result = wildernessRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Fog);

            // Assert
            Assert.True(result.IsAllowed, "Wilderness route should be accessible in fog with navigation tools");
        }

        [Fact]
        public void Complex_Route_Should_Require_Multiple_Equipment_Categories()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("workshop"));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create complex route requiring multiple equipment types
            RouteOption complexRoute = new RouteOption
            {
                Id = "heavy_supply_route",
                Name = "Heavy Supply Route",
                Origin = "workshop",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> 
                { 
                    TerrainCategory.Heavy_Cargo_Route, 
                    TerrainCategory.Wilderness_Terrain, 
                    TerrainCategory.Requires_Climbing 
                },
                BaseStaminaCost = 2,
                TimeBlockCost = 2
            };

            // Act - Check route access without any equipment
            RouteAccessResult result = complexRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);

            // Assert
            Assert.False(result.IsAllowed, "Complex route should be blocked without required equipment");
            Assert.Contains("climbing equipment", result.BlockingReason.ToLower());
        }

        [Fact]
        public void Player_Should_Understand_Equipment_Strategy_Through_Route_Requirements()
        {
            // Arrange - Test the strategic planning scenario from UserStories.md
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithCoins(100));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Create mountain route (requires climbing equipment)
            RouteOption mountainRoute = new RouteOption
            {
                Id = "mountain_trail",
                Name = "Mountain Trail",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Climbing },
                BaseStaminaCost = 2,
                TimeBlockCost = 1
            };

            // Create alternative road route (no equipment required, longer)
            RouteOption roadRoute = new RouteOption
            {
                Id = "mountain_road",
                Name = "Mountain Road",
                Origin = "town_square",
                Destination = "mountain_summit",
                TerrainCategories = new List<TerrainCategory>(), // No special requirements
                BaseStaminaCost = 4,
                TimeBlockCost = 3
            };

            // Act - Check both routes without equipment
            RouteAccessResult mountainResult = mountainRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);
            RouteAccessResult roadResult = roadRoute.CheckRouteAccess(itemRepository, player, WeatherCondition.Clear);

            // Assert - This creates the strategic decision described in UserStories.md
            Assert.False(mountainResult.IsAllowed, "Mountain route blocked without climbing equipment");
            Assert.True(roadResult.IsAllowed, "Road route available but takes longer");
            
            // The player now faces the strategic choice mentioned in UserStories.md:
            // "Do I take the longer [Road] route that's cart-compatible, or delay the contract to get proper equipment?"
            Assert.True(mountainRoute.TimeBlockCost < roadRoute.TimeBlockCost, "Mountain route should be faster");
            Assert.True(mountainRoute.BaseStaminaCost < roadRoute.BaseStaminaCost, "Mountain route should be less tiring");
        }
    }
}