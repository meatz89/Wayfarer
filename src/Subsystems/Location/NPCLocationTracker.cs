using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks NPC positions and availability across locations and spots.
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
    public List<NPC> GetNPCsAtLocation(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<NPC>();

        // Use NPCRepository which handles visibility filtering
        return _npcRepository.GetNPCsForLocation(venueId);
    }

    /// <summary>
    /// Get NPCs at a Venue during a specific time block.
    /// </summary>
    public List<NPC> GetNPCsAtLocationAndTime(string venueId, TimeBlocks timeBlock)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<NPC>();

        // Use NPCRepository method
        return _npcRepository.GetNPCsForLocationAndTime(venueId, timeBlock);
    }

    /// <summary>
    /// Get NPCs at a specific spot during a time block.
    /// </summary>
    public List<NPC> GetNPCsAtSpot(string spotId, TimeBlocks timeBlock)
    {
        if (string.IsNullOrEmpty(spotId)) return new List<NPC>();

        // Use NPCRepository method for spot-specific NPCs
        return _npcRepository.GetNPCsForLocationSpotAndTime(spotId, timeBlock);
    }

    /// <summary>
    /// Get the primary NPC for a spot if available.
    /// </summary>
    public NPC GetPrimaryNPCForSpot(string spotId, TimeBlocks timeBlock)
    {
        if (string.IsNullOrEmpty(spotId)) return null;

        // Use NPCRepository method
        return _npcRepository.GetPrimaryNPCForSpot(spotId, timeBlock);
    }

    /// <summary>
    /// Get all services available at a location.
    /// </summary>
    public List<ServiceTypes> GetAllLocationServices(string venueId)
    {
        if (string.IsNullOrEmpty(venueId)) return new List<ServiceTypes>();

        // Use NPCRepository method
        return _npcRepository.GetAllLocationServices(venueId);
    }

    /// <summary>
    /// Check if an NPC is present at a location.
    /// </summary>
    public bool IsNPCAtLocation(string npcId, string venueId)
    {
        if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(venueId)) return false;

        NPC npc = _npcRepository.GetById(npcId);
        return npc?.Venue?.Equals(venueId, StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Check if an NPC is at a specific spot.
    /// </summary>
    public bool IsNPCAtSpot(string npcId, string spotId)
    {
        if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(spotId)) return false;

        NPC npc = _npcRepository.GetById(npcId);
        return npc?.SpotId?.Equals(spotId, StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Check if an NPC is available at their Venue during a time block.
    /// </summary>
    public bool IsNPCAvailable(string npcId, TimeBlocks timeBlock)
    {
        if (string.IsNullOrEmpty(npcId)) return false;

        NPC npc = _npcRepository.GetById(npcId);
        return npc?.IsAvailable(timeBlock) == true;
    }

    /// <summary>
    /// Get the schedule for an NPC.
    /// </summary>
    public NPCSchedule GetNPCSchedule(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return null;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        NPCSchedule schedule = new NPCSchedule
        {
            NPCId = npcId,
            NPCName = npc.Name,
            VenueId = npc.Venue,
            SpotId = npc.SpotId,
            TimeSlots = new List<NPCTimeSlot>()
        };

        // Build schedule based on availability
        foreach (TimeBlocks timeBlock in Enum.GetValues<TimeBlocks>())
        {
            schedule.TimeSlots.Add(new NPCTimeSlot
            {
                TimeBlock = timeBlock,
                IsAvailable = npc.IsAvailable(timeBlock),
                VenueId = npc.Venue,
                SpotId = npc.SpotId
            });
        }

        return schedule;
    }

    /// <summary>
    /// Get all NPCs that will be at a Venue in the future.
    /// </summary>
    public List<FutureNPCFocus> GetFutureNPCFocus(string venueId)
    {
        List<FutureNPCFocus> result = new List<FutureNPCFocus>();
        List<NPC> npcs = GetNPCsAtLocation(venueId);

        foreach (NPC npc in npcs)
        {
            foreach (TimeBlocks timeBlock in Enum.GetValues<TimeBlocks>())
            {
                if (npc.IsAvailable(timeBlock))
                {
                    result.Add(new FutureNPCFocus
                    {
                        NPCId = npc.ID,
                        NPCName = npc.Name,
                        TimeBlock = timeBlock,
                        SpotId = npc.SpotId
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
            NPCId = npc.ID,
            NPCName = npc.Name,
            VenueId = npc.Venue,
            SpotId = npc.SpotId,
            IsCurrentlyAvailable = npc.IsAvailable(_gameWorld.CurrentTimeBlock)
        };
    }

    /// <summary>
    /// Get NPCs that provide a specific service.
    /// </summary>
    public List<NPC> GetNPCsProvidingService(ServiceTypes service)
    {
        return _npcRepository.GetNPCsProvidingService(service);
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
    public int CountNPCsAtLocation(string venueId)
    {
        return GetNPCsAtLocation(venueId).Count;
    }

    /// <summary>
    /// Count NPCs at a spot during a specific time.
    /// </summary>
    public int CountNPCsAtSpot(string spotId, TimeBlocks timeBlock)
    {
        return GetNPCsAtSpot(spotId, timeBlock).Count;
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
/// </summary>
public class NPCSchedule
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public string VenueId { get; set; }
    public string SpotId { get; set; }
    public List<NPCTimeSlot> TimeSlots { get; set; }
}

/// <summary>
/// Represents an NPC's availability in a time slot.
/// </summary>
public class NPCTimeSlot
{
    public TimeBlocks TimeBlock { get; set; }
    public bool IsAvailable { get; set; }
    public string VenueId { get; set; }
    public string SpotId { get; set; }
}

/// <summary>
/// Represents future NPC focus at a location.
/// </summary>
public class FutureNPCFocus
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public TimeBlocks TimeBlock { get; set; }
    public string SpotId { get; set; }
}

/// <summary>
/// Represents an NPC's current location.
/// </summary>
public class NPCLocation
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public string VenueId { get; set; }
    public string SpotId { get; set; }
    public bool IsCurrentlyAvailable { get; set; }
}
