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