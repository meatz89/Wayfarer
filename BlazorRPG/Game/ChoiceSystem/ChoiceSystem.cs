public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly NarrativeChoiceGenerator choiceSetGenerator;

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
        ChoiceRepository repository = new ChoiceRepository();
        NarrativeChoiceGenerator generator = new NarrativeChoiceGenerator(repository);
        List<Choice> choices = generator.GenerateChoiceSet(state);
        return choices;
    }

    public ChoiceSet CreateChoiceSet(EncounterContext context, List<Choice> choices)
    {
        foreach (Choice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context);
        }

        return new ChoiceSet(context.ActionType.ToString(), choices);
    }
}

