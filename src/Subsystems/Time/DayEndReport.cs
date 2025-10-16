using System.Collections.Generic;

/// <summary>
/// Day-end summary data structure containing all changes that occurred during the day
/// Used for comprehensive reporting and player feedback at day boundary
/// </summary>
public class DayEndReport
{
    // Obligation tracking
    public List<CompletedObligationInfo> CompletedObligations { get; set; } = new List<CompletedObligationInfo>();
    public List<FailedObligationInfo> FailedObligations { get; set; } = new List<FailedObligationInfo>();
    public List<NewObligationInfo> NewObligations { get; set; } = new List<NewObligationInfo>();

    // Financial tracking
    public int CoinsEarned { get; set; } = 0;
    public int CoinsSpent { get; set; } = 0;
    public int NetCoins => CoinsEarned - CoinsSpent;

    // Equipment tracking
    public List<string> NewEquipment { get; set; } = new List<string>();

    // Stat progression tracking
    public List<StatIncreaseInfo> StatsIncreased { get; set; } = new List<StatIncreaseInfo>();

    // Cube gains (localized mastery)
    public List<CubeGainInfo> CubesGained { get; set; } = new List<CubeGainInfo>();

    // Current resources after restoration
    public ResourceSnapshot CurrentResources { get; set; } = new ResourceSnapshot();
}

/// <summary>
/// Information about a completed obligation (investigation)
/// </summary>
public class CompletedObligationInfo
{
    public string ObligationName { get; set; }
    public int RewardCoins { get; set; }
    public int RewardCubes { get; set; }
}

/// <summary>
/// Information about a failed obligation (missed deadline)
/// </summary>
public class FailedObligationInfo
{
    public string ObligationName { get; set; }
    public string PatronName { get; set; }
    public int CubesRemoved { get; set; }
}

/// <summary>
/// Information about a newly spawned obligation
/// </summary>
public class NewObligationInfo
{
    public string ObligationName { get; set; }
    public string ObligationType { get; set; }
}

/// <summary>
/// Information about stat increases during the day
/// </summary>
public class StatIncreaseInfo
{
    public string StatName { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
}

/// <summary>
/// Information about cube gains (localized mastery)
/// </summary>
public class CubeGainInfo
{
    public CubeType Type { get; set; }
    public string EntityName { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Type of cube (localized mastery system)
/// </summary>
public enum CubeType
{
    Investigation,
    Story,
    Exploration
}

/// <summary>
/// Snapshot of player resources after day-end restoration
/// </summary>
public class ResourceSnapshot
{
    public int Health { get; set; }
    public int Focus { get; set; }
    public int Stamina { get; set; }
    public int Coins { get; set; }
}
