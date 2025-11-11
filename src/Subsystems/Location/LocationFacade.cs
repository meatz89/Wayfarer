
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
    private readonly DifficultyCalculationService _difficultyService;
    private readonly ItemRepository _itemRepository;
    private readonly SceneFacade _sceneFacade;

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
        DifficultyCalculationService difficultyService,
        ItemRepository itemRepository,
        SceneFacade sceneFacade)
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
        _difficultyService = difficultyService ?? throw new ArgumentNullException(nameof(difficultyService));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _sceneFacade = sceneFacade ?? throw new ArgumentNullException(nameof(sceneFacade));
    }

    /// <summary>
    /// Get the player's current Location (spatial position).
    /// Venue is derived: GetCurrentLocation().Venue
    /// </summary>
    public Location GetCurrentLocation()
    {
        return _locationManager.GetCurrentLocation();
    }

    /// <summary>
    /// Move player to a different Location within the current Venue.
    /// Movement between Locations within a Venue is FREE (no attention cost).
    /// </summary>
    public bool MoveToSpot(string locationId)
    {
        // Validation
        if (string.IsNullOrEmpty(locationId))
        {
            _messageSystem.AddSystemMessage("Invalid location ID", SystemMessageTypes.Warning);
            return false;
        }

        // Get current state
        Player player = _gameWorld.GetPlayer();
        Venue currentVenue = GetCurrentLocation().Venue;
        Location currentLocation = GetCurrentLocation();

        if (!_movementValidator.ValidateCurrentState(player, currentVenue, currentLocation))
        {
            _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
            return false;
        }

        // Check if already at target
        if (currentLocation.Id == locationId)
        {
            return true; // Already there - no-op success
        }

        // Find target location by ID (HIGHLANDER: runtime lookups use ID only)
        List<Location> Locations = _spotManager.GetLocationsForVenue(currentVenue.Id);
        Location targetSpot = Locations.FirstOrDefault(s => s.Id == locationId);
        if (targetSpot == null)
        {
            _messageSystem.AddSystemMessage($"Location ID '{locationId}' not found in {currentVenue.Name}", SystemMessageTypes.Warning);
            return false;
        }

        // Validate movement
        MovementValidationResult validationResult = _movementValidator.ValidateMovement(currentVenue, currentLocation, targetSpot);
        if (!validationResult.IsValid)
        {
            _messageSystem.AddSystemMessage(validationResult.ErrorMessage, SystemMessageTypes.Warning);
            return false;
        }

        // Execute movement
        _locationManager.SetCurrentSpot(targetSpot);
        player.AddKnownLocation(targetSpot.Id);
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
        Venue venue = GetCurrentLocation().Venue;
        Location location = GetCurrentLocation();

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
        if (_gameWorld.GetPlayerCurrentLocation() != null)
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
        if (_gameWorld.GetPlayerCurrentLocation() == null)
            throw new InvalidOperationException("Player has no current location");

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        return _npcTracker.GetNPCsAtSpot(_gameWorld.GetPlayerCurrentLocation().Id, currentTime);
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
            "request" => "Request", // Actual text comes from Situation.Name
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
        List<Observation> locationObservations = _observationSystem.GetObservationsForLocation(venueId, locationId);
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
            Location destSpot = _gameWorld.GetLocation(route.DestinationLocationId);
            if (destSpot == null)
                throw new InvalidOperationException($"Destination location spot not found: {route.DestinationLocationId}");

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
    /// Get available obligation approaches based on player stats
    /// </summary>
    public List<ObligationApproach> GetAvailableApproaches(Player player)
    {
        List<ObligationApproach> approaches = new List<ObligationApproach> { ObligationApproach.Standard };

        if (player.Stats.GetLevel(PlayerStatType.Insight) >= 2)
            approaches.Add(ObligationApproach.Systematic);

        if (player.Stats.GetLevel(PlayerStatType.Rapport) >= 2)
            approaches.Add(ObligationApproach.LocalInquiry);

        if (player.Stats.GetLevel(PlayerStatType.Authority) >= 2)
            approaches.Add(ObligationApproach.DemandAccess);

        if (player.Stats.GetLevel(PlayerStatType.Diplomacy) >= 2)
            approaches.Add(ObligationApproach.PurchaseInfo);

        if (player.Stats.GetLevel(PlayerStatType.Cunning) >= 2)
            approaches.Add(ObligationApproach.CovertSearch);

        return approaches;
    }

    /// <summary>
    /// OLD V2 Obligation - Stubbed out (replaced by V3 card-based system)
    /// </summary>
    /// <param name="LocationId">ID of the location where obligation takes place</param>
    /// <returns>Always returns false - V2 obligation system removed</returns>
    public bool InvestigateLocation(string LocationId)
    {
        return InvestigateLocation(LocationId, ObligationApproach.Standard);
    }

    /// <summary>
    /// OLD V2 Obligation - Stubbed out (replaced by V3 card-based system)
    /// </summary>
    /// <param name="LocationId">ID of the location where obligation takes place</param>
    /// <param name="approach">Obligation approach to use</param>
    /// <returns>Always returns false - V2 obligation system removed</returns>
    public bool InvestigateLocation(string LocationId, ObligationApproach approach)
    {
        // V2 Obligation system removed - replaced by V3 card-based obligation
        _messageSystem.AddSystemMessage("Obligation system temporarily unavailable (transitioning to new system)", SystemMessageTypes.Warning);
        return false;
    }

    /// <summary>
    /// Get complete LocationContent view model with ALL data pre-built
    /// NO FILTERING/QUERYING IN UI - all logic here in backend
    /// </summary>
    public LocationContentViewModel GetLocationContentViewModel()
    {
        Player player = _gameWorld.GetPlayer();
        Venue venue = GetCurrentLocation().Venue;
        Location spot = GetCurrentLocation();
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

        if (venue == null)
            throw new InvalidOperationException("Current venue is null");
        if (spot == null)
            throw new InvalidOperationException("Current location spot is null");

        ChallengeBuildResult mentalResult = BuildMentalChallenges(spot);
        List<SituationCardViewModel> ambientMental = mentalResult.AmbientSituations;
        List<SceneWithSituationsViewModel> mentalScenes = mentalResult.SceneGroups;
        ChallengeBuildResult physicalResult = BuildPhysicalChallenges(spot);
        List<SituationCardViewModel> ambientPhysical = physicalResult.AmbientSituations;
        List<SceneWithSituationsViewModel> physicalScenes = physicalResult.SceneGroups;

        // SCENE-SITUATION ARCHITECTURE: Locked situations handled by RequirementFormula in ChoiceTemplate
        // Scene-based situations have requirements checked at action instantiation time
        // For now, locked situations displayed as empty (UI shows available situations only)
        List<LockedSituationViewModel> lockedSituations = new List<LockedSituationViewModel>();

        LocationContentViewModel viewModel = new LocationContentViewModel
        {
            Header = BuildLocationHeader(venue, spot, currentTime),
            TravelActions = GetTravelActions(venue, spot),
            LocationSpecificActions = GetLocationSpecificActions(venue, spot),
            PlayerActions = GetPlayerActions(spot),
            // REMOVED: HasSpots (intra-venue movement now data-driven from LocationActionCatalog)
            NPCsWithSituations = BuildNPCsWithSituations(spot, currentTime),
            AmbientMentalSituations = ambientMental,
            MentalScenes = mentalScenes,
            AmbientPhysicalSituations = ambientPhysical,
            PhysicalScenes = physicalScenes,
            LockedSituations = lockedSituations
        };

        return viewModel;
    }

    /// <summary>
    /// Map SceneDisplay locked situations to view models with strongly-typed requirement gaps
    /// </summary>
    private List<LockedSituationViewModel> MapLockedSituations(LocationSceneDisplay sceneDisplay)
    {
        if (sceneDisplay == null || sceneDisplay.LockedSituations.Count == 0)
            return new List<LockedSituationViewModel>();

        List<LockedSituationViewModel> viewModels = new List<LockedSituationViewModel>();

        foreach (SituationWithLockReason lockedSituation in sceneDisplay.LockedSituations)
        {
            Situation situation = lockedSituation.Situation;

            LockedSituationViewModel vm = new LockedSituationViewModel
            {
                SituationId = situation.Id,
                Name = situation.Name,
                Description = situation.Description,
                SystemType = situation.SystemType.ToString().ToLower(),
                LockReason = lockedSituation.LockReason,

                // Map strongly-typed requirement gaps
                UnmetBonds = lockedSituation.UnmetBonds,
                UnmetScales = lockedSituation.UnmetScales,
                UnmetResolve = lockedSituation.UnmetResolve,
                UnmetCoins = lockedSituation.UnmetCoins,
                UnmetSituationCount = lockedSituation.UnmetSituationCount,
                UnmetAchievements = lockedSituation.UnmetAchievements,
                UnmetStates = lockedSituation.UnmetStates
            };

            viewModels.Add(vm);
        }

        return viewModels;
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

    /// <summary>
    /// Get time-of-day atmosphere modifier based on venue type.
    /// Uses strongly-typed VenueType enum (no string matching).
    /// </summary>
    private string GetLocationTimeModifier(Venue venue, TimeBlocks currentTime)
    {
        return venue.Type switch
        {
            VenueType.Market => currentTime switch
            {
                TimeBlocks.Morning => "Opening",
                TimeBlocks.Midday => "Busy",
                TimeBlocks.Afternoon => "Closing",
                TimeBlocks.Evening => "Empty",
                _ => ""
            },

            VenueType.Tavern => currentTime switch
            {
                TimeBlocks.Morning => "Quiet",
                TimeBlocks.Midday => "Quiet",
                TimeBlocks.Afternoon => "Busy",
                TimeBlocks.Evening => "Lively",
                _ => ""
            },

            VenueType.NobleDistrict => currentTime switch
            {
                TimeBlocks.Morning => "Formal",
                TimeBlocks.Midday => "Active",
                TimeBlocks.Afternoon => "Reception",
                TimeBlocks.Evening => "Private",
                _ => ""
            },

            _ => ""  // Other venue types have no time modifier
        };
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

    private List<LocationActionViewModel> GetPlayerActions(Location spot)
    {
        List<LocationActionViewModel> playerActions = new List<LocationActionViewModel>();

        if (_gameWorld.PlayerActions == null)
            throw new InvalidOperationException("PlayerActions not initialized");

        // Sort by Priority (ascending - lower number = higher priority)
        List<PlayerAction> sortedActions = _gameWorld.PlayerActions
            .OrderBy(action => action.Priority)
            .ThenBy(action => action.Name)
            .ToList();

        foreach (PlayerAction action in sortedActions)
        {
            // Filter: Check if location has ALL required properties for this action
            if (action.RequiredLocationProperties.Count > 0)
            {
                bool hasAllRequiredProperties = action.RequiredLocationProperties
                    .All(required => spot.LocationProperties.Contains(required));

                if (!hasAllRequiredProperties)
                {
                    continue;  // Skip - location missing required properties
                }
            }

            playerActions.Add(new LocationActionViewModel
            {
                Title = action.Name,
                Detail = action.Description,
                ActionType = action.ActionType.ToString().ToLower(),
                Cost = GetCostDisplay(action.Costs),
                IsAvailable = true,
                Icon = ""
            });
        }

        return playerActions;
    }

    /// <summary>
    /// Get display string for action costs (shared between LocationActions and PlayerActions)
    /// </summary>
    private string GetCostDisplay(ActionCosts costs)
    {
        List<string> costParts = new List<string>();

        if (costs.Coins > 0)
            costParts.Add($"{costs.Coins} coins");

        if (costs.Focus > 0)
            costParts.Add($"{costs.Focus} focus");

        if (costs.Stamina > 0)
            costParts.Add($"{costs.Stamina} stamina");

        if (costs.Health > 0)
            costParts.Add($"{costs.Health} health");

        if (costParts.Count == 0)
            return "Free!";

        return string.Join(", ", costParts);
    }

    private List<LocationActionViewModel> GetLocationSpecificActions(Venue venue, Location spot)
    {
        List<LocationActionViewModel> actions = _actionManager.GetLocationActions(venue, spot);
        // Return non-travel location actions (rest, work, secure room, food, etc.)
        // These are location-specific atmospheric actions generated from LocationPropertyTypes
        return actions.Where(a => a.ActionType != "travel").ToList();
    }

    private List<NpcWithSituationsViewModel> BuildNPCsWithSituations(Location spot, TimeBlocks currentTime)
    {
        List<NpcWithSituationsViewModel> result = new List<NpcWithSituationsViewModel>();

        // Get NPCs at spot
        List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot.Id, currentTime);
        Console.WriteLine($"[LocationFacade.BuildNPCsWithSituations] Found {npcsAtSpot.Count} NPCs at '{spot.Id}' during {currentTime}");

        // Build SIMPLE NPC cards for "Look Around" view
        // NPCs ALWAYS visible (physical presence), button conditional on scene availability
        foreach (NPC npc in npcsAtSpot)
        {
            ConnectionState connectionState = GetNPCConnectionState(npc);

            // Find ALL active scenes for this NPC (multi-scene display)
            List<Scene> activeScenes = _gameWorld.Scenes.Where(s =>
                s.State == SceneState.Active &&
                s.PlacementType == PlacementType.NPC &&
                s.PlacementId == npc.ID).ToList();

            // Build scene view model for each active scene
            List<NpcSceneViewModel> availableScenes = new List<NpcSceneViewModel>();
            foreach (Scene scene in activeScenes)
            {
                // Derive label using fallback hierarchy: DisplayName → FirstSituation.Name → Placeholder
                string label = null;
                if (!string.IsNullOrEmpty(scene.DisplayName))
                {
                    label = scene.DisplayName;
                }
                else if (scene.Situations.Any())
                {
                    Situation firstSituation = scene.Situations.First();
                    if (firstSituation != null && !string.IsNullOrEmpty(firstSituation.Name))
                    {
                        label = firstSituation.Name;
                    }
                }

                // Fallback to placeholder if no label found (playability over aesthetics)
                if (string.IsNullOrEmpty(label))
                {
                    label = $"Talk to {npc.Name}";
                }

                availableScenes.Add(new NpcSceneViewModel
                {
                    Scene = scene,
                    Label = label,
                    Description = scene.IntroNarrative
                });
            }

            NpcWithSituationsViewModel viewModel = new NpcWithSituationsViewModel
            {
                Id = npc.ID,
                Name = npc.Name,
                PersonalityType = npc.PersonalityType.ToString(),
                ConnectionState = connectionState.ToString(),
                StateClass = GetConnectionStateClass(connectionState),
                Description = GetNPCDescriptionText(npc, connectionState),
                HasExchange = npc.HasExchangeCards(),
                ExchangeDescription = npc.HasExchangeCards() ? "Trading - Buy supplies and equipment" : null,
                AvailableScenes = availableScenes
            };

            result.Add(viewModel);
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

    /// <summary>
    /// Generate human-readable lock reason from CompoundRequirement
    /// Sir Brante pattern: Show player WHY choice is locked (visible requirements)
    /// Returns formatted string like "Requires Authority 3+" or "Requires Bond 15+ OR Morality +8"
    /// </summary>
    private string GenerateLockReason(CompoundRequirement requirement, Player player)
    {
        if (requirement == null || requirement.OrPaths == null || requirement.OrPaths.Count == 0)
            return "Requirements not met";

        // Collect all path labels from unmet paths
        List<string> pathLabels = new List<string>();

        // NOTE: No marker resolution here - this is UI display only, not actual requirement checking
        // Actual requirement checking happens elsewhere with proper marker map context
        Dictionary<string, string> emptyMarkerMap = new Dictionary<string, string>();

        foreach (OrPath path in requirement.OrPaths)
        {
            if (!path.IsSatisfied(player, _gameWorld, emptyMarkerMap))
            {
                // Use path label if available, otherwise generate from requirements
                if (!string.IsNullOrEmpty(path.Label))
                {
                    pathLabels.Add(path.Label);
                }
                else if (path.NumericRequirements != null && path.NumericRequirements.Count > 0)
                {
                    // Use first requirement's label as fallback
                    NumericRequirement firstReq = path.NumericRequirements.First();
                    if (!string.IsNullOrEmpty(firstReq.Label))
                    {
                        pathLabels.Add(firstReq.Label);
                    }
                }
            }
        }

        if (pathLabels.Count == 0)
            return "Requirements not met";
        else if (pathLabels.Count == 1)
            return $"Requires {pathLabels[0]}";
        else
            return $"Requires {string.Join(" OR ", pathLabels)}";
    }

    private ChallengeBuildResult BuildMentalChallenges(Location spot)
    {
        return BuildChallengesBySystemType(spot, TacticalSystemType.Mental, "mental", "Exposure");
    }

    private ChallengeBuildResult BuildPhysicalChallenges(Location spot)
    {
        return BuildChallengesBySystemType(spot, TacticalSystemType.Physical, "physical", "Danger");
    }

    private ChallengeBuildResult BuildChallengesBySystemType(
        Location spot, TacticalSystemType systemType, string systemTypeStr, string difficultyLabel)
    {
        List<SituationCardViewModel> ambientSituations = new List<SituationCardViewModel>();
        List<SceneWithSituationsViewModel> sceneGroups = new List<SceneWithSituationsViewModel>();

        // SCENE-SITUATION ARCHITECTURE: Query active Scenes at this location, get Situation IDs, query GameWorld.Situations
        List<Scene> scenesAtLocation = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.PlacementType == PlacementType.Location &&
                       s.PlacementId == spot.Id)
            .ToList();

        // Get all situations from scenes at this location (direct object ownership)
        List<Situation> allVisibleSituations = scenesAtLocation
            .SelectMany(scene => scene.Situations)
            .ToList();

        // Filter to this system type only
        // NOTE: Obligation situations ARE included - they may have parent scenes for hierarchical display
        List<Situation> systemSituations = allVisibleSituations
            .Where(g => g.SystemType == systemType)
            .Where(g => g.IsAvailable && !g.IsCompleted)
            .ToList();

        // Group situations by scene (ambient situations have no scene parent)
        Dictionary<string, List<Situation>> situationsByScene = new Dictionary<string, List<Situation>>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in systemSituations)
        {
            // Check if this situation belongs to an scene
            Scene parentScene = FindParentScene(spot, situation);

            if (parentScene != null)
            {
                if (!situationsByScene.ContainsKey(parentScene.Id))
                {
                    situationsByScene[parentScene.Id] = new List<Situation>();
                }
                situationsByScene[parentScene.Id].Add(situation);
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach (KeyValuePair<string, List<Situation>> kvp in situationsByScene)
        {
            Scene scene = _gameWorld.Scenes.FirstOrDefault(o => o.Id == kvp.Key);
            if (scene != null)
            {
                sceneGroups.Add(BuildSceneWithSituations(scene, kvp.Value, systemTypeStr, difficultyLabel));
            }
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentScene(Location spot, Situation situation)
    {
        // Query GameWorld.Scenes by placement, check if SituationIds contains this situation.Id
        return _gameWorld.Scenes
            .Where(s => s.PlacementType == PlacementType.Location)
            .Where(s => s.PlacementId == spot.Id)
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.Situations.Any(sit => sit.Id == situation.Id));
    }

    private ChallengeBuildResult GroupSituationsByScene(
        NPC npc, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        List<SituationCardViewModel> ambientSituations = new List<SituationCardViewModel>();
        List<SceneWithSituationsViewModel> sceneGroups = new List<SceneWithSituationsViewModel>();

        // Group situations by scene (ambient situations have no scene parent)
        Dictionary<string, List<Situation>> situationsByScene = new Dictionary<string, List<Situation>>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in situations)
        {
            // Check if this situation belongs to an scene from this NPC
            Scene parentScene = FindParentSceneForNPC(npc, situation);

            if (parentScene != null)
            {
                if (!situationsByScene.ContainsKey(parentScene.Id))
                {
                    situationsByScene[parentScene.Id] = new List<Situation>();
                }
                situationsByScene[parentScene.Id].Add(situation);
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach (KeyValuePair<string, List<Situation>> kvp in situationsByScene)
        {
            Scene scene = _gameWorld.Scenes.FirstOrDefault(o => o.Id == kvp.Key);
            if (scene != null)
            {
                sceneGroups.Add(BuildSceneWithSituations(scene, kvp.Value, systemTypeStr, difficultyLabel));
            }
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentSceneForNPC(NPC npc, Situation situation)
    {
        // Query GameWorld.Scenes by placement type and ID, check if SituationIds contains this situation.Id
        return _gameWorld.Scenes
            .Where(s => s.PlacementType == PlacementType.NPC)
            .Where(s => s.PlacementId == npc.ID)
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.Situations.Any(sit => sit.Id == situation.Id));
    }

    private SceneWithSituationsViewModel BuildSceneWithSituations(Scene scene, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        return new SceneWithSituationsViewModel
        {
            Id = scene.Id,
            Name = scene.DisplayName,
            Description = scene.IntroNarrative,
            Intensity = 0,  // Intensity removed from Scene - defaulting to 0
            Contexts = new List<string>(),  // Contexts removed from Scene
            ContextsDisplay = "",  // Contexts removed from Scene
            Situations = situations.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList()
        };
    }

    private SituationCardViewModel BuildSituationCard(Situation situation, string systemType, string difficultyLabel)
    {
        int baseDifficulty = GetBaseDifficultyForSituation(situation);
        DifficultyResult difficultyResult = _difficultyService.CalculateDifficulty(situation, baseDifficulty, _itemRepository);

        return new SituationCardViewModel
        {
            Id = situation.Id,
            Name = situation.Name,
            Description = situation.Description,
            SystemType = systemType,
            Type = situation.Type.ToString(),  // Copy from domain entity (Normal/Crisis)
            Difficulty = difficultyResult.FinalDifficulty,
            DifficultyLabel = difficultyLabel,
            ObligationId = situation.Obligation?.Id,
            IsIntroAction = !string.IsNullOrEmpty(situation.Obligation?.Id),
            FocusCost = situation.Costs.Focus,
            StaminaCost = situation.Costs.Stamina
        };
    }

    private int GetBaseDifficultyForSituation(Situation situation)
    {
        switch (situation.SystemType)
        {
            case TacticalSystemType.Social:
                SocialChallengeDeck socialDeck = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId);
                return socialDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Mental:
                MentalChallengeDeck mentalDeck = _gameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId);
                return mentalDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Physical:
                PhysicalChallengeDeck physicalDeck = _gameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId);
                return physicalDeck?.DangerThreshold ?? 10;

            default:
                return 10;
        }
    }
}
