using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Repository for loading letter deck configurations from JSON.
/// Maps NPCs to their available letter goal cards with eligibility requirements.
/// </summary>
public class LetterDeckRepository
{
    private readonly string _letterDecksPath;
    private Dictionary<string, List<LetterCardConfiguration>> _npcLetterDecks;
    
    public LetterDeckRepository(string contentPath)
    {
        _letterDecksPath = Path.Combine(contentPath, "letter_decks.json");
        _npcLetterDecks = new Dictionary<string, List<LetterCardConfiguration>>();
    }
    
    /// <summary>
    /// Load letter deck configurations from JSON
    /// </summary>
    public void LoadLetterDecks()
    {
        if (!File.Exists(_letterDecksPath))
        {
            Console.WriteLine($"[LetterDeckRepository] letter_decks.json not found at {_letterDecksPath}");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(_letterDecksPath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty("letterDecks", out JsonElement letterDecksArray))
            {
                foreach (JsonElement deckElement in letterDecksArray.EnumerateArray())
                {
                    ParseLetterDeck(deckElement);
                }
            }
            
            Console.WriteLine($"[LetterDeckRepository] Loaded letter decks for {_npcLetterDecks.Count} NPCs");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LetterDeckRepository] Error loading letter_decks.json: {ex.Message}");
        }
    }
    
    private void ParseLetterDeck(JsonElement deckElement)
    {
        string npcId = GetStringProperty(deckElement, "npcId", "");
        if (string.IsNullOrEmpty(npcId)) return;
        
        var letterCards = new List<LetterCardConfiguration>();
        
        if (deckElement.TryGetProperty("letterCards", out JsonElement cardsArray))
        {
            foreach (JsonElement cardElement in cardsArray.EnumerateArray())
            {
                var config = ParseLetterCardConfiguration(cardElement);
                if (config != null)
                {
                    letterCards.Add(config);
                }
            }
        }
        
        if (letterCards.Any())
        {
            _npcLetterDecks[npcId] = letterCards;
            Console.WriteLine($"[LetterDeckRepository] Loaded {letterCards.Count} letter cards for NPC {npcId}");
        }
    }
    
    private LetterCardConfiguration ParseLetterCardConfiguration(JsonElement cardElement)
    {
        var config = new LetterCardConfiguration
        {
            CardId = GetStringProperty(cardElement, "cardId", ""),
            EligibilityRequirements = ParseEligibilityRequirements(cardElement.GetProperty("eligibilityRequirements")),
            NegotiationTerms = ParseNegotiationTerms(cardElement.GetProperty("negotiationTerms")),
            LetterDetails = ParseLetterDetails(cardElement.GetProperty("letterDetails"))
        };
        
        return config;
    }
    
    private EligibilityRequirements ParseEligibilityRequirements(JsonElement element)
    {
        var requirements = new EligibilityRequirements();
        
        // Parse required states
        if (element.TryGetProperty("requiredStates", out JsonElement statesArray))
        {
            foreach (JsonElement stateElement in statesArray.EnumerateArray())
            {
                string stateStr = stateElement.GetString();
                if (Enum.TryParse<EmotionalState>(stateStr, true, out EmotionalState state))
                {
                    requirements.RequiredStates.Add(state);
                }
            }
        }
        
        // Parse required tokens
        if (element.TryGetProperty("requiredTokens", out JsonElement tokensElement))
        {
            foreach (JsonProperty tokenProp in tokensElement.EnumerateObject())
            {
                if (Enum.TryParse<ConnectionType>(tokenProp.Name, true, out ConnectionType tokenType))
                {
                    requirements.RequiredTokens[tokenType] = tokenProp.Value.GetInt32();
                }
            }
        }
        
        requirements.MinTokens = GetIntProperty(element, "minTokens", 0);
        
        return requirements;
    }
    
    private NegotiationTerms ParseNegotiationTerms(JsonElement element)
    {
        var terms = new NegotiationTerms
        {
            BaseSuccessRate = GetIntProperty(element, "baseSuccessRate", 50),
            SuccessTerms = ParseTermDetails(element.GetProperty("successTerms")),
            FailureTerms = ParseTermDetails(element.GetProperty("failureTerms"))
        };
        
        return terms;
    }
    
    private TermDetails ParseTermDetails(JsonElement element)
    {
        return new TermDetails
        {
            DeadlineMinutes = GetIntProperty(element, "deadlineMinutes", 1440),
            QueuePosition = GetIntProperty(element, "queuePosition", 3),
            Payment = GetIntProperty(element, "payment", 5),
            ForcesPositionOne = GetBoolProperty(element, "forcesPositionOne", false)
        };
    }
    
    private LetterDetails ParseLetterDetails(JsonElement element)
    {
        var details = new LetterDetails
        {
            RecipientId = GetStringProperty(element, "recipientId", ""),
            RecipientName = GetStringProperty(element, "recipientName", ""),
            Description = GetStringProperty(element, "description", "")
        };
        
        // Parse token type
        string tokenTypeStr = GetStringProperty(element, "tokenType", "Trust");
        if (Enum.TryParse<ConnectionType>(tokenTypeStr, true, out ConnectionType tokenType))
        {
            details.TokenType = tokenType;
        }
        
        // Parse stakes
        string stakesStr = GetStringProperty(element, "stakes", "REPUTATION");
        if (Enum.TryParse<StakeType>(stakesStr, true, out StakeType stakes))
        {
            details.Stakes = stakes;
        }
        
        // Parse emotional weight
        string weightStr = GetStringProperty(element, "emotionalWeight", "MEDIUM");
        if (Enum.TryParse<EmotionalWeight>(weightStr, true, out EmotionalWeight weight))
        {
            details.EmotionalWeight = weight;
        }
        
        return details;
    }
    
    /// <summary>
    /// Get letter card configurations for a specific NPC
    /// </summary>
    public List<LetterCardConfiguration> GetLetterCardsForNPC(string npcId)
    {
        if (_npcLetterDecks.TryGetValue(npcId, out var cards))
        {
            return cards;
        }
        return new List<LetterCardConfiguration>();
    }
    
    /// <summary>
    /// Check if NPC has any letter cards
    /// </summary>
    public bool NPCHasLetterCards(string npcId)
    {
        return _npcLetterDecks.ContainsKey(npcId) && _npcLetterDecks[npcId].Any();
    }
    
    private string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? defaultValue;
        }
        return defaultValue;
    }
    
    private int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }
    
    private bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.True) return true;
            if (property.ValueKind == JsonValueKind.False) return false;
        }
        return defaultValue;
    }
}

/// <summary>
/// Configuration for a letter card in an NPC's letter deck
/// </summary>
public class LetterCardConfiguration
{
    public string CardId { get; set; }
    public EligibilityRequirements EligibilityRequirements { get; set; }
    public NegotiationTerms NegotiationTerms { get; set; }
    public LetterDetails LetterDetails { get; set; }
}

public class EligibilityRequirements
{
    public List<EmotionalState> RequiredStates { get; set; } = new();
    public Dictionary<ConnectionType, int> RequiredTokens { get; set; } = new();
    public int MinTokens { get; set; }
}

public class NegotiationTerms
{
    public int BaseSuccessRate { get; set; }
    public TermDetails SuccessTerms { get; set; }
    public TermDetails FailureTerms { get; set; }
}

public class TermDetails
{
    public int DeadlineMinutes { get; set; }
    public int QueuePosition { get; set; }
    public int Payment { get; set; }
    public bool ForcesPositionOne { get; set; }
}

public class LetterDetails
{
    public string RecipientId { get; set; }
    public string RecipientName { get; set; }
    public string Description { get; set; }
    public ConnectionType TokenType { get; set; }
    public StakeType Stakes { get; set; }
    public EmotionalWeight EmotionalWeight { get; set; }
}