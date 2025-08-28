using System;

/// <summary>
/// Factory for creating goal cards that are shuffled into conversation decks.
/// Goal cards define the purpose of a conversation and end it when played.
/// All cards are loaded from conversations.json
/// </summary>
public static class GoalCardFactory
{
    private static ConversationCardParser _cardParser;
    
    /// <summary>
    /// Initialize the factory with a card parser
    /// </summary>
    public static void Initialize(ConversationCardParser cardParser)
    {
        _cardParser = cardParser;
    }
    
    /// <summary>
    /// Create a goal card based on the conversation type
    /// </summary>
    public static ConversationCard CreateGoalCard(ConversationType conversationType, string npcId, string npcName)
    {
        if (_cardParser == null)
        {
            Console.WriteLine("WARNING: GoalCardFactory not initialized with ConversationCardParser");
            return null;
        }
        
        return _cardParser.GetGoalCard(conversationType, npcId, npcName);
    }
}