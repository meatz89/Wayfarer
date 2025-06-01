public class FocusCost
{
    public int Amount { get; private set; }

    public FocusCost(int amount)
    {
        Amount = amount;
    }

    // For JSON serialization
    public object ToJsonObject()
    {
        return new { Amount = Amount };
    }
}