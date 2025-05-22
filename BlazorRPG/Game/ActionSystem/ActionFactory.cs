public class ActionFactory
{
    private readonly ActionRepository actionRepository;
    private readonly EncounterFactory encounterFactory;
    private readonly PlayerState playerState;
    private readonly WorldState worldState;

    public ActionFactory(
        ActionRepository actionRepository,
        GameState gameState,
        EncounterFactory encounterFactory)
    {
        this.actionRepository = actionRepository;
        this.encounterFactory = encounterFactory;
        this.playerState = gameState.PlayerState;
        this.worldState = gameState.WorldState;
    }

    public ActionImplementation CreateActionFromTemplate(ActionDefinition template, string location, string locationSpot, ActionExecutionTypes actionType)
    {
        ActionImplementation actionImplementation = new ActionImplementation();
        actionImplementation.Id = template.Id;
        actionImplementation.Name = template.Name;
        actionImplementation.Description = template.Description;
        actionImplementation.LocationId = location;
        actionImplementation.LocationSpotId = locationSpot;

        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            actionImplementation.DestinationLocation = template.MoveToLocation;
        }
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            actionImplementation.DestinationLocationSpot = template.MoveToLocationSpot;
        }
        actionImplementation.ActionType = actionType;

        actionImplementation.Requirements = CreateRequirements(template);
        actionImplementation.Costs = CreateCosts(template);
        actionImplementation.Yields = CreateYields(template);

        int actionCost = 1;
        actionImplementation.Requirements.Add(new ActionPointRequirement(actionCost));
        actionImplementation.Costs.Add(new ActionPointOutcome(-actionCost));
        return actionImplementation;
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
            LocationSpot spot = worldState.locationSpots.FirstOrDefault(s => s.Id == spotId);
            if (spot != null && preferredSpotNames.Any(name => spot.Id.Contains(name)))
            {
                return spot;
            }
        }

        // If no preferred spot found, return the first available spot
        string firstSpotId = location.LocationSpotIds.First();
        return worldState.locationSpots.FirstOrDefault(s => s.Id == firstSpotId);
    }

    private List<Outcome> CreateYields(ActionDefinition template)
    {
        return new List<Outcome>();
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

    private List<Outcome> CreateCosts(ActionDefinition template)
    {
        List<Outcome> costs = new();
        return costs;
    }

    public ActionImplementation CreateActionFromCommission(CommissionDefinition commission)
    {
        ActionImplementation actionImplementation = new ActionImplementation();

        actionImplementation.Id = commission.Id;
        actionImplementation.Name = commission.Name;
        actionImplementation.Description = commission.Description;
        actionImplementation.Commission = commission;

        if (commission.Type == CommissionTypes.Accumulative)
        {
            actionImplementation.LocationId = commission.InitialLocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialLocationId);
            actionImplementation.LocationSpotId = defaultSpot?.Id;

            actionImplementation.Approaches = commission.Approaches;
        }
        else if (commission.Type == CommissionTypes.Sequential && commission.InitialStep != null)
        {
            actionImplementation.LocationId = commission.InitialStep.LocationId;

            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialStep.LocationId);
            actionImplementation.LocationSpotId = defaultSpot?.Id;

            actionImplementation.Approaches = commission.InitialStep.Approaches;
        }

        actionImplementation.ActionType = ActionExecutionTypes.Encounter;

        actionImplementation.Requirements = new List<IRequirement>
        {
            new ActionPointRequirement(1),
        };

        actionImplementation.Costs = new List<Outcome>
        {
            new ActionPointOutcome(-1)
        };

        actionImplementation.Yields = new List<Outcome>();

        return actionImplementation;
    }
}