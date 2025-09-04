using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.LocationSubsystem;
using Wayfarer.Subsystems.NarrativeSubsystem;
using Wayfarer.Subsystems.ObligationSubsystem;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TimeSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;
using Wayfarer.Subsystems.TravelSubsystem;

/// <summary>
/// GameFacade - Pure orchestrator for UI-Backend communication.
/// Delegates ALL business logic to specialized facades.
/// Coordinates cross-facade operations and handles UI-specific orchestration.
/// </summary>
public class GameFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConversationFacade _conversationFacade;
    private readonly LocationFacade _locationFacade;
    private readonly ObligationFacade _obligationFacade;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TravelFacade _travelFacade;
    private readonly TokenFacade _tokenFacade;
    private readonly NarrativeFacade _narrativeFacade;

    public GameFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ConversationFacade conversationFacade,
        LocationFacade locationFacade,
        ObligationFacade obligationFacade,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TravelFacade travelFacade,
        TokenFacade tokenFacade,
        NarrativeFacade narrativeFacade)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _conversationFacade = conversationFacade;
        _locationFacade = locationFacade;
        _obligationFacade = obligationFacade;
        _resourceFacade = resourceFacade;
        _timeFacade = timeFacade;
        _travelFacade = travelFacade;
        _tokenFacade = tokenFacade;
        _narrativeFacade = narrativeFacade;
    }

    // ========== CORE GAME STATE ==========

    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }

    public List<SystemMessage> GetSystemMessages()
    {
        return _messageSystem.GetMessages();
    }

    public void ClearSystemMessages()
    {
        _messageSystem.ClearMessages();
    }

    public MessageSystem GetMessageSystem()
    {
        return _messageSystem;
    }

    public AttentionStateInfo GetCurrentAttentionState()
    {
        TimeBlocks timeBlock = _timeFacade.GetCurrentTimeBlock();
        AttentionInfo attention = _resourceFacade.GetAttention(timeBlock);
        return new AttentionStateInfo(attention.Current, attention.Max, timeBlock);
    }

    // ========== LOCATION OPERATIONS ==========

    public Location GetCurrentLocation()
    {
        return _locationFacade.GetCurrentLocation();
    }

    public LocationSpot GetCurrentLocationSpot()
    {
        return _locationFacade.GetCurrentLocationSpot();
    }

    public Location GetLocationById(string locationId)
    {
        return _locationFacade.GetLocationById(locationId);
    }

    public bool MoveToSpot(string spotName)
    {
        return _locationFacade.MoveToSpot(spotName);
    }

    public NPC GetNPCById(string npcId)
    {
        return _locationFacade.GetNPCById(npcId);
    }

    public List<NPC> GetNPCsAtLocation(string locationId)
    {
        return _locationFacade.GetNPCsAtLocation(locationId);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        return _locationFacade.GetNPCsAtCurrentSpot();
    }

    public LocationScreenViewModel GetLocationScreen()
    {
        List<NPCConversationOptions> npcConversationOptions = GetNPCConversationOptionsForCurrentLocation();
        return _locationFacade.GetLocationScreen(npcConversationOptions);
    }

    private List<NPCConversationOptions> GetNPCConversationOptionsForCurrentLocation()
    {
        List<NPC> npcs = _locationFacade.GetNPCsAtCurrentSpot();
        List<NPCConversationOptions> options = new List<NPCConversationOptions>();

        foreach (NPC npc in npcs)
        {
            List<ConversationType> conversationTypes = _conversationFacade.GetAvailableConversationTypes(npc);
            int attentionCost = _conversationFacade.GetAttentionCost(ConversationType.FriendlyChat);
            AttentionInfo attentionInfo = _resourceFacade.GetAttention(_timeFacade.GetCurrentTimeBlock());
            int currentAttention = attentionInfo.Current;

            options.Add(new NPCConversationOptions
            {
                NpcId = npc.ID,
                NpcName = npc.Name,
                AvailableTypes = conversationTypes,
                AttentionCost = attentionCost,
                CanAfford = currentAttention >= attentionCost
            });
        }

        return options;
    }

    // ========== TIME OPERATIONS ==========

    public TimeInfo GetTimeInfo()
    {
        return _timeFacade.GetTimeInfo();
    }

    public int GetCurrentHour()
    {
        return _timeFacade.GetCurrentHour();
    }

    public string GetFormattedTimeDisplay()
    {
        return _timeFacade.GetFormattedTimeDisplay();
    }


    // ========== TRAVEL OPERATIONS ==========

    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        return _travelFacade.GetTravelDestinations();
    }

    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes()
    {
        return GetTravelDestinations();
    }

    public async Task<bool> TravelToDestinationAsync(string routeId)
    {
        // Get all routes and find the one with matching ID
        List<RouteOption> allRoutes = _travelFacade.GetAvailableRoutesFromCurrentLocation();
        RouteOption? targetRoute = allRoutes.FirstOrDefault(r => r.Id == routeId);

        if (targetRoute == null)
        {
            _narrativeFacade.AddSystemMessage($"Route {routeId} not found", SystemMessageTypes.Danger);
            return false;
        }

        // Routes are always available - no discovery mechanic needed

        // Calculate travel time and cost
        int travelTime = targetRoute.TravelTimeMinutes;
        int coinCost = _travelFacade.CalculateTravelCost(targetRoute, TravelMethods.Walking);

        // Check if player can afford
        if (coinCost > 0 && _gameWorld.GetPlayer().Coins < coinCost)
        {
            _narrativeFacade.AddSystemMessage($"Not enough coins. Need {coinCost}, have {_gameWorld.GetPlayer().Coins}", SystemMessageTypes.Warning);
            return false;
        }

        // Create a successful travel result since we're handling travel directly
        TravelResult travelResult = new TravelResult
        {
            Success = true,
            TravelTimeMinutes = travelTime,
            CoinCost = coinCost,
            RouteId = routeId,
            TransportMethod = TravelMethods.Walking
        };

        if (travelResult.Success)
        {
            _resourceFacade.SpendCoins(travelResult.CoinCost);

            // Get the actual destination spot from the route
            RouteOption? actualRoute = _travelFacade.GetAvailableRoutesFromCurrentLocation()
                .FirstOrDefault(r => r.Id == routeId);

            if (actualRoute != null)
            {
                Player player = _gameWorld.GetPlayer();
                // Find the destination spot by its ID from GameWorld's Spots dictionary
                LocationSpot? destSpot = _gameWorld.GetSpot(actualRoute.DestinationLocationSpot);

                if (destSpot != null)
                {
                    player.CurrentLocationSpot = destSpot;
                    Console.WriteLine($"[GameFacade] Player moved to spot: {destSpot.SpotID} at location: {destSpot.LocationId}");
                }
            }

            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.AdvanceTimeByMinutes(travelResult.TravelTimeMinutes);

            ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                HoursAdvanced = travelResult.TravelTimeMinutes / 60,
                MinutesAdvanced = travelResult.TravelTimeMinutes
            });

            // Get destination location name for the message
            LocationSpot? finalDestSpot = _gameWorld.GetSpot(targetRoute.DestinationLocationSpot);

            string destinationName = "Unknown";
            if (finalDestSpot != null)
            {
                Location? destLocation = _gameWorld.WorldState.locations
                    ?.FirstOrDefault(l => l.Id == finalDestSpot.LocationId);
                destinationName = destLocation?.Name ?? finalDestSpot.Name;
            }

            _narrativeFacade.AddSystemMessage($"Traveled to {destinationName}", SystemMessageTypes.Info);
        }

        return travelResult.Success;
    }

    // ========== OBLIGATION OPERATIONS ==========

    public LetterQueueViewModel GetLetterQueue()
    {
        return _obligationFacade.GetLetterQueue();
    }

    public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition)
    {
        return _obligationFacade.GetDisplacementPreview(obligationId, targetPosition);
    }

    public async Task<bool> DisplaceObligation(string obligationId, int targetPosition)
    {
        return _obligationFacade.DisplaceObligation(obligationId, targetPosition);
    }

    public async Task<bool> AcceptLetterOfferAsync(string offerId)
    {
        return _obligationFacade.AcceptLetterOffer(offerId);
    }

    public int GetLetterQueueCount()
    {
        return _obligationFacade.GetLetterQueueCount();
    }

    public ObligationFacade GetObligationQueueManager()
    {
        return _obligationFacade;
    }

    // ========== RESOURCE OPERATIONS ==========

    public InventoryViewModel GetInventory()
    {
        return _resourceFacade.GetInventoryViewModel();
    }

    public async Task<WorkResult> PerformWork()
    {
        WorkResult result = _resourceFacade.PerformWork();
        if (result.Success)
        {
            TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            TimeBlocks newTimeBlock = _timeFacade.AdvanceTimeByMinutes(120);

            ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                HoursAdvanced = 2,
                MinutesAdvanced = 120
            });
        }
        return result;
    }

    // ========== TOKEN OPERATIONS ==========

    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        Dictionary<ConnectionType, int> tokens = _tokenFacade.GetTokensWithNPC(npcId);
        return new NPCTokenBalance
        {
            Balances = tokens.Select(kvp => new TokenBalance
            {
                TokenType = kvp.Key,
                Amount = kvp.Value
            }).ToList()
        };
    }


    // ========== CONVERSATION OPERATIONS ==========

    public ConversationFacade GetConversationFacade()
    {
        return _conversationFacade;
    }

    public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.FriendlyChat)
    {
        return await _conversationFacade.CreateConversationContext(npcId, conversationType);
    }

    // ========== NARRATIVE OPERATIONS ==========

    public async Task<bool> TakeObservationAsync(string observationId)
    {
        Location currentLocation = _locationFacade.GetCurrentLocation();
        LocationSpot currentSpot = _locationFacade.GetCurrentLocationSpot();

        Observation? observation = _narrativeFacade.GetLocationObservations(
            currentLocation?.Id,
            currentSpot?.SpotID)
            .FirstOrDefault(o => o.Id == observationId);

        if (observation != null && _resourceFacade.SpendAttention(observation.AttentionCost))
        {
            return _narrativeFacade.TakeObservation(observationId);
        }

        return false;
    }

    public List<TakenObservation> GetTakenObservations()
    {
        return _narrativeFacade.GetActiveObservationCards().Select(card => new TakenObservation
        {
            Id = card.Id,
            Name = card.Name,
            NarrativeText = card.DialogueFragment,
            GeneratedCard = card,
            TimeTaken = DateTime.Now,
            TimeBlockTaken = _timeFacade.GetCurrentTimeBlock()
        }).ToList();
    }

    // ========== GAME INITIALIZATION ==========

    public async Task StartGameAsync()
    {
        // Check if game is already started to prevent duplicate initialization
        if (_gameWorld.IsGameStarted)
        {
            Console.WriteLine($"[GameFacade.StartGameAsync] Game already started, skipping initialization");
            return;
        }

        // Initialize player at starting location
        var player = _gameWorld.GetPlayer();
        var startingLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == "market_square");
        if (startingLocation != null)
        {
            var startingSpot = _gameWorld.Spots.Values.FirstOrDefault(s => s.LocationId == "market_square" && s.SpotID == "central_fountain");
            if (startingSpot != null)
            {
                player.CurrentLocationSpot = startingSpot;
                Console.WriteLine($"[GameFacade.StartGameAsync] Player initialized at {startingLocation.Name} - {startingSpot.Name}");
            }
        }
        
        // Initialize player resources for testing
        player.Coins = 50;  // Starting coins for testing
        player.Health = 10; // Starting health for testing
        player.Hunger = 5;   // Starting food for testing
        
        Console.WriteLine($"[GameFacade.StartGameAsync] Player resources initialized - Coins: {player.Coins}, Health: {player.Health}, Food: {player.Hunger}");
        
        // Mark game as started
        _gameWorld.IsGameStarted = true;
        
        _messageSystem.AddSystemMessage("Game started", SystemMessageTypes.Success);
    }

    // ========== INTENT PROCESSING ==========

    public async Task<bool> ProcessIntent(PlayerIntent intent)
    {
        return intent switch
        {
            TravelIntent travel => await TravelToDestinationAsync(travel.RouteId),
            MoveIntent move => MoveToSpot(move.TargetSpotId),
            WaitIntent => ProcessWaitIntent(),
            RestIntent rest => ProcessRestIntent(rest.Hours),
            DeliverLetterIntent deliver => _obligationFacade.DeliverObligation(deliver.LetterId).Success,
            CollectLetterIntent collect => await AcceptLetterOfferAsync(collect.LetterId),
            AcceptLetterOfferIntent accept => await AcceptLetterOfferAsync(accept.OfferId),
            _ => ProcessGenericIntent(intent)
        };
    }

    private bool ProcessWaitIntent()
    {
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        TimeBlocks newTimeBlock = _timeFacade.AdvanceTimeByMinutes(60);

        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            HoursAdvanced = 1,
            MinutesAdvanced = 60
        });

        _narrativeFacade.AddSystemMessage("You wait and time passes", SystemMessageTypes.Info);
        return true;
    }

    private bool ProcessRestIntent(int hours)
    {
        TimeBlocks oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        int minutesToRest = hours * 60;
        TimeBlocks newTimeBlock = _timeFacade.AdvanceTimeByMinutes(minutesToRest);

        ProcessTimeAdvancement(new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            HoursAdvanced = hours,
            MinutesAdvanced = minutesToRest
        });

        _narrativeFacade.AddSystemMessage($"You rest for {hours} hour(s)", SystemMessageTypes.Info);
        return true;
    }

    private bool ProcessGenericIntent(PlayerIntent intent)
    {
        _narrativeFacade.AddSystemMessage($"Intent type '{intent.GetType().Name}' not implemented", SystemMessageTypes.Warning);
        return false;
    }

    // ========== LEGACY/STUB METHODS ==========

    public EmotionalState GetNPCEmotionalState(string npcId)
    {
        return EmotionalState.NEUTRAL;
    }

    public List<RouteOption> GetAvailableRoutes()
    {
        return _travelFacade.GetAvailableRoutesFromCurrentLocation();
    }

    public DailyActivityResult GetDailyActivities()
    {
        return new DailyActivityResult();
    }

    public List<RouteOption> GetRoutesToDestination(string destinationId)
    {
        return new List<RouteOption>();
    }

    public void AddLetterWithObligationEffects(object letterData)
    {
        if (letterData is DeliveryObligation obligation)
        {
            _obligationFacade.AddLetterWithObligationEffects(obligation);
        }
        else
        {
            Console.WriteLine($"Warning: AddLetterWithObligationEffects called with invalid data type: {letterData?.GetType()?.Name ?? "null"}");
        }
    }

    /// <summary>
    /// Get the LocationActionManager for managing location-specific actions.
    /// </summary>
    public LocationActionManager GetLocationActionManager()
    {
        return _locationFacade.GetLocationActionManager();
    }

    // ========== PRIVATE HELPERS ==========

    private void ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        if (result.CrossedTimeBlock)
        {
            _resourceFacade.ProcessTimeBlockTransition(result.OldTimeBlock, result.NewTimeBlock);
            _obligationFacade.ProcessHourlyDeadlines(result.HoursAdvanced);
            _narrativeFacade.RefreshObservationsForNewTimeBlock();
        }
        else if (result.HoursAdvanced > 0)
        {
            _obligationFacade.ProcessHourlyDeadlines(result.HoursAdvanced);
        }
    }
}