public class ChoiceSetFactory
{
    public ChoiceSet CreateFromChoiceSet(ChoiceSetTemplate template, EncounterContext context)
    {
        if (!IsTemplateValid(template, context))
            return null;

        // Create base choices from patterns
        List<EncounterChoice> choices = new();
        foreach (ChoiceTemplate choiceTemplate in template.ChoiceTemplates)
        {
            EncounterChoice choice = CreateChoiceFromTemplate(choiceTemplate, context);
            choices.Add(choice);
        }

        return new ChoiceSet(choices);
    }

    private EncounterChoice CreateChoiceFromTemplate(ChoiceTemplate template, EncounterContext context)
    {
        string description = GenerateDescription(template, context);

        // Create the choice with only base values
        ChoiceBuilder choiceBuilder = new ChoiceBuilder()
            .WithDescription(description)
            .WithArchetype(template.Archetype)
            .WithApproach(template.Approach)
            .WithRelevantSkill(template.RelevantSkill)
            .RequiresEnergy(template.EnergyType, template.BaseEnergyCost)
            .WithValueChanges(template.BaseValueChanges)
            .WithRequirements(template.Requirements)
            .WithBaseCosts(template.Costs)
            .WithBaseRewards(template.Rewards);

        return choiceBuilder.Build();
    }

    private string GenerateDescription(ChoiceTemplate pattern, EncounterContext context)
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

        // Requirements
        if (pattern.Requirements.Any())
        {
            description += " (Requires: ";
            description += string.Join(", ", pattern.Requirements.Select(r => r.GetDescription()));
            description += ")";
        }

        return description;
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