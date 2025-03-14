


public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly INarrativeAIService narrativeService;

    public EncounterManager Encounter;
    public EncounterResult encounterResult;

    private bool useAiNarrative = false;

    public EncounterSystem(
        GameState gameState,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        INarrativeAIService narrativeService,
        IConfiguration configuration)
    {
        this.gameState = gameState;
        this.narrativeService = narrativeService;
        useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
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

        // Create encounter manager
        EncounterManager encounter = new EncounterManager(actionImplementation, cardSelector, useAiNarrative);
        this.Encounter = encounter;

        SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(location);
        choiceRepository.AddSpecialChoice(location.Name, negotiatePriceChoice);

        // Start the encounter with narrative
        string incitingAction = "decided to visit the market to purchase supplies";
        NarrativeResult initialResult = await encounter.StartEncounterWithNarrativeAsync(
            location,
            playerState,
            incitingAction,
            narrativeService);

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
}