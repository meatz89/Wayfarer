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

    public List<Location> GetAllLocations()
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

        List<LocationPropertyChoiceEffect> effects = new List<LocationPropertyChoiceEffect>();
        foreach (LocationPropertyChoiceEffect locationContextEffect in locationContextEffects)
        {
            effects.Add(locationContextEffect);
        }
        return effects;
    }

}
