public class CharacterPropertiesContent
{
    public static Character MarketVendor => new CharacterPropertiesBuilder()
        .ForCharacter(CharacterNames.MarketVendor)
        .InLocation(LocationNames.HarborStreets)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}
