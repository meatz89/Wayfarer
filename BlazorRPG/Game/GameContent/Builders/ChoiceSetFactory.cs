using Microsoft.Extensions.FileSystemGlobbing.Internal;

public class ChoiceSetFactory
{
    public ChoiceSet CreateFromChoiceSet(ChoiceSetTemplate template, EncounterContext context)
    {
        // Create base choices from patterns
        List<EncounterChoice> choices = new();
        int i = 0;
        foreach (ChoiceTemplate choiceTemplate in template.ChoiceTemplates)
        {
            i++;
            EncounterChoice choice = CreateChoiceFromTemplate(i, choiceTemplate, context);
            choices.Add(choice);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private EncounterChoice CreateChoiceFromTemplate(int index, ChoiceTemplate template, EncounterContext context)
    {
        string description = GenerateDescription(template, context);
        
        // Create the choice with only base values
        ChoiceBuilder choiceBuilder = new ChoiceBuilder()
            .WithIndex(index)
            .WithDescription(description)
            .WithArchetype(template.ChoiceArchetype)
            .WithApproach(template.ChoiceApproach)
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
        description += $"{pattern.ChoiceArchetype} - ";
        description += $"{pattern.ChoiceApproach}";
        return description;
    }
}