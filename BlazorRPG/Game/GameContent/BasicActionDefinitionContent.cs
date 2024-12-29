public class BasicActionDefinitionContent
{
    public static BasicActionDefinition LaborAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Labor)
        .ExpendsPhysicalEnergy(1)
        .RewardsCoins(2)
        .Build();

    public static BasicActionDefinition FoodBuyAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Trade)
        .ExpendsCoins(1)
        .RewardsFood(2)
        .Build();

    public static BasicActionDefinition FoodSellAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Trade)
        .ExpendsFood(1)
        .RewardsCoins(1)
        .Build();

    public static BasicActionDefinition DiscussAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Labor)
        .ExpendsSocialEnergy(1)
        .RewardsTrust(2)
        .Build();

    public static BasicActionDefinition RestAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Labor)
        .ExpendsPhysicalEnergy(1)
        .ExpendsCoins(2)
        .RewardsFullRecovery()
        .Build();

}
