// Represents a specific value change in the narrative state
public class ValueChange
{
    public ValueTypes Type { get; }
    public int Amount { get; }

    public ValueChange(ValueTypes type, int amount)
    {
        Type = type;
        Amount = amount;
    }

    public void Apply(NarrativeStateValues state)
    {
        switch (Type)
        {
            case ValueTypes.Momentum:
                state.Momentum += Amount;
                break;
            case ValueTypes.Advantage:
                state.Advantage += Amount;
                break;
            case ValueTypes.Understanding:
                state.Understanding += Amount;
                break;
            case ValueTypes.Connection:
                state.Connection += Amount;
                break;
            case ValueTypes.Tension:
                state.Tension += Amount;
                break;
        }
    }
}