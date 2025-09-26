using System.Collections.Generic;

/// <summary>
/// Represents a one-time request from an NPC with multiple cards at different rapport thresholds
/// </summary>
public class NPCRequest
{
    /// <summary>
    /// Unique identifier for the request
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the request
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of what the NPC is asking
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The text displayed when the NPC presents this request (shown on LISTEN action)
    /// </summary>
    public string NpcRequestText { get; set; }

    /// <summary>
    /// The conversation type ID this request uses (e.g., "desperate_request", "trade_negotiation")
    /// </summary>
    public string ConversationTypeId { get; set; }

    /// <summary>
    /// Category that must match the conversation type's category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this request
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the request
    /// </summary>
    public RequestStatus Status { get; set; } = RequestStatus.Available;

    /// <summary>
    /// Request card IDs with different rapport thresholds offering varying rewards
    /// Each card represents a different level of commitment to the request
    /// Cards are stored by ID only - actual cards are in GameWorld.AllCardDefinitions
    /// </summary>
    public List<string> RequestCardIds { get; set; } = new List<string>();

    /// <summary>
    /// Promise card IDs that force queue position 1 and burn tokens for instant rapport
    /// These represent immediate action at the cost of other relationships
    /// Cards are stored by ID only - actual cards are in GameWorld.AllCardDefinitions
    /// </summary>
    public List<string> PromiseCardIds { get; set; } = new List<string>();

    /// <summary>
    /// Momentum thresholds for tiered rewards (used by stranger conversations)
    /// </summary>
    public List<int> MomentumThresholds { get; set; } = new List<int>();

    /// <summary>
    /// Rewards for reaching momentum thresholds (used by stranger conversations)
    /// </summary>
    public List<RequestReward> Rewards { get; set; } = new List<RequestReward>();

    /// <summary>
    /// Tiered goals with different momentum thresholds and weights (new system)
    /// </summary>
    public List<NPCRequestGoal> Goals { get; set; } = new List<NPCRequestGoal>();

    /// <summary>
    /// Check if this request is available to attempt
    /// </summary>
    public bool IsAvailable()
    {
        return Status == RequestStatus.Available;
    }

    /// <summary>
    /// Mark this request as completed
    /// </summary>
    public void Complete()
    {
        Status = RequestStatus.Completed;
    }

    /// <summary>
    /// Get all cards associated with this request (both request and promise cards)
    /// Resolves card IDs to actual cards from GameWorld
    /// </summary>
    public List<ConversationCard> GetAllCards(GameWorld gameWorld)
    {
        List<ConversationCard> allCards = new List<ConversationCard>();

        // Resolve request card IDs
        foreach (string cardId in RequestCardIds)
        {
            CardDefinitionEntry? entry = gameWorld.AllCardDefinitions.FindById(cardId);
            if (entry != null)
                allCards.Add(entry.Card);
        }

        // Resolve promise card IDs
        foreach (string cardId in PromiseCardIds)
        {
            CardDefinitionEntry? entry = gameWorld.AllCardDefinitions.FindById(cardId);
            if (entry != null)
                allCards.Add(entry.Card);
        }

        return allCards;
    }

    /// <summary>
    /// Get request cards by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetRequestCards(GameWorld gameWorld)
    {
        List<ConversationCard> cards = new List<ConversationCard>();
        foreach (string cardId in RequestCardIds)
        {
            CardDefinitionEntry? entry = gameWorld.AllCardDefinitions.FindById(cardId);
            if (entry != null)
                cards.Add(entry.Card);
        }
        return cards;
    }

    /// <summary>
    /// Get promise cards by resolving IDs from GameWorld
    /// </summary>
    public List<ConversationCard> GetPromiseCards(GameWorld gameWorld)
    {
        List<ConversationCard> cards = new List<ConversationCard>();
        foreach (string cardId in PromiseCardIds)
        {
            CardDefinitionEntry? entry = gameWorld.AllCardDefinitions.FindById(cardId);
            if (entry != null)
                cards.Add(entry.Card);
        }
        return cards;
    }
}

/// <summary>
/// Status of an NPC request
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// Request is available to attempt
    /// </summary>
    Available,

    /// <summary>
    /// Request has been completed (any card was successfully played)
    /// </summary>
    Completed,

    /// <summary>
    /// Request was failed and is no longer available
    /// </summary>
    Failed
}

/// <summary>
/// Reward for reaching a rapport threshold in stranger conversations
/// </summary>
public class RequestReward
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Food { get; set; }
    public int Familiarity { get; set; }
    public string Item { get; set; }
    public string Permit { get; set; }
    public string Observation { get; set; }
    public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Tiered request goal with weight-based rewards
/// </summary>
public class NPCRequestGoal
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MomentumThreshold { get; set; }
    public int Weight { get; set; }
    public NPCRequestRewards Rewards { get; set; } = new NPCRequestRewards();
}

/// <summary>
/// Reward structure for tiered request goals
/// </summary>
public class NPCRequestRewards
{
    public int? Coins { get; set; }
    public string LetterId { get; set; }
    public string Obligation { get; set; }
    public string Item { get; set; }
    public Dictionary<string, int> Tokens { get; set; } = new Dictionary<string, int>();
}