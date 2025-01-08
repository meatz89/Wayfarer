
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
                case ValueTypes.Outcome:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Outcome,
                        baseChange.Change + modifiers.OutcomeModifier));
                    break;
                case ValueTypes.Pressure:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Pressure,
                        baseChange.Change + modifiers.PressureGainModifier));
                    break;
                    // etc for other value types
            }
        }

        string description = GenerateDescription(pattern, context);

        // Create the choice using the builder and add requirements, costs, and rewards
        return new ChoiceBuilder()
            .WithName(description)
            .WithChoiceType(pattern.ChoiceType)
            .RequiresEnergy(pattern.EnergyType,
                pattern.BaseCost + modifiers.EnergyCostModifier)
            .WithValueChanges(finalValueChanges)
            .WithRequirements(pattern.Requirements)
            .WithCosts(pattern.Costs)
            .WithRewards(pattern.Rewards)
            .Build();
    }


    private ChoiceValueModifiers CalculateModifiers(
        ChoicePattern pattern,
        EncounterActionContext context)
    {
        ChoiceValueModifiers mods = new ChoiceValueModifiers();

        // Apply skill vs difficulty modifier
        mods.OutcomeModifier +=
            context.PlayerState.GetSkillLevel(GetRelevantSkill(context)) -
            context.LocationDifficulty;

        return mods;
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

    private string GenerateDescription(ChoicePattern pattern, EncounterActionContext context)
    {
        string description = "";

        // Action type
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                description += "Work";
                break;
            case BasicActionTypes.Investigate:
                description += "Investigate";
                break;
            case BasicActionTypes.Mingle:
                description += "Mingle";
                break;
            default:
                description += pattern.ChoiceType.ToString(); // Fallback
                break;
        }

        // Location
        description += $" at the {context.LocationArchetype}";

        // Choice type modifier
        switch (pattern.ChoiceType)
        {
            case ChoiceTypes.Aggressive:
                description += " (Aggressively)";
                break;
            case ChoiceTypes.Careful:
                description += " (Carefully)";
                break;
            case ChoiceTypes.Tactical:
                description += " (Tactically)";
                break;
        }

        // Requirements
        if (pattern.Requirements.Any())
        {
            description += " (Requires: ";
            description += string.Join(", ", pattern.Requirements.Select(r => r.GetDescription()));
            description += ")";
        }

        return description;
    }

}