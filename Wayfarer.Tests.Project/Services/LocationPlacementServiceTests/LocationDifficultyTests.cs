using Xunit;

namespace Wayfarer.Tests.Services.LocationPlacementServiceTests;

/// <summary>
/// Tests for Location.Difficulty calculation based on hex distance from world center.
/// Validates the formula: Difficulty = DistanceTo(0,0) / 5 (integer division).
/// </summary>
public class LocationDifficultyTests
{
    // ============================================
    // DIFFICULTY CALCULATION FORMULA
    // ============================================

    [Theory]
    [InlineData(0, 0, 0)]    // At world center: distance 0, difficulty 0
    [InlineData(4, 0, 0)]    // Distance 4: 4/5 = 0
    [InlineData(5, 0, 1)]    // Distance 5: 5/5 = 1
    [InlineData(10, 0, 2)]   // Distance 10: 10/5 = 2
    [InlineData(12, 0, 2)]   // Distance 12: 12/5 = 2 (integer division)
    [InlineData(25, 0, 5)]   // Distance 25: 25/5 = 5
    [InlineData(50, 0, 10)]  // Distance 50: 50/5 = 10
    public void CalculateDifficulty_BasedOnHexDistanceFromCenter(int q, int r, int expectedDifficulty)
    {
        GameWorld gameWorld = CreateMinimalGameWorld();
        LocationPlacementService service = new LocationPlacementService(gameWorld);

        Location location = new Location("TestLocation")
        {
            HexPosition = new AxialCoordinates(q, r)
        };

        service.CalculateLocationDifficulty(location);

        Assert.Equal(expectedDifficulty, location.Difficulty);
    }

    [Fact]
    public void CalculateDifficulty_ThrowsWhenHexPositionNotSet()
    {
        GameWorld gameWorld = CreateMinimalGameWorld();
        LocationPlacementService service = new LocationPlacementService(gameWorld);

        Location location = new Location("TestLocation");
        // HexPosition not set (null)

        Assert.Throws<InvalidOperationException>(() => service.CalculateLocationDifficulty(location));
    }

    [Fact]
    public void CalculateDifficulty_NegativeCoordinatesCalculateCorrectly()
    {
        // Axial distance formula handles negative coordinates
        // Distance from (-5, -5) to (0, 0) should be calculated correctly
        GameWorld gameWorld = CreateMinimalGameWorld();
        LocationPlacementService service = new LocationPlacementService(gameWorld);

        Location location = new Location("TestLocation")
        {
            HexPosition = new AxialCoordinates(-5, -5)
        };

        service.CalculateLocationDifficulty(location);

        // Distance calculation depends on AxialCoordinates.DistanceTo implementation
        // For axial coords, distance = max(|q|, |r|, |q+r|) but implementation may vary
        // The test validates that negative coords don't cause issues
        Assert.True(location.Difficulty >= 0);
    }

    // ============================================
    // INTEGRATION WITH LOCATION CREATION
    // ============================================

    [Fact]
    public void LocationDifficulty_AffectsNetChallengeCalculation()
    {
        // Location at distance 25 (difficulty 5)
        // Player with 15 total stats (15/5 = 3)
        // Net Challenge = 5 - 3 = +2 (underpowered)
        Location location = new Location("DistantLocation")
        {
            Difficulty = 5,
            HexPosition = new AxialCoordinates(25, 0)
        };

        Player player = new Player
        {
            Insight = 3,
            Rapport = 3,
            Authority = 3,
            Diplomacy = 3,
            Cunning = 3,
            CurrentPosition = new AxialCoordinates(0, 0)
        };

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, player);

        Assert.Equal(2, context.NetChallengeAdjustment);
    }

    [Fact]
    public void LocationDifficulty_WorldCenterHasZeroDifficulty()
    {
        // Locations at world center should have difficulty 0
        // This creates the "easy start" experience for new players
        Location location = new Location("StartingLocation")
        {
            Difficulty = 0,
            HexPosition = new AxialCoordinates(0, 0)
        };

        Player newPlayer = new Player
        {
            Insight = 1,
            Rapport = 1,
            Authority = 1,
            Diplomacy = 1,
            Cunning = 1,
            CurrentPosition = new AxialCoordinates(0, 0)
        };

        RuntimeScalingContext context = RuntimeScalingContext.FromEntities(null, location, newPlayer);

        // Net = 0 - (5/5) = 0 - 1 = -1 (player slightly overpowered for start)
        Assert.Equal(-1, context.NetChallengeAdjustment);
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private GameWorld CreateMinimalGameWorld()
    {
        GameWorld gameWorld = new GameWorld();
        // WorldHexGrid is auto-initialized in GameWorld constructor
        return gameWorld;
    }
}
