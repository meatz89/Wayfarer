public class ActionGenerator
{
    private readonly NarrativeService _narrativeService;
    private readonly ActionRepository _repository;

    public ActionGenerator(
        NarrativeService narrativeService,
        ActionRepository repository)
    {
        _narrativeService = narrativeService;
        _repository = repository;
    }

    public async Task<string> CreateEncounterForAction(
        string actionId,
        SpotAction actionTemplate,
        WorldStateInput worldStateInput)
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionTemplate.ActionId,
            Goal = actionTemplate.Goal,
            Complication = actionTemplate.Complication,
            BasicActionType = actionTemplate.BasicActionType.ToString(),
            SpotName = actionTemplate.LocationSpotName,
            LocationName = actionTemplate.LocationName,
        };

        // Get action and encounter details from AI
        string jsonResponse = await _narrativeService.GenerateActionsAsync(context, worldStateInput);

        // Parse the response
        ActionCreationResult result = ActionJsonParser.Parse(jsonResponse);

        // Create and register the encounter template
        EncounterTemplate encounterTemplate = CreateEncounterTemplate(actionId, result.EncounterTemplate);

        _repository.RegisterEncounterTemplate(actionId, encounterTemplate);

        return actionId;
    }

    public async Task<string> GenerateActionAndEncounter(
        WorldStateInput worldStateInput,
        string actionId,
        string actionName,
        string locationSpotName,
        string locationName,
        string goal = "",
        string complication = "",
        string basicActionType = "")
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionId,
            SpotName = locationSpotName,
            LocationName = locationName,
            Goal = goal,
            Complication = complication,
            BasicActionType = basicActionType
        };

        // Get action and encounter details from AI
        string jsonResponse = await _narrativeService.GenerateActionsAsync(context, worldStateInput);

        // Parse the response
        ActionCreationResult result = ActionJsonParser.Parse(jsonResponse);

        // Create and register the encounter template
        EncounterTemplate encounterTemplate = CreateEncounterTemplate(actionId, result.EncounterTemplate);
        _repository.RegisterEncounterTemplate(actionId, encounterTemplate);

        // Create action template linked to the encounter
        actionId = _repository.AddActionTemplate(actionId, result.Action);
        return actionId;
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
            encounterStrategicTags = model.StrategicTags.Select(t =>
                new EnvironmentPropertyTag(
                    t.Name,
                    ParseEnvironmentalProperty(t.EnvironmentalProperty))
            ).ToList(),
        };

        return template;
    }

    private IEnvironmentalProperty ParseEnvironmentalProperty(string property)
    {
        // Create the appropriate environmental property
        if (property.Equals("Bright", StringComparison.OrdinalIgnoreCase))
            return Illumination.Bright;
        if (property.Equals("Shadowy", StringComparison.OrdinalIgnoreCase))
            return Illumination.Shadowy;
        if (property.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            return Illumination.Dark;

        if (property.Equals("Crowded", StringComparison.OrdinalIgnoreCase))
            return Population.Crowded;
        if (property.Equals("Quiet", StringComparison.OrdinalIgnoreCase))
            return Population.Quiet;
        if (property.Equals("Isolated", StringComparison.OrdinalIgnoreCase))
            return Population.Scholarly;

        if (property.Equals("Rough", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Rough;
        if (property.Equals("Formal", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Formal;
        if (property.Equals("Chaotic", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Chaotic;

        if (property.Equals("Confined", StringComparison.OrdinalIgnoreCase))
            return Physical.Confined;
        if (property.Equals("Expansive", StringComparison.OrdinalIgnoreCase))
            return Physical.Expansive;
        if (property.Equals("Hazardous", StringComparison.OrdinalIgnoreCase))
            return Physical.Hazardous;

        // Default fallback
        return Illumination.Bright;
    }

    private EncounterInfo.HostilityLevels ParseHostility(string hostility)
    {
        return hostility.ToLower() switch
        {
            "friendly" => EncounterInfo.HostilityLevels.Friendly,
            "hostile" => EncounterInfo.HostilityLevels.Hostile,
            _ => EncounterInfo.HostilityLevels.Neutral
        };
    }

}