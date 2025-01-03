public class QuestStepBuilder
{
    private string description;

    private List<Requirement> requirements = new();
    private BasicAction action;
    private LocationNames location;
    private LocationSpotNames locationSpot;
    private CharacterNames character;

    public QuestStepBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public QuestStepBuilder WithAction(Action<BasicActionDefinitionBuilder> buildAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
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

    public QuestStepBuilder RequiresReputation(ReputationTypes reputationType, int amount)
    {
        ReputationRequirement item = new ReputationRequirement()
        {
            ReputationTypes = reputationType,
            Amount = amount
        };

        requirements.Add(item);

        return this;
    }

    public QuestStepBuilder RequiresCoins(int amount)
    {
        CoinsRequirement item = new CoinsRequirement() { Amount = amount };
        requirements.Add(item);

        return this;
    }

    public QuestStepBuilder RequiresStatus(StatusTypes status)
    {
        StatusRequirement item = new StatusRequirement() { Status = status };
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