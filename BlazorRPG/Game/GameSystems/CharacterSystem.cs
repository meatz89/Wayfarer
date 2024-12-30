
public class CharacterSystem
{
    private readonly GameState gameState;
    private readonly List<CharacterProperties> allCharacterProperties;

    public CharacterSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.allCharacterProperties = contentProvider.GetCharacterProperties();
    }

    public List<BasicAction> GetActionsForCharacter(CharacterNames character)
    {
        CharacterProperties characterProperties = allCharacterProperties.FirstOrDefault(x => x.Character == character);
        if (characterProperties == null) return null;

        List<BasicAction> actions = characterProperties.Actions;
        return actions;
    }

}