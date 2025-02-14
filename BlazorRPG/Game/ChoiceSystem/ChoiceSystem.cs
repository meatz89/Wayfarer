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
        EncounterContext encounterContext,
        EncounterStageContext encounterStageContext
        )
    {
        ChoiceSetTemplate template = GetChoiceSetTemplate(encounterContext);
        EncounterStageState currentValues = encounterStageContext.StageValues;

        List<EncounterChoice> choices = choiceSetGenerator.CreateEncounterChoices(encounter, encounterContext, encounterStageContext, template);
        var choiceSet = CreateChoiceSet(currentValues, encounterContext, choices);

        return choiceSet;
    }

    public ChoiceSet CreateChoiceSet(EncounterStageState initialValues, EncounterContext context, List<EncounterChoice> choices)
    {
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context, initialValues);
        }

        foreach (EncounterChoice choice in choices)
        {
            EncounterStageState projection = calculator.GetProjectedEncounterState(choice, initialValues, choice.CalculationResult.ValueModifications);
            choice.CalculationResult.ProjectedEncounterState = projection;
            if (!choice.IsEncounterFailingChoice && IsEncounterWon(context, projection)) choice.IsEncounterWinningChoice = true;
            if (!choice.IsEncounterWinningChoice && IsEncounterLost(context, projection)) choice.IsEncounterFailingChoice = true;
        }

        return new ChoiceSet(context.ActionType.ToString(), choices);
    }

    private bool IsEncounterWon(EncounterContext context, EncounterStageState projection)
    {
        const int WIN_BASE = 10;
        int OUTCOME_WIN = context.Location.Difficulty + WIN_BASE;

        if(projection.Momentum >= OUTCOME_WIN)
            return true;

        return false;
    }

    private bool IsEncounterLost(EncounterContext context, EncounterStageState projection)
    {
        const int LOSE_BASE = -10;
        if (projection.Momentum <= LOSE_BASE)
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
