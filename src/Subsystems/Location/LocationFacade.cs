using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Actions;
using Wayfarer.GameState.Enums;
using Wayfarer.Subsystems.NarrativeSubsystem;

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Different approaches to investigate a location, unlocked by player stats
    /// </summary>
    public enum InvestigationApproach
    {
        Standard,      // Always available
        Systematic,    // Insight 2+: +1 familiarity
        LocalInquiry,  // Rapport 2+: Learn NPC preferences
        DemandAccess,  // Authority 2+: Access restricted spots
        PurchaseInfo,  // Commerce 2+: Pay for familiarity
        CovertSearch   // Cunning 2+: No alerts
    }

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
        /// Get a specific location by ID.
        /// </summary>
        public Location GetLocationById(string locationId)
        {
            return _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
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
            MovementValidationResult validationResult = _movementValidator.ValidateMovement(currentLocation, currentSpot, targetSpot);
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

            LocationScreenViewModel viewModel = new LocationScreenViewModel
            {
                CurrentTime = _timeManager.GetFormattedTimeDisplay(),
                DeadlineTimer = GetNextDeadlineDisplay(),
                LocationPath = _spotManager.BuildLocationPath(location, spot),
                LocationName = location?.Name ?? "Unknown Location",
                CurrentSpotName = spot?.Name,
                LocationTraits = _spotManager.GetLocationTraits(location, spot, _timeManager.GetCurrentTimeBlock()),
                AtmosphereText = _narrativeGenerator.GenerateAtmosphereText(spot, location, _timeManager.GetCurrentTimeBlock(), GetUrgentObligationCount(), GetNPCCountAtSpot(spot)),
                QuickActions = new List<LocationActionViewModel>(),
                NPCsPresent = new List<NPCFocusViewModel>(),
                Observations = new List<ObservationViewModel>(),
                AreasWithinLocation = new List<AreaWithinLocationViewModel>(),
                Routes = new List<RouteOptionViewModel>()
            };

            if (location != null && spot != null)
            {
                // Add location-specific actions
                viewModel.QuickActions = _actionManager.GetLocationActions(location, spot);

                // Add NPCs with connection states
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
            Player player = _gameWorld.GetPlayer();
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
            Player player = _gameWorld.GetPlayer();
            if (player?.CurrentLocationSpot == null) return new List<NPC>();

            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            return _npcTracker.GetNPCsAtSpot(player.CurrentLocationSpot.SpotID, currentTime);
        }

        /// <summary>
        /// Get a specific NPC by ID.
        /// </summary>
        public NPC GetNPCById(string npcId)
        {
            return _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == npcId);
        }

        /// <summary>
        /// Get all NPCs in the game world.
        /// </summary>
        public List<NPC> GetAllNPCs()
        {
            return _gameWorld.WorldState.NPCs;
        }

        // Private helper methods

        private List<NPCFocusViewModel> GetNPCsWithInteractions(LocationSpot spot, TimeBlocks currentTime, List<NPCConversationOptions> npcConversationOptions)
        {
            List<NPCFocusViewModel> result = new List<NPCFocusViewModel>();

            Console.WriteLine($"[LocationFacade.GetNPCsWithInteractions] Looking for NPCs at {spot.SpotID} during {currentTime}");
            List<NPC> npcs = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
            Console.WriteLine($"[LocationFacade.GetNPCsWithInteractions] Found {npcs.Count} NPCs");

            foreach (NPC npc in npcs)
            {
                ConnectionState connectionState = GetNPCConnectionState(npc);
                List<InteractionOptionViewModel> interactions = new List<InteractionOptionViewModel>();

                // Find conversation options for this NPC if provided
                NPCConversationOptions? npcOptions = npcConversationOptions?.FirstOrDefault(opt => opt.NpcId == npc.ID);
                if (npcOptions != null && npcOptions.AvailableTypes != null)
                {
                    foreach (ConversationType conversationType in npcOptions.AvailableTypes)
                    {
                        InteractionOptionViewModel interaction = GenerateConversationInteraction(npc, conversationType, connectionState);
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
                        Text = "No interactions available",
                        Cost = "—"
                    });
                }

                result.Add(new NPCFocusViewModel
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
            Console.WriteLine($"[LocationFacade.GetNPCConnectionState] Called for NPC: {npc?.Name ?? "null"}");
            return ConversationRules.DetermineInitialState(npc, _letterQueueManager);
        }

        private InteractionOptionViewModel GenerateConversationInteraction(NPC npc, ConversationType conversationType, ConnectionState connectionState)
        {
            // Generate interaction based on conversation type
            InteractionOptionViewModel interaction = new InteractionOptionViewModel
            {
                ConversationType = conversationType
            };

            // Set display text based on type
            switch (conversationType)
            {
                case ConversationType.FriendlyChat:
                    interaction.Text = "Friendly Chat";
                    break;
                case ConversationType.Request:
                    interaction.Text = "Request"; // Actual text comes from NPCRequest.Name
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

        private string GetNPCDescription(NPC npc, ConnectionState state)
        {
            DeliveryObligation[] obligations = _letterQueueManager.GetActiveObligations();
            bool hasUrgentLetter = obligations.Any(o =>
                (o.SenderId == npc.ID || o.SenderName == npc.Name) &&
                o.DeadlineInSegments < 360);

            string template = _dialogueGenerator.GenerateNPCDescription(npc, state, hasUrgentLetter);
            return _narrativeRenderer.RenderTemplate(template);
        }

        private List<ObservationViewModel> GetLocationObservations(string locationId, string currentSpotId)
        {
            List<ObservationViewModel> observations = new List<ObservationViewModel>();

            Console.WriteLine($"[LocationFacade.GetLocationObservations] Looking for observations at {locationId}, spot {currentSpotId}");

            List<Observation>? locationObservations = _observationSystem?.GetObservationsForLocationSpot(locationId, currentSpotId);

            Console.WriteLine($"[LocationFacade.GetLocationObservations] Got {locationObservations?.Count ?? 0} observations");

            if (locationObservations != null)
            {
                TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
                int currentSegment = _timeManager.CurrentSegment;
                List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpotId, currentTimeBlock);
                HashSet<string> npcIdsAtCurrentSpot = npcsAtCurrentSpot.Select(n => n.ID).ToHashSet();

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
                string npcs = string.Join(", ", obs.RelevantNPCs.Select(id =>
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
            List<RouteOptionViewModel> routes = new List<RouteOptionViewModel>();
            IEnumerable<RouteOption> availableRoutes = _routeRepository.GetRoutesFromLocation(location.Id);

            foreach (RouteOption route in availableRoutes)
            {
                LocationSpot? destSpot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == route.DestinationLocationSpot);
                Location? destination = destSpot != null ? _locationManager.GetLocation(destSpot.LocationId) : null;

                if (destination != null)
                {
                    routes.Add(new RouteOptionViewModel
                    {
                        RouteId = route.Id,
                        Destination = destination.Name,
                        TravelTime = $"{route.TravelTimeSegments} seg",
                        Detail = route.Description ?? route.Name,
                        IsLocked = !route.IsDiscovered,
                        LockReason = !route.IsDiscovered ? "Route not yet discovered" : null,
                        // Removed RequiredTier - route access is based on actual requirements in JSON
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
            DeliveryObligation[] obligations = _letterQueueManager.GetActiveObligations();
            if (obligations == null || !obligations.Any()) return "";

            DeliveryObligation? mostUrgent = obligations
                .Where(o => o != null)
                .OrderBy(o => o.DeadlineInSegments)
                .FirstOrDefault();

            if (mostUrgent != null)
            {
                return $"⏰ {mostUrgent.SegmentsUntilDeadline}seg";
            }

            return "";
        }

        private int GetUrgentObligationCount()
        {
            return _letterQueueManager.GetActiveObligations()
                .Count(o => o.DeadlineInSegments < 360);
        }

        private int GetNPCCountAtSpot(LocationSpot spot)
        {
            if (spot == null) return 0;
            return _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, _timeManager.GetCurrentTimeBlock()).Count();
        }

        /// <summary>
        /// Get the LocationActionManager for direct access to location actions.
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

            if (player.Stats.GetLevel(PlayerStatType.Commerce) >= 2)
                approaches.Add(InvestigationApproach.PurchaseInfo);

            if (player.Stats.GetLevel(PlayerStatType.Cunning) >= 2)
                approaches.Add(InvestigationApproach.CovertSearch);

            return approaches;
        }

        /// <summary>
        /// Investigate a location to gain familiarity. Costs 1 attention and takes 1 segment.
        /// Familiarity gain depends on spot properties: Quiet spots +2, Busy spots +1, others +1.
        /// Familiarity is capped at the location's MaxFamiliarity (typically 3).
        /// </summary>
        /// <param name="locationId">ID of the location to investigate</param>
        /// <param name="spotId">ID of the spot where investigation takes place</param>
        /// <returns>True if investigation was successful, false if not possible</returns>
        public bool InvestigateLocation(string locationId, string spotId)
        {
            return InvestigateLocation(locationId, spotId, InvestigationApproach.Standard);
        }

        /// <summary>
        /// Investigate a location with a specific approach. Different approaches provide different benefits
        /// based on player stats and may have different costs or requirements.
        /// </summary>
        /// <param name="locationId">ID of the location to investigate</param>
        /// <param name="spotId">ID of the spot where investigation takes place</param>
        /// <param name="approach">Investigation approach to use</param>
        /// <returns>True if investigation was successful, false if not possible</returns>
        public bool InvestigateLocation(string locationId, string spotId, InvestigationApproach approach)
        {
            Player player = _gameWorld.GetPlayer();
            Location location = GetLocationById(locationId);
            LocationSpot spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);

            if (player == null || location == null || spot == null)
            {
                _messageSystem.AddSystemMessage("Cannot investigate - invalid location or spot", SystemMessageTypes.Warning);
                return false;
            }

            // Check if player can use this approach
            List<InvestigationApproach> availableApproaches = GetAvailableApproaches(player);
            if (!availableApproaches.Contains(approach))
            {
                _messageSystem.AddSystemMessage($"Cannot use {approach} approach - insufficient stats", SystemMessageTypes.Warning);
                return false;
            }

            InvestigationAction investigation = new InvestigationAction();

            // Check basic investigation requirements
            if (!investigation.CanInvestigate(player, location))
            {
                if (!player.HasAttention(investigation.AttentionCost))
                {
                    _messageSystem.AddSystemMessage("Not enough attention to investigate", SystemMessageTypes.Warning);
                }
                else
                {
                    _messageSystem.AddSystemMessage($"Already fully familiar with {location.Name}", SystemMessageTypes.Info);
                }
                return false;
            }

            // Handle approach-specific costs and requirements
            int attentionCost = investigation.AttentionCost;
            int coinCost = 0;

            switch (approach)
            {
                case InvestigationApproach.PurchaseInfo:
                    // Commerce approach: pay coins for instant familiarity
                    int currentFamiliarity = player.GetLocationFamiliarity(locationId);
                    int maxGain = location.MaxFamiliarity - currentFamiliarity;
                    coinCost = maxGain * 2; // 2 coins per familiarity level
                    if (coinCost > 0 && player.Coins < coinCost)
                    {
                        _messageSystem.AddSystemMessage($"Not enough coins for purchase approach. Need {coinCost}, have {player.Coins}", SystemMessageTypes.Warning);
                        return false;
                    }
                    break;
                case InvestigationApproach.CovertSearch:
                    // Cunning approach: no time advancement (avoids alerts)
                    break;
            }

            // Get current familiarity and time
            int currentFam = player.GetLocationFamiliarity(locationId);
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

            // Calculate familiarity gain with approach bonuses
            int familiarityGain = investigation.GetFamiliarityGain(spot, currentTime);

            // Apply approach-specific bonuses
            switch (approach)
            {
                case InvestigationApproach.Systematic:
                    // Insight bonus: +1 familiarity
                    familiarityGain += 1;
                    break;
                case InvestigationApproach.PurchaseInfo:
                    // Commerce: instant familiarity gain to max
                    familiarityGain = location.MaxFamiliarity - currentFam;
                    break;
                case InvestigationApproach.LocalInquiry:
                    // Rapport: Learn which NPCs want observations from this location
                    HandleLocalInquiryBonus(locationId);
                    break;
                case InvestigationApproach.DemandAccess:
                    // Authority: Allow investigation of restricted spots (implementation depends on spot system)
                    break;
            }

            // Apply familiarity gain (capped at MaxFamiliarity)
            int newFamiliarity = Math.Min(location.MaxFamiliarity, currentFam + familiarityGain);
            player.SetLocationFamiliarity(locationId, newFamiliarity);

            // Check for observation rewards at new familiarity level
            CheckAndGrantObservationRewards(location, player, currentFam, newFamiliarity);

            // Apply costs
            player.SpendAttention(attentionCost);
            if (coinCost > 0)
            {
                player.ModifyCoins(-coinCost);
            }

            // Advance time (except for covert approach)
            if (approach != InvestigationApproach.CovertSearch)
            {
                _timeManager.AdvanceSegments(investigation.TimeSegments);
            }

            // Generate success message
            string approachText = approach == InvestigationApproach.Standard ? "" : $" using {approach} approach";
            string gainText = familiarityGain >= 2 ? "significant insights" : "new insights";
            _messageSystem.AddSystemMessage(
                $"Investigated {location.Name}{approachText}. Gained {gainText} (+{familiarityGain} familiarity). " +
                $"Familiarity: {newFamiliarity}/{location.MaxFamiliarity}",
                SystemMessageTypes.Success
            );

            Console.WriteLine($"[InvestigateLocation] {player.Name} investigated {location.Name} from {currentFam} to {newFamiliarity} familiarity using {approach}");

            return true;
        }

        /// <summary>
        /// Check and grant observation rewards when reaching new familiarity levels
        /// </summary>
        private void CheckAndGrantObservationRewards(Location location, Player player, int oldFamiliarity, int newFamiliarity)
        {
            // Check each familiarity level between old and new (in case of multi-level jumps)
            for (int level = oldFamiliarity + 1; level <= newFamiliarity; level++)
            {
                // Find observation rewards for this familiarity level
                List<ObservationReward> applicableRewards = location.ObservationRewards
                    .Where(r => r.FamiliarityRequired == level)
                    .Where(r => !r.PriorObservationRequired.HasValue ||
                               location.HighestObservationCompleted >= r.PriorObservationRequired.Value)
                    .ToList();

                foreach (ObservationReward reward in applicableRewards)
                {
                    if (reward.ObservationCard != null)
                    {
                        // Grant the observation card to the player
                        GrantObservationCard(reward.ObservationCard, player);

                        // Update the highest observation completed
                        location.HighestObservationCompleted = Math.Max(location.HighestObservationCompleted, level);

                        // Notify player
                        _messageSystem.AddSystemMessage(
                            $"Discovered observation: {reward.ObservationCard.Name}",
                            SystemMessageTypes.Success
                        );

                        Console.WriteLine($"[LocationFacade] Granted observation card '{reward.ObservationCard.Name}' for reaching familiarity {level} at {location.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant an observation card to the player by adding it to the target NPC's observation deck
        /// </summary>
        private void GrantObservationCard(ObservationCardReward cardReward, Player player)
        {
            // Find the target NPC
            NPC targetNpc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == cardReward.TargetNpcId);
            if (targetNpc == null)
            {
                Console.WriteLine($"[LocationFacade] Warning: Could not find target NPC '{cardReward.TargetNpcId}' for observation card");
                return;
            }

            // Create the observation card
            ObservationCard observationCard = new ObservationCard(cardReward.Id, cardReward.Name)
            {
                LocationDiscovered = player.CurrentLocationSpot?.LocationId ?? "Unknown",
                ItemName = cardReward.Name,
                ObservationId = cardReward.Id,
                TimeDiscovered = _timeManager.GetCurrentTimeBlock().ToString(),
                TokenType = ConnectionType.None, // Default for observations
                SuccessType = ParseSuccessType(cardReward.Effect),
                DialogueFragment = cardReward.Description
            };

            // Initialize NPC's observation deck if needed
            if (targetNpc.ObservationDeck == null)
            {
                targetNpc.ObservationDeck = new CardDeck();
            }

            // Add the observation card to the NPC's observation deck
            targetNpc.ObservationDeck.AddCard(observationCard);

            // Track that player has this observation
            if (!player.CollectedObservations.Contains(cardReward.Id))
            {
                player.CollectedObservations.Add(cardReward.Id);
            }

            Console.WriteLine($"[LocationFacade] Added observation card '{cardReward.Name}' to {targetNpc.Name}'s observation deck");
        }

        /// <summary>
        /// Parse effect string into success type for observation cards
        /// </summary>
        private SuccessEffectType ParseSuccessType(string effect)
        {
            if (string.IsNullOrEmpty(effect))
                return SuccessEffectType.None;

            string lowerEffect = effect.ToLower();
            if (lowerEffect.Contains("rapport"))
                return SuccessEffectType.Rapport;
            if (lowerEffect.Contains("thread"))
                return SuccessEffectType.Threading;
            if (lowerEffect.Contains("focus"))
                return SuccessEffectType.Focusing;
            if (lowerEffect.Contains("advance"))
                return SuccessEffectType.Advancing;

            return SuccessEffectType.None;
        }

        /// <summary>
        /// Handle the LocalInquiry approach bonus - reveal which NPCs want observations from this location
        /// </summary>
        private void HandleLocalInquiryBonus(string locationId)
        {
            // This would integrate with the observation system to reveal NPC preferences
            // For now, just add a message indicating the benefit
            _messageSystem.AddSystemMessage(
                "Through local inquiries, you learn which NPCs are interested in observations from this area",
                SystemMessageTypes.Info
            );
        }

        // ========== WORK SYSTEM ==========

        /// <summary>
        /// Get available work actions at the current location
        /// </summary>
        public List<WorkAction> GetAvailableWork(string locationId)
        {
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
            if (location == null) return new List<WorkAction>();

            List<WorkAction> availableWork = new List<WorkAction>();
            Player player = _gameWorld.GetPlayer();

            foreach (WorkAction work in location.AvailableWork)
            {
                // Check requirements
                if (work.RequiredTokens.HasValue && work.RequiredTokenType.HasValue)
                {
                    // Token checking should be done at a higher level (GameFacade)
                    // For now, skip token-gated work until GameFacade can filter it
                    continue;
                }

                if (!string.IsNullOrEmpty(work.RequiredPermit))
                {
                    if (!player.Inventory.HasItem(work.RequiredPermit))
                        continue;
                }

                availableWork.Add(work);
            }

            return availableWork;
        }

        /// <summary>
        /// Perform work action, consuming entire time block and generating coins
        /// </summary>
        public bool PerformWork(string workActionId, string locationId)
        {
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
            if (location == null) return false;

            WorkAction work = location.AvailableWork.FirstOrDefault(w => w.Id == workActionId);
            if (work == null) return false;

            Player player = _gameWorld.GetPlayer();

            // Calculate coin output based on hunger
            int currentHunger = player.Hunger;
            int hungerPenalty = currentHunger / 25; // floor(hunger/25)
            int coinsEarned = Math.Max(0, work.BaseCoins - hungerPenalty);

            // Apply rewards
            player.ModifyCoins(coinsEarned);

            // Apply additional benefits for service work
            if (work.HungerReduction.HasValue)
            {
                player.ReduceHunger(work.HungerReduction.Value);
            }

            if (work.HealthRestore.HasValue)
            {
                player.ModifyHealth(work.HealthRestore.Value);
            }

            if (!string.IsNullOrEmpty(work.GrantedItem))
            {
                player.Inventory.AddItem(work.GrantedItem);
            }

            // Advance time by entire block (4 segments)
            _timeManager.JumpToNextPeriod();

            // Send success message
            string workTypeText = work.Type == WorkType.Service ? "service" : "work";
            _messageSystem.AddSystemMessage(
                $"Completed {work.Name} - earned {coinsEarned} coins (base {work.BaseCoins} - {hungerPenalty} hunger penalty)",
                SystemMessageTypes.Success
            );

            Console.WriteLine($"[LocationFacade.PerformWork] {player.Name} completed {work.Name} at {location.Name}, earned {coinsEarned} coins");

            return true;
        }

        /// <summary>
        /// Check if player can afford to work (has time in current block)
        /// </summary>
        public bool CanAffordWork()
        {
            // Work consumes entire block, so just check if we're not at the last segment
            // The TimeManager will handle advancing to next block
            return true; // Let TimeManager handle block advancement logic
        }

        /// <summary>
        /// Get work efficiency preview showing hunger impact
        /// </summary>
        public (int baseCoins, int hungerPenalty, int actualCoins) CalculateWorkOutput(WorkAction work)
        {
            Player player = _gameWorld.GetPlayer();
            int currentHunger = player.Hunger;
            int hungerPenalty = currentHunger / 25;
            int actualCoins = Math.Max(0, work.BaseCoins - hungerPenalty);

            return (work.BaseCoins, hungerPenalty, actualCoins);
        }
    }
}