public class ContentValidator
{
    private GameWorld _gameWorld;

    public ContentValidator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public ContentValidationResult ValidateContent()
    {
        ContentValidationResult result = new ContentValidationResult();

        // Check venues have valid Venue location references
        foreach (Venue venue in _gameWorld.Venues)
        {
            foreach (string locationSpotId in venue.LocationSpotIds)
            {
                if (!_gameWorld.Venues.Any(ls => ls.Id == locationSpotId))
                {
                    result.AddMissingLocationSpot(locationSpotId, venue);
                }
            }
        }

        // ConnectedVenueIds validation removed - connections are on locations, not Venues

        return result;
    }
}