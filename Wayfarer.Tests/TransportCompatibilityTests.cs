using System.Collections.Generic;
using System.Linq;
using Xunit;

public class TransportCompatibilityTests
    {
        private readonly TransportCompatibilityValidator _validator;
        private readonly ItemRepository _itemRepository;
        private readonly GameWorld _gameWorld;

        public TransportCompatibilityTests()
        {
            // Use simple test initialization
            // Create factories needed for GameWorldInitializer
            var locationFactory = new LocationFactory();
            var locationSpotFactory = new LocationSpotFactory();
            var npcFactory = new NPCFactory();
            var itemFactory = new ItemFactory();
            var routeFactory = new RouteFactory();
            var routeDiscoveryFactory = new RouteDiscoveryFactory();
            var networkUnlockFactory = new NetworkUnlockFactory();
            var letterTemplateFactory = new LetterTemplateFactory();
            var standingObligationFactory = new StandingObligationFactory();
            var actionDefinitionFactory = new ActionDefinitionFactory();
            
            var contentDirectory = new ContentDirectory { Path = "Content" };
            GameWorldInitializer initializer = new GameWorldInitializer(
                contentDirectory,
                locationFactory,
                locationSpotFactory,
                npcFactory,
                itemFactory,
                routeFactory,
                routeDiscoveryFactory,
                networkUnlockFactory,
                letterTemplateFactory,
                standingObligationFactory,
                actionDefinitionFactory);
            _gameWorld = initializer.LoadGame();
            _itemRepository = new ItemRepository(_gameWorld);
            _validator = new TransportCompatibilityValidator(_itemRepository);
        }

        [Fact]
        public void Cart_ShouldBeBlocked_OnMountainTerrain()
        {
            // Arrange
            List<TerrainCategory> terrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Climbing };

            // Act
            TransportCompatibilityResult result = _validator.CheckTerrainCompatibility(TravelMethods.Cart, terrainCategories);

            // Assert
            Assert.False(result.IsCompatible);
            Assert.Contains("Cart cannot navigate mountain terrain", result.BlockingReason);
        }

        [Fact]
        public void Cart_ShouldBeBlocked_OnWildernessTerrain()
        {
            // Arrange
            List<TerrainCategory> terrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain };

            // Act
            TransportCompatibilityResult result = _validator.CheckTerrainCompatibility(TravelMethods.Cart, terrainCategories);

            // Assert
            Assert.False(result.IsCompatible);
            Assert.Contains("Cart cannot navigate rough wilderness terrain", result.BlockingReason);
        }

        [Fact]
        public void Boat_ShouldOnlyWork_OnWaterRoutes()
        {
            // Arrange
            List<TerrainCategory> landTerrain = new List<TerrainCategory> { TerrainCategory.Exposed_Weather };
            List<TerrainCategory> waterTerrain = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport };

            // Act
            TransportCompatibilityResult landResult = _validator.CheckTerrainCompatibility(TravelMethods.Boat, landTerrain);
            TransportCompatibilityResult waterResult = _validator.CheckTerrainCompatibility(TravelMethods.Boat, waterTerrain);

            // Assert
            Assert.False(landResult.IsCompatible);
            Assert.Contains("Boat transport only works on water routes", landResult.BlockingReason);
            Assert.True(waterResult.IsCompatible);
        }

        [Fact]
        public void NonBoatTransport_ShouldBeBlocked_OnWaterRoutes()
        {
            // Arrange
            List<TerrainCategory> waterTerrain = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport };

            // Act
            TransportCompatibilityResult walkingResult = _validator.CheckTerrainCompatibility(TravelMethods.Walking, waterTerrain);
            TransportCompatibilityResult horsebackResult = _validator.CheckTerrainCompatibility(TravelMethods.Horseback, waterTerrain);

            // Assert
            Assert.False(walkingResult.IsCompatible);
            Assert.Contains("Water routes require boat transport", walkingResult.BlockingReason);
            Assert.False(horsebackResult.IsCompatible);
        }


        [Fact]
        public void MassiveItems_ShouldBlockCarriageTransport()
        {
            // Arrange
            Player player = _gameWorld.GetPlayer();

            // Create a massive test item
            Item testItem = new Item
            {
                Id = "test_massive",
                Name = "Test Massive Item",
                Size = SizeCategory.Massive
            };

            // Add to game world for repository to find
            _gameWorld.WorldState.Items.Add(testItem);
            player.Inventory.ItemSlots[0] = testItem.Id;

            // Act
            TransportCompatibilityResult result = _validator.CheckEquipmentCompatibility(TravelMethods.Carriage, player);

            // Assert
            Assert.False(result.IsCompatible);
            Assert.Contains("Massive items cannot fit in carriage", result.BlockingReason);
        }

        [Fact]
        public void Walking_ShouldAlwaysBeCompatible_WithAnyEquipment()
        {
            // Arrange
            Player player = _gameWorld.GetPlayer();

            // Give player various items
            player.Inventory.ItemSlots[0] = "herbs";
            player.Inventory.ItemSlots[1] = "tools";

            // Act
            TransportCompatibilityResult result = _validator.CheckEquipmentCompatibility(TravelMethods.Walking, player);

            // Assert
            Assert.True(result.IsCompatible);
        }

        // TODO: Add TravelManager integration tests once constructor dependencies are resolved

        [Fact]
        public void FullCompatibilityCheck_ShouldCombineTerrainAndEquipmentChecks()
        {
            // Arrange
            Player player = _gameWorld.GetPlayer();

            // Give player heavy equipment
            Item testItem = new Item
            {
                Id = "test_heavy",
                Name = "Heavy Equipment",
                Size = SizeCategory.Large
            };
            _gameWorld.WorldState.Items.Add(testItem);
            player.Inventory.ItemSlots[0] = testItem.Id;

            RouteOption route = new RouteOption
            {
                Id = "test_water_route",
                TerrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport }
            };

            // Act
            TransportCompatibilityResult result = _validator.CheckFullCompatibility(TravelMethods.Boat, route, player);

            // Assert
            Assert.False(result.IsCompatible);
            Assert.Contains("Heavy equipment blocks boat transport", result.BlockingReason);
        }

    }