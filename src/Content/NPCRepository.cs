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
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _debugLogger = debugLogger; // Optional - can be null during initialization, used for debugging
        _visibilityService = visibilityService ?? throw new ArgumentNullException(nameof(visibilityService));

        if (_gameWorld.GetCharacters() == null)
        { }

        if (!_gameWorld.GetCharacters().Any())
        { }
    }

    #region Read Methods

    /// <summary>
    /// Checks if an NPC should be visible based on registered visibility rules
    /// </summary>
    private bool IsNPCVisible(NPC npc)
    {
        // ADR-007: Use Name (natural key) instead of deleted ID property
        return _visibilityService.IsNPCVisible(npc.Name);
    }

    /// <summary>
    /// Filters a list of NPCs based on visibility rules
    /// </summary>
    private List<NPC> FilterByVisibility(List<NPC> npcs)
    {
        return _visibilityService.FilterVisibleNPCs(npcs);
    }

    // HIGHLANDER: GetById and GetByName DELETED - violate object reference architecture
    // Use object references throughout call chain instead of string lookup

    public List<NPC> GetAllNPCs()
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        return FilterByVisibility(npcs);
    }

    public List<NPC> GetNPCsForLocation(Location location)
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> locationNpcs = npcs.Where(n => n.Location == location).ToList();
        return FilterByVisibility(locationNpcs);
    }

    public List<NPC> GetAvailableNPCs(TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> availableNpcs = npcs.Where(n => n.IsAvailable(currentTime)).ToList();
        return FilterByVisibility(availableNpcs);
    }

    public List<NPC> GetNPCsByProfession(Professions profession)
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        List<NPC> professionNpcs = npcs.Where(n => n.Profession == profession).ToList();
        return FilterByVisibility(professionNpcs);
    }
    public List<NPC> GetNPCsForLocationAndTimeDeprecated(Location location, TimeBlocks currentTime)
    {
        // DEPRECATED: Use GetNPCsForLocationAndTime instead
        // This method is kept temporarily for compatibility
        return GetNPCsForLocationAndTime(location, currentTime);
    }

    /// <summary>
    /// Gets NPCs available at a specific location and time
    /// </summary>
    public List<NPC> GetNPCsForLocationAndTime(Location location, TimeBlocks currentTime)
    {
        // Return all NPCs at this location, regardless of availability
        // UI will handle whether they're interactable based on availability
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }

        // Optional - debugLogger might be null during initialization
        _debugLogger?.LogNPCActivity("GetNPCsForLocationAndTime", null,
            $"Looking for NPCs at location '{location.Name}' during {currentTime}");

        Console.WriteLine($"[NPCRepository] Checking {npcs.Count} NPCs for location '{location.Name}'");
        foreach (NPC npc in npcs)
        {
            // ADR-007: NPC.ID deleted - use Name for logging
            Console.WriteLine($"[NPCRepository]   NPC '{npc.Name}' - Location: {(npc.Location != null ? $"'{npc.Location.Name}'" : "NULL")}");
        }

        List<NPC> npcsAtLocation = npcs.Where(n => n.Location == location).ToList();

        // Apply visibility filtering
        npcsAtLocation = FilterByVisibility(npcsAtLocation);

        // ADR-007: NPC.ID deleted - use Name for logging
        _debugLogger?.LogDebug($"Found {npcsAtLocation.Count} NPCs at location '{location.Name}': " +
            string.Join(", ", npcsAtLocation.Select(n => $"{n.Name} - Available: {n.IsAvailable(currentTime)}")));

        return npcsAtLocation;
    }

    /// <summary>
    /// Gets the primary NPC for a specific location if available at the current time
    /// </summary>
    public NPC GetPrimaryNPCForSpot(Location location, TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("NPCs collection not initialized - data loading failed");
        }
        NPC? npc = npcs.FirstOrDefault(n => n.Location == location && n.IsAvailable(currentTime));
        if (npc != null && !IsNPCVisible(npc))
            return null;
        return npc;
    }

    /// <summary>
    /// Get time block service planning data for UI display
    /// </summary>
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(Location location)
    {
        List<TimeBlockServiceInfo> timeBlockPlan = new List<TimeBlockServiceInfo>();
        TimeBlocks[] allTimeBlocks = Enum.GetValues<TimeBlocks>();
        // GetNPCsForLocation already applies visibility filtering
        List<NPC> locationNPCs = GetNPCsForLocation(location);

        foreach (TimeBlocks timeBlock in allTimeBlocks)
        {
            List<NPC> availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();

            timeBlockPlan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableNPCs = availableNPCs,
                AvailableServices = new(),
                IsCurrentTimeBlock = timeBlock == _gameWorld.CurrentTimeBlock
            });
        }

        return timeBlockPlan;
    }
    #endregion

    #region Write Methods

    public void UpdateNPC(NPC npc)
    {
        List<NPC> npcs = _gameWorld.GetCharacters();
        if (npcs == null)
        {
            throw new InvalidOperationException("No NPCs collection exists.");
        }

        // ADR-007: Use object equality instead of string matching
        int index = npcs.IndexOf(npc);
        if (index == -1)
        {
            throw new InvalidOperationException($"NPC '{npc.Name}' not found in collection.");
        }

        npcs[index] = npc;
    }

    #endregion

    #region Request Card Resolution

    #endregion
}
