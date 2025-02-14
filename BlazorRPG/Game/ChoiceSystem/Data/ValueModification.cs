public class BaseValueChange
{
    public int Amount { get; }

    public BaseValueChange(int amount)
    {
        Amount = amount;
    }

    public override string ToString()
    {
        return $"{Amount}";
    }
}

// Modifications - track source and can be chained/combined
public abstract class ValueModification
{
    public int Amount { get; set; }
    public string Source { get; set; }
}

// Modifications - track source and can be chained/combined
public class MomentumModification : ValueModification
{
    public MomentumModification(int amount, string source)
    {
        Amount = amount;
        Source = source;
    }

    public override string ToString()
    {
        return $"Momentum: {Amount} (from {Source}";
    }
}

public class EnergyCostReduction : ValueModification
{
    public EnergyTypes EnergyType { get; set; }

    public EnergyCostReduction(EnergyTypes type, int amount, string source)
    {
        EnergyType = type;
        Amount = amount;
        Source = source;
    }

    public override string ToString()
    {
        return $"{EnergyType}: {Amount} (from {Source}";
    }
}
