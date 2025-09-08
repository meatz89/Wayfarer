using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stateless parser for converting conversation card DTOs to domain models
/// </summary>
public static class ConversationCardParser
{
    /// <summary>
    /// Get all cards for an NPC's personality from provided card data
    /// </summary>
    public static List<ConversationCard> GetCardsForNPC(NPC npc,
        Dictionary<string, ConversationCardDTO> cardTemplates,
        Dictionary<PersonalityType, PersonalityCardMapping> personalityMappings,
        Dictionary<int, List<string>> tokenUnlocks,
        Dictionary<ConnectionType, int> tokens = null)
    {
        List<ConversationCard> cards = new List<ConversationCard>();

        // Add universal cards
        IEnumerable<ConversationCardDTO> universalCards = cardTemplates.Values
            .Where(c => string.IsNullOrEmpty(c.ForNPC) || c.ForNPC == npc.ID)
            .Where(c => c.IsRequestCard != true);

        foreach (ConversationCardDTO cardDto in universalCards)
        {
            cards.Add(ConvertDTOToCard(cardDto, npc));
        }

        // Add personality-specific cards
        if (personalityMappings.TryGetValue(npc.PersonalityType, out PersonalityCardMapping mapping))
        {
            foreach (string cardId in mapping.Cards)
            {
                if (cardTemplates.TryGetValue(cardId, out ConversationCardDTO cardDto))
                {
                    cards.Add(ConvertDTOToCard(cardDto, npc));
                }
            }
        }

        // Add token-unlocked cards
        if (tokens != null)
        {
            int totalTokens = tokens.Values.Sum();
            foreach (KeyValuePair<int, List<string>> kvp in tokenUnlocks.Where(u => u.Key <= totalTokens))
            {
                foreach (string cardId in kvp.Value)
                {
                    if (cardTemplates.TryGetValue(cardId, out ConversationCardDTO cardDto))
                    {
                        cards.Add(ConvertDTOToCard(cardDto, npc));
                    }
                }
            }
        }

        return cards;
    }

    /// <summary>
    /// Get request card for conversation type from provided card data
    /// </summary>
    public static ConversationCard GetRequestCard(ConversationType conversationType, string npcId, string npcName,
        Dictionary<string, ConversationCardDTO> cardTemplates)
    {
        ConversationType? requestType = GetRequestTypeForConversation(conversationType);
        if (!requestType.HasValue)
            return null;

        string cardId = $"request_{requestType.Value.ToString().ToLower()}";
        if (!cardTemplates.TryGetValue(cardId, out ConversationCardDTO dto))
            return null;

        // Mark as request card to ensure proper parsing
        dto.IsRequestCard = true;
        
        // Convert to RequestCard (will preserve default EndConversation effects)
        ConversationCard card = ConvertDTOToCard(dto);
        
        // If it's a RequestCard, customize the ID for this specific NPC
        if (card is RequestCard requestCard)
        {
            requestCard.Id = $"{cardId}_{npcId}";
            return requestCard;
        }

        // Fallback for non-request cards (shouldn't happen)
        card.Id = $"{cardId}_{npcId}";
        return card;
    }

    /// <summary>
    /// Convert a ConversationCardDTO to a ConversationCard domain model
    /// </summary>
    public static ConversationCard ConvertDTOToCard(ConversationCardDTO dto, NPC npc = null)
    {
        // Parse token type from connection type
        TokenType tokenType = TokenType.Trust; // Default
        if (!string.IsNullOrEmpty(dto.ConnectionType))
        {
            ConnectionType connType = ConnectionType.None;
            if (Enum.TryParse<ConnectionType>(dto.ConnectionType, true, out connType))
            {
                tokenType = connType switch
                {
                    ConnectionType.Trust => TokenType.Trust,
                    ConnectionType.Commerce => TokenType.Commerce,
                    ConnectionType.Status => TokenType.Status,
                    ConnectionType.Shadow => TokenType.Shadow,
                    _ => TokenType.Trust
                };
            }
        }

        // Parse difficulty
        Difficulty difficulty = Difficulty.Medium; // Default
        if (!string.IsNullOrEmpty(dto.Difficulty))
        {
            Enum.TryParse<Difficulty>(dto.Difficulty, true, out difficulty);
        }

        // Parse three-effect system from DTO
        CardEffect successEffect = ParseEffect(dto.SuccessEffect) ?? CardEffect.None;
        CardEffect failureEffect = ParseEffect(dto.FailureEffect) ?? CardEffect.None;
        CardEffect exhaustEffect = ParseEffect(dto.ExhaustEffect) ?? CardEffect.None;

        // Check if this is a Request card
        ConversationCard card;
        if (dto.Type == "Request" || dto.Type == "Goal" || dto.IsRequestCard == true)
        {
            // Create RequestCard for request/promise cards
            RequestCard requestCard = new RequestCard
            {
                Id = dto.Id,
                Description = dto.Description ?? "",
                TokenType = tokenType,
                // Request cards have 0 focus now
                Focus = 0,
                // Always 100% success
                Difficulty = Difficulty.VeryEasy,
                PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
                DialogueFragment = dto.DialogueFragment,
                VerbPhrase = "", // Will be set later if needed
                // Set rapport threshold
                RapportThreshold = dto.RapportThreshold ?? 5,
                // Set goal type for FriendlyChat detection
                GoalType = dto.GoalType
            };
            
            // Only override effects if explicitly provided in JSON
            // RequestCard constructor already sets default EndConversation effects
            if (successEffect != null && successEffect.Type != CardEffectType.None)
            {
                requestCard.SuccessEffect = successEffect;
            }
            // RequestCard defaults handle failure and exhaust effects
            
            card = requestCard;
        }
        else
        {
            // Create normal ConversationCard
            card = new ConversationCard
            {
                Id = dto.Id,
                Description = dto.Description ?? "",
                TokenType = tokenType,
                Focus = dto.Focus,
                Difficulty = difficulty,
                PersonalityTypes = dto.PersonalityTypes != null ? new List<string>(dto.PersonalityTypes) : new List<string>(),
                // Three-effect system
                SuccessEffect = successEffect ?? CardEffect.None,
                FailureEffect = failureEffect ?? CardEffect.None,
                ExhaustEffect = exhaustEffect ?? CardEffect.None,
                DialogueFragment = dto.DialogueFragment,
                VerbPhrase = "" // Will be set later if needed
            };
        }

        // Parse properties array
        if (dto.Properties != null && dto.Properties.Count > 0)
        {
            foreach (string prop in dto.Properties)
            {
                if (Enum.TryParse<CardProperty>(prop, true, out CardProperty cardProp))
                {
                    if (!card.Properties.Contains(cardProp))
                        card.Properties.Add(cardProp);
                }
            }
        }

        return card;
    }

    private static ConversationType? GetRequestTypeForConversation(ConversationType type)
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

    /// <summary>
    /// Convert NPCGoalCardDTO to RequestCard
    /// </summary>
    public static RequestCard ConvertGoalCardDTO(NPCGoalCardDTO dto)
    {
        RequestCard card = new RequestCard
        {
            Id = dto.Id,
            Description = dto.Description,
            Focus = dto.Focus,
            DialogueFragment = dto.DialogueFragment,
            RapportThreshold = dto.RapportThreshold,
            GoalType = dto.Type // Store the goal type from JSON
        };

        // Parse difficulty
        if (Enum.TryParse<Difficulty>(dto.Difficulty, true, out Difficulty difficulty))
        {
            card.Difficulty = difficulty;
        }

        // Parse token type
        if (Enum.TryParse<TokenType>(dto.ConnectionType, true, out TokenType tokenType))
        {
            card.TokenType = tokenType;
        }

        // Parse properties
        if (dto.Properties != null)
        {
            foreach (string prop in dto.Properties)
            {
                if (Enum.TryParse<CardProperty>(prop, true, out CardProperty cardProp))
                {
                    if (!card.Properties.Contains(cardProp))
                    {
                        card.Properties.Add(cardProp);
                    }
                }
            }
        }

        // Only override success effect if explicitly provided in JSON
        // RequestCard constructor already sets default EndConversation effect
        if (dto.SuccessEffect != null)
        {
            CardEffect parsed = ParseEffect(dto.SuccessEffect);
            if (parsed != null && parsed.Type != CardEffectType.None)
            {
                card.SuccessEffect = parsed;
            }
        }

        return card;
    }

    /// <summary>
    /// Parse a CardEffect from DTO
    /// </summary>
    private static CardEffect ParseEffect(CardEffectDTO dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Type))
            return null;

        if (!Enum.TryParse<CardEffectType>(dto.Type, true, out CardEffectType effectType))
            return null;

        CardEffect effect = new CardEffect
        {
            Type = effectType,
            Value = dto.Value
        };

        // For Exchange effects, the Value field contains the exchange ID
        // The actual exchange data will be populated at runtime from GameWorld.ExchangeDefinitions

        return effect;
    }

    /// <summary>
    /// Parse resource type from string
    /// </summary>
    private static ResourceType? ParseResourceType(string name)
    {
        return name.ToLower() switch
        {
            "coins" => ResourceType.Coins,
            "health" => ResourceType.Health,
            "food" => ResourceType.Food,
            "hunger" => ResourceType.Hunger,
            "attention" => ResourceType.Attention,
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
    public string ConnectionType { get; set; }
    public string Persistence { get; set; }
    public int Focus { get; set; }
    public int BaseFlow { get; set; }
    public bool? IsRequestCard { get; set; }
    public string RequestCardType { get; set; }
    public int? RapportThreshold { get; set; } // For request cards
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int? SuccessRate { get; set; }
    public string ForNPC { get; set; }
    public bool? GeneratesLetterOnSuccess { get; set; }
    public bool? IsStateCard { get; set; }
    public string SuccessState { get; set; }
    public int? PatienceBonus { get; set; } // Patience added when this card succeeds
    public string DialogueFragment { get; set; } // Added dialogue fragment

    // New properties array replaces persistence and special flags
    public List<string> Properties { get; set; }

    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; }

    // New target system properties
    public string Difficulty { get; set; }
    // Three-effect system
    public CardEffectDTO SuccessEffect { get; set; }
    public CardEffectDTO FailureEffect { get; set; }
    public CardEffectDTO ExhaustEffect { get; set; }
    
    // Goal type for FriendlyChat detection
    public string GoalType { get; set; }
}

/// <summary>
/// DTO for card effect data
/// </summary>
public class CardEffectDTO
{
    public string Type { get; set; }
    public string Value { get; set; }
    public Dictionary<string, object> Data { get; set; }

    // For Exchange effects specifically - populated during deserialization
    public ExchangeCost Cost { get; set; }
    public ExchangeReward Reward { get; set; }
}

/// <summary>
/// Personality to card mappings
/// </summary>
public class PersonalityCardMapping
{
    public List<string> Cards { get; set; }
    public string StateBias { get; set; }
}