using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Constants;
using Wayfarer.ViewModels;

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
    private readonly GameConfiguration _gameConfiguration;
    
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
        NPCLetterOfferService npcLetterOfferService,
        GameConfiguration gameConfiguration)
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
        _gameConfiguration = gameConfiguration;
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
        var location = player.CurrentLocationSpot != null 
            ? _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId)
            : null;
        return (location, player.CurrentLocationSpot);
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
        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Starting execution for action: {commandId}");
        var result = await _locationActionsService.ExecuteActionAsync(commandId);
        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Action {commandId} completed with result: {result}");
        return result;
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
                TotalStaminaCost = route.TotalStaminaCost,
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
    
    public TravelContextViewModel GetTravelContext()
    {
        var player = _gameWorld.GetPlayer();
        var totalWeight = CalculateTotalWeight();
        var carriedLetters = player.CarriedLetters;
        
        // Calculate weight status
        string weightStatus;
        string weightClass;
        int baseStaminaPenalty;
        
        if (totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX)
        {
            weightStatus = "Light load";
            weightClass = "";
            baseStaminaPenalty = 0;
        }
        else if (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX)
        {
            weightStatus = "Medium load (+1 stamina)";
            weightClass = "warning";
            baseStaminaPenalty = GameConstants.LoadWeight.MEDIUM_LOAD_STAMINA_PENALTY;
        }
        else
        {
            weightStatus = "Heavy load (+2 stamina)";
            weightClass = "danger";
            baseStaminaPenalty = GameConstants.LoadWeight.HEAVY_LOAD_STAMINA_PENALTY;
        }
        
        // Check letter properties
        bool hasHeavyLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Heavy)) ?? false;
        bool hasFragileLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Fragile)) ?? false;
        bool hasBulkyLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Bulky)) ?? false;
        bool hasPerishableLetters = carriedLetters?.Any(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Perishable)) ?? false;
        
        // Determine letter warning
        string letterWarning = "";
        if (hasHeavyLetters)
            letterWarning = "Heavy letters +1 stamina cost";
        else if (hasBulkyLetters)
            letterWarning = "Bulky letters restrict movement";
        else if (hasPerishableLetters)
            letterWarning = "Perishable letters - time sensitive";
        else if (hasFragileLetters)
            letterWarning = "Fragile letters need careful handling";
        
        // Get equipment categories
        var equipmentCategories = new List<ItemCategory>();
        foreach (var itemName in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                var item = _itemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    equipmentCategories.AddRange(item.Categories);
                }
            }
        }
        
        return new TravelContextViewModel
        {
            CurrentStamina = player.Stamina,
            TotalWeight = totalWeight,
            WeightStatus = weightStatus,
            WeightClass = weightClass,
            BaseStaminaPenalty = baseStaminaPenalty,
            CarriedLetterCount = carriedLetters?.Count ?? 0,
            HasHeavyLetters = hasHeavyLetters,
            HasFragileLetters = hasFragileLetters,
            HasBulkyLetters = hasBulkyLetters,
            HasPerishableLetters = hasPerishableLetters,
            LetterWarning = letterWarning,
            CurrentEquipmentCategories = equipmentCategories.Distinct().ToList(),
            CurrentWeather = _gameWorld.CurrentWeather,
            WeatherIcon = GetWeatherIcon(_gameWorld.CurrentWeather)
        };
    }
    
    public List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes()
    {
        var currentLocation = _locationRepository.GetCurrentLocation();
        var destinations = new List<TravelDestinationViewModel>();
        var travelContext = GetTravelContext();
        
        // Get all locations that can be traveled to
        var allLocations = _locationRepository.GetAllLocations();
        
        foreach (var location in allLocations)
        {
            if (location.Id == currentLocation.Id) continue;
            
            // Get all routes to this destination
            var allRoutes = _routeRepository.GetRoutesFromLocation(currentLocation.Id)
                .Where(r => r.Destination == location.Id)
                .ToList();
            
            if (!allRoutes.Any()) continue;
            
            var routeViewModels = new List<TravelRouteViewModel>();
            
            foreach (var route in allRoutes)
            {
                routeViewModels.Add(CreateTravelRouteViewModel(route, travelContext));
            }
            
            bool canTravel = routeViewModels.Any(r => r.CanTravel);
            
            destinations.Add(new TravelDestinationViewModel
            {
                LocationId = location.Id,
                LocationName = location.Name,
                Description = location.Description,
                CanTravel = canTravel,
                CannotTravelReason = !canTravel ? "No available routes" : null,
                MinimumCost = routeViewModels.Where(r => r.CanTravel).Select(r => r.CoinCost).DefaultIfEmpty(0).Min(),
                MinimumTime = routeViewModels.Where(r => r.CanTravel).Select(r => r.TimeCost).DefaultIfEmpty(0).Min(),
                IsCurrent = false,
                Routes = routeViewModels
            });
        }
        
        return destinations;
    }
    
    private TravelRouteViewModel CreateTravelRouteViewModel(RouteOption route, TravelContextViewModel travelContext)
    {
        var player = _gameWorld.GetPlayer();
        
        // Calculate costs
        var coinCost = _travelManager.CalculateCoinCost(route);
        var baseStaminaCost = _travelManager.CalculateStaminaCost(route);
        var letterStaminaPenalty = travelContext.HasHeavyLetters ? 1 : 0;
        var totalStaminaCost = baseStaminaCost + letterStaminaPenalty;
        
        // Check if can travel
        var canAffordStamina = player.Stamina >= totalStaminaCost;
        var canAffordCoins = player.Coins >= coinCost;
        var accessInfo = _travelManager.GetRouteAccessInfo(route);
        var tokenAccessInfo = _travelManager.GetTokenAccessInfo(route);
        
        var canTravel = canAffordStamina && canAffordCoins && accessInfo.IsAllowed && tokenAccessInfo.IsAllowed;
        
        // Build token requirements
        var tokenRequirements = new Dictionary<string, RouteTokenRequirementViewModel>();
        
        if (route.AccessRequirement != null)
        {
            // Type-based requirements
            foreach (var typeReq in route.AccessRequirement.RequiredTokensPerType)
            {
                var currentCount = _connectionTokenManager.GetTotalTokensOfType(typeReq.TokenType);
                var key = $"type_{typeReq.TokenType}";
                tokenRequirements[key] = new RouteTokenRequirementViewModel
                {
                    RequirementKey = key,
                    RequiredAmount = typeReq.MinimumCount,
                    CurrentAmount = currentCount,
                    DisplayName = $"{typeReq.TokenType} tokens (any NPC)",
                    Icon = GetTokenIcon(typeReq.TokenType),
                    IsMet = currentCount >= typeReq.MinimumCount
                };
            }
            
            // NPC-specific requirements
            foreach (var tokenReq in route.AccessRequirement.RequiredTokensPerNPC)
            {
                var npcTokens = _connectionTokenManager.GetTokensWithNPC(tokenReq.NPCId);
                var currentCount = npcTokens.Values.Sum();
                var npc = _npcRepository.GetById(tokenReq.NPCId);
                var key = $"npc_{tokenReq.NPCId}";
                tokenRequirements[key] = new RouteTokenRequirementViewModel
                {
                    RequirementKey = key,
                    RequiredAmount = tokenReq.MinimumCount,
                    CurrentAmount = currentCount,
                    DisplayName = $"tokens with {npc?.Name ?? tokenReq.NPCId}",
                    Icon = "🎭",
                    IsMet = currentCount >= tokenReq.MinimumCount
                };
            }
        }
        
        // Build warnings
        var warnings = new List<string>(accessInfo.Warnings);
        
        // Add fragile letter warnings
        if (travelContext.HasFragileLetters && 
            (route.TerrainCategories.Contains(TerrainCategory.Requires_Climbing) || 
             route.TerrainCategories.Contains(TerrainCategory.Wilderness_Terrain)))
        {
            warnings.Insert(0, "Fragile letters at risk on this route!");
        }
        
        // Get discovery options if route is locked
        var discoveryOptions = new List<RouteDiscoveryOptionViewModel>();
        if (!route.IsDiscovered)
        {
            // Note: RouteDiscoveryManager would need to be injected for this to work fully
            // For now, leaving empty as it would require more refactoring
        }
        
        // Determine cannot travel reason
        string cannotTravelReason = null;
        if (!canTravel)
        {
            if (!accessInfo.IsAllowed)
                cannotTravelReason = accessInfo.BlockingReason;
            else if (!tokenAccessInfo.IsAllowed)
                cannotTravelReason = tokenAccessInfo.BlockedMessage;
            else if (!canAffordCoins)
                cannotTravelReason = "Not enough coins";
            else if (!canAffordStamina)
                cannotTravelReason = "Not enough stamina";
        }
        
        return new TravelRouteViewModel
        {
            RouteId = route.Id,
            RouteName = route.Name,
            Description = route.Description,
            TransportMethod = route.Method,
            TimeCost = route.TravelTimeHours,
            BaseStaminaCost = route.BaseStaminaCost,
            TotalStaminaCost = totalStaminaCost,
            CoinCost = coinCost,
            CanTravel = canTravel,
            CannotTravelReason = cannotTravelReason,
            IsDiscovered = route.IsDiscovered,
            IsBlocked = !accessInfo.IsAllowed || !tokenAccessInfo.IsAllowed,
            BlockingReason = !tokenAccessInfo.IsAllowed ? tokenAccessInfo.BlockedMessage : accessInfo.BlockingReason,
            HintMessage = tokenAccessInfo.HintMessage,
            Warnings = warnings,
            TerrainCategories = route.TerrainCategories,
            DepartureTime = route.DepartureTime,
            TokenRequirements = tokenRequirements,
            DiscoveryOptions = discoveryOptions
        };
    }
    
    public int CalculateTotalWeight()
    {
        return _gameWorldManager.CalculateTotalWeight();
    }
    
    private string GetWeatherIcon(WeatherCondition weather)
    {
        return weather switch
        {
            WeatherCondition.Clear => "☀️",
            WeatherCondition.Rain => "🌧️",
            WeatherCondition.Snow => "❄️",
            WeatherCondition.Fog => "🌫️",
            _ => "❓"
        };
    }
    
    private string GetTokenIcon(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "💝",
            ConnectionType.Commerce => "🤝",
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌑",
            _ => "🎭"
        };
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
        
        // Process the choice
        var outcome = await currentConversation.ProcessPlayerChoice(choice);
        
        // If conversation is complete, clear it from state manager
        if (outcome.IsConversationComplete)
        {
            _conversationStateManager.ClearPendingConversation();
        }
        else
        {
            // Generate new choices for the next beat
            await currentConversation.ProcessNextBeat();
        }
        
        return CreateConversationViewModel(currentConversation);
    }
    
    public ConversationViewModel GetCurrentConversation()
    {
        Console.WriteLine($"[GameFacade.GetCurrentConversation] Called");
        Console.WriteLine($"[GameFacade.GetCurrentConversation] ConversationPending: {_conversationStateManager.ConversationPending}");
        Console.WriteLine($"[GameFacade.GetCurrentConversation] PendingConversationManager null? {_conversationStateManager.PendingConversationManager == null}");
        
        var currentConversation = _conversationStateManager.PendingConversationManager;
        if (currentConversation == null || !_conversationStateManager.ConversationPending)
        {
            Console.WriteLine($"[GameFacade.GetCurrentConversation] Returning null - no pending conversation");
            return null;
        }
        
        var viewModel = CreateConversationViewModel(currentConversation);
        Console.WriteLine($"[GameFacade.GetCurrentConversation] Created ViewModel with NpcId: {viewModel?.NpcId}, Text: {viewModel?.CurrentText?.Substring(0, Math.Min(50, viewModel?.CurrentText?.Length ?? 0))}");
        return viewModel;
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
            DeadlineDays = offer.DeadlineInDays,
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
        var location = player.CurrentLocationSpot != null 
            ? _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId)
            : null;
        return location != null ? _marketService.GetMarketViewModel(location.Id) : null;
    }
    
    public async Task<bool> BuyItemAsync(string itemId, string traderId)
    {
        // traderId is actually locationId in our current implementation
        // since items are sold by location, not specific traders
        return await _marketService.ExecuteTradeAsync(itemId, "buy", traderId);
    }
    
    public async Task<bool> SellItemAsync(string itemId, string traderId)
    {
        // traderId is actually locationId in our current implementation
        // since items are sold by location, not specific traders
        return await _marketService.ExecuteTradeAsync(itemId, "sell", traderId);
    }
    
    // ========== DEBT MANAGEMENT ==========
    
    public DebtViewModel GetDebtInformation()
    {
        var player = _gameWorld.GetPlayer();
        var currentDay = _gameWorld.CurrentDay;
        var viewModel = new DebtViewModel
        {
            PlayerCoins = player.Coins
        };
        
        // Get active debts
        foreach (var debt in player.ActiveDebts.Where(d => !d.IsPaid))
        {
            var creditor = _npcRepository.GetById(debt.CreditorId);
            if (creditor != null)
            {
                var totalOwed = debt.GetTotalOwed(currentDay);
                var accruedInterest = totalOwed - debt.Principal;
                
                viewModel.ActiveDebts.Add(new DebtInfo
                {
                    CreditorId = debt.CreditorId,
                    CreditorName = creditor.Name,
                    Principal = debt.Principal,
                    InterestRate = debt.InterestRate,
                    DaysActive = currentDay - debt.StartDay,
                    AccruedInterest = accruedInterest,
                    TotalOwed = totalOwed,
                    TokenDebt = GetTokenDebt(debt.CreditorId)
                });
            }
        }
        
        viewModel.TotalDebt = viewModel.ActiveDebts.Sum(d => d.TotalOwed);
        viewModel.TotalDailyInterest = viewModel.ActiveDebts.Sum(d => (int)(d.Principal * d.InterestRate / 100.0));
        
        // Get available lenders (merchants at current location)
        var currentLocation = player.CurrentLocationSpot;
        if (currentLocation != null)
        {
            var npcsAtLocation = _npcRepository.GetNPCsForLocationSpotAndTime(
                currentLocation.SpotID, 
                _timeManager.GetCurrentTimeBlock());
                
            foreach (var npc in npcsAtLocation)
            {
                // Check if NPC can lend (has Commerce or Shadow tokens)
                if ((npc.LetterTokenTypes.Contains(ConnectionType.Commerce) || 
                     npc.LetterTokenTypes.Contains(ConnectionType.Shadow)) &&
                    !viewModel.ActiveDebts.Any(d => d.CreditorId == npc.ID))
                {
                    viewModel.AvailableLenders.Add(new LenderInfo
                    {
                        NPCId = npc.ID,
                        Name = npc.Name,
                        LocationId = currentLocation.LocationId,
                        LocationName = _locationRepository.GetLocation(currentLocation.LocationId)?.Name ?? "Unknown",
                        MaxLoanAmount = 100,
                        InterestRate = 5,
                        IsAvailable = true
                    });
                }
            }
        }
        
        return viewModel;
    }
    
    public async Task<bool> BorrowMoneyAsync(string npcId)
    {
        var command = new BorrowMoneyCommand(
            npcId,
            _npcRepository,
            _connectionTokenManager,
            _messageSystem,
            _gameConfiguration);
            
        var validation = command.CanExecute(_gameWorld);
        if (!validation.IsValid)
        {
            _messageSystem.AddSystemMessage(validation.FailureReason, SystemMessageTypes.Warning);
            return false;
        }
        
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    public async Task<bool> RepayDebtAsync(string npcId, int amount)
    {
        var command = new RepayDebtCommand(
            npcId,
            amount,
            _npcRepository,
            _messageSystem);
            
        var validation = command.CanExecute(_gameWorld);
        if (!validation.IsValid)
        {
            _messageSystem.AddSystemMessage(validation.FailureReason, SystemMessageTypes.Warning);
            return false;
        }
        
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    private int GetTokenDebt(string creditorId)
    {
        var tokens = _connectionTokenManager.GetTokensWithNPC(creditorId);
        var commerceTokens = tokens[ConnectionType.Commerce];
        return commerceTokens < 0 ? Math.Abs(commerceTokens) : 0;
    }
    
    // ========== PERSONAL ERRANDS ==========
    
    public PersonalErrandViewModel GetPersonalErrands()
    {
        var viewModel = new PersonalErrandViewModel();
        var player = _gameWorld.GetPlayer();
        
        // Get player status
        viewModel.PlayerStatus.CurrentStamina = player.Stamina;
        
        // Count medicine items
        foreach (var itemId in player.Inventory.ItemSlots.Where(id => !string.IsNullOrEmpty(id)))
        {
            var item = _itemRepository.GetItemById(itemId);
            if (item != null && item.Categories.Contains(ItemCategory.Medicine))
            {
                viewModel.PlayerStatus.MedicineCount++;
                viewModel.PlayerStatus.MedicineItems.Add(item.Name);
            }
        }
        
        // Get all NPCs with 2+ tokens
        var allNPCs = _npcRepository.GetAllNPCs();
        foreach (var npc in allNPCs)
        {
            var tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            var totalTokens = tokens.Values.Sum();
            
            if (totalTokens >= 2)
            {
                // Create personal errand command to check if valid
                var command = new PersonalErrandCommand(
                    npc.ID,
                    _npcRepository,
                    _connectionTokenManager,
                    _messageSystem,
                    _itemRepository);
                    
                var validation = command.CanExecute(_gameWorld);
                
                var errand = new AvailableErrand
                {
                    NPCId = npc.ID,
                    NPCName = npc.Name,
                    NPCProfession = npc.Profession.ToString().Replace('_', ' '),
                    LocationId = npc.Location,
                    LocationName = _locationRepository.GetLocation(npc.Location)?.Name ?? "Unknown",
                    Description = GetErrandDescription(npc),
                    CanPerform = validation.IsValid,
                    BlockingReason = validation.FailureReason,
                    PlayerTokens = totalTokens,
                    HasMedicine = viewModel.PlayerStatus.MedicineCount > 0,
                    ContextualDescription = GetErrandContext(npc)
                };
                
                viewModel.AvailableErrands.Add(errand);
            }
        }
        
        return viewModel;
    }
    
    public async Task<bool> ExecutePersonalErrandAsync(string npcId)
    {
        var command = new PersonalErrandCommand(
            npcId,
            _npcRepository,
            _connectionTokenManager,
            _messageSystem,
            _itemRepository);
            
        var validation = command.CanExecute(_gameWorld);
        if (!validation.IsValid)
        {
            _messageSystem.AddSystemMessage(validation.FailureReason, SystemMessageTypes.Warning);
            return false;
        }
        
        var result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }
    
    private string GetErrandDescription(NPC npc)
    {
        return npc.Profession switch
        {
            Professions.Dock_Boss => "Their daughter needs medicine urgently",
            Professions.Merchant => "They need help finding important documents",
            Professions.Noble => "They require a discreet remedy delivery",
            Professions.Innkeeper => "A regular patron has fallen ill",
            Professions.TavernKeeper => "Emergency supplies needed at the tavern",
            Professions.Scholar => "Rare ink ingredients are desperately needed",
            Professions.Craftsman => "Workshop accident requires healing salve",
            Professions.Soldier => "Wounds that can't be officially reported",
            _ => "They need help with an urgent personal matter"
        };
    }
    
    private string GetErrandContext(NPC npc)
    {
        return npc.Profession switch
        {
            Professions.Dock_Boss => "Delivered medicine to their sick daughter",
            Professions.Merchant => "Found their missing ledger before the audit",
            Professions.Noble => "Discreetly delivered a remedy for their ailment",
            Professions.Innkeeper => "Helped treat a regular patron who fell ill",
            Professions.TavernKeeper => "Brought supplies for an emergency at the tavern",
            Professions.Scholar => "Retrieved rare ink ingredients they desperately needed",
            Professions.Craftsman => "Delivered healing salve for a workshop accident",
            Professions.Soldier => "Brought medicine for wounds they couldn't report",
            _ => "Helped with an urgent personal matter"
        };
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
        if (player.CurrentLocationSpot == null) return new List<TimeBlockServiceViewModel>();
        
        var location = _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
        var timeBlockServicePlan = _npcRepository.GetTimeBlockServicePlan(location.Id);
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
        if (player.CurrentLocationSpot == null) return new List<NPCWithOffersViewModel>();
        
        var currentTime = _timeManager.GetCurrentTimeBlock();
        var location = _locationRepository.GetLocation(player.CurrentLocationSpot.LocationId);
        var currentNPCs = _npcRepository.GetNPCsForLocationAndTime(location.Id, currentTime);
        
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
    
    // ========== SEAL MANAGEMENT ==========
    
    public SealProgressionViewModel GetSealProgression()
    {
        var viewModel = new SealProgressionViewModel();
        var player = _gameWorld.GetPlayer();
        
        // Get all owned seals
        foreach (var seal in player.OwnedSeals)
        {
            var ownedSeal = new OwnedSeal
            {
                Id = seal.Id,
                Name = seal.GetFullName(),
                Type = seal.Type,
                Tier = seal.Tier,
                Description = seal.Description,
                Material = seal.Material,
                Insignia = seal.Insignia,
                DayIssued = seal.DayIssued,
                IssuingGuild = GetGuildNameFromId(seal.IssuingGuildId),
                IsWorn = player.WornSeals.Contains(seal),
                Benefits = GetSealBenefits(seal)
            };
            viewModel.OwnedSeals.Add(ownedSeal);
        }
        
        // Get worn seals with slot numbers
        int slotNumber = 1;
        foreach (var seal in player.WornSeals)
        {
            viewModel.WornSeals.Add(new WornSeal
            {
                Id = seal.Id,
                Name = seal.GetFullName(),
                Type = seal.Type,
                Tier = seal.Tier,
                SlotNumber = slotNumber++
            });
        }
        
        // Get endorsement progress for each seal type
        foreach (SealType sealType in Enum.GetValues<SealType>())
        {
            var progress = new EndorsementProgress
            {
                Type = sealType,
                TypeName = sealType.ToString()
            };
            
            // Get current endorsement count
            string endorsementKey = $"endorsements_delivered_{sealType}";
            var memory = player.GetMemory(endorsementKey);
            progress.CurrentEndorsements = memory?.Importance ?? 0;
            
            // Get current seal tier
            var currentSeal = player.OwnedSeals.FirstOrDefault(s => s.Type == sealType);
            progress.CurrentTier = currentSeal?.Tier ?? (SealTier)0; // 0 means no seal
            
            // Determine next tier and requirements
            if (progress.CurrentTier < SealTier.Master)
            {
                progress.NextTier = progress.CurrentTier == 0 ? SealTier.Apprentice : progress.CurrentTier + 1;
                progress.EndorsementsToNext = GetEndorsementsRequired(progress.NextTier.Value) - progress.CurrentEndorsements;
                progress.NextTierBenefits = GetTierBenefits(sealType, progress.NextTier.Value);
            }
            
            // Get guild locations for this seal type
            progress.GuildLocations = GetGuildLocationsForSealType(sealType);
            
            viewModel.EndorsementTracking.Add(progress);
        }
        
        return viewModel;
    }
    
    public async Task<bool> EquipSealAsync(string sealId)
    {
        var player = _gameWorld.GetPlayer();
        var seal = player.OwnedSeals.FirstOrDefault(s => s.Id == sealId);
        
        if (seal == null)
        {
            _messageSystem.AddSystemMessage("You don't own this seal.", SystemMessageTypes.Danger);
            return false;
        }
        
        if (player.WornSeals.Contains(seal))
        {
            _messageSystem.AddSystemMessage("This seal is already equipped.", SystemMessageTypes.Warning);
            return false;
        }
        
        if (player.WornSeals.Count >= 3)
        {
            _messageSystem.AddSystemMessage("You can only wear 3 seals at once. Unequip one first.", SystemMessageTypes.Warning);
            return false;
        }
        
        bool success = player.EquipSeal(seal);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Equipped {seal.GetFullName()}.", SystemMessageTypes.Success);
        }
        
        return success;
    }
    
    public async Task<bool> UnequipSealAsync(string sealId)
    {
        var player = _gameWorld.GetPlayer();
        var seal = player.WornSeals.FirstOrDefault(s => s.Id == sealId);
        
        if (seal == null)
        {
            _messageSystem.AddSystemMessage("This seal is not equipped.", SystemMessageTypes.Warning);
            return false;
        }
        
        bool success = player.UnequipSeal(seal);
        if (success)
        {
            _messageSystem.AddSystemMessage($"Unequipped {seal.GetFullName()}.", SystemMessageTypes.Success);
        }
        
        return success;
    }
    
    private List<string> GetSealBenefits(Seal seal)
    {
        var benefits = new List<string>();
        
        // Base benefits by type
        switch (seal.Type)
        {
            case SealType.Commerce:
                benefits.Add("Better prices at markets");
                benefits.Add("Access to merchant guild services");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Priority trading with guild members");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Exclusive trade route information");
                break;
                
            case SealType.Status:
                benefits.Add("Recognition by nobles and scholars");
                benefits.Add("Access to restricted libraries");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Invitation to exclusive events");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Authority to issue recommendations");
                break;
                
            case SealType.Shadow:
                benefits.Add("Safe passage through dangerous areas");
                benefits.Add("Access to black market contacts");
                if (seal.Tier >= SealTier.Journeyman)
                    benefits.Add("Protection from rival gangs");
                if (seal.Tier >= SealTier.Master)
                    benefits.Add("Control over underground networks");
                break;
        }
        
        return benefits;
    }
    
    private string GetTierBenefits(SealType type, SealTier tier)
    {
        return (type, tier) switch
        {
            (SealType.Commerce, SealTier.Apprentice) => "Basic trading privileges and guild recognition",
            (SealType.Commerce, SealTier.Journeyman) => "Full merchant rights and priority trading",
            (SealType.Commerce, SealTier.Master) => "Elite status with exclusive trade routes",
            (SealType.Status, SealTier.Apprentice) => "Recognition among educated classes",
            (SealType.Status, SealTier.Journeyman) => "Library access and event invitations",
            (SealType.Status, SealTier.Master) => "Academic authority and teaching rights",
            (SealType.Shadow, SealTier.Apprentice) => "Safe passage and underground contacts",
            (SealType.Shadow, SealTier.Journeyman) => "Gang protection and safe house access",
            (SealType.Shadow, SealTier.Master) => "Network control and criminal authority",
            _ => "Guild recognition and privileges"
        };
    }
    
    private int GetEndorsementsRequired(SealTier tier)
    {
        return tier switch
        {
            SealTier.Apprentice => 3,
            SealTier.Journeyman => 5,
            SealTier.Master => 7,
            _ => 3
        };
    }
    
    private List<string> GetGuildLocationsForSealType(SealType type)
    {
        return type switch
        {
            SealType.Commerce => new List<string> { "Merchant Guild", "Courier's Guild" },
            SealType.Status => new List<string> { "Scholar's Atheneum" },
            SealType.Shadow => new List<string> { "Underground Guild Halls" },
            _ => new List<string>()
        };
    }
    
    private string GetGuildNameFromId(string guildId)
    {
        return guildId switch
        {
            "merchant_guild" => "Merchant Guild",
            "messenger_guild" => "Courier's Guild",
            "scholar_guild" => "Scholar's Atheneum",
            _ => "Unknown Guild"
        };
    }
}