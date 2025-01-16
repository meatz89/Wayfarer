public class ChoiceSetGenerator
{
    private readonly GameState gameState;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceEffectsGenerator baseValueGenerator;

    public ChoiceSetGenerator(GameState gameState
        )
    {
        this.gameState = gameState;
        this.calculator = new ChoiceCalculator();
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
        // Try Direct if we have energy and the relevant skill
        if (HasSufficientEnergy(archetype, playerState, 2) &&
            HasRelevantSkill(archetype, playerState))
            return ChoiceApproaches.Direct;

        // Try Pragmatic if pressure is low enough
        if (values.Pressure < 5)
            return ChoiceApproaches.Pragmatic;

        // Try Tactical if we meet the special requirement
        if (MeetsTacticalRequirement(archetype, playerState) &&
            HasHighSecondaryValue(archetype, values))
            return ChoiceApproaches.Tactical;

        // Fallback to Improvised
        return ChoiceApproaches.Improvised;
    }

    private bool HasHighSecondaryValue(ChoiceArchetypes archetype, EncounterStateValues values)
    {
        return true;
    }

    private bool MeetsTacticalRequirement(ChoiceArchetypes archetype, PlayerState playerState)
    {
        return true;
    }

    private bool HasRelevantSkill(ChoiceArchetypes archetype, PlayerState playerState)
    {
        return true;
    }

    private bool HasSufficientEnergy(ChoiceArchetypes archetype, PlayerState playerState, int v)
    {
        return true;
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

    private void AddImprovisedChoice(List<EncounterChoice> choices)
    {
        ChoiceArchetypes archetype = GetLeastRepresentedArchetype(choices);
        choices.Add(CreateChoice(choices.Count, archetype, ChoiceApproaches.Improvised));
    }

    private ChoiceArchetypes GetLeastRepresentedArchetype(List<EncounterChoice> choices)
    {
        return ChoiceArchetypes.Physical;
    }
}