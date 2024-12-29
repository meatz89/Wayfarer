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
            Amount = -cost
        });

        outcomes.Add(new PhysicalEnergyOutcome
        {
            Amount = -cost
        });

        return this;
    }

    public BasicActionDefinitionBuilder RewardsCoins(int coins)
    {
        outcomes.Add(new CoinsOutcome
        {
            Amount = coins
        });

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
