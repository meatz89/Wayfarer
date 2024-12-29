public class BasicActionDefinitionContent
{
    public static BasicActionDefinition LaborAction => new BasicActionDefinitionBuilder()
        .ForAction(BasicActionTypes.Labor)
        .ExpendsPhysicalEnergy(1)
        .RewardsCoins(2)
        .Build();

}
