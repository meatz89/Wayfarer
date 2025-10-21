using System;
using System.Collections.Generic;
using System.Linq;

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
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string venueId)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = _repository.GetNPCsForLocation(venueId).ToList();

        foreach (TimeBlocks timeBlock in allTimeBlocks)
        {
            List<NPC> availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();
            List<ServiceTypes> availableServices = availableNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();

            timeBlockPlan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableNPCs = availableNPCs,
                AvailableServices = availableServices,
                IsCurrentTimeBlock = timeBlock == _timeManager.GetCurrentTimeBlock()
            });
        }

        return timeBlockPlan;
    }

    /// <summary>
    /// Get all unique services available at a Venue across all time blocks
    /// </summary>
    public List<ServiceTypes> GetAllLocationServices(string venueId)
    {
        IEnumerable<NPC> locationNPCs = _repository.GetNPCsForLocation(venueId);
        return locationNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();
    }

    /// <summary>
    /// Get service availability summary for a specific service across all time blocks
    /// </summary>
    public ServiceAvailabilityPlan GetServiceAvailabilityPlan(string venueId, ServiceTypes service)
    {
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = _repository.GetNPCsForLocation(venueId).ToList();
        List<NPC> serviceProviders = locationNPCs.Where(npc => npc.ProvidedServices.Contains(service)).ToList();

        List<TimeBlocks> availableTimeBlocks = new List<TimeBlocks>();
        foreach (TimeBlocks timeBlock in allTimeBlocks)
        {
            if (serviceProviders.Any(npc => npc.IsAvailable(timeBlock)))
            {
                availableTimeBlocks.Add(timeBlock);
            }
        }

        return new ServiceAvailabilityPlan
        {
            Service = service,
            AvailableTimeBlocks = availableTimeBlocks,
            ServiceProviders = serviceProviders
        };
    }

    /// <summary>
    /// Find NPCs that can provide a specific service at the current time
    /// </summary>
    public IEnumerable<NPC> GetAvailableServiceProviders(ServiceTypes service, string venueId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        IEnumerable<NPC> npcs = string.IsNullOrWhiteSpace(venueId)
            ? _repository.GetAvailableNPCs(currentTime)
            : _repository.GetNPCsForLocationAndTime(venueId, currentTime);

        return npcs.Where(n => n.CanProvideService(service));
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
    public IEnumerable<NPCInteractionInfo> GetAvailableInteractions(string locationSpotId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        IEnumerable<NPC> npcs = _repository.GetNPCsForLocationAndTime(locationSpotId, currentTime);

        return npcs.Select(npc => new NPCInteractionInfo
        {
            NPC = npc,
            IsAvailable = npc.IsAvailable(currentTime),
            AvailableServices = npc.IsAvailable(currentTime) ? npc.ProvidedServices : new List<ServiceTypes>(),
            UnavailableReason = !npc.IsAvailable(currentTime) ? "NPC is not available at this time" : null
        });
    }

    internal NPC GetNPCById(string npcId)
    {
        return _repository.GetById(npcId);
    }
}

/// <summary>
/// Information about an NPC interaction opening
/// </summary>
public class NPCInteractionInfo
{
    public NPC NPC { get; set; }
    public bool IsAvailable { get; set; }
    public List<ServiceTypes> AvailableServices { get; set; }
    public string UnavailableReason { get; set; }
}