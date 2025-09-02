public class ContentValidator
{
    private WorldState _worldState;

    public ContentValidator(GameWorld gameWorld)
    {
        _worldState = gameWorld.WorldState;
    }

    public ContentValidationResult ValidateContent()
    {
        ContentValidationResult result = new ContentValidationResult();

        // Check locations have valid location spot references
        foreach (Location location in _worldState.locations)
        {
            foreach (string locationSpotId in location.LocationSpotIds)
            {
                if (!_worldState.locationSpots.Any(ls => ls.SpotID == locationSpotId))
                {
                    result.AddMissingLocationSpot(locationSpotId, location);
                }
            }
        }

        // Actions removed - using letter queue system

        foreach (Location location in _worldState.locations)
        {
            if (location.ConnectedLocationIds != null)
            {
                foreach (string connectedLocationId in location.ConnectedLocationIds)
                {
                    if (!_worldState.locations.Any(l =>
                    {
                        return l.Id == connectedLocationId;
                    }))
                    {
                        result.AddMissingConnectedLocation(connectedLocationId, location);
                    }
                }
            }
        }

        return result;
    }
}