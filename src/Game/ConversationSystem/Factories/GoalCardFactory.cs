using System;

/// <summary>
/// Factory for creating goal cards that are shuffled into conversation decks.
/// Goal cards define the purpose of a conversation and end it when played.
/// All cards are loaded from GameWorld.AllCardDefinitions
/// </summary>
public static class GoalCardFactory
{
    private static GameWorld _gameWorld;
    
    /// <summary>
    /// Initialize the factory with GameWorld reference
    /// GameWorld is the single source of truth for all card data
    /// </summary>
    public static void Initialize(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }
    
    /// <summary>
    /// Create a goal card based on the conversation type
    /// </summary>
    public static ConversationCard CreateGoalCard(ConversationType conversationType, string npcId, string npcName)
    {
        if (_gameWorld == null)
        {
            Console.WriteLine("WARNING: GoalCardFactory not initialized with GameWorld");
            return null;
        }
        
        // Look for a goal card matching the conversation type
        foreach (var kvp in _gameWorld.AllCardDefinitions)
        {
            var card = kvp.Value;
            if (card.IsGoalCard == true && 
                card.GoalCardType == conversationType)
            {
                return CreateGoalCardInstance(card, npcId, npcName);
            }
        }
        
        Console.WriteLine($"WARNING: No goal card found for conversation type {conversationType}");
        return null;
    }
    
    private static ConversationCard CreateGoalCardInstance(ConversationCard templateCard, string npcId, string npcName)
    {
        // Create a new instance based on the template card but with updated context
        return new ConversationCard
        {
            Id = $"{templateCard.Id}_{npcId}_{Guid.NewGuid().ToString()[..8]}", // Unique ID for this instance
            TemplateId = templateCard.TemplateId,
            Mechanics = templateCard.Mechanics,
            Context = new CardContext
            {
                Personality = templateCard.Context?.Personality ?? PersonalityType.STEADFAST, // Use template default
                EmotionalState = templateCard.Context?.EmotionalState ?? EmotionalState.NEUTRAL, // Use template default
                NPCName = npcName
            },
            Type = templateCard.Type,
            Category = templateCard.Category,
            Persistence = templateCard.Persistence,
            Weight = templateCard.Weight,
            BaseComfort = templateCard.BaseComfort,
            IsGoalCard = true,
            GoalCardType = templateCard.GoalCardType,
            DisplayName = templateCard.DisplayName,
            Description = templateCard.Description,
            SuccessRate = templateCard.SuccessRate
        };
    }
}