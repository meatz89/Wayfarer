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
            ActionName = actionTemplate.Name,
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

        string encounterName = $"{result.Action.IsEncounterAction}";
        _repository.RegisterEncounterTemplate(actionId, encounterTemplate);

        return encounterName;
    }

    public async Task<string> GenerateActionAndEncounter(
        WorldStateInput worldStateInput,
        string actionId,
        string name,
        string locationSpotName,
        string locationName,
        string goal = "",
        string complication = "",
        string basicActionType = "")
    {
        // Create context for generation
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionId,
            ActionName = name,
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
        string actionTemplate = _repository.CreateActionTemplate(
            actionId,
            result.Action.Name,
            result.Action.Goal,
            result.Action.Complication,
            result.Action.BasicActionType,
            result.Action.ActionType,
            result.Action.CoinCost);

        return actionTemplate;
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
            PressureReducingFocuses = model.PressureReducingFocuses.Select(ParseFocusTag).ToList(),
            MomentumReducingFocuses = model.MomentumReducingFocuses.Select(ParseFocusTag).ToList(),
            encounterStrategicTags = model.StrategicTags.Select(t =>
                new StrategicTag(
                    t.Name,
                    ParseEnvironmentalProperty(t.EnvironmentalProperty))
            ).ToList(),
            EncounterNarrativeTags = model.NarrativeTags.Select(GetNarrativeTag).ToList()
        };

        return template;
    }

    private ApproachTags ParseApproachTag(string approach)
    {
        return approach switch
        {
            "Dominance" => ApproachTags.Dominance,
            "Rapport" => ApproachTags.Rapport,
            "Analysis" => ApproachTags.Analysis,
            "Precision" => ApproachTags.Precision,
            "Evasion" => ApproachTags.Evasion,
            _ => ApproachTags.Analysis // Default
        };
    }

    private FocusTags ParseFocusTag(string focus)
    {
        return focus switch
        {
            "Relationship" => FocusTags.Relationship,
            "Information" => FocusTags.Information,
            "Physical" => FocusTags.Physical,
            "Environment" => FocusTags.Environment,
            "Resource" => FocusTags.Resource,
            _ => FocusTags.Information // Default
        };
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
            return Population.Isolated;

        if (property.Equals("Tense", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Tense;
        if (property.Equals("Formal", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Formal;
        if (property.Equals("Chaotic", StringComparison.OrdinalIgnoreCase))
            return Atmosphere.Chaotic;

        if (property.Equals("Wealthy", StringComparison.OrdinalIgnoreCase))
            return Economic.Wealthy;
        if (property.Equals("Commercial", StringComparison.OrdinalIgnoreCase))
            return Economic.Commercial;
        if (property.Equals("Humble", StringComparison.OrdinalIgnoreCase))
            return Economic.Humble;

        if (property.Equals("Confined", StringComparison.OrdinalIgnoreCase))
            return Physical.Confined;
        if (property.Equals("Expansive", StringComparison.OrdinalIgnoreCase))
            return Physical.Expansive;
        if (property.Equals("Hazardous", StringComparison.OrdinalIgnoreCase))
            return Physical.Hazardous;

        // Default fallback
        return Illumination.Bright;
    }

    private NarrativeTag GetNarrativeTag(string tagName)
    {
        return tagName switch
        {
            "IntimidatingPresence" => NarrativeTagRepository.IntimidatingPresence,
            "BattleRage" => NarrativeTagRepository.BattleRage,
            "BruteForceFixation" => NarrativeTagRepository.BruteForceFixation,
            "TunnelVision" => NarrativeTagRepository.TunnelVision,
            "DestructiveImpulse" => NarrativeTagRepository.DestructiveImpulse,
            "SuperficialCharm" => NarrativeTagRepository.SuperficialCharm,
            "SocialAwkwardness" => NarrativeTagRepository.SocialAwkwardness,
            "HesitantPoliteness" => NarrativeTagRepository.HesitantPoliteness,
            "PublicAwareness" => NarrativeTagRepository.PublicAwareness,
            "GenerousSpirit" => NarrativeTagRepository.GenerousSpirit,
            "ColdCalculation" => NarrativeTagRepository.ColdCalculation,
            "AnalysisParalysis" => NarrativeTagRepository.AnalysisParalysis,
            "Overthinking" => NarrativeTagRepository.Overthinking,
            "DetailFixation" => NarrativeTagRepository.DetailFixation,
            "TheoreticalMindset" => NarrativeTagRepository.TheoreticalMindset,
            "MechanicalInteraction" => NarrativeTagRepository.MechanicalInteraction,
            "NarrowFocus" => NarrativeTagRepository.NarrowFocus,
            "PerfectionistParalysis" => NarrativeTagRepository.PerfectionistParalysis,
            "DetailObsession" => NarrativeTagRepository.DetailObsession,
            "InefficientPerfectionism" => NarrativeTagRepository.InefficientPerfectionism,
            "ShadowVeil" => NarrativeTagRepository.ShadowVeil,
            "ParanoidMindset" => NarrativeTagRepository.ParanoidMindset,
            "CautiousRestraint" => NarrativeTagRepository.CautiousRestraint,
            "HidingPlaceFixation" => NarrativeTagRepository.HidingPlaceFixation,
            "HoardingInstinct" => NarrativeTagRepository.HoardingInstinct,
            _ => NarrativeTagRepository.DetailFixation // Default
        };
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