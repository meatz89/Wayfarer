public class ChoiceSetFactory
{
    public ChoiceSet CreateFromTemplate(ChoiceSetTemplate template, EncounterContext context)
    {
        if (!IsTemplateValid(template, context))
            return null;

        // Create base choices from patterns
        List<EncounterChoice> choices = new();
        foreach (ChoicePattern pattern in template.ChoicePatterns)
        {
            EncounterChoice choice = CreateChoiceFromPattern(pattern, context);
            choices.Add(choice);
        }

        return new ChoiceSet(choices);
    }

    private EncounterChoice CreateChoiceFromPattern(ChoicePattern pattern, EncounterContext context)
    {
        string description = GenerateDescription(pattern, context);

        // Create the choice with only base values
        ChoiceBuilder choiceBuilder = new ChoiceBuilder()
            .WithName(description)
            .RequiresEnergy(pattern.EnergyType, pattern.BaseCost)
            .WithValueChanges(pattern.BaseValueChanges)
            .WithRequirements(pattern.Requirements)
            .WithBaseCosts(pattern.Costs)
            .WithBaseRewards(pattern.Rewards);

        return choiceBuilder.Build();
    }

    private string GenerateDescription(ChoicePattern pattern, EncounterContext context)
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
                description += context.ActionType.ToString();
                break;
        }

        // Location
        description += $" at the {context.LocationArchetype}";

        // Choice type based on base value changes
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

    private string GetChoiceType(List<ValueChange> valueChanges)
    {
        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 2) &&
            (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0) ||
             valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change > 0)))
        {
            return "Carefully";
        }

        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change >= 3))
        {
            return "Aggressively";
        }

        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Outcome && vc.Change >= 1) &&
            valueChanges.Any(vc => vc.ValueType == ValueTypes.Insight && vc.Change >= 2))
        {
            return "Tactically";
        }

        if (valueChanges.Any(vc => vc.ValueType == ValueTypes.Pressure && vc.Change < 0))
        {
            return "Carefully";
        }

        return "";
    }

    private bool IsTemplateValid(ChoiceSetTemplate template, EncounterContext context)
    {
        bool hasLocationConditions = template.AvailabilityConditions.Any();
        bool locationConditionsMet = hasLocationConditions &&
            template.AvailabilityConditions.All(cond => cond.IsMet(context.LocationProperties));
        if (hasLocationConditions && !locationConditionsMet)
        {
            return false;
        }

        bool hasStateConditions = template.StateConditions.Any();
        bool stateConditionsMet = hasStateConditions &&
            template.StateConditions.All(cond => cond.IsMet(context.CurrentValues));
        if (hasStateConditions && !stateConditionsMet)
        {
            return false;
        }

        return true;
    }
}