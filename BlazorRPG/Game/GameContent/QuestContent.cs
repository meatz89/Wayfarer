public class QuestContent
{
    public static Quest MerchantApprentice => new QuestBuilder()
        .WithTitle("The Merchant's Son")
        .WithDescription("Help the merchant's son learn the trade")
        .AddStep(step => step
            .WithDescription("Prove yourself as reliable to the merchant")
            .RequiresReputation(ReputationTypes.Reliable, 2)
            .RequiresCoins(50)
            .WithLocation(LocationNames.MarketSquare)
            .WithLocationSpot(LocationSpotNames.GeneralStore)
            .WithCharacter(CharacterNames.WealthyMerchant)
            .WithAction(action => action
                .ForAction(BasicActionTypes.Trade)
                .WithDescription("Prove yourself to Merchant")
                .ExpendsEnergy(1, EnergyTypes.Social)
                .RewardsReputation(ReputationTypes.Reliable, 1)
            ))
        .AddStep(step => step
            .WithDescription("Teach basic trading to the merchant's son")
            .RequiresStatus(StatusTypes.WellRested)
            .WithLocation(LocationNames.MarketSquare)
            .WithLocationSpot(LocationSpotNames.GeneralStore)
            .WithCharacter(CharacterNames.WealthyMerchant)
            .WithAction(action => action
                .ForAction(BasicActionTypes.Teach)
                .WithDescription("Teach Merchant's Son")
                .ExpendsEnergy(2, EnergyTypes.Social)
                .ExpendsEnergy(1, EnergyTypes.Focus)
                .RewardsCoins(10)
                .UnlocksAchievement(AchievementTypes.TrustedHelper)
            ))
        .Build();
}