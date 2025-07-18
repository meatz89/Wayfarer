public class NPCRepository
{
    private readonly GameWorld _gameWorld;

    public NPCRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;

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

    public NPC GetNPCById(string id)
    {
        return _gameWorld.WorldState.GetCharacters()?.FirstOrDefault(n => n.ID == id);
    }

    public List<NPC> GetAllNPCs()
    {
        return _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
    }

    public List<NPC> GetNPCsForLocation(string locationId)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return npcs.Where(n => n.Location == locationId).ToList();
    }

    public List<NPC> GetAvailableNPCs(TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return npcs.Where(n => n.IsAvailable(currentTime)).ToList();
    }

    public List<NPC> GetNPCsByProfession(Professions profession)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return npcs.Where(n => n.Profession == profession).ToList();
    }

    public List<NPC> GetNPCsProvidingService(ServiceTypes service)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return npcs.Where(n => n.ProvidedServices.Contains(service)).ToList();
    }

    public List<NPC> GetNPCsForLocationAndTime(string locationId, TimeBlocks currentTime)
    {
        List<NPC> npcs = _gameWorld.WorldState.GetCharacters() ?? new List<NPC>();
        return npcs.Where(n => n.Location == locationId && n.IsAvailable(currentTime)).ToList();
    }

    /// <summary>
    /// Gets NPCs available at a specific location spot and time
    /// </summary>
    public List<NPC> GetNPCsForLocationSpotAndTime(string locationSpotId, TimeBlocks currentTime)
    {
        List<LocationSpot> spots = _gameWorld.WorldState.locationSpots ?? new List<LocationSpot>();
        LocationSpot spot = spots.FirstOrDefault(s => s.SpotID == locationSpotId);

        if (spot?.PrimaryNPC != null && spot.PrimaryNPC.IsAvailable(currentTime))
        {
            return new List<NPC> { spot.PrimaryNPC };
        }

        return new List<NPC>();
    }

    /// <summary>
    /// Gets the primary NPC for a specific location spot if available at the current time
    /// </summary>
    public NPC GetPrimaryNPCForSpot(string locationSpotId, TimeBlocks currentTime)
    {
        List<LocationSpot> spots = _gameWorld.WorldState.locationSpots ?? new List<LocationSpot>();
        LocationSpot spot = spots.FirstOrDefault(s => s.SpotID == locationSpotId);

        if (spot?.PrimaryNPC != null && spot.PrimaryNPC.IsAvailable(currentTime))
        {
            return spot.PrimaryNPC;
        }

        return null;
    }

    /// <summary>
    /// Get time block service planning data for UI display
    /// </summary>
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        var timeBlockPlan = new List<TimeBlockServiceInfo>();
        var allTimeBlocks = Enum.GetValues<TimeBlocks>();
        var locationNPCs = GetNPCsForLocation(locationId);

        foreach (var timeBlock in allTimeBlocks)
        {
            var availableNPCs = locationNPCs.Where(npc => npc.IsAvailable(timeBlock)).ToList();
            var availableServices = availableNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();

            timeBlockPlan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableNPCs = availableNPCs,
                AvailableServices = availableServices,
                IsCurrentTimeBlock = timeBlock == _gameWorld.TimeManager.GetCurrentTimeBlock()
            });
        }

        return timeBlockPlan;
    }

    /// <summary>
    /// Get all unique services available at a location across all time blocks
    /// </summary>
    public List<ServiceTypes> GetAllLocationServices(string locationId)
    {
        var locationNPCs = GetNPCsForLocation(locationId);
        return locationNPCs.SelectMany(npc => npc.ProvidedServices).Distinct().ToList();
    }

    /// <summary>
    /// Get service availability summary for a specific service across all time blocks
    /// </summary>
    public ServiceAvailabilityPlan GetServiceAvailabilityPlan(string locationId, ServiceTypes service)
    {
        var allTimeBlocks = Enum.GetValues<TimeBlocks>();
        var locationNPCs = GetNPCsForLocation(locationId);
        var serviceProviders = locationNPCs.Where(npc => npc.ProvidedServices.Contains(service)).ToList();

        var availableTimeBlocks = new List<TimeBlocks>();
        foreach (var timeBlock in allTimeBlocks)
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
}