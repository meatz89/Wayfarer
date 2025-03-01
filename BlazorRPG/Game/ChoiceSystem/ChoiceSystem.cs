using System.Reflection.Emit;

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
        //this.choiceSetGenerator = new ChoiceGenerator(gameState);
        this.calculator = new ChoiceCalculator(gameState);
    }

    public ChoicesModel GenerateChoices(
        Encounter encounter,
        EncounterContext encounterContext,
        EncounterState encounterState
        )
    {
        ChoiceSetTemplate template = GetChoiceSetTemplate(encounterContext);

        //List<EncounterChoice> choices = new List<EncounterChoice>(); // choiceSetGenerator.CreateEncounterChoices(encounter, encounterContext, encounterStageContext, template);
        //ChoiceSet choiceSet = CreateChoiceSet(currentValues, encounterContext, choices);

        //RunExample();

        // Create an initial encounter state
        EncounterState state = new EncounterState
        {
            Momentum = 0,
            Pressure = 0,
            CurrentTurn = 1
        };

        // Set some initial tag values
        state.ApproachTags[ApproachTypes.Force] = 1;
        state.ApproachTags[ApproachTypes.Charm] = 2;
        state.ApproachTags[ApproachTypes.Wit] = 1;
        state.FocusTags[FocusTypes.Relationship] = 1;
        state.FocusTags[FocusTypes.Information] = 2;

        ChoiceGenerator generator = new ChoiceGenerator();
        List<Choice> choices = generator.GenerateChoiceSet(state);

        ChoicesModel choicesModel = new ChoicesModel();
        choicesModel.Choices = choices;
        choicesModel.EncounterState = state;

        return choicesModel;
    }

    public void ApplyChoice(EncounterState state, Choice selectedChoice)
    {
        state.ApplyChoice(selectedChoice, state.IsStable);
    }

    private static void RunExample()
    {
        // Create an initial encounter state
        EncounterState state = new EncounterState
        {
            Momentum = 0,
            Pressure = 0,
            CurrentTurn = 1
        };

        // Set some initial tag values
        state.ApproachTags[ApproachTypes.Force] = 1;
        state.ApproachTags[ApproachTypes.Charm] = 2;
        state.ApproachTags[ApproachTypes.Wit] = 1;
        state.FocusTags[FocusTypes.Relationship] = 1;
        state.FocusTags[FocusTypes.Information] = 2;

        // Create choice generator
        ChoiceGenerator generator = new ChoiceGenerator();

        // Example of running an encounter for 6 turns
        for (int turn = 1; turn <= 6; turn++)
        {
            Console.WriteLine($"Turn {turn}");
            Console.WriteLine($"Momentum: {state.Momentum}, Pressure: {state.Pressure}");
            Console.WriteLine("Tag Values:");

            foreach (KeyValuePair<ApproachTypes, int> pair in state.ApproachTags)
            {
                Console.WriteLine($"  {pair.Key}: {pair.Value}");
            }

            foreach (KeyValuePair<FocusTypes, int> pair in state.FocusTags)
            {
                Console.WriteLine($"  {pair.Key}: {pair.Value}");
            }

            // Generate choices for this turn
            List<Choice> choices = generator.GenerateChoiceSet(state);

            Console.WriteLine("Available Choices:");
            for (int i = 0; i < choices.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {choices[i].Name} ({choices[i].EffectType}, {choices[i].Approach}, {choices[i].Focus})");
            }

            // Simulate player selecting the first choice
            Choice selectedChoice = choices[0];
            Console.WriteLine($"Selected: {selectedChoice.Name}");

            // Apply the choice effects
            state.ApplyChoice(selectedChoice, state.IsStable);

            Console.WriteLine();
        }
    }

    public ChoiceSet CreateChoiceSet(EncounterStageState initialValues, EncounterContext context, List<Choice> choices)
    {
        foreach (Choice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context, initialValues);
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

    private bool IsEncounterWon(EncounterContext context, EncounterStageState projection)
    {
        const int WIN_BASE = 10;
        int OUTCOME_WIN = context.Location.Difficulty + WIN_BASE;

        if (projection.Momentum >= OUTCOME_WIN)
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
