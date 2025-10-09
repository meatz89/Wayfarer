


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
        List<NPC> characters = _gameWorld.WorldState.GetCharacters();
        if (characters == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetById({id})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }

        NPC? npc = characters.FirstOrDefault(n => n.ID == id);
        if (npc != null && !IsNPCVisible(npc))
            return null;

        return npc;
    }


    public NPC GetByName(string name)
    {
        List<NPC> characters = _gameWorld.WorldState.GetCharacters();
        if (characters == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetByName({name})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }

        NPC? npc = characters.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (npc != null && !IsNPCVisible(npc))
            return null;
        return npc;
    }

    public List<NPC> GetAllNPCs()
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine("ERROR: Characters collection is null in GetAllNPCs");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        return FilterByVisibility(npcs);
    }

    public List<NPC> GetNPCsForLocation(string venueId)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetNPCsForLocation({venueId})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> locationNpcs = npcs.Where(n => n.Venue == venueId).ToList();
        return FilterByVisibility(locationNpcs);
    }

    public List<NPC> GetAvailableNPCs(TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetAvailableNPCs({currentTime})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> availableNpcs = npcs.Where(n => n.IsAvailable(currentTime)).ToList();
        return FilterByVisibility(availableNpcs);
    }

    public List<NPC> GetNPCsByProfession(Professions profession)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetNPCsByProfession({profession})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> professionNpcs = npcs.Where(n => n.Profession == profession).ToList();
        return FilterByVisibility(professionNpcs);
    }

    public List<NPC> GetNPCsProvidingService(ServiceTypes service)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetNPCsProvidingService({service})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> serviceNpcs = npcs.Where(n => n.ProvidedServices.Contains(service)).ToList();
        return FilterByVisibility(serviceNpcs);
    }

    public List<NPC> GetNPCsForVenueAndTime(string venueId, TimeBlocks currentTime)
    {
        // Return all NPCs at venue, regardless of availability
        // UI will handle whether they're interactable based on availability
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetNPCsForVenueAndTime({venueId}, {currentTime})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> venueNpcs = npcs.Where(n => n.Venue == venueId).ToList();
        return FilterByVisibility(venueNpcs);
    }

    /// <summary>
    /// Gets NPCs available at a specific Venue location and time
    /// </summary>
    public List<NPC> GetNPCsForLocationAndTime(string LocationId, TimeBlocks currentTime)
    {
        // Return all NPCs at this location, regardless of availability
        // UI will handle whether they're interactable based on availability
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetNPCsForLocationAndTime({LocationId}, {currentTime})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }

        _debugLogger?.LogNPCActivity("GetNPCsForLocationAndTime", null,
            $"Looking for NPCs at location '{LocationId}' during {currentTime}");

        List<NPC> npcsAtLocation = npcs.Where(n => n.LocationId == LocationId).ToList();

        // Apply visibility filtering
        npcsAtLocation = FilterByVisibility(npcsAtLocation);

        _debugLogger?.LogDebug($"Found {npcsAtLocation.Count} NPCs at location '{LocationId}': " +
            string.Join(", ", npcsAtLocation.Select(n => $"{n.Name} ({n.ID}) - Available: {n.IsAvailable(currentTime)}")));

        return npcsAtLocation;
    }

    /// <summary>
    /// Gets the primary NPC for a specific Venue location if available at the current time
    /// </summary>
    public NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters();
        if (npcs == null)
        {
            Console.WriteLine($"ERROR: Characters collection is null in GetPrimaryNPCForSpot({locationSpotId}, {currentTime})");
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        NPC? npc = npcs.FirstOrDefault(n => n.LocationId == locationSpotId && n.IsAvailable(currentTime));
        if (npc != null && !IsNPCVisible(npc))
            return null;
        return npc;
    }

    /// <summary>
    /// Get time block service planning data for UI display
    /// </summary>
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string venueId)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        // GetNPCsForLocation already applies visibility filtering
        List<NPC> locationNPCs = GetNPCsForLocation(venueId);

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
    /// Get all unique services available at a Venue across all time blocks
    /// </summary>
    public List<ServiceTypes> GetAllLocationServices(string venueId)
    {
        List<NPC> locationNPCs = GetNPCsForLocation(venueId);
        return locationNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();
    }

    /// <summary>
    /// Get service availability summary for a specific service across all time blocks
    /// </summary>
    public ServiceAvailabilityPlan GetServiceAvailabilityPlan(string venueId, ServiceTypes service)
    {
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        List<NPC> locationNPCs = GetNPCsForLocation(venueId);
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


    #endregion

    #region Request Card Resolution

    #endregion
}
