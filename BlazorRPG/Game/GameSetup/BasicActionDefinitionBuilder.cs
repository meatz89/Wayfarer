


using Microsoft.Extensions.Hosting;

public class BasicActionDefinitionBuilder
{
    private BasicActionTypes actionType;
    public List<IRequirement> requirements { get; set; } = new();
    public List<IOutcome> outcomes { get; set; } = new();

    public BasicActionDefinitionBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public BasicActionDefinitionBuilder ExpendsPhysicalEnergy(int cost)
    {
        requirements.Add(new PhysicalEnergyRequirement
        {
            Amount = cost
        });

        outcomes.Add(new PhysicalEnergyOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsFocusEnergy(int cost)
    {
        requirements.Add(new FocusEnergyRequirement
        {
            Amount = cost
        });

        outcomes.Add(new FocusEnergyOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsSocialEnergy(int cost)
    {
        requirements.Add(new SocialEnergyRequirement
        {
            Amount = cost
        });

        outcomes.Add(new SocialEnergyOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsCoins(int cost)
    {
        requirements.Add(new CoinsRequirement
        {
            Amount = cost
        });

        outcomes.Add(new CoinsOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsFood(int cost)
    {
        requirements.Add(new FoodRequirement
        {
            Amount = cost
        });

        outcomes.Add(new FoodOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsCoins(int amount)
    {
        outcomes.Add(new CoinsOutcome
        {
            Amount = amount
        });

        return this;
    }

    internal BasicActionDefinitionBuilder RewardsFood(int amount)
    {
        outcomes.Add(new FoodOutcome
        {
            Amount = amount
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsTrust(int amount)
    {
        return this;
    }

    public BasicActionDefinitionBuilder RewardsFullRecovery()
    {
        return this;
    }

    public BasicActionDefinition Build()
    {
        return new BasicActionDefinition
        {
            ActionType = actionType,
            Requirements = requirements,
            Outcomes = outcomes
        };
    }
}
