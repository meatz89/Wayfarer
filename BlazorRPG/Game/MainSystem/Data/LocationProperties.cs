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


public class LocationProperties
{
    public List<LocationPropertyChoiceEffect> ChoiceEffects { get; set; } = new();

    // Action Availability
    public LocationArchetypes? Archetype { get; set; }
    public bool IsArchetypeSet { get; private set; } = false;
    public ResourceTypes? Resource { get; set; }
    public bool IsResourceSet { get; private set; } = false;
    public CrowdDensity? CrowdDensity { get; set; }
    public bool IsCrowdDensitySet { get; private set; } = false;
    public LocationScale? LocationScale { get; set; }
    public bool IsLocationScaleSet { get; private set; } = false;

    // Action Availability
    public Accessability? Accessability { get; set; }
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
            case LocationPropertyTypes.Archetype:
                return Archetype;
            case LocationPropertyTypes.Resource:
                return Resource;
            case LocationPropertyTypes.CrowdDensity:
                return CrowdDensity;
            case LocationPropertyTypes.LocationScale:
                return LocationScale;

            case LocationPropertyTypes.Accessibility:
                return Accessability;
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
            case LocationPropertyTypes.Archetype:
                Archetype = (LocationArchetypes)value;
                IsArchetypeSet = true;
                break;
            case LocationPropertyTypes.Resource:
                Resource = (ResourceTypes)value;
                IsResourceSet = true;
                break;
            case LocationPropertyTypes.CrowdDensity:
                CrowdDensity = (CrowdDensity)value;
                IsCrowdDensitySet = true;
                break;
            case LocationPropertyTypes.LocationScale:
                LocationScale = (LocationScale)value;
                IsLocationScaleSet = true;
                break;


            case LocationPropertyTypes.Accessibility:
                Accessability = (Accessability)value;
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
