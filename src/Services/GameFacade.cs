using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Implementation of IGameFacade - THE single entry point for all UI-Backend communication.
/// This class delegates to existing UIServices and managers to maintain clean separation.
/// </summary>
public class GameFacade : IGameFacade
{
    // Core dependencies
    private readonly GameWorld _gameWorld;
    private readonly GameWorldManager _gameWorldManager;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly CommandExecutor _commandExecutor;
    
    // UI Services
    private readonly LocationActionsUIService _locationActionsService;
    private readonly TravelUIService _travelService;
    private readonly RestUIService _restService;
    private readonly LetterQueueUIService _letterQueueService;
    private readonly MarketUIService _marketService;
    private readonly ReadableLetterUIService _readableLetterService;
    
    // Managers
    private readonly TravelManager _travelManager;
    
    // Domain services
    private readonly ConversationFactory _conversationFactory;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly RouteRepository _routeRepository;
    private readonly NarrativeManager _narrativeManager;
    private readonly FlagService _flagService;
    private readonly ItemRepository _itemRepository;
    private readonly ConversationStateManager _conversationStateManager;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly NPCLetterOfferService _npcLetterOfferService;
    
    public GameFacade(
        GameWorld gameWorld,
        GameWorldManager gameWorldManager,
        ITimeManager timeManager,
        MessageSystem messageSystem,
        CommandExecutor commandExecutor,
        LocationActionsUIService locationActionsService,
        TravelUIService travelService,
        RestUIService restService,
        LetterQueueUIService letterQueueService,
        MarketUIService marketService,
        ReadableLetterUIService readableLetterService,
        TravelManager travelManager,
        ConversationFactory conversationFactory,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        RouteRepository routeRepository,
        NarrativeManager narrativeManager,
        FlagService flagService,
        ItemRepository itemRepository,
        ConversationStateManager conversationStateManager,
        ConnectionTokenManager connectionTokenManager,
        NPCLetterOfferService npcLetterOfferService)
    {
        _gameWorld = gameWorld;
        _gameWorldManager = gameWorldManager;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _commandExecutor = commandExecutor;
        _locationActionsService = locationActionsService;
        _travelService = travelService;
        _restService = restService;
        _letterQueueService = letterQueueService;
        _marketService = marketService;
        _readableLetterService = readableLetterService;
        _travelManager = travelManager;
        _conversationFactory = conversationFactory;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _routeRepository = routeRepository;
        _narrativeManager = narrativeManager;
        _flagService = flagService;
        _itemRepository = itemRepository;
        _conversationStateManager = conversationStateManager;
        _connectionTokenManager = connectionTokenManager;
        _npcLetterOfferService = npcLetterOfferService;
    }
    
    // ========== GAME STATE QUERIES ==========
    
    public GameWorldSnapshot GetGameSnapshot()
    {
        return _gameWorldManager.GetGameSnapshot();
    }
    
    public Player GetPlayer()
    {
        return _gameWorld.GetPlayer();
    }
    
    public (Location location, LocationSpot spot) GetCurrentLocation()
    {
        var player = _gameWorld.GetPlayer();
        return (player.CurrentLocation, player.CurrentLocationSpot);
    }
    
    public (TimeBlocks timeBlock, int hoursRemaining, int currentDay) GetTimeInfo()
    {
        return (_timeManager.GetCurrentTimeBlock(), 
                _timeManager.HoursRemaining, 
                _gameWorld.CurrentDay);
    }
    
    // ========== LOCATION ACTIONS ==========
    
    public LocationActionsViewModel GetLocationActions()
    {
        return _locationActionsService.GetLocationActionsViewModel();
    }
    
    public async Task<bool> ExecuteLocationActionAsync(string commandId)
    {
        return await _locationActionsService.ExecuteActionAsync(commandId);
    }
    
    // ========== TRAVEL ==========
    
    public List<TravelDestinationViewModel> GetTravelDestinations()
    {
        var travelViewModel = _travelService.GetTravelViewModel();
        var destinations = new List<TravelDestinationViewModel>();
        
        foreach (var dest in travelViewModel.Destinations)
        {
            var location = _locationRepository.GetLocation(dest.LocationId);
            bool canTravel = dest.AvailableRoutes.Any(r => !r.IsBlocked);
            
            destinations.Add(new TravelDestinationViewModel
            {
                LocationId = dest.LocationId,
                LocationName = dest.LocationName,
                Description = location?.Description ?? "",
                CanTravel = canTravel,
                CannotTravelReason = !canTravel ? "No available routes" : null,
                MinimumCost = dest.AvailableRoutes.Where(r => !r.IsBlocked).Select(r => r.CoinCost).DefaultIfEmpty(0).Min(),
                MinimumTime = dest.AvailableRoutes.Where(r => !r.IsBlocked).Select(r => r.TravelTimeHours).DefaultIfEmpty(0).Min()
            });
        }
        
        return destinations;
    }
    
    public List<TravelRouteViewModel> GetRoutesToDestination(string destinationId)
    {
        var travelViewModel = _travelService.GetTravelViewModel();
        var destination = travelViewModel.Destinations.FirstOrDefault(d => d.LocationId == destinationId);
        
        if (destination == null)
            return new List<TravelRouteViewModel>();
        
        var routes = new List<TravelRouteViewModel>();
        
        foreach (var route in destination.AvailableRoutes)
        {
            routes.Add(new TravelRouteViewModel
            {
                RouteId = route.RouteId,
                RouteName = route.TerrainType,
                Description = route.TransportRequirement ?? "Standard route",
                TransportMethod = TravelMethods.Walking, // TODO: Determine from route
                TimeCost = route.TravelTimeHours,
                StaminaCost = route.TotalStaminaCost,
                CoinCost = route.CoinCost,
                CanTravel = !route.IsBlocked,
                CannotTravelReason = route.BlockedReason
            });
        }
        
        return routes;
    }
    
    public async Task<bool> TravelToDestinationAsync(string destinationId, string routeId)
    {
        // Find the route
        var route = _routeRepository.GetRouteById(routeId);
        if (route == null) return false;
        
        var routeOption = new RouteOption
        {
            Id = routeId,
            Method = route.Method
        };
        
        var command = new TravelCommand(routeOption, _travelManager, _messageSystem);
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    // ========== REST ==========
    
    public RestOptionsViewModel GetRestOptions()
    {
        var restViewModel = _restService.GetRestViewModel();
        
        return new RestOptionsViewModel
        {
            RestOptions = restViewModel.RestOptions,
            LocationActions = restViewModel.LocationActions,
            WaitOptions = restViewModel.WaitOptions
        };
    }
    
    public async Task<bool> ExecuteRestAsync(string restOptionId)
    {
        return await _restService.ExecuteRestOptionAsync(restOptionId);
    }
    
    // ========== CONVERSATIONS ==========
    
    public async Task<ConversationViewModel> StartConversationAsync(string npcId)
    {
        var conversationManager = await _gameWorldManager.StartConversation(npcId);
        if (conversationManager == null) return null;
        
        return CreateConversationViewModel(conversationManager);
    }
    
    public async Task<ConversationViewModel> ContinueConversationAsync(string choiceId)
    {
        var currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending) return null;
        
        var choice = currentConversation.Choices?.FirstOrDefault(c => c.ChoiceID == choiceId);
        if (choice == null) return null;
        
        // Process choice - conversation manager handles choice processing internally
        // The choice selection would trigger events that update the conversation state
        // TODO: Implement proper choice processing mechanism
        
        return CreateConversationViewModel(currentConversation);
    }
    
    public ConversationViewModel GetCurrentConversation()
    {
        var currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending) return null;
        
        return CreateConversationViewModel(currentConversation);
    }
    
    private ConversationViewModel CreateConversationViewModel(ConversationManager conversation)
    {
        return new ConversationViewModel
        {
            NpcName = conversation.Context.TargetNPC.Name,
            NpcId = conversation.Context.TargetNPC.ID,
            CurrentText = conversation.State?.CurrentNarrative ?? "",
            Choices = conversation.Choices?.Select(c => new ConversationChoiceViewModel
            {
                Id = c.ChoiceID,
                Text = c.NarrativeText,
                IsAvailable = c.IsAffordable,
                UnavailableReason = !c.IsAffordable ? $"Requires {c.FocusCost} focus" : null
            }).ToList() ?? new List<ConversationChoiceViewModel>(),
            IsComplete = conversation.State?.IsConversationComplete ?? false,
            ConversationTopic = conversation.Context.ConversationTopic
        };
    }
    
    // ========== LETTER QUEUE ==========
    
    public LetterQueueViewModel GetLetterQueue()
    {
        return _letterQueueService.GetQueueViewModel();
    }
    
    public async Task<bool> ExecuteLetterActionAsync(string actionType, string letterId)
    {
        // Parse action type and letter position
        if (!int.TryParse(letterId, out int position))
            return false;
            
        switch (actionType.ToLower())
        {
            case "deliver":
                return await _letterQueueService.DeliverLetterAsync(position);
            case "skip":
                await _letterQueueService.TriggerSkipConversationAsync(position);
                return true;
            case "priority":
                return await _letterQueueService.PriorityMoveAsync(position);
            case "extend":
                return await _letterQueueService.ExtendDeadlineAsync(position);
            default:
                return false;
        }
    }
    
    public async Task<bool> DeliverLetterAsync(string letterId)
    {
        // For now, we only support delivering from position 1
        // The letterId parameter is kept for future flexibility if we need to deliver by ID
        return await _letterQueueService.DeliverLetterAsync(1);
    }
    
    public async Task<bool> SkipLetterAsync(int position)
    {
        // Validate position is valid for skipping (positions 2-8 when position 1 is empty)
        if (position < 2 || position > 8)
            return false;
            
        await _letterQueueService.TriggerSkipConversationAsync(position);
        return true;
    }
    
    public async Task<bool> LetterQueueMorningSwapAsync(int position1, int position2)
    {
        // Validate positions are within range
        if (position1 < 1 || position1 > 8 || position2 < 1 || position2 > 8)
            return false;
            
        if (position1 == position2)
            return false;
            
        return await _letterQueueService.MorningSwapAsync(position1, position2);
    }
    
    public async Task<bool> LetterQueuePriorityMoveAsync(int fromPosition)
    {
        // Validate position is within range and not already at position 1
        if (fromPosition < 2 || fromPosition > 8)
            return false;
            
        return await _letterQueueService.PriorityMoveAsync(fromPosition);
    }
    
    public async Task<bool> LetterQueueExtendDeadlineAsync(int position)
    {
        // Validate position is within range
        if (position < 1 || position > 8)
            return false;
            
        return await _letterQueueService.ExtendDeadlineAsync(position);
    }
    
    public async Task<bool> LetterQueuePurgeAsync(Dictionary<string, int> tokenSelection)
    {
        // Validate token selection
        if (tokenSelection == null || tokenSelection.Count == 0)
            return false;
            
        // InitiatePurgeAsync handles the conversation trigger
        await _letterQueueService.InitiatePurgeAsync(tokenSelection);
        return true;
    }
    
    public LetterBoardViewModel GetLetterBoard()
    {
        // Check if it's dawn
        var currentTime = _timeManager.GetCurrentTimeBlock();
        if (currentTime != TimeBlocks.Dawn)
        {
            return new LetterBoardViewModel
            {
                IsAvailable = false,
                UnavailableReason = "Letter board is only available at Dawn",
                Offers = new List<LetterOfferViewModel>(),
                CurrentTime = currentTime
            };
        }
        
        // Get offers from the letter queue manager
        var player = _gameWorld.GetPlayer();
        var queueViewModel = _letterQueueService.GetQueueViewModel();
        var offers = new List<LetterOffer>();
        
        // For now, return empty offers until letter board functionality is implemented
        // TODO: Implement letter board offers in LetterQueueManager
        var offerViewModels = offers.Select(offer => new LetterOfferViewModel
        {
            Id = offer.Id,
            SenderName = offer.NPCName,
            RecipientName = "Unknown", // TODO: Get recipient from template
            Description = offer.Message,
            Payment = offer.Payment,
            DeadlineDays = offer.Deadline,
            CanAccept = true, // TODO: Check queue capacity
            CannotAcceptReason = null,
            TokenTypes = new List<string> { offer.LetterType.ToString() }
        }).ToList();
        
        return new LetterBoardViewModel
        {
            IsAvailable = true,
            Offers = offerViewModels,
            CurrentTime = currentTime
        };
    }
    
    public async Task<bool> AcceptLetterOfferAsync(string offerId)
    {
        // TODO: Implement letter offer acceptance in LetterQueueManager
        return false;
    }
    
    // ========== MARKET ==========
    
    public MarketViewModel GetMarket()
    {
        var player = _gameWorld.GetPlayer();
        return _marketService.GetMarketViewModel(player.CurrentLocation.Id);
    }
    
    public async Task<bool> BuyItemAsync(string itemId, string traderId)
    {
        var player = _gameWorld.GetPlayer();
        var command = new MarketTradeCommand(
            itemId,
            MarketTradeCommand.TradeAction.Buy,
            player.CurrentLocation.Id,
            _itemRepository,
            null // TODO: Inject IGameRuleEngine
        );
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    public async Task<bool> SellItemAsync(string itemId, string traderId)
    {
        var player = _gameWorld.GetPlayer();
        var command = new MarketTradeCommand(
            itemId,
            MarketTradeCommand.TradeAction.Sell,
            player.CurrentLocation.Id,
            _itemRepository,
            null // TODO: Inject IGameRuleEngine
        );
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    // ========== INVENTORY ==========
    
    public InventoryViewModel GetInventory()
    {
        var player = _gameWorld.GetPlayer();
        var items = new List<InventoryItemViewModel>();
        
        foreach (var itemId in player.Inventory.ItemSlots.Where(s => !string.IsNullOrEmpty(s)))
        {
            var item = _itemRepository.GetItemById(itemId);
            if (item != null)
            {
                items.Add(new InventoryItemViewModel
                {
                    ItemId = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Weight = item.Weight,
                    Value = item.SellPrice,
                    CanUse = false, // TODO: Implement consumable items
                    CanRead = item.IsReadable()
                });
            }
        }
        
        return new InventoryViewModel
        {
            Items = items,
            TotalWeight = items.Sum(i => i.Weight),
            MaxSlots = player.Inventory.ItemSlots.Length,
            UsedSlots = items.Count,
            Coins = player.Coins
        };
    }
    
    public async Task<bool> UseItemAsync(string itemId)
    {
        var item = _itemRepository.GetItemById(itemId);
        if (item == null) return false;
        
        if (item.IsReadable())
        {
            return await _readableLetterService.ReadLetterAsync(itemId);
        }
        
        // Other item uses can be implemented here
        return false;
    }
    
    // ========== NARRATIVE/TUTORIAL ==========
    
    public NarrativeStateViewModel GetNarrativeState()
    {
        var activeNarratives = _narrativeManager.GetActiveNarratives();
        var states = new List<NarrativeViewModel>();
        
        foreach (var narrativeId in activeNarratives)
        {
            var currentStep = _narrativeManager.GetCurrentStep(narrativeId);
            if (currentStep != null)
            {
                states.Add(new NarrativeViewModel
                {
                    NarrativeId = narrativeId,
                    CurrentStepId = currentStep.Id,
                    StepName = currentStep.Name,
                    StepDescription = currentStep.Description,
                    IsComplete = false
                });
            }
        }
        
        return new NarrativeStateViewModel
        {
            ActiveNarratives = states,
            IsTutorialActive = _flagService.HasFlag("tutorial_active"),
            TutorialComplete = _flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)
        };
    }
    
    public bool IsTutorialActive()
    {
        return _flagService.HasFlag("tutorial_active");
    }
    
    public TutorialGuidanceViewModel GetTutorialGuidance()
    {
        if (!_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
        {
            return new TutorialGuidanceViewModel { IsActive = false };
        }
        
        var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
        if (currentStep == null)
        {
            return new TutorialGuidanceViewModel { IsActive = false };
        }
        
        var totalSteps = _narrativeManager.GetNarrativeDefinition("wayfarer_tutorial")?.Steps?.Count ?? 0;
        var currentStepIndex = _narrativeManager.GetNarrativeDefinition("wayfarer_tutorial")?.Steps?
            .FindIndex(s => s.Id == currentStep.Id) ?? 0;
        
        return new TutorialGuidanceViewModel
        {
            IsActive = true,
            CurrentStep = currentStepIndex + 1,
            TotalSteps = totalSteps,
            StepTitle = currentStep.Name,
            GuidanceText = currentStep.GuidanceText,
            AllowedActions = currentStep.AllowedActions ?? new List<string>()
        };
    }
    
    // ========== GAME FLOW ==========
    
    public async Task StartGameAsync()
    {
        await _gameWorldManager.StartGame();
    }
    
    public async Task<MorningActivityResult> AdvanceToNextDayAsync()
    {
        await _gameWorldManager.AdvanceToNextDay();
        return _gameWorldManager.GetMorningActivitySummary();
    }
    
    public MorningActivityResult GetMorningActivities()
    {
        return _gameWorldManager.GetMorningActivitySummary();
    }
    
    // ========== SYSTEM MESSAGES ==========
    
    public List<SystemMessage> GetSystemMessages()
    {
        // MessageSystem doesn't store messages, they're in GameWorld
        return _gameWorld.SystemMessages ?? new List<SystemMessage>();
    }
    
    public void ClearSystemMessages()
    {
        // Clear messages from GameWorld
        if (_gameWorld.SystemMessages != null)
            _gameWorld.SystemMessages.Clear();
    }
    
    // ========== NPC & RELATIONSHIPS ==========
    
    public List<TimeBlockServiceViewModel> GetTimeBlockServicePlan()
    {
        var player = _gameWorld.GetPlayer();
        if (player.CurrentLocation == null) return new List<TimeBlockServiceViewModel>();
        
        var timeBlockServicePlan = _npcRepository.GetTimeBlockServicePlan(player.CurrentLocation.Id);
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        
        return timeBlockServicePlan.Select(plan => new TimeBlockServiceViewModel
        {
            TimeBlock = plan.TimeBlock,
            IsCurrentTimeBlock = plan.TimeBlock == currentTimeBlock,
            AvailableServices = plan.AvailableServices,
            AvailableNPCs = plan.AvailableNPCs?.Select(npc => npc.Name).ToList() ?? new List<string>()
        }).ToList();
    }
    
    public List<NPCWithOffersViewModel> GetNPCsWithOffers()
    {
        var player = _gameWorld.GetPlayer();
        if (player.CurrentLocation == null) return new List<NPCWithOffersViewModel>();
        
        var currentTime = _timeManager.GetCurrentTimeBlock();
        var currentNPCs = _npcRepository.GetNPCsForLocationAndTime(player.CurrentLocation.Id, currentTime);
        
        return currentNPCs.Select(npc => new NPCWithOffersViewModel
        {
            NPCId = npc.ID,
            NPCName = npc.Name,
            Role = npc.Role,
            HasDirectOfferAvailable = _connectionTokenManager.HasEnoughTokensForDirectOffer(npc.ID),
            PendingOfferCount = _npcLetterOfferService.GetPendingOffersForNPC(npc.ID).Count,
            IsAvailable = npc.IsAvailable(currentTime)
        })
        .Where(npc => npc.HasDirectOfferAvailable || npc.PendingOfferCount > 0)
        .ToList();
    }
    
    public List<NPCRelationshipViewModel> GetNPCRelationships()
    {
        var player = _gameWorld.GetPlayer();
        var relationships = new List<NPCRelationshipViewModel>();
        
        // Get all NPCs from all locations
        var allLocations = _locationRepository.GetAllLocations();
        
        foreach (var location in allLocations)
        {
            var npcs = _npcRepository.GetNPCsForLocation(location.Id);
            foreach (var npc in npcs)
            {
                // Get tokens with this NPC
                var npcTokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
                var totalTokens = npcTokens.Values.Sum();
                
                if (totalTokens > 0)
                {
                    relationships.Add(new NPCRelationshipViewModel
                    {
                        NPCId = npc.ID,
                        NPCName = npc.Name,
                        Role = npc.Role,
                        LocationId = location.Id,
                        LocationName = location.Name,
                        ConnectionTokens = totalTokens,
                        CanMakeDirectOffer = _connectionTokenManager.HasEnoughTokensForDirectOffer(npc.ID),
                        TokensByType = npcTokens
                    });
                }
            }
        }
        
        return relationships.OrderByDescending(r => r.ConnectionTokens).ToList();
    }
    
    public List<ObligationViewModel> GetStandingObligations()
    {
        var obligations = new List<ObligationViewModel>();
        
        // Get active standing obligations from player
        var player = _gameWorld.GetPlayer();
        foreach (var obligation in player.StandingObligations.Where(o => o.IsActive))
        {
            obligations.Add(new ObligationViewModel
            {
                Name = obligation.Name,
                Description = obligation.GetEffectsSummary(),
                Type = obligation.Source,
                Priority = 1
            });
        }
        
        // Check for patron obligations based on leverage
        if (player.HasPatron && player.PatronLeverage > 0)
        {
            obligations.Add(new ObligationViewModel
            {
                Name = "Patron Employment",
                Description = $"Patron has {player.PatronLeverage} leverage over you",
                Type = "Patron",
                Priority = 2
            });
        }
        
        // Check for token debts (negative tokens)
        var allNpcs = _npcRepository.GetAllNPCs();
        foreach (var npc in allNpcs)
        {
            var tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            foreach (var tokenType in tokens.Where(t => t.Value < 0))
            {
                obligations.Add(new ObligationViewModel
                {
                    Name = $"Debt to {npc.Name}",
                    Description = $"{Math.Abs(tokenType.Value)} {tokenType.Key} tokens owed",
                    Type = "Debt",
                    Priority = 3
                });
            }
        }
        
        return obligations.OrderBy(o => o.Priority).ToList();
    }
}