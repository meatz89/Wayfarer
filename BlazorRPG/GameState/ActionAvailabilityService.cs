public class ActionAvailabilityService
{
    // Core method to check if an action can be performed at a location spot
    public bool IsActionAvailable(
        ActionTemplate template,
        Location location,
        LocationSpot locationSpot,
        WorldState worldState,
        PlayerState playerState)
    {
        if (!IsLaborActionAvailable(template, location, playerState)) return false;

        return true;
    }

    private bool IsLaborActionAvailable(ActionTemplate template, Location location, PlayerState playerState)
    {
        if (template.ActionType == BasicActionTypes.Labor)
        {
            (LocationNames LocationName, BasicActionTypes ActionType) item = (location.LocationName, template.ActionType);
            bool isLaborAllowedAtLocation = playerState.LocationActionAvailability.Contains(item);
            return isLaborAllowedAtLocation;
        }
        return true;
    }
}

// Extension to ActionTemplate to make the checking more elegant
public static class ActionTemplateExtensions
{
    public static bool IsValidForSpot(this ActionTemplate template, Location location, LocationSpot locationSpot, WorldState worldState, PlayerState playerState)
    {
        ActionAvailabilityService service = new ActionAvailabilityService();
        bool isValid = service.IsActionAvailable(template, location, locationSpot, worldState, playerState);
        return isValid;
    }
}