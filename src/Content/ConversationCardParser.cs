using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Parses conversation card definitions from JSON
/// </summary>
public class ConversationCardParser
{
    private readonly string _contentPath;
    private Dictionary<string, ConversationCardDTO> _cardTemplates;
    private Dictionary<PersonalityType, PersonalityCardMapping> _personalityMappings;
    private Dictionary<int, List<string>> _tokenUnlocks;

    public ConversationCardParser(string contentPath)
    {
        _contentPath = contentPath;
        _cardTemplates = new Dictionary<string, ConversationCardDTO>();
        _personalityMappings = new Dictionary<PersonalityType, PersonalityCardMapping>();
        _tokenUnlocks = new Dictionary<int, List<string>>();
    }

    /// <summary>
    /// Get the loaded card templates for transfer to GameWorld
    /// This allows the parser to be discarded after initialization
    /// </summary>
    public Dictionary<string, ConversationCardDTO> GetCardTemplates()
    {
        return new Dictionary<string, ConversationCardDTO>(_cardTemplates);
    }

    /// <summary>
    /// Get the loaded personality mappings for transfer to GameWorld
    /// </summary>
    public Dictionary<PersonalityType, PersonalityCardMapping> GetPersonalityMappings()
    {
        return new Dictionary<PersonalityType, PersonalityCardMapping>(_personalityMappings);
    }

    /// <summary>
    /// Get the loaded token unlocks for transfer to GameWorld
    /// </summary>
    public Dictionary<int, List<string>> GetTokenUnlocks()
    {
        return new Dictionary<int, List<string>>(_tokenUnlocks);
    }

    /// <summary>
    /// Get a conversation card by ID
    /// </summary>
    public ConversationCard GetCard(string cardId)
    {
        if (!_cardTemplates.TryGetValue(cardId, out var dto))
            return null;

        return ConvertDTOToCard(dto);
    }

    /// <summary>
    /// Get all cards for an NPC's personality
    /// </summary>
    public List<ConversationCard> GetCardsForNPC(NPC npc, Dictionary<ConnectionType, int> tokens = null)
    {
        var cards = new List<ConversationCard>();

        // Add universal cards
        var universalCards = _cardTemplates.Values
            .Where(c => string.IsNullOrEmpty(c.ForNPC) || c.ForNPC == npc.ID)
            .Where(c => c.IsGoalCard != true && c.Category != CardCategory.BURDEN);

        foreach (var cardDto in universalCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        // Add personality-specific cards
        if (_personalityMappings.TryGetValue(npc.PersonalityType, out var mapping))
        {
            foreach (var cardId in mapping.Cards)
            {
                if (_cardTemplates.TryGetValue(cardId, out var cardDto))
                {
                    cards.Add(ConvertDTOToCard(cardDto, npc));
                }
            }
        }

        // Add token-unlocked cards
        if (tokens != null)
        {
            var totalTokens = tokens.Values.Sum();
            foreach (var kvp in _tokenUnlocks.Where(u => u.Key <= totalTokens))
            {
                foreach (var cardId in kvp.Value)
                {
                    if (_cardTemplates.TryGetValue(cardId, out var cardDto))
                    {
                        cards.Add(ConvertDTOToCard(cardDto, npc));
                    }
                }
            }
        }

        return cards;
    }

    /// <summary>
    /// Get goal card for conversation type
    /// </summary>
    public ConversationCard GetGoalCard(ConversationType conversationType, string npcId, string npcName)
    {
        var goalType = GetGoalTypeForConversation(conversationType);
        if (!goalType.HasValue)
            return null;

        var cardId = $"goal_{goalType.Value.ToString().ToLower()}";
        if (!_cardTemplates.TryGetValue(cardId, out var dto))
            return null;

        var card = ConvertDTOToCard(dto);
        
        // Create new card with customized values (init-only properties)
        return new ConversationCard
        {
            Id = $"{cardId}_{npcId}",
            Template = card.Template,
            Context = new CardContext
            {
                NPCName = npcName,
                Personality = card.Context?.Personality ?? PersonalityType.STEADFAST,
                EmotionalState = card.Context?.EmotionalState ?? EmotionalState.NEUTRAL,
                GeneratesLetterOnSuccess = card.Context?.GeneratesLetterOnSuccess ?? false
            },
            Type = card.Type,
            Persistence = card.Persistence,
            Weight = card.Weight,
            BaseComfort = card.BaseComfort,
            Depth = card.Depth,
            Category = card.Category,
            IsGoalCard = card.IsGoalCard,
            GoalCardType = card.GoalCardType,
            DisplayName = card.DisplayName,
            Description = card.Description,
            SuccessRate = card.SuccessRate,
            SuccessState = card.SuccessState
        };
    }

    /// <summary>
    /// Get crisis cards for an NPC
    /// </summary>
    public List<ConversationCard> GetCrisisCards(NPC npc)
    {
        var cards = new List<ConversationCard>();
        
        var crisisCards = _cardTemplates.Values
            .Where(c => c.Category == CardCategory.BURDEN)
            .Where(c => string.IsNullOrEmpty(c.ForNPC) || c.ForNPC == npc.ID);

        foreach (var cardDto in crisisCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        return cards;
    }

    private ConversationCard ConvertDTOToCard(ConversationCardDTO dto, NPC npc = null)
    {
        // Parse template
        CardTemplateType template = CardTemplateType.SimpleGreeting;
        if (!string.IsNullOrEmpty(dto.Template))
        {
            Enum.TryParse<CardTemplateType>(dto.Template, true, out template);
        }

        // Parse goal type if goal card
        ConversationType? goalType = null;
        if (dto.IsGoalCard == true && !string.IsNullOrEmpty(dto.GoalCardType))
        {
            if (Enum.TryParse<ConversationType>(dto.GoalCardType, true, out var parsed))
            {
                goalType = parsed;
            }
        }

        // Parse success state for state cards
        EmotionalState? successState = null;
        if (dto.IsStateCard == true && !string.IsNullOrEmpty(dto.SuccessState))
        {
            if (Enum.TryParse<EmotionalState>(dto.SuccessState, true, out var parsed))
            {
                successState = parsed;
            }
        }

        // Create card with all init-only properties set at once
        return new ConversationCard
        {
            Id = dto.Id,
            Template = template,
            Context = new CardContext
            {
                Personality = npc?.PersonalityType ?? PersonalityType.STEADFAST,
                EmotionalState = npc?.CurrentEmotionalState ?? EmotionalState.NEUTRAL,
                NPCName = npc?.Name,
                GeneratesLetterOnSuccess = dto.GeneratesLetterOnSuccess ?? false
            },
            Type = Enum.Parse<CardType>(dto.Type, true),
            Persistence = Enum.Parse<PersistenceType>(dto.Persistence, true),
            Weight = dto.Weight,
            BaseComfort = dto.BaseComfort,
            Depth = dto.Depth ?? 0,
            Category = dto.Category,
            IsGoalCard = dto.IsGoalCard ?? false,
            GoalCardType = goalType,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            SuccessRate = dto.SuccessRate ?? 0,
            SuccessState = successState
        };
    }

    private ConversationType? GetGoalTypeForConversation(ConversationType type)
    {
        return type switch
        {
            ConversationType.FriendlyChat => ConversationType.Letter,
            ConversationType.Promise => ConversationType.Letter,
            ConversationType.Resolution => ConversationType.Resolution,
            ConversationType.Crisis => ConversationType.Crisis,
            ConversationType.Commerce => null,
            _ => null
        };
    }
}

/// <summary>
/// DTO for the entire conversations.json file
/// </summary>
public class ConversationDataDTO
{
    public List<ConversationCardDTO> ConversationCards { get; set; }
    public Dictionary<string, PersonalityCardMapping> PersonalityCardMappings { get; set; }
    public Dictionary<string, List<string>> TokenCardUnlocks { get; set; }
}

/// <summary>
/// DTO for individual conversation cards
/// </summary>
public class ConversationCardDTO
{
    public string Id { get; set; }
    public string Template { get; set; }
    public string Type { get; set; }
    public string Persistence { get; set; }
    public int Weight { get; set; }
    public int BaseComfort { get; set; }
    public int? Depth { get; set; }
    public CardCategory Category { get; set; }
    public bool? IsGoalCard { get; set; }
    public string GoalCardType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int? SuccessRate { get; set; }
    public string ForNPC { get; set; }
    public bool? GeneratesLetterOnSuccess { get; set; }
    public bool? IsStateCard { get; set; }
    public string SuccessState { get; set; }
}

/// <summary>
/// Personality to card mappings
/// </summary>
public class PersonalityCardMapping
{
    public List<string> Cards { get; set; }
    public string StateBias { get; set; }
}