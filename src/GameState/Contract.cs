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

    // Time pressure enhancements
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();
    public int CompletedDay { get; set; } = -1; // Day when contract was completed

    public bool IsAvailable(int currentDay, TimeBlocks currentTimeBlock)
    {
        bool basicAvailability = !IsCompleted && !IsFailed && currentDay >= StartDay && currentDay <= DueDay;

        // If no specific time blocks are required, available anytime
        if (!AvailableTimeBlocks.Any())
        {
            return basicAvailability;
        }

        // Check if current time block is in the allowed list
        return basicAvailability && AvailableTimeBlocks.Contains(currentTimeBlock);
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