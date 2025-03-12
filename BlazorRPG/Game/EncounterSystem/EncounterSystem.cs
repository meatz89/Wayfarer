using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly NarrativeSystem narrativeSystem;
    private readonly INarrativeAIService narrativeService;

    public EncounterManager Encounter;
    public EncounterResult encounterResult;

    public EncounterSystem(
        GameState gameState,
        NarrativeSystem narrativeSystem,
        MessageSystem messageSystem,
        GameContentProvider contentProvider,
        INarrativeAIService narrativeService)
    {
        this.gameState = gameState;
        this.narrativeSystem = narrativeSystem;
        this.narrativeService = narrativeService;
    }

    public async Task<EncounterResult> GenerateEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        Location inn = context.Location;

        // Create a location
        LocationInfo location = LocationFactory.CreateBanditAmbush();

        // Create encounter manager
        encounterResult = await StartEncounterAt(location);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        narrativeSystem.NewEncounter(context, actionImplementation);

        return encounterResult;
    }

    public async Task<EncounterResult> StartEncounterAt(LocationInfo location)
    {
        // Create the core components
        ChoiceRepository choiceRepository = new ChoiceRepository();
        CardSelectionAlgorithm cardSelector = new CardSelectionAlgorithm(choiceRepository);
        NarrativePresenter narrativePresenter = new NarrativePresenter();

        // Create encounter manager
        EncounterManager encounter = new EncounterManager(cardSelector, choiceRepository, narrativePresenter);
        this.Encounter = encounter;

        SpecialChoice negotiatePriceChoice = GetSpecialChoiceFor(location);
        choiceRepository.AddSpecialChoice(location.Name, negotiatePriceChoice);

        // Start the encounter with narrative
        string incitingAction = "decided to visit the market to purchase supplies";
        NarrativeResult initialResult = await encounter.StartEncounterWithNarrativeAsync(
            location,
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
        ChoiceNarrative selectedDescription = currentResult.ChoiceDescriptions[choice];

        if (!currentResult.IsEncounterOver)
        {
            currentResult = await Encounter.ApplyChoiceWithNarrativeAsync(
                choice,
                selectedDescription);

            if (currentResult.IsEncounterOver)
            {
                return new EncounterResult()
                {
                    Encounter = encounter,
                    EncounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = "=== Encounter Over: {currentResult.Outcome} ===",
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
            ApproachTypes.Charm,
            FocusTags.Resource,
            new List<TagModification>
            {
                    TagModification.ForApproach(ApproachTags.Rapport, 1),
                    TagModification.ForFocus(FocusTags.Resource, 2)
            },
            new List<Func<BaseTagSystem, bool>>
            {
                    ChoiceFactory.ApproachTagRequirement(ApproachTags.Rapport, 2),
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