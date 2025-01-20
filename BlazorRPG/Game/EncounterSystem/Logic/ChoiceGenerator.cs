public class ChoiceGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceEffectsGenerator baseValueGenerator;
    private readonly HashSet<(ChoiceArchetypes, ChoiceApproaches)> usedCombinations;

    public ChoiceGenerator(GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.baseValueGenerator = new ChoiceEffectsGenerator();
        this.usedCombinations = new HashSet<(ChoiceArchetypes, ChoiceApproaches)>();
    }

    public ChoiceSet Generate(ChoiceSetTemplate template, EncounterContext context)
    {
        usedCombinations.Clear();

        PlayerState playerState = gameState.Player;
        EncounterValues currentValues = context.CurrentValues;

        // Generate base choices
        List<EncounterChoice> choices = GenerateBaseChoices(template, currentValues, playerState);

        // Pre-calculate effects for each choice
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoiceEffects(choice, context);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateBaseChoices(
        ChoiceSetTemplate template,
        EncounterValues values,
        PlayerState playerState)
    {
        List<EncounterChoice> choices = new();
        CompositionPattern pattern = template.CompositionPattern;

        if (values.Pressure >= 9)
        {
            return GenerateDesperateOnlyChoices(pattern.PrimaryArchetype);
        }

        // First Choice: Best non-strategic approach from primary archetype
        EncounterChoice firstChoice = GenerateFirstChoice(pattern.PrimaryArchetype, values, playerState);
        if (firstChoice != null)
        {
            choices.Add(firstChoice);
        }

        // Second Choice: Strategic or best available from primary/secondary
        EncounterChoice secondChoice = GenerateSecondChoice(pattern, firstChoice, values, playerState);
        if (secondChoice != null)
        {
            choices.Add(secondChoice);
        }

        // Third Choice: Strategic or best available from primary/secondary
        if (choices.Count >= 2)
        {
            EncounterChoice thirdChoice = GenerateThirdChoice(pattern, firstChoice, secondChoice, values, playerState);
            if (thirdChoice != null)
            {
                choices.Add(thirdChoice);
            }
        }

        // Special fourth choice if high mastery value
        if (choices.Count < 4 && ShouldAddSpecialChoice(values))
        {
            EncounterChoice specialChoice = TryGenerateSpecialChoice(choices.Count, template, values);
            // Only add if it doesn't duplicate an existing strategic choice
            if (specialChoice != null && !HasStrategicChoice(choices, specialChoice.Archetype))
            {
                choices.Add(specialChoice);
            }
        }

        // Fallback if insufficient choices
        if (choices.Count < 3)
        {
            if (IsChoicePossible(pattern.PrimaryArchetype, ChoiceApproaches.Desperate, values, playerState))
                choices.Add(CreateChoice(choices.Count + 1, pattern.PrimaryArchetype, ChoiceApproaches.Desperate));
        }

        if (choices.Count < 3)
        {
            if (IsChoicePossible(pattern.SecondaryArchetype, ChoiceApproaches.Desperate, values, playerState))
                choices.Add(CreateChoice(choices.Count + 1, pattern.SecondaryArchetype, ChoiceApproaches.Desperate));
        }

        return choices;
    }

    private EncounterChoice GenerateFirstChoice(
        ChoiceArchetypes archetype,
        EncounterValues values,
        PlayerState playerState)
    {
        const int choiceIndex = 1;

        // Get available approaches excluding Strategic
        List<ChoiceApproaches> approaches = GetAvailableApproaches(archetype, values, playerState)
            .Where(a => a != ChoiceApproaches.Strategic)
            .ToList();

        // Use SelectBestApproach to choose the best available approach
        ChoiceApproaches selectedApproach = SelectBestApproach(archetype, approaches, values, playerState);

        return CreateChoice(choiceIndex, archetype, selectedApproach);
    }

    private bool HasStrategicChoice(List<EncounterChoice> choices, ChoiceArchetypes archetype)
    {
        return choices.Any(c =>
            c.Archetype == archetype &&
            c.Approach == ChoiceApproaches.Strategic);
    }

    private EncounterChoice GenerateSecondChoice(
        CompositionPattern pattern,
        EncounterChoice firstChoice,
        EncounterValues values,
        PlayerState playerState)
    {
        const int choiceIndex = 2;

        // First try a different approach with the primary archetype
        // Get all available approaches except Strategic and the one used in first choice
        List<ChoiceApproaches> availableApproaches = GetAvailableApproaches(pattern.PrimaryArchetype, values, playerState)
            .Where(a => a != firstChoice.Approach && a != ChoiceApproaches.Strategic)
            .ToList();

        if (availableApproaches.Any())
        {
            // Use our priority-based approach selection for the primary archetype
            ChoiceApproaches approach = SelectBestApproach(pattern.PrimaryArchetype, availableApproaches, values, playerState);
            return CreateChoice(choiceIndex, pattern.PrimaryArchetype, approach);
        }

        // If we couldn't make another primary archetype choice, then try strategic choices
        if (IsChoicePossible(pattern.SecondaryArchetype, ChoiceApproaches.Strategic, values, playerState))
        {
            return CreateChoice(choiceIndex, pattern.SecondaryArchetype, ChoiceApproaches.Strategic);
        }

        ChoiceArchetypes remainingArchetype = GetRemainingArchetype(pattern.PrimaryArchetype, pattern.SecondaryArchetype);
        if (IsChoicePossible(remainingArchetype, ChoiceApproaches.Strategic, values, playerState))
        {
            return CreateChoice(choiceIndex, remainingArchetype, ChoiceApproaches.Strategic);
        }

        // As a last resort, try a regular choice with the secondary archetype
        return TryGenerateRegularChoice(pattern.SecondaryArchetype, firstChoice, values, playerState);
    }

    private EncounterChoice GenerateThirdChoice(
        CompositionPattern pattern,
        EncounterChoice firstChoice,
        EncounterChoice secondChoice,
        EncounterValues values,
        PlayerState playerState)
    {
        const int choiceIndex = 3;

        // First try Strategic choices in priority order
        // Primary archetype strategic
        if (!HasArchetype(firstChoice, secondChoice, pattern.PrimaryArchetype) &&
            IsChoicePossible(pattern.PrimaryArchetype, ChoiceApproaches.Strategic, values, playerState))
        {
            return CreateChoice(choiceIndex, pattern.PrimaryArchetype, ChoiceApproaches.Strategic);
        }

        // Secondary archetype strategic
        if (!HasArchetype(firstChoice, secondChoice, pattern.SecondaryArchetype) &&
            IsChoicePossible(pattern.SecondaryArchetype, ChoiceApproaches.Strategic, values, playerState))
        {
            return CreateChoice(choiceIndex, pattern.SecondaryArchetype, ChoiceApproaches.Strategic);
        }

        // Third archetype strategic (special case allowed)
        ChoiceArchetypes thirdArchetype = GetRemainingArchetype(pattern.PrimaryArchetype, pattern.SecondaryArchetype);
        if (IsChoicePossible(thirdArchetype, ChoiceApproaches.Strategic, values, playerState))
        {
            return CreateChoice(choiceIndex, thirdArchetype, ChoiceApproaches.Strategic);
        }

        //// If no Strategic choice possible, must use either primary or secondary archetype
        //// Figure out which one hasn't been used yet
        //if (!HasArchetype(firstChoice, secondChoice, pattern.PrimaryArchetype))
        //{
        //    return TryGenerateRegularChoice(pattern.PrimaryArchetype, secondChoice, values, playerState);
        //}
        //if (!HasArchetype(firstChoice, secondChoice, pattern.SecondaryArchetype))
        //{
        //    return TryGenerateRegularChoice(pattern.SecondaryArchetype, secondChoice, values, playerState);
        //}

        // If both archetypes are used, we can't generate a third choice
        return null;
    }

    private bool HasArchetype(EncounterChoice first, EncounterChoice second, ChoiceArchetypes archetype)
    {
        return (first?.Archetype == archetype) || (second?.Archetype == archetype);
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
        List<ChoiceApproaches> approaches = GetAvailableApproaches(archetype, values, playerState)
            .Where(a => !usedCombinations.Contains((archetype, a)))
            .ToList();

        // Try each approach in priority order
        foreach (ChoiceApproaches approach in GameRules.GetPriorityOrder(archetype, values))
        {
            if (approaches.Contains(approach) && IsChoicePossible(archetype, approach, values, playerState))
            {
                return CreateChoice(existingChoice.Index + 1, archetype, approach);
            }
        }

        return null;
    }

    private bool IsChoicePossible(
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        EncounterValues values,
        PlayerState playerState)
    {
        // Check mastery value requirements
        if (approach == ChoiceApproaches.Strategic)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical when values.Momentum < GameRules.StrategicMomentumRequirement:
                case ChoiceArchetypes.Focus when values.Insight < GameRules.StrategicInsightRequirement:
                case ChoiceArchetypes.Social when values.Resonance < GameRules.StrategicResonanceRequirement:
                    return false;
            }
        }

        // Add other requirement checks here (energy, skills, etc.)
        // We can expand this as needed

        if (usedCombinations.Contains((archetype, approach))) return false;

        return true;
    }

    private List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterValues values,
        PlayerState playerState)
    {
        List<ChoiceApproaches> approaches = new();

        // Apply archetype-specific pressure thresholds
        int pressureThreshold = archetype switch
        {
            ChoiceArchetypes.Physical => 4, // Physical work inherently creates pressure
            ChoiceArchetypes.Focus => 2,    // Pressure disrupts concentration more
            ChoiceArchetypes.Social => 3,    // Standard threshold
            _ => throw new ArgumentException("Invalid archetype")
        };

        // Check each approach against requirements
        if (values.Pressure <= pressureThreshold)
            approaches.Add(ChoiceApproaches.Aggressive);

        if (values.Pressure <= 6)
            approaches.Add(ChoiceApproaches.Strategic);

        if (values.Pressure <= 8)
            approaches.Add(ChoiceApproaches.Careful);

        return approaches;
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

    private bool ShouldAddSpecialChoice(EncounterValues values)
    {
        // Add special choice if any mastery value is high enough and pressure isn't too high
        return values.Pressure <= 6 && (
            values.Insight >= 7 ||
            values.Resonance >= 7 ||
            values.Momentum >= 7
        );
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
            if (!IsChoicePossible(archetype, approach, values, playerState))
            {
                continue;
            }

            if (availableApproaches.Contains(approach))
            {
                return approach;
            }
        }

        return ChoiceApproaches.Desperate;
    }

    private EncounterChoice TryGenerateSpecialChoice(
        int index,
        ChoiceSetTemplate template,
        EncounterValues values)
    {
        // Determine which mastery value enables this special choice
        ChoiceArchetypes specialArchetype = ChoiceArchetypes.Focus;
        if (values.Insight >= 7) specialArchetype = ChoiceArchetypes.Focus;
        else if (values.Resonance >= 7) specialArchetype = ChoiceArchetypes.Social;
        else if (values.Momentum >= 7) specialArchetype = ChoiceArchetypes.Physical;

        return CreateChoice(index, specialArchetype, ChoiceApproaches.Strategic);
    }

    private EncounterChoice CreateChoice(int index, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        bool requireTool = archetype == ChoiceArchetypes.Physical && approach == ChoiceApproaches.Strategic;
        bool requireKnowledge = archetype == ChoiceArchetypes.Focus && approach == ChoiceApproaches.Strategic;
        bool requireReputation = archetype == ChoiceArchetypes.Social && approach == ChoiceApproaches.Strategic;

        EncounterChoice choice = new(
            index,
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

}
