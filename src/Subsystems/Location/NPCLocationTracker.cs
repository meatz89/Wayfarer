/// <summary>
/// Tracks NPC positions and availability across locations and Locations.
/// Manages NPC scheduling and focus based on time blocks.
/// </summary>
public class NPCLocationTracker
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;

    public NPCLocationTracker(GameWorld gameWorld, NPCRepository npcRepository)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
    }

    /// <summary>
    /// Get all NPCs at a specific location.
    /// </summary>
    public List<NPC> GetNPCsAtLocation(Location location)
    {
        if (location == null) return new List<NPC>();

        // Use NPCRepository which handles visibility filtering
        return _npcRepository.GetNPCsForLocation(location);
    }

    /// <summary>
    /// Get NPCs at a location during a specific time block.
    /// </summary>
    public List<NPC> GetNPCsAtLocationAndTime(Location location, TimeBlocks timeBlock)
    {
        if (location == null) return new List<NPC>();

        // Use NPCRepository method
        return _npcRepository.GetNPCsForLocationAndTime(location, timeBlock);
    }

    /// <summary>
    /// Get NPCs at a specific location during a time block.
    /// </summary>
    public List<NPC> GetNPCsAtSpot(Location location, TimeBlocks timeBlock)
    {
        if (location == null) return new List<NPC>();

        // Use NPCRepository method for location-specific NPCs
        return _npcRepository.GetNPCsForLocationAndTime(location, timeBlock);
    }

    /// <summary>
    /// Get the primary NPC for a location if available.
    /// </summary>
    public NPC GetPrimaryNPCForSpot(Location location, TimeBlocks timeBlock)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        return _npcRepository.GetPrimaryNPCForSpot(location, timeBlock);
    }
    /// <summary>
    /// Check if an NPC is at a specific location.
    /// </summary>
    public bool IsNPCAtSpot(string npcId, Location location)
    {
        if (string.IsNullOrEmpty(npcId) || location == null) return false;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null || npc.Location == null) return false;

        return npc.Location == location;
    }

    /// <summary>
    /// Check if an NPC is available during a time block.
    /// </summary>
    public bool IsNPCAvailable(string npcId, TimeBlocks timeBlock)
    {
        if (string.IsNullOrEmpty(npcId)) return false;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return false;

        return npc.IsAvailable(timeBlock);
    }

    /// <summary>
    /// Get the schedule for an NPC.
    /// </summary>
    public NPCSchedule GetNPCSchedule(NPC npc)
    {
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        NPCSchedule schedule = new NPCSchedule
        {
            Npc = npc,
            Location = npc.Location,
            TimeSlots = new List<NPCTimeSlot>()
        };

        // Build schedule based on availability
        foreach (TimeBlocks timeBlock in Enum.GetValues<TimeBlocks>())
        {
            schedule.TimeSlots.Add(new NPCTimeSlot
            {
                TimeBlock = timeBlock,
                IsAvailable = npc.IsAvailable(timeBlock),
                Location = npc.Location
            });
        }

        return schedule;
    }

    /// <summary>
    /// Get all NPCs that will be at a location in the future.
    /// </summary>
    public List<FutureNPCFocus> GetFutureNPCFocus(Location location)
    {
        List<FutureNPCFocus> result = new List<FutureNPCFocus>();
        List<NPC> npcs = GetNPCsAtLocation(location);

        foreach (NPC npc in npcs)
        {
            foreach (TimeBlocks timeBlock in Enum.GetValues<TimeBlocks>())
            {
                if (npc.IsAvailable(timeBlock))
                {
                    result.Add(new FutureNPCFocus
                    {
                        // HIGHLANDER: Store NPC and Location objects, not IDs
                        Npc = npc,
                        TimeBlock = timeBlock,
                        Location = npc.Location
                    });
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Find where an NPC is currently located.
    /// </summary>
    public NPCLocation FindNPC(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return null;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        return new NPCLocation
        {
            // HIGHLANDER: Store NPC and Location objects, not IDs
            Npc = npc,
            Location = npc.Location,
            IsCurrentlyAvailable = npc.IsAvailable(_gameWorld.CurrentTimeBlock)
        };
    }
    /// <summary>
    /// Get NPCs by profession.
    /// </summary>
    public List<NPC> GetNPCsByProfession(Professions profession)
    {
        return _npcRepository.GetNPCsByProfession(profession);
    }

    /// <summary>
    /// Count NPCs at a location.
    /// </summary>
    public int CountNPCsAtLocation(Location location)
    {
        return GetNPCsAtLocation(location).Count;
    }

    /// <summary>
    /// Count NPCs at a location during a specific time.
    /// </summary>
    public int CountNPCsAtSpot(Location location, TimeBlocks timeBlock)
    {
        return GetNPCsAtSpot(location, timeBlock).Count;
    }

    /// <summary>
    /// Get time blocks when an NPC is available.
    /// </summary>
    public List<TimeBlocks> GetNPCAvailableTimes(string npcId)
    {
        List<TimeBlocks> availableTimes = new List<TimeBlocks>();

        if (string.IsNullOrEmpty(npcId)) return availableTimes;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return availableTimes;

        foreach (TimeBlocks timeBlock in Enum.GetValues<TimeBlocks>())
        {
            if (npc.IsAvailable(timeBlock))
            {
                availableTimes.Add(timeBlock);
            }
        }

        return availableTimes;
    }
}

/// <summary>
/// Represents an NPC's schedule.
/// HIGHLANDER: Object references only, no string IDs
/// </summary>
public class NPCSchedule
{
    public NPC Npc { get; set; }
    public Location Location { get; set; }
    public List<NPCTimeSlot> TimeSlots { get; set; }
}

/// <summary>
/// Represents an NPC's availability in a time slot.
/// HIGHLANDER: Object references only, no string IDs
/// </summary>
public class NPCTimeSlot
{
    public TimeBlocks TimeBlock { get; set; }
    public bool IsAvailable { get; set; }
    public Location Location { get; set; }
}

/// <summary>
/// Represents future NPC focus at a location.
/// HIGHLANDER: Object references only, no string IDs
/// </summary>
public class FutureNPCFocus
{
    public NPC Npc { get; set; }
    public TimeBlocks TimeBlock { get; set; }
    public Location Location { get; set; }
}

/// <summary>
/// Represents an NPC's current location.
/// HIGHLANDER: Object references only, no string IDs
/// </summary>
public class NPCLocation
{
    public NPC Npc { get; set; }
    public Location Location { get; set; }
    public bool IsCurrentlyAvailable { get; set; }
}
