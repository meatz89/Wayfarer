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

    public LocationSpot GetLocationSpotForLocation(LocationNames locationName, string locationSpot)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetLocationSpots(location);
        LocationSpot? locationSpot1 = spots.FirstOrDefault(x => x.Name == locationSpot);
        return locationSpot1;
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects(LocationNames locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        LocationSpot locationSpot = GetLocationSpotForLocation(locationName, locationSpotName);
        LocationProperties locationProperties = locationSpot.SpotProperties;

        List<LocationPropertyChoiceEffect> effects = new List<LocationPropertyChoiceEffect>();
        foreach (LocationPropertyChoiceEffect locationContextEffect in locationContextEffects)
        {
            effects.Add(locationContextEffect);
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
            case LocationPropertyTypes.CrowdDensity:
                return locProperties.CrowdDensity == ((CrowdDensityValue)locPropertyTypeValue).CrowdDensity;
            case LocationPropertyTypes.LocationScale:
                return locProperties.LocationScale == ((LocationScaleValue)locPropertyTypeValue).LocationScale;

            case LocationPropertyTypes.Accessibility:
                return locProperties.Accessability == ((AccessabilityValue)locPropertyTypeValue).Accessability;
            case LocationPropertyTypes.Engagement:
                return locProperties.Engagement == ((EngagementValue)locPropertyTypeValue).Engagement;
            case LocationPropertyTypes.Atmosphere:
                return locProperties.Atmosphere == ((AtmosphereValue)locPropertyTypeValue).Atmosphere;
            case LocationPropertyTypes.RoomLayout:
                return locProperties.RoomLayout == ((RoomLayoutValue)locPropertyTypeValue).RoomLayout;
            case LocationPropertyTypes.Temperature:
                return locProperties.Temperature == ((TemperatureValue)locPropertyTypeValue).Temperature;
            default:
                return false;
        }
    }

}
