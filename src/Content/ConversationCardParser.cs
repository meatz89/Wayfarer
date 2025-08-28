using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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

    public void LoadConversationCards()
    {
        var filePath = Path.Combine(_contentPath, "conversations.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: conversations.json not found at {filePath}");
            return;
        }

        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<ConversationDataDTO>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (data?.ConversationCards != null)
        {
            foreach (var card in data.ConversationCards)
            {
                _cardTemplates[card.Id] = card;
            }
            Console.WriteLine($"Loaded {_cardTemplates.Count} conversation card templates");
        }

        if (data?.PersonalityCardMappings != null)
        {
            foreach (var kvp in data.PersonalityCardMappings)
            {
                if (Enum.TryParse<PersonalityType>(kvp.Key, true, out var personality))
                {
                    _personalityMappings[personality] = kvp.Value;
                }
            }
        }

        if (data?.TokenCardUnlocks != null)
        {
            foreach (var kvp in data.TokenCardUnlocks)
            {
                if (int.TryParse(kvp.Key, out var tokenCount))
                {
                    _tokenUnlocks[tokenCount] = kvp.Value;
                }
            }
        }
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
            .Where(c => c.Category != CardCategory.GOAL && c.Category != CardCategory.CRISIS);

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
            .Where(c => c.Category == CardCategory.CRISIS)
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
        GoalType? goalType = null;
        if (dto.IsGoalCard == true && !string.IsNullOrEmpty(dto.GoalCardType))
        {
            if (Enum.TryParse<GoalType>(dto.GoalCardType, true, out var parsed))
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

    private GoalType? GetGoalTypeForConversation(ConversationType type)
    {
        return type switch
        {
            ConversationType.Standard => GoalType.Letter,
            ConversationType.LetterOffer => GoalType.Letter,
            ConversationType.MakeAmends => GoalType.Resolution,
            ConversationType.Crisis => GoalType.Crisis,
            ConversationType.QuickExchange => null,
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