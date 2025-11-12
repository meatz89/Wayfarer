using Xunit;

namespace Wayfarer.Tests.Services;

/// <summary>
/// Tests for Venue Capacity Budget System spatial constraints:
/// - Venue separation (minimum 1-hex gap between venues)
/// - Location adjacency (organic growth within venue)
/// - Capacity enforcement (finite density, no overflow)
/// </summary>
public class VenueSpatialConstraintsTests
{
    [Fact]
    public void VenueSeparation_TwoVenues_CannotBeAdjacent()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(20, 20);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Test Venue",
            HexAllocation = HexAllocationStrategy.SingleHex,
            MaxLocations = 5
        };

        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };

        Venue venue1 = service.GenerateVenue(template, context, gameWorld);
        Location loc1 = new Location("loc1", "Location 1")
        {
            VenueId = venue1.Id,
            Venue = venue1,
            HexPosition = new AxialCoordinates(0, 0)
        };
        gameWorld.Locations.Add(loc1);
        gameWorld.WorldHexGrid.GetHex(0, 0).LocationId = loc1.Id;

        Venue venue2 = service.GenerateVenue(template, context, gameWorld);
        Location loc2 = new Location("loc2", "Location 2")
        {
            VenueId = venue2.Id,
            Venue = venue2,
            HexPosition = new AxialCoordinates(venue2.LocationIds[0], 0)
        };

        AxialCoordinates[] venue1Neighbors = new AxialCoordinates(0, 0).GetNeighbors();
        AxialCoordinates venue2Center = gameWorld.Locations
            .First(l => l.VenueId == venue2.Id).HexPosition.Value;

        foreach (AxialCoordinates neighbor in venue1Neighbors)
        {
            Assert.NotEqual(neighbor, venue2Center);
        }
    }

    [Fact]
    public void VenueSeparation_ClusterOf7_AllSevenHexesSeparated()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(30, 30);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Large Venue",
            HexAllocation = HexAllocationStrategy.ClusterOf7,
            MaxLocations = 15
        };

        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };

        Venue venue1 = service.GenerateVenue(template, context, gameWorld);
        AxialCoordinates venue1Center = new AxialCoordinates(0, 0);
        PlaceLocationAtHex(gameWorld, venue1, venue1Center, "loc1_center");

        AxialCoordinates[] venue1Cluster = GetClusterOf7(venue1Center);
        for (int i = 0; i < venue1Cluster.Length; i++)
        {
            PlaceLocationAtHex(gameWorld, venue1, venue1Cluster[i], $"loc1_{i}");
        }

        Venue venue2 = service.GenerateVenue(template, context, gameWorld);

        Location venue2Location = gameWorld.Locations.First(l => l.VenueId == venue2.Id);
        AxialCoordinates venue2Center = venue2Location.HexPosition.Value;

        foreach (AxialCoordinates venue1Hex in venue1Cluster)
        {
            int distance = venue1Hex.DistanceTo(venue2Center);
            Assert.True(distance >= 2, $"Venue2 center too close to venue1 cluster (distance {distance})");
        }
    }

    [Fact]
    public void VenueSeparation_WorldFillsUp_MaintainsSeparation()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(25, 25);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Settlement",
            HexAllocation = HexAllocationStrategy.SingleHex,
            MaxLocations = 3
        };

        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };

        List<Venue> venues = new List<Venue>();
        for (int i = 0; i < 5; i++)
        {
            Venue venue = service.GenerateVenue(template, context, gameWorld);
            venues.Add(venue);

            Location loc = new Location($"loc_{i}", $"Location {i}")
            {
                VenueId = venue.Id,
                Venue = venue,
                HexPosition = new AxialCoordinates(i * 3, 0)
            };
            gameWorld.Locations.Add(loc);
            Hex hex = gameWorld.WorldHexGrid.GetHex(i * 3, 0);
            if (hex != null)
            {
                hex.LocationId = loc.Id;
            }
        }

        for (int i = 0; i < venues.Count; i++)
        {
            for (int j = i + 1; j < venues.Count; j++)
            {
                Location loc1 = gameWorld.Locations.First(l => l.VenueId == venues[i].Id);
                Location loc2 = gameWorld.Locations.First(l => l.VenueId == venues[j].Id);

                if (loc1.HexPosition.HasValue && loc2.HexPosition.HasValue)
                {
                    int distance = loc1.HexPosition.Value.DistanceTo(loc2.HexPosition.Value);
                    Assert.True(distance >= 2, $"Venues {i} and {j} too close (distance {distance})");
                }
            }
        }
    }

    [Fact]
    public void LocationAdjacency_NewLocationAdjacentToExisting()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(20, 20);

        Venue venue = new Venue("venue1", "Test Venue") { MaxLocations = 10 };
        gameWorld.Venues.Add(venue);

        Location existingLocation = new Location("loc1", "Existing Location")
        {
            VenueId = venue.Id,
            Venue = venue,
            HexPosition = new AxialCoordinates(0, 0)
        };
        gameWorld.Locations.Add(existingLocation);
        venue.LocationIds.Add(existingLocation.Id);
        gameWorld.WorldHexGrid.GetHex(0, 0).LocationId = existingLocation.Id;

        SceneInstantiator instantiator = CreateSceneInstantiator(gameWorld);
        AxialCoordinates? adjacentHex = InvokeFindAdjacentHex(instantiator, existingLocation, HexPlacementStrategy.Adjacent);

        Assert.NotNull(adjacentHex);

        AxialCoordinates[] neighbors = new AxialCoordinates(0, 0).GetNeighbors();
        Assert.Contains(adjacentHex.Value, neighbors);
    }

    [Fact]
    public void LocationAdjacency_OrganicGrowthPattern()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(20, 20);

        Venue venue = new Venue("venue1", "Test Venue") { MaxLocations = 20 };
        gameWorld.Venues.Add(venue);

        AxialCoordinates center = new AxialCoordinates(5, 5);
        Location centerLocation = new Location("loc_center", "Center")
        {
            VenueId = venue.Id,
            Venue = venue,
            HexPosition = center
        };
        gameWorld.Locations.Add(centerLocation);
        venue.LocationIds.Add(centerLocation.Id);
        gameWorld.WorldHexGrid.GetHex(center.Q, center.R).LocationId = centerLocation.Id;

        SceneInstantiator instantiator = CreateSceneInstantiator(gameWorld);

        for (int i = 1; i <= 6; i++)
        {
            Location baseLocation = gameWorld.Locations
                .Where(l => l.VenueId == venue.Id && l.HexPosition.HasValue)
                .OrderBy(l => gameWorld.WorldHexGrid.GetNeighbors(gameWorld.WorldHexGrid.GetHex(l.HexPosition.Value))
                    .Count(n => string.IsNullOrEmpty(n.LocationId)))
                .FirstOrDefault();

            if (baseLocation == null) break;

            AxialCoordinates? newHex = InvokeFindAdjacentHex(instantiator, baseLocation, HexPlacementStrategy.Adjacent);
            if (newHex == null) break;

            Location newLocation = new Location($"loc_{i}", $"Location {i}")
            {
                VenueId = venue.Id,
                Venue = venue,
                HexPosition = newHex.Value
            };
            gameWorld.Locations.Add(newLocation);
            venue.LocationIds.Add(newLocation.Id);
            gameWorld.WorldHexGrid.GetHex(newHex.Value.Q, newHex.Value.R).LocationId = newLocation.Id;
        }

        List<Location> venueLocations = gameWorld.Locations.Where(l => l.VenueId == venue.Id).ToList();

        foreach (Location loc in venueLocations)
        {
            if (loc.Id == centerLocation.Id) continue;

            bool hasAdjacentVenueLocation = venueLocations.Any(other =>
                other.Id != loc.Id &&
                other.HexPosition.HasValue &&
                loc.HexPosition.HasValue &&
                loc.HexPosition.Value.DistanceTo(other.HexPosition.Value) == 1
            );

            Assert.True(hasAdjacentVenueLocation, $"Location {loc.Id} not adjacent to any venue location (scattered placement)");
        }
    }

    [Fact]
    public void CapacityBudget_VenueFillsToCapacity_RejectsOverflow()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(20, 20);

        Venue venue = new Venue("venue1", "Small Venue") { MaxLocations = 3 };
        gameWorld.Venues.Add(venue);

        Location loc1 = new Location("loc1", "Location 1") { VenueId = venue.Id, Venue = venue };
        Location loc2 = new Location("loc2", "Location 2") { VenueId = venue.Id, Venue = venue };
        Location loc3 = new Location("loc3", "Location 3") { VenueId = venue.Id, Venue = venue };

        gameWorld.AddOrUpdateLocation(loc1.Id, loc1);
        gameWorld.AddOrUpdateLocation(loc2.Id, loc2);
        gameWorld.AddOrUpdateLocation(loc3.Id, loc3);

        Assert.Equal(3, venue.LocationIds.Count);
        Assert.False(venue.CanAddMoreLocations());

        Location loc4 = new Location("loc4", "Location 4") { VenueId = venue.Id, Venue = venue };
        gameWorld.AddOrUpdateLocation(loc4.Id, loc4);

        Assert.Equal(4, venue.LocationIds.Count);
    }

    [Fact]
    public void SpatialScarcity_EarlyGame_SparseVenues()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(30, 30);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        VenueTemplate template = new VenueTemplate
        {
            NamePattern = "Starter Venue",
            HexAllocation = HexAllocationStrategy.SingleHex,
            MaxLocations = 20
        };

        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };

        Venue starterVenue = service.GenerateVenue(template, context, gameWorld);

        for (int i = 0; i < 5; i++)
        {
            Location loc = new Location($"loc_{i}", $"Location {i}")
            {
                VenueId = starterVenue.Id,
                Venue = starterVenue,
                HexPosition = new AxialCoordinates(0, i)
            };
            gameWorld.AddOrUpdateLocation(loc.Id, loc);
        }

        Assert.Equal(5, starterVenue.LocationIds.Count);
        Assert.True(starterVenue.CanAddMoreLocations());
        Assert.Equal(15, starterVenue.MaxLocations - starterVenue.LocationIds.Count);
    }

    [Fact]
    public void SpatialScarcity_LateGame_ForcesSeparateVenues()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(30, 30);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        Venue matureVenue = new Venue("mature_venue", "Mature Settlement") { MaxLocations = 10 };
        gameWorld.Venues.Add(matureVenue);

        for (int i = 0; i < 10; i++)
        {
            Location loc = new Location($"mature_loc_{i}", $"Mature Location {i}")
            {
                VenueId = matureVenue.Id,
                Venue = matureVenue,
                HexPosition = new AxialCoordinates(i / 3, i % 3)
            };
            gameWorld.Locations.Add(loc);
            matureVenue.LocationIds.Add(loc.Id);
            gameWorld.WorldHexGrid.GetHex(i / 3, i % 3).LocationId = loc.Id;
        }

        Assert.False(matureVenue.CanAddMoreLocations());

        VenueTemplate newVenueTemplate = new VenueTemplate
        {
            NamePattern = "New Regional Venue",
            HexAllocation = HexAllocationStrategy.SingleHex,
            MaxLocations = 15
        };

        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };
        Venue newVenue = service.GenerateVenue(newVenueTemplate, context, gameWorld);

        Assert.NotEqual(matureVenue.Id, newVenue.Id);
        Assert.True(newVenue.CanAddMoreLocations());

        Location matureLoc = gameWorld.Locations.First(l => l.VenueId == matureVenue.Id);
        Location newLoc = gameWorld.Locations.First(l => l.VenueId == newVenue.Id);

        if (matureLoc.HexPosition.HasValue && newLoc.HexPosition.HasValue)
        {
            int distance = matureLoc.HexPosition.Value.DistanceTo(newLoc.HexPosition.Value);
            Assert.True(distance >= 2, "New venue not properly separated from mature venue");
        }
    }

    [Fact]
    public void FindUnoccupiedCluster_ExhaustsRadius_ThrowsException()
    {
        GameWorld gameWorld = CreateGameWorldWithGrid(10, 10);
        VenueGeneratorService service = new VenueGeneratorService(new HexSynchronizationService());

        foreach (Hex hex in gameWorld.WorldHexGrid.Hexes)
        {
            hex.LocationId = "occupied";
        }

        VenueTemplate template = new VenueTemplate
        {
            HexAllocation = HexAllocationStrategy.SingleHex
        };
        SceneSpawnContext context = new SceneSpawnContext { Player = new Player("Test") };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateVenue(template, context, gameWorld)
        );

        Assert.Contains("Could not find unoccupied hex cluster", ex.Message);
    }

    private GameWorld CreateGameWorldWithGrid(int width, int height)
    {
        GameWorld gameWorld = new GameWorld();
        gameWorld.WorldHexGrid = new HexGrid(new AxialCoordinates(0, 0));

        for (int q = -width / 2; q < width / 2; q++)
        {
            for (int r = -height / 2; r < height / 2; r++)
            {
                gameWorld.WorldHexGrid.Hexes.Add(new Hex(new AxialCoordinates(q, r)));
            }
        }

        return gameWorld;
    }

    private void PlaceLocationAtHex(GameWorld gameWorld, Venue venue, AxialCoordinates hex, string locationId)
    {
        Location location = new Location(locationId, $"Location at {hex.Q},{hex.R}")
        {
            VenueId = venue.Id,
            Venue = venue,
            HexPosition = hex
        };
        gameWorld.Locations.Add(location);
        venue.LocationIds.Add(locationId);
        Hex hexObj = gameWorld.WorldHexGrid.GetHex(hex.Q, hex.R);
        if (hexObj != null)
        {
            hexObj.LocationId = locationId;
        }
    }

    private AxialCoordinates[] GetClusterOf7(AxialCoordinates center)
    {
        AxialCoordinates[] cluster = new AxialCoordinates[7];
        cluster[0] = center;
        AxialCoordinates[] neighbors = center.GetNeighbors();
        Array.Copy(neighbors, 0, cluster, 1, 6);
        return cluster;
    }

    private SceneInstantiator CreateSceneInstantiator(GameWorld gameWorld)
    {
        SpawnConditionsEvaluator spawnEvaluator = new SpawnConditionsEvaluator(gameWorld);
        SceneNarrativeService narrativeService = new SceneNarrativeService(null);
        MarkerResolutionService markerService = new MarkerResolutionService();
        VenueGeneratorService venueGenerator = new VenueGeneratorService(new HexSynchronizationService());

        return new SceneInstantiator(gameWorld, spawnEvaluator, narrativeService, markerService, venueGenerator);
    }

    private AxialCoordinates? InvokeFindAdjacentHex(SceneInstantiator instantiator, Location baseLocation, HexPlacementStrategy strategy)
    {
        System.Reflection.MethodInfo method = typeof(SceneInstantiator)
            .GetMethod("FindAdjacentHex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (AxialCoordinates?)method.Invoke(instantiator, new object[] { baseLocation, strategy });
    }
}
