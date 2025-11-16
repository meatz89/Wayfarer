/// <summary>
/// Service for NPC-related business logic
/// </summary>
public class NPCService
{
    private readonly NPCRepository _repository;
    private readonly TimeManager _timeManager;

    public NPCService(NPCRepository repository, TimeManager timeManager)
    {
        _repository = repository;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Get time block service planning data for UI display
    /// </summary>
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = _repository.GetNPCsForLocation(locationId).ToList();

        foreach (TimeBlocks timeBlock in allTimeBlocks)
        {
            List<NPC> availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();

            timeBlockPlan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableNPCs = availableNPCs,
                AvailableServices = new(),
                IsCurrentTimeBlock = timeBlock == _timeManager.GetCurrentTimeBlock()
            });
        }

        return timeBlockPlan;
    }
    /// <summary>
    /// Check if a specific NPC is available to interact with at the current time
    /// </summary>
    public bool IsNPCAvailable(string npcId)
    {
        NPC npc = _repository.GetById(npcId);
        if (npc == null)
        {
            return false;
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return npc.IsAvailable(currentTime);
    }

    /// <summary>
    /// Get NPCs available for interaction at a specific Venue location
    /// </summary>
    public IEnumerable<NPCInteractionInfo> GetAvailableInteractions(string locationId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        IEnumerable<NPC> npcs = _repository.GetNPCsForLocationAndTime(locationId, currentTime);

        return npcs.Select(npc => new NPCInteractionInfo
        {
            NPC = npc,
            IsAvailable = npc.IsAvailable(currentTime),
            AvailableServices = new(),
            UnavailableReason = !npc.IsAvailable(currentTime) ? "NPC is not available at this time" : null
        });
    }

}

/// <summary>
/// Information about an NPC interaction opening
/// </summary>
public class NPCInteractionInfo
{
    public NPC NPC { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> AvailableServices { get; set; }
    public string UnavailableReason { get; set; }
}