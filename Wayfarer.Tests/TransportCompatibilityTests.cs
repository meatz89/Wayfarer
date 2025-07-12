using Xunit;
using Wayfarer.Game.MainSystem;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Tests
{
    public class TransportCompatibilityTests
    {
        private readonly TransportCompatibilityValidator _validator;
        private readonly ItemRepository _itemRepository;
        private readonly GameWorld _gameWorld;

        public TransportCompatibilityTests()
        {
            // Use simple test initialization
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            _gameWorld = initializer.LoadGame();
            _itemRepository = new ItemRepository(_gameWorld);
            _validator = new TransportCompatibilityValidator(_itemRepository);
        }

    [Fact]
    public void Cart_ShouldBeBlocked_OnMountainTerrain()
    {
        // Arrange
        var terrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Climbing };
        
        // Act
        var result = _validator.CheckTerrainCompatibility(TravelMethods.Cart, terrainCategories);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Cart cannot navigate mountain terrain", result.BlockingReason);
    }

    [Fact]
    public void Cart_ShouldBeBlocked_OnWildernessTerrain()
    {
        // Arrange
        var terrainCategories = new List<TerrainCategory> { TerrainCategory.Wilderness_Terrain };
        
        // Act
        var result = _validator.CheckTerrainCompatibility(TravelMethods.Cart, terrainCategories);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Cart cannot navigate rough wilderness terrain", result.BlockingReason);
    }

    [Fact]
    public void Boat_ShouldOnlyWork_OnWaterRoutes()
    {
        // Arrange
        var landTerrain = new List<TerrainCategory> { TerrainCategory.Exposed_Weather };
        var waterTerrain = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport };
        
        // Act
        var landResult = _validator.CheckTerrainCompatibility(TravelMethods.Boat, landTerrain);
        var waterResult = _validator.CheckTerrainCompatibility(TravelMethods.Boat, waterTerrain);
        
        // Assert
        Assert.False(landResult.IsCompatible);
        Assert.Contains("Boat transport only works on water routes", landResult.BlockingReason);
        Assert.True(waterResult.IsCompatible);
    }

    [Fact]
    public void NonBoatTransport_ShouldBeBlocked_OnWaterRoutes()
    {
        // Arrange
        var waterTerrain = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport };
        
        // Act
        var walkingResult = _validator.CheckTerrainCompatibility(TravelMethods.Walking, waterTerrain);
        var horsebackResult = _validator.CheckTerrainCompatibility(TravelMethods.Horseback, waterTerrain);
        
        // Assert
        Assert.False(walkingResult.IsCompatible);
        Assert.Contains("Water routes require boat transport", walkingResult.BlockingReason);
        Assert.False(horsebackResult.IsCompatible);
    }

    [Fact]
    public void HeavyEquipment_ShouldBlockBoatTransport()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Give player a large item
        var largeItem = _itemRepository.GetAllItems()
            .FirstOrDefault(item => item.Size == SizeCategory.Large);
        
        if (largeItem != null)
        {
            player.Inventory.ItemSlots[0] = largeItem.Id;
        }
        
        // Act
        var result = _validator.CheckEquipmentCompatibility(TravelMethods.Boat, player);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Heavy equipment blocks boat transport", result.BlockingReason);
    }

    [Fact]
    public void MassiveItems_ShouldBlockCarriageTransport()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Create a massive test item
        var testItem = new Item
        {
            Id = "test_massive",
            Name = "Test Massive Item",
            Size = SizeCategory.Massive
        };
        
        // Add to game world for repository to find
        _gameWorld.WorldState.Items.Add(testItem);
        player.Inventory.ItemSlots[0] = testItem.Id;
        
        // Act
        var result = _validator.CheckEquipmentCompatibility(TravelMethods.Carriage, player);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Massive items cannot fit in carriage", result.BlockingReason);
    }

    [Fact]
    public void Walking_ShouldAlwaysBeCompatible_WithAnyEquipment()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Give player various items
        player.Inventory.ItemSlots[0] = "herbs";
        player.Inventory.ItemSlots[1] = "tools";
        
        // Act
        var result = _validator.CheckEquipmentCompatibility(TravelMethods.Walking, player);
        
        // Assert
        Assert.True(result.IsCompatible);
    }

    // TODO: Add TravelManager integration tests once constructor dependencies are resolved

    [Fact]
    public void FullCompatibilityCheck_ShouldCombineTerrainAndEquipmentChecks()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Give player heavy equipment
        var testItem = new Item
        {
            Id = "test_heavy",
            Name = "Heavy Equipment",
            Size = SizeCategory.Large
        };
        _gameWorld.WorldState.Items.Add(testItem);
        player.Inventory.ItemSlots[0] = testItem.Id;
        
        var route = new RouteOption
        {
            Id = "test_water_route",
            TerrainCategories = new List<TerrainCategory> { TerrainCategory.Requires_Water_Transport }
        };
        
        // Act
        var result = _validator.CheckFullCompatibility(TravelMethods.Boat, route, player);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Heavy equipment blocks boat transport", result.BlockingReason);
    }

    [Fact]
    public void HorsebackTransport_ShouldBeBlocked_WithHeavyEquipment()
    {
        // Arrange
        var player = _gameWorld.GetPlayer();
        
        // Give player a large item
        var largeItem = _itemRepository.GetAllItems()
            .FirstOrDefault(item => item.Size == SizeCategory.Large);
        
        if (largeItem != null)
        {
            player.Inventory.ItemSlots[0] = largeItem.Id;
        }
        
        // Act
        var result = _validator.CheckEquipmentCompatibility(TravelMethods.Horseback, player);
        
        // Assert
        Assert.False(result.IsCompatible);
        Assert.Contains("Heavy equipment incompatible with horseback travel", result.BlockingReason);
    }
}
}