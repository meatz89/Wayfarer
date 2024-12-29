public class BasicActionDefinitionContent
{
    public static BasicActionDefinition LaborAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Labor)
        .ExpendsPhysicalEnergy(1)
        .RewardsCoins(2)
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
