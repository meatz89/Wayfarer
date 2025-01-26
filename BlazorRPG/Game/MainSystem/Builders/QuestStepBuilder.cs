public class QuestStepBuilder
{
    private string description;

    private List<Requirement> requirements = new();
    private ActionImplementation action;
    private LocationNames location;
    private CharacterNames character;

    public QuestStepBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public QuestStepBuilder WithAction(Action<ActionBuilder> buildAction)
    {
        ActionBuilder builder = new ActionBuilder();
        buildAction(builder);
        this.action = builder.Build();

        return this;
    }

    public QuestStepBuilder WithLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public QuestStepBuilder WithCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }

    public QuestStepBuilder RequiresReputation(int count)
    {
        ReputationRequirement item = new ReputationRequirement(count);
        requirements.Add(item);
        return this;
    }

    public QuestStepBuilder RequiresCoins(int count)
    {
        CoinsRequirement item = new CoinsRequirement(count);
        requirements.Add(item);

        return this;
    }

    public QuestStepBuilder RequiresStatus(PlayerStatus status)
    {
        StatusRequirement item = new StatusRequirement(status);
        requirements.Add(item);

        return this;
    }

    public QuestStep Build()
    {
        return new QuestStep()
        {
            Description = description,
            Requirements = requirements,
            QuestAction = action,
            Location = location,
            Character = character
        };
    }

}