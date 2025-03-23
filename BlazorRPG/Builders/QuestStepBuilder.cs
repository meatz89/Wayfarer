public class QuestStepBuilder
{
    private string description;

    private List<Requirement> requirements = new();
    private ActionImplementation action;
    private string location;
    private string character;

    public QuestStepBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public QuestStepBuilder WithLocation(string location)
    {
        this.location = location;
        return this;
    }

    public QuestStepBuilder WithCharacter(string character)
    {
        this.character = character;
        return this;
    }

    public QuestStepBuilder RequiresConfidence(PlayerConfidenceTypes confidence)
    {
        PlayerConfidenceRequirement item = new PlayerConfidenceRequirement(confidence);
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