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

    public QuestStepBuilder RequiresConfidence(PlayerConfidenceTypes reputation)
    {
        PlayerConfidenceRequirement item = new PlayerConfidenceRequirement(reputation);
        requirements.Add(item);
        return this;
    }

    public QuestStepBuilder RequiresCoins(int count)
    {
        CoinsRequirement item = new CoinsRequirement(count);
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