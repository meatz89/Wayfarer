/// <summary>
/// Fluent builder for creating Location test data.
/// Provides sensible defaults and chainable configuration methods.
/// Eliminates hard-coded test fixtures - generates data programmatically.
/// </summary>
public class LocationBuilder
{
    private string _name;
    private LocationPurpose _purpose;
    private string _distanceHint;
    private LocationSafety _safety;
    private LocationPrivacy _privacy;
    private LocationActivity _activity;

    public LocationBuilder()
    {
        // Sensible defaults for minimal test setup
        _name = GenerateUniqueName("Location");
        _purpose = LocationPurpose.Generic;
        _distanceHint = "medium";
        _safety = LocationSafety.Neutral;
        _privacy = LocationPrivacy.Public;
        _activity = LocationActivity.Moderate;
    }

    public LocationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public LocationBuilder WithPurpose(LocationPurpose purpose)
    {
        _purpose = purpose;
        return this;
    }

    public LocationBuilder WithDistanceHint(string hint)
    {
        _distanceHint = hint;
        return this;
    }

    public LocationBuilder WithSafety(LocationSafety safety)
    {
        _safety = safety;
        return this;
    }

    public LocationBuilder WithPrivacy(LocationPrivacy privacy)
    {
        _privacy = privacy;
        return this;
    }

    public LocationBuilder WithActivity(LocationActivity activity)
    {
        _activity = activity;
        return this;
    }

    public Location Build()
    {
        Location location = new Location(_name)
        {
            Purpose = _purpose,
            DistanceHintForPlacement = _distanceHint,
            Safety = _safety,
            Privacy = _privacy,
            Activity = _activity
        };
        return location;
    }

    // Convenience factory methods for common scenarios
    public static Location Commerce()
    {
        return new LocationBuilder()
            .WithPurpose(LocationPurpose.Commerce)
            .WithName(GenerateUniqueName("Shop"))
            .Build();
    }

    public static Location Dwelling()
    {
        return new LocationBuilder()
            .WithPurpose(LocationPurpose.Dwelling)
            .WithName(GenerateUniqueName("Room"))
            .Build();
    }

    public static Location Worship()
    {
        return new LocationBuilder()
            .WithPurpose(LocationPurpose.Worship)
            .WithName(GenerateUniqueName("Temple"))
            .Build();
    }

    public static Location Transit()
    {
        return new LocationBuilder()
            .WithPurpose(LocationPurpose.Transit)
            .WithName(GenerateUniqueName("Road"))
            .Build();
    }

    public static Location StartLocation()
    {
        return new LocationBuilder()
            .WithDistanceHint("start")
            .WithName(GenerateUniqueName("StartLoc"))
            .Build();
    }

    public static Location NearLocation()
    {
        return new LocationBuilder()
            .WithDistanceHint("near")
            .WithName(GenerateUniqueName("NearLoc"))
            .Build();
    }

    public static Location FarLocation()
    {
        return new LocationBuilder()
            .WithDistanceHint("far")
            .WithName(GenerateUniqueName("FarLoc"))
            .Build();
    }

    private static string GenerateUniqueName(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}
