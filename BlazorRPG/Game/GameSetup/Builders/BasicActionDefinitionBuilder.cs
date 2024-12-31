
using Microsoft.Extensions.Hosting;

public class BasicActionDefinitionBuilder
{
    private BasicActionTypes actionType;
    private string description;
    private List<TimeWindows> timeSlots = new();
    public List<IRequirement> requirements = new();
    public List<IOutcome> outcomes = new();

    public BasicActionDefinitionBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public BasicActionDefinitionBuilder WithDescription(string description)
    {
        this.description = description;

        return this;
    }

    public BasicActionDefinitionBuilder RequiresInventorySlots(int slots)
    {
        this.requirements.Add(new InventorySlotsRequirement
        {
            Count = slots
        });

        return this;
    }

    public BasicActionDefinitionBuilder AddTimeWindow(TimeWindows timeSlot)
    {
        this.timeSlots.Add(timeSlot);

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsHealth(int cost)
    {
        requirements.Add(new HealthRequirement
        {
            Amount = cost
        });

        outcomes.Add(new HealthOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder ExpendsEnergy(int energyCost, EnergyTypes energyType)
    {
        switch (energyType)
        {
            case EnergyTypes.Physical:
                return ExpendsPhysicalEnergy(energyCost);

            case EnergyTypes.Focus:
                return ExpendsFocusEnergy(energyCost);

            case EnergyTypes.Social:
                return ExpendsSocialEnergy(energyCost);
        }
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

    public BasicActionDefinitionBuilder ExpendsItem(ResourceTypes item, int count)
    {
        requirements.Add(new ItemRequirement
        {
            ResourceType = item,
            Count = count
        });

        outcomes.Add(new ItemOutcome
        {
            ChangeType = ItemChangeType.Removed,
            ResourceType = item,
            Count = count
        });

        return this;
    }
    public BasicActionDefinitionBuilder RewardsItem(ResourceTypes item, int count)
    {
        outcomes.Add(new ItemOutcome
        {
            ChangeType = ItemChangeType.Added,
            ResourceType = item,
            Count = count
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


    public BasicActionDefinitionBuilder RewardsFood(int amount)
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

    public BasicActionDefinitionBuilder RewardsHealth(int amount)
    {
        outcomes.Add(new HealthOutcome
        {
            Amount = amount
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsPhysicalEnergy(int amount)
    {
        outcomes.Add(new PhysicalEnergyOutcome
        {
            Amount = amount
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsFocusEnergy(int amount)
    {
        outcomes.Add(new FocusEnergyOutcome
        {
            Amount = amount
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsSocialEnergy(int amount)
    {
        outcomes.Add(new SocialEnergyOutcome
        {
            Amount = amount
        });

        return this;
    }

    public BasicAction Build()
    {
        return new BasicAction
        {
            Id = actionType,
            Name = description,
            TimeSlots = timeSlots,
            Requirements = requirements,
            Outcomes = outcomes
        };
    }
}
