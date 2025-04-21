public class ActionGenerator
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository _repository;
    private readonly WorldStateInputCreator worldStateInputCreator;
    private readonly IConfiguration configuration;
    public ActionGenerator(
        NarrativeService narrativeService,
        ActionRepository repository,
        WorldStateInputCreator worldStateInputCreator,
        IConfiguration configuration)
    {
        _narrativeService = narrativeService;
        _repository = repository;
        this.worldStateInputCreator = worldStateInputCreator;
        this.configuration = configuration;
    }

    public async Task<string> GenerateActionAndEncounter(
        string actionName,
        string locationSpotName,
        string locationName,
        string goal = "",
        string complication = "",
        string basicActionType = "")
    {
        ActionDefinition actionDefinition = GetDefaultActionDefinition(actionName, locationSpotName, locationName);

        if (configuration.GetValue<bool>("actionGeneration"))
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

            // Get action and encounter details from AI
            WorldStateInput worldStateInput = await worldStateInputCreator.CreateWorldStateInput(locationName);

            string jsonResponse = await _narrativeService.GenerateActionsAsync(context, worldStateInput);

            // Parse the response
            ActionCreationResult result = ActionJsonParser.Parse(jsonResponse);
            actionDefinition = result.Action;
        }

        string actionId = _repository.AddActionTemplate(actionDefinition);
        return actionId;
    }

    private ActionDefinition GetDefaultActionDefinition(
        string actionName,
        string locationSpotName,
        string locationName)
    {
        ActionDefinition actionDefinition = new()
        {
            Id = actionName,
            Category = ActionCategories.Exploration,
            Difficulty = 1,
            Goal = "Goal",
            Complication = "Complication",
            Description = "Description",
            EncounterChance = 50,
            IsRepeatable = true,
            EnergyCost = 1,
            TimeCost = 1,
            EncounterType = EncounterTypes.Intellectual,
            LocationName = locationName,
            LocationSpotName = locationSpotName
        };
        return actionDefinition;
    }

    public EncounterTemplate CreateEncounterTemplate(string id, EncounterTemplateModel model)
    {
        EncounterTemplate template = new EncounterTemplate
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

        return template;
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
