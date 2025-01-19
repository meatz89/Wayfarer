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
        int index = 0;

        if (values.Pressure >= 9)
        {
            foreach (ChoiceArchetypes archetype in Enum.GetValues(typeof(ChoiceArchetypes)))
            {
                choices.Add(CreateChoice(
                    choices.Count,
                    archetype,
                    ChoiceApproaches.Desperate));
            }
            return choices;
        }

        foreach (CompositionPattern pattern in template.CompositionPatterns)
        {
            // Generate primary choices
            for (int i = 0; i < pattern.PrimaryCount; i++)
            {
                ChoiceApproaches approach = SelectApproach(pattern.PrimaryArchetype, values, playerState);
                choices.Add(CreateChoice(index++, pattern.PrimaryArchetype, approach));

            }

            // Generate secondary choices
            for (int i = 0; i < pattern.SecondaryCount; i++)
            {
                ChoiceApproaches approach = SelectApproach(pattern.SecondaryArchetype, values, playerState);
                choices.Add(CreateChoice(index++, pattern.SecondaryArchetype, approach));

            }
        }

        // Ensure fallback choice exists
        if (!choices.Any(c => c.Approach == ChoiceApproaches.Desperate))
        {
            ChoiceArchetypes archetype = GetLeastRepresentedArchetype(choices);
            ChoiceApproaches approach = ChoiceApproaches.Desperate;
            if (!usedCombinations.Contains((archetype, approach)))
            {
                choices.Add(CreateImprovisedChoice(choices.Count, archetype));
                usedCombinations.Add((archetype, approach));
            }
        }

        return choices;
    }

    private bool IsApproachAvailable(
    ChoiceApproaches approach,
    EncounterStateValues values)
    {
        switch (approach)
        {
            case ChoiceApproaches.Aggressive:
                return values.Pressure < 7; // Too risky at high pressure

            case ChoiceApproaches.Careful:
                return values.Pressure < 9; // Still possible under pressure

            case ChoiceApproaches.Strategic:
                return values.Pressure < 7; // Requires calm situation

            case ChoiceApproaches.Desperate:
                return true; // Always available

            default:
                return false;
        }
    }

    private List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        List<ChoiceApproaches> approaches = new();

        // Check Direct approach availability
        if (IsApproachAvailable(ChoiceApproaches.Aggressive, values))
            approaches.Add(ChoiceApproaches.Aggressive);

        // Check Pragmatic approach availability
        if (IsApproachAvailable(ChoiceApproaches.Careful, values))
            approaches.Add(ChoiceApproaches.Careful);

        // Check Tactical approach availability
        if (IsApproachAvailable(ChoiceApproaches.Strategic, values))
            approaches.Add(ChoiceApproaches.Strategic);

        if (IsApproachAvailable(ChoiceApproaches.Desperate, values))
            approaches.Add(ChoiceApproaches.Strategic);

        return approaches;
    }


    private ChoiceApproaches SelectApproach(
            ChoiceArchetypes archetype,
            EncounterStateValues values,
            PlayerState playerState)
    {
        // Check approach availability based on archetype and current values
        List<ChoiceApproaches> availableApproaches = GetAvailableApproaches(
            archetype, values, playerState);

        // Filter out approaches that have already been used with the current archetype
        List<ChoiceApproaches> validApproaches = new List<ChoiceApproaches>();
        foreach (ChoiceApproaches approach in availableApproaches)
        {
            if (!usedCombinations.Contains((archetype, approach)))
            {
                validApproaches.Add(approach);
            }
        }

        // If no valid approaches, return Improvised as fallback
        if (validApproaches.Count == 0)
            return ChoiceApproaches.Desperate;

        // Prioritize approaches based on current state
        return SelectBestApproach(validApproaches, archetype, values, playerState);
    }

    private ChoiceApproaches SelectBestApproach(
        List<ChoiceApproaches> availableApproaches,
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Prioritize based on current state
        if (values.Pressure >= 6)
        {
            // High pressure - prioritize pressure reduction
            if (availableApproaches.Contains(ChoiceApproaches.Strategic))
                return ChoiceApproaches.Strategic;
            if (availableApproaches.Contains(ChoiceApproaches.Careful))
                return ChoiceApproaches.Careful;
        }
        else if (NeedsOutcome(values))
        {
            // Need outcome - prioritize direct conversion
            if (availableApproaches.Contains(ChoiceApproaches.Aggressive))
                return ChoiceApproaches.Aggressive;
            if (availableApproaches.Contains(ChoiceApproaches.Strategic))
                return ChoiceApproaches.Strategic;
        }
        else
        {
            // Building up values - prefer pragmatic
            if (availableApproaches.Contains(ChoiceApproaches.Careful))
                return ChoiceApproaches.Careful;
        }

        // Return first available as fallback
        return availableApproaches[0];
    }

    private bool NeedsOutcome(EncounterStateValues values)
    {
        // Consider pushing for outcome if we're not making sufficient progress
        return values.Outcome < (10 - values.Pressure);
    }

    private EncounterChoice CreateImprovisedChoice(int index, ChoiceArchetypes archetype)
    {
        ChoiceApproaches approach = ChoiceApproaches.Desperate;
        EncounterChoice choice = CreateChoice(index, archetype, approach);

        // Set base values using generator
        choice.BaseEncounterValueChanges = baseValueGenerator.GenerateBaseValueChanges(archetype, approach);

        return choice;
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

    private ChoiceArchetypes GetLeastRepresentedArchetype(List<EncounterChoice> choices)
    {
        // Count the occurrences of each archetype in the list of choices
        Dictionary<ChoiceArchetypes, int> archetypeCounts = new Dictionary<ChoiceArchetypes, int>();
        foreach (ChoiceArchetypes archetype in Enum.GetValues(typeof(ChoiceArchetypes)))
        {
            archetypeCounts[archetype] = 0;
        }

        foreach (EncounterChoice choice in choices)
        {
            archetypeCounts[choice.Archetype]++;
        }

        // Find the archetype with the minimum count
        ChoiceArchetypes leastRepresentedArchetype = ChoiceArchetypes.Physical; // Default
        int minCount = int.MaxValue;

        foreach (KeyValuePair<ChoiceArchetypes, int> pair in archetypeCounts)
        {
            if (pair.Value < minCount)
            {
                minCount = pair.Value;
                leastRepresentedArchetype = pair.Key;
            }
        }

        return leastRepresentedArchetype;
    }

}
