/// <summary>
/// Epic 9: Result of attempting to refresh attention with coins
/// </summary>
public class AttentionRefreshResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingAttention { get; set; }
    public int AttentionGained { get; set; }
    public int CoinsSpent { get; set; }
}

/// <summary>
/// Epic 9: Current status of attention refresh availability
/// </summary>
public class AttentionRefreshStatus
{
    public bool CanRefresh { get; set; }
    public bool RefreshUsedThisBlock { get; set; }
    public int CurrentAttention { get; set; }
    public int MaximumPossible { get; set; }
    public bool AtMaximum { get; set; }
}