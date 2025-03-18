public class CharacterBuilder
{
    private CharacterNames character;
    private LocationNames location;
    private List<ActionImplementation> actions = new();
    private List<Schedule> schedules = new();

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

    public CharacterBuilder AddSchedule(Action<ScheduleBuilder> buildSchedule)
    {
        ScheduleBuilder builder = new ScheduleBuilder();
        buildSchedule(builder);
        schedules.Add(builder.Build());
        return this;
    }

    public Character Build()
    {
        return new Character
        {
            CharacterName = character,
            Location = location,
        };
    }
}
