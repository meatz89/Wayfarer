public class CharacterBuilder
{
    private CharacterNames character;
    private LocationNames location;
    private List<BasicAction> actions = new();

    public CharacterBuilder ForCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }

    public CharacterBuilder InLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public CharacterBuilder SetCharacterType(CharacterTypes characterType)
    {
        return this;
    }

    public CharacterBuilder SetDangerLevel(DangerLevels dangerLevel)
    {
        return this;
    }

    public CharacterBuilder AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public Character Build()
    {
        List<BasicAction> locActions = CharacterActionsFactory.Create(
        );

        locActions.AddRange(actions);

        return new Character
        {
            CharacterName = character,
            Location = location,
            Actions = locActions
        };
    }
}
