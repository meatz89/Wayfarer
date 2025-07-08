using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Tests to verify the critical game initialization fix works correctly,
/// ensuring LocationSpot is properly set by WorldState.SetCurrentLocation method.
/// 
/// This tests the fix for the bug where the SetCurrentLocation method
/// received a currentLocationSpot parameter but failed to assign it.
/// </summary>
public class GameInitializationTests
{
    [Fact]
    public void WorldState_SetCurrentLocation_ShouldSetBothLocationAndSpot()
    {
        // Arrange
        var gameWorld = new GameWorld();
        var location = new Location("test_location", "Test Location");
        var spot = new LocationSpot("test_spot", "Test Spot") 
        { 
            LocationId = "test_location" 
        };

        // Act - This was the broken method that we fixed
        gameWorld.WorldState.SetCurrentLocation(location, spot);

        // Assert - Verify the fix works
        Assert.Equal(location, gameWorld.WorldState.CurrentLocation);
        Assert.Equal(spot, gameWorld.WorldState.CurrentLocationSpot);
        Assert.Equal(location.Id, gameWorld.WorldState.CurrentLocationSpot.LocationId);
    }

    [Fact]
    public void WorldState_SetCurrentLocation_WithNullLocation_ShouldNotSetSpot()
    {
        // Arrange
        var gameWorld = new GameWorld();
        var spot = new LocationSpot("test_spot", "Test Spot") 
        { 
            LocationId = "test_location" 
        };

        // Act
        gameWorld.WorldState.SetCurrentLocation(null, spot);

        // Assert
        Assert.Null(gameWorld.WorldState.CurrentLocation);
        Assert.Null(gameWorld.WorldState.CurrentLocationSpot);
    }

    [Fact]
    public void WorldState_SetCurrentLocation_WithNullSpot_ShouldSetLocationButNotSpot()
    {
        // Arrange
        var gameWorld = new GameWorld();
        var location = new Location("test_location", "Test Location");

        // Act
        gameWorld.WorldState.SetCurrentLocation(location, null);

        // Assert
        Assert.Equal(location, gameWorld.WorldState.CurrentLocation);
        Assert.Null(gameWorld.WorldState.CurrentLocationSpot);
    }

    [Fact]
    public void WorldState_SetCurrentLocation_OverwriteExistingSpot_ShouldWork()
    {
        // Arrange
        var gameWorld = new GameWorld();
        var location1 = new Location("location1", "Location 1");
        var location2 = new Location("location2", "Location 2");
        var spot1 = new LocationSpot("spot1", "Spot 1") { LocationId = "location1" };
        var spot2 = new LocationSpot("spot2", "Spot 2") { LocationId = "location2" };

        // Act - Set first location/spot
        gameWorld.WorldState.SetCurrentLocation(location1, spot1);
        
        // Verify first assignment
        Assert.Equal(location1, gameWorld.WorldState.CurrentLocation);
        Assert.Equal(spot1, gameWorld.WorldState.CurrentLocationSpot);

        // Act - Overwrite with second location/spot
        gameWorld.WorldState.SetCurrentLocation(location2, spot2);

        // Assert - Should be updated to second location/spot
        Assert.Equal(location2, gameWorld.WorldState.CurrentLocation);
        Assert.Equal(spot2, gameWorld.WorldState.CurrentLocationSpot);
        Assert.Equal(location2.Id, gameWorld.WorldState.CurrentLocationSpot.LocationId);
    }

    [Fact]
    public void WorldState_SetCurrentLocation_EnsuresSpotMatchesLocation()
    {
        // Arrange
        var gameWorld = new GameWorld();
        var location = new Location("correct_location", "Correct Location");
        var spot = new LocationSpot("test_spot", "Test Spot") 
        { 
            LocationId = "correct_location"  // Spot should match location
        };

        // Act
        gameWorld.WorldState.SetCurrentLocation(location, spot);

        // Assert - Verify the relationship is maintained
        Assert.Equal(location.Id, gameWorld.WorldState.CurrentLocationSpot.LocationId);
        Assert.Equal("correct_location", gameWorld.WorldState.CurrentLocation.Id);
        Assert.Equal("correct_location", gameWorld.WorldState.CurrentLocationSpot.LocationId);
    }
}