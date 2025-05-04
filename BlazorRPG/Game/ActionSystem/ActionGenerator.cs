/// <summary>
/// Generates actions via AI and registers them safely.
/// </summary>
public class ActionGenerator
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository actionRepository;
    private readonly WorldStateInputBuilder _worldStateInputCreator;
    private readonly IConfiguration _configuration;

    public ActionGenerator(
        NarrativeService narrativeService,
        ActionRepository actionRepository,
        WorldStateInputBuilder worldStateInputCreator,
        IConfiguration configuration)
    {
        _narrativeService = narrativeService;
        this.actionRepository = actionRepository;
        _worldStateInputCreator = worldStateInputCreator;
        _configuration = configuration;
    }

    public async Task<string> GenerateAction(
        string actionName,
        string locationSpotName,
        string locationName)
    {
        ActionDefinition actionDef = GetDefaultActionDefinition(actionName, locationSpotName, locationName);

        if (_configuration.GetValue<bool>("actionGeneration"))
        {
            ActionGenerationContext context = new ActionGenerationContext
            {
                ActionId = actionName.Replace(" ", ""),
                SpotName = locationSpotName,
                LocationName = locationName,
            };

            WorldStateInput worldStateInput = await _worldStateInputCreator.CreateWorldStateInput(locationName);
            string json = await _narrativeService.GenerateActionsAsync(context, worldStateInput);

            actionDef = ActionParser.ParseAction(json);
        }

        actionRepository.AddAction(actionDef);
        return actionDef.Id;
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