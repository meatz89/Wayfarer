public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly NarrativeSystem narrativeSystem;
    private readonly ChoiceExecutor choiceExecutor;

    private Encounter Encounter;
    private EncounterProcessor encounterProcessor;

    public EncounterSystem(
        GameState gameState,
        NarrativeSystem narrativeSystem,
        MessageSystem messageSystem,
        GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.narrativeSystem = narrativeSystem;
        this.choiceExecutor = new ChoiceExecutor(gameState);
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
        inn.Name = "Harbor Tavern";
        inn.Duration = 5;
        inn.FailThreshold = 7;
        inn.PartialSuccessThreshold = 8;
        inn.StandardSuccessThreshold = 10;
        inn.ExceptionalSuccessThreshold = 12;

        Encounter = new Encounter(context, initialApproachTypess, initialFocusTypess);

        EncounterFactory _factory = new EncounterFactory();
        encounterProcessor = _factory.CreateBanditCampEncounter();
        EncounterState state = encounterProcessor.GetState();

        // Create first ChoiceSet
        encounterProcessor.GenerateChoices();

        // Create Encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        gameState.Actions.SetActiveEncounter(Encounter);
        narrativeSystem.NewEncounter(context, actionImplementation);

        return Encounter;
    }

    public ChoiceProjection GetChoiceProjection(
        Encounter encounter,
        Choice choice)
    {
        EncounterState oldState = encounterProcessor.GetState();
        ChoiceProjection projection = encounterProcessor.ProjectChoice(choice);
        return projection;
    }

    public EncounterResult ExecuteChoice(
        Encounter encounter,
        Choice choice,
        LocationSpot locationSpot)
    {
        // Execute the choice with the actual modified values from the result
        //choiceExecutor.ExecuteChoice(Encounter, choice, choice.CalculationResult);

        // Update last choice type
        Encounter.History.LastChoice = choice;
        Encounter.History.LastChoiceEffectType = choice.EffectType;
        Encounter.History.LastChoiceApproach = choice.ApproachType;
        Encounter.History.LastChoiceFocusType = choice.FocusType;

        EncounterState oldState = encounterProcessor.GetState();
        encounterProcessor.ProcessChoice(choice);

        // Check for encounter success/failure conditions
        EncounterStatus encounterStatus = encounterProcessor.CheckEncounterConditions();

        // Generate new choices for next turn if encounter is still in progress
        if (encounterStatus == EncounterStatus.InProgress)
        {
            encounterProcessor.GenerateChoices();
        }

        StrategicSignature signature = encounterProcessor.GetSignature();
        List<EncounterTag> activeTags = encounterProcessor.GetActiveTags();

        EncounterState newState = encounterProcessor.GetState();

        narrativeSystem.MakeChoicePrompt(choice);

        return new EncounterResult()
        {
            Encounter = encounter,
            EncounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = "Ongoing"
        };
    }

    public EncounterProcessor GetActiveEncounterProcessor()
    {
        return encounterProcessor;
    }

    public Encounter GetActiveEncounter()
    {
        return gameState.Actions.CurrentEncounter;
    }

    public List<UserEncounterChoiceOption> GetCurrentChoices()
    {
        return gameState.Actions.CurrentChoiceOptions;
    }
}
