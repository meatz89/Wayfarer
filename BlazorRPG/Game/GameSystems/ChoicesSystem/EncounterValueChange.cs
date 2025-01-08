// Represents a specific value change in the encounter state
public class EncounterValueChange
{
    public EncounterValues ValueType { get; }
    public int Change { get; }

    public EncounterValueChange(EncounterValues type, int amount)
    {
        ValueType = type;
        Change = amount;
    }

}

public enum EncounterValues
{
    Momentum,
    Advantage,
    Understanding,
    Connection,
    Tension
}
