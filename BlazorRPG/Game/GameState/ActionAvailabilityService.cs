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
        if (!DoesActionMatchLocation(template, location)) return false;
        if (!DoesActionMatchLocationSpot(template, locationSpot)) return false;
        if (!DoesActionMatchWorldState(template, worldState)) return false;
        if (!DoesActionMatchPlayerState(template, playerState)) return false;

        return true;
    }

    private bool IsLaborActionAvailable(ActionTemplate template, Location location, PlayerState playerState)
    {
        if(template.ActionType == BasicActionTypes.Labor)
        {
            (LocationNames LocationName, BasicActionTypes ActionType) item = (location.LocationName, template.ActionType);
            bool isLaborAllowedAtLocation = playerState.LocationActionAvailability.Contains(item);
            return isLaborAllowedAtLocation;
        }
        return true;
    }

    private bool DoesActionMatchLocation(ActionTemplate template, Location location)
    {
        foreach (LocationPropertyCondition locationPropertyCondition in template.LocationPropertyConditions)
        {
            bool locationPropertyMatch = locationPropertyCondition.IsMet(location);
            if (!locationPropertyMatch) return false;
        }
        return true;
    }

    private bool DoesActionMatchLocationSpot(ActionTemplate template, LocationSpot locationSpot)
    {
        foreach (LocationSpotPropertyCondition locationSpotPropertyCondition in template.LocationSpotPropertyConditions)
        {
            bool locationSpotPropertyMatch = locationSpotPropertyCondition.IsMet(locationSpot);
            if (!locationSpotPropertyMatch) return false;
        }
        return true;
    }

    private bool DoesActionMatchWorldState(ActionTemplate template, WorldState worldState)
    {
        foreach (WorldStatePropertyCondition worldStatePropertyCondition in template.WorldStatePropertyConditions)
        {
            bool worldStatePropertyMatch = worldStatePropertyCondition.IsMet(worldState);
            if (!worldStatePropertyMatch) return false;
        }
        return true;
    }

    private bool DoesActionMatchPlayerState(ActionTemplate template, PlayerState playerState)
    {
        foreach (PlayerStatusPropertyCondition playerStatusPropertyConditions in template.PlayerStatusPropertyConditions)
        {
            bool playerStatusPropertyMatch = playerStatusPropertyConditions.IsMet(playerState);
            if (!playerStatusPropertyMatch) return false;
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