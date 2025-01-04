public class CharacterSystem
{
    private readonly GameState gameState;
    private readonly List<Character> allCharacterProperties;

    public CharacterSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allCharacterProperties = contentProvider.GetCharacters();
    }

    public List<BasicAction> GetActionsForCharacter(CharacterNames character)
    {
        Character characterProperties = allCharacterProperties.FirstOrDefault(x => x.CharacterName == character);
        if (characterProperties == null) return null;

        List<BasicAction> actions = characterProperties.Actions;
        return actions;
    }

    public CharacterNames? GetCharacterAtLocation(LocationNames currentLocation)
    {
        Character characterProperties = allCharacterProperties.FirstOrDefault(x => x.Location == currentLocation);
        if (characterProperties == null) return null;
        return characterProperties.CharacterName;
    }

    public void ProcessActionImpact(BasicAction basicAction)
    {
    }
}