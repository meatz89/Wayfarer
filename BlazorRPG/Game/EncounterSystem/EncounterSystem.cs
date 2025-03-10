using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly NarrativeSystem narrativeSystem;

    private Encounter Encounter;
    public EncounterManager EncounterManager;

    public EncounterSystem(
        GameState gameState,
        NarrativeSystem narrativeSystem,
        MessageSystem messageSystem,
        GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.narrativeSystem = narrativeSystem;
    }

    public Encounter GenerateEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        // Initial tags for a social Encounter
        Dictionary<ApproachTypes, int> initialApproachTypess = new Dictionary<ApproachTypes, int>
            {
                { ApproachTypes.Charm, 1 },
                { ApproachTypes.Wit, 1 }
            };

        Dictionary<FocusTypes, int> initialFocusTypess = new Dictionary<FocusTypes, int>
            {
                { FocusTypes.Relationship, 1 },
                { FocusTypes.Resource, 1 }
            };


        Location inn = context.Location;
        Encounter = new Encounter(context, initialApproachTypess, initialFocusTypess);

        // Create a location
        LocationInfo villageMarket = LocationFactory.CreateVillageMarket();

        // Create encounter manager
        EncounterManager = StartEncounterAt(villageMarket);

        // Create first ChoiceSet
        List<IChoice> choices = EncounterManager.GetCurrentChoices();

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        narrativeSystem.NewEncounter(context, actionImplementation);

        return Encounter;
    }

    public EncounterManager StartEncounterAt(LocationInfo villageMarket)
    {
        // Create the core components
        ChoiceRepository choiceRepository = new ChoiceRepository();
        CardSelectionAlgorithm cardSelector = new CardSelectionAlgorithm(choiceRepository);
        NarrativePresenter narrativePresenter = new NarrativePresenter();

        // Create encounter manager
        EncounterManager encounterManager = new EncounterManager(cardSelector, choiceRepository, narrativePresenter);

        // Add special choices for this location
        SpecialChoice negotiatePriceChoice = new SpecialChoice(
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

        choiceRepository.AddSpecialChoice(villageMarket.Name, negotiatePriceChoice);

        // Start an encounter at the village market
        encounterManager.StartEncounter(villageMarket);

        return encounterManager;
    }

    public EncounterResult ExecuteChoice(
        Encounter encounter,
        IChoice choice,
        LocationSpot locationSpot)
    {
        ChoiceProjection choiceProjection = EncounterManager.GetChoiceProjection(choice);
        ChoiceOutcome choiceOutcome = EncounterManager.ApplyChoiceProjection(choiceProjection);

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = choiceOutcome.ToString()
        };
    }


    public Encounter GetActiveEncounter()
    {
        return gameState.Actions.CurrentEncounter;
    }

    public List<IChoice> GetChoices()
    {
        return EncounterManager.GetCurrentChoices();
    }

    public List<UserEncounterChoiceOption> GetCurrentChoices()
    {
        return gameState.Actions.CurrentChoiceOptions;
    }

    internal ChoiceProjection GetChoiceProjection(Encounter encounter, IChoice choice)
    {
        List<ChoiceProjection> projections = EncounterManager.GetChoiceProjections();
        return projections.FirstOrDefault(x => x.Choice == choice);
    }
}
