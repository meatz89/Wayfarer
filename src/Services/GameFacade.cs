using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Constants;
using Wayfarer.ViewModels;

/// <summary>
/// GameFacade - THE single entry point for all UI-Backend communication.
/// This class delegates to existing UIServices and managers to maintain clean separation.
/// </summary>
public class GameFacade
{
    // Core dependencies
    private readonly GameWorld _gameWorld;
    private readonly GameWorldManager _gameWorldManager;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly CommandExecutor _commandExecutor;
    private readonly IGameRuleEngine _ruleEngine;
    
    // UI Services
    private readonly TravelUIService _travelService;
    private readonly LetterQueueUIService _letterQueueService;
    private readonly ReadableLetterUIService _readableLetterService;
    
    // Managers
    private readonly TravelManager _travelManager;
    private readonly RestManager _restManager;
    
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
    private readonly NPCLetterOfferService _letterOfferService;
    private readonly GameConfiguration _gameConfiguration;
    private readonly InformationDiscoveryManager _informationDiscoveryManager;
    private readonly StandingObligationManager _standingObligationManager;
    private readonly StandingObligationRepository _standingObligationRepository;
    private readonly CommandDiscoveryService _commandDiscoveryService;
    private readonly MarketManager _marketManager;
    private readonly LetterCategoryService _letterCategoryService;
    private readonly SpecialLetterGenerationService _specialLetterService;
    
    public GameFacade(
        GameWorld gameWorld,
        GameWorldManager gameWorldManager,
        ITimeManager timeManager,
        MessageSystem messageSystem,
        CommandExecutor commandExecutor,
        TravelUIService travelService,
        LetterQueueUIService letterQueueService,
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
        NPCLetterOfferService letterOfferService,
        GameConfiguration gameConfiguration,
        InformationDiscoveryManager informationDiscoveryManager = null,
        StandingObligationManager standingObligationManager = null,
        StandingObligationRepository standingObligationRepository = null,
        CommandDiscoveryService commandDiscoveryService = null,
        MarketManager marketManager = null,
        RestManager restManager = null,
        IGameRuleEngine ruleEngine = null,
        LetterCategoryService letterCategoryService = null,
        SpecialLetterGenerationService specialLetterService = null)
    {
        _gameWorld = gameWorld;
        _gameWorldManager = gameWorldManager;
        _timeManager = timeManager;
        _messageSystem = messageSystem;
        _commandExecutor = commandExecutor;
        _travelService = travelService;
        _letterQueueService = letterQueueService;
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
        _letterOfferService = letterOfferService;
        _gameConfiguration = gameConfiguration;
        _informationDiscoveryManager = informationDiscoveryManager;
        _standingObligationManager = standingObligationManager;
        _standingObligationRepository = standingObligationRepository;
        _commandDiscoveryService = commandDiscoveryService;
        _marketManager = marketManager;
        _restManager = restManager;
        _ruleEngine = ruleEngine;
        _letterCategoryService = letterCategoryService;
        _specialLetterService = specialLetterService;
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
    
    public int GetCurrentDay()
    {
        return _gameWorld.CurrentDay;
    }
    
    // ========== LOCATION ACTIONS ==========
    
    public LocationActionsViewModel GetLocationActions()
    {
        var player = _gameWorld.GetPlayer();
        var currentSpot = player.CurrentLocationSpot;

        if (currentSpot == null)
        {
            return new LocationActionsViewModel
            {
                LocationName = "Unknown Location",
                CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
                HoursRemaining = _timeManager.HoursRemaining,
                PlayerStamina = player.Stamina,
                PlayerCoins = player.Coins,
                ActionGroups = new List<ActionGroupViewModel>()
            };
        }

        // Discover available commands
        var discovery = _commandDiscoveryService?.DiscoverCommands(_gameWorld);
        
        // Get allowed command types from narrative if available
        HashSet<string> allowedCommandTypes = null;
        if (_narrativeManager != null && _narrativeManager.HasActiveNarrative())
        {
            allowedCommandTypes = _narrativeManager.GetAllowedCommandTypes();
        }

        var viewModel = new LocationActionsViewModel
        {
            LocationName = currentSpot.Name,
            CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
            HoursRemaining = _timeManager.HoursRemaining,
            PlayerStamina = player.Stamina,
            PlayerCoins = player.Coins,
            ActionGroups = new List<ActionGroupViewModel>()
        };

        if (discovery != null)
        {
            // Convert command categories to action groups
            foreach (var categoryGroup in discovery.CommandsByCategory)
            {
                var group = new ActionGroupViewModel
                {
                    ActionType = categoryGroup.Key.ToString(),
                    Actions = ConvertCommands(categoryGroup.Value, allowedCommandTypes)
                };

                viewModel.ActionGroups.Add(group);
            }
        }
        
        // Add closed services information
        AddClosedServicesInfo(viewModel, currentSpot);

        return viewModel;
    }
    
    public async Task<bool> ExecuteLocationActionAsync(string commandId)
    {
        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Starting execution for action: {commandId}");
        
        if (_commandDiscoveryService == null)
        {
            Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] CommandDiscoveryService not available");
            return false;
        }
        
        // Discover commands again to find the one to execute
        var discovery = _commandDiscoveryService.DiscoverCommands(_gameWorld);
        
        var commandToExecute = discovery.AllCommands.FirstOrDefault(c => c.UniqueId == commandId);

        if (commandToExecute == null)
        {
            Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Command not found: '{commandId}'");
            _messageSystem.AddSystemMessage($"Command not found: '{commandId}'", SystemMessageTypes.Danger);
            return false;
        }

        if (!commandToExecute.IsAvailable)
        {
            Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Command not available: {commandToExecute.UnavailableReason}");
            _messageSystem.AddSystemMessage(commandToExecute.UnavailableReason, SystemMessageTypes.Warning);
            return false;
        }

        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Executing command: {commandToExecute.Command.GetType().Name}");
        // Execute through CommandExecutor
        var result = await _commandExecutor.ExecuteAsync(commandToExecute.Command);

        Console.WriteLine($"[GameFacade.ExecuteLocationActionAsync] Command result: Success={result.IsSuccess}, Message={result.Message}");
        return result.IsSuccess;
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
        return new RestOptionsViewModel
        {
            RestOptions = GetRestOptionsList(),
            LocationActions = GetRestLocationActions(),
            WaitOptions = GetWaitOptionsList()
        };
    }
    
    public async Task<bool> ExecuteRestAsync(string restOptionId)
    {
        if (_restManager == null)
        {
            _messageSystem.AddSystemMessage("Rest manager not available", SystemMessageTypes.Danger);
            return false;
        }
        
        var option = _restManager.GetAvailableRestOptions().FirstOrDefault(o => o.Id == restOptionId);
        if (option == null)
        {
            _messageSystem.AddSystemMessage("Rest option not found", SystemMessageTypes.Danger);
            return false;
        }

        // Create rest command
        var restCommand = new RestCommand(
            _gameWorld.GetPlayer().CurrentLocationSpot?.SpotID,
            option.RestTimeHours,
            option.StaminaRecovery,
            _messageSystem
        );

        // Apply costs if any
        if (option.CoinCost > 0)
        {
            var spendCoinsCommand = new SpendCoinsCommand(option.CoinCost, $"Rest at {option.Name}");
            var coinResult = await _commandExecutor.ExecuteAsync(spendCoinsCommand);
            if (!coinResult.IsSuccess)
            {
                return false;
            }
        }

        // Execute rest command
        var result = await _commandExecutor.ExecuteAsync(restCommand);
        return result.IsSuccess;
    }
    
    private List<RestOptionViewModel> GetRestOptionsList()
    {
        if (_restManager == null)
            return new List<RestOptionViewModel>();
            
        var player = _gameWorld.GetPlayer();
        var restOptions = _restManager.GetAvailableRestOptions()
            .Where(o => o.StaminaRecovery > 0 && o.RestTimeHours >= 1)
            .ToList();

        return restOptions.Select(option =>
        {
            bool hasRequiredItem = string.IsNullOrEmpty(option.RequiredItem) ||
                                 player.Inventory.HasItem(option.RequiredItem);
            bool canAffordCoins = player.Coins >= option.CoinCost;
            bool isAvailable = canAffordCoins && hasRequiredItem;

            string unavailableReason = null;
            if (!canAffordCoins)
                unavailableReason = $"Need {option.CoinCost} coins";
            else if (!hasRequiredItem)
                unavailableReason = $"Need {option.RequiredItem}";

            return new RestOptionViewModel
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Name, // RestOption doesn't have Description, use Name
                StaminaRecovery = option.StaminaRecovery,
                CoinCost = option.CoinCost,
                TimeHours = option.RestTimeHours,
                RequiredItem = option.RequiredItem,
                CanAffordCoins = canAffordCoins,
                HasRequiredItem = hasRequiredItem,
                IsAvailable = isAvailable,
                UnavailableReason = unavailableReason
            };
        }).ToList();
    }
    
    private List<LocationActionViewModel> GetRestLocationActions()
    {
        if (_commandDiscoveryService == null)
            return new List<LocationActionViewModel>();
            
        // Discover commands and filter for Rest category
        var discovery = _commandDiscoveryService.DiscoverCommands(_gameWorld);
        var restCommands = discovery.CommandsByCategory
            .Where(kvp => kvp.Key == CommandCategory.Rest)
            .SelectMany(kvp => kvp.Value)
            .ToList();

        return restCommands.Select(cmd =>
        {
            string npcName = ExtractNPCName(cmd.DisplayName);
            var npc = string.IsNullOrEmpty(npcName) ? null : _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == npcName);

            return new LocationActionViewModel
            {
                Id = cmd.UniqueId,
                Description = cmd.Description,
                NPCName = npc?.Name,
                NPCProfession = npc?.Profession.ToString(),
                TimeCost = cmd.TimeCost,
                StaminaCost = cmd.StaminaCost,
                CoinCost = cmd.CoinCost,
                StaminaReward = ExtractStaminaReward(cmd.PotentialReward),
                IsAvailable = cmd.IsAvailable,
                UnavailableReason = cmd.UnavailableReason,
                CanBeRemedied = cmd.CanRemediate,
                RemediationHint = cmd.RemediationHint
            };
        }).ToList();
    }
    
    private List<WaitOptionViewModel> GetWaitOptionsList()
    {
        // Simple wait options - could be expanded
        return new List<WaitOptionViewModel>
        {
            new WaitOptionViewModel { Hours = 1, Description = "Wait 1 hour" },
            new WaitOptionViewModel { Hours = GameConstants.UI.WAIT_OPTION_SHORT_HOURS, Description = $"Wait {GameConstants.UI.WAIT_OPTION_SHORT_HOURS} hours" },
            new WaitOptionViewModel { Hours = GameConstants.UI.WAIT_OPTION_LONG_HOURS, Description = $"Wait {GameConstants.UI.WAIT_OPTION_LONG_HOURS} hours" }
        };
    }
    
    private int ExtractStaminaReward(string rewardDescription)
    {
        if (string.IsNullOrEmpty(rewardDescription))
            return 0;

        // Simple extraction from "+X stamina" format
        if (rewardDescription.Contains("stamina"))
        {
            string[] parts = rewardDescription.Split(' ');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].StartsWith("+") && parts[i + 1].StartsWith("stamina"))
                {
                    if (int.TryParse(parts[i].Substring(1), out int stamina))
                    {
                        return stamina;
                    }
                }
            }
        }
        return 0;
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
    
    public async Task<bool> LetterQueuePurgeAsync(List<TokenSelection> tokenSelections)
    {
        // Validate token selection
        if (tokenSelections == null || tokenSelections.Count == 0)
            return false;
            
        // InitiatePurgeAsync handles the conversation trigger
        await _letterQueueService.InitiatePurgeAsync(tokenSelections);
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
        var locationSpot = player.CurrentLocationSpot;
        if (locationSpot == null)
            return null;
            
        var location = _locationRepository.GetLocation(locationSpot.LocationId);
        if (location == null)
            return null;
            
        string marketStatus = _gameWorldManager.GetMarketAvailabilityStatus(location.Id);
        var traders = _gameWorldManager.GetTradingNPCs(location.Id)
            .Where(npc => npc.IsAvailable(_timeManager.GetCurrentTimeBlock()))
            .ToList();

        // Get all available items
        var marketItems = _gameWorldManager.GetAvailableMarketItems(location.Id);

        // Convert items to view models
        var allCategories = new HashSet<string> { "All" };
        var itemViewModels = new List<MarketItemViewModel>();

        foreach (var item in marketItems)
        {
            if (item == null) continue;

            bool canBuy = _gameWorldManager.CanBuyMarketItem(item.Id ?? item.Name, location.Id);
            bool canSell = player.Inventory.HasItem(item.Name);

            itemViewModels.Add(new MarketItemViewModel
            {
                ItemId = item.Id ?? item.Name,
                Name = item.Name,
                BuyPrice = item.BuyPrice,
                SellPrice = item.SellPrice,
                CanBuy = canBuy,
                CanSell = canSell,
                TraderId = location.Id, // Use location as trader ID for now since items are location-based
                Categories = item.Categories.Select(c => c.ToString()).ToList(),
                Item = item
            });

            // Collect categories
            foreach (var category in item.Categories)
            {
                allCategories.Add(category.ToString());
            }
        }

        // Build view model
        return new MarketViewModel
        {
            LocationName = location.Name,
            MarketStatus = marketStatus,
            IsOpen = marketStatus.Contains("Open"),
            TraderCount = traders.Count,
            PlayerCoins = player.Coins,
            InventoryUsed = player.Inventory.UsedCapacity,
            InventoryCapacity = player.Inventory.Size,
            Items = itemViewModels,
            AvailableCategories = allCategories.OrderBy(c => c).ToList()
        };
    }
    
    public async Task<bool> BuyItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, "buy", traderId);
    }
    
    public async Task<bool> SellItemAsync(string itemId, string traderId)
    {
        return await ExecuteMarketTradeAsync(itemId, "sell", traderId);
    }
    
    private async Task<bool> ExecuteMarketTradeAsync(string itemId, string action, string locationId)
    {
        if (_ruleEngine == null || _itemRepository == null)
        {
            _messageSystem.AddSystemMessage("Market trading not available", SystemMessageTypes.Danger);
            return false;
        }
        
        var tradeAction = action.ToLower() == "buy"
            ? MarketTradeCommand.TradeAction.Buy
            : MarketTradeCommand.TradeAction.Sell;

        var command = new MarketTradeCommand(
            itemId,
            tradeAction,
            locationId,
            _itemRepository,
            _ruleEngine);

        var result = await _commandExecutor.ExecuteAsync(command);

        if (result.IsSuccess)
        {
            _messageSystem.AddSystemMessage(result.Message, SystemMessageTypes.Success);
        }
        else
        {
            _messageSystem.AddSystemMessage(result.ErrorMessage, SystemMessageTypes.Danger);
        }

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
            PendingOfferCount = _letterOfferService.GetPendingOffersForNPC(npc.ID).Count,
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
    
    public DetailedObligationsViewModel GetDetailedObligations()
    {
        var vm = new DetailedObligationsViewModel();
        var player = _gameWorld.GetPlayer();
        
        if (_standingObligationManager == null || _standingObligationRepository == null)
        {
            return vm; // Return empty view model if managers not available
        }
        
        // Get active obligations
        foreach (var obligation in _standingObligationManager.GetActiveObligations())
        {
            var activeVm = new ActiveObligationViewModel
            {
                ID = obligation.ID,
                Name = obligation.Name,
                Description = obligation.Description,
                Source = obligation.Source,
                DaysSinceAccepted = obligation.DaysSinceAccepted,
                RelatedTokenType = obligation.RelatedTokenType,
                TokenCount = 0,
                HasForcedLetterWarning = false,
                DaysUntilForcedLetter = 0
            };
            
            // Add benefit descriptions
            foreach (var effect in obligation.BenefitEffects)
            {
                activeVm.BenefitDescriptions.Add(GetEffectDescription(effect));
            }
            
            // Add constraint descriptions
            foreach (var effect in obligation.ConstraintEffects)
            {
                activeVm.ConstraintDescriptions.Add(GetEffectDescription(effect));
            }
            
            // Get token count if applicable
            if (obligation.RelatedTokenType.HasValue)
            {
                activeVm.TokenCount = _connectionTokenManager.GetTokenCount(obligation.RelatedTokenType.Value);
            }
            
            // Check for forced letter warnings
            if (obligation.HasEffect(ObligationEffect.ShadowForced))
            {
                activeVm.HasForcedLetterWarning = obligation.DaysSinceLastForcedLetter >= 2;
                activeVm.DaysUntilForcedLetter = Math.Max(0, 3 - obligation.DaysSinceLastForcedLetter);
            }
            else if (obligation.HasEffect(ObligationEffect.PatronMonthly))
            {
                activeVm.HasForcedLetterWarning = obligation.DaysSinceLastForcedLetter >= 28;
                activeVm.DaysUntilForcedLetter = Math.Max(0, 30 - obligation.DaysSinceLastForcedLetter);
            }
            
            // Check for conflicts
            activeVm.HasConflicts = CheckForConflicts(obligation, _standingObligationManager.GetActiveObligations());
            
            vm.ActiveObligations.Add(activeVm);
        }
        
        // Get debt obligations
        foreach (var obligation in vm.ActiveObligations.Where(o => o.ID.StartsWith("debt_")))
        {
            var parts = obligation.ID.Split('_');
            if (parts.Length >= 3)
            {
                var npcId = parts[1];
                var npc = _npcRepository.GetById(npcId);
                if (npc != null && obligation.RelatedTokenType.HasValue)
                {
                    var tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
                    var debtAmount = tokens[obligation.RelatedTokenType.Value];
                    
                    var debtVm = new DebtObligationViewModel
                    {
                        ID = obligation.ID,
                        Name = obligation.Name,
                        Description = obligation.Description,
                        LeverageHolderName = npc.Name,
                        LeverageAmount = Math.Abs(debtAmount)
                    };
                    
                    // Add effect descriptions
                    debtVm.EffectDescriptions.Add($"Letters get {Math.Abs(debtAmount)} position priority");
                    if (obligation.ConstraintDescriptions.Any(c => c.Contains("Skip costs doubled")))
                        debtVm.EffectDescriptions.Add("Skip costs doubled");
                    if (obligation.ConstraintDescriptions.Any(c => c.Contains("Cannot refuse")))
                        debtVm.EffectDescriptions.Add("Cannot refuse letters");
                    
                    vm.DebtObligations.Add(debtVm);
                }
            }
        }
        
        // Check threshold warnings
        var allNpcs = _npcRepository.GetAllNPCs();
        foreach (var npc in allNpcs)
        {
            var tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            
            foreach (var (tokenType, balance) in tokens)
            {
                // Warning for approaching debt
                if (balance == -1 || balance == -2)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "Debt",
                        Message = $"At -3 {tokenType} tokens, '{GetDebtObligationName(tokenType)}' will activate",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = -3,
                        TokenType = tokenType
                    });
                }
                
                // Warning for extreme debt
                if (balance == -4)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "Debt",
                        Message = $"At -5 {tokenType} tokens, extreme leverage effects will apply",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = -5,
                        TokenType = tokenType
                    });
                }
                
                // Warning for high positive tokens
                if (balance == 4)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "HighTokens",
                        Message = $"At 5 {tokenType} tokens, new benefits may unlock",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = 5,
                        TokenType = tokenType
                    });
                }
                
                if (balance == 9)
                {
                    vm.ThresholdWarnings.Add(new ThresholdWarningViewModel
                    {
                        WarningType = "HighTokens",
                        Message = $"At 10 {tokenType} tokens, powerful benefits may activate",
                        NPCName = npc.Name,
                        CurrentValue = balance,
                        ThresholdValue = 10,
                        TokenType = tokenType
                    });
                }
            }
        }
        
        // Sort warnings by proximity to threshold
        vm.ThresholdWarnings = vm.ThresholdWarnings
            .OrderBy(w => Math.Abs(w.CurrentValue - w.ThresholdValue))
            .Take(5) // Show top 5 most relevant
            .ToList();
        
        // Get available templates
        var allTemplates = _standingObligationRepository.GetAllObligationTemplates();
        foreach (var template in allTemplates.Where(t => !HasObligation(t.ID, vm.ActiveObligations)))
        {
            var templateVm = new ObligationTemplateViewModel
            {
                ID = template.ID,
                Name = template.Name,
                Description = template.Description,
                TriggerDescription = template.Source,
                RequirementDescription = "Available through special letters",
                CanAccept = true
            };
            
            // Check for conflicts
            var conflicts = _standingObligationManager.CheckObligationConflicts(template);
            if (conflicts.Any())
            {
                templateVm.CanAccept = false;
                templateVm.CannotAcceptReason = "Conflicts with existing obligations";
                templateVm.ConflictingObligations = conflicts.Select(c => c.Name).ToList();
            }
            
            vm.AvailableTemplates.Add(templateVm);
        }
        
        return vm;
    }
    
    private bool CheckForConflicts(StandingObligation obligation, List<StandingObligation> allObligations)
    {
        return allObligations
            .Where(other => other.ID != obligation.ID && other.RelatedTokenType == obligation.RelatedTokenType)
            .Any(other => 
                (obligation.BenefitEffects.Any(b => other.ConstraintEffects.Contains(b)) ||
                 obligation.ConstraintEffects.Any(c => other.BenefitEffects.Contains(c))));
    }
    
    private bool HasObligation(string obligationId, List<ActiveObligationViewModel> activeObligations)
    {
        return activeObligations.Any(o => o.ID.Equals(obligationId, StringComparison.OrdinalIgnoreCase));
    }
    
    private string GetEffectDescription(ObligationEffect effect)
    {
        return effect switch
        {
            ObligationEffect.StatusPriority => "Status letters enter at slot 3",
            ObligationEffect.CommercePriority => "Commerce letters enter at slot 5",
            ObligationEffect.TrustPriority => "Trust letters enter at slot 7",
            ObligationEffect.PatronJumpToTop => "Patron letters jump to top slots",
            ObligationEffect.CommerceBonus => "Commerce letters +10 coins",
            ObligationEffect.CommerceBonusPlus3 => "Commerce letters +3 coins bonus",
            ObligationEffect.ShadowTriplePay => "Shadow letters pay triple",
            ObligationEffect.TrustFreeExtend => "Trust letters extend deadline free",
            ObligationEffect.ShadowForced => "Forced shadow letter every 3 days",
            ObligationEffect.PatronMonthly => "Monthly patron resource package",
            ObligationEffect.NoStatusRefusal => "Cannot refuse status letters",
            ObligationEffect.NoCommercePurge => "Cannot purge commerce letters",
            ObligationEffect.TrustSkipDoubleCost => "Skipping trust letters costs double",
            ObligationEffect.PatronLettersPosition1 => "Patron letters locked to position 1",
            ObligationEffect.PatronLettersPosition3 => "Patron letters locked to position 3",
            ObligationEffect.DeadlinePlus2Days => "All letters get +2 days deadline",
            ObligationEffect.CannotRefuseLetters => "Cannot refuse any letters",
            ObligationEffect.ShadowEqualsStatus => "Shadow letters use Status position",
            ObligationEffect.MerchantRespect => "Commerce 5+ tokens: +1 position",
            ObligationEffect.PatronAbsolute => "Patron letters push everything down",
            ObligationEffect.DebtSpiral => "All debts get extra leverage",
            ObligationEffect.DynamicLeverageModifier => "Leverage scales with tokens",
            ObligationEffect.DynamicPaymentBonus => "Payment scales with tokens",
            ObligationEffect.DynamicDeadlineBonus => "Deadline bonus scales with tokens",
            _ => effect.ToString()
        };
    }
    
    private string GetDebtObligationName(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "Personal Betrayal",
            ConnectionType.Commerce => "Outstanding Payment",
            ConnectionType.Status => "Social Disgrace",
            ConnectionType.Shadow => "Dangerous Enemy",
            _ => "Debt Obligation"
        };
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
    
    // ========== INFORMATION DISCOVERY ==========
    
    public InformationDiscoveryViewModel GetDiscoveredInformation()
    {
        var vm = new InformationDiscoveryViewModel();
        
        if (_informationDiscoveryManager == null)
        {
            // Information discovery not yet implemented
            return vm;
        }
        
        var allInfo = _informationDiscoveryManager.GetDiscoveredInformation();
        var player = _gameWorld.GetPlayer();
        
        foreach (var info in allInfo)
        {
            var infoVm = new DiscoveredInfoViewModel
            {
                Id = info.Id,
                Name = info.Name,
                Description = info.Description,
                Type = info.Type,
                Tier = info.Tier,
                IsAccessUnlocked = info.IsAccessUnlocked,
                DayDiscovered = info.DayDiscovered,
                TokenRequirements = info.TokenRequirements,
                SealRequirements = info.SealRequirements,
                EquipmentRequirements = info.EquipmentRequirements,
                CoinCost = info.CoinCost,
                CanBeUsedAsLeverage = info.CanBeUsedAsLeverage,
                LeverageValue = info.LeverageValue
            };
            
            // Check if player can afford to unlock
            infoVm.CanAfford = CanAffordInformationAccess(info);
            
            // Get leverage target name if applicable
            if (info.CanBeUsedAsLeverage && !string.IsNullOrEmpty(info.LeverageAgainstNpcId))
            {
                var npc = _npcRepository.GetById(info.LeverageAgainstNpcId);
                infoVm.LeverageTargetName = npc?.Name ?? "Unknown";
            }
            
            // Categorize by type
            switch (info.Type)
            {
                case InformationType.RouteExistence:
                    vm.RouteInformation.Add(infoVm);
                    break;
                case InformationType.LocationExistence:
                    vm.LocationInformation.Add(infoVm);
                    break;
                case InformationType.NPCExistence:
                    vm.NPCInformation.Add(infoVm);
                    break;
                case InformationType.ServiceAvailability:
                    vm.ServiceInformation.Add(infoVm);
                    break;
                case InformationType.SecretKnowledge:
                    vm.SecretInformation.Add(infoVm);
                    break;
            }
        }
        
        vm.TotalDiscovered = allInfo.Count;
        vm.TotalUnlocked = allInfo.Count(i => i.IsAccessUnlocked);
        
        return vm;
    }
    
    public async Task<bool> UnlockInformationAccessAsync(string informationId)
    {
        if (_informationDiscoveryManager == null)
            return false;
            
        var result = _informationDiscoveryManager.TryUnlockAccess(informationId);
        
        // Save will be handled by the command system
        
        return result;
    }
    
    public async Task<bool> UseInformationAsLeverageAsync(string informationId, string targetNpcId)
    {
        if (_informationDiscoveryManager == null)
            return false;
            
        var result = _informationDiscoveryManager.UseAsLeverage(informationId, targetNpcId);
        
        // Save will be handled by the command system
        
        return result;
    }
    
    private bool CanAffordInformationAccess(Information info)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check tokens
        foreach (var tokenReq in info.TokenRequirements)
        {
            if (!_connectionTokenManager.HasTokens(tokenReq.Key, tokenReq.Value))
                return false;
        }
        
        // Check seals
        foreach (var sealId in info.SealRequirements)
        {
            if (!player.Seals.Any(s => s.Id == sealId))
                return false;
        }
        
        // Check equipment
        foreach (var equipmentId in info.EquipmentRequirements)
        {
            if (!player.Inventory.HasItem(equipmentId))
                return false;
        }
        
        // Check coins
        if (player.Coins < info.CoinCost)
            return false;
            
        return true;
    }
    
    // ========== LOCATION ACTIONS HELPER METHODS ==========
    
    private List<ActionOptionViewModel> ConvertCommands(List<DiscoveredCommand> commands, HashSet<string> allowedCommandTypes = null)
    {
        return commands.Select(cmd => {
            // If commands reach this point, they've already been filtered by NarrativeManager
            // So during tutorial, all commands that made it here are allowed
            bool isInTutorial = _narrativeManager != null && _narrativeManager.IsNarrativeActive("wayfarer_tutorial");
            
            return new ActionOptionViewModel
            {
                Id = cmd.UniqueId,
                Description = cmd.DisplayName,
                NPCName = ExtractNPCName(cmd.DisplayName),

                // Costs
                TimeCost = cmd.TimeCost,
                StaminaCost = cmd.StaminaCost,
                CoinCost = cmd.CoinCost,

                // Affordability
                HasEnoughTime = cmd.TimeCost == 0 || _timeManager.HoursRemaining >= cmd.TimeCost,
                HasEnoughStamina = cmd.StaminaCost == 0 || _gameWorld.GetPlayer().Stamina >= cmd.StaminaCost,
                HasEnoughCoins = cmd.CoinCost == 0 || _gameWorld.GetPlayer().Coins >= cmd.CoinCost,

                // Rewards
                RewardsDescription = cmd.PotentialReward,

                // Availability
                IsAvailable = cmd.IsAvailable,
                UnavailableReasons = cmd.IsAvailable ? new List<string>() : new List<string> { cmd.UnavailableReason },
                
                // Tutorial allowed action - if we're in tutorial and command made it here, it's allowed
                IsAllowedInTutorial = !isInTutorial || true  // Always true if in tutorial since filtering already happened
            };
        }).ToList();
    }

    private string ExtractNPCName(string displayName)
    {
        // Simple extraction - could be improved
        if (displayName.Contains(" with "))
        {
            int startIndex = displayName.IndexOf(" with ") + 6;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" for "))
        {
            int startIndex = displayName.IndexOf(" for ") + 5;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" from "))
        {
            int startIndex = displayName.IndexOf(" from ") + 6;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" to "))
        {
            int startIndex = displayName.IndexOf(" to ") + 4;
            return displayName.Substring(startIndex);
        }
        return null;
    }
    
    private void AddClosedServicesInfo(LocationActionsViewModel viewModel, LocationSpot currentSpot)
    {
        // Skip adding closed service info during tutorial
        bool isInTutorial = _narrativeManager != null && _narrativeManager.IsNarrativeActive("wayfarer_tutorial");
        if (isInTutorial)
        {
            return;
        }
        
        // Check for Letter Board availability
        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            ActionOptionViewModel letterBoardInfo = new ActionOptionViewModel
            {
                Id = "letter_board_closed",
                Description = "Visit Letter Board",
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = GetNextAvailableTime(TimeBlocks.Dawn),
                ServiceSchedule = "Available only at Dawn",
                UnavailableReasons = new List<string> { "Letter Board is closed. Only available during Dawn hours." }
            };
            
            // Add to Special category
            ActionGroupViewModel specialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Special.ToString());
            if (specialGroup == null)
            {
                specialGroup = new ActionGroupViewModel
                {
                    ActionType = CommandCategory.Special.ToString(),
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(specialGroup);
            }
            specialGroup.Actions.Add(letterBoardInfo);
        }
        
        // Check for Market availability
        if (_marketManager != null)
        {
            string marketStatus = _marketManager.GetMarketAvailabilityStatus(currentSpot.LocationId);
            if (!marketStatus.Contains("Market Open"))
            {
                List<NPC> allTraders = _marketManager.GetAllTraders(currentSpot.LocationId);
                if (allTraders.Any())
                {
                    string schedule = GetTradersSchedule(allTraders);
                    string nextAvailable = GetNextMarketAvailable(currentSpot.LocationId, allTraders);
                    
                    ActionOptionViewModel marketInfo = new ActionOptionViewModel
                    {
                        Id = "market_closed",
                        Description = "Browse Market",
                        IsAvailable = false,
                        IsServiceClosed = true,
                        NextAvailableTime = nextAvailable,
                        ServiceSchedule = schedule,
                        UnavailableReasons = new List<string> { marketStatus }
                    };
                    
                    // Add to Economic category
                    ActionGroupViewModel economicGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Economic.ToString());
                    if (economicGroup == null)
                    {
                        economicGroup = new ActionGroupViewModel
                        {
                            ActionType = CommandCategory.Economic.ToString(),
                            Actions = new List<ActionOptionViewModel>()
                        };
                        viewModel.ActionGroups.Add(economicGroup);
                    }
                    economicGroup.Actions.Add(marketInfo);
                }
            }
        }
        
        // Check for missing NPCs and their schedules
        List<NPC> allNPCs = _npcRepository.GetNPCsForLocation(currentSpot.LocationId);
        List<NPC> currentNPCs = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpot.SpotID, _timeManager.GetCurrentTimeBlock());
        List<NPC> missingNPCs = allNPCs.Where(npc => !currentNPCs.Any(c => c.ID == npc.ID)).ToList();
        
        foreach (NPC missingNPC in missingNPCs)
        {
            string schedule = GetNPCSchedule(missingNPC);
            string nextAvailable = GetNextNPCAvailable(missingNPC);
            
            // Add info about when this NPC will be available
            ActionOptionViewModel npcInfo = new ActionOptionViewModel
            {
                Id = $"npc_unavailable_{missingNPC.ID}",
                Description = $"Wait for {missingNPC.Name}",
                NPCName = missingNPC.Name,
                NPCProfession = missingNPC.Profession.ToString(),
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = nextAvailable,
                ServiceSchedule = schedule,
                UnavailableReasons = new List<string> { $"{missingNPC.Name} is not here right now." }
            };
            
            // Add to Social category
            ActionGroupViewModel socialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Social.ToString());
            if (socialGroup == null)
            {
                socialGroup = new ActionGroupViewModel
                {
                    ActionType = CommandCategory.Social.ToString(),
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(socialGroup);
            }
            socialGroup.Actions.Add(npcInfo);
        }
    }
    
    private string GetNextAvailableTime(TimeBlocks targetTime)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        int currentHour = _timeManager.GetCurrentTimeHours();
        
        // Calculate hours until target time
        int hoursUntilTarget = CalculateHoursUntilTimeBlock(currentTime, targetTime, currentHour);
        
        if (hoursUntilTarget <= 0)
        {
            // It's available next day
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
        else if (hoursUntilTarget == 1)
        {
            return "Available in 1 hour";
        }
        else if (hoursUntilTarget < 12)
        {
            return $"Available in {hoursUntilTarget} hours";
        }
        else
        {
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
    }
    
    private string GetTradersSchedule(List<NPC> traders)
    {
        List<string> schedules = new List<string>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> availableTimes = GetNPCAvailableTimes(trader);
            if (availableTimes.Any())
            {
                string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
                schedules.Add($"{trader.Name}: {timeList}");
            }
        }
        return string.Join("; ", schedules);
    }
    
    private string GetNPCSchedule(NPC npc)
    {
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);
        if (!availableTimes.Any())
        {
            return "Schedule unknown";
        }
        
        string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
        return $"Available: {timeList}";
    }
    
    private List<TimeBlocks> GetNPCAvailableTimes(NPC npc)
    {
        List<TimeBlocks> availableTimes = new List<TimeBlocks>();
        TimeBlocks[] allTimes = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
        
        foreach (TimeBlocks time in allTimes)
        {
            if (npc.IsAvailable(time))
            {
                availableTimes.Add(time);
            }
        }
        
        return availableTimes;
    }
    
    private string GetNextNPCAvailable(NPC npc)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);
        
        if (!availableTimes.Any())
        {
            return "Availability unknown";
        }
        
        // Find next available time
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, availableTimes);
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }
        
        return "Available tomorrow";
    }
    
    private string GetNextMarketAvailable(string locationId, List<NPC> traders)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Find all times when at least one trader is available
        HashSet<TimeBlocks> marketTimes = new HashSet<TimeBlocks>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> traderTimes = GetNPCAvailableTimes(trader);
            foreach (TimeBlocks time in traderTimes)
            {
                marketTimes.Add(time);
            }
        }
        
        if (!marketTimes.Any())
        {
            return "Market schedule unknown";
        }
        
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, marketTimes.ToList());
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }
        
        return "Available tomorrow";
    }
    
    private TimeBlocks? GetNextAvailableTimeBlock(TimeBlocks current, List<TimeBlocks> availableTimes)
    {
        TimeBlocks[] timeOrder = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
        int currentIndex = Array.IndexOf(timeOrder, current);
        
        // Check remaining times today
        for (int i = currentIndex + 1; i < timeOrder.Length; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }
        
        // Check times tomorrow (starting from Dawn)
        for (int i = 0; i <= currentIndex; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }
        
        return null;
    }
    
    private int CalculateHoursUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentHour)
    {
        // Map time blocks to hour ranges
        int targetStartHour = target switch
        {
            TimeBlocks.Dawn => 4,
            TimeBlocks.Morning => 8,
            TimeBlocks.Afternoon => 12,
            TimeBlocks.Evening => 17,
            TimeBlocks.Night => 20,
            _ => 0
        };
        
        if (targetStartHour > currentHour)
        {
            return targetStartHour - currentHour;
        }
        else
        {
            // Next day
            return (24 - currentHour) + targetStartHour;
        }
    }
    
    private string GetTimeBlockDisplayName(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "Dawn (4-8 AM)",
            TimeBlocks.Morning => "Morning (8 AM-12 PM)",
            TimeBlocks.Afternoon => "Afternoon (12-5 PM)",
            TimeBlocks.Evening => "Evening (5-8 PM)",
            TimeBlocks.Night => "Night (8 PM-4 AM)",
            _ => timeBlock.ToString()
        };
    }
    
    // ========== TOKEN MANAGEMENT ==========
    
    public NPCTokenBalance GetTokensWithNPC(string npcId)
    {
        var tokenDict = _connectionTokenManager?.GetTokensWithNPC(npcId) ?? new Dictionary<ConnectionType, int>();
        
        var balance = new NPCTokenBalance();
        
        foreach (var kvp in tokenDict)
        {
            balance.Balances.Add(new TokenBalance
            {
                TokenType = kvp.Key,
                Amount = kvp.Value
            });
        }
        
        return balance;
    }
    
    public int GetTokenCount(ConnectionType tokenType)
    {
        return _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
    }
    
    // ========== NPC & LOCATION QUERIES ==========
    
    public List<NPC> GetAllNPCs()
    {
        return _npcRepository?.GetAllNPCs() ?? new List<NPC>();
    }
    
    public NPC GetNPCById(string npcId)
    {
        return _npcRepository?.GetById(npcId);
    }
    
    public NPC GetNPCByName(string name)
    {
        return _npcRepository?.GetByName(name);
    }
    
    public Location GetLocation(string locationId)
    {
        return _locationRepository?.GetLocation(locationId);
    }
    
    // ========== LETTER SERVICES ==========
    
    public bool CanNPCOfferLetters(string npcId)
    {
        // Check if NPC has tokens to offer letters
        var npc = _npcRepository?.GetById(npcId);
        if (npc == null) return false;
        
        // Check if NPC can offer letters based on tokens
        var tokens = _connectionTokenManager?.GetTokensWithNPC(npcId);
        return tokens != null && tokens.Any(t => t.Value > 0);
    }
    
    public List<SpecialLetterOption> GetAvailableSpecialLetters(string npcId)
    {
        return _specialLetterService?.GetAvailableSpecialLetters(npcId) ?? new List<SpecialLetterOption>();
    }
    
    public bool RequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        return _specialLetterService?.RequestSpecialLetter(npcId, tokenType) ?? false;
    }
    
    public LetterCategory GetAvailableCategory(string npcId, ConnectionType tokenType)
    {
        return _letterCategoryService?.GetAvailableCategory(npcId, tokenType) ?? LetterCategory.Basic;
    }
    
    public int GetTokensToNextCategory(string npcId, ConnectionType tokenType)
    {
        return _letterCategoryService?.GetTokensToNextCategory(npcId, tokenType) ?? 0;
    }
    
    public (int min, int max) GetCategoryPaymentRange(LetterCategory category)
    {
        return _letterCategoryService?.GetCategoryPaymentRange(category) ?? (0, 0);
    }
    
    // ========== ITEM MANAGEMENT ==========
    
    public Item GetItemById(string itemId)
    {
        return _itemRepository?.GetItemById(itemId);
    }
    
    public async Task<bool> ReadLetterAsync(string itemId)
    {
        if (_readableLetterService == null) return false;
        return await _readableLetterService.ReadLetterAsync(itemId);
    }
    
    // ========== LETTER QUEUE MANAGEMENT ==========
    
    public int GetLetterQueueCount()
    {
        return _gameWorld.GetPlayer()?.CarriedLetters?.Count ?? 0;
    }
    
    public bool IsLetterQueueFull()
    {
        var count = GetLetterQueueCount();
        return count >= 8; // MAX_LETTER_QUEUE_SIZE
    }
    
    public int AddLetterWithObligationEffects(Letter letter)
    {
        if (letter == null || IsLetterQueueFull())
            return -1;
            
        // Add letter to player's queue
        var player = _gameWorld.GetPlayer();
        player.CarriedLetters.Add(letter);
        
        // Calculate position with obligation effects
        if (_standingObligationManager != null)
        {
            int basePosition = player.CarriedLetters.Count;
            int positionModifier = 0;
            if (letter.TokenType == ConnectionType.Status)
            {
                var obligations = _standingObligationManager.GetActiveObligations();
                foreach (var obligation in obligations)
                {
                    if (obligation.HasEffect(ObligationEffect.StatusPriority))
                        positionModifier = -2; // Move up to position 3
                }
            }
            int finalPosition = Math.Max(1, Math.Min(basePosition + positionModifier, 8));
            
            // Reorder queue if needed
            if (finalPosition != basePosition)
            {
                player.CarriedLetters.Remove(letter);
                player.CarriedLetters.Insert(finalPosition - 1, letter);
            }
            
            return finalPosition;
        }
        
        return player.CarriedLetters.Count;
    }
    
    public bool IsActionForbidden(string actionType, Letter letter, out string reason)
    {
        reason = null;
        
        if (_standingObligationManager == null)
            return false;
            
        return _standingObligationManager.IsActionForbidden(actionType, letter, out reason);
    }
    
    // ========== NOTICE BOARD ==========
    
    public List<NoticeBoardOption> GetNoticeBoardOptions()
    {
        var options = new List<NoticeBoardOption>();
        
        // Define the three standard notice board options
        options.Add(new NoticeBoardOption
        {
            Id = "anything_heading",
            Name = "Anything heading...",
            Description = "Ask about letters going to specific locations",
            TokenCost = 2,
            OptionType = NoticeBoardOptionType.AnythingHeading,
            RequiresDirection = true
        });
        
        options.Add(new NoticeBoardOption
        {
            Id = "looking_for_work",
            Name = "Looking for work...",
            Description = "Request letters of a specific type",
            TokenCost = 3,
            OptionType = NoticeBoardOptionType.LookingForWork,
            RequiresTokenType = true
        });
        
        options.Add(new NoticeBoardOption
        {
            Id = "urgent_deliveries",
            Name = "Urgent deliveries?",
            Description = "High-paying letters with tight deadlines",
            TokenCost = 5,
            OptionType = NoticeBoardOptionType.UrgentDeliveries
        });
        
        return options;
    }
    
    public bool CanAffordNoticeBoardOption(NoticeBoardOption option)
    {
        if (option == null)
            return false;
            
        var player = _gameWorld.GetPlayer();
        var totalTokens = 0;
        
        // Count all tokens
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            totalTokens += _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
        }
        
        return totalTokens >= option.TokenCost;
    }
    
    public async Task<bool> ExecuteNoticeBoardOption(NoticeBoardOption option, string direction = null)
    {
        if (option == null || !CanAffordNoticeBoardOption(option))
            return false;
            
        var player = _gameWorld.GetPlayer();
        
        // Deduct tokens (randomly from available types)
        var availableTokens = new List<(ConnectionType type, int count)>();
        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            int count = _connectionTokenManager?.GetTokenCount(tokenType) ?? 0;
            if (count > 0)
                availableTokens.Add((tokenType, count));
        }
        
        if (!availableTokens.Any())
            return false;
            
        // Deduct tokens randomly
        var random = new Random();
        int tokensToDeduct = option.TokenCost;
        
        while (tokensToDeduct > 0 && availableTokens.Any())
        {
            int index = random.Next(availableTokens.Count);
            var (tokenType, count) = availableTokens[index];
            
            // Find an NPC to deduct from
            var allNpcs = _npcRepository?.GetAllNPCs() ?? new List<NPC>();
            foreach (var npc in allNpcs)
            {
                var npcTokens = _connectionTokenManager?.GetTokensWithNPC(npc.ID);
                if (npcTokens != null && npcTokens.ContainsKey(tokenType) && npcTokens[tokenType] > 0)
                {
                    // Deduct token directly from player's balance with NPC
                    var playerTokens = _gameWorld.GetPlayer().NPCTokens;
                    if (playerTokens.ContainsKey(npc.ID) && playerTokens[npc.ID].ContainsKey(tokenType))
                    {
                        playerTokens[npc.ID][tokenType]--;
                        if (playerTokens[npc.ID][tokenType] <= 0)
                        {
                            playerTokens[npc.ID].Remove(tokenType);
                            if (playerTokens[npc.ID].Count == 0)
                                playerTokens.Remove(npc.ID);
                        }
                    }
                    tokensToDeduct--;
                    
                    availableTokens[index] = (tokenType, count - 1);
                    if (availableTokens[index].count == 0)
                        availableTokens.RemoveAt(index);
                    
                    break;
                }
            }
        }
        
        // Generate a letter based on the option type
        Letter generatedLetter = null;
        
        switch (option.OptionType)
        {
            case NoticeBoardOptionType.AnythingHeading:
                // Generate letter heading to specified direction
                generatedLetter = GenerateDirectionalLetter(direction);
                break;
                
            case NoticeBoardOptionType.LookingForWork:
                // Generate letter of specific type (would need token type parameter)
                generatedLetter = GenerateTypedLetter(ConnectionType.Commerce); // Default for now
                break;
                
            case NoticeBoardOptionType.UrgentDeliveries:
                // Generate urgent high-paying letter
                generatedLetter = GenerateUrgentLetter();
                break;
        }
        
        if (generatedLetter != null)
        {
            int position = AddLetterWithObligationEffects(generatedLetter);
            if (position > 0)
            {
                _messageSystem?.AddSystemMessage(
                    $"📬 Letter added to your queue at position {position}!",
                    SystemMessageTypes.Success
                );
                return true;
            }
        }
        
        return false;
    }
    
    private Letter GenerateDirectionalLetter(string direction)
    {
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Notice Board Client",
            RecipientName = $"Recipient in {direction}",
            Payment = 20 + new Random().Next(10),
            DeadlineInDays = 3 + new Random().Next(2),
            TokenType = (ConnectionType)new Random().Next(4)
        };
        
        return letter;
    }
    
    private Letter GenerateTypedLetter(ConnectionType tokenType)
    {
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = $"{tokenType} Contact",
            RecipientName = $"{tokenType} Recipient",
            Payment = tokenType == ConnectionType.Shadow ? 40 : 25,
            DeadlineInDays = 4,
            TokenType = tokenType
        };
        
        return letter;
    }
    
    private Letter GenerateUrgentLetter()
    {
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Urgent Client",
            RecipientName = "Time-Sensitive Recipient",
            Payment = 50 + new Random().Next(20),
            DeadlineInDays = 2,
            TokenType = (ConnectionType)new Random().Next(4),
            PhysicalProperties = LetterPhysicalProperties.Fragile
        };
        
        return letter;
    }
}