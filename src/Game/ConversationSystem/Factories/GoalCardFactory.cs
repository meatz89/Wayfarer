using System;

/// <summary>
/// Factory for creating goal cards that are shuffled into conversation decks.
/// Goal cards define the purpose of a conversation and end it when played.
/// All cards are loaded from GameWorld.CardTemplates
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
        
        // Find the appropriate goal card from GameWorld templates
        var goalType = GetGoalTypeForConversation(conversationType);
        
        // Look for a goal card matching the conversation type
        foreach (var kvp in _gameWorld.CardTemplates)
        {
            var dto = kvp.Value;
            if (dto.IsGoalCard == true && 
                dto.GoalCardType == goalType?.ToString() &&
                (string.IsNullOrEmpty(dto.ForNPC) || dto.ForNPC == npcId))
            {
                return ConvertDTOToCard(dto, npcId, npcName);
            }
        }
        
        // Fallback to generic goal card if no specific one found
        foreach (var kvp in _gameWorld.CardTemplates)
        {
            var dto = kvp.Value;
            if (dto.IsGoalCard == true && 
                dto.GoalCardType == goalType?.ToString() &&
                string.IsNullOrEmpty(dto.ForNPC))
            {
                return ConvertDTOToCard(dto, npcId, npcName);
            }
        }
        
        Console.WriteLine($"WARNING: No goal card found for conversation type {conversationType}");
        return null;
    }
    
    private static GoalType? GetGoalTypeForConversation(ConversationType type)
    {
        return type switch
        {
            ConversationType.Standard => GoalType.Letter,
            ConversationType.LetterOffer => GoalType.Letter,
            ConversationType.MakeAmends => GoalType.Resolution,
            ConversationType.QuickExchange => GoalType.Commerce,
            ConversationType.Crisis => GoalType.Crisis,
            _ => null
        };
    }
    
    private static ConversationCard ConvertDTOToCard(ConversationCardDTO dto, string npcId, string npcName)
    {
        // Parse template
        CardTemplateType template = CardTemplateType.SimpleGreeting;
        if (!string.IsNullOrEmpty(dto.Template))
        {
            Enum.TryParse<CardTemplateType>(dto.Template, true, out template);
        }

        // Parse goal type
        GoalType? goalType = null;
        if (!string.IsNullOrEmpty(dto.GoalCardType))
        {
            if (Enum.TryParse<GoalType>(dto.GoalCardType, true, out var parsed))
            {
                goalType = parsed;
            }
        }

        // Create card with all init-only properties set at once
        return new ConversationCard
        {
            Id = dto.Id,
            Template = template,
            Context = new CardContext
            {
                Personality = PersonalityType.STEADFAST, // Default for goal cards
                EmotionalState = EmotionalState.NEUTRAL,
                NPCName = npcName,
                GeneratesLetterOnSuccess = dto.GeneratesLetterOnSuccess ?? false
            },
            Type = Enum.Parse<CardType>(dto.Type, true),
            Persistence = Enum.Parse<PersistenceType>(dto.Persistence, true),
            Weight = dto.Weight,
            BaseComfort = dto.BaseComfort,
            Depth = dto.Depth ?? 0,
            Category = dto.Category,
            IsGoalCard = true,
            GoalCardType = goalType,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            SuccessRate = dto.SuccessRate ?? 0
        };
    }
}