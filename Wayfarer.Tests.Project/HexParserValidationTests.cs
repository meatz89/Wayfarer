using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Tests for HexParser validation logic after HashSet → List refactoring.
/// Validates that List-based duplicate detection correctly prevents coordinate conflicts.
/// </summary>
public class HexParserValidationTests
{
    [Fact]
    public void CoordinatePair_Equality_SameValues_ReturnsTrue()
    {
        CoordinatePair pair1 = new CoordinatePair(5, 10);
        CoordinatePair pair2 = new CoordinatePair(5, 10);

        bool result = pair1.Equals(pair2);

        Assert.True(result, "CoordinatePairs with same Q and R should be equal");
    }

    [Fact]
    public void CoordinatePair_Equality_DifferentQ_ReturnsFalse()
    {
        CoordinatePair pair1 = new CoordinatePair(5, 10);
        CoordinatePair pair2 = new CoordinatePair(6, 10);

        bool result = pair1.Equals(pair2);

        Assert.False(result, "CoordinatePairs with different Q should not be equal");
    }

    [Fact]
    public void CoordinatePair_Equality_DifferentR_ReturnsFalse()
    {
        CoordinatePair pair1 = new CoordinatePair(5, 10);
        CoordinatePair pair2 = new CoordinatePair(5, 11);

        bool result = pair1.Equals(pair2);

        Assert.False(result, "CoordinatePairs with different R should not be equal");
    }

    [Fact]
    public void CoordinatePair_GetHashCode_SameValues_SameHash()
    {
        CoordinatePair pair1 = new CoordinatePair(5, 10);
        CoordinatePair pair2 = new CoordinatePair(5, 10);

        int hash1 = pair1.GetHashCode();
        int hash2 = pair2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ParseHexMap_ValidHex_Success()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO
                {
                    q = 5,
                    r = 5,
                    terrain = "Plains",
                    dangerLevel = 0,
                    isDiscovered = true
                }
            }
        };

        HexMap result = HexParser.ParseHexMap(dto);

        Assert.NotNull(result);
        Assert.Single(result.Hexes);
        Assert.Equal(5, result.Hexes[0].Coordinates.Q);
        Assert.Equal(5, result.Hexes[0].Coordinates.R);
        Assert.Equal(TerrainType.Plains, result.Hexes[0].Terrain);
    }

    [Fact]
    public void ParseHexMap_DuplicateCoordinates_ThrowsException()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 5, r = 5, terrain = "Plains", dangerLevel = 0 },
                new HexDTO { q = 5, r = 5, terrain = "Forest", dangerLevel = 1 }
            }
        };

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() =>
            HexParser.ParseHexMap(dto));

        Assert.Contains("Duplicate hex coordinate", ex.Message);
        Assert.Contains("(5, 5)", ex.Message);
    }

    [Fact]
    public void ParseHexMap_InvalidTerrainType_ThrowsException()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 5, r = 5, terrain = "InvalidTerrain", dangerLevel = 0 }
            }
        };

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() =>
            HexParser.ParseHexMap(dto));

        Assert.Contains("invalid terrain type", ex.Message);
        Assert.Contains("InvalidTerrain", ex.Message);
    }

    [Fact]
    public void ParseHexMap_DangerLevelTooHigh_ThrowsException()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 5, r = 5, terrain = "Plains", dangerLevel = 15 }
            }
        };

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() =>
            HexParser.ParseHexMap(dto));

        Assert.Contains("invalid danger level", ex.Message);
        Assert.Contains("15", ex.Message);
    }

    [Fact]
    public void ParseHexMap_DangerLevelNegative_ThrowsException()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 5, r = 5, terrain = "Plains", dangerLevel = -1 }
            }
        };

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() =>
            HexParser.ParseHexMap(dto));

        Assert.Contains("invalid danger level", ex.Message);
    }

    [Fact]
    public void ParseHexMap_MultipleHexes_NoDuplicates_Success()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 0, r = 0, terrain = "Plains", dangerLevel = 0 },
                new HexDTO { q = 1, r = 0, terrain = "Forest", dangerLevel = 1 },
                new HexDTO { q = 0, r = 1, terrain = "Road", dangerLevel = 0 }
            }
        };

        HexMap result = HexParser.ParseHexMap(dto);

        Assert.NotNull(result);
        Assert.Equal(3, result.Hexes.Count);
    }

    [Fact]
    public void ParseHexMap_DuplicateLocationIds_ThrowsException()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 0, r = 0, terrain = "Plains", dangerLevel = 0, locationId = "village_01" },
                new HexDTO { q = 1, r = 0, terrain = "Forest", dangerLevel = 1, locationId = "village_01" }
            }
        };

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() =>
            HexParser.ParseHexMap(dto));

        Assert.Contains("referenced by multiple hexes", ex.Message);
        Assert.Contains("village_01", ex.Message);
    }

    [Fact]
    public void ParseHexMap_SameLocationIdNullWilderness_Success()
    {
        HexMapDTO dto = new HexMapDTO
        {
            width = 10,
            height = 10,
            originQ = 0,
            originR = 0,
            hexes = new List<HexDTO>
            {
                new HexDTO { q = 0, r = 0, terrain = "Plains", dangerLevel = 0, locationId = null },
                new HexDTO { q = 1, r = 0, terrain = "Forest", dangerLevel = 1, locationId = null }
            }
        };

        HexMap result = HexParser.ParseHexMap(dto);

        Assert.NotNull(result);
        Assert.Equal(2, result.Hexes.Count);
        Assert.All(result.Hexes, hex => Assert.Null(hex.LocationId));
    }
}
