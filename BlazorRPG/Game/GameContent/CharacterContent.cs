public class CharacterContent
{
    public static Character Bartender => new CharacterBuilder()
        .ForCharacter(CharacterNames.Bartender)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static Character WealthyMerchant => new CharacterBuilder()
        .ForCharacter(CharacterNames.WealthyMerchant)
        .WithMotivation(CharacterMotivations.Profit)
        .WithPersonality(personality => personality
            .PrefersDirect(true)          // Prefers business talk
            .ValuesTrust(true)            // Rewards consistent behavior
            .SharesKnowledge(false))      // Keeps trade secrets
        .AddSchedule(schedule => schedule
            .AtTime(TimeSlots.Morning)
            .AtSpot(LocationSpotNames.MarketBazaar)
            .WithAction(BasicActionTypes.Trade))
        .AddGoal(goal => goal
            .WithDescription("Train son to take over business")
            .WithReward(RewardTypes.TradeDiscount))
        .Build();
}
