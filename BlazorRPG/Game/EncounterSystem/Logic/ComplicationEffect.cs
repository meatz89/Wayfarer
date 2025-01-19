
public class ComplicationEffect
{
    public ComplicationTypes Type { get; }
    public string Description { get; }
    public Outcome Consequence { get; }

    public ComplicationEffect(ComplicationTypes type, string description, Outcome consequence)
    {
        Type = type;
        Description = description;
        Consequence = consequence;
    }
}
