/// <summary>
/// Fluent builder for creating GameWorld test scenarios.
/// Provides pre-configured scenarios for common test cases.
/// Handles HexMap initialization and Player setup.
/// </summary>
public class GameWorldBuilder
{
    private List<Venue> _venues;
    private List<Location> _locations;
    private HexMap _hexMap;
    private AxialCoordinates _playerPosition;

    public GameWorldBuilder()
    {
        _venues = new List<Venue>();
        _locations = new List<Location>();
        _hexMap = CreateDefaultHexMap();
        _playerPosition = new AxialCoordinates(0, 0);
    }

    public GameWorldBuilder WithVenue(Venue venue)
    {
        _venues.Add(venue);
        return this;
    }

    public GameWorldBuilder WithVenues(List<Venue> venues)
    {
        _venues.AddRange(venues);
        return this;
    }

    public GameWorldBuilder WithLocation(Location location)
    {
        _locations.Add(location);
        return this;
    }

    public GameWorldBuilder WithPlayerAt(int q, int r)
    {
        _playerPosition = new AxialCoordinates(q, r);
        return this;
    }

    public GameWorldBuilder WithPlayerAt(AxialCoordinates position)
    {
        _playerPosition = position;
        return this;
    }

    public GameWorld Build()
    {
        GameWorld world = new GameWorld();

        // Add venues to GameWorld (Venues is public List<Venue>)
        foreach (Venue venue in _venues)
        {
            world.Venues.Add(venue);
        }

        // Add locations to GameWorld (use AddOrUpdateLocation with name parameter)
        foreach (Location location in _locations)
        {
            world.AddOrUpdateLocation(location.Name, location);
        }

        // Set hex map
        world.WorldHexGrid = _hexMap;

        return world;
    }

    public (GameWorld world, Player player) BuildWithPlayer()
    {
        GameWorld world = Build();

        Player player = new Player()
        {
            Name = "TestPlayer",
            CurrentPosition = _playerPosition
        };

        return (world, player);
    }

    private HexMap CreateDefaultHexMap()
    {
        // Create hex map with basic hexes for testing
        HexMap map = new HexMap
        {
            Width = 100,
            Height = 100,
            Origin = new AxialCoordinates(0, 0)
        };

        // Populate with hexes in a 100x100 grid
        for (int q = -50; q <= 50; q++)
        {
            for (int r = -50; r <= 50; r++)
            {
                Hex hex = new Hex
                {
                    Coordinates = new AxialCoordinates(q, r)
                };
                map.Hexes.Add(hex);
            }
        }

        // Build lookup dictionary for fast coordinate access
        map.BuildLookup();

        return map;
    }

    // Convenience factory methods for common scenarios
    public static GameWorld Empty()
    {
        return new GameWorldBuilder().Build();
    }

    public static GameWorld WithCommerceVenues()
    {
        List<Venue> venues = new List<Venue>
        {
            VenueBuilder.Market(),
            VenueBuilder.Merchant(),
            VenueBuilder.Workshop()
        };

        return new GameWorldBuilder()
            .WithVenues(venues)
            .Build();
    }

    public static GameWorld WithMixedVenues()
    {
        List<Venue> venues = new List<Venue>
        {
            VenueBuilder.Market(),
            VenueBuilder.Tavern(),
            VenueBuilder.Temple(),
            VenueBuilder.Wilderness()
        };

        return new GameWorldBuilder()
            .WithVenues(venues)
            .Build();
    }

    public static GameWorld WithDwellingVenues()
    {
        List<Venue> venues = new List<Venue>
        {
            VenueBuilder.Tavern()
        };

        return new GameWorldBuilder()
            .WithVenues(venues)
            .Build();
    }

    public static GameWorld WithWorshipVenues()
    {
        List<Venue> venues = new List<Venue>
        {
            VenueBuilder.Temple()
        };

        return new GameWorldBuilder()
            .WithVenues(venues)
            .Build();
    }
}
