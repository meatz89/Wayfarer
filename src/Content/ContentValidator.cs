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

        // Check venues have valid Venue location references
        foreach (Venue venue in _worldState.venues)
        {
            foreach (string locationSpotId in venue.LocationSpotIds)
            {
                if (!_worldState.venues.Any(ls => ls.Id == locationSpotId))
                {
                    result.AddMissingLocationSpot(locationSpotId, venue);
                }
            }
        }

        // ConnectedVenueIds validation removed - connections are on locations, not Venues

        return result;
    }
}