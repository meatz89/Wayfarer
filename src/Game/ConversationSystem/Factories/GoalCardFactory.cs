using System;

/// <summary>
/// Factory for creating goal cards that are shuffled into conversation decks.
/// Goal cards define the purpose of a conversation and end it when played.
/// </summary>
public static class GoalCardFactory
{
    /// <summary>
    /// Create a goal card based on the conversation type
    /// </summary>
    public static ConversationCard CreateGoalCard(ConversationType conversationType, string npcId, string npcName)
    {
        // Map conversation type to goal type
        var goalType = GetGoalTypeForConversation(conversationType);
        if (!goalType.HasValue)
            return null; // Some conversation types don't have goals
        
        return goalType.Value switch
        {
            GoalType.Letter => CreateLetterGoalCard(npcId, npcName),
            GoalType.Promise => CreatePromiseGoalCard(npcId, npcName),
            GoalType.Resolution => CreateResolutionGoalCard(npcId, npcName),
            GoalType.Commerce => CreateCommerceGoalCard(npcId, npcName),
            GoalType.Crisis => CreateCrisisGoalCard(npcId, npcName),
            _ => null
        };
    }
    
    /// <summary>
    /// Determine which goal type is used for a conversation type
    /// </summary>
    private static GoalType? GetGoalTypeForConversation(ConversationType type)
    {
        // Based on conversation-system.md lines 107-112
        return type switch
        {
            ConversationType.Standard => GoalType.Letter,  // Elena's main use case
            ConversationType.Deep => GoalType.Promise,     // Deep conversations create obligations
            ConversationType.Crisis => GoalType.Crisis,    // Crisis needs resolution
            ConversationType.QuickExchange => null,        // Exchanges don't have goals
            _ => null
        };
    }
    
    /// <summary>
    /// Create a letter goal card for accepting delivery obligations
    /// </summary>
    private static ConversationCard CreateLetterGoalCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = $"goal_letter_{npcId}",
            Template = CardTemplateType.Negotiate,
            Context = new CardContext
            {
                NPCName = npcName,
                CustomText = "Accept Letter Delivery",
                LetterDetails = "Negotiate terms for delivering a letter"
            },
            Type = CardType.Trust,  // Letters build trust
            Persistence = PersistenceType.Persistent,  // Stays in hand once drawn
            Weight = 1,  // Has a cost to play
            BaseComfort = 5,  // Significant comfort gain for accepting obligation
            Category = CardCategory.GOAL,
            IsGoalCard = true,
            GoalCardType = GoalType.Letter,
            DisplayName = "Accept Letter Delivery",
            Description = "Accept an obligation to deliver a letter. Success determines deadline and payment.",
            SuccessRate = 50  // Base 50% success, modified by tokens
        };
    }
    
    /// <summary>
    /// Create a promise goal card for accepting meeting/escort obligations
    /// </summary>
    private static ConversationCard CreatePromiseGoalCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = $"goal_promise_{npcId}",
            Template = CardTemplateType.Promise,
            Context = new CardContext
            {
                NPCName = npcName,
                CustomText = "Make a Promise",
                LetterDetails = "Commit to a future meeting or task"
            },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 2,  // More significant commitment
            BaseComfort = 7,
            Category = CardCategory.GOAL,
            IsGoalCard = true,
            GoalCardType = GoalType.Promise,
            DisplayName = "Make a Promise",
            Description = "Commit to meeting someone or completing a task. Success determines the terms.",
            SuccessRate = 45  // Slightly harder than letters
        };
    }
    
    /// <summary>
    /// Create a resolution goal card for removing burden cards
    /// </summary>
    private static ConversationCard CreateResolutionGoalCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = $"goal_resolution_{npcId}",
            Template = CardTemplateType.Resolve,
            Context = new CardContext
            {
                NPCName = npcName,
                CustomText = "Resolve Issues",
                LetterDetails = "Work through problems to clear burden cards"
            },
            Type = CardType.Shadow,  // Resolution deals with dark matters
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 3,  // Less comfort, but removes burdens
            Category = CardCategory.GOAL,
            IsGoalCard = true,
            GoalCardType = GoalType.Resolution,
            DisplayName = "Resolve Issues",
            Description = "Work through problems together. Success removes burden cards from the deck.",
            SuccessRate = 40  // Harder to achieve resolution
        };
    }
    
    /// <summary>
    /// Create a commerce goal card for special trades
    /// </summary>
    private static ConversationCard CreateCommerceGoalCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = $"goal_commerce_{npcId}",
            Template = CardTemplateType.Negotiate,
            Context = new CardContext
            {
                NPCName = npcName,
                CustomText = "Negotiate Special Trade",
                LetterDetails = "Work out terms for a unique exchange"
            },
            Type = CardType.Commerce,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 4,
            Category = CardCategory.GOAL,
            IsGoalCard = true,
            GoalCardType = GoalType.Commerce,
            DisplayName = "Special Trade",
            Description = "Negotiate a unique trade opportunity. Success determines the terms.",
            SuccessRate = 50  // Commerce is straightforward
        };
    }
    
    /// <summary>
    /// Create a crisis goal card for resolving emergencies
    /// </summary>
    private static ConversationCard CreateCrisisGoalCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = $"goal_crisis_{npcId}",
            Template = CardTemplateType.Crisis,
            Context = new CardContext
            {
                NPCName = npcName,
                CustomText = "Resolve Crisis",
                LetterDetails = "Deal with the emergency situation"
            },
            Type = CardType.Trust,  // Crisis resolution builds trust
            Persistence = PersistenceType.Persistent,
            Weight = 0,  // Free to play in crisis
            BaseComfort = 10,  // Major comfort for resolving crisis
            Category = CardCategory.GOAL,
            IsGoalCard = true,
            GoalCardType = GoalType.Crisis,
            DisplayName = "Resolve Crisis",
            Description = "Deal with the emergency situation. Must be resolved quickly!",
            SuccessRate = 35  // Crisis is difficult
        };
    }
}