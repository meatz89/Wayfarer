using System.Security.AccessControl;

public class ChoiceSetGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceEffectsGenerator baseValueGenerator;
    private readonly HashSet<(ChoiceArchetypes, ChoiceApproaches)> usedCombinations;

    public ChoiceSetGenerator(GameState gameState
        )
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.baseValueGenerator = new ChoiceEffectsGenerator();
        this.usedCombinations = new HashSet<(ChoiceArchetypes, ChoiceApproaches)>();
    }

    public ChoiceSet Generate(ChoiceSetTemplate template, EncounterContext context)
    {
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

        List<ChoiceArchetypes> unusedChoiceChoiceArchetypes = new List<ChoiceArchetypes>() {
            ChoiceArchetypes.Physical,
            ChoiceArchetypes.Focus,
            ChoiceArchetypes.Social
        };
        foreach (CompositionPattern pattern in template.CompositionPatterns)
        {
            // Generate primary choices
            for (int i = 0; i < pattern.PrimaryCount; i++)
            {
                ChoiceApproaches approach = SelectApproach(pattern.PrimaryArchetype, values, playerState);
                choices.Add(CreateChoice(index++, pattern.PrimaryArchetype, approach));

                if(unusedChoiceChoiceArchetypes.Contains(pattern.PrimaryArchetype))
                {
                    unusedChoiceChoiceArchetypes.Remove(pattern.PrimaryArchetype);
                }
            }

            // Generate secondary choices
            for (int i = 0; i < pattern.SecondaryCount; i++)
            {
                ChoiceApproaches approach = SelectApproach(pattern.SecondaryArchetype, values, playerState);
                choices.Add(CreateChoice(index++, pattern.SecondaryArchetype, approach));
                
                if (unusedChoiceChoiceArchetypes.Contains(pattern.SecondaryArchetype))
                {
                    unusedChoiceChoiceArchetypes.Remove(pattern.SecondaryArchetype);
                }
            }

            // Generate unused archetype choices
            foreach (ChoiceArchetypes choiceArchetype in unusedChoiceChoiceArchetypes)
            {
                ChoiceApproaches approach = SelectApproach(choiceArchetype, values, playerState);
                choices.Add(CreateChoice(index++, choiceArchetype, approach));
            }
        }

        // Ensure fallback choice exists
        if (!choices.Any(c => c.Approach == ChoiceApproaches.Improvised))
        {
            ChoiceArchetypes archetype = GetLeastRepresentedArchetype(choices);
            ChoiceApproaches approach = ChoiceApproaches.Improvised;
            if (!usedCombinations.Contains((archetype, approach)))
            {
                choices.Add(CreateImprovisedChoice(choices.Count, archetype));
                usedCombinations.Add((archetype, approach));
            }
        }

        return choices;
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
            return ChoiceApproaches.Improvised;

        // Prioritize approaches based on current state
        return SelectBestApproach(validApproaches, archetype, values, playerState);
    }

    private List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        List<ChoiceApproaches> approaches = new();

        // Check Direct approach availability
        if (IsDirectApproachAvailable(archetype, values, playerState))
            approaches.Add(ChoiceApproaches.Direct);

        // Check Pragmatic approach availability
        if (IsPragmaticApproachAvailable(archetype, values, playerState))
            approaches.Add(ChoiceApproaches.Pragmatic);

        // Check Tactical approach availability
        if (IsTacticalApproachAvailable(archetype, values, playerState))
            approaches.Add(ChoiceApproaches.Tactical);

        // Improvised is always available as fallback
        approaches.Add(ChoiceApproaches.Improvised);

        return approaches;
    }

    private bool IsDirectApproachAvailable(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Must have sufficient energy for direct approach
        if (!HasSufficientEnergy(archetype, playerState, 3))
            return false;

        // Archetype-specific direct approach requirements
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                // Physical Direct needs sufficient momentum to overcome pressure
                return values.Momentum >= values.Pressure;

            case ChoiceArchetypes.Focus:
                // Focus Direct needs sufficient insight for breakthrough
                return values.Insight >= values.Pressure;

            case ChoiceArchetypes.Social:
                // Social Direct needs sufficient resonance for leverage
                return values.Resonance >= values.Pressure;

            default:
                return false;
        }
    }

    private bool IsPragmaticApproachAvailable(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Pragmatic requires moderate energy and low pressure
        if (!HasSufficientEnergy(archetype, playerState, 2))
            return false;

        if (values.Pressure >= 5)
            return false;

        // Archetype-specific pragmatic requirements
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                // Need some momentum to maintain flow
                return values.Momentum >= 2;

            case ChoiceArchetypes.Focus:
                // Need base insight to build upon
                return values.Insight >= 2;

            case ChoiceArchetypes.Social:
                // Need base resonance to leverage
                return values.Resonance >= 2;

            default:
                return false;
        }
    }

    private bool IsTacticalApproachAvailable(
        ChoiceArchetypes archetype,
        EncounterStateValues values,
        PlayerState playerState)
    {
        // Tactical requires low energy cost but high secondary values
        if (!HasSufficientEnergy(archetype, playerState, 1))
            return false;

        // Check archetype-specific tactical requirements
        switch (archetype)
        {
            case ChoiceArchetypes.Physical:
                // Need high momentum to spend effectively
                return values.Momentum >= 4 && HasTool(playerState);

            case ChoiceArchetypes.Focus:
                // Need high insight to convert
                return values.Insight >= 4 && HasRelevantKnowledge(playerState);

            case ChoiceArchetypes.Social:
                // Need high resonance to leverage
                return values.Resonance >= 4 && HasSufficientReputation(playerState);

            default:
                return false;
        }
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
            if (availableApproaches.Contains(ChoiceApproaches.Tactical))
                return ChoiceApproaches.Tactical;
            if (availableApproaches.Contains(ChoiceApproaches.Pragmatic))
                return ChoiceApproaches.Pragmatic;
        }
        else if (NeedsOutcome(values))
        {
            // Need outcome - prioritize direct conversion
            if (availableApproaches.Contains(ChoiceApproaches.Direct))
                return ChoiceApproaches.Direct;
            if (availableApproaches.Contains(ChoiceApproaches.Tactical))
                return ChoiceApproaches.Tactical;
        }
        else
        {
            // Building up values - prefer pragmatic
            if (availableApproaches.Contains(ChoiceApproaches.Pragmatic))
                return ChoiceApproaches.Pragmatic;
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
        ChoiceApproaches approach = ChoiceApproaches.Improvised;
        EncounterChoice choice = new(
            index,
            $"{archetype} - {approach}",
            archetype,
            approach,
            false,
            false,
            false);

        // Set base values using generator
        choice.BaseEncounterValueChanges = baseValueGenerator.GenerateBaseValueChanges(archetype, approach);

        return choice;
    }


    private EncounterChoice CreateChoice(int index, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        bool requireTool = archetype == ChoiceArchetypes.Physical && approach == ChoiceApproaches.Tactical;
        bool requireKnowledge = archetype == ChoiceArchetypes.Focus && approach == ChoiceApproaches.Tactical;
        bool requireReputation = archetype == ChoiceArchetypes.Social && approach == ChoiceApproaches.Tactical;

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


    private bool HasSufficientEnergy(ChoiceArchetypes archetype, PlayerState playerState, int v)
    {
        return true;
    }

    private bool HasSufficientReputation(PlayerState playerState)
    {
        return true;
    }

    private bool HasRelevantKnowledge(PlayerState playerState)
    {
        return true;
    }

    private bool HasTool(PlayerState playerState)
    {
        return true;
    }
}