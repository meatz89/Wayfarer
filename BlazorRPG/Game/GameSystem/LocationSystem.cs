public class LocationSystem
{
    private readonly GameState gameState;
    private readonly List<Location> allLocations;
    private readonly List<LocationPropertyChoiceEffect> locationContextEffects;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allLocations = contentProvider.GetLocations();
        this.locationContextEffects = contentProvider.GetLocationArchetypeEffects();
    }

    public List<Location> GetLocations()
    {
        return allLocations;
    }

    public List<LocationNames> GetLocationConnections(LocationNames currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.TravelConnections;
    }

    public Location GetLocation(LocationNames locationName)
    {
        Location location = allLocations.FirstOrDefault(x => x.LocationName == locationName);
        return location;
    }

    public List<LocationSpot> GetLocationSpots(Location location)
    {
        return location.LocationSpots;
    }

    public LocationSpot GetLocationSpotForLocation(LocationNames locationName, string locationSpotType)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetLocationSpots(location);
        return spots.FirstOrDefault(x => x.Name == locationSpotType);
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(LocationNames locationName)
    {
        Location location = GetLocation(locationName);
        LocationProperties locationProperties = location.LocationProperties;

        List<LocationPropertyChoiceEffect> effects = new List<LocationPropertyChoiceEffect>();
        foreach (LocationPropertyChoiceEffect locationContextEffect in locationContextEffects)
        {
            if (IsLocationPropertyMatch(locationContextEffect.LocationProperty, locationProperties))
            {
                effects.Add(locationContextEffect);
            }
        }
        return effects;
    }

    private bool IsLocationPropertyMatch(LocationPropertyTypeValue locPropertyTypeValue, LocationProperties locProperties)
    {
        switch (locPropertyTypeValue.GetPropertyType())
        {
            case LocationPropertyTypes.Archetype:
                return locProperties.Archetype == ((ArchetypeValue)locPropertyTypeValue).Archetype;
            case LocationPropertyTypes.Resource:
                return locProperties.Resource == ((ResourceValue)locPropertyTypeValue).Resource;


            case LocationPropertyTypes.ActivityLevel:
                return locProperties.ActivityLevel == ((ActivityLevelValue)locPropertyTypeValue).ActivityLevel;
            case LocationPropertyTypes.Accessibility:
                return locProperties.Exposure == ((ExposureValue)locPropertyTypeValue).Exposure;
            case LocationPropertyTypes.Supervision:
                return locProperties.Supervision == ((SupervisionValue)locPropertyTypeValue).Supervision;

            case LocationPropertyTypes.Atmosphere:
                return locProperties.Atmosphere == ((AtmosphereValue)locPropertyTypeValue).Atmosphere;
            case LocationPropertyTypes.Space:
                return locProperties.Space == ((SpaceValue)locPropertyTypeValue).Space;
            case LocationPropertyTypes.Lighting:
                return locProperties.Lighting == ((LightingValue)locPropertyTypeValue).Lighting;
            case LocationPropertyTypes.Exposure:
                return locProperties.Accessability == ((AccessabilityLevelValue)locPropertyTypeValue).Accessability;
            default:
                return false;
        }
    }

}
