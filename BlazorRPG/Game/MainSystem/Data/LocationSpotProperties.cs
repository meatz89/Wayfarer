// For LOCATION

// --- Resource ---
public enum ResourceTypes
{
    None = 0,
    Food,
    Wood,
    Fish,
    Herbs,
    Cloth,
    Any,
    Ale,
    Metal
}

// --- Archetype ---
public enum LocationArchetypes
{
    Undefined = 0,
    Tavern,
    Market,
    Forest,
    Road,
    Field,
    Dock,
    Warehouse,
    Factory,
    Workshop,
    Shop,
    Garden,
    None,
    Library,
    ConstructionSite,
    Docks,
    CraftsmanWorkshop,
    Crossroads
}

public enum CrowdDensity
{
    Deserted, Quiet, Busy
}

public enum LocationScale
{
    Small, Medium, Large
}


public class LocationSpotProperties
{
    public List<LocationPropertyChoiceEffect> ChoiceEffects { get; set; } = new();

    // Action Availability
    public Accessability? Accessibility { get; set; }
    public bool IsAccessabilitySet { get; private set; } = false;
    public Engagement? Engagement { get; set; }
    public bool IsEngagementSet { get; private set; } = false;
    public Atmosphere? Atmosphere { get; set; }
    public bool IsAtmosphereSet { get; private set; } = false;
    public RoomLayout? RoomLayout { get; set; }
    public bool IsRoomLayoutSet { get; private set; } = false;
    public Temperature? Temperature { get; set; }
    public bool IsTemperatureSet { get; private set; } = false;

    public object GetProperty(LocationPropertyTypes propertyType)
    {
        switch (propertyType)
        {
            case LocationPropertyTypes.Accessibility:
                return Accessibility;
            case LocationPropertyTypes.Engagement:
                return Engagement;
            case LocationPropertyTypes.Atmosphere:
                return Atmosphere;
            case LocationPropertyTypes.RoomLayout:
                return RoomLayout;
            case LocationPropertyTypes.Temperature:
                return Temperature;

            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }

    // SetProperty now also sets the corresponding IsSet flag
    public void SetProperty(LocationPropertyTypes propertyType, object value)
    {
        switch (propertyType)
        {
            case LocationPropertyTypes.Accessibility:
                Accessibility = (Accessability)value;
                IsAccessabilitySet = true;
                break;
            case LocationPropertyTypes.Engagement:
                Engagement = (Engagement)value;
                IsEngagementSet = true;
                break;
            case LocationPropertyTypes.Atmosphere:
                Atmosphere = (Atmosphere)value;
                IsAtmosphereSet = true;
                break;
            case LocationPropertyTypes.RoomLayout:
                RoomLayout = (RoomLayout)value;
                IsRoomLayoutSet = true;
                break;
            case LocationPropertyTypes.Temperature:
                Temperature = (Temperature)value;
                IsTemperatureSet = true;
                break;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}

public abstract class LocationPropertyTypeValue
{
    public abstract LocationPropertyTypes GetPropertyType();
}

public class ArchetypeValue : LocationPropertyTypeValue
{
    public LocationArchetypes Archetype { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Archetype;
    }
}

public class ResourceValue : LocationPropertyTypeValue
{
    public ResourceTypes Resource { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Resource;
    }
}

public class CrowdDensityValue : LocationPropertyTypeValue
{
    public CrowdDensity CrowdDensity { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.CrowdDensity;
    }
}

public class LocationScaleValue : LocationPropertyTypeValue
{
    public LocationScale LocationScale { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.LocationScale;
    }
}

public class AccessabilityValue : LocationPropertyTypeValue
{
    public Accessability Accessability { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Accessibility;
    }
}

public class EngagementValue : LocationPropertyTypeValue
{
    public Engagement Engagement { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Engagement;
    }
}
public class AtmosphereValue : LocationPropertyTypeValue
{
    public Atmosphere Atmosphere { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Atmosphere;
    }
}

public class RoomLayoutValue : LocationPropertyTypeValue
{
    public RoomLayout RoomLayout { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.RoomLayout;
    }
}

public class TemperatureValue : LocationPropertyTypeValue
{
    public Temperature Temperature { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Temperature;
    }
}
