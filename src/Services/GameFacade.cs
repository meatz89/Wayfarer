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

    public (int Current, int Max, TimeBlocks TimeBlock) GetCurrentAttentionState()
    {
        var timeBlock = _timeFacade.GetCurrentTimeBlock();
        var (current, max) = _resourceFacade.GetAttention(timeBlock);
        return (current, max, timeBlock);
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


    // ========== TRAVEL OPERATIONS ==========

    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        return _travelFacade.GetTravelDestinations();
    }

    public async Task<bool> TravelToDestinationAsync(string destinationId, string routeId)
    {
        var travelResult = _travelFacade.TravelTo(destinationId, TravelMethods.Walking);
        
        if (travelResult.Success)
        {
            // Orchestrate cross-facade operations
            _resourceFacade.SpendCoins(travelResult.CoinCost);
            
            // Update location through LocationFacade
            var destination = _locationFacade.GetLocationById(destinationId);
            if (destination != null)
            {
                var player = _gameWorld.GetPlayer();
                // TODO: Get proper entry spot from location
                var entrySpot = destination.Spots?.FirstOrDefault();
                if (entrySpot != null)
                {
                    player.CurrentLocationSpot = entrySpot;
                }
            }

            // Advance time and process effects
            var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(travelResult.TravelTimeMinutes);
            
            var timeResult = new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                HoursAdvanced = travelResult.TravelTimeMinutes / 60,
                MinutesAdvanced = travelResult.TravelTimeMinutes
            };
            
            ProcessTimeAdvancement(timeResult);

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
        return _resourceFacade.GetInventoryViewModel();
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
            var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
            var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(120); // 2 hours for work
            
            var timeResult = new TimeAdvancementResult
            {
                OldTimeBlock = oldTimeBlock,
                NewTimeBlock = newTimeBlock,
                CrossedTimeBlock = oldTimeBlock != newTimeBlock,
                HoursAdvanced = 2,
                MinutesAdvanced = 120
            };
            
            ProcessTimeAdvancement(timeResult);
        }
        return result;
    }

    // ========== TOKEN OPERATIONS ==========

    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        var tokens = _tokenFacade.GetTokensWithNPC(npcId);
        var npc = _locationFacade.GetNPCById(npcId);
        
        return new NPCTokenBalance
        {
            Balances = tokens.Select(kvp => new TokenBalance 
            { 
                TokenType = kvp.Key, 
                Amount = kvp.Value 
            }).ToList()
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
        return _locationFacade.GetNPCById(npcId);
    }

    public List<NPC> GetAllNPCs()
    {
        return _locationFacade.GetAllNPCs();
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
        
        return await _conversationFacade.CreateConversationContext(npcId, conversationType);
    }

    public async Task<bool> ExecuteExchange(string npcId, ExchangeData exchange)
    {
        var result = _conversationFacade.ExecuteExchange(exchange);
        return result != null;
    }

    public async Task<bool> EndConversationAsync()
    {
        return await _conversationFacade.EndConversationAsync();
    }

    // ========== NARRATIVE OPERATIONS ==========

    public async Task<bool> TakeObservationAsync(string observationId)
    {
        // Orchestrate between facades
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

    // ========== CROSS-FACADE ORCHESTRATION ==========

    public async Task<bool> ExecuteIntent(PlayerIntent intent)
    {
        return await ProcessIntent(intent);
    }

    public async Task<bool> ProcessIntent(PlayerIntent intent)
    {
        return intent switch
        {
            TalkIntent talkIntent => await ProcessTalkIntent(talkIntent),
            TravelIntent travelIntent => await ProcessTravelIntent(travelIntent),
            MoveIntent moveIntent => await ProcessMoveIntent(moveIntent),
            WaitIntent waitIntent => await ProcessWaitIntent(waitIntent),
            RestIntent restIntent => await ProcessRestIntent(restIntent),
            DeliverLetterIntent deliverIntent => await ProcessDeliverLetterIntent(deliverIntent),
            CollectLetterIntent collectIntent => await ProcessCollectLetterIntent(collectIntent),
            AcceptLetterOfferIntent acceptIntent => await ProcessAcceptLetterOfferIntent(acceptIntent),
            _ => ProcessGenericIntent(intent)
        };
    }

    private async Task<bool> ProcessTalkIntent(TalkIntent intent)
    {
        var context = await CreateConversationContext(intent.NpcId, ConversationType.FriendlyChat);
        return context != null;
    }

    private async Task<bool> ProcessTravelIntent(TravelIntent intent)
    {
        // Use the route ID to find destination and travel
        return await TravelToDestinationAsync("", intent.RouteId);
    }

    private async Task<bool> ProcessMoveIntent(MoveIntent intent)
    {
        return MoveToSpot(intent.TargetSpotId);
    }

    private async Task<bool> ProcessWaitIntent(WaitIntent intent)
    {
        var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(60);
        
        var result = new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            HoursAdvanced = 1,
            MinutesAdvanced = 60
        };
        
        ProcessTimeAdvancement(result);
        _narrativeFacade.AddSystemMessage("You wait and time passes", SystemMessageTypes.Info);
        return true;
    }

    private async Task<bool> ProcessRestIntent(RestIntent intent)
    {
        var oldTimeBlock = _timeFacade.GetCurrentTimeBlock();
        var minutesToRest = intent.Hours * 60;
        var newTimeBlock = _timeFacade.AdvanceTimeByMinutes(minutesToRest);
        
        var result = new TimeAdvancementResult
        {
            OldTimeBlock = oldTimeBlock,
            NewTimeBlock = newTimeBlock,
            CrossedTimeBlock = oldTimeBlock != newTimeBlock,
            HoursAdvanced = intent.Hours,
            MinutesAdvanced = minutesToRest
        };
        
        ProcessTimeAdvancement(result);
        _narrativeFacade.AddSystemMessage($"You rest for {intent.Hours} hour(s)", SystemMessageTypes.Info);
        return true;
    }

    private async Task<bool> ProcessDeliverLetterIntent(DeliverLetterIntent intent)
    {
        var result = _obligationFacade.DeliverObligation(intent.LetterId);
        return result.Success;
    }

    private async Task<bool> ProcessCollectLetterIntent(CollectLetterIntent intent)
    {
        // CollectLetter functionality - for now just accept the letter
        return await AcceptLetterOfferAsync(intent.LetterId);
    }

    private async Task<bool> ProcessAcceptLetterOfferIntent(AcceptLetterOfferIntent intent)
    {
        return await AcceptLetterOfferAsync(intent.OfferId);
    }

    private bool ProcessGenericIntent(PlayerIntent intent)
    {
        _narrativeFacade.AddSystemMessage($"Intent type '{intent.GetType().Name}' not implemented", SystemMessageTypes.Warning);
        return false;
    }

    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes()
    {
        return GetTravelDestinations(); // Use existing method
    }
    
    public EmotionalState GetNPCEmotionalState(string npcId)
    {
        return EmotionalState.NEUTRAL; // TODO: Implement NPC emotional state tracking
    }
    
    public object GetObservationsViewModel()
    {
        return new { }; // TODO: Implement observations view model
    }
    
    public ObligationFacade GetObligationQueueManager()
    {
        return _obligationFacade; // Return the obligation facade which has GetActiveObligations
    }
    
    public List<RouteOption> GetAvailableRoutes()
    {
        return new List<RouteOption>(); // TODO: Implement available routes
    }
    
    public DailyActivityResult GetDailyActivities()
    {
        return new DailyActivityResult(); // Return empty result for now
    }
    
    public List<RouteOption> GetRoutesToDestination(string destinationId)
    {
        return new List<RouteOption>(); // TODO: Implement route discovery
    }
    
    public void AddLetterWithObligationEffects(object letterData)
    {
        // TODO: Implement letter addition with obligation effects
    }

    // ========== HELPER METHODS ==========

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

    private List<NPCConversationOptions> GetNPCConversationOptionsForCurrentLocation()
    {
        var npcs = _locationFacade.GetNPCsAtCurrentSpot();
        var options = new List<NPCConversationOptions>();

        foreach (var npc in npcs)
        {
            var conversationTypes = _conversationFacade.GetAvailableConversationTypes(npc);
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

// No supporting types needed - using existing types from other files