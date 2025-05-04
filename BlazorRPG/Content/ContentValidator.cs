public class ContentValidator
{
    private readonly WorldState _worldState;

    public ContentValidator(GameState gameState)
    {
        _worldState = gameState.WorldState;
    }

    public ContentValidationResult ValidateContent()
    {
        ContentValidationResult result = new ContentValidationResult();

        // Check locations have valid location spot references
        foreach (Location location in _worldState.locations)
        {
            foreach (string locationSpotId in location.LocationSpotIds)
            {
                if (!_worldState.locationSpots.Any(ls => ls.Id == locationSpotId))
                {
                    result.AddMissingLocationSpot(locationSpotId, location);
                }
            }
        }

        // Check location spot actions exist
        foreach (LocationSpot spot in _worldState.locationSpots)
        {
            foreach (SpotLevel level in spot.LevelData)
            {
                foreach (string actionId in level.AddedActionIds)
                {
                    if (!_worldState.actions.Any(a =>
                    {
                        return a.Id == actionId;
                    }))
                    {
                        result.AddMissingAction(actionId, spot);
                    }
                }
            }
        }

        foreach (Location location in _worldState.locations)
        {
            if (location.ConnectedTo != null)
            {
                foreach (string connectedLocationId in location.ConnectedTo)
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