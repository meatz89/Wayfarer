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
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(Location location)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = _repository.GetNPCsForLocation(location).ToList();

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
    /// Get NPCs available for interaction at a specific Venue location
    /// </summary>
    public IEnumerable<NPCInteractionInfo> GetAvailableInteractions(Location location)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        IEnumerable<NPC> npcs = _repository.GetNPCsForLocationAndTime(location, currentTime);

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