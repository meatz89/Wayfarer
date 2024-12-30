public class CharacterPropertiesContent
{
    public static CharacterProperties MarketVendor => new CharacterPropertiesBuilder()
        .ForCharacter(CharacterNames.MarketVendor)
        .InLocation(LocationNames.HarborStreets)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}
