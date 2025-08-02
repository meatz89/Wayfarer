using System.Collections.Generic;
using System.Threading.Tasks;
using Wayfarer.ViewModels;

/// <summary>
/// Single interface for ALL UI-Backend communication.
/// This is THE contract between the UI layer and the game backend.
/// All UI components and test controllers MUST use this interface exclusively.
/// </summary>
public interface IGameFacade
{
    // ========== GAME STATE QUERIES ==========
    
    /// <summary>
    /// Get the current game world snapshot for UI display
    /// </summary>
    GameWorldSnapshot GetGameSnapshot();
    
    /// <summary>
    /// Get current player state
    /// </summary>
    Player GetPlayer();
    
    /// <summary>
    /// Get current location and spot
    /// </summary>
    (Location location, LocationSpot spot) GetCurrentLocation();
    
    /// <summary>
    /// Get current time information
    /// </summary>
    (TimeBlocks timeBlock, int hoursRemaining, int currentDay) GetTimeInfo();
    
    // ========== LOCATION ACTIONS ==========
    
    /// <summary>
    /// Get available actions at current location
    /// </summary>
    LocationActionsViewModel GetLocationActions();
    
    /// <summary>
    /// Execute a location action by command ID
    /// </summary>
    Task<bool> ExecuteLocationActionAsync(string commandId);
    
    // ========== TRAVEL ==========
    
    /// <summary>
    /// Get travel context information (weight, stamina, equipment, etc.)
    /// </summary>
    TravelContextViewModel GetTravelContext();
    
    /// <summary>
    /// Get available travel destinations with all routes
    /// </summary>
    List<TravelDestinationViewModel> GetTravelDestinationsWithRoutes();
    
    /// <summary>
    /// Get available travel destinations (legacy)
    /// </summary>
    List<TravelDestinationViewModel> GetTravelDestinations();
    
    /// <summary>
    /// Get routes to a specific destination
    /// </summary>
    List<TravelRouteViewModel> GetRoutesToDestination(string destinationId);
    
    /// <summary>
    /// Execute travel to a destination
    /// </summary>
    Task<bool> TravelToDestinationAsync(string destinationId, string routeId);
    
    /// <summary>
    /// Calculate total weight including inventory and letters
    /// </summary>
    int CalculateTotalWeight();
    
    // ========== REST ==========
    
    /// <summary>
    /// Get available rest options
    /// </summary>
    RestOptionsViewModel GetRestOptions();
    
    /// <summary>
    /// Execute a rest action
    /// </summary>
    Task<bool> ExecuteRestAsync(string restOptionId);
    
    // ========== CONVERSATIONS ==========
    
    /// <summary>
    /// Start a conversation with an NPC
    /// </summary>
    Task<ConversationViewModel> StartConversationAsync(string npcId);
    
    /// <summary>
    /// Continue a conversation with a choice
    /// </summary>
    Task<ConversationViewModel> ContinueConversationAsync(string choiceId);
    
    /// <summary>
    /// Get current conversation state if any
    /// </summary>
    ConversationViewModel GetCurrentConversation();
    
    // ========== LETTER QUEUE ==========
    
    /// <summary>
    /// Get current letter queue state
    /// </summary>
    LetterQueueViewModel GetLetterQueue();
    
    /// <summary>
    /// Execute a letter queue action
    /// </summary>
    Task<bool> ExecuteLetterActionAsync(string actionType, string letterId);
    
    /// <summary>
    /// Deliver a letter that's in position 1
    /// </summary>
    Task<bool> DeliverLetterAsync(string letterId);
    
    /// <summary>
    /// Skip a letter to position 1 (triggers conversation)
    /// </summary>
    Task<bool> SkipLetterAsync(int position);
    
    /// <summary>
    /// Morning swap two letter positions (Dawn only)
    /// </summary>
    Task<bool> LetterQueueMorningSwapAsync(int position1, int position2);
    
    /// <summary>
    /// Priority move a letter to position 1
    /// </summary>
    Task<bool> LetterQueuePriorityMoveAsync(int fromPosition);
    
    /// <summary>
    /// Extend deadline of a letter
    /// </summary>
    Task<bool> LetterQueueExtendDeadlineAsync(int position);
    
    /// <summary>
    /// Purge bottom letter using tokens
    /// </summary>
    Task<bool> LetterQueuePurgeAsync(Dictionary<string, int> tokenSelection);
    
    /// <summary>
    /// Get letter board offers (Dawn only)
    /// </summary>
    LetterBoardViewModel GetLetterBoard();
    
    /// <summary>
    /// Accept a letter from the board
    /// </summary>
    Task<bool> AcceptLetterOfferAsync(string offerId);
    
    // ========== MARKET ==========
    
    /// <summary>
    /// Get market state at current location
    /// </summary>
    MarketViewModel GetMarket();
    
    /// <summary>
    /// Buy an item from the market
    /// </summary>
    Task<bool> BuyItemAsync(string itemId, string traderId);
    
    /// <summary>
    /// Sell an item to the market
    /// </summary>
    Task<bool> SellItemAsync(string itemId, string traderId);
    
    // ========== DEBT MANAGEMENT ==========
    
    /// <summary>
    /// Get all active debts and available lenders
    /// </summary>
    DebtViewModel GetDebtInformation();
    
    /// <summary>
    /// Borrow money from an NPC
    /// </summary>
    Task<bool> BorrowMoneyAsync(string npcId);
    
    /// <summary>
    /// Repay debt to an NPC
    /// </summary>
    Task<bool> RepayDebtAsync(string npcId, int amount);
    
    // ========== PERSONAL ERRANDS ==========
    
    /// <summary>
    /// Get available personal errands
    /// </summary>
    PersonalErrandViewModel GetPersonalErrands();
    
    /// <summary>
    /// Execute a personal errand for an NPC
    /// </summary>
    Task<bool> ExecutePersonalErrandAsync(string npcId);
    
    // ========== INVENTORY ==========
    
    /// <summary>
    /// Get player inventory
    /// </summary>
    InventoryViewModel GetInventory();
    
    /// <summary>
    /// Use an item (like reading a special letter)
    /// </summary>
    Task<bool> UseItemAsync(string itemId);
    
    // ========== NARRATIVE/TUTORIAL ==========
    
    /// <summary>
    /// Get current narrative/tutorial state
    /// </summary>
    NarrativeStateViewModel GetNarrativeState();
    
    /// <summary>
    /// Check if tutorial is active
    /// </summary>
    bool IsTutorialActive();
    
    /// <summary>
    /// Get tutorial guidance for current step
    /// </summary>
    TutorialGuidanceViewModel GetTutorialGuidance();
    
    // ========== GAME FLOW ==========
    
    /// <summary>
    /// Start a new game
    /// </summary>
    Task StartGameAsync();
    
    /// <summary>
    /// Advance to next day
    /// </summary>
    Task<MorningActivityResult> AdvanceToNextDayAsync();
    
    /// <summary>
    /// Get morning activities if any
    /// </summary>
    MorningActivityResult GetMorningActivities();
    
    // ========== SYSTEM MESSAGES ==========
    
    /// <summary>
    /// Get recent system messages
    /// </summary>
    List<SystemMessage> GetSystemMessages();
    
    /// <summary>
    /// Clear system messages
    /// </summary>
    void ClearSystemMessages();
    
    // ========== NPC & RELATIONSHIPS ==========
    
    /// <summary>
    /// Get time block service plan for current location
    /// Shows what services/NPCs are available at different times
    /// </summary>
    List<TimeBlockServiceViewModel> GetTimeBlockServicePlan();
    
    /// <summary>
    /// Get NPCs at current location and time with offer information
    /// </summary>
    List<NPCWithOffersViewModel> GetNPCsWithOffers();
    
    /// <summary>
    /// Get NPC relationships for UI display
    /// </summary>
    List<NPCRelationshipViewModel> GetNPCRelationships();
    
    /// <summary>
    /// Get standing obligations affecting the letter queue
    /// </summary>
    List<ObligationViewModel> GetStandingObligations();
    
    // ========== SEAL MANAGEMENT ==========
    
    /// <summary>
    /// Get seal progression information
    /// </summary>
    SealProgressionViewModel GetSealProgression();
    
    /// <summary>
    /// Equip a seal
    /// </summary>
    Task<bool> EquipSealAsync(string sealId);
    
    /// <summary>
    /// Unequip a seal
    /// </summary>
    Task<bool> UnequipSealAsync(string sealId);
}