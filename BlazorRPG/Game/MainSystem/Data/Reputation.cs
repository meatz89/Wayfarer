
public class Confidence
{
    public PlayerConfidenceTypes ConfidenceType { get; set; }
    public int Value { get; set; }

    public Confidence(PlayerConfidenceTypes reputationType, int value)
    {
        ConfidenceType = reputationType;
        Value = value;
    }
}
