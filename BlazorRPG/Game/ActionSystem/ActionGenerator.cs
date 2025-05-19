

/// <summary>
/// Generates actions via AI and registers them safely.
/// </summary>
public class ActionGenerator
{
    private readonly ActionRepository actionRepository;
    private readonly LocationRepository locationRepository;
    private readonly WorldStateInputBuilder _worldStateInputCreator;
    private readonly IConfiguration _configuration;
    private readonly IAIService _aiService;

    public ActionGenerator(
        ActionRepository actionRepository,
        LocationRepository locationRepository,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration,
        IAIService aiService)
    {
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        _worldStateInputCreator = worldStateInputCreator;
        _configuration = configuration;
        this._aiService = aiService;
    }

    public async Task<string> GenerateAction(
        string actionName,
        string locationId,
        string spotId)
    {
        Location location = locationRepository.GetLocationById(locationId);
        LocationSpot locationSpot = locationRepository.GetSpot(locationId, spotId);
        ActionDefinition actionDef = GetDefaultActionDefinition(actionName, locationSpot.Id);

        if (_configuration.GetValue<bool>("actionGeneration"))
        {
            ActionGenerationContext context = new ActionGenerationContext
            {
                ActionId = actionName.Replace(" ", ""),
                SpotName = locationSpot.Name,
                LocationName = location.Name,
            };

            WorldStateInput worldStateInput = await _worldStateInputCreator.CreateWorldStateInput(location.Name);
            string json = await _aiService.GenerateActionsAsync(context, worldStateInput);

            actionDef = ActionParser.ParseAction(json);
        }

        actionRepository.AddAction(actionDef);
        return actionDef.Id;
    }

    internal async Task<string> GenerateCommission(string name, string id1, string id2)
    {
        throw new NotImplementedException();
    }

    private ActionDefinition GetDefaultActionDefinition(
        string actionName,
        string spotId)
    {
        return new ActionDefinition(actionName.Replace(" ", "_").ToLowerInvariant(), actionName, spotId)
        {
            Description = "Description",
        };
    }
}