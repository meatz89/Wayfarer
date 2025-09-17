


public class NPCRepository
{
    private readonly GameWorld _gameWorld;
    private readonly DebugLogger _debugLogger;
    private readonly NPCVisibilityService _visibilityService;

    public NPCRepository(
        GameWorld gameWorld,
        DebugLogger debugLogger,
        NPCVisibilityService visibilityService)
    {
        _gameWorld = gameWorld;
        _debugLogger = debugLogger; // Can be null during initialization
        _visibilityService = visibilityService;

        if (_gameWorld.WorldState.GetCharacters() == null)
        {
            Console.WriteLine("WARNING: No NPCs collection exists in WorldState.");
        }

        if (!_gameWorld.WorldState.GetCharacters().Any())
        {
            Console.WriteLine("INFO: No NPCs loaded from GameWorld. This may be expected if npcs.json is not available.");
        }
    }

    #region Read Methods

    /// <summary>
    /// Checks if an NPC should be visible based on registered visibility rules
    /// </summary>
    private bool IsNPCVisible(NPC npc)
    {
        return _visibilityService.IsNPCVisible(npc.ID);
    }

    /// <summary>
    /// Filters a list of NPCs based on visibility rules
    /// </summary>
    private List<NPC> FilterByVisibility(List<NPC> npcs)
    {
        return _visibilityService.FilterVisibleNPCs(npcs);
    }

    public NPC GetById(string id)
    {
        NPC? npc = _gameWorld.WorldState.GetCharacters()?.FirstOrDefault(n => n.ID == id);
        if (npc != null && !IsNPCVisible(npc))
            return null;

        return npc;
    }


    public NPC GetByName(string name)
    {
        NPC? npc = _gameWorld.WorldState.GetCharacters()?.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (npc != null && !IsNPCVisible(npc))
            return null;
        return npc;
    }

    public List<NPC> GetAllNPCs()
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return FilterByVisibility(npcs);
    }

    public List<NPC> GetNPCsForLocation(string locationId)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        List<NPC> locationNpcs = npcs.Where(n => n.Location == locationId).ToList();
        return FilterByVisibility(locationNpcs);
    }

    public List<NPC> GetAvailableNPCs(TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        List<NPC> availableNpcs = npcs.Where(n => n.IsAvailable(currentTime)).ToList();
        return FilterByVisibility(availableNpcs);
    }

    public List<NPC> GetNPCsByProfession(Professions profession)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        List<NPC> professionNpcs = npcs.Where(n => n.Profession == profession).ToList();
        return FilterByVisibility(professionNpcs);
    }

    public List<NPC> GetNPCsProvidingService(ServiceTypes service)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        List<NPC> serviceNpcs = npcs.Where(n => n.ProvidedServices.Contains(service)).ToList();
        return FilterByVisibility(serviceNpcs);
    }

    public List<NPC> GetNPCsForLocationAndTime(string locationId, TimeBlocks currentTime)
    {
        // Return all NPCs at location, regardless of availability
        // UI will handle whether they're interactable based on availability
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        List<NPC> locationNpcs = npcs.Where(n => n.Location == locationId).ToList();
        return FilterByVisibility(locationNpcs);
    }

    /// <summary>
    /// Gets NPCs available at a specific location spot and time
    /// </summary>
    public List<NPC> GetNPCsForLocationSpotAndTime(string locationSpotId, TimeBlocks currentTime)
    {
        // Return all NPCs at this spot, regardless of availability
        // UI will handle whether they're interactable based on availability
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>(); ;

        _debugLogger?.LogNPCActivity("GetNPCsForLocationSpotAndTime", null,
            $"Looking for NPCs at spot '{locationSpotId}' during {currentTime}");

        List<NPC> npcsAtSpot = npcs.Where(n => n.SpotId == locationSpotId).ToList();

        // Apply visibility filtering
        npcsAtSpot = FilterByVisibility(npcsAtSpot);

        _debugLogger?.LogDebug($"Found {npcsAtSpot.Count} NPCs at spot '{locationSpotId}' (after visibility filtering): " +
            string.Join(", ", npcsAtSpot.Select(n => $"{n.Name} ({n.ID}) - Available: {n.IsAvailable(currentTime)}")));

        return npcsAtSpot;
    }

    /// <summary>
    /// Gets the primary NPC for a specific location spot if available at the current time
    /// </summary>
    public NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        NPC? npc = npcs.FirstOrDefault(n => n.SpotId == locationSpotId && n.IsAvailable(currentTime));
        if (npc != null && !IsNPCVisible(npc))
            return null;
        return npc;
    }

    /// <summary>
    /// Get time block service planning data for UI display
    /// </summary>
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        // GetNPCsForLocation already applies visibility filtering
        List<NPC> locationNPCs = GetNPCsForLocation(locationId);

        foreach (TimeBlocks timeBlock in allTimeBlocks)
        {
            List<NPC> availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();
            List<ServiceTypes> availableServices = availableNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();

            timeBlockPlan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableNPCs = availableNPCs,
                AvailableServices = availableServices,
                IsCurrentTimeBlock = timeBlock == _gameWorld.CurrentTimeBlock
            });
        }

        return timeBlockPlan;
    }

    /// <summary>
    /// Get all unique services available at a location across all time blocks
    /// </summary>
    public List<ServiceTypes> GetAllLocationServices(string locationId)
    {
        List<NPC> locationNPCs = GetNPCsForLocation(locationId);
        return locationNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();
    }

    /// <summary>
    /// Get service availability summary for a specific service across all time blocks
    /// </summary>
    public ServiceAvailabilityPlan GetServiceAvailabilityPlan(string locationId, ServiceTypes service)
    {
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = GetNPCsForLocation(locationId);
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

    #endregion

    #region Write Methods

    public void AddNPC(NPC npc)
    {
        if (GetAllNPCs().Any(n => n.ID == npc.ID))
        {
            throw new InvalidOperationException($"NPC with ID '{npc.ID}' already exists.");
        }

        _gameWorld.WorldState.AddCharacter(npc);
    }

    public void AddNPCs(IEnumerable<NPC> npcs)
    {
        foreach (NPC npc in npcs)
        {
            AddNPC(npc);
        }
    }

    public bool RemoveNPC(string id)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            return false;
        }

        NPC npc = npcs.FirstOrDefault(n => n.ID == id);
        if (npc != null)
        {
            return npcs.Remove(npc);
        }

        return false;
    }

    public void UpdateNPC(NPC npc)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("No NPCs collection exists.");
        }

        NPC existingNPC = npcs.FirstOrDefault(n => n.ID == npc.ID);
        if (existingNPC == null)
        {
            throw new InvalidOperationException($"NPC with ID '{npc.ID}' not found.");
        }

        int index = npcs.IndexOf(existingNPC);
        npcs[index] = npc;
    }

    public void ClearAllNPCs()
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs != null)
        {
            npcs.Clear();
        }
    }

    #endregion

    #region Request Card Resolution

    /// <summary>
    /// Get request cards from an NPCRequest by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetRequestCards(NPCRequest request)
    {
        if (request == null) return new List<ConversationCard>();
        return request.GetRequestCards(_gameWorld);
    }

    /// <summary>
    /// Get promise cards from an NPCRequest by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetPromiseCards(NPCRequest request)
    {
        if (request == null) return new List<ConversationCard>();
        return request.GetPromiseCards(_gameWorld);
    }

    #endregion
}
