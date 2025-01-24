
using Microsoft.Extensions.Hosting;
using System;

public static class ActionFactory
{
    public static ActionImplementation CreateAction(ActionTemplate template, Location location)
    {
        // Create a new instance based on the template
        ActionImplementation action = CreateActionImplementationFromTemplate(template);

        // Add rewards based on action type
        switch (action.ActionType)
        {
            case BasicActionTypes.Labor:
                action.SuccessOutcomes.Add(new CoinsOutcome(3)); // Add 3 coins
                break;
            case BasicActionTypes.Gather:
                break;
        }

        return action;
    }

    private static ActionImplementation CreateActionImplementationFromTemplate(ActionTemplate template)
    {
        ActionImplementation actionImplementation = new ActionImplementation
        {
            Name = template.Name,
            ActionType = template.ActionType,
            TimeSlots = new List<TimeSlots>(template.TimeSlots),
            SpotAvailabilityConditions = template.AvailabilityConditions,
            LocationArchetype = template.LocationArchetype,
            CrowdDensity = template.CrowdDensity,
            LocationScale = template.LocationScale,
            FailureOutcomes = GameRules.CreateCostsForAction(template),
            SuccessOutcomes = GameRules.CreateRewardsForTemplate(template),
            EnergyCosts = CreateEnergyCostsForAction(template),
        };
        return actionImplementation;
    }

    private static List<Outcome> CreateEnergyCostsForAction(ActionTemplate template)
    {
        var energyCosts = new List<Outcome>();

        int cost = GameRules.GetBaseEnergyCost(template.ActionType);

        var energy = new EnergyOutcome(EnergyTypes.Social, -cost);
        energyCosts.Add(energy);

        return energyCosts;
    }


}