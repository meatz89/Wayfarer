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
        
        locationAction.RequiredCardType = ActionTypes.Physical;
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

    public LocationAction CreateActionFromCommission(CommissionDefinition commission)
    {
        LocationAction locationAction = new LocationAction();

        locationAction.ActionId = commission.Id;
        locationAction.Name = commission.Name;
        locationAction.ObjectiveDescription = commission.Description;
        locationAction.Commission = commission;

        if (commission.Type == CommissionTypes.Accumulative)
        {
            locationAction.LocationId = commission.InitialLocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialLocationId);
            locationAction.LocationSpotId = defaultSpot?.SpotID;

            locationAction.Approaches = commission.Approaches;
        }
        else if (commission.Type == CommissionTypes.Sequential && commission.InitialStep != null)
        {
            locationAction.LocationId = commission.InitialStep.LocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialStep.LocationId);
            locationAction.LocationSpotId = defaultSpot?.SpotID;

            locationAction.Approaches = commission.InitialStep.Approaches;
        }

        locationAction.RequiredCardType = ActionExecutionTypes.Encounter;

        locationAction.Requirements = new List<IRequirement>
        {
            new ActionPointRequirement(1),
        };

        return locationAction;
    }
}