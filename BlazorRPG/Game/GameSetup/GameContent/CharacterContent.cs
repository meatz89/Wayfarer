public class CharacterContent
{
    public static Character Bartender => new CharacterBuilder()
        .ForCharacter(CharacterNames.Bartender)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}
