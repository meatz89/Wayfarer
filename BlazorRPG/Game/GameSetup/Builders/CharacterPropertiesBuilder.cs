public class CharacterPropertiesBuilder
{
    private CharacterNames character;
    private LocationNames location;
    private List<BasicAction> actions = new();

    public CharacterPropertiesBuilder ForCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }
    
    public CharacterPropertiesBuilder InLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public CharacterPropertiesBuilder SetCharacterType(CharacterTypes characterType)
    {
        return this;
    }

    public CharacterPropertiesBuilder SetDangerLevel(DangerLevels dangerLevel)
    {
        return this;
    }

    public CharacterPropertiesBuilder AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public CharacterProperties Build()
    {
        List<BasicAction> locActions = CharacterActionsFactory.Create(
        );

        locActions.AddRange(actions);

        return new CharacterProperties
        {
            Character = character,
            Location = location,
            Actions = locActions
        };
    }
}
