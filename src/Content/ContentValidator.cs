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

        // Check venues have valid Venue spot references
        foreach (Venue venue in _worldState.locations)
        {
            foreach (string locationSpotId in venue.LocationSpotIds)
            {
                if (!_worldState.locationSpots.Any(ls => ls.Id == locationSpotId))
                {
                    result.AddMissingLocationSpot(locationSpotId, venue);
                }
            }
        }

        // ConnectedVenueIds validation removed - connections are on LocationSpots, not Venues

        return result;
    }
}