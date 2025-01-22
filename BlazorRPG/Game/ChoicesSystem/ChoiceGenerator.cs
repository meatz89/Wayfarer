public class ChoiceGenerator
{
    private const int numberOfChoicesToGenerate = 4;

    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly List<(ChoiceArchetypes archetype, ChoiceApproaches approach)> usedCombinations;

    public ChoiceGenerator(GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.usedCombinations = new List<(ChoiceArchetypes, ChoiceApproaches)>();
    }

    public ChoiceSet Generate(ChoiceSetTemplate template, EncounterContext context)
    {
        usedCombinations.Clear();

        PlayerState playerState = gameState.Player;
        EncounterValues initialValues = context.CurrentValues;

        List<EncounterChoice> choices = initialValues.Pressure >= 9
            ? GenerateDesperateOnlyChoices(template.CompositionPattern)
            : GenerateBaseChoices(template, initialValues, playerState, numberOfChoicesToGenerate);

        foreach (EncounterChoice choice in choices)
        {
            choice.CalculationResult = calculator.CalculateChoiceEffects(choice, context.LocationProperties, initialValues);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateBaseChoices(
        ChoiceSetTemplate template,
        EncounterValues values,
        PlayerState playerState,
        int desiredChoiceCount)
    {
        List<EncounterChoice> choices = new();
        CompositionPattern pattern = template.CompositionPattern;

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
            usedCombinations.Add((flexibleChoice.Archetype, flexibleChoice.Approach));
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
            usedCombinations.Add((archetype, approach.Value));
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
                    .Where(approach => !usedCombinations.Any(used =>
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

        if (!usedCombinations.Any(c => c.archetype == pattern.PrimaryArchetype && c.approach == approach))
        {
            var choice = CreateChoice(usedCombinations.Count + 1, archetype, approach.Value);
            return choice;
        }

        return null;
    }

    private List<EncounterChoice> GenerateDesperateOnlyChoices(CompositionPattern pattern)
    {
        List<EncounterChoice> choices = new();

        // Only generate desperate choices if the combination hasn't been used
        if (!usedCombinations.Any(c => c.archetype == pattern.PrimaryArchetype && c.approach == ChoiceApproaches.Desperate))
        {
            choices.Add(CreateChoice(0, pattern.PrimaryArchetype, ChoiceApproaches.Desperate));
            usedCombinations.Add((pattern.PrimaryArchetype, ChoiceApproaches.Desperate));
        }

        if (!usedCombinations.Any(c => c.archetype == pattern.SecondaryArchetype && c.approach == ChoiceApproaches.Desperate))
        {
            choices.Add(CreateChoice(choices.Count, pattern.SecondaryArchetype, ChoiceApproaches.Desperate));
            usedCombinations.Add((pattern.SecondaryArchetype, ChoiceApproaches.Desperate));
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