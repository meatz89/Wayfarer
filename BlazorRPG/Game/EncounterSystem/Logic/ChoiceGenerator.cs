public class ChoiceGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceEffectsCalculator calculator;
    private readonly BaseValueChangeGenerator baseValueGenerator;
    private readonly HashSet<(ChoiceArchetypes, ChoiceApproaches)> usedCombinations;

    public ChoiceGenerator(GameState gameState)
    {
        this.gameState = gameState;
        this.calculator = new ChoiceEffectsCalculator(gameState);
        this.baseValueGenerator = new BaseValueChangeGenerator();
        this.usedCombinations = new HashSet<(ChoiceArchetypes, ChoiceApproaches)>();
    }
    private List<EncounterChoice> GenerateBaseChoices(
    ChoiceSetTemplate template,
    EncounterValues values,
    PlayerState playerState,
    int desiredChoiceCount)  // New parameter
    {
        List<EncounterChoice> choices = new();
        CompositionPattern pattern = template.CompositionPattern;

        if (values.Pressure >= 9)
        {
            return GenerateDesperateOnlyChoices(pattern.PrimaryArchetype);
        }

        // First third of choices: Core approach establishers
        int coreChoiceCount = Math.Max(1, desiredChoiceCount / 3);
        for (int i = 0; i < coreChoiceCount && choices.Count < desiredChoiceCount; i++)
        {
            EncounterChoice choice = null;
            if (i == 0)
            {
                // First choice always follows original first choice rules
                choice = GenerateFirstChoice(pattern.PrimaryArchetype, values, playerState);
            }
            else
            {
                // Additional core choices use alternate archetypes with non-Strategic approaches
                ChoiceArchetypes archetype = i % 2 == 0 ? pattern.PrimaryArchetype : pattern.SecondaryArchetype;
                choice = GenerateAlternateChoice(archetype, choices, values, playerState);
            }
            if (choice != null) choices.Add(choice);
        }

        // Middle third: Strategic choices
        int strategicChoiceCount = Math.Max(1, desiredChoiceCount / 3);
        for (int i = 0; i < strategicChoiceCount && choices.Count < desiredChoiceCount; i++)
        {
            ChoiceArchetypes[] archetypeOrder = GameRules.GetStrategicArchetypeOrder(pattern);
            foreach (ChoiceArchetypes archetype in archetypeOrder)
            {
                if (IsChoicePossible(archetype, ChoiceApproaches.Strategic, values, playerState)
                    && !HasArchetypeWithApproach(choices, archetype, ChoiceApproaches.Strategic))
                {
                    choices.Add(CreateChoice(choices.Count + 1, archetype, ChoiceApproaches.Strategic));
                    break;
                }
            }
        }

        // Final third: Flexible choices
        while (choices.Count < desiredChoiceCount)
        {
            EncounterChoice choice = GenerateFlexibleChoice(pattern, choices, values, playerState);
            if (choice == null) break; // No more possible choices
            choices.Add(choice);
        }

        return choices;
    }

    private bool HasArchetypeWithApproach(
        List<EncounterChoice> choices,
        ChoiceArchetypes archetype,
        ChoiceApproaches approach)
    {
        return choices.Any(c => c.Archetype == archetype && c.Approach == approach);
    }

    private EncounterChoice GenerateAlternateChoice(
        ChoiceArchetypes archetype,
        List<EncounterChoice> existingChoices,
        EncounterValues values,
        PlayerState playerState)
    {
        List<ChoiceApproaches> availableApproaches = GetAvailableApproaches(archetype, values, playerState)
            .Where(a => a != ChoiceApproaches.Strategic
                   && !existingChoices.Any(c => c.Archetype == archetype && c.Approach == a))
            .ToList();

        if (!availableApproaches.Any()) return null;

        return CreateChoice(
            existingChoices.Count + 1,
            archetype,
            SelectBestApproach(archetype, availableApproaches, values, playerState));
    }

    private EncounterChoice GenerateFlexibleChoice(
        CompositionPattern pattern,
        List<EncounterChoice> existingChoices,
        EncounterValues values,
        PlayerState playerState)
    {
        // Try each archetype-approach combination not yet used
        foreach (ChoiceArchetypes archetype in GameRules.GetArchetypePriority(pattern))
        {
            foreach (ChoiceApproaches approach in GameRules.GetPriorityOrder(archetype, values))
            {
                if (IsChoicePossible(archetype, approach, values, playerState)
                    && !existingChoices.Any(c => c.Archetype == archetype && c.Approach == approach))
                {
                    return CreateChoice(existingChoices.Count + 1, archetype, approach);
                }
            }
        }
        return null;
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
