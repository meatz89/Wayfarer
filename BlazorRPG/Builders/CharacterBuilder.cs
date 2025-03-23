public class CharacterBuilder
{
    private string character;
    private string location;
    private List<ActionImplementation> actions = new();

    public CharacterBuilder ForCharacter(string character)
    {
        this.character = character;
        return this;
    }

    public CharacterBuilder InLocation(string location)
    {
        this.location = location;
        return this;
    }

    public CharacterBuilder SetCharacterType(CharacterTypes characterType)
    {
        return this;
    }

    public Character Build()
    {
        return new Character
        {
            Name = character,
            HomeLocationId = location,
        };
    }
}
