public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceGenerator choiceSetGenerator;

    public ChoiceSystem(
        GameContentProvider contentProvider,
        GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
    }

    public List<Choice> GenerateChoices(
        EncounterState state
        )
    {
        //List<EncounterChoice> choices = new List<EncounterChoice>(); // choiceSetGenerator.CreateEncounterChoices(Encounter, EncounterContext, EncounterStageContext, template);
        //ChoiceSet choiceSet = CreateChoiceSet(currentValues, EncounterContext, choices);

        ChoiceRepository repository = new ChoiceRepository();
        ChoiceGenerator generator = new ChoiceGenerator(repository);

        // Set some initial tag values
        state.ApproachTypesDic[ApproachTypes.Force] = 1;
        state.ApproachTypesDic[ApproachTypes.Charm] = 2;
        state.ApproachTypesDic[ApproachTypes.Wit] = 1;
        state.FocusTypesDic[FocusTypes.Relationship] = 1;
        state.FocusTypesDic[FocusTypes.Information] = 2;

        List<Choice> choices = generator.GenerateChoiceSet(state);

        foreach (Choice choice in choices)
        {
            EncounterState projectedState = state.ApplyChoice(choice);
        }

        return choices;
    }

    public ChoiceSet CreateChoiceSet(EncounterContext context, List<Choice> choices)
    {
        foreach (Choice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context);
        }

        foreach (Choice choice in choices)
        {
            //EncounterStageState projection = calculator.GetProjectedEncounterState(choice, initialValues, choice.CalculationResult.ValueModifications);
            ////choice.CalculationResult.ProjectedEncounterState = projection;
            //if (!choice.IsEncounterFailingChoice && IsEncounterWon(context, projection)) choice.IsEncounterWinningChoice = true;
            //if (!choice.IsEncounterWinningChoice && IsEncounterLost(context, projection)) choice.IsEncounterFailingChoice = true;
        }

        return new ChoiceSet(context.ActionType.ToString(), choices);
    }
}

