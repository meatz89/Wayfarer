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

        // Check location spots have valid location references
        foreach (LocationSpot spot in _worldState.locationSpots)
        {
            if (!_worldState.locations.Any(l =>
            {
                return l.Id == spot.LocationName;
            }))
            {
                result.AddMissingLocation(spot.LocationName, spot);
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

        // Check location connections exist
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