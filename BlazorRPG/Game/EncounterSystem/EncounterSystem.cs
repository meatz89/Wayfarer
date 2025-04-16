public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly ILogger<EncounterSystem> logger;
    private AIProviderType currentAIProvider;

    public EncounterResult failureResult;

    private ResourceManager resourceManager;
    private readonly NarrativeService narrativeService;
    private CardSelectionAlgorithm cardSelector;

    public WorldState worldState;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        ResourceManager resourceManager,
        NarrativeContextManager narrativeContextManager,
        NarrativeService narrativeService,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        this.gameState = gameState;
        this.configuration = configuration;
        this.logger = logger;

        // Create the switchable narrative service
        this.resourceManager = resourceManager;
        this.narrativeService = narrativeService;

        // Initialize with the default provider from config
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";

        // Set current provider based on configuration value
        switch (defaultProvider.ToLower())
        {
            case "claude":
                currentAIProvider = AIProviderType.Claude;
                break;
            case "gemma":
                currentAIProvider = AIProviderType.Gemma3;
                break;
            default:
                currentAIProvider = AIProviderType.OpenAI;
                break;
        }
    }

    public async Task<EncounterResult> GenerateEncounter(
        Location location,
        string locationSpot,
        EncounterContext context,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        this.worldState = worldState;

        Location loc = context.Location;
        EncounterTypes encounterType = GetPresentationStyleFromBaseAction(actionImplementation);

        EncounterTemplate template = actionImplementation.EncounterTemplate;

        EncounterInfo encounter = EncounterFactory.CreateEncounterFromTemplate(template,
            location, locationSpot, encounterType);

        // Create encounter manager
        failureResult = await StartEncounterAt(location, encounter, this.worldState, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.ActionStateTracker.SetActiveEncounter(GetCurrentEncounter());
        return failureResult;
    }


    public List<EnvironmentPropertyTag> GetActiveStrategicTags(string locationId, EncounterContext encounterContext)
    {
        Location location = worldState.GetLocation(locationId);
        List<IEnvironmentalProperty> properties = GetCurrentEnvironmentalProperties(locationId, encounterContext.TimeOfDay);

        // Base tags from location
        List<EnvironmentPropertyTag> activeTags = new List<EnvironmentPropertyTag>(location.StrategicTags);

        return activeTags;
    }

    public List<IEnvironmentalProperty> GetCurrentEnvironmentalProperties(string locationId, string timeOfDay)
    {
        Location location = worldState.GetLocation(locationId);

        // Fall back to general properties
        return location.EnvironmentalProperties;
    }

    public async Task<EncounterResult> StartEncounterAt(
        Location location,
        EncounterInfo encounterInfo,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        // Create the core components
        CardRepository choiceRepository = new CardRepository();
        cardSelector = new CardSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the switchable service
        EncounterManager encounterManager = new EncounterManager(
            actionImplementation,
            cardSelector,
            narrativeService,
            resourceManager,
            configuration,
            logger);

        // Set the current AI provider
        encounterManager.SwitchAIProvider(currentAIProvider);
        SetCurrentEncounter(encounterManager);

        //SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(encounter);
        //choiceRepository.AddSpecialChoice(encounter.Name, negotiatePriceChoice);

        WorldStateInput worldStateInput = new WorldStateInput();

        // Start the encounter with narrative
        NarrativeResult initialResult = await GetCurrentEncounter().StartEncounterWithNarrativeAsync(
            location,
            encounterInfo,
            worldState,
            playerState,
            actionImplementation,
            currentAIProvider,
            worldStateInput);

        failureResult = new EncounterResult()
        {
            Encounter = GetCurrentEncounter(),
            EncounterResults = EncounterResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult,
            NarrativeContext = GetCurrentEncounter().GetNarrativeContext()
        };
        return failureResult;
    }

    public async Task<EncounterResult> ExecuteChoice(
        EncounterManager encounter,
        NarrativeResult narrativeResult,
        ChoiceCard choice,
        WorldStateInput worldStateInput)
    {
        NarrativeResult currentNarrative = narrativeResult;

        Dictionary<ChoiceCard, ChoiceNarrative> choiceDescriptions = currentNarrative.ChoiceDescriptions;

        ChoiceNarrative? selectedDescription = null;
        if (currentNarrative.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
        {
            selectedDescription = currentNarrative.ChoiceDescriptions[choice];
        }

        currentNarrative = await GetCurrentEncounter().ApplyChoiceWithNarrativeAsync(
            choice,
            encounter.playerState,
            encounter.worldState,
            selectedDescription,
            worldStateInput);

        EncounterResult encounterResult = CreateEncounterResult(encounter, currentNarrative);
        return encounterResult;

    }

    private EncounterResult CreateEncounterResult(EncounterManager encounter, NarrativeResult currentNarrative)
    {
        if (currentNarrative.IsEncounterOver)
        {
            if (currentNarrative.Outcome == EncounterOutcomes.Failure)
            {
                var failureResult = new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterFailure,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    NarrativeResult = currentNarrative,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return failureResult;
            }
            else
            {
                var successResult = new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentNarrative.Outcome} ===",
                    NarrativeResult = currentNarrative,
                    NarrativeContext = encounter.GetNarrativeContext()
                };
                return successResult;
            }
        }

        var ongoingResult = new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentNarrative,
            NarrativeContext = encounter.GetNarrativeContext()
        };
        return ongoingResult;
    }

    public List<ChoiceCard> GetChoices()
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        List<ChoiceCard> choices = encounterManager.GetCurrentChoices();
        return choices;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, ChoiceCard choice)
    {
        EncounterManager encounterManager = GetCurrentEncounter();
        ChoiceProjection choiceProjection = encounterManager.ProjectChoice(choice);
        return choiceProjection;
    }

    public void SetCurrentEncounter(EncounterManager encounterManager)
    {
        gameState.ActionStateTracker.SetActiveEncounter(encounterManager);
    }

    public EncounterManager GetCurrentEncounter()
    {
        return gameState.ActionStateTracker.CurrentEncounter;
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions()
    {
        return gameState.ActionStateTracker.UserEncounterChoiceOptions;
    }

    // New method to switch AI providers
    public void SwitchAIProvider(AIProviderType providerType)
    {
        currentAIProvider = providerType;
        narrativeService.SwitchProvider(providerType);

        // If we have an active encounter, update its provider too
        if (GetCurrentEncounter() != null)
        {
            GetCurrentEncounter().SwitchAIProvider(providerType);
        }
    }

    private static EncounterTypes GetPresentationStyleFromBaseAction(ActionImplementation actionImplementation)
    {
        EncounterTypes encounterTypes = actionImplementation.BasicActionType switch
        {
            BasicActionTypes.Rest => EncounterTypes.Social,
            BasicActionTypes.Travel => EncounterTypes.Physical,

            BasicActionTypes.Labor => EncounterTypes.Physical,
            BasicActionTypes.Gather => EncounterTypes.Physical,
            BasicActionTypes.Fight => EncounterTypes.Physical,

            BasicActionTypes.Study => EncounterTypes.Intellectual,
            BasicActionTypes.Investigate => EncounterTypes.Intellectual,
            BasicActionTypes.Analyze => EncounterTypes.Intellectual,

            BasicActionTypes.Discuss => EncounterTypes.Social,
            BasicActionTypes.Persuade => EncounterTypes.Social,
            BasicActionTypes.Perform => EncounterTypes.Social,

            BasicActionTypes.Consume => EncounterTypes.Physical,
            BasicActionTypes.Observe => EncounterTypes.Intellectual,
            BasicActionTypes.Forage => EncounterTypes.Intellectual,
            BasicActionTypes.Explore => EncounterTypes.Intellectual,
            BasicActionTypes.Climb => EncounterTypes.Physical,

            _ => EncounterTypes.Physical,
        };
        return encounterTypes;
    }
}