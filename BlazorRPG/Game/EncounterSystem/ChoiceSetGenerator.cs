public class ChoiceSetGenerator
{
    private readonly ChoiceCalculator calculator;
    private readonly EncounterContext context;
    private readonly bool debugMode;

    public ChoiceSetGenerator(EncounterContext context)
    {
        this.debugMode = true;

        this.context = context;
        this.calculator = new ChoiceCalculator(context.LocationPropertyChoiceEffects);
    }

    public ChoiceSet Generate(ChoiceSetTemplate template)
    {
        // First, analyze the current encounter state
        EncounterStateAnalysis analysis = EncounterStateAnalysis.Analyze(
            context.CurrentValues,
            context.StageNumber,
            context.PlayerState);

        // Generate choices based on debug mode
        List<EncounterChoice> choices = debugMode ?
            GenerateAllChoiceCombinations() :
            GenerateStandardChoiceSet(template, analysis);

        // Apply location and state effects to each choice
        foreach (EncounterChoice choice in choices)
        {
            // First apply base calculations from location
            calculator.CalculateChoice(choice, context);

            // Then apply state-specific modifications
            ModifyChoiceForState(choice, analysis.StateCategory);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateStandardChoiceSet(
        ChoiceSetTemplate template,
        EncounterStateAnalysis analysis)
    {
        List<EncounterChoice> choices = new();
        int index = 0;

        // Generate choices based on the template's composition patterns
        foreach (var composition in template.CompositionPatterns)
        {
            // Generate primary archetype choices
            for (int i = 0; i < composition.PrimaryCount; i++)
            {
                ChoiceApproaches approach = SelectBestApproachForArchetype(
                    composition.PrimaryArchetype,
                    context.CurrentValues,
                    context.PlayerState);

                choices.Add(CreateChoice(
                    index++,
                    composition.PrimaryArchetype,
                    approach,
                    analysis));
            }

            // Generate secondary archetype choices
            for (int i = 0; i < composition.SecondaryCount; i++)
            {
                ChoiceApproaches approach = SelectBestApproachForArchetype(
                    composition.SecondaryArchetype,
                    context.CurrentValues,
                    context.PlayerState);

                choices.Add(CreateChoice(
                    index++,
                    composition.SecondaryArchetype,
                    approach,
                    analysis));
            }
        }

        // Always ensure we have a fallback option
        if (!choices.Any(c => c.Approach == ChoiceApproaches.Improvised))
        {
            AddImprovisedChoice(choices);
        }

        return choices;
    }

    private void ModifyChoiceForState(EncounterChoice choice, EncounterStateCategory state)
    {
        // Early exit for improvised choices - they don't get modified by state
        if (choice.Approach == ChoiceApproaches.Improvised)
            return;

        switch (state)
        {
            case EncounterStateCategory.Critical:
                ModifyChoiceForCriticalState(choice);
                break;
            case EncounterStateCategory.Escalation:
                ModifyChoiceForEscalationState(choice);
                break;
        }
    }

    private void ModifyChoiceForCriticalState(EncounterChoice choice)
    {
        // In critical state, we want to emphasize risk/reward trade-offs
        if (choice.Approach == ChoiceApproaches.Direct)
        {
            // Direct approaches become more extreme
            choice.ModifiedValueChanges.Add(new ValueChange(ValueTypes.Outcome, 2));
            choice.ModifiedValueChanges.Add(new ValueChange(ValueTypes.Pressure, 2));
        }
        else if (choice.Approach == ChoiceApproaches.Pragmatic)
        {
            // Pragmatic approaches get better at managing pressure
            choice.ModifiedValueChanges.Add(new ValueChange(ValueTypes.Pressure, -2));
        }
    }

    private void ModifyChoiceForEscalationState(EncounterChoice choice)
    {
        // In escalation state, we want to enable faster progress
        if (choice.Approach == ChoiceApproaches.Direct)
        {
            // Direct approaches get better at gaining outcome
            choice.ModifiedValueChanges.Add(new ValueChange(ValueTypes.Outcome, 4));
        }
        else if (choice.Approach == ChoiceApproaches.Tactical)
        {
            // Tactical approaches can convert their specialty into outcome
            bool hasInsight = choice.ModifiedValueChanges
                .Any(v => v.ValueType == ValueTypes.Insight);
            bool hasResonance = choice.ModifiedValueChanges
                .Any(v => v.ValueType == ValueTypes.Resonance);

            if (hasInsight || hasResonance)
            {
                choice.ModifiedValueChanges.Add(new ValueChange(ValueTypes.Outcome, 2));
            }
        }
    }

    private List<EncounterChoice> GenerateAllChoiceCombinations()
    {
        List<EncounterChoice> choices = new();
        int index = 1;

        // Generate a choice for every archetype-approach combination
        foreach (ChoiceArchetypes archetype in Enum.GetValues(typeof(ChoiceArchetypes)))
        {
            foreach (ChoiceApproaches approach in Enum.GetValues(typeof(ChoiceApproaches)))
            {
                EncounterChoice choice = new ChoiceBuilder()
                    .WithIndex(index++)
                    .WithArchetype(archetype)
                    .WithApproach(approach)
                    .WithBaseValueChanges(GenerateBaseValueChanges(archetype, approach))
                    .WithRequirements(GenerateRequirements(archetype, approach))
                    .Build();

                choices.Add(choice);
            }
        }

        return choices;
    }

    private List<ValueChange> GenerateBaseValueChanges(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        List<ValueChange> changes = new();

        // Base values are now doubled
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                changes.Add(new ValueChange(ValueTypes.Outcome, 4)); 
                break;
            case ChoiceApproaches.Pragmatic:
                changes.Add(new ValueChange(ValueTypes.Outcome, 2)); 
                break;
            case ChoiceApproaches.Tactical:
                // No immediate outcome gain
                changes.Add(new ValueChange(ValueTypes.Pressure, -2)); 
                break;
            case ChoiceApproaches.Improvised:
                changes.Add(new ValueChange(ValueTypes.Outcome, 2)); 
                changes.Add(new ValueChange(ValueTypes.Pressure, 4)); 
                break;
        }

        // Archetype-specific secondary value changes
        switch (archetype)
        {
            case ChoiceArchetypes.Focus:
                int insightChange = approach == ChoiceApproaches.Tactical ? 4 : 2; 
                changes.Add(new ValueChange(ValueTypes.Insight, insightChange));
                break;
            case ChoiceArchetypes.Social:
                int resonanceChange = approach == ChoiceApproaches.Tactical ? 4 : 2;
                changes.Add(new ValueChange(ValueTypes.Resonance, resonanceChange));
                break;
        }

        return changes;
    }

    private List<ValueChange> ModifyValuesForCriticalState(List<ValueChange> baseChanges, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // In critical states (high pressure), we want to:
        // 1. Make direct approaches more impactful but riskier
        // 2. Make pragmatic/tactical approaches better at managing pressure
        // 3. Make improvised choices even riskier but more rewarding

        foreach (ValueChange change in baseChanges)
        {
            // Modify existing values based on the approach
            switch (approach)
            {
                case ChoiceApproaches.Direct:
                    // Direct approaches become high-risk, high-reward
                    if (change.ValueType == ValueTypes.Outcome)
                    {
                        change.Change += 2; // More outcome
                    }
                    // Add pressure cost to direct approaches
                    if (change.ValueType == ValueTypes.Pressure)
                    {
                        baseChanges.Add(new ValueChange(ValueTypes.Pressure, 2));
                    }
                    break;

                case ChoiceApproaches.Pragmatic:
                    // Pragmatic approaches gain pressure reduction
                    if (change.ValueType == ValueTypes.Pressure)
                    {
                        baseChanges.Add(new ValueChange(ValueTypes.Pressure, -2));
                    }
                    break;

                case ChoiceApproaches.Tactical:
                    // Tactical approaches get better at their specialties
                    if (change.ValueType == ValueTypes.Insight && archetype == ChoiceArchetypes.Focus)
                    {
                        change.Change += 2; // Better insight gain
                    }
                    if (change.ValueType == ValueTypes.Resonance && archetype == ChoiceArchetypes.Social)
                    {
                        change.Change += 2; // Better resonance gain
                    }
                    break;

                case ChoiceApproaches.Improvised:
                    // Improvised becomes very risky but potentially game-changing
                    if (change.ValueType == ValueTypes.Outcome)
                    {
                        change.Change += 4; // Much more outcome
                    }
                    if (change.ValueType == ValueTypes.Pressure)
                    {
                        change.Change += 2; // But also more pressure
                    }
                    break;
            }
        }

        return baseChanges;
    }

    private List<ValueChange> ModifyValuesForEscalationState(List<ValueChange> baseChanges, ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // In escalation states (low progress), we want to:
        // 1. Make all approaches provide more outcome
        // 2. Allow converting secondary values (insight/resonance) into outcome
        // 3. Make pressure costs relatively lower to encourage aggressive play

        foreach (ValueChange change in baseChanges)
        {
            switch (approach)
            {
                case ChoiceApproaches.Direct:
                    // Direct approaches get even better at gaining outcome
                    if (change.ValueType == ValueTypes.Outcome)
                    {
                        change.Change += 4;
                    }
                    break;

                case ChoiceApproaches.Tactical:
                    // Tactical approaches can convert their specialty into outcome
                    if (change.ValueType == ValueTypes.Insight && archetype == ChoiceArchetypes.Focus)
                    {
                        baseChanges.Add(new ValueChange(ValueTypes.Outcome, change.Change / 2));
                    }
                    if (change.ValueType == ValueTypes.Resonance && archetype == ChoiceArchetypes.Social)
                    {
                        baseChanges.Add(new ValueChange(ValueTypes.Outcome, change.Change / 2));
                    }
                    break;

                case ChoiceApproaches.Improvised:
                    // Improvised gets better outcome but keeps same pressure
                    if (change.ValueType == ValueTypes.Outcome)
                    {
                        change.Change += 2;
                    }
                    break;
            }
        }

        return baseChanges;
    }

    private EncounterChoice CreateChoice(int index, ChoiceArchetypes archetype, ChoiceApproaches approach, EncounterStateAnalysis analysis)
    {
        // Start with base value changes
        List<ValueChange> changes = GenerateBaseValueChanges(archetype, approach);

        // Modify based on state if needed
        changes = analysis.StateCategory switch
        {
            EncounterStateCategory.Critical => ModifyValuesForCriticalState(changes, archetype, approach),
            EncounterStateCategory.Escalation => ModifyValuesForEscalationState(changes, archetype, approach),
            _ => changes
        };

        // Create the choice with our modified values
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithArchetype(archetype)
            .WithApproach(approach)
            .WithBaseValueChanges(changes)
            .WithRequirements(GenerateRequirements(archetype, approach))
            .Build();
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

}