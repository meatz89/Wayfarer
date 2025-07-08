public class ActionAvailabilityService
{
    public bool IsActionAvailable(
        ActionDefinition template,
        Location location,
        LocationSpot locationSpot,
        WorldState worldState,
        Player player)
    {
        return true;
    }
}

public static class ActionTemplateExtensions
{
    public static bool IsValidForSpot(this ActionDefinition template, Location location, LocationSpot locationSpot, WorldState worldState, Player player)
    {
        ActionAvailabilityService service = new ActionAvailabilityService();
        bool isValid = service.IsActionAvailable(template, location, locationSpot, worldState, player);
        return isValid;
    }
}