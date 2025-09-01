public class AttentionStateInfo
{
    public int Current { get; init; }
    public int Max { get; init; }
    public TimeBlocks TimeBlock { get; init; }

    public AttentionStateInfo(int current, int max, TimeBlocks timeBlock)
    {
        Current = current;
        Max = max;
        TimeBlock = timeBlock;
    }
}