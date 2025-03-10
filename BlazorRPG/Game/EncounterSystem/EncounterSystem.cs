using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly NarrativeSystem narrativeSystem;

    public Encounter EncounterManager;

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
        Location inn = context.Location;

        // Create a location
        LocationInfo villageMarket = LocationFactory.CreateVillageMarket();

        // Create encounter manager
        EncounterManager = StartEncounterAt(villageMarket);

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(EncounterManager);
        narrativeSystem.NewEncounter(context, actionImplementation);

        return EncounterManager;
    }

    public Encounter StartEncounterAt(LocationInfo villageMarket)
    {
        // Create the core components
        ChoiceRepository choiceRepository = new ChoiceRepository();
        CardSelectionAlgorithm cardSelector = new CardSelectionAlgorithm(choiceRepository);
        NarrativePresenter narrativePresenter = new NarrativePresenter();

        // Create encounter manager
        Encounter encounterManager = new Encounter(cardSelector, choiceRepository, narrativePresenter);

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
        IChoice choice)
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

    public ChoiceProjection GetChoiceProjection(Encounter encounter, IChoice choice)
    {
        return EncounterManager.GetChoiceProjection(choice);
    }
}
