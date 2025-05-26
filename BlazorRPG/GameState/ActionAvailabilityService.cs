public class ActionAvailabilityService
{
    public bool IsActionAvailable(
        ActionDefinition template,
        Location location,
        LocationSpot locationSpot,
        WorldState worldState,
        Player playerState)
    {
        return true;
    }
}

public static class ActionTemplateExtensions
{
    public static bool IsValidForSpot(this ActionDefinition template, Location location, LocationSpot locationSpot, WorldState worldState, Player playerState)
    {
        ActionAvailabilityService service = new ActionAvailabilityService();
        bool isValid = service.IsActionAvailable(template, location, locationSpot, worldState, playerState);
        return isValid;
    }
}