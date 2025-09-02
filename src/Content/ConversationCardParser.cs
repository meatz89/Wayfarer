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
            .Where(c => c.IsGoalCard != true);

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
    /// Get goal card for conversation type from provided card data
    /// </summary>
    public static ConversationCard GetGoalCard(ConversationType conversationType, string npcId, string npcName, 
        Dictionary<string, ConversationCardDTO> cardTemplates)
    {
        ConversationType? goalType = GetGoalTypeForConversation(conversationType);
        if (!goalType.HasValue)
            return null;

        string cardId = $"goal_{goalType.Value.ToString().ToLower()}";
        if (!cardTemplates.TryGetValue(cardId, out ConversationCardDTO dto))
            return null;

        ConversationCard card = ConvertDTOToCard(dto);

        // Create new card with customized values
        var newCard = new ConversationCard
        {
            Id = $"{cardId}_{npcId}",
            Name = card.Name,
            Description = card.Description,
            Weight = card.Weight,
            TokenType = card.TokenType,
            Difficulty = card.Difficulty,
            SuccessEffect = card.SuccessEffect,
            FailureEffect = card.FailureEffect,
            ExhaustEffect = card.ExhaustEffect,
            DialogueFragment = card.DialogueFragment,
            VerbPhrase = card.VerbPhrase,
            Properties = new List<CardProperty>(card.Properties) // Copy properties from source
        };
        
        return newCard;
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
        CardEffect successEffect = ParseEffect(dto.SuccessEffect) ?? 
            ParseLegacyEffect(dto.EffectType, dto.EffectValue, dto.EffectFormula, dto.AtmosphereTypeChange);
        CardEffect failureEffect = ParseEffect(dto.FailureEffect);
        CardEffect exhaustEffect = ParseEffect(dto.ExhaustEffect);

        // Create card with essential properties
        var card = new ConversationCard
        {
            Id = dto.Id,
            Name = dto.DisplayName ?? dto.Id,
            Description = dto.Description ?? "",
            TokenType = tokenType,
            Weight = dto.Weight,
            Difficulty = difficulty,
            // Three-effect system
            SuccessEffect = successEffect ?? CardEffect.None,
            FailureEffect = failureEffect ?? CardEffect.None,
            ExhaustEffect = exhaustEffect ?? CardEffect.None,
            DialogueFragment = dto.DialogueFragment,
            VerbPhrase = "" // Will be set later if needed
        };
        
        // Parse properties array if present (new format)
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
        else
        {
            // Legacy property handling for backwards compatibility
            if (dto.IsGoalCard ?? false)
            {
                card.Properties.Add(CardProperty.Fleeting);
                card.Properties.Add(CardProperty.Opportunity);
            }
            
            // Handle observation cards
            if (dto.Type == "Observation")
            {
                card.Properties.Add(CardProperty.Observable);
            }
            
            // Set persistence property from legacy field
            if (dto.Persistence == "Fleeting")
            {
                if (!card.Properties.Contains(CardProperty.Fleeting))
                    card.Properties.Add(CardProperty.Fleeting);
            }
            else if (dto.Persistence == "Opportunity")
            {
                if (!card.Properties.Contains(CardProperty.Opportunity))
                    card.Properties.Add(CardProperty.Opportunity);
            }
            else
            {
                if (!card.Properties.Contains(CardProperty.Persistent))
                    card.Properties.Add(CardProperty.Persistent);
            }
        }
        
        // Ensure at least one property is set
        card.EnsureDefaultProperties();
        
        return card;
    }

    private static ConversationType? GetGoalTypeForConversation(ConversationType type)
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
    /// Parse a CardEffect from DTO
    /// </summary>
    private static CardEffect ParseEffect(CardEffectDTO dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Type))
            return null;
            
        if (!Enum.TryParse<CardEffectType>(dto.Type, true, out CardEffectType effectType))
            return null;
            
        return new CardEffect
        {
            Type = effectType,
            Value = dto.Value,
            Data = dto.Data
        };
    }
    
    /// <summary>
    /// Parse legacy effect properties into new CardEffect
    /// </summary>
    private static CardEffect ParseLegacyEffect(string effectTypeStr, string effectValue, string effectFormula, string atmosphereChange)
    {
        if (string.IsNullOrEmpty(effectTypeStr) && string.IsNullOrEmpty(effectValue) && 
            string.IsNullOrEmpty(effectFormula) && string.IsNullOrEmpty(atmosphereChange))
            return null;
            
        // Handle atmosphere change
        if (!string.IsNullOrEmpty(atmosphereChange))
        {
            return new CardEffect
            {
                Type = CardEffectType.SetAtmosphere,
                Value = atmosphereChange
            };
        }
        
        // Parse effect type
        CardEffectType effectType = CardEffectType.AddComfort; // Default
        if (!string.IsNullOrEmpty(effectTypeStr))
        {
            if (!Enum.TryParse<CardEffectType>(effectTypeStr, true, out effectType))
            {
                // Map legacy types
                effectType = effectTypeStr.ToLower() switch
                {
                    "fixedcomfort" => CardEffectType.AddComfort,
                    "scaledcomfort" => CardEffectType.ScaleByTokens,
                    "drawcards" => CardEffectType.DrawCards,
                    "addweight" => CardEffectType.AddWeight,
                    _ => CardEffectType.AddComfort
                };
            }
        }
        
        // Determine value
        string value = !string.IsNullOrEmpty(effectFormula) ? effectFormula : 
                      !string.IsNullOrEmpty(effectValue) ? effectValue : "0";
        
        return new CardEffect
        {
            Type = effectType,
            Value = value
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
    public int? PatienceBonus { get; set; } // Patience added when this card succeeds
    public string DialogueFragment { get; set; } // Added dialogue fragment

    // New properties array replaces persistence and special flags
    public List<string> Properties { get; set; }
    
    // New target system properties
    public string Difficulty { get; set; }
    // Three-effect system
    public CardEffectDTO SuccessEffect { get; set; }
    public CardEffectDTO FailureEffect { get; set; }
    public CardEffectDTO ExhaustEffect { get; set; }
    // Legacy properties for compatibility
    public string EffectType { get; set; }
    public string EffectValue { get; set; }
    public string EffectFormula { get; set; }
    public string AtmosphereTypeChange { get; set; }
}

/// <summary>
/// DTO for card effect data
/// </summary>
public class CardEffectDTO
{
    public string Type { get; set; }
    public string Value { get; set; }
    public Dictionary<string, object> Data { get; set; }
}

/// <summary>
/// Personality to card mappings
/// </summary>
public class PersonalityCardMapping
{
    public List<string> Cards { get; set; }
    public string StateBias { get; set; }
}