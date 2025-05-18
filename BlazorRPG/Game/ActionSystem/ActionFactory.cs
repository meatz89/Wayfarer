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
        // Transfer basic properties
        actionImplementation.Id = template.Id;
        actionImplementation.Name = template.Name;
        actionImplementation.Description = template.Description;
        actionImplementation.LocationId = location;
        actionImplementation.LocationSpotId = locationSpot;

        // Handle movement actions
        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            actionImplementation.DestinationLocation = template.MoveToLocation;
        }
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            actionImplementation.DestinationLocationSpot = template.MoveToLocationSpot;
        }
        // Set encounter type
        actionImplementation.ActionType = ActionExecutionTypes.Instant;

        // Create requirements, costs, and yields
        actionImplementation.Requirements = CreateRequirements(template);
        actionImplementation.Costs = CreateCosts(template);
        actionImplementation.Yields = CreateYields(template);
        // Add base AP cost requirement
        int actionCost = 1;
        actionImplementation.Requirements.Add(new ActionPointRequirement(actionCost));
        actionImplementation.Costs.Add(new ActionPointOutcome(-actionCost));
        return actionImplementation;
    }



    public ActionImplementation CreateActionFromCommission(CommissionDefinition commission)
    {
        ActionImplementation actionImplementation = new ActionImplementation();

        // Set basic commission properties
        actionImplementation.Id = commission.Id;
        actionImplementation.Name = commission.Name;
        actionImplementation.Description = commission.Description;
        actionImplementation.Commission = commission;

        // Different location handling based on commission type
        if (commission.Type == CommissionTypes.Accumulative)
        {
            // For accumulative commissions, set location to initial location
            actionImplementation.LocationId = commission.InitialLocationId;

            // Find a suitable spot in that location (e.g., the marketplace for town_square)
            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialLocationId);
            actionImplementation.LocationSpotId = defaultSpot?.Id;

            // Set approaches from commission
            actionImplementation.Approaches = commission.Approaches;
        }
        else if (commission.Type == CommissionTypes.Sequential && commission.InitialStep != null)
        {
            // For sequential commissions, set location to the initial step's location
            actionImplementation.LocationId = commission.InitialStep.LocationId;

            // Find a suitable spot in that location
            LocationSpot defaultSpot = FindDefaultSpotForLocation(commission.InitialStep.LocationId);
            actionImplementation.LocationSpotId = defaultSpot?.Id;

            // Set approaches from initial step
            actionImplementation.Approaches = commission.InitialStep.Approaches;
        }

        // Set action and encounter types
        actionImplementation.ActionType = ActionExecutionTypes.Encounter;

        // Set standard costs and requirements
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

    // Helper method to find a default spot in a location
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
}