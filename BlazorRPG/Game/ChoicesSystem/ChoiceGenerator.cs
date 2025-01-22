public class ChoiceGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly BaseValueChangeGenerator baseValueGenerator;
    private readonly HashSet<(ChoiceArchetypes, ChoiceApproaches)> usedCombinations;

    public ChoiceGenerator(GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.baseValueGenerator = new BaseValueChangeGenerator();
        this.usedCombinations = new HashSet<(ChoiceArchetypes, ChoiceApproaches)>();
    }

    private List<EncounterChoice> GenerateBaseChoices(
        ChoiceSetTemplate template,
        EncounterValues values,
        PlayerState playerState,
        int desiredChoiceCount)
    {
        List<EncounterChoice> choices = new();
        CompositionPattern pattern = template.CompositionPattern;

        if (values.Pressure >= 9)
            return GenerateDesperateOnlyChoices(pattern.PrimaryArchetype);

        // First: Generate Primary Archetype choices (should be about half of desired choices)
        int primaryChoiceCount = Math.Max(1, desiredChoiceCount / 2);
        GenerateArchetypeChoices(
            pattern.PrimaryArchetype,
            primaryChoiceCount,
            choices,
            values,
            playerState);

        // Second: Generate Secondary Archetype choices (about a third of desired choices)
        int secondaryChoiceCount = Math.Max(1, desiredChoiceCount / 3);
        GenerateArchetypeChoices(
            pattern.SecondaryArchetype,
            secondaryChoiceCount,
            choices,
            values,
            playerState);

        // Fill remaining slots with flexible choices
        while (choices.Count < desiredChoiceCount)
        {
            var flexibleChoice = GenerateFlexibleChoice(pattern, choices, values, playerState);
            if (flexibleChoice == null)
                break;
            choices.Add(flexibleChoice);
        }

        return choices;
    }

    private void GenerateArchetypeChoices(
        ChoiceArchetypes archetype,
        int count,
        List<EncounterChoice> existingChoices,
        EncounterValues values,
        PlayerState playerState)
    {
        var availableApproaches = GameRules.GetAvailableApproaches(archetype, values, playerState)
            .Where(a => !usedCombinations.Contains((archetype, a)))
            .ToList();

        var priorityOrder = GameRules.GetPriorityOrder(archetype, values);

        // Try to generate the requested number of choices
        for (int i = 0; i < count && availableApproaches.Any(); i++)
        {
            // Get the highest priority approach that's still available
            var approach = priorityOrder
                .FirstOrDefault(a => availableApproaches.Contains(a));

            existingChoices.Add(CreateChoice(
                existingChoices.Count + 1,
                archetype,
                approach));

            // Remove used approach from available approaches
            availableApproaches.Remove(approach);
        }
    }

    private EncounterChoice GenerateFlexibleChoice(
        CompositionPattern pattern,
        List<EncounterChoice> existingChoices,
        EncounterValues values,
        PlayerState playerState)
    {
        // First try primary archetype
        var choice = TryGenerateChoiceForArchetype(
            pattern.PrimaryArchetype,
            existingChoices,
            values,
            playerState);

        // Then try secondary archetype
        if (choice == null)
        {
            choice = TryGenerateChoiceForArchetype(
                pattern.SecondaryArchetype,
                existingChoices,
                values,
                playerState);
        }

        return choice;
    }

    private EncounterChoice TryGenerateChoiceForArchetype(
        ChoiceArchetypes archetype,
        List<EncounterChoice> existingChoices,
        EncounterValues values,
        PlayerState playerState)
    {
        var availableApproaches = GameRules.GetAvailableApproaches(archetype, values, playerState)
            .Where(a => !usedCombinations.Contains((archetype, a)))
            .ToList();

        if (!availableApproaches.Any())
            return null;

        var priorityOrder = GameRules.GetPriorityOrder(archetype, values);
        var approach = priorityOrder.FirstOrDefault(a => availableApproaches.Contains(a));

        return CreateChoice(
            existingChoices.Count + 1,
            archetype,
            approach);
    }

    public ChoiceSet Generate(ChoiceSetTemplate template, EncounterContext context)
    {
        usedCombinations.Clear();

        PlayerState playerState = gameState.Player;
        EncounterValues currentValues = context.CurrentValues;

        // Generate base choices
        List<EncounterChoice> choices = GenerateBaseChoices(template, currentValues, playerState, 9);

        // Pre-calculate effects for each choice
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private ChoiceApproaches SelectBestApproach(
        ChoiceArchetypes archetype,
        List<ChoiceApproaches> availableApproaches,
        EncounterValues values,
        PlayerState playerState)
    {
        // Define archetype-specific priority orders
        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);

        foreach (ChoiceApproaches approach in priorityOrder)
        {
            if (GameRules.IsChoicePossible(archetype, approach, values, playerState) &&
                    !usedCombinations.Contains((archetype, approach)))
            {
                if (availableApproaches.Contains(approach))
                {
                    return approach;
                }
            }
        }

        return ChoiceApproaches.Desperate;
    }


    private List<EncounterChoice> GenerateDesperateOnlyChoices(ChoiceArchetypes primaryArchetype)
    {
        List<EncounterChoice> choices = new();

        // Two primary archetype choices
        for (int i = 0; i < 2; i++)
        {
            choices.Add(CreateChoice(i, primaryArchetype, ChoiceApproaches.Desperate));
        }

        // One secondary choice (Focus for Physical/Social, Social for Focus)
        ChoiceArchetypes secondaryArchetype = primaryArchetype == ChoiceArchetypes.Focus
            ? ChoiceArchetypes.Social
            : ChoiceArchetypes.Focus;

        choices.Add(CreateChoice(2, secondaryArchetype, ChoiceApproaches.Desperate));

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

        // Set base values using generator
        choice.BaseEncounterValueChanges = baseValueGenerator.GenerateBaseValueChanges(archetype, approach);

        // Mark the combination as used
        usedCombinations.Add((archetype, approach));

        return choice;
    }

    private ChoiceArchetypes GetRemainingArchetype(ChoiceArchetypes first, ChoiceArchetypes second)
    {
        return Enum.GetValues<ChoiceArchetypes>()
            .First(a => a != first && a != second);
    }

    private EncounterChoice TryGenerateRegularChoice(
        ChoiceArchetypes archetype,
        EncounterChoice existingChoice,
        EncounterValues values,
        PlayerState playerState)
    {
        // Get available approaches excluding those already used
        List<ChoiceApproaches> approaches = GameRules.GetAvailableApproaches(archetype, values, playerState)
            .Where(a => !usedCombinations.Contains((archetype, a)))
            .ToList();

        // Try each approach in priority order
        foreach (ChoiceApproaches approach in GameRules.GetPriorityOrder(archetype, values))
        {
            if (approaches.Contains(approach) && GameRules.IsChoicePossible(archetype, approach, values, playerState))
            {
                return CreateChoice(existingChoice.Index + 1, archetype, approach);
            }
        }

        return null;
    }

}
