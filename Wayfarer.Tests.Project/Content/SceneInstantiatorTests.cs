using Xunit;

namespace Wayfarer.Tests.Content;

public class SceneInstantiatorTests
{
    [Theory]
    [InlineData(1, TimeBlocks.Morning, 1, 1)]
    [InlineData(1, TimeBlocks.Morning, 4, 4)]
    [InlineData(1, TimeBlocks.Midday, 1, 5)]
    [InlineData(1, TimeBlocks.Afternoon, 1, 9)]
    [InlineData(1, TimeBlocks.Evening, 1, 13)]
    [InlineData(2, TimeBlocks.Morning, 1, 17)]
    [InlineData(2, TimeBlocks.Midday, 3, 23)]
    [InlineData(5, TimeBlocks.Evening, 4, 80)]
    public void CalculateTimestamp_ReturnsCorrectTimestamp(int day, TimeBlocks timeBlock, int segment, long expected)
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        long result = InvokeCalculateTimestamp(instantiator, day, timeBlock, segment);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateTimestamp_UsesConstants()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        long day2Morning1 = InvokeCalculateTimestamp(instantiator, 2, TimeBlocks.Morning, 1);
        long day1Evening4 = InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Evening, 4);

        Assert.Equal(17, day2Morning1);
        Assert.Equal(16, day1Evening4);
        Assert.True(day2Morning1 > day1Evening4);
    }

    [Fact]
    public void CalculateTimestamp_OrderingIsMonotonic()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        List<long> timestamps = new List<long>
        {
            InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Morning, 1),
            InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Morning, 2),
            InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Midday, 1),
            InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Afternoon, 1),
            InvokeCalculateTimestamp(instantiator, 1, TimeBlocks.Evening, 4),
            InvokeCalculateTimestamp(instantiator, 2, TimeBlocks.Morning, 1)
        };

        for (int i = 0; i < timestamps.Count - 1; i++)
        {
            Assert.True(timestamps[i] < timestamps[i + 1],
                $"Timestamp {i} ({timestamps[i]}) should be less than timestamp {i+1} ({timestamps[i+1]})");
        }
    }

    [Fact]
    public void SelectWeightedRandomNPC_ReturnsNPCFromCandidates()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        List<NPC> candidates = new List<NPC>
        {
            new NPC("npc1", "NPC 1"),
            new NPC("npc2", "NPC 2"),
            new NPC("npc3", "NPC 3")
        };

        NPC result = InvokeSelectWeightedRandomNPC(instantiator, candidates);

        Assert.NotNull(result);
        Assert.Contains(result, candidates);
    }

    [Fact]
    public void SelectWeightedRandomNPC_ProducesVariedResults()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        List<NPC> candidates = new List<NPC>
        {
            new NPC("npc1", "NPC 1"),
            new NPC("npc2", "NPC 2"),
            new NPC("npc3", "NPC 3")
        };

        Dictionary<string, int> selectionCounts = new Dictionary<string, int>();
        int iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            NPC selected = InvokeSelectWeightedRandomNPC(instantiator, candidates);

            if (!selectionCounts.ContainsKey(selected.ID))
            {
                selectionCounts[selected.ID] = 0;
            }
            selectionCounts[selected.ID]++;
        }

        Assert.Equal(3, selectionCounts.Count);

        foreach (int count in selectionCounts.Values)
        {
            Assert.InRange(count, iterations / 6, iterations / 2);
        }
    }

    [Fact]
    public void SelectWeightedRandomLocation_ReturnsLocationFromCandidates()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        List<Location> candidates = new List<Location>
        {
            new Location("loc1", "Location 1"),
            new Location("loc2", "Location 2"),
            new Location("loc3", "Location 3")
        };

        Location result = InvokeSelectWeightedRandomLocation(instantiator, candidates);

        Assert.NotNull(result);
        Assert.Contains(result, candidates);
    }

    [Fact]
    public void SelectWeightedRandomLocation_ProducesVariedResults()
    {
        SceneInstantiator instantiator = CreateSceneInstantiator();

        List<Location> candidates = new List<Location>
        {
            new Location("loc1", "Location 1"),
            new Location("loc2", "Location 2"),
            new Location("loc3", "Location 3")
        };

        Dictionary<string, int> selectionCounts = new Dictionary<string, int>();
        int iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            Location selected = InvokeSelectWeightedRandomLocation(instantiator, candidates);

            if (!selectionCounts.ContainsKey(selected.Id))
            {
                selectionCounts[selected.Id] = 0;
            }
            selectionCounts[selected.Id]++;
        }

        Assert.Equal(3, selectionCounts.Count);

        foreach (int count in selectionCounts.Values)
        {
            Assert.InRange(count, iterations / 6, iterations / 2);
        }
    }

    private SceneInstantiator CreateSceneInstantiator()
    {
        GameWorld gameWorld = new GameWorld();
        SpawnConditionsEvaluator spawnEvaluator = new SpawnConditionsEvaluator(gameWorld);
        SceneNarrativeService narrativeService = new SceneNarrativeService(null);
        MarkerResolutionService markerService = new MarkerResolutionService();
        VenueGeneratorService venueGenerator = new VenueGeneratorService(new HexSynchronizationService());

        return new SceneInstantiator(
            gameWorld,
            spawnEvaluator,
            narrativeService,
            markerService,
            venueGenerator
        );
    }

    private long InvokeCalculateTimestamp(SceneInstantiator instantiator, int day, TimeBlocks timeBlock, int segment)
    {
        System.Reflection.MethodInfo method = typeof(SceneInstantiator)
            .GetMethod("CalculateTimestamp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (long)method.Invoke(instantiator, new object[] { day, timeBlock, segment });
    }

    private NPC InvokeSelectWeightedRandomNPC(SceneInstantiator instantiator, List<NPC> candidates)
    {
        System.Reflection.MethodInfo method = typeof(SceneInstantiator)
            .GetMethod("SelectWeightedRandomNPC", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (NPC)method.Invoke(instantiator, new object[] { candidates });
    }

    private Location InvokeSelectWeightedRandomLocation(SceneInstantiator instantiator, List<Location> candidates)
    {
        System.Reflection.MethodInfo method = typeof(SceneInstantiator)
            .GetMethod("SelectWeightedRandomLocation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (Location)method.Invoke(instantiator, new object[] { candidates });
    }
}
