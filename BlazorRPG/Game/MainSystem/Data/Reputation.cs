
public class Confidence
{
    public PlayerConfidenceTypes ConfidenceType { get; set; }
    public int Value { get; set; }

    public Confidence(PlayerConfidenceTypes confidenceType, int value)
    {
        ConfidenceType = confidenceType;
        Value = value;
    }
}
