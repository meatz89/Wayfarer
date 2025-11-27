
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
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly ContentGenerationFacade _contentGenerationFacade;
    private readonly PackageLoader _packageLoader;

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
        SceneFacade sceneFacade,
        SceneInstantiator sceneInstantiator,
        ContentGenerationFacade contentGenerationFacade,
        PackageLoader packageLoader)
    {
        _gameWorld = gameWorld;
        _locationManager = locationManager;
        _spotManager = spotManager;
        _movementValidator = movementValidator;
        _npcTracker = npcTracker;
        _actionManager = actionManager;
        _narrativeGenerator = narrativeGenerator;
        _observationSystem = observationSystem;
        _routeRepository = routeRepository;
        _npcRepository = npcRepository;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _dialogueGenerator = dialogueGenerator;
        _narrativeRenderer = narrativeRenderer;
        _difficultyService = difficultyService ?? throw new ArgumentNullException(nameof(difficultyService));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _sceneFacade = sceneFacade ?? throw new ArgumentNullException(nameof(sceneFacade));
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _contentGenerationFacade = contentGenerationFacade ?? throw new ArgumentNullException(nameof(contentGenerationFacade));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
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
    /// HIGHLANDER: Accepts Location object, not string identifier
    /// TWO-PHASE SPAWNING: Activates deferred scenes when player enters location
    /// INITIAL PLACEMENT: Handles first-time placement when player has no current location
    /// </summary>
    public async Task<bool> MoveToSpot(Location targetLocation)
    {
        // Validation
        if (targetLocation == null)
        {
            _messageSystem.AddSystemMessage("Invalid location", SystemMessageTypes.Warning);
            return false;
        }

        // Get current state
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = GetCurrentLocation();

        // INITIAL PLACEMENT: Handle first-time placement when player has no current location yet
        bool isInitialPlacement = (currentLocation == null);

        if (!isInitialPlacement)
        {
            // REGULAR MOVEMENT: Validate current state and movement
            Venue currentVenue = currentLocation.Venue;

            if (!_movementValidator.ValidateCurrentState(player, currentVenue, currentLocation))
            {
                _messageSystem.AddSystemMessage("Cannot determine current location", SystemMessageTypes.Danger);
                return false;
            }

            // Check if already at target
            if (currentLocation == targetLocation)
            {
                return true; // Already there - no-op success
            }

            // Validate movement
            MovementValidationResult validationResult = _movementValidator.ValidateMovement(currentVenue, currentLocation, targetLocation);
            if (!validationResult.IsValid)
            {
                _messageSystem.AddSystemMessage(validationResult.ErrorMessage, SystemMessageTypes.Warning);
                return false;
            }
        }

        // Execute movement (works for both initial placement and regular movement)
        _locationManager.SetCurrentSpot(targetLocation);
        player.AddKnownLocation(targetLocation);

        if (!isInitialPlacement)
        {
            _messageSystem.AddSystemMessage($"Moved to {targetLocation.Name}", SystemMessageTypes.Info);
        }

        // TWO-PHASE SPAWNING - PHASE 2: Activate deferred scenes when player enters location
        // Check for deferred scenes at this location and activate them (spawn dependent resources)
        await CheckAndActivateDeferredScenes(targetLocation);

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
            // HIGHLANDER: Pass Location object, not string
            viewModel.Observations = GetLocationObservations(location);

            // Add areas within location
            viewModel.AreasWithinLocation = _spotManager.GetAreasWithinVenue(venue, location, currentTime, _npcRepository);

            // Add routes to other locations
            viewModel.Routes = GetRoutesFromLocation(location);
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
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public List<NPC> GetNPCsAtLocation(Location location)
    {
        return _npcTracker.GetNPCsAtLocation(location);
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
        return _npcTracker.GetNPCsAtSpot(_gameWorld.GetPlayerCurrentLocation(), currentTime);
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
        List<NPCInteractionViewModel> result = new List<NPCInteractionViewModel>(); List<NPC> npcs = _npcRepository.GetNPCsForLocationAndTime(location, currentTime); foreach (NPC npc in npcs)
        {
            ConnectionState connectionState = GetNPCConnectionState(npc);
            List<InteractionOptionViewModel> interactions = new List<InteractionOptionViewModel>();

            // Find conversation options for this NPC if provided
            // HIGHLANDER: Name is natural key, no ID property
            NPCConversationOptions? npcOptions = npcConversationOptions.FirstOrDefault(opt => opt.NpcId == npc.Name);
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
                Npc = npc, // HIGHLANDER: Object reference, not ID
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
            ConversationType = conversationType
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

    // HIGHLANDER: Accept Location object, not string
    private List<ObservationViewModel> GetLocationObservations(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        List<ObservationViewModel> observations = new List<ObservationViewModel>();

        // HIGHLANDER: Pass Location object, not string names
        List<Observation> locationObservations = _observationSystem.GetObservationsForLocation(location);
        if (locationObservations.Count > 0)
        {
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            int currentSegment = _timeManager.CurrentSegment;
            List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationAndTime(location, currentTimeBlock);

            foreach (Observation obs in locationObservations)
            {
                // For observations that require specific NPCs, only show if NPC is present
                // HIGHLANDER: Direct object comparison, no ID extraction
                if (obs.Automatic == true && obs.RelevantNPCs?.Any() == true)
                {
                    bool hasNpcAtSpot = obs.RelevantNPCs.Any(npc => npcsAtCurrentSpot.Contains(npc));
                    if (!hasNpcAtSpot) continue;
                }

                observations.Add(new ObservationViewModel
                {
                    Observation = obs, // HIGHLANDER: Object reference, not ID
                    Text = obs.Text,
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
            // HIGHLANDER: obs.RelevantNPCs is List<NPC>, directly access Name for display
            string npcs = string.Join(", ", obs.RelevantNPCs.Select(npc => npc.Name));

            if (obs.CreatesState.HasValue)
                return $"→ {npcs} ({obs.CreatesState.Value})";
            else
                return $"→ {npcs}";
        }
        return "";
    }

    private List<RouteOptionViewModel> GetRoutesFromLocation(Location location)
    {
        List<RouteOptionViewModel> routes = new List<RouteOptionViewModel>();
        IEnumerable<RouteOption> availableRoutes = _routeRepository.GetRoutesFromLocation(location);

        foreach (RouteOption route in availableRoutes)
        {
            Location destSpot = route.DestinationLocation;
            if (destSpot == null)
                throw new InvalidOperationException($"Destination location not found for route: {route.Name}");

            Venue destination = destSpot.Venue;
            if (destination == null)
                throw new InvalidOperationException($"Destination venue not found for location: {destSpot.Name}");

            routes.Add(new RouteOptionViewModel
            {
                Route = route, // HIGHLANDER: Object reference, not ID
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
        return _npcRepository.GetNPCsForLocationAndTime(location, _timeManager.GetCurrentTimeBlock()).Count();
    }

    /// <summary>
    /// TWO-PHASE SPAWNING - PHASE 2: Check for deferred scenes with location activation triggers
    /// Evaluates LocationActivationFilter using categorical matching (BEFORE entity resolution)
    /// Triggers dependent resource spawning and entity resolution
    /// Transitions Scene.State: Deferred → Active
    /// </summary>
    private async Task CheckAndActivateDeferredScenes(Location location)
    {
        Player player = _gameWorld.GetPlayer();

        // Find all deferred scenes with location activation filters matching this location
        // CRITICAL: Check LocationActivationFilter (activation trigger), NOT CurrentSituation.Location (not yet resolved)
        List<Scene> deferredScenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Deferred)
            .Where(s => s.LocationActivationFilter != null &&
                       LocationMatchesActivationFilter(location, s.LocationActivationFilter, player))
            .ToList();

        if (deferredScenes.Count == 0)
            return; // No deferred scenes triggered by this location

        foreach (Scene scene in deferredScenes)
        {
            Console.WriteLine($"[LocationFacade] Activating deferred scene '{scene.DisplayName}' triggered by location '{location.Name}'");

            // Construct activation context
            SceneSpawnContext activationContext = new SceneSpawnContext
            {
                Player = player,
                CurrentLocation = location,
                CurrentVenue = location.Venue,
                CurrentNPC = null,
                CurrentRoute = null,
                CurrentSituation = null
            };

            // CORRECT ARCHITECTURE (from arc42/08_crosscutting_concepts.md §8.13-8.14):
            // ActivateScene is an INTEGRATED PROCESS that:
            // 1. Creates Situation instances from SituationTemplates
            // 2. Resolves entities (find-or-create) for each Situation
            // 3. Writes entity references to Situation instances
            // 4. Transitions state Deferred → Active
            // ALL IN ONE CALL - no separate entity resolution step

            Console.WriteLine($"[SceneActivation] Starting activation for scene '{scene.DisplayName}' (Template: {scene.TemplateId})");

            // INTEGRATED ACTIVATION: Creates Situations AND resolves entities in one call
            _sceneInstantiator.ActivateScene(scene, activationContext);

            Console.WriteLine($"[LocationFacade] ✅ Scene '{scene.DisplayName}' activated successfully (State: {scene.State})");
        }
    }

    /// <summary>
    /// Check if location matches activation filter using categorical properties
    /// Uses intentionally named enum properties: Privacy, Safety, Activity, Purpose
    /// Does NOT resolve entities - pure categorical matching (BEFORE entity resolution)
    /// </summary>
    private bool LocationMatchesActivationFilter(Location location, PlacementFilter filter, Player player)
    {
        // Check Privacy (if specified)
        if (filter.Privacy.HasValue)
        {
            if (location.Privacy != filter.Privacy.Value)
                return false;
        }

        // Check Safety (if specified)
        if (filter.Safety.HasValue)
        {
            if (location.Safety != filter.Safety.Value)
                return false;
        }

        // Check Activity (if specified)
        if (filter.Activity.HasValue)
        {
            if (location.Activity != filter.Activity.Value)
                return false;
        }

        // Check Purpose (if specified)
        if (filter.Purpose.HasValue)
        {
            if (location.Purpose != filter.Purpose.Value)
                return false;
        }

        return true; // All categorical checks passed
    }

    /// <summary>
    /// Configure dependent resources after PackageLoader loads them.
    /// Sets Origin = SceneCreated and Provenance for accessibility model (ADR-012).
    /// Also adds items to inventory if AddToInventoryOnCreation = true.
    ///
    /// HIGHLANDER: Creates locations PER-SITUATION via CreateSingleLocation - no JSON, no matching
    /// </summary>
    private void ConfigureDependentResources(Scene scene, PackageLoadResult loadResult, Player player, Venue contextVenue)
    {
        // Build provenance for newly created resources
        SceneProvenance provenance = new SceneProvenance
        {
            Scene = scene,
            CreatedDay = _timeManager.CurrentDay,
            CreatedTimeBlock = _timeManager.CurrentTimeBlock,
            CreatedSegment = _timeManager.CurrentSegment
        };

        // Get template for AddToInventoryOnCreation check
        SceneTemplate template = scene.Template ?? _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);

        // Configure dependent items (still from loadResult - items use JSON path)
        foreach (Item item in loadResult.ItemsAdded)
        {
            // Set provenance for forensic tracking
            item.Provenance = provenance;
        }

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

        if (player.Insight >= 2)
            approaches.Add(ObligationApproach.Systematic);

        if (player.Rapport >= 2)
            approaches.Add(ObligationApproach.LocalInquiry);

        if (player.Authority >= 2)
            approaches.Add(ObligationApproach.DemandAccess);

        if (player.Diplomacy >= 2)
            approaches.Add(ObligationApproach.PurchaseInfo);

        if (player.Cunning >= 2)
            approaches.Add(ObligationApproach.CovertSearch);

        return approaches;
    }

    /// <summary>
    /// OLD V2 Obligation - Stubbed out (replaced by V3 card-based system)
    /// HIGHLANDER: Accept Location object (stub only)
    /// </summary>
    /// <param name="location">Location where obligation takes place</param>
    /// <returns>Always returns false - V2 obligation system removed</returns>
    public bool InvestigateLocation(Location location)
    {
        return InvestigateLocation(location, ObligationApproach.Standard);
    }

    /// <summary>
    /// OLD V2 Obligation - Stubbed out (replaced by V3 card-based system)
    /// HIGHLANDER: Accept Location object (stub only)
    /// </summary>
    /// <param name="location">Location where obligation takes place</param>
    /// <param name="approach">Obligation approach to use</param>
    /// <returns>Always returns false - V2 obligation system removed</returns>
    public bool InvestigateLocation(Location location, ObligationApproach approach)
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
        Location spot = GetCurrentLocation();

        // ZERO NULL TOLERANCE: Fail-fast with clear diagnostic message
        if (spot == null)
        {
            throw new InvalidOperationException("CRITICAL: GetCurrentLocation() returned null. This indicates hex grid initialization failure. Check GameFacade.StartGameAsync() validation.");
        }

        Venue venue = spot.Venue;
        if (venue == null)
        {
            throw new InvalidOperationException($"CRITICAL: Location '{spot.Name}' has null Venue. This violates spatial scaffolding pattern where all locations must have venue assignment.");
        }

        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

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
                Situation = situation,
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
        List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot, currentTime);

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

        // Build traits from orthogonal categorical properties
        if (spot.Environment != default) traits.Add(spot.Environment.ToString());
        if (spot.Setting != default) traits.Add(spot.Setting.ToString());
        if (spot.Role != default) traits.Add(spot.Role.ToString());
        if (spot.Purpose != default) traits.Add(spot.Purpose.ToString());
        if (spot.Privacy != default) traits.Add(spot.Privacy.ToString());
        if (spot.Safety != default) traits.Add(spot.Safety.ToString());
        if (spot.Activity != default) traits.Add(spot.Activity.ToString());

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
            // Filter: Check if location has required role for this action
            if (action.RequiredLocationRole != null)
            {
                // Check if spot has required role
                if (spot.Role != action.RequiredLocationRole.Value)
                {
                    continue;  // Skip - location missing required role
                }
            }

            playerActions.Add(new LocationActionViewModel
            {
                Title = action.Name,
                Detail = action.Description,
                ActionType = action.ActionType.ToString().ToLower(),
                Cost = GetCostDisplay(action.Costs),
                IsAvailable = true
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
        // These are location-specific atmospheric actions generated from LocationCapability flags
        return actions.Where(a => a.ActionType != "travel").ToList();
    }

    private List<NpcWithSituationsViewModel> BuildNPCsWithSituations(Location spot, TimeBlocks currentTime)
    {
        List<NpcWithSituationsViewModel> result = new List<NpcWithSituationsViewModel>();

        // Get NPCs at spot
        List<NPC> npcsAtSpot = _npcTracker.GetNPCsAtSpot(spot, currentTime);
        Console.WriteLine($"[LocationFacade.BuildNPCsWithSituations] Found {npcsAtSpot.Count} NPCs at '{spot.Name}' during {currentTime}");

        // Build SIMPLE NPC cards for "Look Around" view
        // NPCs ALWAYS visible (physical presence), button conditional on scene availability
        foreach (NPC npc in npcsAtSpot)
        {
            ConnectionState connectionState = GetNPCConnectionState(npc);

            // Find ALL active scenes for this NPC (multi-scene display)
            // HIERARCHICAL PLACEMENT: Check CurrentSituation.Npc (situation owns placement)
            List<Scene> activeScenes = _gameWorld.Scenes.Where(s =>
                s.State == SceneState.Active &&
                s.CurrentSituation?.Npc != null &&
                s.CurrentSituation.Npc == npc).ToList(); // HIGHLANDER: Object equality

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
                Npc = npc, // HIGHLANDER: Object reference, not ID
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
        // HIGHLANDER: IsSatisfied signature changed to accept only player and gameWorld (markerMap removed)
        foreach (OrPath path in requirement.OrPaths)
        {
            if (!path.IsSatisfied(player, _gameWorld))
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

        // SCENE-SITUATION ARCHITECTURE: Query active Scenes at this location, get Situations from Scene.Situations
        // HIERARCHICAL PLACEMENT: Check CurrentSituation.Location (situation owns placement)
        List<Scene> scenesAtLocation = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active &&
                       s.CurrentSituation?.Location != null &&
                       s.CurrentSituation.Location == spot)
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
        // DOMAIN COLLECTION PRINCIPLE: Use List, not Dictionary
        List<(Scene Scene, List<Situation> Situations)> situationsByScene = new List<(Scene, List<Situation>)>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in systemSituations)
        {
            // Check if this situation belongs to an scene
            Scene parentScene = FindParentScene(spot, situation);

            if (parentScene != null)
            {
                // Find existing entry by object equality
                int existingIndex = -1;
                for (int i = 0; i < situationsByScene.Count; i++)
                {
                    if (situationsByScene[i].Scene == parentScene)
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    situationsByScene[existingIndex].Situations.Add(situation);
                }
                else
                {
                    situationsByScene.Add((parentScene, new List<Situation> { situation }));
                }
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach ((Scene scene, List<Situation> sceneSituations) in situationsByScene)
        {
            sceneGroups.Add(BuildSceneWithSituations(scene, sceneSituations, systemTypeStr, difficultyLabel));
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentScene(Location spot, Situation situation)
    {
        // Query GameWorld.Scenes by placement, check if situation matches
        // HIERARCHICAL PLACEMENT: Check if any situation in scene is at this location
        return _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .Where(s => s.Situations.Any(sit => sit.Location == spot))
            .FirstOrDefault(s => s.Situations.Contains(situation));
    }

    private ChallengeBuildResult GroupSituationsByScene(
        NPC npc, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        List<SituationCardViewModel> ambientSituations = new List<SituationCardViewModel>();
        List<SceneWithSituationsViewModel> sceneGroups = new List<SceneWithSituationsViewModel>();

        // Group situations by scene (ambient situations have no scene parent)
        // DOMAIN COLLECTION PRINCIPLE: Use List, not Dictionary
        List<(Scene Scene, List<Situation> Situations)> situationsByScene = new List<(Scene, List<Situation>)>();
        List<Situation> ambientSituationsList = new List<Situation>();

        foreach (Situation situation in situations)
        {
            // Check if this situation belongs to an scene from this NPC
            Scene parentScene = FindParentSceneForNPC(npc, situation);

            if (parentScene != null)
            {
                // Find existing entry by object equality
                int existingIndex = -1;
                for (int i = 0; i < situationsByScene.Count; i++)
                {
                    if (situationsByScene[i].Scene == parentScene)
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    situationsByScene[existingIndex].Situations.Add(situation);
                }
                else
                {
                    situationsByScene.Add((parentScene, new List<Situation> { situation }));
                }
            }
            else
            {
                ambientSituationsList.Add(situation);
            }
        }

        // Build ambient situations view models
        ambientSituations = ambientSituationsList.Select(g => BuildSituationCard(g, systemTypeStr, difficultyLabel)).ToList();

        // Build scene groups
        foreach ((Scene scene, List<Situation> sceneSituations) in situationsByScene)
        {
            sceneGroups.Add(BuildSceneWithSituations(scene, sceneSituations, systemTypeStr, difficultyLabel));
        }

        return new ChallengeBuildResult(ambientSituations, sceneGroups);
    }

    private Scene FindParentSceneForNPC(NPC npc, Situation situation)
    {
        // Query GameWorld.Scenes by placement type, check if situation matches
        // HIERARCHICAL PLACEMENT: Check if any situation in scene has this NPC
        // HIGHLANDER: Object equality, no ID comparison
        return _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .Where(s => s.Situations.Any(sit => sit.Npc == npc))
            .FirstOrDefault(s => s.Situations.Contains(situation));
    }

    private SceneWithSituationsViewModel BuildSceneWithSituations(Scene scene, List<Situation> situations, string systemTypeStr, string difficultyLabel)
    {
        return new SceneWithSituationsViewModel
        {
            Scene = scene,
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
            Situation = situation,
            Name = situation.Name,
            Description = situation.Description,
            SystemType = systemType,
            Type = situation.Type.ToString(),  // Copy from domain entity (Normal/Crisis)
            Difficulty = difficultyResult.FinalDifficulty,
            DifficultyLabel = difficultyLabel,
            Obligation = situation.Obligation,
            IsIntroAction = situation.Obligation != null,
            FocusCost = situation.Costs.Focus,
            StaminaCost = situation.Costs.Stamina
        };
    }

    private int GetBaseDifficultyForSituation(Situation situation)
    {
        switch (situation.SystemType)
        {
            case TacticalSystemType.Social:
                SocialChallengeDeck socialDeck = situation.Deck as SocialChallengeDeck;
                return socialDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Mental:
                MentalChallengeDeck mentalDeck = situation.Deck as MentalChallengeDeck;
                return mentalDeck?.DangerThreshold ?? 10;

            case TacticalSystemType.Physical:
                PhysicalChallengeDeck physicalDeck = situation.Deck as PhysicalChallengeDeck;
                return physicalDeck?.DangerThreshold ?? 10;

            default:
                return 10;
        }
    }
}
