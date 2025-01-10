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

    public List<LocationNames> GetLocationResonances(LocationNames currentLocation)
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
            case LocationPropertyTypes.Scale:
                return locProperties.Scale == ((ScaleValue)locPropertyTypeValue).ScaleVariation;
            case LocationPropertyTypes.Exposure:
                return locProperties.Exposure == ((ExposureValue)locPropertyTypeValue).Exposure;
            case LocationPropertyTypes.Legality:
                return locProperties.Legality == ((LegalityValue)locPropertyTypeValue).Legality;
            case LocationPropertyTypes.Pressure:
                return locProperties.Pressure == ((PressureStateValue)locPropertyTypeValue).PressureState;
            case LocationPropertyTypes.Complexity:
                return locProperties.Complexity == ((ComplexityValue)locPropertyTypeValue).Complexity;
            case LocationPropertyTypes.Resource:
                return locProperties.Resource == ((ResourceValue)locPropertyTypeValue).Resource;
            case LocationPropertyTypes.CrowdLevel:
                return locProperties.CrowdLevel == ((CrowdLevelValue)locPropertyTypeValue).CrowdLevel;
            case LocationPropertyTypes.ReputationType:
                return locProperties.LocationReputationType == ((LocationReputationTypeValue)locPropertyTypeValue).ReputationType;
            default:
                return false;
        }
    }

}
