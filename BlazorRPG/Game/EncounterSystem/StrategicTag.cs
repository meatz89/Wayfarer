public class StrategicTag : IEncounterTag
{
    public string Name { get; }
    public IEnvironmentalProperty EnvironmentalProperty { get; }

    public StrategicTag(
        string name,
        IEnvironmentalProperty environmentalProperty)
    {
        Name = name;
        EnvironmentalProperty = environmentalProperty;
    }

    // Strategic tags are ALWAYS active
    public bool IsActive(BaseTagSystem tagSystem)
    {
        return true;
    }

    public void ApplyEffect(EncounterState state)
    {
        // Strategic tags don't directly apply effects to the state
        // Their effects are calculated during projection and then applied through the projection
    }

    public string GetEffectDescription()
    {
        string baseDesc = EnvironmentalProperty.ForDisplay();
        return baseDesc;
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}

public enum TimeWindows
{
    Night,
    Morning,
    Afternoon,
    Evening
}

public enum WeatherTypes
{
    Clear,
    Sunny,
    Windy,
    Stormy
}

public enum Accessibility
{
    Private, Communal, Public
}

public enum LocationType
{
    Rest, Commercial, Service
}

public enum RoomLayout
{
    Open, Cramped, Hazardous, Secluded
}

public enum Temperature
{
    Warm, Cold
}
public enum ResourceTypes
{
    Health,
    Concentration,
    Confidence
}

public enum LocationTypes
{
    None = 0,
    ForestRoad,
    Crossroads,
    AncientLibrary,
    Market
}
