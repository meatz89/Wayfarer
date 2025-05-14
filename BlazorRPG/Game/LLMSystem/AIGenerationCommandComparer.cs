public class AIGenerationCommandComparer : IComparer<AIGenerationCommand>
{
    public int Compare(AIGenerationCommand x, AIGenerationCommand y)
    {
        // First compare by priority
        int priorityComparison = x.Priority.CompareTo(y.Priority);
        if (priorityComparison != 0)
            return priorityComparison;

        // If same priority, use timestamp (FIFO)
        return x.Timestamp.CompareTo(y.Timestamp);
    }
}
