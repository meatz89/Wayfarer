using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.LocationSubsystem;
using Wayfarer.Subsystems.ObligationSubsystem;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TimeSubsystem;
using Wayfarer.Subsystems.TravelSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;
using Wayfarer.Subsystems.NarrativeSubsystem;

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

    public GameWorldSnapshot GetGameSnapshot() => new GameWorldSnapshot(_gameWorld);

    public Player GetPlayer() => _gameWorld.GetPlayer();

    public List<SystemMessage> GetSystemMessages() => _messageSystem.GetMessages();

    public void ClearSystemMessages() => _messageSystem.ClearMessages();

    public MessageSystem GetMessageSystem() => _messageSystem;

    public AttentionStateInfo GetCurrentAttentionState()
    {
        var timeBlock = _timeFacade.GetCurrentTimeBlock();
        var attention = _resourceFacade.GetAttention(timeBlock);
        return new AttentionStateInfo(attention.Current, attention.Max, timeBlock);
    }

    // ========== LOCATION OPERATIONS ==========

    public Location GetCurrentLocation() => _locationFacade.GetCurrentLocation();

    public LocationSpot GetCurrentLocationSpot() => _locationFacade.GetCurrentLocationSpot();

    public Location GetLocationById(string locationId) => _locationFacade.GetLocationById(locationId);

    public bool MoveToSpot(string spotName) => _locationFacade.MoveToSpot(spotName);

    public NPC GetNPCById(string npcId) => _locationFacade.GetNPCById(npcId);

    public List<NPC> GetNPCsAtLocation(string locationId) => _locationFacade.GetNPCsAtLocation(locationId);

    public List<NPC> GetNPCsAtCurrentSpot() => _locationFacade.GetNPCsAtCurrentSpot();

    public LocationScreenViewModel GetLocationScreen()
    {
        var npcConversationOptions = GetNPCConversationOptionsForCurrentLocation();
        return _locationFacade.GetLocationScreen(npcConversationOptions);
    }

    private List<NPCConversationOptions> GetNPCConversationOptionsForCurrentLocation()
    {
        var npcs = _locationFacade.GetNPCsAtCurrentSpot();
        var options = new List<NPCConversationOptions>();

        foreach (var npc in npcs)
        {
            var conversationTypes = _conversationFacade.GetAvailableConversationTypes(npc);
            var attentionCost = _conversationFacade.GetAttentionCost(ConversationType.FriendlyChat);
            var attentionInfo = _resourceFacade.GetAttention(_timeFacade.GetCurrentTimeBlock());
            var currentAttention = attentionInfo.Current;
            
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

    public TimeInfo GetTimeInfo() => _timeFacade.GetTimeInfo();

    public int GetCurrentHour() => _timeFacade.GetCurrentHour();

    public string GetFormattedTimeDisplay() => _timeFacade.GetFormattedTimeDisplay();


    // ========== TRAVEL OPERATIONS ==========

    public List<TravelDestinationViewModel> GetTravelDestinations() => _travelFacade.GetTravelDestinations();

    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes() => GetTravelDestinations();

    public async Task<bool> TravelToDestinationAsync(string routeId)
    {
        string destinationId = string.Empty;

        // Get Destination from the route
        var route = _travelFacade.GetRouteBetweenLocations(
            _gameWorld.GetPlayer().CurrentLocationSpot?.LocationId ?? "", 
            "");
        
        // Get all routes and find the one with matching ID
        var allRoutes = _travelFacade.GetAvailableRoutesFromCurrentLocation();
        var targetRoute = allRoutes.FirstOrDefault(r => r.Id == routeId);
        if (targetRoute != null)
        {
            // Extract destination location ID from the destination spot
            var destSpotId = targetRoute.DestinationLocationSpot;
            var destSpot = _gameWorld.WorldState.locations
                ?.SelectMany(l => l.Spots ?? new List<LocationSpot>())
                .FirstOrDefault(s => s.SpotID == destSpotId);
            if (destSpot != null)
            {
                destinationId = destSpot.LocationId;
            }
        }
        
        var travelResult = _travelFacade.TravelTo(destinationId, TravelMethods.Walking);
        
        if (travelResult.Success)
        {
            _resourceFacade.SpendCoins(travelResult.CoinCost);
            
            // Get the actual destination spot from the route
            var targetRoute = _travelFacade.GetAvailableRoutesFromCurrentLocation()
                .FirstOrDefault(r => r.Id == routeId);
            
            if (targetRoute != null)
            {
                var player = _gameWorld.GetPlayer();
                // Find the destination spot by its ID
                var destSpot = _gameWorld.WorldState.locations
                    ?.SelectMany(l => l.Spots ?? new List<LocationSpot>())
                    .FirstOrDefault(s => s.SpotID == targetRoute.DestinationLocationSpot);
                    
                if (destSpot != null)
                {
                    player.CurrentLocationSpot = destSpot;
                    Console.WriteLine($"[GameFacade] Player moved to spot: {destSpot.SpotID} at location: {destSpot.LocationId}");
                }
            }

            var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(travelResult.TravelTimeMinutes);
            
            ProcessTimeAdvancement(new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                HoursAdvanced = travelResult.TravelTimeMinutes / 60,
                MinutesAdvanced = travelResult.TravelTimeMinutes
            });

            _narrativeFacade.AddSystemMessage($"Traveled to {destination?.Name}", SystemMessageTypes.Info);
        }

        return travelResult.Success;
    }

    // ========== OBLIGATION OPERATIONS ==========

    public LetterQueueViewModel GetLetterQueue() => _obligationFacade.GetLetterQueue();

    public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition) 
        => _obligationFacade.GetDisplacementPreview(obligationId, targetPosition);

    public async Task<bool> DisplaceObligation(string obligationId, int targetPosition) 
        => _obligationFacade.DisplaceObligation(obligationId, targetPosition);

    public async Task<bool> AcceptLetterOfferAsync(string offerId) 
        => _obligationFacade.AcceptLetterOffer(offerId);

    public int GetLetterQueueCount() => _obligationFacade.GetLetterQueueCount();

    public ObligationFacade GetObligationQueueManager() => _obligationFacade;

    // ========== RESOURCE OPERATIONS ==========

    public InventoryViewModel GetInventory() => _resourceFacade.GetInventoryViewModel();

    public async Task<WorkResult> PerformWork()
    {
        var result = _resourceFacade.PerformWork();
        if (result.Success)
        {
            var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(120);
            
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
        var tokens = _tokenFacade.GetTokensWithNPC(npcId);
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

    public ConversationFacade GetConversationFacade() => _conversationFacade;

    public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.FriendlyChat)
        => await _conversationFacade.CreateConversationContext(npcId, conversationType);

    // ========== NARRATIVE OPERATIONS ==========

    public async Task<bool> TakeObservationAsync(string observationId)
    {
        var currentLocation = _locationFacade.GetCurrentLocation();
        var currentSpot = _locationFacade.GetCurrentLocationSpot();
        
        var observation = _narrativeFacade.GetLocationObservations(
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
        var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(60);
        
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
        var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var minutesToRest = hours * 60;
        var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(minutesToRest);
        
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

    public EmotionalState GetNPCEmotionalState(string npcId) => EmotionalState.NEUTRAL;
    
    public List<RouteOption> GetAvailableRoutes() => _travelFacade.GetAvailableRoutesFromCurrentLocation();
    
    public DailyActivityResult GetDailyActivities() => new DailyActivityResult();
    
    public List<RouteOption> GetRoutesToDestination(string destinationId) => new List<RouteOption>();
    
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