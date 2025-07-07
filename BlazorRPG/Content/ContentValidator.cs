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

        foreach (ActionDefinition actionDefinition in _worldState.actions)
        {
            if (!_worldState.locationSpots.Any((Func<LocationSpot, bool>)(ls =>
            {
                return ls.SpotID == actionDefinition.LocationSpotId;
            })))
            {
                // Need to get the locationId for this spot, not just use the spotId twice
                string locationIdForSpot = _worldState.GetLocationIdForSpot(actionDefinition.LocationSpotId);
                result.AddMissingAction(locationIdForSpot, actionDefinition.LocationSpotId, actionDefinition);
            }
        }

        foreach (Location location in _worldState.locations)
        {
            if (location.Connections != null)
            {
                foreach (string connectedLocationId in location.Connections)
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