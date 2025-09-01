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
            .Where(c => c.IsGoalCard != true);

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
            TemplateId = card.TemplateId,
            Mechanics = card.Mechanics,
            Category = card.Category,
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
            IsGoalCard = card.IsGoalCard,
            GoalCardType = card.GoalCardType,
            DisplayName = card.DisplayName,
            Description = card.Description,
            SuccessRate = card.SuccessRate,
            SuccessState = card.SuccessState,
            DrawableStates = card.DrawableStates,
            PatienceBonus = card.PatienceBonus
        };
    }

    private ConversationCard ConvertDTOToCard(ConversationCardDTO dto, NPC npc = null)
    {
        // Parse mechanics from template string or default to Standard
        CardMechanicsType mechanics = CardMechanicsType.Standard;
        if (!string.IsNullOrEmpty(dto.Mechanics))
        {
            Enum.TryParse<CardMechanicsType>(dto.Mechanics, true, out mechanics);
        }
        
        // Parse category from DTO or infer from mechanics
        CardCategory category = CardCategory.Comfort; // Default
        if (!string.IsNullOrEmpty(dto.Category))
        {
            Enum.TryParse<CardCategory>(dto.Category, true, out category);
        }
        else
        {
            // Infer category from mechanics if not specified
            category = mechanics switch
            {
                CardMechanicsType.Exchange => CardCategory.Exchange,
                CardMechanicsType.Promise => CardCategory.Promise,
                CardMechanicsType.StateChange => CardCategory.State,
                _ => CardCategory.Comfort
            };
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
        
        // Parse DrawableStates list
        List<EmotionalState> drawableStates = null;
        if (dto.DrawableStates != null && dto.DrawableStates.Any())
        {
            drawableStates = new List<EmotionalState>();
            foreach (var stateString in dto.DrawableStates)
            {
                if (Enum.TryParse<EmotionalState>(stateString, true, out var state))
                {
                    drawableStates.Add(state);
                }
            }
        }

        // Parse new target system properties
        Difficulty difficulty = Difficulty.Medium; // Default
        if (!string.IsNullOrEmpty(dto.Difficulty))
        {
            Enum.TryParse<Difficulty>(dto.Difficulty, true, out difficulty);
        }

        CardEffectType effectType = CardEffectType.FixedComfort; // Default
        if (!string.IsNullOrEmpty(dto.EffectType))
        {
            Enum.TryParse<CardEffectType>(dto.EffectType, true, out effectType);
        }

        ConversationAtmosphere? atmosphereChange = null;
        if (!string.IsNullOrEmpty(dto.ConversationAtmosphereChange))
        {
            if (Enum.TryParse<ConversationAtmosphere>(dto.ConversationAtmosphereChange, true, out var atmosphere))
            {
                atmosphereChange = atmosphere;
            }
        }

        // Create card with all init-only properties set at once
        return new ConversationCard
        {
            Id = dto.Id,
            TemplateId = dto.Template,
            Mechanics = mechanics,
            Category = category.ToString(),
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
            IsGoalCard = dto.IsGoalCard ?? false,
            GoalCardType = goalType?.ToString(),
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            SuccessRate = dto.SuccessRate ?? 0,
            SuccessState = successState,
            DrawableStates = drawableStates,
            PatienceBonus = dto.PatienceBonus ?? 0,
            
            // New target system properties
            Difficulty = difficulty,
            EffectType = effectType,
            EffectValue = dto.EffectValue,
            EffectFormula = dto.EffectFormula,
            ConversationAtmosphereChange = atmosphereChange
        };
    }

    private ConversationType? GetGoalTypeForConversation(ConversationType type)
    {
        return type switch
        {
            ConversationType.FriendlyChat => ConversationType.Promise,
            ConversationType.Promise => ConversationType.Promise,
            ConversationType.Resolution => ConversationType.Resolution,
            ConversationType.Delivery => ConversationType.Delivery,
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
    public string Template { get; set; } // Maps to TemplateId in card
    public string Mechanics { get; set; } // New mechanics field
    public string Category { get; set; } // New category field
    public string Type { get; set; }
    public string Persistence { get; set; }
    public int Weight { get; set; }
    public int BaseComfort { get; set; }
    public bool? IsGoalCard { get; set; }
    public string GoalCardType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int? SuccessRate { get; set; }
    public string ForNPC { get; set; }
    public bool? GeneratesLetterOnSuccess { get; set; }
    public bool? IsStateCard { get; set; }
    public string SuccessState { get; set; }
    public List<string> DrawableStates { get; set; } // NEW: States this card can be drawn in
    public int? PatienceBonus { get; set; } // Patience added when this card succeeds
    
    // New target system properties
    public string Difficulty { get; set; }
    public string EffectType { get; set; }
    public string EffectValue { get; set; }
    public string EffectFormula { get; set; }
    public string ConversationAtmosphereChange { get; set; }
}

/// <summary>
/// Personality to card mappings
/// </summary>
public class PersonalityCardMapping
{
    public List<string> Cards { get; set; }
    public string StateBias { get; set; }
}