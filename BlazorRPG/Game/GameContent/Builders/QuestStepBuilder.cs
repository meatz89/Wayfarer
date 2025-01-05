public class QuestStepBuilder
{
    private string description;

    private List<Requirement> requirements = new();
    private ActionImplementation action;
    private LocationNames location;
    private LocationSpotNames locationSpot;
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

    public QuestStepBuilder WithLocationSpot(LocationSpotNames locationSpot)
    {
        this.locationSpot = locationSpot;
        return this;
    }

    public QuestStepBuilder WithCharacter(CharacterNames character)
    {
        this.character = character;
        return this;
    }

    public QuestStepBuilder RequiresReputation(ReputationTypes reputationType, int count)
    {
        ReputationRequirement item = new ReputationRequirement(reputationType, count);
        requirements.Add(item);
        return this;
    }

    public QuestStepBuilder RequiresCoins(int count)
    {
        CoinsRequirement item = new CoinsRequirement(count);
        requirements.Add(item);

        return this;
    }

    public QuestStepBuilder RequiresStatus(PlayerStatusTypes status)
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
            LocationSpot = locationSpot,
            Character = character
        };
    }

}