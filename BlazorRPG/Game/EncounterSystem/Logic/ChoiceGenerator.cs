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

        // Handle high pressure scenario first
        if (values.Pressure >= 9)
        {
            return GenerateDesperateOnlyChoices(pattern.PrimaryArchetype);
        }

        // Generate primary archetype choices (2 choices)
        for (int i = 0; i < 2; i++)
        {
            ChoiceApproaches approach = SelectApproach(pattern.PrimaryArchetype, values, playerState);
            choices.Add(CreateChoice(choices.Count, pattern.PrimaryArchetype, approach));
        }

        // Generate secondary archetype choice (1 choice)
        ChoiceApproaches secondaryApproach = SelectApproach(pattern.SecondaryArchetype, values, playerState);
        choices.Add(CreateChoice(choices.Count, pattern.SecondaryArchetype, secondaryApproach));

        // Add special choice if applicable (high mastery value)
        if (ShouldAddSpecialChoice(values))
        {
            choices.Add(CreateSpecialChoice(choices.Count, template, values));
        }

        return choices;
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
        List<ChoiceApproaches> priorityOrder = GetPriorityOrder(archetype, values);

        // Return the highest priority available approach
        return priorityOrder.First(approach => availableApproaches.Contains(approach));
    }

    private List<ChoiceApproaches> GetPriorityOrder(
        ChoiceArchetypes archetype,
        EncounterStateValues values)
    {
        if (values.Pressure >= 7)
        {
            // High pressure priority order is the same for all archetypes
            return new List<ChoiceApproaches>
        {
            ChoiceApproaches.Careful,
            ChoiceApproaches.Desperate
        };
        }

        // Normal pressure priority orders vary by archetype
        return archetype switch
        {
            ChoiceArchetypes.Physical => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Aggressive,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Desperate
        },

            ChoiceArchetypes.Focus => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Aggressive,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Desperate
        },

            ChoiceArchetypes.Social => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Careful,
            ChoiceApproaches.Aggressive,
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Desperate
        },

            _ => throw new ArgumentException("Invalid archetype")
        };
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

    private EncounterChoice CreateSpecialChoice(
        int index,
        ChoiceSetTemplate template,
        EncounterStateValues values)
    {
        // Determine which mastery value enables this special choice
        ChoiceArchetypes specialArchetype;
        if (values.Insight >= 7) specialArchetype = ChoiceArchetypes.Focus;
        else if (values.Resonance >= 7) specialArchetype = ChoiceArchetypes.Social;
        else specialArchetype = ChoiceArchetypes.Physical;

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
