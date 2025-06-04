/// <summary>
/// Generates actions via AI and registers them safely.
/// </summary>
public class ActionGenerator
{
    private ActionRepository actionRepository;
    private LocationRepository locationRepository;
    private WorldStateInputBuilder _worldStateInputCreator;
    private IConfiguration _configuration;
    private AIGameMaster _aiService;
    private WorldState worldState;

    public ActionGenerator(
        GameWorld gameWorld,
        ActionRepository actionRepository,
        LocationRepository locationRepository,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration,
        AIGameMaster aiService)
    {
        this.actionRepository = actionRepository;
        this.locationRepository = locationRepository;
        _worldStateInputCreator = worldStateInputCreator;
        _configuration = configuration;
        this._aiService = aiService;
        worldState = gameWorld.WorldState;
    }

    public async Task<string> GenerateAction(
        string actionName,
        string locationId,
        string spotId)
    {
        //Location location = locationRepository.GetLocationById(locationId);
        //LocationSpot locationSpot = locationRepository.GetSpot(locationId, spotId);

        //if (location == null || locationSpot == null)
        //{
        //    location = worldState.CurrentLocation;
        //    locationSpot = worldState.CurrentLocationSpot;
        //}

        //ActionDefinition actionDef = GetDefaultActionDefinition(actionName, locationSpot.SpotID);

        //if (_configuration.GetValue<bool>("actionGeneration"))
        //{
        //    ActionGenerationContext context = new ActionGenerationContext
        //    {
        //        ActionId = actionName.Replace(" ", ""),
        //        SpotName = locationSpot.Name,
        //        LocationName = location.Name,
        //    };

        //    WorldStateInput worldStateInput = await _worldStateInputCreator.CreateWorldStateInput(location.Name);
        //    string json = await _aiService.GenerateActions(context, worldStateInput);

        //    actionDef = ActionParser.ParseAction(json);
        //}

        //actionRepository.AddAction(actionDef);
        //return actionDef.Id;

        return GetDefaultActionDefinition(actionName, spotId).Id;
    }

    public async Task<string> GenerateOpportunity(string name, string id1, string id2)
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