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

        // **Calculate Modifiers**
        modifiers = CalculateModifiers(pattern, context);

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
                case ValueTypes.Insight:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Insight,
                        baseChange.Change + modifiers.InsightGainModifier));
                    break;
                case ValueTypes.Resonance:
                    finalValueChanges.Add(new ValueChange(
                        ValueTypes.Resonance,
                        baseChange.Change + modifiers.ResonanceGainModifier));
                    break;
            }
        }

        string description = GenerateDescription(pattern, context);

        // Create the choice using the builder and add requirements, costs, and rewards
        return new ChoiceBuilder()
            .WithName(description)
            .RequiresEnergy(pattern.EnergyType, pattern.BaseCost + modifiers.EnergyCostModifier)
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

        // **Skill vs. Difficulty**
        mods.OutcomeModifier += context.PlayerState.GetSkillLevel(GetRelevantSkill(context)) - context.LocationDifficulty;

        // **Insight reduces Pressure gain**
        mods.PressureGainModifier -= context.CurrentValues.Insight;
        mods.OutcomeModifier += context.CurrentValues.Resonance;

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
                description += context.ActionType.ToString(); // Use the action type as a fallback
                break;
        }

        // Location
        description += $" at the {context.LocationArchetype}";

        // Dynamically determine choice type based on value changes
        string choiceType = GetChoiceType(pattern.BaseValueChanges);
        if (!string.IsNullOrEmpty(choiceType))
        {
            description += $" ({choiceType})";
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

    // Helper method to determine choice type based on value changes
    private string GetChoiceType(List<ValueChange> valueChanges)
    {
        // If a choice increases Outcome significantly and decreases Pressure or increases Insight, it could be considered "Careful"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 2) &&
            (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0) ||
             valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change > 0)))
        {
            return "Carefully";
        }

        // If a choice significantly increases Pressure, it could be considered "Aggressive"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change >= 3))
        {
            return "Aggressively";
        }

        // If a choice increases both Outcome and Insight, it could be considered "Tactical"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 1) &&
            valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change >= 2))
        {
            return "Tactically";
        }

        // If a choice restores values like Health, Energy, or reduces Pressure, it could be considered "Recovery"
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0))
        {
            return "Carefully";
        }

        // Default: no specific choice type
        return "";
    }

}