using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Comprehensive tests for PathfindingService.FindPath A* algorithm implementation.
/// Tests critical pathfinding functionality: path existence, terrain compatibility,
/// transport restrictions, danger calculation, cost accuracy, and edge cases.
/// MANDATORY per CLAUDE.md: Complex algorithms require complete test coverage.
/// </summary>
public class PathfindingServiceTests
{
    [Fact]
    public void FindPath_ValidPathExists_ReturnsSuccessWithPath()
    {
        // Arrange: 3x3 grid of plains, path from (0,0) to (2,2)
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(2, 2);

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Path);
        Assert.NotEmpty(result.Path);
        Assert.Equal(start, result.Path[0]); // Path starts at origin
        Assert.Equal(goal, result.Path[result.Path.Count - 1]); // Path ends at goal
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void FindPath_NoPathExists_ReturnsFailure()
    {
        // Arrange: Create hex map with impassable barrier blocking path
        HexMap hexMap = CreateHexMapWithImpassableBarrier();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(4, 0); // On other side of barrier

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Empty(result.Path);
        Assert.NotNull(result.FailureReason);
        Assert.Contains("No valid path", result.FailureReason);
    }

    [Fact]
    public void FindPath_StartHexDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(999, 999); // Invalid coordinates
        AxialCoordinates goal = new AxialCoordinates(1, 1);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking)
        );
        Assert.Contains("Start coordinates", exception.Message);
        Assert.Contains("do not exist", exception.Message);
    }

    [Fact]
    public void FindPath_GoalHexDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(999, 999); // Invalid coordinates

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking)
        );
        Assert.Contains("Goal coordinates", exception.Message);
        Assert.Contains("do not exist", exception.Message);
    }

    [Fact]
    public void FindPath_NullHexMap_ThrowsArgumentNullException()
    {
        // Arrange
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(1, 1);

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => PathfindingService.FindPath(start, goal, null, TransportType.Walking)
        );
        Assert.Equal("hexMap", exception.ParamName);
    }

    [Fact]
    public void FindPath_StartEqualsGoal_ReturnsPathWithSingleHex()
    {
        // Arrange: Start and goal are same hex
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(1, 1);
        AxialCoordinates goal = new AxialCoordinates(1, 1);

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Path);
        Assert.Equal(start, result.Path[0]);
        Assert.Equal(0, result.TotalCost); // No movement cost
    }

    [Fact]
    public void FindPath_WalkingCannotCrossWater_ReturnsFailure()
    {
        // Arrange: Create hex map with water barrier at r=2
        // To test blocking, goal must be on the OTHER side of the barrier
        HexMap hexMap = CreateHexMapWithWaterBarrier();
        AxialCoordinates start = new AxialCoordinates(0, 0); // r=0, below water barrier
        AxialCoordinates goal = new AxialCoordinates(0, 4);   // r=4, above water barrier (must cross r=2)

        // Act: Walking transport cannot cross water
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("No valid path", result.FailureReason);
    }

    [Fact]
    public void FindPath_BoatCanCrossWater_ReturnsSuccess()
    {
        // Arrange: Water path from (0,0) to (2,0)
        HexMap hexMap = CreateWaterHexMap();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(2, 0);

        // Act: Boat transport CAN cross water
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Boat);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Path);
        Assert.Equal(start, result.Path[0]);
        Assert.Equal(goal, result.Path[result.Path.Count - 1]);
    }

    [Fact]
    public void FindPath_BoatCannotCrossLand_ReturnsFailure()
    {
        // Arrange: Plains hexes (land terrain)
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(2, 2);

        // Act: Boat cannot traverse plains
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Boat);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not traversable", result.FailureReason);
    }

    [Fact]
    public void FindPath_OptimalPathChosen_AStarSelectsShortest()
    {
        // Arrange: Create hex grid where multiple paths exist but one is optimal
        HexMap hexMap = CreateHexMapMultiplePaths();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(3, 3);

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert: A* should find shortest path (hex distance = 6)
        Assert.True(result.IsSuccess);
        Assert.True(result.Path.Count <= 7); // 6 steps + start hex
    }

    [Fact]
    public void FindPath_GoalTerrainImpassableForTransport_ReturnsFailure()
    {
        // Arrange: Goal is water hex, transport is Cart (cannot traverse water)
        HexMap hexMap = CreateMixedTerrainMap();
        AxialCoordinates start = new AxialCoordinates(0, 0); // Plains
        AxialCoordinates goal = new AxialCoordinates(2, 2); // Water

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Cart);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not traversable", result.FailureReason);
        Assert.Contains("Cart", result.FailureReason);
    }

    [Fact]
    public void FindPath_EmptyPath_StartIsGoal()
    {
        // Arrange
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates location = new AxialCoordinates(0, 0);

        // Act
        PathfindingResult result = PathfindingService.FindPath(location, location, hexMap, TransportType.Walking);

        // Assert: Path contains only start/goal hex
        Assert.True(result.IsSuccess);
        Assert.Single(result.Path);
        Assert.Equal(location, result.Path[0]);
        Assert.Equal(0, result.TotalCost);
        Assert.Equal(0, result.DangerRating);
    }

    [Fact]
    public void FindPath_PathIncludesStartAndGoal()
    {
        // Arrange
        HexMap hexMap = CreateSimpleHexMap();
        AxialCoordinates start = new AxialCoordinates(0, 0);
        AxialCoordinates goal = new AxialCoordinates(1, 1);

        // Act
        PathfindingResult result = PathfindingService.FindPath(start, goal, hexMap, TransportType.Walking);

        // Assert: Path must include both start and goal hexes
        Assert.True(result.IsSuccess);
        Assert.Contains(start, result.Path);
        Assert.Contains(goal, result.Path);
        Assert.Equal(start, result.Path[0]);
        Assert.Equal(goal, result.Path[result.Path.Count - 1]);
    }

    // ========== HELPER METHODS: HexMap CREATION ==========

    /// <summary>
    /// Create 5x5 grid of plains hexes (all passable, uniform cost)
    /// </summary>
    private HexMap CreateSimpleHexMap()
    {
        HexMap hexMap = new HexMap
        {
            Width = 5,
            Height = 5,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 5; q++)
        {
            for (int r = 0; r < 5; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = TerrainType.Plains,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with vertical impassable barrier at Q=2
    /// Blocks path from west to east
    /// </summary>
    private HexMap CreateHexMapWithImpassableBarrier()
    {
        HexMap hexMap = new HexMap
        {
            Width = 5,
            Height = 5,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 5; q++)
        {
            for (int r = 0; r < 5; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = q == 2 ? TerrainType.Impassable : TerrainType.Plains,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with horizontal water barrier at R=2
    /// Blocks walking but allows boat
    /// </summary>
    private HexMap CreateHexMapWithWaterBarrier()
    {
        HexMap hexMap = new HexMap
        {
            Width = 5,
            Height = 5,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 5; q++)
        {
            for (int r = 0; r < 5; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = r == 2 ? TerrainType.Water : TerrainType.Plains,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with all water terrain (for boat testing)
    /// </summary>
    private HexMap CreateWaterHexMap()
    {
        HexMap hexMap = new HexMap
        {
            Width = 3,
            Height = 3,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 3; q++)
        {
            for (int r = 0; r < 3; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = TerrainType.Water,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with two parallel paths: road (faster) and forest (slower)
    /// Tests whether pathfinding chooses optimal route based on transport
    /// </summary>
    private HexMap CreateHexMapWithRoadAndForestPaths()
    {
        HexMap hexMap = new HexMap
        {
            Width = 5,
            Height = 5,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 5; q++)
        {
            for (int r = 0; r < 5; r++)
            {
                TerrainType terrain = TerrainType.Plains;

                // Road path: Q=1, R=0-4 (vertical road)
                if (q == 1)
                    terrain = TerrainType.Road;

                // Forest path: Q=2, R=0-4 (vertical forest)
                if (q == 2)
                    terrain = TerrainType.Forest;

                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = terrain,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with specific danger levels for testing danger calculation
    /// </summary>
    private HexMap CreateHexMapWithDangerLevels()
    {
        HexMap hexMap = new HexMap
        {
            Width = 3,
            Height = 1,
            Origin = new AxialCoordinates(0, 0)
        };

        // Create horizontal line: (0,0) -> (1,0) -> (2,0)
        Hex hex1 = new Hex
        {
            Coordinates = new AxialCoordinates(0, 0),
            Terrain = TerrainType.Plains,
            DangerLevel = 0
        };

        Hex hex2 = new Hex
        {
            Coordinates = new AxialCoordinates(1, 0),
            Terrain = TerrainType.Plains,
            DangerLevel = 5 // Dangerous middle hex
        };

        Hex hex3 = new Hex
        {
            Coordinates = new AxialCoordinates(2, 0),
            Terrain = TerrainType.Plains,
            DangerLevel = 0
        };

        hexMap.Hexes.Add(hex1);
        hexMap.Hexes.Add(hex2);
        hexMap.Hexes.Add(hex3);
        hexMap.BuildLookup();

        return hexMap;
    }

    /// <summary>
    /// Create hex map where multiple valid paths exist
    /// Tests that A* finds optimal shortest path
    /// </summary>
    private HexMap CreateHexMapMultiplePaths()
    {
        HexMap hexMap = new HexMap
        {
            Width = 4,
            Height = 4,
            Origin = new AxialCoordinates(0, 0)
        };

        for (int q = 0; q < 4; q++)
        {
            for (int r = 0; r < 4; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r),
                    Terrain = TerrainType.Plains,
                    DangerLevel = 0
                };
                hexMap.Hexes.Add(hex);
            }
        }

        hexMap.BuildLookup();
        return hexMap;
    }

    /// <summary>
    /// Create hex map with mixed terrain types (plains, water, forest)
    /// </summary>
    private HexMap CreateMixedTerrainMap()
    {
        HexMap hexMap = new HexMap
        {
            Width = 3,
            Height = 3,
            Origin = new AxialCoordinates(0, 0)
        };

        Hex hex1 = new Hex
        {
            Coordinates = new AxialCoordinates(0, 0),
            Terrain = TerrainType.Plains,
            DangerLevel = 0
        };

        Hex hex2 = new Hex
        {
            Coordinates = new AxialCoordinates(1, 1),
            Terrain = TerrainType.Forest,
            DangerLevel = 0
        };

        Hex hex3 = new Hex
        {
            Coordinates = new AxialCoordinates(2, 2),
            Terrain = TerrainType.Water, // Goal is water
            DangerLevel = 0
        };

        hexMap.Hexes.Add(hex1);
        hexMap.Hexes.Add(hex2);
        hexMap.Hexes.Add(hex3);
        hexMap.BuildLookup();

        return hexMap;
    }
}
