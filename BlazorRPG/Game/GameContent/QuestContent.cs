public class QuestContent
{
    public static Quest MerchantApprentice => new QuestBuilder()
        .WithTitle("The Merchant's Son")
        .WithDescription("Help the merchant's son learn the trade")
        .AddStep(step => step
            .WithDescription("Prove yourself as reliable to the merchant")
            .RequiresReputation(ReputationTypes.Reliable, 2)
            .RequiresCoins(50)
            .WithLocation(LocationNames.GenericMarket)
            .WithCharacter(CharacterNames.WealthyMerchant)
            .WithAction(action => action
                .ForAction(BasicActionTypes.Persuade)
                .WithDescription("Prove yourself to Merchant")
                .ExpendsEnergy(1, EnergyTypes.Social)
                .RewardsReputation(ReputationTypes.Reliable, 1)
            ))
        .Build();
}