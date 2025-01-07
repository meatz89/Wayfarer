public class CharacterContent
{
    public static Character Bartender => new CharacterBuilder()
        .ForCharacter(CharacterNames.Bartender)
        .AddSchedule(schedule => schedule
            .AtTime(TimeSlots.Night)
            .AtLocation(LocationNames.GenericMarket)
            .WithAction(BasicActionTypes.Trade))
        .Build();

    public static Character WealthyMerchant => new CharacterBuilder()
        .ForCharacter(CharacterNames.WealthyMerchant)
        .AddSchedule(schedule => schedule
            .AtTime(TimeSlots.Morning)
            .AtLocation(LocationNames.GenericMarket)
            .WithAction(BasicActionTypes.Trade))
        .Build();
}
