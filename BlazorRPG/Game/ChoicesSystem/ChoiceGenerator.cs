public class ChoiceGenerator
{
    private const int numberOfChoicesToGenerate = 4;

    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly List<(ChoiceArchetypes archetype, ChoiceApproaches approach)> alreadyUsedCombinations;

    public ChoiceGenerator(GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.alreadyUsedCombinations = new List<(ChoiceArchetypes, ChoiceApproaches)>();
    }

    public ChoiceSet Generate(EncounterChoiceTemplate template, EncounterContext context)
    {
        alreadyUsedCombinations.Clear();

        PlayerState playerState = gameState.Player;
        EncounterValues initialValues = context.CurrentValues;

        CompositionPattern pattern = GameRules.GetCompositionPatternForActionType(template.ActionType);

        List<EncounterChoice> choices = GenerateEncounterChoices(playerState, initialValues, pattern);

        foreach (EncounterChoice choice in choices)
        {
            choice.CalculationResult = calculator.CalculateChoiceEffects(choice, context, initialValues);

            EncounterValues projection = choice.CalculationResult.ProjectedEncounterState;
            choice.IsEncounterWinningChoice = IsEncounterWon(context, projection);

            if (!choice.IsEncounterWinningChoice)
            {
                choice.IsEncounterFailingChoice = IsEncounterLost(context, projection);
            }
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateEncounterChoices(PlayerState playerState, EncounterValues initialValues, CompositionPattern pattern)
    {
        List<EncounterChoice> encounterChoices = new List<EncounterChoice>();
        if (initialValues.Pressure < 9)
        {
            encounterChoices =
                GenerateBaseChoices(initialValues, playerState, numberOfChoicesToGenerate, pattern);
        }
        else
        {
            encounterChoices = GenerateDesperateOnlyChoices(pattern);
        }

        return encounterChoices;
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

    private List<EncounterChoice> GenerateBaseChoices(
        EncounterValues values,
        PlayerState playerState,
        int desiredChoiceCount, 
        CompositionPattern pattern)
    {
        List<EncounterChoice> choices = new();

        // First: Generate Primary Archetype choices
        int primaryChoiceCount = Math.Max(1, desiredChoiceCount / 2);
        GenerateArchetypeChoices(
            pattern.PrimaryArchetype,
            primaryChoiceCount,
            values,
            playerState,
            pattern,
            choices);

        // Second: Generate Secondary Archetype choices
        int secondaryChoiceCount = Math.Max(1, desiredChoiceCount / 3);
        GenerateArchetypeChoices(
            pattern.SecondaryArchetype,
            secondaryChoiceCount,
            values,
            playerState,
            pattern,
            choices);

        // Fill remaining slots with flexible choices
        while (choices.Count < desiredChoiceCount)
        {
            EncounterChoice flexibleChoice = TryGenerateFlexibleChoice(pattern, values, playerState);
            if (flexibleChoice == null)
                break;

            choices.Add(flexibleChoice);
            alreadyUsedCombinations.Add((flexibleChoice.Archetype, flexibleChoice.Approach));
        }

        return choices;
    }

    private void GenerateArchetypeChoices(
        ChoiceArchetypes archetype,
        int count,
        EncounterValues values,
        PlayerState playerState,
        CompositionPattern pattern,
        List<EncounterChoice> choices)
    {
        List<ChoiceApproaches> availableApproaches = GetUnusedAvailableApproaches(archetype, values, playerState, pattern);
        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);

        for (int i = 0; i < count && availableApproaches.Any(); i++)
        {
            ChoiceApproaches? approach = priorityOrder
                .FirstOrDefault(a => availableApproaches.Contains(a));

            if (approach == null)
                break;

            EncounterChoice choice = CreateChoice(choices.Count + 1, archetype, approach.Value);
            choices.Add(choice);
            alreadyUsedCombinations.Add((archetype, approach.Value));
            availableApproaches.Remove(approach.Value);
        }
    }

    private EncounterChoice TryGenerateFlexibleChoice(
        CompositionPattern pattern,
        EncounterValues values,
        PlayerState playerState)
    {
        // Try primary archetype first
        EncounterChoice primaryChoice = TryGenerateChoiceForArchetype(
            pattern.PrimaryArchetype,
            values,
            playerState,
            pattern);

        if (primaryChoice != null)
            return primaryChoice;

        // Then try secondary archetype
        EncounterChoice secondaryChoice = TryGenerateChoiceForArchetype(
                    pattern.SecondaryArchetype,
                    values,
                    playerState,
                    pattern);

        if (secondaryChoice != null)
            return secondaryChoice;

        return null;
    }

    private List<ChoiceApproaches> GetUnusedAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterValues values,
        PlayerState playerState,
        CompositionPattern pattern)
    {
        List<ChoiceApproaches> choiceApproaches = GameRules.GetAvailableApproaches(archetype, values, playerState);
        List<ChoiceApproaches> unusedChoiceApproaches = choiceApproaches
                    .Where(approach => !alreadyUsedCombinations.Any(used =>
                        used.archetype == archetype && used.approach == approach))
                    .ToList();
        return unusedChoiceApproaches;
    }

    private EncounterChoice TryGenerateChoiceForArchetype(
        ChoiceArchetypes archetype,
        EncounterValues values,
        PlayerState playerState,
        CompositionPattern pattern)
    {
        List<ChoiceApproaches> availableApproaches = GetUnusedAvailableApproaches(
            archetype, values, playerState, pattern);

        if (!availableApproaches.Any())
            return null;

        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);
        ChoiceApproaches? approach = priorityOrder.FirstOrDefault(a => availableApproaches.Contains(a));

        if (approach == null)
            return null;

        if (!alreadyUsedCombinations.Any(c => c.archetype == pattern.PrimaryArchetype && c.approach == approach))
        {
            EncounterChoice choice = CreateChoice(alreadyUsedCombinations.Count + 1, archetype, approach.Value);
            return choice;
        }

        return null;
    }

    private List<EncounterChoice> GenerateDesperateOnlyChoices(CompositionPattern pattern)
    {
        List<EncounterChoice> choices = new();

        // Only generate desperate choices if the combination hasn't been used
        if (!alreadyUsedCombinations.Any(c => c.archetype == pattern.PrimaryArchetype && c.approach == ChoiceApproaches.Desperate))
        {
            choices.Add(CreateChoice(0, pattern.PrimaryArchetype, ChoiceApproaches.Careful));
            choices.Add(CreateChoice(0, pattern.PrimaryArchetype, ChoiceApproaches.Desperate));
            alreadyUsedCombinations.Add((pattern.PrimaryArchetype, ChoiceApproaches.Desperate));
        }

        if (!alreadyUsedCombinations.Any(c => c.archetype == pattern.SecondaryArchetype && c.approach == ChoiceApproaches.Desperate))
        {
            choices.Add(CreateChoice(choices.Count, pattern.SecondaryArchetype, ChoiceApproaches.Desperate));
            alreadyUsedCombinations.Add((pattern.SecondaryArchetype, ChoiceApproaches.Desperate));
        }

        return choices;
    }

    private EncounterChoice CreateChoice(int index, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        bool requireTool = archetype == ChoiceArchetypes.Physical && approach == ChoiceApproaches.Strategic;
        bool requireKnowledge = archetype == ChoiceArchetypes.Focus && approach == ChoiceApproaches.Strategic;
        bool requireReputation = archetype == ChoiceArchetypes.Social && approach == ChoiceApproaches.Strategic;

        EncounterChoice choice = new(
            index,
            $"{archetype} - {approach}",
            $"{archetype} - {approach}",
            archetype,
            approach,
            requireTool,
            requireKnowledge,
            requireReputation);

        return choice;
    }
}