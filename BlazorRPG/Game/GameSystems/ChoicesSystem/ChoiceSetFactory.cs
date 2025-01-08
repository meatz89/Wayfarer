public class ChoiceSetFactory
{
    public ChoiceSet CreateFromTemplate(
        ChoiceSetTemplate template,
        EncounterActionContext context)
    {
        // Check if template is valid for this context
        if (!IsTemplateValid(template, context))
            return null;

        // Create base choices from patterns
        List<EncounterChoice> choices = new();
        foreach (ChoicePattern pattern in template.ChoicePatterns)
        {
            EncounterChoice choice = CreateChoiceFromPattern(
                pattern,
                context,
                CalculateModifiers(pattern, context));
            choices.Add(choice);
        }

        return new ChoiceSet(choices);
    }

    private bool IsTemplateValid(ChoiceSetTemplate template, EncounterActionContext context)
    {
        // A choice set is invalid if any of its availability conditions are not met
        bool hasLocationConditions = template.AvailabilityConditions.Any();
        bool locationConditionsMet = hasLocationConditions && template.AvailabilityConditions.All(cond => cond.IsMet(context.LocationProperties));
        if (hasLocationConditions && !locationConditionsMet)
        {
            return false;
        }

        // A choice set is invalid if any of its state conditions are not met
        bool hasStateConditions = template.StateConditions.Any();
        bool stateConditionsMet = hasStateConditions && template.StateConditions.All(cond => cond.IsMet(context.CurrentValues));
        if (hasStateConditions && !stateConditionsMet)
        {
            return false;
        }

        return true;
    }

    private EncounterChoice CreateChoiceFromPattern(
    ChoicePattern pattern,
    EncounterActionContext context,
    ChoiceValueModifiers modifiers)
    {
        // Start with base value changes
        List<ValueChange> finalValueChanges = new(pattern.BaseValueChanges);

        // Apply modifiers
        foreach (ValueChange baseChange in pattern.BaseValueChanges)
        {
            switch (baseChange.ValueType)
            {
                case ValueTypes.Advantage:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Advantage,
                        baseChange.Change + modifiers.AdvantageModifier));
                    break;
                case ValueTypes.Tension:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Tension,
                        baseChange.Change + modifiers.TensionGainModifier));
                    break;
                    // etc for other value types
            }
        }

        // Create the choice using the builder and add requirements, costs, and rewards
        return new ChoiceBuilder()
            .WithChoiceType(pattern.ChoiceType)
            .RequiresEnergy(pattern.EnergyType,
                pattern.BaseCost + modifiers.EnergyCostModifier)
            .WithValueChanges(finalValueChanges)
            .WithRequirements(pattern.Requirements) // Add requirements
            .WithCosts(pattern.Costs) // Add costs
            .WithRewards(pattern.Rewards) // Add rewards
            .Build();
    }

    private ChoiceValueModifiers CalculateModifiers(
        ChoicePattern pattern,
        EncounterActionContext context)
    {
        ChoiceValueModifiers mods = new ChoiceValueModifiers();

        // Apply skill vs difficulty modifier
        mods.AdvantageModifier +=
            context.PlayerState.GetSkillLevel(GetRelevantSkill(context)) -
            context.LocationDifficulty;

        return mods;
    }

    private SkillTypes GetRelevantSkill(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => SkillTypes.Strength,
            BasicActionTypes.Investigate => SkillTypes.Perception,
            BasicActionTypes.Mingle => SkillTypes.Charisma,
            _ => SkillTypes.None
        };
    }
}