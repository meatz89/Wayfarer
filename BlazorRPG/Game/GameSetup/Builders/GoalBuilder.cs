
public class GoalBuilder
{
    private string description;
    private RewardTypes reward;

    public GoalBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public GoalBuilder WithReward(RewardTypes reward)
    {
        this.reward = reward;
        return this;
    }

    public Goal Build()
    {
        return new Goal();
    }
}