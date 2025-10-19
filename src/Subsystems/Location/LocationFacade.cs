using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Public facade for all location-related operations.
/// Coordinates between Venue managers and provides a clean API for GameFacade.
/// </summary>
public class LocationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly LocationManager _locationManager;
    private readonly LocationManager _spotManager;
    private readonly MovementValidator _movementValidator;
    private readonly NPCLocationTracker _npcTracker;
    private readonly LocationActionManager _actionManager;
    private readonly LocationNarrativeGenerator _narrativeGenerator;

    // External dependencies for references
    private readonly ObservationSystem _observationSystem;
    // ObservationManager eliminated - observation system removed
    private readonly RouteRepository _routeRepository;
    private readonly NPCRepository _npcRepository;
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly DialogueGenerationService _dialogueGenerator;
    private readonly NarrativeRenderer _narrativeRenderer;
    private readonly ObstacleGoalFilter _obstacleGoalFilter;
    private readonly DifficultyCalculationService _difficultyService;
    private readonly ItemRepository _itemRepository;

    public LocationFacade(
        GameWorld gameWorld,
        LocationManager locationManager,
        LocationManager spotManager,
        MovementValidator movementValidator,
        NPCLocationTracker npcTracker,
        LocationActionManager actionManager,
        LocationNarrativeGenerator narrativeGenerator,
        ObservationSystem observationSystem,
        RouteRepository routeRepository,
        NPCRepository npcRepository,
        TimeManager timeManager,
        MessageSystem messageSystem,
        DialogueGenerationService dialogueGenerator,
        NarrativeRenderer narrativeRenderer,
        ObstacleGoalFilter obstacleGoalFilter,
        DifficultyCalculationService difficultyService,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _locationManager = locationManager;
        _spotManager = spotManager;
        _movementValidator = movementValidator;
        _npcTracker = npcTracker;
        _actionManager = actionManager;
        _narrativeGenerator = narrativeGenerator;
        _observationSystem = observationSystem;
        // ObservationManager eliminated
        _routeRepository = routeRepository;
        _npcRepository = npcRepository;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _dialogueGenerator = dialogueGenerator;
        _narrativeRenderer = narrativeRenderer;
        _obstacleGoalFilter = obstacleGoalFilter ?? throw new ArgumentNullException(nameof(obstacleGoalFilter));
        _difficultyService = difficultyService ?? throw new ArgumentNullException(nameof(difficultyService));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
    }

    /// <summary>
    /// Get the player's current location.
    /// </summary>
    public Venue GetCurrentLocation()
    {
        return _locationManager.GetCurrentLocation();
    }

    /// <summary>
    /// Get the player's current Venue location.
    /// </summary>
    public Location GetCurrentLocationSpot()
    {
        return _locationManager.GetCurrentLocationSpot();
    }

    /// <summary>
    /// Get a specific Venue by ID.
    /// </summary>
    public Venue GetLocationById(string venueId)
    {
        return _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
    }

    /// <summary>
    /// Move player to a different location within the current location.
    /// Movement between Locations within a Venue is FREE (no attention cost).
    /// </summary>
    public bool MoveToSpot(string spotName)
    {
        // Validation
        if (!_movementValidator.ValidateSpotName(spotName))
        {
            _messageSystem.AddSystemMessage("Invalid location name", SystemMessageTypes.Warning);
            return false;
        }

        // Get current state
        Player player = _gameWorld.GetPlayer();
        Venue currentLocation = GetCurrentLocation();
        Location currentSpot = GetCurrentLocationSpot();

        if (!_movementValidator.ValidateCurrentState(player, currentLocation, currentSpot))
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        // Check if already at target
        if (_movementValidator.IsAlreadyAtSpot(currentSpot, spotName))
        {
            return true; // Already there - no-op success
        }

        // Find target location by name
        List<Location> Locations = _spotManager.GetLocationsForVenue(currentLocation.Id);
        Location targetSpot = Locations.FirstOrDefault(s => s.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase));
        if (targetSpot == null)
        {
            _messageSystem.AddSystemMessage($"location '{spotName}' not found in {currentLocation.Name}", SystemMessageTypes.Warning);
            return false;
        }

        // Validate movement
        MovementValidationResult validationResult = _movementValidator.ValidateMovement(currentLocation, currentSpot, targetSpot);
        if (!validationResult.IsValid)
        {
            _messageSystem.AddSystemMessage(validationResult.ErrorMessage, SystemMessageTypes.Warning);
            return false;
        }

        // Execute movement
        _locationManager.SetCurrentSpot(targetSpot);
        player.AddKnownLocationSpot(targetSpot.Id);
        _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Info);

        return true;
    }

    /// <summary>
    /// Get the complete Venue screen view model with all Venue data.
    /// </summary>
    /// <param name="npcConversationOptions">List of NPCs with their available conversation types, provided by GameFacade from ConversationFacade</param>
    public LocationScreenViewModel GetLocationScreen(List<NPCConversationOptions> npcConversationOptions)
    {
        Player player = _gameWorld.GetPlayer();
        Venue venue = GetCurrentLocation();
        Location location = GetCurrentLocationSpot();

        if (venue == null)
            throw new InvalidOperationException("Current venue is null");
        if (location == null)
            throw new InvalidOperationException("Current location spot is null");

        LocationScreenViewModel viewModel = new LocationScreenViewModel
        {
            CurrentTime = _timeManager.GetFormattedTimeDisplay(),
            LocationPath = _spotManager.BuildLocationPath(venue, location),
            LocationName = venue.Name,
            CurrentSpotName = location.Name,
            LocationTraits = _spotManager.GetLocationTraits(venue, location, _timeManager.GetCurrentTimeBlock()),
            AtmosphereText = _narrativeGenerator.GenerateAtmosphereText(location, venue, _timeManager.GetCurrentTimeBlock(), GetNPCCountAtSpot(location)),
            QuickActions = new List<LocationActionViewModel>(),
            NPCsPresent = new List<NPCInteractionViewModel>(),
            Observations = new List<ObservationViewModel>(),
            AreasWithinLocation = new List<AreaWithinLocationViewModel>(),
            Routes = new List<RouteOptionViewModel>()
        };

        if (venue != null && location != null)
        {
            // Add location-specific actions
            viewModel.QuickActions = _actionManager.GetLocationActions(venue, location);

            // Add NPCs with connection states
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            viewModel.NPCsPresent = GetNPCsWithInteractions(location, currentTime, npcConversationOptions);

            // Add observations
            viewModel.Observations = GetLocationObservations(location.Id);

            // Add areas within location
            viewModel.AreasWithinLocation = _spotManager.GetAreasWithinVenue(venue, location, currentTime, _npcRepository);

            // Add routes to other locations
            viewModel.Routes = GetRoutesFromLocation(venue);
        }
        return viewModel;
    }

    /// <summary>
    /// Refresh the current Venue state.
    /// </summary>
    public void RefreshLocationState()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocation != null)
        {
            _messageSystem.AddSystemMessage("Location refreshed", SystemMessageTypes.Info);
        }
    }

    /// <summary>
    /// Get all NPCs at a specific location.
    /// </summary>
    public List<NPC> GetNPCsAtLocation(string locationId)
    {
        return _npcTracker.GetNPCsAtLocation(locationId);
    }

    /// <summary>
    /// Get all NPCs at the player's current location.
    /// </summary>
    public List<NPC> GetNPCsAtCurrentSpot()
    {
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocation == null)
            throw new InvalidOperationException("Player has no current location");

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return _npcTracker.GetNPCsAtSpot(player.CurrentLocation.Id, currentTime);
    }

    /// <summary>
    /// Get a specific NPC by ID.
    /// </summary>
    public NPC GetNPCById(string npcId)
    {
        // KEEP - npcId is external input from caller
        return _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
    }

    /// <summary>
    /// Get all NPCs in the game world.
    /// </summary>
    public List<NPC> GetAllNPCs()
    {
        return _gameWorld.NPCs;
    }

    // Private helper methods

    private List<NPCInteractionViewModel> GetNPCsWithInteractions(Location location, TimeBlocks currentTime, List<NPCConversationOptions> npcConversationOptions)
    {
        List<NPCInteractionViewModel> result = new List<NPCInteractionViewModel>(); List<NPC> npcs = _npcRepository.GetNPCsForLocationAndTime(location.Id, currentTime); foreach (NPC npc in npcs)
        {
            ConnectionState connectionState = GetNPCConnectionState(npc);
            List<InteractionOptionViewModel> interactions = new List<InteractionOptionViewModel>();

            // Find conversation options for this NPC if provided
            NPCConversationOptions? npcOptions = npcConversationOptions.FirstOrDefault(opt => opt.NpcId == npc.ID);
            if (npcOptions != null)
            {
                foreach (string conversationType in npcOptions.AvailableTypes)
                {
                    InteractionOptionViewModel interaction = GenerateConversationInteraction(npc, conversationType, connectionState);
                    interactions.Add(interaction);
                }
            }

            if (!interactions.Any())
            {
                interactions.Add(new InteractionOptionViewModel
                {
                    Text = "No interactions available",
                    Cost = "—"
                });
            }

            result.Add(new NPCInteractionViewModel
            {
                Id = npc.ID,
                Name = npc.Name,
                ConnectionStateName = connectionState.ToString(),
                Description = GetNPCDescription(npc, connectionState),
                Interactions = interactions
            });
        }

        return result;
    }

    private ConnectionState GetNPCConnectionState(NPC npc)
    {
        return ConnectionState.NEUTRAL;
    }

    private InteractionOptionViewModel GenerateConversationInteraction(NPC npc, string conversationType, ConnectionState connectionState)
    {
        // Generate interaction based on conversation type
        InteractionOptionViewModel interaction = new InteractionOptionViewModel
        {
            ConversationTypeId = conversationType
        };

        // Set display text based on type
        string displayText = conversationType switch
        {
            "friendly_chat" => "Friendly Chat",
            "request" => "Request", // Actual text comes from Goal.Name
            "delivery" => "Deliver Letter",
            "resolution" => "Make Amends",
            _ => "Talk"
        };
        interaction.Text = displayText;

        interaction.Cost = "—";

        return interaction;
    }

    private string GetNPCDescription(NPC npc, ConnectionState state)
    {
        string template = _dialogueGenerator.GenerateNPCDescription(npc, state);
        return _narrativeRenderer.RenderTemplate(template);
    }

    private List<ObservationViewModel> GetLocationObservations(string locationId)
    {
        List<ObservationViewModel> observations = new List<ObservationViewModel>();

        // Get location to derive venueId if needed
        Location location = _gameWorld.GetLocation(locationId);
        if (location == null)
            throw new InvalidOperationException($"Location not found: {locationId}");

        string venueId = location.VenueId;
        List<Observation> locationObservations = _observationSystem.GetObservationsForLocationSpot(venueId, locationId);
        if (locationObservations.Count > 0)
        {
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            int currentSegment = _timeManager.CurrentSegment;
            List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationAndTime(locationId, currentTimeBlock);
            List<string> npcIdsAtCurrentSpot = npcsAtCurrentSpot.Select(n => n.ID).ToList();

            foreach (Observation obs in locationObservations)
            {
                // For observations that require specific NPCs, only show if NPC is present
                if (obs.Automatic == true && obs.RelevantNPCs?.Any() == true)
                {
                    bool hasNpcAtSpot = obs.RelevantNPCs.Any(npcId => npcIdsAtCurrentSpot.Contains(npcId));
                    if (!hasNpcAtSpot) continue;
                }

                observations.Add(new ObservationViewModel
                {
                    Id = obs.Id,
                    Text = obs.Text,
                    Icon = obs.Type == ObservationType.Important ? "⚠️" : "👁️",
                    Relevance = BuildRelevanceString(obs),
                    IsObserved = false // ObservationManager eliminated
                });
            }
        }

        return observations;
    }

    private string BuildRelevanceString(Observation obs)
    {
        if (obs.RelevantNPCs != null && obs.RelevantNPCs.Any())
        {
            string npcs = string.Join(", ", obs.RelevantNPCs.Select(id =>
            {
                NPC npc = _npcRepository.GetById(id);
                if (npc == null)
                    throw new InvalidOperationException($"NPC not found: {id}");
                return npc.Name;
            }));

            if (obs.CreatesState.HasValue)
                return $"→ {npcs} ({obs.CreatesState.Value})";
            else
                return $"→ {npcs}";
        }
        return "";
    }

    private List<RouteOptionViewModel> GetRoutesFromLocation(Venue venue)
    {
        List<RouteOptionViewModel> routes = new List<RouteOptionViewModel>();
        IEnumerable<RouteOption> availableRoutes = _routeRepository.GetRoutesFromLocation(venue.Id);

        foreach (RouteOption route in availableRoutes)
        {
            Location destSpot = _gameWorld.GetLocation(route.DestinationLocationSpot);
            if (destSpot == null)
                throw new InvalidOperationException($"Destination location spot not found: {route.DestinationLocationSpot}");

            Venue destination = _locationManager.GetVenue(destSpot.VenueId);
            if (destination == null)
                throw new InvalidOperationException($"Destination venue not found: {destSpot.VenueId}");

            routes.Add(new RouteOptionViewModel
            {
                RouteId = route.Id,
                Destination = destination.Name,
                TravelTime = $"{route.TravelTimeSegments} seg",
                Detail = route.Description,
                IsLocked = false, // Core Loop: All routes visible
                LockReason = null,
                // Removed RequiredTier - route access is based on actual requirements in JSON
                TransportMethod = route.Method.ToString().ToLower(),
                SupportsCart = false,
                SupportsCarriage = false,
                Familiarity = null
            });
        }

        return routes;
    }

    private int GetNPCCountAtSpot(Location location)
    {
        if (location == null) return 0;
        return _npcRepository.GetNPCsForLocationAndTime(location.Id, _timeManager.GetCurrentTimeBlock()).Count();
    }

    /// <summary>
    /// Get the LocationActionManager for direct access to Venue actions.
    /// </summary>
    public LocationActionManager GetLocationActionManager()
    {
        return _actionManager;
    }

    /// <summary>
    /// Get available investigation approaches based on player stats
    /// </summary>
    public List<InvestigationApproach> GetAvailableApproaches(Player player)
    {
        List<InvestigationApproach> approaches = new List<InvestigationApproach> { InvestigationApproach.Standard };

        if (player.Stats.GetLevel(PlayerStatType.Insight) >= 2)
            approaches.Add(InvestigationApproach.Systematic);

        if (player.Stats.GetLevel(PlayerStatType.Rapport) >= 2)
            approaches.Add(InvestigationApproach.LocalInquiry);

        if (player.Stats.GetLevel(PlayerStatType.Authority) >= 2)
            approaches.Add(InvestigationApproach.DemandAccess);

        if (player.Stats.GetLevel(PlayerStatType.Diplomacy) >= 2)
            approaches.Add(InvestigationApproach.PurchaseInfo);

        if (player.Stats.GetLevel(PlayerStatType.Cunning) >= 2)
            approaches.Add(InvestigationApproach.CovertSearch);

        return approaches;
    }

    /// <summary>
    /// OLD V2 Investigation - Stubbed out (replaced by V3 card-based system)
    /// </summary>
    /// <param name="LocationId">ID of the location where investigation takes place</param>
    /// <returns>Always returns false - V2 investigation system removed</returns>
    public bool InvestigateLocation(string LocationId)
    {
        return InvestigateLocation(LocationId, InvestigationApproach.Standard);
    }

    /// <summary>
    /// OLD V2 Investigation - Stubbed out (replaced by V3 card-based system)
    /// </summary>
    /// <param name="LocationId">ID of the location where investigation takes place</param>
    /// <param name="approach">Investigation approach to use</param>
    /// <returns>Always returns false - V2 investigation system removed</returns>
    public bool InvestigateLocation(string LocationId, InvestigationApproach approach)
    {
        // V2 Investigation system removed - replaced by V3 card-based investigation
        _messageSystem.AddSystemMessage("Investigation system temporarily unavailable (transitioning to new system)", SystemMessageTypes.Warning);
        return false;
    }

    /// <summary>
    /// Get complete LocationContent view model with ALL data pre-built
    /// NO FILTERING/QUERYING IN UI - all logic here in backend
    /// </summary>
    public LocationContentViewModel GetLocationContentViewModel()
    {
        Player player = _gameWorld.GetPlayer();
        Venue venue = GetCurrentLocation();
        Location spot = GetCurrentLocationSpot();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        if (venue == null)
            throw new InvalidOperationException("Current venue is null");
        if (spot == null)
            throw new InvalidOperationException("Current location spot is null");

        LocationContentViewModel viewModel = new LocationContentViewModel
        {
            Header = BuildLocationHeader(venue, spot, currentTime),
            TravelActions = GetTravelActions(venue, spot),
            PlayerActions = GetPlayerActions(),
            HasSpots = GetSpotsForVenue(venue).Count > 1,
            NPCsWithGoals = BuildNPCsWithGoals(spot, currentTime),
            MentalGoals = BuildMentalGoals(spot),
            PhysicalGoals = BuildPhysicalGoals(spot),
            AvailableSpots = BuildSpotsWithNPCs(venue, spot, currentTime)
        };

        return viewModel;
    }

    // ============================================
    // PRIVATE HELPER METHODS - ALL UI LOGIC MOVED HERE
    // ============================================

    private LocationHeaderViewModel BuildLocationHeader(Venue venue, Location spot, TimeBlocks currentTime)
    {
        List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot.Id, currentTime);

        return new LocationHeaderViewModel
        {
            VenueName = venue.Name,
            SpotName = spot.Name,
            TimeOfDayTrait = GetTimeOfDayTrait(venue, currentTime),
            SpotTraits = BuildSpotTraits(spot),
            AtmosphereText = _narrativeGenerator.GenerateAtmosphereText(spot, venue, currentTime, npcsAtSpot.Count),
            CurrentTime = currentTime,
            NPCsPresent = npcsAtSpot
        };
    }

    private string GetTimeOfDayTrait(Venue venue, TimeBlocks currentTime)
    {
        string timeStr = currentTime switch
        {
            TimeBlocks.Morning => "Morning",
            TimeBlocks.Midday => "Midday",
            TimeBlocks.Afternoon => "Afternoon",
            TimeBlocks.Evening => "Evening",
            _ => "Unknown"
        };

        string modifier = GetLocationTimeModifier(venue, currentTime);
        return string.IsNullOrEmpty(modifier) ? timeStr : $"{timeStr}: {modifier}";
    }

    private string GetLocationTimeModifier(Venue venue, TimeBlocks currentTime)
    {
        string locationName = venue.Name?.ToLower() ?? "";

        if (locationName.Contains("market") || locationName.Contains("square"))
        {
            return currentTime switch
            {
                TimeBlocks.Morning => "Opening",
                TimeBlocks.Midday => "Busy",
                TimeBlocks.Afternoon => "Closing",
                TimeBlocks.Evening => "Empty",
                _ => ""
            };
        }

        if (locationName.Contains("tavern") || locationName.Contains("kettle"))
        {
            return currentTime switch
            {
                TimeBlocks.Morning => "Quiet",
                TimeBlocks.Midday => "Quiet",
                TimeBlocks.Afternoon => "Busy",
                TimeBlocks.Evening => "Lively",
                _ => ""
            };
        }

        if (locationName.Contains("noble") || locationName.Contains("manor"))
        {
            return currentTime switch
            {
                TimeBlocks.Morning => "Formal",
                TimeBlocks.Midday => "Active",
                TimeBlocks.Afternoon => "Reception",
                TimeBlocks.Evening => "Private",
                _ => ""
            };
        }

        return "";
    }

    private List<string> BuildSpotTraits(Location spot)
    {
        List<string> traits = new List<string>();

        if (spot.LocationProperties == null) return traits;

        foreach (LocationPropertyType prop in spot.LocationProperties)
        {
            string display = prop switch
            {
                LocationPropertyType.Private => "Private (+1 patience)",
                LocationPropertyType.Public => "Public (-1 patience)",
                LocationPropertyType.Discrete => "Discrete (+1 patience)",
                LocationPropertyType.Exposed => "Exposed (-1 patience)",
                LocationPropertyType.Crossroads => "Crossroads",
                LocationPropertyType.Commercial => "Commercial",
                LocationPropertyType.Quiet => "Quiet (+1 flow)",
                LocationPropertyType.Loud => "Loud (-1 flow)",
                LocationPropertyType.Warm => "Warm (+1 flow)",
                _ => prop.ToString()
            };
            traits.Add(display);
        }

        return traits;
    }

    private List<LocationActionViewModel> GetTravelActions(Venue venue, Location spot)
    {
        List<LocationActionViewModel> actions = _actionManager.GetLocationActions(venue, spot);
        return actions.Where(a => a.ActionType == "travel").ToList();
    }

    private List<LocationActionViewModel> GetPlayerActions()
    {
        List<LocationActionViewModel> playerActions = new List<LocationActionViewModel>();

        if (_gameWorld.PlayerActions == null)
            throw new InvalidOperationException("PlayerActions not initialized");

        foreach (PlayerAction action in _gameWorld.PlayerActions)
        {
            playerActions.Add(new LocationActionViewModel
            {
                Title = action.Name,
                Detail = action.Description,
                ActionType = action.ActionType.ToString().ToLower(),
                IsAvailable = true,
                Icon = ""
            });
        }

        return playerActions;
    }

    private List<NpcWithGoalsViewModel> BuildNPCsWithGoals(Location spot, TimeBlocks currentTime)
    {
        List<NpcWithGoalsViewModel> result = new List<NpcWithGoalsViewModel>();

        // Get NPCs at spot
        List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot.Id, currentTime);

        // Get ALL social goals at spot (filtered)
        List<Goal> allSocialGoals = GetFilteredSocialGoals(spot);

        // Build NPCs with their goals PRE-GROUPED
        foreach (NPC npc in npcsAtSpot)
        {
            ConnectionState connectionState = GetNPCConnectionState(npc);

            // Filter goals for THIS NPC
            List<Goal> npcGoals = allSocialGoals.Where(g => g.PlacementNpcId == npc.ID).ToList();

            result.Add(new NpcWithGoalsViewModel
            {
                Id = npc.ID,
                Name = npc.Name,
                PersonalityType = npc.PersonalityType.ToString(),
                ConnectionState = connectionState.ToString(),
                StateClass = GetConnectionStateClass(connectionState),
                Description = GetNPCDescriptionText(npc, connectionState),
                SocialGoals = npcGoals.Select(g => BuildGoalCard(g, "social", "Doubt")).ToList()
            });
        }

        return result;
    }

    private string GetConnectionStateClass(ConnectionState state)
    {
        return state switch
        {
            ConnectionState.DISCONNECTED => "disconnected",
            _ => ""
        };
    }

    private string GetNPCDescriptionText(NPC npc, ConnectionState state)
    {
        // Try actual NPC description from JSON first
        if (!string.IsNullOrEmpty(npc.Description))
            return npc.Description;

        // Fallback to generated description
        string template = _dialogueGenerator.GenerateNPCDescription(npc, state);
        return _narrativeRenderer.RenderTemplate(template);
    }

    private List<Goal> GetFilteredSocialGoals(Location spot)
    {
        List<Goal> allVisibleGoals = _obstacleGoalFilter.GetVisibleLocationGoals(spot, _gameWorld);

        return allVisibleGoals
            .Where(g => g.SystemType == TacticalSystemType.Social)
            .Where(g => g.IsAvailable && !g.IsCompleted)
            .Where(g => string.IsNullOrEmpty(g.InvestigationId))
            .ToList();
    }

    private List<GoalCardViewModel> BuildMentalGoals(Location spot)
    {
        List<Goal> allVisibleGoals = _obstacleGoalFilter.GetVisibleLocationGoals(spot, _gameWorld);

        List<Goal> mentalGoals = allVisibleGoals
            .Where(g => g.SystemType == TacticalSystemType.Mental)
            .Where(g => g.IsAvailable && !g.IsCompleted)
            .Where(g => string.IsNullOrEmpty(g.InvestigationId))
            .ToList();

        return mentalGoals.Select(g => BuildGoalCard(g, "mental", "Exposure")).ToList();
    }

    private List<GoalCardViewModel> BuildPhysicalGoals(Location spot)
    {
        List<Goal> allVisibleGoals = _obstacleGoalFilter.GetVisibleLocationGoals(spot, _gameWorld);

        List<Goal> physicalGoals = allVisibleGoals
            .Where(g => g.SystemType == TacticalSystemType.Physical)
            .Where(g => g.IsAvailable && !g.IsCompleted)
            .Where(g => string.IsNullOrEmpty(g.InvestigationId))
            .ToList();

        return physicalGoals.Select(g => BuildGoalCard(g, "physical", "Danger")).ToList();
    }

    private GoalCardViewModel BuildGoalCard(Goal goal, string systemType, string difficultyLabel)
    {
        DifficultyResult difficultyResult = _difficultyService.CalculateDifficulty(goal, _itemRepository);

        return new GoalCardViewModel
        {
            Id = goal.Id,
            Name = goal.Name,
            Description = goal.Description,
            SystemType = systemType,
            Difficulty = difficultyResult.FinalDifficulty,
            DifficultyLabel = difficultyLabel,
            InvestigationId = goal.InvestigationId,
            IsIntroAction = !string.IsNullOrEmpty(goal.InvestigationId),
            FocusCost = goal.Costs.Focus,
            StaminaCost = goal.Costs.Stamina
        };
    }

    private List<SpotWithNpcsViewModel> BuildSpotsWithNPCs(Venue venue, Location currentSpot, TimeBlocks currentTime)
    {
        List<SpotWithNpcsViewModel> spots = new List<SpotWithNpcsViewModel>();

        IEnumerable<Location> allSpots = _gameWorld.Locations.Where(s => s.VenueId == venue.Id);

        foreach (Location spot in allSpots)
        {
            List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot.Id, currentTime);

            spots.Add(new SpotWithNpcsViewModel
            {
                Id = spot.Name,
                Name = spot.Name,
                IsCurrentSpot = spot.Id == currentSpot.Id,
                NPCs = npcsAtSpot.Select(npc => new NpcAtSpotViewModel
                {
                    Name = npc.Name,
                    ConnectionState = GetNPCConnectionState(npc).ToString()
                }).ToList()
            });
        }

        return spots;
    }

    private List<Location> GetSpotsForVenue(Venue venue)
    {
        return _gameWorld.Locations.Where(s => s.VenueId == venue.Id).ToList();
    }
}
