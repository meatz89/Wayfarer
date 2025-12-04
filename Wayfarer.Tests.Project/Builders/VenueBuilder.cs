/// <summary>
/// Fluent builder for creating Venue test data.
/// Provides sensible defaults and chainable configuration methods.
/// Supports spatial positioning for distance-based tests.
/// </summary>
public class VenueBuilder
{
    private string _name;
    private VenueType _type;
    private int _maxLocations;
    private AxialCoordinates _centerHex;
    private HexAllocationStrategy _allocation;

    public VenueBuilder()
    {
        _name = GenerateUniqueName("Venue");
        _type = VenueType.Market;
        _maxLocations = 10;
        _centerHex = new AxialCoordinates(0, 0);
        _allocation = HexAllocationStrategy.ClusterOf7;
    }

    public VenueBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public VenueBuilder WithType(VenueType type)
    {
        _type = type;
        return this;
    }

    public VenueBuilder WithMaxLocations(int max)
    {
        _maxLocations = max;
        return this;
    }

    public VenueBuilder WithCenterHex(AxialCoordinates hex)
    {
        _centerHex = hex;
        return this;
    }

    public VenueBuilder WithCenterHex(int q, int r)
    {
        _centerHex = new AxialCoordinates(q, r);
        return this;
    }

    public VenueBuilder WithAllocation(HexAllocationStrategy strategy)
    {
        _allocation = strategy;
        return this;
    }

    public Venue Build()
    {
        Venue venue = new Venue(_name)
        {
            Type = _type,
            MaxLocations = _maxLocations,
            CenterHex = _centerHex,
            HexAllocation = _allocation
        };
        return venue;
    }

    // Convenience factory methods for common venue types
    public static Venue Market()
    {
        return new VenueBuilder()
            .WithType(VenueType.Market)
            .WithName(GenerateUniqueName("Market"))
            .Build();
    }

    public static Venue Tavern()
    {
        return new VenueBuilder()
            .WithType(VenueType.Tavern)
            .WithName(GenerateUniqueName("Tavern"))
            .Build();
    }

    public static Venue Temple()
    {
        return new VenueBuilder()
            .WithType(VenueType.Temple)
            .WithName(GenerateUniqueName("Temple"))
            .Build();
    }

    public static Venue Workshop()
    {
        return new VenueBuilder()
            .WithType(VenueType.Workshop)
            .WithName(GenerateUniqueName("Workshop"))
            .Build();
    }

    public static Venue Merchant()
    {
        return new VenueBuilder()
            .WithType(VenueType.Merchant)
            .WithName(GenerateUniqueName("Merchant"))
            .Build();
    }

    public static Venue Wilderness()
    {
        return new VenueBuilder()
            .WithType(VenueType.Wilderness)
            .WithName(GenerateUniqueName("Wilderness"))
            .Build();
    }

    public static Venue AtCapacity(int capacity)
    {
        return new VenueBuilder()
            .WithMaxLocations(capacity)
            .WithName(GenerateUniqueName("FullVenue"))
            .Build();
    }

    public static Venue Near(AxialCoordinates playerPosition, int distance)
    {
        AxialCoordinates venuePos = new AxialCoordinates(
            playerPosition.Q + distance,
            playerPosition.R
        );
        return new VenueBuilder()
            .WithCenterHex(venuePos)
            .WithName(GenerateUniqueName("NearVenue"))
            .Build();
    }

    public static Venue Far(AxialCoordinates playerPosition, int distance)
    {
        AxialCoordinates venuePos = new AxialCoordinates(
            playerPosition.Q + distance,
            playerPosition.R
        );
        return new VenueBuilder()
            .WithCenterHex(venuePos)
            .WithName(GenerateUniqueName("FarVenue"))
            .Build();
    }

    private static string GenerateUniqueName(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}
