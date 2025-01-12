
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
    Ale
}

// --- Archetype ---
public enum LocationArchetypes
{
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
    CraftsmanWorkshop
}

public enum LocationPropertyTypes
{
    Archetype,
    Resource,

    ActivityLevel,
    Accessibility,
    Supervision,

    Atmosphere,
    Space,
    Lighting,
    Exposure,
}

public class LocationProperties
{
    // Action Availability
    public LocationArchetypes? Archetype { get; set; }
    public bool IsArchetypeSet { get; private set; } = false;
    public ResourceTypes? Resource { get; set; }
    public bool IsResourceSet { get; private set; } = false;

    // Action Availability
    public AccessibilityTypes? Accessability { get; set; }
    public bool IsAccessabilitySet { get; private set; } = false;
    public ActivityLevelTypes? Activity { get; set; }
    public bool IsActivitySet { get; private set; } = false;
    public SupervisionTypes? Supervision { get; set; }
    public bool IsSupervisionSet { get; private set; } = false;


    // Action Availability
    public AtmosphereTypes? Atmosphere { get; set; }
    public bool IsAtmosphereSet { get; private set; } = false;
    public SpaceTypes? Space { get; set; }
    public bool IsSpaceSet { get; private set; } = false;
    public LightingTypes? Lighting { get; set; }
    public bool IsLightingSet { get; private set; } = false;
    public ExposureTypes? Exposure { get; set; }
    public bool IsExposureSet { get; private set; } = false;

    public object GetProperty(LocationPropertyTypes propertyType)
    {
        switch (propertyType)
        {
            case LocationPropertyTypes.Archetype:
                return Archetype;
            case LocationPropertyTypes.Resource:
                return Resource;

            case LocationPropertyTypes.Accessibility:
                return Accessability;
            case LocationPropertyTypes.ActivityLevel:
                return Activity;
            case LocationPropertyTypes.Supervision:
                return Supervision;

            case LocationPropertyTypes.Atmosphere:
                return Space;
            case LocationPropertyTypes.Space:
                return Atmosphere;
            case LocationPropertyTypes.Lighting:
                return Lighting;
            case LocationPropertyTypes.Exposure:
                return Exposure;

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

            case LocationPropertyTypes.Accessibility:
                Accessability = (AccessibilityTypes)value;
                IsAccessabilitySet = true;
                break;
            case LocationPropertyTypes.ActivityLevel:
                Activity = (ActivityLevelTypes)value;
                IsActivitySet = true;
                break;
            case LocationPropertyTypes.Supervision:
                Supervision = (SupervisionTypes)value;
                IsSupervisionSet = true;
                break;

            case LocationPropertyTypes.Atmosphere:
                Atmosphere = (AtmosphereTypes)value;
                IsAtmosphereSet = true;
                break;
            case LocationPropertyTypes.Space:
                Space = (SpaceTypes)value;
                IsSpaceSet = true;
                break;
            case LocationPropertyTypes.Lighting:
                Lighting = (LightingTypes)value;
                IsLightingSet = true;
                break;
            case LocationPropertyTypes.Exposure:
                Exposure = (ExposureTypes)value;
                IsExposureSet = true;
                break;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}

public enum ActivityLevelTypes
{
    Deserted, Quiet, Bustling
}

public enum AccessibilityTypes
{
    Private, Restricted, Public
}

public enum SupervisionTypes
{
    Unsupervised, Patrolled, Watched
}


public enum AtmosphereTypes
{
    Formal, Causal, Tense
}

public enum SpaceTypes
{
    Open, Cramped, Hazardous
}

public enum LightingTypes
{
    Bright, Dim, Dark
}

public enum ExposureTypes
{
    Indoor, Outdoor
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



public class ActivityLevelValue : LocationPropertyTypeValue
{
    public ActivityLevelTypes ActivityLevel { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.ActivityLevel;
    }
}

public class AccessabilityLevelValue : LocationPropertyTypeValue
{
    public AccessibilityTypes Accessability { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Exposure;
    }
}

public class SupervisionValue : LocationPropertyTypeValue
{
    public SupervisionTypes Supervision { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Supervision;
    }
}


public class AtmosphereValue : LocationPropertyTypeValue
{
    public AtmosphereTypes Atmosphere { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Atmosphere;
    }
}

public class SpaceValue : LocationPropertyTypeValue
{
    public SpaceTypes Space { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Space;
    }
}

public class LightingValue : LocationPropertyTypeValue
{
    public LightingTypes Lighting { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Lighting;
    }
}

public class ExposureValue : LocationPropertyTypeValue
{
    public ExposureTypes Exposure { get; set; }
    public override LocationPropertyTypes GetPropertyType()
    {
        return LocationPropertyTypes.Accessibility;
    }
}
