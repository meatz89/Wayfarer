
public class ChoiceSetGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceEffectsGenerator baseValueGenerator;

    public ChoiceSetGenerator(GameState gameState
        )
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator(gameState);
        this.baseValueGenerator = new ChoiceEffectsGenerator();
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
        if (!choices.Any(c => c.Approach == ChoiceApproaches.Improvised))
        {
            AddImprovisedChoice(choices);
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

        // If no valid approaches, return Improvised as fallback
        if (availableApproaches.Count == 0)
            return ChoiceApproaches.Improvised;

        // Prioritize approaches based on current state
        return SelectBestApproach(availableApproaches, archetype, values, playerState);
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

    private void AddImprovisedChoice(List<EncounterChoice> choices)
    {
        ChoiceArchetypes archetype = GetLeastRepresentedArchetype(choices);
        choices.Add(CreateChoice(choices.Count, archetype, ChoiceApproaches.Improvised));
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

        return choice;
    }

    private ChoiceArchetypes GetLeastRepresentedArchetype(List<EncounterChoice> choices)
    {
        return ChoiceArchetypes.Physical;
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