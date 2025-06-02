public class ActionFactory
{
    private ActionRepository actionRepository;
    private LocationActionProcessor encounterFactory;
    private Player playerState;
    private WorldState worldState;

    public ActionFactory(
        ActionRepository actionRepository,
        GameWorld gameState,
        LocationActionProcessor encounterFactory)
    {
        this.actionRepository = actionRepository;
        this.encounterFactory = encounterFactory;
        this.playerState = gameState.Player;
        this.worldState = gameState.WorldState;
    }

    public LocationAction CreateActionFromTemplate(
        ActionDefinition template, 
        string location, 
        string locationSpot, 
        ActionExecutionTypes actionType)
    {
        LocationAction locationAction = new LocationAction();
        locationAction.ActionId = template.Id;
        locationAction.Name = template.Name;
        locationAction.ObjectiveDescription = template.Description;
        locationAction.LocationId = location;
        locationAction.LocationSpotId = locationSpot;

        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            locationAction.DestinationLocation = template.MoveToLocation;
        }
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            locationAction.DestinationLocationSpot = template.MoveToLocationSpot;
        }
        
        locationAction.RequiredCardType = SkillCategories.Physical;
        locationAction.ActionExecutionType = actionType;

        locationAction.Requirements = CreateRequirements(template);

        int actionCost = 1;
        locationAction.Requirements.Add(new ActionPointRequirement(actionCost));
        return locationAction;
    }

    private LocationSpot FindDefaultSpotForLocation(string locationId)
    {
        Location location = worldState.locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null || location.LocationSpotIds == null || location.LocationSpotIds.Count == 0)
        {
            return null;
        }

        // Try to find a central/main spot first (marketplace, common_room, etc.)
        string[] preferredSpotNames = { "marketplace", "common_room", "center", "main" };

        foreach (string spotId in location.LocationSpotIds)
        {
            LocationSpot spot = worldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
            if (spot != null && preferredSpotNames.Any(name => spot.SpotID.Contains(name)))
            {
                return spot;
            }
        }

        // If no preferred spot found, return the first available spot
        string firstSpotId = location.LocationSpotIds.First();
        return worldState.locationSpots.FirstOrDefault(s => s.SpotID == firstSpotId);
    }

    private List<IRequirement> CreateRequirements(ActionDefinition template)
    {
        List<IRequirement> requirements = new();
        // Time window requirement
        if (template.TimeWindows != null && template.TimeWindows.Count > 0)
        {
            requirements.Add(new TimeWindowRequirement(template.TimeWindows));
        }
        return requirements;
    }

    public LocationAction CreateActionFromOpportunity(OpportunityDefinition opportunity)
    {
        LocationAction locationAction = new LocationAction();

        locationAction.ActionId = opportunity.Id;
        locationAction.Name = opportunity.Name;
        locationAction.ObjectiveDescription = opportunity.Description;
        locationAction.Opportunity = opportunity;

        if (opportunity.Type == OpportunityTypes.Accumulative)
        {
            locationAction.LocationId = opportunity.InitialLocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(opportunity.InitialLocationId);
            locationAction.LocationSpotId = defaultSpot?.SpotID;

            locationAction.Approaches = opportunity.Approaches;
        }
        else if (opportunity.Type == OpportunityTypes.Sequential && opportunity.InitialStep != null)
        {
            locationAction.LocationId = opportunity.InitialStep.LocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(opportunity.InitialStep.LocationId);
            locationAction.LocationSpotId = defaultSpot?.SpotID;

            locationAction.Approaches = opportunity.InitialStep.Approaches;
        }

        locationAction.RequiredCardType = SkillCategories.Physical;

        locationAction.Requirements = new List<IRequirement>
        {
            new ActionPointRequirement(1),
        };

        return locationAction;
    }
}