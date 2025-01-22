
public class Reputation
{
    public ReputationTypes ReputationType { get; set; }
    public int Value { get; set; }

    public Reputation(ReputationTypes reputationType, int value)
    {
        ReputationType = reputationType;
        Value = value;
    }
}
