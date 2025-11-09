public class PaymentRange
{
public int Min { get; init; }
public int Max { get; init; }

public PaymentRange(int min, int max)
{
    Min = min;
    Max = max;
}
}