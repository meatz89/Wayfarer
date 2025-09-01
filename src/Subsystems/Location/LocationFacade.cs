using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.NarrativeSubsystem;

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Represents conversation options available for a specific NPC
    /// </summary>
    public class NPCConversationOptions
    {
        public string NpcId { get; set; }
        public string NpcName { get; set; }
        public List<ConversationType> AvailableTypes { get; set; } = new List<ConversationType>();
        public int AttentionCost { get; set; }
        public bool CanAfford { get; set; }
    }
    /// <summary>
    /// Public facade for all location-related operations.
    /// Coordinates between location managers and provides a clean API for GameFacade.
    /// </summary>
    public class LocationFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly LocationManager _locationManager;
        private readonly LocationSpotManager _spotManager;
        private readonly MovementValidator _movementValidator;
        private readonly NPCLocationTracker _npcTracker;
        private readonly LocationActionManager _actionManager;
        private readonly LocationNarrativeGenerator _narrativeGenerator;
        
        // External dependencies for references
        private readonly ObservationSystem _observationSystem;
        private readonly ObservationManager _observationManager;
        private readonly RouteRepository _routeRepository;
        private readonly NPCRepository _npcRepository;
        private readonly TimeManager _timeManager;
        private readonly MessageSystem _messageSystem;
        private readonly ObligationQueueManager _letterQueueManager;
        private readonly DialogueGenerationService _dialogueGenerator;
        private readonly NarrativeRenderer _narrativeRenderer;
        
        public LocationFacade(
            GameWorld gameWorld,
            LocationManager locationManager,
            LocationSpotManager spotManager,
            MovementValidator movementValidator,
            NPCLocationTracker npcTracker,
            LocationActionManager actionManager,
            LocationNarrativeGenerator narrativeGenerator,
            ObservationSystem observationSystem,
            ObservationManager observationManager,
            RouteRepository routeRepository,
            NPCRepository npcRepository,
            TimeManager timeManager,
            MessageSystem messageSystem,
            ObligationQueueManager letterQueueManager,
            DialogueGenerationService dialogueGenerator,
            NarrativeRenderer narrativeRenderer)
        {
            _gameWorld = gameWorld;
            _locationManager = locationManager;
            _spotManager = spotManager;
            _movementValidator = movementValidator;
            _npcTracker = npcTracker;
            _actionManager = actionManager;
            _narrativeGenerator = narrativeGenerator;
            _observationSystem = observationSystem;
            _observationManager = observationManager;
            _routeRepository = routeRepository;
            _npcRepository = npcRepository;
            _timeManager = timeManager;
            _messageSystem = messageSystem;
            _letterQueueManager = letterQueueManager;
            _dialogueGenerator = dialogueGenerator;
            _narrativeRenderer = narrativeRenderer;
        }
        
        /// <summary>
        /// Get the player's current location.
        /// </summary>
        public Location GetCurrentLocation()
        {
            return _locationManager.GetCurrentLocation();
        }
        
        /// <summary>
        /// Get the player's current location spot.
        /// </summary>
        public LocationSpot GetCurrentLocationSpot()
        {
            return _locationManager.GetCurrentLocationSpot();
        }
        
        /// <summary>
        /// Move player to a different spot within the current location.
        /// Movement between spots within a location is FREE (no attention cost).
        /// </summary>
        public bool MoveToSpot(string spotName)
        {
            // Validation
            if (!_movementValidator.ValidateSpotName(spotName))
            {
                _messageSystem.AddSystemMessage("Invalid spot name", SystemMessageTypes.Warning);
                return false;
            }
            
            // Get current state
            Player player = _gameWorld.GetPlayer();
            Location currentLocation = GetCurrentLocation();
            LocationSpot currentSpot = GetCurrentLocationSpot();
            
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
            
            // Find target spot
            LocationSpot targetSpot = _spotManager.FindSpotInLocation(currentLocation, spotName);
            if (targetSpot == null)
            {
                _messageSystem.AddSystemMessage($"Spot '{spotName}' not found in {currentLocation.Name}", SystemMessageTypes.Warning);
                return false;
            }
            
            // Validate movement
            var validationResult = _movementValidator.ValidateMovement(currentLocation, currentSpot, targetSpot);
            if (!validationResult.IsValid)
            {
                _messageSystem.AddSystemMessage(validationResult.ErrorMessage, SystemMessageTypes.Warning);
                return false;
            }
            
            // Execute movement
            _locationManager.SetCurrentSpot(targetSpot);
            player.AddKnownLocationSpot(targetSpot.SpotID);
            _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Info);
            
            return true;
        }
        
        /// <summary>
        /// Get the complete location screen view model with all location data.
        /// </summary>
        /// <param name="npcConversationOptions">List of NPCs with their available conversation types, provided by GameFacade from ConversationFacade</param>
        public LocationScreenViewModel GetLocationScreen(List<NPCConversationOptions> npcConversationOptions = null)
        {
            Console.WriteLine("[LocationFacade.GetLocationScreen] Starting...");
            
            Player player = _gameWorld.GetPlayer();
            Location location = GetCurrentLocation();
            LocationSpot spot = GetCurrentLocationSpot();
            
            Console.WriteLine($"[LocationFacade.GetLocationScreen] Location: {location?.Name}, Spot: {spot?.Name} ({spot?.SpotID})");
            
            var viewModel = new LocationScreenViewModel
            {
                CurrentTime = _timeManager.GetFormattedTimeDisplay(),
                DeadlineTimer = GetNextDeadlineDisplay(),
                LocationPath = _spotManager.BuildLocationPath(location, spot),
                LocationName = location?.Name ?? "Unknown Location",
                CurrentSpotName = spot?.Name,
                LocationTraits = _spotManager.GetLocationTraits(location, spot, _timeManager.GetCurrentTimeBlock()),
                AtmosphereText = _narrativeGenerator.GenerateAtmosphereText(spot, location, _timeManager.GetCurrentTimeBlock(), GetUrgentObligationCount(), GetNPCCountAtSpot(spot)),
                QuickActions = new List<LocationActionViewModel>(),
                NPCsPresent = new List<NPCPresenceViewModel>(),
                Observations = new List<ObservationViewModel>(),
                AreasWithinLocation = new List<AreaWithinLocationViewModel>(),
                Routes = new List<RouteOptionViewModel>()
            };
            
            if (location != null && spot != null)
            {
                // Add location-specific actions
                viewModel.QuickActions = _actionManager.GetLocationActions(location, spot);
                
                // Add NPCs with emotional states
                TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
                viewModel.NPCsPresent = GetNPCsWithInteractions(spot, currentTime, npcConversationOptions);
                
                // Add observations
                viewModel.Observations = GetLocationObservations(location.Id, spot.SpotID);
                
                // Add areas within location
                viewModel.AreasWithinLocation = _spotManager.GetAreasWithinLocation(location, spot, currentTime, _npcRepository);
                
                // Add routes to other locations
                viewModel.Routes = GetRoutesFromLocation(location);
            }
            
            Console.WriteLine($"[LocationFacade.GetLocationScreen] Returning viewModel with {viewModel.NPCsPresent.Count} NPCs");
            return viewModel;
        }
        
        /// <summary>
        /// Refresh the current location state.
        /// </summary>
        public void RefreshLocationState()
        {
            var player = _gameWorld.GetPlayer();
            if (player.CurrentLocationSpot != null)
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
        /// Get all NPCs at the player's current spot.
        /// </summary>
        public List<NPC> GetNPCsAtCurrentSpot()
        {
            var player = _gameWorld.GetPlayer();
            if (player?.CurrentLocationSpot == null) return new List<NPC>();
            
            var currentTime = _timeManager.GetCurrentTimeBlock();
            return _npcTracker.GetNPCsAtSpot(player.CurrentLocationSpot.SpotID, currentTime);
        }
        
        // Private helper methods
        
        private List<NPCPresenceViewModel> GetNPCsWithInteractions(LocationSpot spot, TimeBlocks currentTime, List<NPCConversationOptions> npcConversationOptions)
        {
            var result = new List<NPCPresenceViewModel>();
            
            Console.WriteLine($"[LocationFacade.GetNPCsWithInteractions] Looking for NPCs at {spot.SpotID} during {currentTime}");
            var npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            Console.WriteLine($"[LocationFacade.GetNPCsWithInteractions] Found {npcs.Count} NPCs");
            
            foreach (var npc in npcs)
            {
                var emotionalState = GetNPCEmotionalState(npc);
                var interactions = new List<InteractionOptionViewModel>();
                
                // Find conversation options for this NPC if provided
                var npcOptions = npcConversationOptions?.FirstOrDefault(opt => opt.NpcId == npc.ID);
                if (npcOptions != null && npcOptions.AvailableTypes != null)
                {
                    foreach (var conversationType in npcOptions.AvailableTypes)
                    {
                        var interaction = GenerateConversationInteraction(npc, conversationType, emotionalState);
                        if (interaction != null)
                        {
                            interactions.Add(interaction);
                        }
                    }
                }
                
                if (!interactions.Any())
                {
                    interactions.Add(new InteractionOptionViewModel
                    {
                        Text = emotionalState == EmotionalState.HOSTILE ? "Too hostile to approach" : "No interactions available",
                        Cost = "—"
                    });
                }
                
                result.Add(new NPCPresenceViewModel
                {
                    Id = npc.ID,
                    Name = npc.Name,
                    EmotionalStateName = emotionalState.ToString(),
                    Description = GetNPCDescription(npc, emotionalState),
                    Interactions = interactions
                });
            }
            
            return result;
        }
        
        private EmotionalState GetNPCEmotionalState(NPC npc)
        {
            Console.WriteLine($"[LocationFacade.GetNPCEmotionalState] Called for NPC: {npc?.Name ?? "null"}");
            return ConversationRules.DetermineInitialState(npc, _letterQueueManager);
        }
        
        private InteractionOptionViewModel GenerateConversationInteraction(NPC npc, ConversationType conversationType, EmotionalState emotionalState)
        {
            // Generate interaction based on conversation type
            var interaction = new InteractionOptionViewModel
            {
                ConversationType = conversationType
            };
            
            // Set display text based on type
            switch (conversationType)
            {
                case ConversationType.FriendlyChat:
                    interaction.Text = "Friendly Chat";
                    break;
                case ConversationType.Commerce:
                    interaction.Text = "Quick Trade";
                    break;
                case ConversationType.Promise:
                    interaction.Text = "Letter Offer";
                    break;
                case ConversationType.Delivery:
                    interaction.Text = "Deliver Letter";
                    break;
                case ConversationType.Resolution:
                    interaction.Text = "Make Amends";
                    break;
                default:
                    interaction.Text = "Talk";
                    break;
            }
            
            // Set attention cost
            // Default attention cost - should be determined by conversation rules
            int attentionCost = 1;
            interaction.Cost = $"Need {attentionCost} attention";
            
            return interaction;
        }
        
        private string GetNPCDescription(NPC npc, EmotionalState state)
        {
            var obligations = _letterQueueManager.GetActiveObligations();
            var hasUrgentLetter = obligations.Any(o => 
                (o.SenderId == npc.ID || o.SenderName == npc.Name) && 
                o.DeadlineInMinutes < 360);
            
            var template = _dialogueGenerator.GenerateNPCDescription(npc, state, hasUrgentLetter);
            return _narrativeRenderer.RenderTemplate(template);
        }
        
        private List<ObservationViewModel> GetLocationObservations(string locationId, string currentSpotId)
        {
            var observations = new List<ObservationViewModel>();
            
            Console.WriteLine($"[LocationFacade.GetLocationObservations] Looking for observations at {locationId}, spot {currentSpotId}");
            
            var locationObservations = _observationSystem?.GetObservationsForLocationSpot(locationId, currentSpotId);
            
            Console.WriteLine($"[LocationFacade.GetLocationObservations] Got {locationObservations?.Count ?? 0} observations");
            
            if (locationObservations != null)
            {
                var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
                var currentHour = _timeManager.GetCurrentTimeHours();
                var npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpotId, currentTimeBlock);
                var npcIdsAtCurrentSpot = npcsAtCurrentSpot.Select(n => n.ID).ToHashSet();
                
                foreach (var obs in locationObservations)
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
                        AttentionCost = obs.AttentionCost,
                        Relevance = BuildRelevanceString(obs),
                        IsObserved = _observationManager.HasTakenObservation(obs.Id)
                    });
                }
            }
            
            return observations;
        }
        
        private string BuildRelevanceString(Observation obs)
        {
            if (obs.RelevantNPCs?.Any() == true)
            {
                var npcs = string.Join(", ", obs.RelevantNPCs.Select(id => 
                    _npcRepository.GetById(id)?.Name ?? id));
                
                if (obs.CreatesState.HasValue)
                    return $"→ {npcs} ({obs.CreatesState.Value})";
                else
                    return $"→ {npcs}";
            }
            return "";
        }
        
        private List<RouteOptionViewModel> GetRoutesFromLocation(Location location)
        {
            var routes = new List<RouteOptionViewModel>();
            var availableRoutes = _routeRepository.GetRoutesFromLocation(location.Id);
            
            foreach (var route in availableRoutes)
            {
                var destSpot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);
                var destination = destSpot != null ? _locationManager.GetLocation(destSpot.LocationId) : null;
                
                if (destination != null)
                {
                    routes.Add(new RouteOptionViewModel
                    {
                        RouteId = route.Id,
                        Destination = destination.Name,
                        TravelTime = $"{route.TravelTimeMinutes} min",
                        Detail = route.Description ?? route.Name,
                        IsLocked = !route.IsDiscovered,
                        LockReason = !route.IsDiscovered ? "Route not yet discovered" : null,
                        RequiredTier = route.TierRequired,
                        TransportMethod = route.Method.ToString().ToLower(),
                        SupportsCart = false,
                        SupportsCarriage = false,
                        Familiarity = route.IsDiscovered ? null : "Unknown route"
                    });
                }
            }
            
            return routes;
        }
        
        private string GetNextDeadlineDisplay()
        {
            var obligations = _letterQueueManager.GetActiveObligations();
            if (obligations == null || !obligations.Any()) return "";
            
            var mostUrgent = obligations
                .Where(o => o != null)
                .OrderBy(o => o.DeadlineInMinutes)
                .FirstOrDefault();
            
            if (mostUrgent != null)
            {
                return $"⏰ {mostUrgent.HoursUntilDeadline}h";
            }
            
            return "";
        }
        
        private int GetUrgentObligationCount()
        {
            return _letterQueueManager.GetActiveObligations()
                .Count(o => o.DeadlineInMinutes < 360);
        }
        
        private int GetNPCCountAtSpot(LocationSpot spot)
        {
            if (spot == null) return 0;
            return _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, _timeManager.GetCurrentTimeBlock()).Count();
        }
    }
}