using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.LocationSubsystem;
using Wayfarer.Subsystems.ObligationSubsystem;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TimeSubsystem;
using Wayfarer.Subsystems.TravelSubsystem;
using Wayfarer.Subsystems.MarketSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;
using Wayfarer.Subsystems.NarrativeSubsystem;

/// <summary>
/// GameFacade - Pure orchestrator for UI-Backend communication.
/// Delegates ALL business logic to specialized facades.
/// Coordinates cross-facade operations and handles UI-specific orchestration.
/// </summary>
public class GameFacade
{
    // Core dependencies
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConversationFacade _conversationFacade;
    
    // Subsystem facades
    private readonly LocationFacade _locationFacade;
    private readonly ObligationFacade _obligationFacade;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TravelFacade _travelFacade;
    private readonly MarketFacade _marketFacade;
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
        MarketFacade marketFacade,
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
        _marketFacade = marketFacade;
        _tokenFacade = tokenFacade;
        _narrativeFacade = narrativeFacade;
    }

    // ========== CORE GAME STATE ==========

    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld);
    }

    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }

    public MessageSystem GetMessageSystem()
    {
        return _messageSystem;
    }

    public List<SystemMessage> GetSystemMessages()
    {
        return _messageSystem.GetMessages();
    }

    public void ClearSystemMessages()
    {
        _messageSystem.ClearMessages();
    }

    // ========== ATTENTION STATE ==========

    public (int Current, int Max, TimeBlocks TimeBlock) GetCurrentAttentionState()
    {
        TimeBlocks currentTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var (current, max) = _resourceFacade.GetAttention(currentTimeBlock);
        return (current, max, currentTimeBlock);
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
        return _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
    }

    public bool MoveToSpot(string spotName)
    {
        return _locationFacade.MoveToSpot(spotName);
    }

    public LocationScreenViewModel GetLocationScreen()
    {
        // Get conversation options from ConversationFacade
        var npcConversationOptions = GetNPCConversationOptionsForCurrentLocation();
        return _locationFacade.GetLocationScreen(npcConversationOptions);
    }

    public void RefreshLocationState()
    {
        _locationFacade.RefreshLocationState();
    }

    public List<NPC> GetNPCsAtLocation(string locationId)
    {
        return _locationFacade.GetNPCsAtLocation(locationId);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        return _locationFacade.GetNPCsAtCurrentSpot();
    }

    // ========== TIME OPERATIONS ==========

    public (TimeBlocks timeBlock, int hoursRemaining, int currentDay) GetTimeInfo()
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

    public async Task<bool> ExecuteWaitAction()
    {
        var result = _timeFacade.AdvanceTime(60); // 1 hour
        ProcessTimeAdvancement(result);
        return true;
    }

    public async Task<bool> ExecuteRestAction(string actionType, string cost)
    {
        var result = _resourceFacade.ExecuteRestAction(actionType);
        if (result.Success)
        {
            ProcessTimeAdvancement(_timeFacade.AdvanceTime(result.TimeAdvanced));
        }
        return result.Success;
    }

    // ========== TRAVEL OPERATIONS ==========

    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        var routes = _travelFacade.GetAvailableRoutesFromCurrentLocation();
        var destinations = new List<TravelDestinationViewModel>();

        foreach (var route in routes)
        {
            var destination = GetLocationById(route.DestinationLocationId);
            if (destination != null)
            {
                destinations.Add(new TravelDestinationViewModel
                {
                    LocationId = destination.Id,
                    LocationName = destination.Name,
                    TravelTime = route.TravelTimeMinutes,
                    IsLocked = !_travelFacade.IsRouteDiscovered(route.Id),
                    LockReason = !_travelFacade.IsRouteDiscovered(route.Id) ? "Route not discovered" : null
                });
            }
        }

        return destinations;
    }

    public async Task<bool> TravelToDestinationAsync(string destinationId, string routeId)
    {
        var travelResult = _travelFacade.TravelTo(destinationId, TravelMethods.Walking);
        
        if (travelResult.Success)
        {
            // Deduct costs through ResourceFacade
            _resourceFacade.SpendCoins(travelResult.CoinCost);
            
            // Update location through LocationFacade
            var destination = GetLocationById(destinationId);
            if (destination != null)
            {
                var player = _gameWorld.GetPlayer();
                var entrySpot = destination.LocationSpots?.FirstOrDefault();
                if (entrySpot != null)
                {
                    player.CurrentLocationSpot = entrySpot;
                }
            }

            // Advance time
            ProcessTimeAdvancement(_timeFacade.AdvanceTime(travelResult.TravelTimeMinutes));

            _narrativeFacade.AddSystemMessage($"Traveled to {destination?.Name}", SystemMessageTypes.Info);
        }

        return travelResult.Success;
    }

    // ========== OBLIGATION OPERATIONS ==========

    public LetterQueueViewModel GetLetterQueue()
    {
        return _obligationFacade.GetLetterQueue();
    }

    public LetterBoardViewModel GetLetterBoard()
    {
        return _obligationFacade.GetLetterBoard();
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

    public bool IsLetterQueueFull()
    {
        return _obligationFacade.IsLetterQueueFull();
    }

    // ========== RESOURCE OPERATIONS ==========

    public InventoryViewModel GetInventory()
    {
        return _resourceFacade.GetInventory();
    }

    public int CalculateTotalWeight()
    {
        return _resourceFacade.CalculateTotalWeight();
    }

    public async Task<WorkResult> PerformWork()
    {
        var result = _resourceFacade.PerformWork();
        if (result.Success)
        {
            ProcessTimeAdvancement(_timeFacade.AdvanceTime(120)); // 2 hours for work
        }
        return result;
    }

    // ========== TOKEN OPERATIONS ==========

    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        var tokens = _tokenFacade.GetTokensWithNPC(npcId);
        var npc = GetNPCById(npcId);
        
        return new NPCTokenBalance
        {
            NPCId = npcId,
            NPCName = npc?.Name ?? "Unknown",
            Tokens = tokens
        };
    }

    public TokenMechanicsManager GetTokenMechanicsManager()
    {
        // Legacy compatibility - should be removed when all UI uses facade
        return null;
    }

    // ========== NPC OPERATIONS ==========

    public NPC GetNPCById(string npcId)
    {
        return _gameWorld.WorldState.npcs.FirstOrDefault(n => n.ID == npcId);
    }

    public List<NPC> GetAllNPCs()
    {
        return _gameWorld.WorldState.npcs;
    }

    // ========== CONVERSATION OPERATIONS ==========

    public ConversationFacade GetConversationFacade()
    {
        return _conversationFacade;
    }

    public async Task<ConversationContext> CreateConversationContext(string npcId, ConversationType conversationType = ConversationType.FriendlyChat)
    {
        TimeBlocks currentTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var (currentAttention, maxAttention) = _resourceFacade.GetAttention(currentTimeBlock);
        
        return await _conversationFacade.CreateConversationContext(
            npcId, 
            conversationType, 
            currentAttention,
            _narrativeFacade.GetObservationCardsAsConversationCards());
    }

    public async Task<bool> ExecuteExchange(string npcId, ExchangeData exchange)
    {
        return await _conversationFacade.ExecuteExchange(npcId, exchange);
    }

    public async Task<bool> EndConversationAsync()
    {
        return await _conversationFacade.EndConversationAsync();
    }

    // ========== NARRATIVE OPERATIONS ==========

    public async Task<bool> TakeObservationAsync(string observationId)
    {
        var tokenManager = new TokenMechanicsManager(_gameWorld.GetPlayer(), _messageSystem);
        var card = _narrativeFacade.TakeObservation(observationId, tokenManager);
        
        if (card != null)
        {
            // Deduct attention through ResourceFacade
            var observation = _narrativeFacade.GetLocationObservations(
                GetCurrentLocation()?.Id, 
                GetCurrentLocationSpot()?.SpotID)
                .FirstOrDefault(o => o.Id == observationId);
                
            if (observation != null)
            {
                _resourceFacade.SpendAttention(observation.AttentionCost);
            }
        }

        return card != null;
    }

    public List<TakenObservation> GetTakenObservations()
    {
        return _narrativeFacade.GetActiveObservationCards().Select(card => new TakenObservation
        {
            Id = card.Id,
            Name = card.Name,
            Text = card.Text
        }).ToList();
    }

    // ========== MARKET OPERATIONS ==========

    // Market operations would be added here when needed
    // Currently delegated to MarketFacade

    // ========== GAME INITIALIZATION ==========

    public async Task StartGameAsync()
    {
        _messageSystem.AddSystemMessage("Game started", SystemMessageTypes.Success);
    }

    // ========== CROSS-FACADE ORCHESTRATION ==========

    public async Task<bool> ExecuteIntent(PlayerIntent intent)
    {
        // This orchestrates multiple facades based on intent type
        switch (intent.IntentType)
        {
            case PlayerIntentType.StartConversation:
                var context = await CreateConversationContext(intent.TargetNPCId, intent.ConversationType);
                return context != null;
                
            case PlayerIntentType.TakeObservation:
                return await TakeObservationAsync(intent.ObservationId);
                
            case PlayerIntentType.Travel:
                return await TravelToDestinationAsync(intent.DestinationId, intent.RouteId);
                
            default:
                return false;
        }
    }

    // ========== HELPER METHODS ==========

    private void ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        if (result.TimeBlockChanged)
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

    private List<NPCConversationOptions> GetNPCConversationOptionsForCurrentLocation()
    {
        var npcs = _locationFacade.GetNPCsAtCurrentSpot();
        var options = new List<NPCConversationOptions>();

        foreach (var npc in npcs)
        {
            var conversationTypes = _conversationFacade.GetAvailableConversationTypes(npc.ID);
            var attentionCost = _conversationFacade.GetAttentionCost(ConversationType.FriendlyChat);
            var (currentAttention, _) = _resourceFacade.GetAttention(_timeFacade.GetCurrentTimeBlock());
            
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
}

// ========== SUPPORTING TYPES ==========

public class TakenObservation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
}