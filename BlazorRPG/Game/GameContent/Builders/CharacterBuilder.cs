public class CharacterBuilder
{
    private CharacterNames character;
    private LocationNames location;
    private List<ActionImplementation> actions = new();
    private CharacterMotivations characterMotivation;
    private CharacterPersonality personality;
    private List<Schedule> schedules = new();
    private List<Goal> goals = new();

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

    public CharacterBuilder WithMotivation(CharacterMotivations motivation)
    {
        this.characterMotivation = motivation;
        return this;
    }

    public CharacterBuilder WithPersonality(Action<PersonalityBuilder> buildStage)
    {
        PersonalityBuilder builder = new PersonalityBuilder();
        buildStage(builder);
        personality = builder.Build();

        return this;
    }


    public CharacterBuilder AddAction(Action<ActionBuilder> buildBasicAction)
    {
        ActionBuilder builder = new ActionBuilder();
        buildBasicAction(builder);
        actions.Add(builder.Build());
        return this;
    }

    public CharacterBuilder AddSchedule(Action<ScheduleBuilder> buildSchedule)
    {
        ScheduleBuilder builder = new ScheduleBuilder();
        buildSchedule(builder);
        schedules.Add(builder.Build());
        return this;
    }

    public CharacterBuilder AddGoal(Action<GoalBuilder> buildGoal)
    {
        GoalBuilder builder = new GoalBuilder();
        buildGoal(builder);
        goals.Add(builder.Build());
        return this;
    }

    public Character Build()
    {
        List<ActionImplementation> locActions = CharacterActionsFactory.Create(
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
