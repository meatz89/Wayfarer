
public class ActionFactory
{
    private readonly ActionRepository actionRepository;
    private readonly EncounterFactory encounterFactory;
    private readonly PlayerState playerState;

    public ActionFactory(
        ActionRepository actionRepository,
        GameState gameState,
        EncounterFactory encounterFactory)
    {
        this.actionRepository = actionRepository;
        this.encounterFactory = encounterFactory;
        this.playerState = gameState.PlayerState;
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

        actionImplementation.Approaches = template.Approaches;

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
        actionImplementation.EncounterType = EncounterCategories.None;

        // Set action type (assuming all actions with encounter category are encounters)
        actionImplementation.ActionType = actionType;

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

        // Relationship level requirement
        if (template.RelationshipLevel > 0)
        {
            requirements.Add(new RelationshipRequirement(template.SpotId, template.RelationshipLevel));
        }

        // Resource requirements
        if (template.CoinCost > 0)
        {
            requirements.Add(new CoinRequirement(template.CoinCost));
        }

        if (template.FoodCost > 0)
        {
            requirements.Add(new FoodRequirement(template.FoodCost));
        }

        return requirements;
    }

    private List<Outcome> CreateCosts(ActionDefinition template)
    {
        List<Outcome> costs = new();

        if (template.CoinCost > 0)
        {
            costs.Add(new CoinOutcome(-template.CoinCost));
        }

        if (template.FoodCost > 0)
        {
            costs.Add(new FoodOutcome(-template.FoodCost));
        }

        return costs;
    }
}