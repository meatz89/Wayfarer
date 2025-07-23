using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Core location actions that consume hours
/// Each action is independent and has its own purpose
/// </summary>
public enum LocationAction
{
    // Basic Actions (1 hour each)
    Converse,       // Talk with someone
    Work,           // Earn coins through labor
    Socialize,      // Build relationship with NPC
    Rest,           // Recover stamina
    Collect,        // Get physical letter from sender
    Deliver,        // Complete letter delivery
    Trade,          // Buy or sell items
    
    // Environmental Actions (1 hour each)
    GatherResources,    // Gather berries, herbs, etc. based on location
    Browse,             // Browse market stalls or notice boards
    Observe,            // Listen to gossip or watch for opportunities
    
    // Debt Actions (1 hour each)
    RequestPatronFunds,     // Request money from patron (-1 token)
    RequestPatronEquipment, // Request equipment from patron (-2 tokens)
    BorrowMoney,           // Borrow from NPC (-2 tokens)
    PleedForAccess,        // Maintain route access (-1 token)
    AcceptIllegalWork,     // Accept shadow work (-1 token)
    
    // Travel Encounters (0 hours - happen during travel)
    TravelEncounter        // Handle obstacle or event during travel
}

/// <summary>
/// Manages location-based actions that consume hours and other resources.
/// Each action has a clear resource cost and benefit.
/// </summary>
public class LocationActionManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    private readonly ConversationFactory _conversationFactory;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly NPCLetterOfferService _letterOfferService;
    private readonly LetterCategoryService _letterCategoryService;
    private readonly NoticeBoardService _noticeBoardService;
    private readonly DebugLogger _debugLogger;
    private readonly FlagService _flagService;
    private readonly NarrativeManager _narrativeManager;
    private readonly NarrativeRequirementFactory _narrativeRequirementFactory;
    
    private TimeManager _timeManager => _gameWorld.TimeManager;
    
    public LocationActionManager(
        GameWorld gameWorld, 
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager,
        LetterQueueManager letterQueueManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository,
        ConversationFactory conversationFactory,
        RouteDiscoveryManager routeDiscoveryManager,
        NPCLetterOfferService letterOfferService,
        LetterCategoryService letterCategoryService,
        NoticeBoardService noticeBoardService,
        DebugLogger debugLogger,
        FlagService flagService,
        NarrativeManager narrativeManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
        _letterQueueManager = letterQueueManager;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
        _conversationFactory = conversationFactory;
        _routeDiscoveryManager = routeDiscoveryManager;
        _letterOfferService = letterOfferService;
        _letterCategoryService = letterCategoryService;
        _noticeBoardService = noticeBoardService;
        _debugLogger = debugLogger;
        _flagService = flagService;
        _narrativeManager = narrativeManager;
        _narrativeRequirementFactory = new NarrativeRequirementFactory(narrativeManager, flagService);
    }
    
    
    /// <summary>
    /// Get available actions at current location
    /// </summary>
    public List<ActionOption> GetAvailableActions()
    {
        var player = _gameWorld.GetPlayer();
        var location = player.CurrentLocation;
        var spot = player.CurrentLocationSpot;
        var actions = new List<ActionOption>();
        
        _debugLogger.LogAction("GetAvailableActions", $"{location?.Name ?? "null"}/{spot?.SpotID ?? "null"}", "Starting");
        
        if (location == null || spot == null)
        {
            _debugLogger.LogWarning("LocationActionManager", "No location or spot - returning empty actions");
            return actions;
        }
        
        // Get NPCs at current location
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        _debugLogger.LogDebug($"Getting NPCs for spot '{spot.SpotID}' at time {currentTimeBlock}");
        var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTimeBlock);
        
        // Filter NPCs through narrative manager if active
        if (_narrativeManager.HasActiveNarrative())
        {
            _debugLogger.LogDebug($"Filtering {npcsHere.Count} NPCs through narrative manager");
            npcsHere = npcsHere.Where(npc => _narrativeManager.ShouldShowNPC(npc.ID)).ToList();
            _debugLogger.LogDebug($"After narrative filter: {npcsHere.Count} NPCs remain");
        }
        
        _debugLogger.LogDebug($"Found {npcsHere.Count} NPCs at current spot");
        
        // NOTE: Basic rest is handled in RestUI screen via RestManager
        // Only add NPC-specific contextual actions here
        
        // ALL actions must come from NPCs present at the spot
        foreach (var npc in npcsHere)
        {
            // Check NPC availability in current time period
            if (!IsNPCAvailable(npc)) continue;
            
            // Add basic NPC interactions
            AddBasicNPCActions(npc, actions);
            
            // Add profession-specific actions
            AddProfessionSpecificActions(npc, actions, npcsHere);
            
            // Add letter-related actions
            AddLetterActions(npc, actions);
            
            // Add debt/emergency actions specific to this NPC
            AddNPCEmergencyActions(npc, actions);
        }
        
        // Environmental actions based on location spot properties
        AddEnvironmentalActions(actions, spot);
        
        // Patron request actions - available at writing locations
        AddPatronRequestActions(actions);
        
        // Filter actions through narrative manager if active
        if (_narrativeManager.HasActiveNarrative())
        {
            _debugLogger.LogDebug($"Filtering {actions.Count} actions through narrative manager");
            var filteredActions = _narrativeManager.FilterActions(actions.Select(a => a.Action).ToList());
            actions = actions.Where(a => filteredActions.Contains(a.Action)).ToList();
            _debugLogger.LogDebug($"After narrative filter: {actions.Count} actions remain");
        }
        
        return actions;
    }
    
    /// <summary>
    /// Generate travel encounter action based on route terrain
    /// </summary>
    public ActionOption GenerateTravelEncounterAction(RouteOption route)
    {
        var player = _gameWorld.GetPlayer();
        var encounterType = DetermineEncounterType(route);
        
        return new ActionOption
        {
            Action = LocationAction.TravelEncounter,
            Name = GetEncounterName(encounterType),
            Description = GetEncounterDescription(encounterType),
            HourCost = 0, // Travel encounters don't consume extra hours
            StaminaCost = 0, // Base stamina cost, choices may add more
            CoinCost = 0,
            InitialNarrative = GetEncounterNarrative(encounterType, route),
            EncounterType = encounterType
        };
    }
    
    private TravelEncounterType DetermineEncounterType(RouteOption route)
    {
        // Determine encounter based on terrain categories
        if (route.TerrainCategories.Contains(TerrainCategory.Wilderness_Terrain))
            return TravelEncounterType.WildernessObstacle;
        if (route.TerrainCategories.Contains(TerrainCategory.Dark_Passage))
            return TravelEncounterType.DarkChallenge;
        if (route.TerrainCategories.Contains(TerrainCategory.Exposed_Weather))
            return TravelEncounterType.WeatherEvent;
        if (route.TerrainCategories.Contains(TerrainCategory.Heavy_Cargo_Route))
            return TravelEncounterType.MerchantEncounter;
            
        return TravelEncounterType.FellowTraveler; // Default encounter
    }
    
    private string GetEncounterName(TravelEncounterType encounterType)
    {
        return encounterType switch
        {
            TravelEncounterType.WildernessObstacle => "Path Blocked",
            TravelEncounterType.DarkChallenge => "Darkness Falls",
            TravelEncounterType.WeatherEvent => "Storm Approaches",
            TravelEncounterType.MerchantEncounter => "Merchant in Need",
            TravelEncounterType.FellowTraveler => "Fellow Traveler",
            _ => "Unexpected Event"
        };
    }
    
    private string GetEncounterDescription(TravelEncounterType encounterType)
    {
        return encounterType switch
        {
            TravelEncounterType.WildernessObstacle => "Navigate around a fallen tree",
            TravelEncounterType.DarkChallenge => "Find your way in the darkness",
            TravelEncounterType.WeatherEvent => "Deal with sudden bad weather",
            TravelEncounterType.MerchantEncounter => "Help a stranded merchant",
            TravelEncounterType.FellowTraveler => "Meet someone on the road",
            _ => "Handle an unexpected situation"
        };
    }
    
    private string GetEncounterNarrative(TravelEncounterType encounterType, RouteOption route)
    {
        return encounterType switch
        {
            TravelEncounterType.WildernessObstacle => $"A large tree has fallen across the {route.Name}, blocking your path completely.",
            TravelEncounterType.DarkChallenge => "The sun sets earlier than expected, plunging the path into near-complete darkness.",
            TravelEncounterType.WeatherEvent => "Dark clouds gather overhead, and the first drops of rain begin to fall.",
            TravelEncounterType.MerchantEncounter => "A merchant's cart sits broken by the roadside, its owner looking distressed.",
            TravelEncounterType.FellowTraveler => "You encounter another traveler heading in the opposite direction.",
            _ => "Something unexpected occurs during your journey."
        };
    }
    
    /// <summary>
    /// Add basic NPC interaction actions (converse, socialize)
    /// </summary>
    private void AddBasicNPCActions(NPC npc, List<ActionOption> actions)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var totalTokens = tokens.Values.Sum();
        
        // Basic conversation - always available when NPC is present
        if (_timeManager.HoursRemaining >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Converse,
                Name = $"Talk with {npc.Name}",
                Description = npc.Description,
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = totalTokens == 0 ? "Meet someone new" : "Catch up"
            });
        }
        
        // Socialize - spend quality time if relationship exists
        if (_timeManager.HoursRemaining >= 1 && totalTokens > 0)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Socialize,
                Name = $"Spend time with {npc.Name}",
                Description = "Deepen your connection",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = $"+1 {npc.LetterTokenTypes.FirstOrDefault()} token"
            });
        }
    }
    
    /// <summary>
    /// Add profession-specific actions for an NPC
    /// </summary>
    private void AddProfessionSpecificActions(NPC npc, List<ActionOption> actions, List<NPC> allNPCsPresent)
    {
        var player = _gameWorld.GetPlayer();
        var currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Work actions are available even without existing relationship!
        switch (npc.Profession)
        {
            case Professions.Merchant:
                // Merchants offer work during business hours
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= GameRules.STAMINA_COST_WORK &&
                    (currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon))
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Help {npc.Name} with inventory",
                        Description = "Earn coins through honest labor",
                        HourCost = 1,
                        StaminaCost = GameRules.STAMINA_COST_WORK,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                
                // Trade option - check for compound opportunities
                if (_timeManager.HoursRemaining >= 1)
                {
                    var tradeEffect = GetTradeCompoundEffect(npc);
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Trade,
                        Name = $"Trade with {npc.Name}",
                        Description = "Buy or sell goods",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = tradeEffect
                    });
                }
                break;
                
            case Professions.TavernKeeper:
                // Tavern keepers offer work in evenings
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= 2 &&
                    currentTime == TimeBlocks.Evening)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Serve drinks for {npc.Name}",
                        Description = "Help with the evening rush",
                        HourCost = 1,
                        StaminaCost = 2,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                
                // Drink and rest option (if you have coins)
                if (_timeManager.HoursRemaining >= 1 && player.Coins >= 3)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Rest,
                        Name = $"Buy drinks and rest with {npc.Name}",
                        Description = "Rest while building connection",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 3,
                        NPCId = npc.ID,
                        Effect = $"+3 Stamina, +1 {npc.LetterTokenTypes.FirstOrDefault()} token"
                    });
                }
                break;
                
            case Professions.Scribe:
                // Scribes offer copying work
                if (_timeManager.HoursRemaining >= 1 && player.Stamina >= 1)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Copy documents for {npc.Name}",
                        Description = "Careful work with quill and ink",
                        HourCost = 1,
                        StaminaCost = 1,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+3 coins"
                    });
                }
                break;
                
            case Professions.Noble:
                // Nobles don't offer work directly, but may have other opportunities later
                break;
                
            case Professions.Innkeeper:
                // Innkeepers offer room cleaning work
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= 2 &&
                    currentTime == TimeBlocks.Morning)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Clean rooms for {npc.Name}",
                        Description = "Morning cleaning duties",
                        HourCost = 1,
                        StaminaCost = 2,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                break;
        }
        
        // Special case: Baker work at dawn (if this is a merchant baker)
        if (npc.Profession == Professions.Merchant && 
            npc.Name.ToLower().Contains("baker") &&
            currentTime == TimeBlocks.Dawn &&
            player.Stamina >= 1 &&
            _timeManager.HoursRemaining >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Work,
                Name = $"Help {npc.Name} with morning baking",
                Description = "Early work with benefits",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "+2 coins, +1 bread (restores 2 stamina when eaten)"
            });
        }
    }
    
    /// <summary>
    /// Add letter-related actions for an NPC
    /// </summary>
    private void AddLetterActions(NPC npc, List<ActionOption> actions)
    {
        // Check for letter work - if NPC has potential letters and relationship exists
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var totalTokens = tokens.Values.Sum();
        
        if (_timeManager.HoursRemaining >= 1 && totalTokens > 0)
        {
            // Check if NPC can offer letters
            var potentialOffers = _letterOfferService.GenerateNPCLetterOffers(npc.ID);
            if (potentialOffers.Any())
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Converse,
                    Name = $"Ask {npc.Name} about letter work",
                    Description = "Check if they need letters delivered",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    NPCId = npc.ID,
                    Effect = "May offer letter delivery work",
                    InitialNarrative = $"You ask {npc.Name} if they have any letters that need delivering.",
                    IsLetterOffer = true // Mark this as a letter offer conversation
                });
            }
        }
        
        // Deliver - if this NPC is a letter recipient
        if (_timeManager.HoursRemaining >= 1 && IsLetterRecipient(npc.ID))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Deliver,
                Name = $"Deliver letter to {npc.Name}",
                Description = "Complete your obligation",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "Earn payment and tokens",
                InitialNarrative = $"You approach {npc.Name} with their awaited correspondence."
            });
        }
        
        // Collect - if this NPC has letters to give
        var lettersFromNPC = GetAcceptedLettersFromNPC(npc.ID);
        foreach (var letter in lettersFromNPC)
        {
            if (_timeManager.HoursRemaining >= 1)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Collect,
                    Name = $"Collect letter from {npc.Name}",
                    Description = $"Get the physical letter",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    NPCId = npc.ID,
                    LetterId = letter.Id,
                    Effect = $"Uses {letter.GetRequiredSlots()} inventory slots",
                    InitialNarrative = $"{npc.Name} hands you a sealed letter with instructions for safe delivery."
                });
            }
        }
    }
    
    /// <summary>
    /// Add emergency actions specific to an NPC
    /// </summary>
    private void AddNPCEmergencyActions(NPC npc, List<ActionOption> actions)
    {
        var player = _gameWorld.GetPlayer();
        
        if (_timeManager.HoursRemaining < 1) return;
        
        // Borrow money from merchants when broke
        if (player.Coins < 10 && npc.Profession == Professions.Merchant)
        {
            var tradeTokens = _tokenManager.GetTokensWithNPC(npc.ID)[ConnectionType.Trade];
            actions.Add(new ActionOption
            {
                Action = LocationAction.BorrowMoney,
                Name = $"Borrow money from {npc.Name}",
                Description = $"20 coins now, -2 Trade leverage (current: {tradeTokens})",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "20 coins, -2 Trade tokens"
            });
        }
        
        // Accept illegal work from shadow NPCs
        if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow) && player.Coins < 15)
        {
            var shadowTokens = _tokenManager.GetTokensWithNPC(npc.ID)[ConnectionType.Shadow];
            actions.Add(new ActionOption
            {
                Action = LocationAction.AcceptIllegalWork,
                Name = $"Accept questionable job from {npc.Name}",
                Description = $"30 coins, but they'll have leverage (current: {shadowTokens})",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "30 coins, -1 Shadow token (they have dirt on you)"
            });
        }
    }
    
    /// <summary>
    /// Execute an action - now launches conversation for thin narrative layer
    /// </summary>
    public async Task<bool> ExecuteAction(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check narrative requirements first
        var narrativeRequirement = _narrativeRequirementFactory.CreateForAction(option.Action, option.NPCId);
        if (!narrativeRequirement.IsSatisfiedBy(player))
        {
            _messageSystem.AddSystemMessage(
                $"❌ {narrativeRequirement.GetFailureReason()}",
                SystemMessageTypes.Warning
            );
            return false;
        }
        
        // Validate resources
        if (!_timeManager.CanPerformAction(option.HourCost))
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough hours remaining! Need {option.HourCost}, have {_timeManager.HoursRemaining}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        if (player.Stamina < option.StaminaCost)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough stamina! Need {option.StaminaCost}, have {player.Stamina}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        if (player.Coins < option.CoinCost)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough coins! Need {option.CoinCost}, have {player.Coins}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Store the action for post-conversation processing
        _gameWorld.PendingAction = option;
        
        // Create conversation context for the action
        var context = new ActionConversationContext
        {
            GameWorld = _gameWorld,
            Player = player,
            LocationName = player.CurrentLocation?.Name ?? "",
            LocationSpotName = player.CurrentLocationSpot?.Name ?? "",
            TargetNPC = option.NPCId != null ? _npcRepository.GetNPCById(option.NPCId) : null,
            ConversationTopic = option.IsLetterOffer ? "LetterOffer" : $"Action_{option.Action}",
            SourceAction = option,
            InitialNarrative = option.InitialNarrative
        };
        
        // Check for narrative-specific conversation overrides
        if (_narrativeManager.HasActiveNarrative() && option.Action == LocationAction.Converse && context.TargetNPC != null)
        {
            var narrativeIntro = _narrativeManager.GetNarrativeIntroduction(context);
            if (!string.IsNullOrEmpty(narrativeIntro))
            {
                context.InitialNarrative = narrativeIntro;
            }
        }
        
        // If this is a letter offer conversation, add available letter templates
        if (option.IsLetterOffer && context.TargetNPC != null)
        {
            var offers = _letterOfferService.GenerateNPCLetterOffers(option.NPCId);
            if (offers.Any())
            {
                context.AvailableTemplates = offers.Select(offer => new ChoiceTemplate
                {
                    Purpose = $"Accept {offer.Category} letter",
                    Description = $"{offer.Category} delivery for {offer.Payment} coins",
                    FocusCost = 0,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    TokenType = offer.LetterType,
                    Category = offer.Category
                }).ToList();
                
                // Add decline option
                context.AvailableTemplates.Add(new ChoiceTemplate
                {
                    Purpose = "Decline letter offers",
                    Description = "Politely refuse any letter work",
                    FocusCost = 0,
                    ChoiceType = ConversationChoiceType.DeclineLetterOffer
                });
            }
        }
        
        // Create conversation properly with async/await
        var conversation = await _conversationFactory.CreateConversation(context, player);
        
        // Set up for MainGameplayView to handle
        _gameWorld.ConversationPending = true;
        _gameWorld.PendingConversationManager = conversation;
        
        return true;
    }
    
    /// <summary>
    /// Complete the action after conversation finishes
    /// </summary>
    public bool CompleteActionAfterConversation(ActionOption option, ConversationChoice selectedChoice = null)
    {
        var player = _gameWorld.GetPlayer();
        
        // Spend resources
        _timeManager.SpendHours(option.HourCost);
        player.ModifyStamina(-option.StaminaCost);
        player.ModifyCoins(-option.CoinCost);
        
        // Execute action - handle special conversation outcomes
        bool result = false;
        switch (option.Action)
        {
            case LocationAction.Rest:
                result = ExecuteRest(option);
                break;
                
            case LocationAction.Converse:
                result = ExecuteConverse(option.NPCId, selectedChoice);
                break;
                
            case LocationAction.Socialize:
                result = ExecuteSocialize(option.NPCId);
                break;
                
            case LocationAction.Work:
                result = ExecuteWork(option);
                break;
                
            case LocationAction.Collect:
                result = ExecuteCollect(option.NPCId, option.LetterId);
                break;
                
            case LocationAction.Deliver:
                result = ExecuteDeliver(option.NPCId, option.LetterId, selectedChoice);
                break;
                
            case LocationAction.Trade:
                result = ExecuteTrade(option.NPCId);
                break;
                
            case LocationAction.GatherResources:
                result = ExecuteGatherResources(option);
                break;
                
            case LocationAction.Browse:
                result = ExecuteBrowse(option);
                break;
                
            case LocationAction.Observe:
                result = ExecuteObserve(option);
                break;
                
            case LocationAction.RequestPatronFunds:
                result = ExecuteRequestPatronFunds(option);
                break;
                
            case LocationAction.RequestPatronEquipment:
                result = ExecuteRequestPatronEquipment(option);
                break;
                
            case LocationAction.BorrowMoney:
                result = ExecuteBorrowMoney(option);
                break;
                
            case LocationAction.PleedForAccess:
                result = ExecutePleedForAccess(option);
                break;
                
            case LocationAction.AcceptIllegalWork:
                result = ExecuteAcceptIllegalWork(option);
                break;
                
            case LocationAction.TravelEncounter:
                result = ExecuteTravelEncounter(option, selectedChoice);
                break;
                
            default:
                result = false;
                break;
        }
        
        // Report completed action to narrative manager
        if (result && _narrativeManager.HasActiveNarrative())
        {
            _narrativeManager.OnActionCompleted(option.Action, option.NPCId);
        }
        
        return result;
    }
    
    private bool ExecuteRest(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        player.ModifyStamina(GameRules.STAMINA_RECOVERY_REST);
        
        // Check if this is the special "buy drinks" rest with specific NPC
        if (option.CoinCost > 0 && !string.IsNullOrEmpty(option.NPCId))
        {
            var npc = _npcRepository.GetNPCById(option.NPCId);
            if (npc != null)
            {
                _messageSystem.AddSystemMessage(
                    $"🍺 You buy drinks and share stories with {npc.Name}.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    $"  • Stamina restored: {player.Stamina}/10",
                    SystemMessageTypes.Info
                );
                
                // Add token with the specific NPC
                _tokenManager.AddTokensToNPC(ConnectionType.Common, 1, option.NPCId);
                _messageSystem.AddSystemMessage(
                    $"  • Your bond with {npc.Name} strengthens over drinks",
                    SystemMessageTypes.Success
                );
            }
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "💤 You rest for an hour and recover some energy.",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Stamina restored: {player.Stamina}/10",
                SystemMessageTypes.Info
            );
        }
        
        return true;
    }
    
    private bool ExecuteConverse(string npcId, ConversationChoice selectedChoice = null)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null) return false;
        
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        
        // Handle conversation choice outcomes
        if (selectedChoice != null)
        {
            if (selectedChoice.ChoiceType == ConversationChoiceType.AcceptLetterOffer)
            {
                // Use the categorical properties to find the matching offer
                if (selectedChoice.OfferTokenType.HasValue && selectedChoice.OfferCategory.HasValue)
                {
                    var tokenType = selectedChoice.OfferTokenType.Value;
                    var category = selectedChoice.OfferCategory.Value;
                    
                    // Generate offers and find the one matching our category
                    var offers = _letterOfferService.GenerateNPCLetterOffers(npcId);
                    var matchingOffer = offers.FirstOrDefault(o => 
                        o.LetterType == tokenType && 
                        o.Category == category);
                    
                    if (matchingOffer != null)
                    {
                        var success = _letterOfferService.AcceptNPCLetterOffer(npcId, matchingOffer.Id);
                        
                        if (!success)
                        {
                            _messageSystem.AddSystemMessage(
                                $"❌ Your letter queue is full! Make room before accepting more letters.",
                                SystemMessageTypes.Danger
                            );
                        }
                        else
                        {
                            // Track first letter accepted for tutorial
                            if (!_flagService.GetFlag(FlagService.FIRST_LETTER_ACCEPTED))
                            {
                                _flagService.SetFlag(FlagService.FIRST_LETTER_ACCEPTED, true);
                                _flagService.SetFlag($"first_letter_accepted", true);
                            }
                        }
                    }
                    else
                    {
                        _messageSystem.AddSystemMessage(
                            $"❌ That letter offer is no longer available.",
                            SystemMessageTypes.Warning
                        );
                    }
                }
                return true;
            }
            else if (selectedChoice.ChoiceType == ConversationChoiceType.Introduction)
            {
                // Grant first token to establish connection
                _tokenManager.AddTokensToNPC(npc.LetterTokenTypes.FirstOrDefault(), 1, npcId);
                
                _messageSystem.AddSystemMessage(
                    $"👋 You introduce yourself to {npc.Name}.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    $"  • \"{npc.Description}\"",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    $"  • Gained +1 {npc.LetterTokenTypes.FirstOrDefault()} token with {npc.Name}",
                    SystemMessageTypes.Success
                );
                
                // Track first conversation for tutorial
                if (!_flagService.GetFlag(FlagService.FIRST_CONVERSATION))
                {
                    _flagService.SetFlag(FlagService.FIRST_CONVERSATION, true);
                    _flagService.SetFlag($"tutorial_first_conversation", true);
                }
                
                return true;
            }
            else if (selectedChoice.ChoiceType == ConversationChoiceType.DiscoverRoute)
            {
                // Show available routes from this NPC
                var routeDiscoveries = _routeDiscoveryManager.GetDiscoveriesFromNPC(npc);
                var discoverableRoutes = routeDiscoveries.Where(r => !r.Route.IsDiscovered && r.CanAfford).ToList();
                
                if (discoverableRoutes.Any())
                {
                    _messageSystem.AddSystemMessage(
                        $"🗺️ {npc.Name} shares their knowledge of hidden paths:",
                        SystemMessageTypes.Success
                    );
                    
                    // For now, discover the first available route
                    // In a full implementation, this would be another conversation choice
                    var routeToDiscover = discoverableRoutes.First();
                    var route = routeToDiscover.Route;
                    var discovery = routeToDiscover.Discovery;
                    
                    // Attempt to discover the route
                    if (_routeDiscoveryManager.TryDiscoverRoute(route.Id, npcId))
                    {
                        _messageSystem.AddSystemMessage(
                            $"  • Learned: {route.Name}",
                            SystemMessageTypes.Success
                        );
                        _messageSystem.AddSystemMessage(
                            $"  • From {route.Origin} to {route.Destination}",
                            SystemMessageTypes.Info
                        );
                        _messageSystem.AddSystemMessage(
                            $"  • Travel time: {route.TravelTimeHours} hours",
                            SystemMessageTypes.Info
                        );
                        
                        // Show token cost
                        var tokenCost = discovery.RequiredTokensWithNPC;
                        _messageSystem.AddSystemMessage(
                            $"  • Cost: {tokenCost} tokens with {npc.Name}",
                            SystemMessageTypes.Warning
                        );
                    }
                }
                else
                {
                    _messageSystem.AddSystemMessage(
                        $"💬 {npc.Name} doesn't know any routes you haven't already discovered.",
                        SystemMessageTypes.Info
                    );
                }
                return true;
            }
        }
        
        // Default conversation outcomes
        if (totalTokens == 0)
        {
            // First meeting - should have been handled by Introduction choice above
            _messageSystem.AddSystemMessage(
                $"💬 You have a brief chat with {npc.Name}.",
                SystemMessageTypes.Success
            );
        }
        else
        {
            // Ongoing relationship
            _messageSystem.AddSystemMessage(
                $"💬 You spend time talking with {npc.Name}.",
                SystemMessageTypes.Success
            );
            
            // May reveal letter opportunities at 1+ tokens
            if (totalTokens >= GameRules.TOKENS_BASIC_THRESHOLD && npc.LetterTokenTypes.Any())
            {
                _messageSystem.AddSystemMessage(
                    $"  • {npc.Name} seems to trust you with their correspondence",
                    SystemMessageTypes.Info
                );
            }
        }
        
        return true;
    }
    
    private bool ExecuteSocialize(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null) return false;
        
        var tokenType = npc.LetterTokenTypes.FirstOrDefault();
        _tokenManager.AddTokensToNPC(tokenType, 1, npcId);
        
        _messageSystem.AddSystemMessage(
            $"🤝 You spend quality time with {npc.Name}.",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"  • Your bond strengthens: +1 {tokenType} token",
            SystemMessageTypes.Success
        );
        
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        
        _messageSystem.AddSystemMessage(
            $"  • Total relationship: {totalTokens} tokens",
            SystemMessageTypes.Info
        );
        
        return true;
    }
    
    private bool ExecuteWork(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        // Handle environmental work actions (no specific NPC)
        if (string.IsNullOrEmpty(option.NPCId))
        {
            if (option.Name.Contains("day labor"))
            {
                // General labor at LABOR tagged locations
                player.ModifyCoins(3);
                _messageSystem.AddSystemMessage(
                    "💪 You find various odd jobs around the area.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    "  • Earned: 3 coins",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    $"  • Stamina: {player.Stamina}/10",
                    SystemMessageTypes.Info
                );
                return true;
            }
            else if (option.Name.Contains("Sharpen tools"))
            {
                // Tool maintenance at CRAFTING locations
                _messageSystem.AddSystemMessage(
                    "🔧 You spend time maintaining your equipment.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    "  • Your tools are in better condition",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • This may help with future crafting or labor",
                    SystemMessageTypes.Info
                );
                return true;
            }
        }
        
        // Original NPC-based work
        var npc = _npcRepository.GetNPCById(option.NPCId);
        
        if (npc == null) 
        {
            _messageSystem.AddSystemMessage(
                "❌ Cannot find the NPC to work for!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Different work types based on NPC profession and context
        string workDescription = "";
        int coinsEarned = 0;
        
        // Special case: Baker work at dawn
        if (npc.Name.ToLower().Contains("baker") && option.StaminaCost == 1)
        {
            // Baker work: 1 stamina → 2 coins + bread
            coinsEarned = 2;
            player.ModifyCoins(coinsEarned);
            
            workDescription = $"🥖 You help {npc.Name} with the morning baking.";
            
            // Add bread to inventory if space available
            var breadItem = _itemRepository.GetItemById("bread");
            if (breadItem != null && player.Inventory.CanAddItem(breadItem, _itemRepository))
            {
                player.Inventory.AddItem(breadItem.Id);
                _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage("  • Received: 1 bread (restores 2 stamina when eaten)", SystemMessageTypes.Success);
            }
            else
            {
                _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
                if (breadItem == null)
                {
                    _messageSystem.AddSystemMessage("  • (Bread item not found in game data)", SystemMessageTypes.Warning);
                }
                else
                {
                    _messageSystem.AddSystemMessage("  • No room for bread in inventory!", SystemMessageTypes.Warning);
                }
            }
        }
        else
        {
            // Standard work based on profession
            switch (npc.Profession)
            {
                case Professions.Merchant:
                    coinsEarned = 4;
                    workDescription = $"💪 You help {npc.Name} organize inventory and serve customers.";
                    break;
                    
                case Professions.TavernKeeper:
                    coinsEarned = 4;
                    workDescription = $"🍺 You serve drinks and clean tables for {npc.Name}.";
                    break;
                    
                case Professions.Scribe:
                    coinsEarned = 3;
                    workDescription = $"✍️ You carefully copy documents for {npc.Name}.";
                    break;
                    
                case Professions.Innkeeper:
                    coinsEarned = 4;
                    workDescription = $"🧹 You clean rooms and change linens for {npc.Name}.";
                    break;
                    
                default:
                    coinsEarned = 3;
                    workDescription = $"💼 You complete various tasks for {npc.Name}.";
                    break;
            }
            
            player.ModifyCoins(coinsEarned);
            
            _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
        }
        
        _messageSystem.AddSystemMessage($"  • Stamina: {player.Stamina}/10", SystemMessageTypes.Info);
        
        // Work builds a small connection with the NPC
        if (npc.LetterTokenTypes.Any())
        {
            var tokenType = npc.LetterTokenTypes.First();
            _tokenManager.AddTokensToNPC(tokenType, 1, option.NPCId);
            _messageSystem.AddSystemMessage(
                $"  • {npc.Name} appreciates your help (+1 {tokenType} token)",
                SystemMessageTypes.Success
            );
            
            // Track first token earned for tutorial
            if (!_flagService.GetFlag(FlagService.FIRST_TOKEN_EARNED))
            {
                _flagService.SetFlag(FlagService.FIRST_TOKEN_EARNED, true);
                _flagService.SetFlag($"first_token_earned", true);
            }
        }
        
        return true;
    }
    
    private bool ExecuteCollect(string npcId, string letterId)
    {
        var letter = _gameWorld.GetPlayer().LetterQueue
            .FirstOrDefault(l => l != null && l.Id == letterId);
            
        if (letter == null || letter.State != LetterState.Accepted)
        {
            _messageSystem.AddSystemMessage(
                "❌ Letter not found or already collected!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Create an inventory item for the letter
        var letterItem = letter.CreateInventoryItem();
        
        // Check inventory space
        var player = _gameWorld.GetPlayer();
        if (!player.Inventory.CanAddItem(letterItem, _itemRepository))
        {
            var availableSlots = player.Inventory.GetAvailableSlots(_itemRepository);
            _messageSystem.AddSystemMessage(
                $"❌ Not enough inventory space! Need {letterItem.GetRequiredSlots()} slots, have {availableSlots}",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                "  • Drop some items or equipment to make room",
                SystemMessageTypes.Info
            );
            return false;
        }
        
        // Add letter to inventory as an item
        if (!player.Inventory.AddItem(letterItem.Id))
        {
            _messageSystem.AddSystemMessage(
                "❌ Failed to add letter to inventory!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Update letter state and add to carried letters list for quick access
        letter.State = LetterState.Collected;
        player.CarriedLetters.Add(letter);
        
        _messageSystem.AddSystemMessage(
            $"📦 You collect the letter from {letter.SenderName}.",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"  • Letter now takes {letterItem.GetRequiredSlots()} inventory slots",
            SystemMessageTypes.Info
        );
        _messageSystem.AddSystemMessage(
            $"  • Ready for delivery when in position 1",
            SystemMessageTypes.Info
        );
        
        // Track first letter collected for tutorial
        if (!_flagService.GetFlag(FlagService.FIRST_LETTER_COLLECTED))
        {
            _flagService.SetFlag(FlagService.FIRST_LETTER_COLLECTED, true);
            _flagService.SetFlag($"first_letter_collected", true);
        }
        
        return true;
    }
    
    private bool ExecuteDeliver(string npcId, string letterId, ConversationChoice selectedChoice = null)
    {
        var player = _gameWorld.GetPlayer();
        var letter = player.LetterQueue[0];
        
        if (letter == null || letter.State != LetterState.Collected || letter.RecipientId != npcId)
        {
            _messageSystem.AddSystemMessage(
                "❌ Cannot deliver - check letter position and collection status!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Remove from carried letters and inventory
        player.CarriedLetters.Remove(letter);
        player.Inventory.RemoveItem($"letter_{letter.Id}");
        
        // Process delivery outcome based on conversation choice
        if (selectedChoice?.DeliveryOutcome != null)
        {
            var outcome = selectedChoice.DeliveryOutcome;
            var npc = _npcRepository.GetNPCById(npcId);
            
            // Calculate total payment
            int totalPayment = outcome.BasePayment + outcome.BonusPayment;
            
            // Apply tip chance if applicable
            if (outcome.ChanceForTip > 0)
            {
                var random = new Random();
                if (random.NextDouble() < outcome.ChanceForTip)
                {
                    totalPayment += outcome.TipAmount;
                    _messageSystem.AddSystemMessage(
                        $"💝 {npc.Name} appreciates your honesty and gives you a {outcome.TipAmount} coin tip!",
                        SystemMessageTypes.Success
                    );
                }
            }
            
            // Process delivery and payment
            _letterQueueManager.RemoveLetterFromQueue(1);
            _letterQueueManager.RecordLetterDelivery(letter);
            player.ModifyCoins(totalPayment);
            
            // Show delivery message
            _messageSystem.AddSystemMessage(
                $"✉️ Letter delivered to {letter.RecipientName}!",
                SystemMessageTypes.Success
            );
            
            // Show payment breakdown
            if (outcome.BonusPayment > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"💰 Earned {totalPayment} coins (base {outcome.BasePayment} + {outcome.BonusPayment} bonus)",
                    SystemMessageTypes.Success
                );
            }
            else if (outcome.BasePayment < letter.Payment)
            {
                _messageSystem.AddSystemMessage(
                    $"💰 Earned {totalPayment} coins ({letter.Payment - outcome.BasePayment} coin penalty for late delivery)",
                    SystemMessageTypes.Warning
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"💰 Earned {totalPayment} coins",
                    SystemMessageTypes.Success
                );
            }
            
            // Handle token rewards/penalties
            if (outcome.TokenReward && outcome.TokenAmount > 0)
            {
                _tokenManager.AddTokensToNPC(outcome.TokenType, outcome.TokenAmount, npcId);
                if (outcome.TokenAmount > 1)
                {
                    _messageSystem.AddSystemMessage(
                        $"🤝 +{outcome.TokenAmount} {outcome.TokenType} tokens with {npc.Name} (relationship strengthened)",
                        SystemMessageTypes.Success
                    );
                }
                else
                {
                    _messageSystem.AddSystemMessage(
                        $"🤝 +1 {outcome.TokenType} token with {npc.Name}",
                        SystemMessageTypes.Success
                    );
                }
            }
            else if (outcome.TokenPenalty && outcome.TokenAmount < 0)
            {
                var currentTokens = _tokenManager.GetTokenCount(outcome.TokenType);
                if (currentTokens >= Math.Abs(outcome.TokenAmount))
                {
                    _tokenManager.SpendTokens(outcome.TokenType, Math.Abs(outcome.TokenAmount));
                    _messageSystem.AddSystemMessage(
                        $"⚠️ Lost {Math.Abs(outcome.TokenAmount)} {outcome.TokenType} token with {npc.Name} (trust damaged)",
                        SystemMessageTypes.Warning
                    );
                }
            }
            else if (!outcome.TokenReward)
            {
                _messageSystem.AddSystemMessage(
                    "  • No token earned",
                    SystemMessageTypes.Info
                );
            }
            
            // Handle additional effects
            if (!string.IsNullOrEmpty(outcome.AdditionalEffect))
            {
                _messageSystem.AddSystemMessage(
                    $"  • {outcome.AdditionalEffect}",
                    SystemMessageTypes.Info
                );
            }
            
            // Handle return letter generation
            if (outcome.GeneratesReturnLetter)
            {
                var returnLetter = _letterOfferService.GenerateReturnLetter(npc, letter);
                if (returnLetter != null)
                {
                    // Try to add to the next available position
                    for (int pos = 1; pos <= 8; pos++)
                    {
                        if (_letterQueueManager.AddLetterToQueue(returnLetter, pos))
                        {
                            break;
                        }
                    }
                    _messageSystem.AddSystemMessage(
                        $"📨 {npc.Name} hands you a return letter to deliver",
                        SystemMessageTypes.Success
                    );
                }
            }
            
            // Handle chain letter unlocking
            if (outcome.UnlocksChainLetters && letter.IsChainLetter)
            {
                // Process chain letter unlocks
                if (letter.UnlocksLetterIds != null && letter.UnlocksLetterIds.Any())
                {
                    foreach (var unlockedId in letter.UnlocksLetterIds)
                    {
                        _noticeBoardService.UnlockChainLetter(unlockedId);
                    }
                    _messageSystem.AddSystemMessage(
                        $"🔗 Chain letter sequence unlocked! Check notice boards for new opportunities.",
                        SystemMessageTypes.Success
                    );
                }
            }
            
            // Handle leverage reduction
            if (outcome.ReducesLeverage > 0 && npc.Profession == Professions.Noble)
            {
                player.PatronLeverage = Math.Max(0, player.PatronLeverage - outcome.ReducesLeverage);
                _messageSystem.AddSystemMessage(
                    $"  • Your patron's leverage over you reduces ({player.PatronLeverage} remaining)",
                    SystemMessageTypes.Success
                );
            }
        }
        else
        {
            // Fallback to original simple delivery logic
            _letterQueueManager.RemoveLetterFromQueue(1);
            _letterQueueManager.RecordLetterDelivery(letter);
            player.ModifyCoins(letter.Payment);
            
            _messageSystem.AddSystemMessage(
                $"✉️ Letter delivered to {letter.RecipientName}!",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"💰 Earned {letter.Payment} coins",
                SystemMessageTypes.Success
            );
            
            // Default token reward
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc != null && npc.LetterTokenTypes.Any())
            {
                var tokenType = npc.LetterTokenTypes.First();
                _tokenManager.AddTokensToNPC(tokenType, 1, npcId);
                _messageSystem.AddSystemMessage(
                    $"🤝 +1 {tokenType} token with {npc.Name}",
                    SystemMessageTypes.Success
                );
            }
        }
        
        // Track first letter delivered for tutorial
        if (!_flagService.GetFlag(FlagService.FIRST_LETTER_DELIVERED))
        {
            _flagService.SetFlag(FlagService.FIRST_LETTER_DELIVERED, true);
            _flagService.SetFlag($"first_delivery_completed", true);
        }
        
        return true;
    }
    
    private bool ExecuteTrade(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null || npc.Profession != Professions.Merchant)
        {
            _messageSystem.AddSystemMessage(
                "❌ This person is not a merchant!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // For now, just acknowledge the trade action
        // The actual trading UI will be handled separately
        _messageSystem.AddSystemMessage(
            $"💰 You browse {npc.Name}'s wares...",
            SystemMessageTypes.Info
        );
        _messageSystem.AddSystemMessage(
            "Trading interface would open here",
            SystemMessageTypes.Info
        );
        
        // Trading happens through the existing market system
        // This is just the action that opens access to it
        // No special compound mechanics - just access to trade
        
        return true;
    }
    
    private bool IsNPCAvailable(NPC npc)
    {
        var currentPeriod = _timeManager.GetCurrentTimeBlock();
        
        // Simple availability for now - expand later
        return currentPeriod switch
        {
            TimeBlocks.Night => npc.LetterTokenTypes.Contains(ConnectionType.Shadow),
            TimeBlocks.Dawn => npc.LetterTokenTypes.Contains(ConnectionType.Common),
            _ => true // Most NPCs available during day
        };
    }
    
    private bool IsLetterRecipient(string npcId)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check if this NPC is the recipient of the letter in position 1
        var letterAtPosition1 = player.LetterQueue[0];
        if (letterAtPosition1 == null) return false;
        
        // Must be collected to deliver
        if (letterAtPosition1.State != LetterState.Collected) return false;
        
        // Check if this NPC is the recipient
        return letterAtPosition1.RecipientId == npcId;
    }
    
    private List<Letter> GetAcceptedLettersFromNPC(string npcId)
    {
        var player = _gameWorld.GetPlayer();
        return player.LetterQueue
            .Where(l => l != null && 
                       l.SenderId == npcId && 
                       l.State == LetterState.Accepted)
            .ToList();
    }
    
    
    /// <summary>
    /// Add environmental actions based on location spot domain tags
    /// </summary>
    private void AddEnvironmentalActions(List<ActionOption> actions, LocationSpot spot)
    {
        if (spot == null || !spot.DomainTags.Any()) return;
        
        var player = _gameWorld.GetPlayer();
        
        // Only add environmental actions if we have hours available
        if (_timeManager.HoursRemaining < 1) return;
        
        // RESOURCES tag - forest/nature locations
        if (spot.DomainTags.Contains("RESOURCES"))
        {
            // Gather berries - simple food gathering
            if (player.Stamina >= 1)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.GatherResources,
                    Name = "Gather wild berries",
                    Description = "Search the area for edible berries",
                    HourCost = 1,
                    StaminaCost = 1,
                    CoinCost = 0,
                    Effect = "+2 food items",
                    InitialNarrative = "You carefully search the area for edible berries."
                });
            }
            
            // Collect herbs - slightly more effort
            if (player.Stamina >= 2)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.GatherResources,
                    Name = "Collect medicinal herbs",
                    Description = "Look for valuable herbs in the undergrowth",
                    HourCost = 1,
                    StaminaCost = 2,
                    CoinCost = 0,
                    Effect = "+1-3 herbs (can sell for 3-5 coins each)",
                    InitialNarrative = "You kneel down and carefully examine the undergrowth for valuable herbs."
                });
            }
        }
        
        // COMMERCE tag - market/trade locations (when no merchant NPCs present)
        if (spot.DomainTags.Contains("COMMERCE"))
        {
            var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(
                spot.SpotID, 
                _timeManager.GetCurrentTimeBlock()
            );
            var hasMerchants = npcsHere.Any(npc => npc.Profession == Professions.Merchant);
            
            if (!hasMerchants)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Browse,
                    Name = "Browse empty market stalls",
                    Description = "Check unattended stalls for posted prices",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    Effect = "Learn current market prices",
                    InitialNarrative = "You examine the market stalls, noting prices and goods."
                });
            }
        }
        
        // SOCIAL tag - gathering places
        if (spot.DomainTags.Contains("SOCIAL"))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Observe,
                Name = "Listen to local gossip",
                Description = "Sit quietly and overhear conversations",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "Learn about recent events or opportunities",
                InitialNarrative = "You blend into the crowd, listening to local gossip."
            });
        }
        
        // LABOR tag - work opportunities (when no specific NPCs offering work)
        if (spot.DomainTags.Contains("LABOR") && player.Stamina >= 2)
        {
            var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(
                spot.SpotID, 
                _timeManager.GetCurrentTimeBlock()
            );
            var hasWorkOffering = npcsHere.Any(npc => 
                npc.Profession == Professions.Merchant || 
                npc.Profession == Professions.Innkeeper ||
                npc.Profession == Professions.TavernKeeper);
            
            if (!hasWorkOffering)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = "Find day labor",
                    Description = "Look for general work opportunities",
                    HourCost = 1,
                    StaminaCost = 2,
                    CoinCost = 0,
                    Effect = "+3 coins from odd jobs",
                    InitialNarrative = "You ask around for any work that needs doing."
                });
            }
        }
        
        // CRAFTING tag - tool maintenance or small crafts
        if (spot.DomainTags.Contains("CRAFTING") && player.Stamina >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Work,
                Name = "Sharpen tools",
                Description = "Maintain your equipment at the workshop",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 2,
                Effect = "Equipment in better condition",
                InitialNarrative = "You use the workshop facilities to maintain your gear."
            });
        }
        
        // TRANSPORT tag - travel arrangements
        if (spot.DomainTags.Contains("TRANSPORT"))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Browse,
                Name = "Check transport schedules",
                Description = "Look for boats, carts, or caravans",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "Learn about alternative travel options",
                InitialNarrative = "You examine the posted schedules and talk to transport operators."
            });
        }
        
        // Notice boards at town centers or market areas
        if ((spot.Type == LocationSpotTypes.FEATURE && spot.Name.ToLower().Contains("market")) || 
            (spot.Type == LocationSpotTypes.FEATURE && spot.Name.ToLower().Contains("square")))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Browse,
                Name = "Read the notice board",
                Description = "Check for public announcements and opportunities",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "Discover new letter opportunities or warnings",
                InitialNarrative = "You scan the weathered notice board for interesting postings."
            });
        }
    }
    
    private void AddPatronRequestActions(List<ActionOption> actions)
    {
        var player = _gameWorld.GetPlayer();
        
        // Can write to patron at any desk/room location
        if (player.CurrentLocationSpot?.SpotID.Contains("room") == true || 
            player.CurrentLocationSpot?.SpotID.Contains("desk") == true ||
            player.CurrentLocationSpot?.SpotID.Contains("study") == true)
        {
            if (_timeManager.HoursRemaining >= 1)
            {
                var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
                
                // Request funds action
                actions.Add(new ActionOption
                {
                    Action = LocationAction.RequestPatronFunds,
                    Name = "Write to patron requesting funds",
                    Description = $"Receive 30 coins, -1 Patron leverage (current: {patronTokens})",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    Effect = "30 coins, -1 Noble token with patron"
                });
                
                // Request equipment if low on stamina
                if (player.Stamina <= 4)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.RequestPatronEquipment,
                        Name = "Request equipment from patron",
                        Description = $"Receive climbing gear, -2 Patron leverage (current: {patronTokens})",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 0,
                        Effect = "Climbing gear, -2 Noble tokens with patron"
                    });
                }
            }
        }
    }
    
    
    private bool ExecuteRequestPatronFunds(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        _messageSystem.AddSystemMessage(
            "📜 You pen a carefully worded letter to your patron...",
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            "  • \"Circumstances require additional resources for your important work...\"",
            SystemMessageTypes.Info
        );
        
        // Reduce tokens with patron (can go negative)
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Noble, 1, "patron");
        
        // Grant funds
        player.ModifyCoins(30);
        
        _messageSystem.AddSystemMessage(
            "💰 Your patron's courier arrives with a small purse. No questions asked... this time.",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            "  • Received 30 coins",
            SystemMessageTypes.Success
        );
        
        var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
        if (patronTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ You now owe your patron {Math.Abs(patronTokens)} favors. Their letters will demand priority.",
                SystemMessageTypes.Warning
            );
        }
        
        return true;
    }
    
    private bool ExecuteRequestPatronEquipment(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        _messageSystem.AddSystemMessage(
            "📜 You write urgently to your patron about equipment needs...",
            SystemMessageTypes.Info
        );
        
        // Reduce tokens with patron
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Noble, 2, "patron");
        
        // Grant equipment (simplified - just add to inventory)
        // In full implementation, would use ItemRepository
        _messageSystem.AddSystemMessage(
            "📦 A package arrives from your patron. Quality climbing gear, no note.",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            "  • Received climbing gear",
            SystemMessageTypes.Success
        );
        
        var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
        if (patronTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ Your debt deepens. You owe your patron {Math.Abs(patronTokens)} favors.",
                SystemMessageTypes.Danger
            );
        }
        
        return true;
    }
    
    private bool ExecuteBorrowMoney(ActionOption option)
    {
        var npc = _npcRepository.GetNPCById(option.NPCId);
        if (npc == null) return false;
        
        _messageSystem.AddSystemMessage(
            $"💸 You swallow your pride and ask {npc.Name} for a loan...",
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            $"  • \"{npc.Name} sighs. 'I'll help, but this isn't charity.'\"",
            SystemMessageTypes.Warning
        );
        
        // Create debt
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Trade, 2, option.NPCId);
        
        // Grant money
        var player = _gameWorld.GetPlayer();
        player.ModifyCoins(20);
        
        _messageSystem.AddSystemMessage(
            $"💰 {npc.Name} counts out 20 coins. 'I expect repayment... with interest.'",
            SystemMessageTypes.Success
        );
        
        var tradeTokens = _tokenManager.GetTokensWithNPC(option.NPCId)[ConnectionType.Trade];
        if (tradeTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ You now owe {npc.Name}. Their letters will take priority in your queue.",
                SystemMessageTypes.Warning
            );
        }
        
        return true;
    }
    
    private bool ExecutePleedForAccess(ActionOption option)
    {
        // This would be implemented when route system is ready
        _messageSystem.AddSystemMessage(
            "Route access maintenance not yet implemented",
            SystemMessageTypes.Info
        );
        return true;
    }
    
    private bool ExecuteAcceptIllegalWork(ActionOption option)
    {
        var npc = _npcRepository.GetNPCById(option.NPCId);
        if (npc == null) return false;
        
        _messageSystem.AddSystemMessage(
            $"🌑 {npc.Name} leans in close. 'I have a job that pays well... if you don't ask questions.'",
            SystemMessageTypes.Warning
        );
        
        _messageSystem.AddSystemMessage(
            "  • You nod silently. Some things are better left unspoken.",
            SystemMessageTypes.Info
        );
        
        // Create leverage
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Shadow, 1, option.NPCId);
        
        // Grant payment
        var player = _gameWorld.GetPlayer();
        player.ModifyCoins(30);
        
        _messageSystem.AddSystemMessage(
            $"💰 {npc.Name} slides a heavy purse across the table. 'Remember, you never saw me.'",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"⚠️ {npc.Name} now has dirt on you. Refusing their future requests may have consequences.",
            SystemMessageTypes.Danger
        );
        
        return true;
    }
    
    private bool ExecuteGatherResources(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        var spot = player.CurrentLocationSpot;
        
        if (option.Name.Contains("berries"))
        {
            // Gather berries - simple and reliable
            _messageSystem.AddSystemMessage(
                "🫐 You spend an hour carefully picking ripe berries from the bushes.",
                SystemMessageTypes.Success
            );
            
            // Add berries to inventory (simplified for now)
            // In full implementation, would use ItemRepository to add actual food items
            _messageSystem.AddSystemMessage(
                "  • Found 2 portions of wild berries",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                "  • Each portion restores 1 stamina when eaten",
                SystemMessageTypes.Info
            );
        }
        else if (option.Name.Contains("herbs"))
        {
            // Collect herbs - variable results
            var herbCount = new Random().Next(1, 4); // 1-3 herbs
            
            _messageSystem.AddSystemMessage(
                "🌿 You search the undergrowth for medicinal herbs.",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                $"  • Found {herbCount} medicinal herb{(herbCount > 1 ? "s" : "")}",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                "  • Can be sold to merchants for 3-5 coins each",
                SystemMessageTypes.Info
            );
        }
        
        return true;
    }
    
    private bool ExecuteBrowse(ActionOption option)
    {
        if (option.Name.Contains("market stalls"))
        {
            _messageSystem.AddSystemMessage(
                "🏪 You browse the empty market stalls, checking posted prices.",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Bread: 2 coins",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Herbs: 3-5 coins (depending on quality)",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Basic supplies: 5-10 coins",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Note: \"Marcus returns at morning market hours\"",
                SystemMessageTypes.Info
            );
        }
        else if (option.Name.Contains("notice board"))
        {
            _messageSystem.AddSystemMessage(
                "📋 You read the notices posted on the board.",
                SystemMessageTypes.Info
            );
            
            var currentTime = _timeManager.GetCurrentTimeBlock();
            var player = _gameWorld.GetPlayer();
            var currentLocation = player.CurrentLocation;
            
            // Different notices based on time of day and location
            if (currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon)
            {
                _messageSystem.AddSystemMessage(
                    "  • \"Warning: Bandits spotted on eastern road at night\"",
                    SystemMessageTypes.Warning
                );
                
                // Check for NPCs seeking carriers
                var scribes = _npcRepository.GetNPCsByProfession(Professions.Scribe)
                    .Where(npc => npc.Location == currentLocation.Id)
                    .ToList();
                    
                if (scribes.Any())
                {
                    var scribe = scribes.First();
                    _messageSystem.AddSystemMessage(
                        $"  • \"{scribe.Name} seeks reliable carriers for correspondence\"",
                        SystemMessageTypes.Info
                    );
                }
                else
                {
                    _messageSystem.AddSystemMessage(
                        "  • \"Seeking experienced letter carriers - inquire at scribe's office\"",
                        SystemMessageTypes.Info
                    );
                }
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    "  • \"Night work available - ask at the docks\"",
                    SystemMessageTypes.Info
                );
                
                _messageSystem.AddSystemMessage(
                    "  • \"Lost: Merchant's ledger. Reward offered.\"",
                    SystemMessageTypes.Info
                );
            }
            
            // Check for dawn time when letter board is active
            if (currentTime == TimeBlocks.Dawn)
            {
                _messageSystem.AddSystemMessage(
                    "  • \"The letter board is active now - check for new opportunities!\"",
                    SystemMessageTypes.Success
                );
            }
        }
        else if (option.Name.Contains("transport schedules"))
        {
            _messageSystem.AddSystemMessage(
                "🚢 You check the transport schedules and talk to operators.",
                SystemMessageTypes.Info
            );
            
            var player = _gameWorld.GetPlayer();
            var currentLocation = player.CurrentLocation;
            
            // Show transport-specific information based on location
            if (currentLocation.Id.ToLower().Contains("bridge") || 
                currentLocation.Id.ToLower().Contains("dock"))
            {
                _messageSystem.AddSystemMessage(
                    "  • River boats depart at dawn and noon",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Boat passage: 5 coins, saves 2 hours to downstream locations",
                    SystemMessageTypes.Info
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    "  • Merchant caravans form weekly for distant cities",
                    SystemMessageTypes.Info
                );
                _messageSystem.AddSystemMessage(
                    "  • Cart rental: 10 coins/day, reduces stamina cost on roads",
                    SystemMessageTypes.Info
                );
            }
        }
        
        return true;
    }
    
    private bool ExecuteObserve(ActionOption option)
    {
        _messageSystem.AddSystemMessage(
            "👂 You find a quiet corner and listen to the local gossip.",
            SystemMessageTypes.Info
        );
        
        // Random gossip based on game state
        var gossipOptions = new List<string>
        {
            "  • \"Did you hear? The noble's courier never arrived...\"",
            "  • \"Marcus has been looking for reliable carriers lately\"",
            "  • \"The mountain pass is treacherous without proper gear\"",
            "  • \"I heard Elena pays well for discrete deliveries\"",
            "  • \"The river route saves time, but ruins letters in the rain\""
        };
        
        var random = new Random();
        var selectedGossip = gossipOptions[random.Next(gossipOptions.Count)];
        
        _messageSystem.AddSystemMessage(
            selectedGossip,
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            "  • You make a mental note of this information",
            SystemMessageTypes.Info
        );
        
        return true;
    }
    
    private bool ExecuteTravelEncounter(ActionOption option, ConversationChoice selectedChoice)
    {
        var player = _gameWorld.GetPlayer();
        
        // Travel encounters are always resolved through conversation choices
        if (selectedChoice == null)
        {
            _messageSystem.AddSystemMessage(
                "❌ Travel encounter requires a choice!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // For travel encounters, we need the route info from the action option
        // The route should be stored in the action context
        if (option.EncounterType == null)
        {
            _messageSystem.AddSystemMessage(
                "❌ Travel encounter missing encounter type!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Apply effects based on choice type and properties
        switch (selectedChoice.ChoiceType)
        {
            case ConversationChoiceType.TravelCautious:
                // Default safe option - no additional effects
                _messageSystem.AddSystemMessage(
                    "🚶 You proceed carefully, taking your time to navigate safely.",
                    SystemMessageTypes.Info
                );
                break;
                
            case ConversationChoiceType.TravelUseEquipment:
                // Use equipment for benefit
                HandleEquipmentChoice(selectedChoice, option);
                break;
                
            case ConversationChoiceType.TravelForceThrough:
                // Spend stamina
                if (selectedChoice.StaminaCost > 0)
                {
                    player.SpendStamina(selectedChoice.StaminaCost.Value);
                    _messageSystem.AddSystemMessage(
                        "💪 You push through with determination, exhausting yourself.",
                        SystemMessageTypes.Warning
                    );
                    
                    if (selectedChoice.TimeModifierMinutes < 0)
                    {
                        _messageSystem.AddSystemMessage(
                            $"  • -{selectedChoice.StaminaCost} stamina, saved {Math.Abs(selectedChoice.TimeModifierMinutes.Value)} minutes",
                            SystemMessageTypes.Warning
                        );
                    }
                    else
                    {
                        _messageSystem.AddSystemMessage(
                            $"  • -{selectedChoice.StaminaCost} stamina",
                            SystemMessageTypes.Warning
                        );
                    }
                }
                break;
                
            case ConversationChoiceType.TravelSlowProgress:
                // Accept time penalty
                if (selectedChoice.TimeModifierMinutes > 0)
                {
                    _messageSystem.AddSystemMessage(
                        "🌑 You make slow but steady progress.",
                        SystemMessageTypes.Warning
                    );
                    _messageSystem.AddSystemMessage(
                        $"  • +{selectedChoice.TimeModifierMinutes / 60} hour to travel time",
                        SystemMessageTypes.Warning
                    );
                }
                break;
                
            case ConversationChoiceType.TravelTradeHelp:
                // Help for benefit
                if (selectedChoice.CoinReward > 0)
                {
                    player.ModifyCoins(selectedChoice.CoinReward.Value);
                    if (selectedChoice.StaminaCost > 0)
                    {
                        player.SpendStamina(selectedChoice.StaminaCost.Value);
                    }
                    _messageSystem.AddSystemMessage(
                        "💪 You help the merchant with their troubles.",
                        SystemMessageTypes.Success
                    );
                    _messageSystem.AddSystemMessage(
                        $"  • Earned {selectedChoice.CoinReward} coins" + 
                        (selectedChoice.StaminaCost > 0 ? $", -{selectedChoice.StaminaCost} stamina" : ""),
                        SystemMessageTypes.Success
                    );
                }
                else if (selectedChoice.TimeModifierMinutes < 0)
                {
                    _messageSystem.AddSystemMessage(
                        "🎠 Your assistance is rewarded with faster travel.",
                        SystemMessageTypes.Success
                    );
                    _messageSystem.AddSystemMessage(
                        $"  • Saved {Math.Abs(selectedChoice.TimeModifierMinutes.Value)} minutes of travel time",
                        SystemMessageTypes.Success
                    );
                }
                break;
                
            case ConversationChoiceType.TravelExchangeInfo:
                // Learn information
                _messageSystem.AddSystemMessage(
                    "💬 You exchange valuable information about the roads.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    "  • You feel more prepared for future journeys",
                    SystemMessageTypes.Info
                );
                break;
        }
        
        // Travel completion will be handled by GameWorldManager after action completes
        // The UI will call CompleteTravelAfterEncounter when appropriate
        
        return true;
    }
    
    private void HandleEquipmentChoice(ConversationChoice choice, ActionOption option)
    {
        var encounterType = option.EncounterType ?? TravelEncounterType.FellowTraveler;
        
        switch (choice.RequiredEquipment)
        {
            case EquipmentType.ClimbingGear:
                _messageSystem.AddSystemMessage(
                    "🧗 Your climbing gear allows you to quickly scale the obstacle.",
                    SystemMessageTypes.Success
                );
                if (choice.TimeModifierMinutes < 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"  • Saved {Math.Abs(choice.TimeModifierMinutes.Value)} minutes of travel time",
                        SystemMessageTypes.Success
                    );
                }
                break;
                
            case EquipmentType.LightSource:
                _messageSystem.AddSystemMessage(
                    "🔦 Your torch illuminates the path, allowing safe passage.",
                    SystemMessageTypes.Success
                );
                break;
                
            case EquipmentType.WeatherProtection:
                _messageSystem.AddSystemMessage(
                    "☔ Your weather gear keeps you dry and comfortable.",
                    SystemMessageTypes.Success
                );
                break;
                
            case EquipmentType.LoadDistribution:
                _messageSystem.AddSystemMessage(
                    "🎠 Your cart proves useful in unexpected ways.",
                    SystemMessageTypes.Success
                );
                if (choice.TimeModifierMinutes < 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"  • Saved {Math.Abs(choice.TimeModifierMinutes.Value)} minutes of travel time",
                        SystemMessageTypes.Success
                    );
                }
                break;
        }
    }
    
    /// <summary>
    /// Check if player carries trade goods with profit potential at this location
    /// This creates natural compound actions where letter delivery and trading overlap
    /// </summary>
    private string GetTradeCompoundEffect(NPC merchant)
    {
        var player = _gameWorld.GetPlayer();
        var currentLocation = player.CurrentLocation;
        var profitableItems = new List<(string itemName, int profit)>();
        
        // Check inventory for trade goods
        foreach (var itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            var item = _itemRepository.GetItemById(itemId);
            if (item == null) continue;
            
            // Check if this is a trade good with profit potential here
            if (item.Categories.Contains(ItemCategory.Trade_Goods))
            {
                // Different locations value different goods
                var localValue = GetLocalItemValue(item, currentLocation);
                var profit = localValue - item.BuyPrice;
                
                if (profit > 0)
                {
                    profitableItems.Add((item.Name, profit));
                }
            }
        }
        
        if (profitableItems.Any())
        {
            var totalProfit = profitableItems.Sum(i => i.profit);
            return $"Access market + sell {profitableItems.Count} items for +{totalProfit} profit";
        }
        
        return "Access market prices";
    }
    
    /// <summary>
    /// Determine local value of an item based on location economics
    /// Items have higher value where they're not produced
    /// </summary>
    private int GetLocalItemValue(Item item, Location location)
    {
        // Base sell price
        var baseValue = item.SellPrice;
        
        // Items sell for more where they're not produced
        if (item.LocationId != null && item.LocationId != location.Id)
        {
            // Foreign goods command premium prices
            return (int)(baseValue * 1.5);
        }
        
        // Local goods sell at standard price
        return baseValue;
    }
}

/// <summary>
/// Represents an action option with its resource costs
/// </summary>
public class ActionOption
{
    public LocationAction Action { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int HourCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinCost { get; set; }
    public int FocusCost { get; set; } // For future mental stamina system
    public string NPCId { get; set; }
    public string LetterId { get; set; }
    public string Effect { get; set; }
    
    // Narrative for thin layer - all actions use conversation
    public string InitialNarrative { get; set; }
    
    // Travel encounter specific properties
    public TravelEncounterType? EncounterType { get; set; }
    
    // Letter offer specific flag
    public bool IsLetterOffer { get; set; }
}

/// <summary>
/// Types of travel encounters based on route terrain
/// </summary>
public enum TravelEncounterType
{
    WildernessObstacle,  // Fallen trees, blocked paths
    DarkChallenge,       // Darkness requiring navigation
    WeatherEvent,        // Rain, storms, weather challenges
    MerchantEncounter,   // Stranded merchants needing help
    FellowTraveler       // Other travelers on the road
}