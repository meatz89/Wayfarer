public class ChoiceSetGenerator
{
    private readonly ChoiceCalculator calculator;
    private readonly EncounterContext context;

    public ChoiceSetGenerator(EncounterContext context)
    {
        this.context = context;
        this.calculator = new ChoiceCalculator(context.LocationPropertyChoiceEffects);
    }

    public ChoiceSet GenerateChoiceSet(ChoiceSetTemplate template)
    {
        // The template now defines how many choices of each archetype we need
        List<EncounterChoice> choices = GenerateBaseChoices(template);

        // Apply location property effects using our existing calculator
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoice(choice, context);
        }

        // Add improvised choice if we don't have enough valid choices
        while (choices.Count < 4)
        {
            AddImprovisedChoice(choices);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateBaseChoices(ChoiceSetTemplate template)
    {
        List<EncounterChoice> choices = new();
        int index = 0;

        // For each composition pattern, generate the required choices
        foreach (var composition in template.CompositionPatterns)
        {
            // Generate primary archetype choices
            for (int i = 0; i < composition.PrimaryCount; i++)
            {
                ChoiceApproaches approach = SelectBestApproachForArchetype(
                    composition.PrimaryArchetype,
                    context.CurrentValues,
                    context.PlayerState);

                EncounterChoice choice = new ChoiceBuilder()
                    .WithIndex(index++)
                    .WithArchetype(composition.PrimaryArchetype)
                    .WithApproach(approach)
                    .WithBaseValueChanges(GenerateBaseValueChanges(composition.PrimaryArchetype, approach))
                    .WithRequirements(GenerateRequirements(composition.PrimaryArchetype, approach))
                    .Build();

                choices.Add(choice);
            }

            // Generate secondary archetype choices
            for (int i = 0; i < composition.SecondaryCount; i++)
            {
                ChoiceApproaches approach = SelectBestApproachForArchetype(
                    composition.SecondaryArchetype,
                    context.CurrentValues,
                    context.PlayerState);

                EncounterChoice choice = new ChoiceBuilder()
                    .WithIndex(index++)
                    .WithArchetype(composition.SecondaryArchetype)
                    .WithApproach(approach)
                    .WithBaseValueChanges(GenerateBaseValueChanges(composition.SecondaryArchetype, approach))
                    .WithRequirements(GenerateRequirements(composition.SecondaryArchetype, approach))
                    .Build();

                choices.Add(choice);
            }
        }

        return choices;
    }

    private List<Requirement> GenerateRequirements(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<Requirement> requirements = new();

        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Direct requires matching skill and sufficient energy
                requirements.Add(new EnergyRequirement(GetArchetypeEnergy(archetype), 2));
                requirements.Add(GetArchetypeSkillRequirement(archetype));
                break;

            case ChoiceApproaches.Pragmatic:
                // Pragmatic requires low pressure and matching skill
                requirements.Add(new MaxPressureRequirement(5));
                requirements.Add(GetArchetypeSkillRequirement(archetype));
                break;

            case ChoiceApproaches.Tactical:
                // Tactical has type-specific requirements
                requirements.Add(GetTacticalRequirement(archetype));
                break;

            case ChoiceApproaches.Improvised:
                // Improvised has no requirements
                break;
        }

        return requirements;
    }

    private Requirement GetArchetypeSkillRequirement(ChoiceArchetypes archetype) =>
        archetype switch
        {
            ChoiceArchetypes.Physical => new SkillRequirement(SkillTypes.Strength, 1),
            ChoiceArchetypes.Focus => new SkillRequirement(SkillTypes.Perception, 1),
            ChoiceArchetypes.Social => new SkillRequirement(SkillTypes.Charisma, 1),
            _ => throw new ArgumentException("Invalid archetype")
        };

    private Requirement GetTacticalRequirement(ChoiceArchetypes archetype) =>
        archetype switch
        {
            ChoiceArchetypes.Physical => new ItemRequirement(ItemTypes.Tool),
            ChoiceArchetypes.Focus => new KnowledgeRequirement(KnowledgeTypes.LocalHistory),
            ChoiceArchetypes.Social => new ReputationRequirement(ReputationTypes.Reliable, 5),
            _ => throw new ArgumentException("Invalid archetype")
        };

    private EnergyTypes GetArchetypeEnergy(ChoiceArchetypes archetype) =>
        archetype switch
        {
            ChoiceArchetypes.Physical => EnergyTypes.Physical,
            ChoiceArchetypes.Focus => EnergyTypes.Focus,
            ChoiceArchetypes.Social => EnergyTypes.Social,
            _ => throw new ArgumentException("Invalid archetype")
        };

    private List<ValueChange> GenerateBaseValueChanges(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<ValueChange> changes = new();

        // Base outcome change from archetype-approach combination
        int outcomeChange = approach switch
        {
            ChoiceApproaches.Direct => 3,
            ChoiceApproaches.Pragmatic => 2,
            ChoiceApproaches.Tactical => 1,
            ChoiceApproaches.Improvised => 1,
            _ => throw new ArgumentException("Invalid approach")
        };
        changes.Add(new ValueChange(ValueTypes.Outcome, outcomeChange));

        // Pressure change from approach
        int pressureChange = approach switch
        {
            ChoiceApproaches.Direct => 1,
            ChoiceApproaches.Pragmatic => 0,
            ChoiceApproaches.Tactical => 0,
            ChoiceApproaches.Improvised => 2,
            _ => throw new ArgumentException("Invalid approach")
        };
        changes.Add(new ValueChange(ValueTypes.Pressure, pressureChange));

        // Secondary value based on archetype
        switch (archetype)
        {
            case ChoiceArchetypes.Focus:
                int insightChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new ValueChange(ValueTypes.Insight, insightChange));
                break;
            case ChoiceArchetypes.Social:
                int resonanceChange = approach == ChoiceApproaches.Tactical ? 2 : 1;
                changes.Add(new ValueChange(ValueTypes.Resonance, resonanceChange));
                break;
        }

        return changes;
    }

    private ChoiceApproaches SelectBestApproachForArchetype(
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

    private bool MeetsTacticalRequirement(ChoiceArchetypes archetype, PlayerState playerState)
    {
        return true;
    }

    private bool HasHighSecondaryValue(ChoiceArchetypes archetype, EncounterStateValues values)
    {
        return archetype switch
        {
            ChoiceArchetypes.Focus => values.Insight >= 5,
            ChoiceArchetypes.Social => values.Resonance >= 5,
            _ => true
        };
    }

    private void AddImprovisedChoice(List<EncounterChoice> choices)
    {
        ChoiceArchetypes archetype = DetermineImprovisedArchetype(choices);

        // Create improvised choice using the builder
        EncounterChoice improvisedChoice = new ChoiceBuilder()
            .WithIndex(choices.Count)
            .WithArchetype(archetype)
            .WithApproach(ChoiceApproaches.Improvised)
            .WithDescription(GenerateDescription(archetype, ChoiceApproaches.Improvised))
            .Build();

        choices.Add(improvisedChoice);
    }

    private bool HasSufficientEnergy(ChoiceArchetypes archetype, PlayerState playerState, int amount)
    {
        // Check if player has enough energy based on archetype
        return archetype switch
        {
            ChoiceArchetypes.Physical => playerState.PhysicalEnergy >= amount,
            ChoiceArchetypes.Focus => playerState.FocusEnergy >= amount,
            ChoiceArchetypes.Social => playerState.SocialEnergy >= amount,
            _ => false
        };
    }

    private bool HasRelevantSkill(ChoiceArchetypes archetype, PlayerState playerState)
    {
        // Check if player has the re energy based on archetype
        return archetype switch
        {
            ChoiceArchetypes.Physical => playerState.GetSkillLevel(SkillTypes.Strength) > 0,
            ChoiceArchetypes.Focus => playerState.GetSkillLevel(SkillTypes.Perception) > 0,
            ChoiceArchetypes.Social => playerState.GetSkillLevel(SkillTypes.Charisma) > 0,
            _ => false
        };
    }

    private ChoiceArchetypes DetermineImprovisedArchetype(List<EncounterChoice> existingChoices)
    {
        // Get counts of each archetype
        int physicalCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Physical);
        int focusCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Focus);
        int socialCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Social);

        // Determine primary archetype for current action type
        ChoiceArchetypes primaryArchetype = GetPrimaryArchetypeForAction(context.ActionType);

        // If primary archetype isn't overrepresented, use it
        int primaryCount = primaryArchetype switch
        {
            ChoiceArchetypes.Physical => physicalCount,
            ChoiceArchetypes.Focus => focusCount,
            ChoiceArchetypes.Social => socialCount,
            _ => throw new ArgumentException("Invalid archetype")
        };

        if (primaryCount < 2)
        {
            return primaryArchetype;
        }

        // Otherwise pick the least represented archetype
        if (physicalCount <= focusCount && physicalCount <= socialCount)
            return ChoiceArchetypes.Physical;
        if (focusCount <= physicalCount && focusCount <= socialCount)
            return ChoiceArchetypes.Focus;
        return ChoiceArchetypes.Social;
    }

    private ChoiceArchetypes GetPrimaryArchetypeForAction(BasicActionTypes actionType)
    {
        return actionType switch
        {
            BasicActionTypes.Labor => ChoiceArchetypes.Physical,
            BasicActionTypes.Gather => ChoiceArchetypes.Physical,
            BasicActionTypes.Travel => ChoiceArchetypes.Physical,

            BasicActionTypes.Investigate => ChoiceArchetypes.Focus,
            BasicActionTypes.Study => ChoiceArchetypes.Focus,
            BasicActionTypes.Reflect => ChoiceArchetypes.Focus,

            BasicActionTypes.Mingle => ChoiceArchetypes.Social,
            BasicActionTypes.Persuade => ChoiceArchetypes.Social,
            BasicActionTypes.Perform => ChoiceArchetypes.Social,

            _ => ChoiceArchetypes.Physical // Default fallback
        };
    }


    private string GenerateDescription(ChoiceArchetypes choiceArchetype, ChoiceApproaches choiceApproach)
    {
        return $"{choiceArchetype} - {choiceApproach}";
    }
}