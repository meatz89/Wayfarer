public class LocationPropertyCondition
{
    public LocationPropertyTypes PropertyType { get; }
    public object ExpectedValue { get; }

    public LocationPropertyCondition(LocationPropertyTypes propertyType, object propertyValue)
    {
        PropertyType = propertyType;
        ExpectedValue = propertyValue;
    }

    public bool IsMet(Location location)
    {
        return PropertyType switch
        {
            LocationPropertyTypes.LocationArchetype => location.HasProperty((LocationArchetypes)ExpectedValue),
            LocationPropertyTypes.ResourceType => location.HasProperty((ResourceTypes)ExpectedValue),
            LocationPropertyTypes.CrowdDensity => location.HasProperty((CrowdDensity)ExpectedValue),
            LocationPropertyTypes.LocationScale => location.HasProperty((LocationScale)ExpectedValue),
            _ => throw new ArgumentException($"Invalid LocationPropertyType: {PropertyType}")
        };
    }

    public override string ToString()
    {
        return $"Location Requirement: {PropertyType} - {ExpectedValue}";
    }
}

public class LocationSpotPropertyCondition
{
    public LocationSpotPropertyTypes PropertyType { get; }
    public object ExpectedValue { get; }

    public LocationSpotPropertyCondition(LocationSpotPropertyTypes propertyType, object propertyValue)
    {
        PropertyType = propertyType;
        ExpectedValue = propertyValue;
    }

    public bool IsMet(LocationSpot locationSpot)
    {
        return PropertyType switch
        {
            LocationSpotPropertyTypes.Accessibility => locationSpot.HasProperty((Accessibility)ExpectedValue),
            LocationSpotPropertyTypes.Engagement => locationSpot.HasProperty((Engagement)ExpectedValue),
            LocationSpotPropertyTypes.Atmosphere => locationSpot.HasProperty((Atmosphere)ExpectedValue),
            LocationSpotPropertyTypes.RoomLayout => locationSpot.HasProperty((RoomLayout)ExpectedValue),
            LocationSpotPropertyTypes.Temperature => locationSpot.HasProperty((Temperature)ExpectedValue),
            _ => throw new ArgumentException($"Invalid LocationSpotPropertyType: {PropertyType}")
        };
    }

    public override string ToString()
    {
        return $"Location Spot Requirement: {PropertyType} - {ExpectedValue}";
    }
}

public class WorldStatePropertyCondition
{
    public WorldStatusTypes PropertyType { get; set; }
    public object ExpectedValue { get; set; }

    public WorldStatePropertyCondition(WorldStatusTypes worldStatus, object expectedValue)
    {
        PropertyType = worldStatus;
        ExpectedValue = expectedValue;
    }

    public bool IsMet(WorldState worldState)
    {
        return PropertyType switch
        {
            WorldStatusTypes.Time => worldState.HasProperty((TimeWindows)ExpectedValue),
            WorldStatusTypes.Weather => worldState.HasProperty((WeatherTypes)ExpectedValue),
            _ => throw new ArgumentException($"Invalid WorldStatusTypes: {PropertyType}")
        };
    }

    public override string ToString()
    {
        return $"{PropertyType} is {ExpectedValue.ToString()}";
    }
}

public class PlayerStatusPropertyCondition
{
    public PlayerStatusTypes PropertyType { get; set; }
    public object ExpectedValue { get; set; }

    public PlayerStatusPropertyCondition(PlayerStatusTypes worldStatus, object expectedValue)
    {
        PropertyType = worldStatus;
        ExpectedValue = expectedValue;
    }

    public bool IsMet(PlayerState playerState)
    {
        return PropertyType switch
        {
            PlayerStatusTypes.NegativeEffect => playerState.HasStatusEffect((PlayerNegativeStatus)ExpectedValue),
            PlayerStatusTypes.Reputation => playerState.HasReputation((PlayerReputationTypes)ExpectedValue),
            _ => throw new ArgumentException($"Invalid WorldStatusTypes: {PropertyType}")
        };
    }

    public override string ToString()
    {
        return $"{PropertyType} is {ExpectedValue.ToString()}";
    }
}


public enum LocationPropertyTypes
{
    LocationArchetype,
    ResourceType,
    CrowdDensity,
    LocationScale
}

public enum LocationSpotPropertyTypes
{
    Accessibility,
    Engagement,
    Atmosphere,
    RoomLayout,
    Temperature
}

public enum WorldStatusTypes
{
    Time,
    Weather
}

public enum PlayerStatusTypes
{
    NegativeEffect,
    Reputation
}
