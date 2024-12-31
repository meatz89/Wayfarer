public class CharacterContent
{
    public static Character MarketVendor => new CharacterBuilder()
        .ForCharacter(CharacterNames.MarketVendor)
        .InLocation(LocationNames.HarborStreets)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}
