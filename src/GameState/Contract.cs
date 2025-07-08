public class Contract
{
    public string Id { get; set; }
    public string Description { get; set; }
    public List<string> RequiredItems { get; set; } = new List<string>();
    public List<string> RequiredLocations { get; set; } = new List<string>();
    public string DestinationLocation { get; set; }
    public int StartDay { get; set; }
    public int DueDay { get; set; }
    public int Payment { get; set; }
    public string FailurePenalty { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsFailed { get; set; } = false;
    public List<string> UnlocksContractIds { get; set; } = new List<string>();
    public List<string> LocksContractIds { get; set; } = new List<string>();

    public bool IsAvailable(int currentDay, TimeBlocks currentTimeBlock)
    {
        return !IsCompleted && !IsFailed && currentDay >= StartDay && currentDay <= DueDay;
    }

    public bool CanComplete(Player player, string currentLocationId)
    {
        // Check if player is at the destination
        if (currentLocationId != DestinationLocation) return false;

        // Check if player has all required items
        foreach (string requiredItem in RequiredItems)
        {
            if (Array.IndexOf(player.Inventory.ItemSlots, requiredItem) == -1)
            {
                return false;
            }
        }

        // Check if player has visited all required locations
        foreach (string requiredLocation in RequiredLocations)
        {
            if (!player.HasVisitedLocation(requiredLocation))
            {
                return false;
            }
        }

        return true;
    }
}