public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly IConfiguration configuration;
    private readonly SwitchableNarrativeService narrativeService;
    private AIProviderType currentAIProvider;

    public EncounterManager Encounter;
    public EncounterResult encounterResult;

    private bool useAiNarrative = false;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        IConfiguration configuration)
    {
        this.gameState = gameState;
        this.configuration = configuration;

        // Create the switchable narrative service
        this.narrativeService = new SwitchableNarrativeService(configuration);

        // Initialize with the default provider from config
        string defaultProvider = configuration.GetValue<string>("DefaultAIProvider") ?? "OpenAI";
        currentAIProvider = defaultProvider == "Gemma" ? AIProviderType.Gemma3 : AIProviderType.OpenAI;

        useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
    }

    // New method to switch AI providers
    public void SwitchAIProvider(AIProviderType providerType)
    {
        currentAIProvider = providerType;
        narrativeService.SwitchProvider(providerType);

        // If we have an active encounter, update its provider too
        if (Encounter != null)
        {
            Encounter.SwitchAIProvider(providerType);
        }
    }

    // New method to get current AI provider name for UI
    public string GetCurrentAIProviderName()
    {
        return narrativeService.GetCurrentProviderName();
    }

    public async Task<EncounterResult> GenerateEncounter(
        EncounterContext context,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        Location inn = context.Location;

        // Create a location
        LocationInfo location = LocationFactory.CreateBanditAmbush();

        // Create encounter manager
        encounterResult = await StartEncounterAt(location, playerState, actionImplementation);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);

        return encounterResult;
    }

    public async Task<EncounterResult> StartEncounterAt(
        LocationInfo location,
        PlayerState playerState,
        ActionImplementation actionImplementation)
    {
        // Create the core components
        ChoiceRepository choiceRepository = new ChoiceRepository();
        CardSelectionAlgorithm cardSelector = new CardSelectionAlgorithm(choiceRepository);

        // Create encounter manager with the switchable service
        EncounterManager encounter = new EncounterManager(
            actionImplementation,
            cardSelector,
            useAiNarrative,
            configuration);

        // Set the current AI provider
        encounter.SwitchAIProvider(currentAIProvider);

        this.Encounter = encounter;

        SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(location);
        choiceRepository.AddSpecialChoice(location.Name, negotiatePriceChoice);

        // Start the encounter with narrative
        string incitingAction = "decided to visit the market to purchase supplies";
        NarrativeResult initialResult = await encounter.StartEncounterWithNarrativeAsync(
            location,
            playerState,
            incitingAction,
            currentAIProvider);  // Pass the current provider type

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Started,
            EncounterEndMessage = "",
            NarrativeResult = initialResult
        };
    }

    public async Task<EncounterResult> ExecuteChoice(
        EncounterManager encounter,
        NarrativeResult narrativeResult,
        IChoice choice)
    {
        NarrativeResult currentResult = narrativeResult;
        ChoiceNarrative selectedDescription = null;

        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = currentResult.ChoiceDescriptions;

        if (currentResult.ChoiceDescriptions != null && choiceDescriptions.ContainsKey(choice))
        {
            selectedDescription = currentResult.ChoiceDescriptions[choice];
        }

        if (!currentResult.IsEncounterOver)
        {
            currentResult = await Encounter.ApplyChoiceWithNarrativeAsync(
                choice,
                selectedDescription);

            if (currentResult.IsEncounterOver)
            {
                if (currentResult.Outcome == EncounterOutcomes.Failure)
                {
                    return new EncounterResult()
                    {
                        Encounter = encounter,
                        EncounterResults = EncounterResults.EncounterFailure,
                        EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                        NarrativeResult = currentResult
                    };
                }

                return new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = $"=== Encounter Over: {currentResult.Outcome} ===",
                    NarrativeResult = currentResult
                };
            }
        }

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "",
            NarrativeResult = currentResult
        };
    }


    private static SpecialChoice GetSpecialChoiceFor(LocationInfo location)
    {
        // Add special choices for this location
        return new SpecialChoice(
            "Negotiate Better Price",
            "Use your market knowledge and rapport to secure a favorable deal",
            ApproachTags.Charm,
            FocusTags.Resource,
            new List<TagModification>
            {
                TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Force, 2),
                TagModification.ForFocus(FocusTags.Resource, 2)
            },
            new List<Func<BaseTagSystem, bool>>
            {
                ChoiceFactory.EncounterStateTagRequirement(EncounterStateTags.Rapport, 2),
                ChoiceFactory.ApproachTagRequirement(ApproachTags.Force, 2),
                ChoiceFactory.FocusTagRequirement(FocusTags.Resource, 2)
            }
        );
    }

    public EncounterManager GetActiveEncounter()
    {
        return gameState.Actions.CurrentEncounter;
    }

    public List<IChoice> GetChoices()
    {
        return Encounter.GetCurrentChoices();
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions()
    {
        return gameState.Actions.UserEncounterChoiceOptions;
    }

    public ChoiceProjection GetChoiceProjection(EncounterManager encounter, IChoice choice)
    {
        return Encounter.ProjectChoice(choice);
    }

    // New method to toggle AI providers from the UI or elsewhere
    public void ToggleAIProvider()
    {
        // Switch between OpenAI and Gemma
        AIProviderType newProvider = currentAIProvider == AIProviderType.OpenAI
            ? AIProviderType.Gemma3
            : AIProviderType.OpenAI;

        SwitchAIProvider(newProvider);
    }
}