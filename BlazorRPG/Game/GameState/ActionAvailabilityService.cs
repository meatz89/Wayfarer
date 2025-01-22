public class ActionAvailabilityService
{
    // Core method to check if an action can be performed at a location spot
    public bool IsActionAvailable(ActionTemplate template, LocationSpotProperties locationProperties, LocationArchetypes locationArchetype)
    {
        // First validate the basic location requirements
        if (!AreBasicLocationRequirementsMet(locationArchetype, template, locationProperties))
        {
            return false;
        }

        // Then check specific spot requirements based on action type
        return DoesActionMatchSpotProperties(template.ActionType, locationProperties);
    }

    private bool AreBasicLocationRequirementsMet(LocationArchetypes locationArchetype, ActionTemplate template, LocationSpotProperties properties)
    {
        // Check if archetype matches
        if (template.LocationArchetype != locationArchetype)
        {
            return false;
        }

        return true;
    }

    private bool DoesActionMatchSpotProperties(BasicActionTypes actionType, LocationSpotProperties properties)
    {
        // Each action type has specific requirements for the spot it can be performed in
        return actionType switch
        {
            BasicActionTypes.Recover => IsRecoverActionValid(properties),
            BasicActionTypes.Persuade => IsPersuadeActionValid(properties),
            BasicActionTypes.Investigate => IsInvestigateActionValid(properties),
            BasicActionTypes.Mingle => IsMingleActionValid(properties),
            _ => false
        };
    }

    private bool IsRecoverActionValid(LocationSpotProperties properties)
    {
        bool isValid = properties.IsTemperatureSet
                    && properties.Temperature == Temperature.Warm;
        return isValid;
    }

    private bool IsPersuadeActionValid(LocationSpotProperties properties)
    {
        bool isValid = properties.IsEngagementSet
                    && properties.IsAccessabilitySet
                    && properties.Engagement == Engagement.Service
                    && properties.Accessibility == Accessability.Public;
        return isValid;
    }

    private bool IsInvestigateActionValid(LocationSpotProperties properties)
    {
        bool isValid = properties.IsRoomLayoutSet
                    && properties.IsAccessabilitySet
                    && properties.RoomLayout == RoomLayout.Secluded
                    && properties.Accessibility == Accessability.Private;
        return isValid;
    }

    private bool IsMingleActionValid(LocationSpotProperties properties)
    {
        bool isValid = properties.IsAtmosphereSet
                    && properties.IsAccessabilitySet
                    && properties.Atmosphere == Atmosphere.Social
                    && properties.Accessibility == Accessability.Public;
        return isValid;
    }
}

// Extension to ActionTemplate to make the checking more elegant
public static class ActionTemplateExtensions
{
    public static bool IsValidForSpot(this ActionTemplate template, Location location, LocationSpot spot)
    {
        ActionAvailabilityService service = new ActionAvailabilityService();
        return service.IsActionAvailable(template, spot.SpotProperties, location.LocationArchetype);
    }
}