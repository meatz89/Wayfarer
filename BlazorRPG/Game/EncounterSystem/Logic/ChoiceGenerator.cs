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
        EncounterStateValues currentValues = context.CurrentValues;

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
    EncounterStateValues values,
    PlayerState playerState)
    {
        List<EncounterChoice> choices = new();
        CompositionPattern pattern = template.CompositionPattern;

        // Handle extreme pressure scenario first
        if (values.Pressure >= 9)
        {
            return GenerateDesperateOnlyChoices(pattern.PrimaryArchetype);
        }

        // Try to generate primary archetype choices (2 choices)
        for (int i = 0; i < 2; i++)
        {
            EncounterChoice choice = TryGenerateChoice(pattern.PrimaryArchetype, values, playerState);

            if (choice == null)
            {
                // If we couldn't generate a regular choice, try a desperate fallback
                if (!HasDesperateChoice(choices, pattern.PrimaryArchetype))
                {
                    choice = CreateChoice(choices.Count, pattern.PrimaryArchetype, ChoiceApproaches.Desperate);
                }
                else if (!HasDesperateChoice(choices, pattern.SecondaryArchetype))
                {
                    choice = CreateChoice(choices.Count, pattern.SecondaryArchetype, ChoiceApproaches.Desperate);
                }
                else if (!HasThirdArchetypeDesperateChoice(choices))
                {
                    ChoiceArchetypes thirdArchetype = GetMissingArchetype(choices);
                    choice = CreateChoice(choices.Count, thirdArchetype, ChoiceApproaches.Desperate);
                }
            }

            if (choice != null)
            {
                choices.Add(choice);
            }
        }

        // Only try to generate secondary choice if we have less than 3 choices
        if (choices.Count < 3)
        {
            EncounterChoice secondaryChoice = TryGenerateChoice(pattern.SecondaryArchetype, values, playerState);

            if (secondaryChoice == null)
            {
                // Apply the same fallback logic
                if (!HasDesperateChoice(choices, pattern.PrimaryArchetype))
                {
                    secondaryChoice = CreateChoice(choices.Count, pattern.PrimaryArchetype, ChoiceApproaches.Desperate);
                }
                else if (!HasDesperateChoice(choices, pattern.SecondaryArchetype))
                {
                    secondaryChoice = CreateChoice(choices.Count, pattern.SecondaryArchetype, ChoiceApproaches.Desperate);
                }
                else if (!HasThirdArchetypeDesperateChoice(choices))
                {
                    ChoiceArchetypes thirdArchetype = GetMissingArchetype(choices);
                    secondaryChoice = CreateChoice(choices.Count, thirdArchetype, ChoiceApproaches.Desperate);
                }
            }

            if (secondaryChoice != null)
            {
                choices.Add(secondaryChoice);
            }
        }

        // Add special choice if applicable and we don't already have too many choices
        if (choices.Count < 4 && ShouldAddSpecialChoice(values))
        {
            EncounterChoice specialChoice = TryGenerateSpecialChoice(choices.Count, template, values);
            if (specialChoice != null)
            {
                choices.Add(specialChoice);
            }
        }

        return choices;
    }

    private EncounterChoice TryGenerateChoice(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Get an approach according to our priority rules
        ChoiceApproaches approach = SelectApproach(archetype, values, playerState);

        // Check if this choice would be possible
        if (!IsChoicePossible(archetype, approach, values, playerState))
        {
            return null;
        }

        return CreateChoice(0, archetype, approach);
    }

    private bool IsChoicePossible(
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Check mastery value requirements
        if (approach == ChoiceApproaches.Strategic)
        {
            switch (archetype)
            {
                case ChoiceArchetypes.Physical when values.Momentum < 3:
                case ChoiceArchetypes.Focus when values.Insight < 3:
                case ChoiceArchetypes.Social when values.Resonance < 3:
                    return false;
            }
        }

        // Add other requirement checks here (energy, skills, etc.)
        // We can expand this as needed

        return true;
    }

    private bool HasDesperateChoice(List<EncounterChoice> choices, ChoiceArchetypes archetype)
    {
        return choices.Any(c =>
            c.Archetype == archetype &&
            c.Approach == ChoiceApproaches.Desperate);
    }

    private bool HasThirdArchetypeDesperateChoice(List<EncounterChoice> choices)
    {
        ChoiceArchetypes thirdArchetype = GetMissingArchetype(choices);
        return HasDesperateChoice(choices, thirdArchetype);
    }

    private ChoiceArchetypes GetMissingArchetype(List<EncounterChoice> choices)
    {
        // Get all archetypes present in the choices
        HashSet<ChoiceArchetypes> presentArchetypes = new(
            choices.Select(c => c.Archetype));

        // Return the first archetype that isn't present
        return Enum.GetValues<ChoiceArchetypes>()
            .First(a => !presentArchetypes.Contains(a));
    }

    private ChoiceApproaches SelectApproach(
    ChoiceArchetypes archetype,
    EncounterStateValues values,
    PlayerState playerState)
    {
        // Get available approaches based on pressure thresholds
        List<ChoiceApproaches> availableApproaches = GetAvailableApproaches(
            archetype, values, playerState);

        // Filter out already used combinations
        availableApproaches = availableApproaches
            .Where(approach => !usedCombinations.Contains((archetype, approach)))
            .ToList();

        // If no valid approaches, return Desperate as fallback
        if (availableApproaches.Count == 0)
            return ChoiceApproaches.Desperate;

        // Select based on archetype-specific priorities
        return SelectBestApproach(archetype, availableApproaches, values);
    }

    private List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
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

        // Desperate is always available
        approaches.Add(ChoiceApproaches.Desperate);

        return approaches;
    }

    private ChoiceApproaches SelectBestApproach(
        ChoiceArchetypes archetype,
        List<ChoiceApproaches> availableApproaches,
        EncounterStateValues values)
    {
        // Define archetype-specific priority orders
        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);

        // Return the highest priority available approach
        return priorityOrder.First(approach => availableApproaches.Contains(approach));
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

    private bool ShouldAddSpecialChoice(EncounterStateValues values)
    {
        // Add special choice if any mastery value is high enough and pressure isn't too high
        return values.Pressure <= 6 && (
            values.Insight >= 7 ||
            values.Resonance >= 7 ||
            values.Momentum >= 7
        );
    }

    private EncounterChoice TryGenerateSpecialChoice(
        int index,
        ChoiceSetTemplate template,
        EncounterStateValues values)
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
