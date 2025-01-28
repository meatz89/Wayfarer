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

    public ChoiceSet Generate(Encounter encounter, EncounterContext context)
    {
        alreadyUsedCombinations.Clear();

        PlayerState playerState = gameState.Player;
        EncounterValues initialValues = context.CurrentValues;
        CompositionPattern pattern = GameRules.GetCompositionPatternForActionType(context.ActionType);

        List<EncounterChoice> choices = new List<EncounterChoice>();

        List<EncounterChoice> forcedChoiceSubstitutions = GetForcedChoiceSubstituations(encounter, context);
        choices.AddRange(forcedChoiceSubstitutions);
        if (choices.Count == 0)
        {
            List<EncounterChoice> baseChoices = GenerateBaseEncounterChoices(playerState, encounter, context, initialValues, pattern);
            choices.AddRange(baseChoices);

            foreach (EncounterChoice choice in choices)
            {
                calculator.CalculateChoiceEffects(choice, context, initialValues);
            }
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

    private List<EncounterChoice> GetForcedChoiceSubstituations(Encounter encounter, EncounterContext context)
    {
        List<EncounterChoiceTemplate> encounterChoiceTemplates =
            GetChoiceSubstitutions(encounter, context);

        List<EncounterChoice> encounterChoices = CreateChoicesFromTemplate(encounterChoiceTemplates);

        return encounterChoices;
    }

    private List<EncounterChoice> CreateChoicesFromTemplate(List<EncounterChoiceTemplate> encounterChoiceTemplates)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();

        if (encounterChoiceTemplates.Count != 0)
        {
            int index = 1;
            foreach (EncounterChoiceTemplate template in encounterChoiceTemplates)
            {
                ChoiceSlotTypes choiceSlotTypes = template.ChoiceSlotType;

                ChoiceArchetypes archetype = template.Archetype;
                ChoiceApproaches approach = template.Approach;

                string name = template.Name;
                EncounterChoice choice = new EncounterChoice(
                    index,
                    $"{archetype} - {approach}",
                    name,
                    archetype,
                    approach);

                choice.SetModifiedChoiceSlotUnlocks(template.ChoiceSlotModifications);

                if (template.EncounterResults.HasValue
                    && template.EncounterResults.Value == EncounterResults.EncounterFailure)
                {
                    choice.IsEncounterFailingChoice = true;
                }
                else if (template.EncounterResults.HasValue
                    && template.EncounterResults.Value == EncounterResults.EncounterSuccess)
                {
                    choice.IsEncounterWinningChoice = true;
                }

                choice.CalculationResult = new ChoiceCalculationResult(
                    template.ValueModifications,
                    template.Requirements,
                    template.Costs,
                    template.Rewards);

                choices.Add(choice);
                index++;
            }
        }

        return choices;
    }

    private List<EncounterChoice> GenerateBaseEncounterChoices(
        PlayerState playerState,
        Encounter encounter,
        EncounterContext context,
        EncounterValues initialValues,
        CompositionPattern pattern)
    {
        List<EncounterChoice> encounterChoices = new List<EncounterChoice>();

        if (initialValues.Pressure < 9)
        {
            encounterChoices = GenerateBaseChoices(initialValues, playerState, numberOfChoicesToGenerate, pattern);
        }
        else
        {
            encounterChoices = GenerateDesperateOnlyChoices(pattern);
        }
        return encounterChoices;
    }

    private List<EncounterChoiceTemplate> GetChoiceSubstitutions(Encounter encounter, EncounterContext context)
    {
        EncounterValues currentValues = context.CurrentValues;

        List<EncounterChoiceTemplate> choices = new List<EncounterChoiceTemplate>();
        if (encounter.BaseSlots.Count != 0)
        {
            for (int i = encounter.BaseSlots.Count - 1; i >= 0; i--)
            {
                EncounterChoiceSlot choiceSlot = encounter.BaseSlots[i];
                if (!choiceSlot.MeetsEncounterStateConditions(currentValues)) continue;
                choices.Add(choiceSlot.GetChoiceTemplate());

                encounter.BaseSlots.Remove(choiceSlot);
            }
        }
        if (encounter.ModifiedSlots.Count != 0)
        {
            for (int i = encounter.ModifiedSlots.Count - 1; i >= 0; i--)
            {
                EncounterChoiceSlot choiceSlot = encounter.ModifiedSlots[i];
                if (!choiceSlot.MeetsEncounterStateConditions(currentValues)) continue;
                choices.Add(choiceSlot.GetChoiceTemplate());

                encounter.ModifiedSlots.Remove(choiceSlot);
            }
        }

        return choices;
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
        EncounterChoice choice = new EncounterChoice(
            index,
            $"{archetype} - {approach}",
            $"{archetype} - {approach}",
            archetype,
            approach);

        choice.CalculationResult = new ChoiceCalculationResult(
            new List<ValueModification>(),
            new List<Requirement>(),
            new List<Outcome>(),
            new List<Outcome>()
            );

        return choice;
    }
}