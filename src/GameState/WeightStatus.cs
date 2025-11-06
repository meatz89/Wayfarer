/// <summary>
/// Player's current weight status for UI display
/// Replaces value tuple (int current, int max)
/// </summary>
public class WeightStatus
{
    public int Current { get; init; }
    public int Max { get; init; }

    public WeightStatus(int current, int max)
    {
        Current = current;
        Max = max;
    }
}
