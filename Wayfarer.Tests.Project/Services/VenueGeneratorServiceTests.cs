using Xunit;

namespace Wayfarer.Tests.Services;

public class VenueGeneratorServiceTests
{
    [Fact]
    public void ShuffleList_ProducesUniformDistribution()
    {
        VenueGeneratorService service = new VenueGeneratorService(null);
        List<int> original = new List<int> { 1, 2, 3, 4, 5 };

        Dictionary<string, int> permutationCounts = new Dictionary<string, int>();
        int iterations = 10000;

        for (int i = 0; i < iterations; i++)
        {
            List<int> shuffled = new List<int>(original);
            InvokeShuffleList(service, shuffled);

            string key = string.Join(",", shuffled);
            if (!permutationCounts.ContainsKey(key))
            {
                permutationCounts[key] = 0;
            }
            permutationCounts[key]++;
        }

        int expectedCount = iterations / 120;
        int tolerance = expectedCount / 2;

        foreach (int count in permutationCounts.Values)
        {
            Assert.InRange(count, expectedCount - tolerance, expectedCount + tolerance);
        }

        Assert.True(permutationCounts.Count >= 100);
    }

    [Fact]
    public void ShuffleList_EmptyList_DoesNotThrow()
    {
        VenueGeneratorService service = new VenueGeneratorService(null);
        List<int> empty = new List<int>();

        InvokeShuffleList(service, empty);

        Assert.Empty(empty);
    }

    [Fact]
    public void ShuffleList_SingleElement_RemainsUnchanged()
    {
        VenueGeneratorService service = new VenueGeneratorService(null);
        List<int> single = new List<int> { 42 };

        InvokeShuffleList(service, single);

        Assert.Single(single);
        Assert.Equal(42, single[0]);
    }

    [Fact]
    public void ShuffleList_PreservesAllElements()
    {
        VenueGeneratorService service = new VenueGeneratorService(null);
        List<int> original = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        List<int> shuffled = new List<int>(original);

        InvokeShuffleList(service, shuffled);

        Assert.Equal(original.Count, shuffled.Count);
        foreach (int item in original)
        {
            Assert.Contains(item, shuffled);
        }
    }

    [Fact]
    public void FindUnoccupiedCluster_SingleHex_FindsFirstAvailable()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(10, 10);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        Hex hex = gameWorld.WorldHexGrid.GetHex(1, 0);
        Assert.NotNull(hex);
        Assert.Null(hex.LocationId);

        AxialCoordinates result = InvokeFindUnoccupiedCluster(
            service,
            HexAllocationStrategy.SingleHex,
            gameWorld
        );

        Assert.NotNull(result);
        Assert.True(result.Q >= 0 || result.R >= 0);
    }

    [Fact]
    public void FindUnoccupiedCluster_AllOccupied_ThrowsInvalidOperationException()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(5, 5);

        foreach (Hex hex in gameWorld.WorldHexGrid.Hexes)
        {
            hex.LocationId = "occupied";
        }

        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            InvokeFindUnoccupiedCluster(service, HexAllocationStrategy.SingleHex, gameWorld)
        );

        Assert.Contains("Could not find unoccupied hex cluster", ex.Message);
    }

    [Fact]
    public void GenerateVenue_CreatesVenueWithCorrectProperties()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(20, 20);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Test Venue",
            DescriptionPattern = "Test Description",
            Type = VenueType.Settlement,
            Tier = 2,
            MaxLocations = 15,
            District = "test_district",
            HexAllocation = HexAllocationStrategy.SingleHex
        };

        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = new Player("TestPlayer")
        };

        Venue result = service.GenerateVenue(template, context, gameWorld);

        Assert.NotNull(result);
        Assert.Equal("Test Venue", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(VenueType.Settlement, result.Type);
        Assert.Equal(2, result.Tier);
        Assert.Equal(15, result.MaxLocations);
        Assert.Equal("test_district", result.District);
        Assert.False(result.IsSkeleton);
        Assert.Contains(result, gameWorld.Venues);
    }

    [Fact]
    public void GenerateVenue_NoDistrictProvided_DefaultsToWilderness()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(10, 10);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Test Venue",
            District = null,
            HexAllocation = HexAllocationStrategy.SingleHex
        };

        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = new Player("TestPlayer"),
            CurrentLocation = null
        };

        Venue result = service.GenerateVenue(template, context, gameWorld);

        Assert.Equal("wilderness", result.District);
    }

    [Fact]
    public void GenerateVenue_InheritsDistrictFromContext()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(10, 10);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        Venue contextVenue = new Venue("context_venue", "Context Venue")
        {
            District = "upper_wards"
        };

        Location contextLocation = new Location("loc1", "Context Location")
        {
            Venue = contextVenue
        };

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Test Venue",
            District = null,
            HexAllocation = HexAllocationStrategy.SingleHex
        };

        SceneSpawnContext context = new SceneSpawnContext
        {
            Player = new Player("TestPlayer"),
            CurrentLocation = contextLocation
        };

        Venue result = service.GenerateVenue(template, context, gameWorld);

        Assert.Equal("upper_wards", result.District);
    }

    private GameWorld CreateGameWorldWithGrid(int width, int height)
    {
        GameWorld gameWorld = new GameWorld();
        gameWorld.WorldHexGrid = new HexGrid(new AxialCoordinates(0, 0));

        for (int q = -width/2; q < width/2; q++)
        {
            for (int r = -height/2; r < height/2; r++)
            {
                gameWorld.WorldHexGrid.Hexes.Add(new Hex(new AxialCoordinates(q, r)));
            }
        }

        return gameWorld;
    }

    private void InvokeShuffleList<T>(VenueGeneratorService service, List<T> list)
    {
        System.Reflection.MethodInfo method = typeof(VenueGeneratorService)
            .GetMethod("ShuffleList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));
        genericMethod.Invoke(service, new object[] { list });
    }

    private AxialCoordinates InvokeFindUnoccupiedCluster(
        VenueGeneratorService service,
        HexAllocationStrategy strategy,
        GameWorld gameWorld)
    {
        System.Reflection.MethodInfo method = typeof(VenueGeneratorService)
            .GetMethod("FindUnoccupiedCluster", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (AxialCoordinates)method.Invoke(service, new object[] { strategy, gameWorld });
    }
}
