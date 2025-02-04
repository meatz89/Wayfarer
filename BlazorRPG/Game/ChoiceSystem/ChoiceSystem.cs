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
        this.choiceSetGenerator = new ChoiceGenerator(gameState);
        this.calculator = new ChoiceCalculator(gameState);
    }

    public ChoiceSet GenerateChoices(
        Encounter encounter,
        EncounterContext context)
    {
        ChoiceSetTemplate template = GetChoiceSetTemplate(context);
        EncounterValues currentValues = context.CurrentValues;

        List<EncounterChoice> choices = choiceSetGenerator.CreateEncounterChoices(encounter, context, template);
        var choiceSet = CreateChoiceSet(currentValues, context, choices);

        return choiceSet;
    }

    public ChoiceSet CreateChoiceSet(EncounterValues initialValues, EncounterContext context, List<EncounterChoice> choices)
    {
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context, initialValues);
        }

        foreach (EncounterChoice choice in choices)
        {
            EncounterValues projection = calculator.GetProjectedEncounterState(choice, initialValues, choice.CalculationResult.ValueModifications);
            choice.CalculationResult.ProjectedEncounterState = projection;
            if (!choice.IsEncounterFailingChoice && IsEncounterWon(context, projection)) choice.IsEncounterWinningChoice = true;
            if (!choice.IsEncounterWinningChoice && IsEncounterLost(context, projection)) choice.IsEncounterFailingChoice = true;
        }

        return new ChoiceSet(context.ActionType.ToString(), choices);
    }

    private bool IsEncounterWon(EncounterContext context, EncounterValues projection)
    {
        const int WIN_BASE = 10;
        int OUTCOME_WIN = context.Location.Difficulty + WIN_BASE;

        return projection.Outcome >= OUTCOME_WIN;
    }

    private bool IsEncounterLost(EncounterContext context, EncounterValues projection)
    {
        const int LOSE_BASE = 40;
        int PRESSURE_LOOSE = LOSE_BASE - context.Location.Difficulty;

        PlayerState player = gameState.Player;

        // Immediate loss if outcome is 0
        if (projection.Outcome <= 0)
            return true;

        // Immediate loss if pressure maxes out
        if (projection.Pressure >= PRESSURE_LOOSE)
            return true;

        return false;
    }

    private ChoiceSetTemplate GetChoiceSetTemplate(EncounterContext context)
    {
        CompositionPattern pattern = GameRules.GetCompositionPatternForActionType(context.ActionType);
        ChoiceSetTemplate choiceSetTemplate = new ChoiceSetTemplate(4, pattern);
        return choiceSetTemplate;
    }
}
