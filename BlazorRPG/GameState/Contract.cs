
public class Contract
{
    public string Id { get; set; }
    public string Description { get; set; }
    public List<string> RequiredItems { get; set; } = new List<string>();  // For compound contracts
    public List<string> RequiredLocations { get; set; } = new List<string>();  // For multi-stop journeys
    public string DestinationLocation { get; set; }
    public int StartDay { get; set; }
    public int DueDay { get; set; }
    public int Payment { get; set; }
    public string FailurePenalty { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsFailed { get; set; } = false;
    public List<string> UnlocksContractIds { get; set; } = new List<string>();  // For chain contracts
    public List<string> LocksContractIds { get; set; } = new List<string>();  // For competitive contracts

    internal bool IsAvailable(int currentDay, TimeBlocks currentTimeBlock)
    {
        throw new NotImplementedException();
    }
}

