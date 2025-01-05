public class DynamicActionFactory
{
    public ActionImplementation CreateAction(ActionGenerationContext context)
    {
        // Generate the specialized name
        ActionNameGenerator nameGenerator = new ActionNameGenerator(context);
        string actionName = nameGenerator.GenerateName();

        // Create the action with the generated name
        ActionImplementation action = new ActionImplementation
        {
            Name = actionName,
            ActionType = context.BaseAction,
            Requirements = GenerateRequirements(context),
            Costs = GenerateCosts(context),
            Rewards = GenerateRewards(context),
            TimeSlots = GenerateTimeSlots(context)
        };

        return action;
    }

    private List<Requirement> GenerateRequirements(ActionGenerationContext context)
    {
        List<Requirement> requirements = new();

        // Base energy cost based on intensity
        EnergyTypes energyType = GetEnergyType(context.BaseAction);
        int energyCost = 1;

        requirements.Add(new EnergyRequirement(energyType, energyCost));

        // Add skill requirements for complex activities
        if (context.Activity.Complexity == ComplexityTypes.Complex)
        {
            SkillTypes skillType = GetSkillType(context.BaseAction);
            requirements.Add(new SkillRequirement(skillType, 2));
        }

        return requirements;
    }

    private List<Outcome> GenerateCosts(ActionGenerationContext context)
    {
        List<Outcome> costs = new();

        // Base energy cost
        EnergyTypes energyType = GetEnergyType(context.BaseAction);
        int energyCost = -1;

        costs.Add(new EnergyOutcome(energyType, energyCost));

        return costs;
    }

    private List<Outcome> GenerateRewards(ActionGenerationContext context)
    {
        List<Outcome> rewards = new();

        // Base rewards depend on action type
        switch (context.BaseAction)
        {
            case BasicActionTypes.Labor:
                rewards.Add(new CoinsOutcome(3));
                break;

            case BasicActionTypes.Gather:
                rewards.Add(new ResourceOutcome(GetLocationResource(context.LocationType), 1));
                break;

            case BasicActionTypes.Trade:
                rewards.Add(new ResourceOutcome(ResourceTypes.Food, 1));
                break;

            case BasicActionTypes.Mingle:
                rewards.Add(new ReputationOutcome(GetLocationType(context.LocationType), 1));
                break;
        }

        // Bonus rewards for complex activities
        if (context.Activity.Complexity == ComplexityTypes.Complex)
        {
            rewards.Add(new SkillLevelOutcome(GetSkillType(context.BaseAction), 1));
        }

        return rewards;
    }

    private List<TimeSlots> GenerateTimeSlots(ActionGenerationContext context)
    {
        List<TimeSlots> timeSlots = new();

        // Basic time slots based on action type
        switch (context.BaseAction)
        {
            case BasicActionTypes.Labor:
                timeSlots.Add(TimeSlots.Morning);
                timeSlots.Add(TimeSlots.Afternoon);
                break;

            case BasicActionTypes.Gather:
                timeSlots.Add(TimeSlots.Morning);
                timeSlots.Add(TimeSlots.Afternoon);
                timeSlots.Add(TimeSlots.Evening);
                break;

            case BasicActionTypes.Trade:
                timeSlots.Add(TimeSlots.Morning);
                timeSlots.Add(TimeSlots.Afternoon);
                break;

            case BasicActionTypes.Mingle:
                timeSlots.Add(TimeSlots.Evening);
                timeSlots.Add(TimeSlots.Night);
                break;
        }

        return timeSlots;
    }

    private static EnergyTypes GetEnergyType(BasicActionTypes actionType) => actionType switch
    {
        BasicActionTypes.Labor => EnergyTypes.Physical,
        BasicActionTypes.Gather => EnergyTypes.Physical,
        BasicActionTypes.Trade => EnergyTypes.Social,
        BasicActionTypes.Mingle => EnergyTypes.Social,
        BasicActionTypes.Investigate => EnergyTypes.Focus,
        BasicActionTypes.Study => EnergyTypes.Focus,
        _ => EnergyTypes.Physical
    };

    private static SkillTypes GetSkillType(BasicActionTypes actionType) => actionType switch
    {
        BasicActionTypes.Labor => SkillTypes.PhysicalLabor,
        BasicActionTypes.Gather => SkillTypes.PhysicalLabor,
        BasicActionTypes.Trade => SkillTypes.Trading,
        BasicActionTypes.Mingle => SkillTypes.Socializing,
        BasicActionTypes.Investigate => SkillTypes.Observation,
        _ => SkillTypes.PhysicalLabor
    };

    private static ResourceTypes GetLocationResource(LocationTypes locationType) => locationType switch
    {
        LocationTypes.Industrial => ResourceTypes.Wood,
        LocationTypes.Commercial => ResourceTypes.Cloth,
        LocationTypes.Social => ResourceTypes.Food,
        LocationTypes.Nature => ResourceTypes.Herbs,
        _ => ResourceTypes.Food
    };

    private static ReputationTypes GetLocationType(LocationTypes locationType) => locationType switch
    {
        LocationTypes.Industrial => ReputationTypes.Official,
        LocationTypes.Commercial => ReputationTypes.Merchant,
        LocationTypes.Social => ReputationTypes.Social,
        _ => ReputationTypes.Social
    };
}