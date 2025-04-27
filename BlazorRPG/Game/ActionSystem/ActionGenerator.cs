/// <summary>
/// Generates actions via AI and registers them safely.
/// </summary>
public class ActionGenerator
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository _actionRepo;
    private readonly WorldStateInputBuilder _worldStateInputCreator;
    private readonly IConfiguration _configuration;

    public ActionGenerator(
        NarrativeService narrativeService,
        ActionRepository actionRepository,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration)
    {
        _narrativeService = narrativeService;
        _actionRepo = actionRepository;
        _worldStateInputCreator = worldStateInputCreator;
        _configuration = configuration;
    }

    public async Task<string> GenerateActionAndEncounter(
        string actionName,
        string locationSpotName,
        string locationName,
        string goal = "",
        string complication = "",
        string basicActionType = "")
    {
        ActionDefinition actionDef = GetDefaultActionDefinition(actionName, locationSpotName, locationName);

        if (_configuration.GetValue<bool>("actionGeneration"))
        {
            ActionGenerationContext context = new ActionGenerationContext
            {
                ActionId = actionName.Replace(" ", ""),
                SpotName = locationSpotName,
                LocationName = locationName,
                Goal = goal,
                Complication = complication,
                BasicActionType = basicActionType
            };

            WorldStateInput worldStateInput = await _worldStateInputCreator.CreateWorldStateInput(locationName);
            string json = await _narrativeService.GenerateActionsAsync(context, worldStateInput);

            actionDef = ActionParser.ParseAction(json);
        }

        // Register AI or default action via repository
        _actionRepo.RegisterAction(actionDef);
        return actionDef.Name;
    }

    private ActionDefinition GetDefaultActionDefinition(
        string actionName,
        string locationSpotName,
        string locationName)
    {
        return new ActionDefinition(actionName, actionName)
        {
            Goal = "Goal",
            Complication = "Complication",
            Description = "Description",
        };
    }

    public EncounterTemplate CreateEncounterTemplate(string id, EncounterTemplateModel model)
    {
        return new EncounterTemplate
        {
            ActionId = id,
            Name = model.Name,
            Duration = model.Duration,
            MaxPressure = model.MaxPressure,
            PartialThreshold = model.PartialThreshold,
            StandardThreshold = model.StandardThreshold,
            ExceptionalThreshold = model.ExceptionalThreshold,
            Hostility = ParseHostility(model.Hostility),
            EncounterStrategicTags = new()
        };
    }

    private Encounter.HostilityLevels ParseHostility(string hostility)
    {
        return hostility.ToLower() switch
        {
            "friendly" => Encounter.HostilityLevels.Friendly,
            "hostile" => Encounter.HostilityLevels.Hostile,
            _ => Encounter.HostilityLevels.Neutral
        };
    }
}