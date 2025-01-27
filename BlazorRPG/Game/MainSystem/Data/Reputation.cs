
public class Reputation
{
    public PlayerReputationTypes ReputationType { get; set; }
    public int Value { get; set; }

    public Reputation(PlayerReputationTypes reputationType, int value)
    {
        ReputationType = reputationType;
        Value = value;
    }
}
