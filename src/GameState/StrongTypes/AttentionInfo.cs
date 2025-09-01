public class AttentionInfo
{
    public int Current { get; init; }
    public int Max { get; init; }

    public AttentionInfo(int current, int max)
    {
        Current = current;
        Max = max;
    }
}