using System.Collections.Generic;
using System.Threading.Tasks;

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
    /// Get available travel destinations
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
}