using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Loads all card deck configurations from JSON files
/// Creates unified ConversationCard objects for all deck types
/// </summary>
public class CardDeckLoader
{
    private readonly string _contentPath;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Storage for all loaded data
    private Dictionary<string, ConversationCard> _allCards = new();
    private Dictionary<string, List<string>> _npcConversationDecks = new();
    private Dictionary<string, List<ConversationCard>> _npcGoalDecks = new();
    private Dictionary<string, List<ConversationCard>> _npcExchangeDecks = new();
    private List<ConversationCard> _playerObservationCards = new();
    private List<TravelCard> _travelCards = new();
    private TravelCardLoader _travelCardLoader;
    
    public CardDeckLoader(string contentPath)
    {
        _contentPath = contentPath;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        _travelCardLoader = new TravelCardLoader(contentPath);
    }
    
    /// <summary>
    /// Load all deck configurations from JSON files
    /// </summary>
    public void LoadAllDecks()
    {
        // 1. Load card templates first (base definitions)
        LoadCardTemplates();
        
        // 2. Load NPC conversation deck mappings
        LoadNPCConversationDecks();
        
        // 3. Load NPC goal decks (full card definitions)
        LoadNPCGoalDecks();
        
        // 4. Load NPC exchange decks (mercantile NPCs only)
        LoadNPCExchangeDecks();
        
        // 5. Load player observation cards
        LoadPlayerObservationCards();
        
        // 6. Load travel cards
        LoadTravelCards();
        
        Console.WriteLine($"[CardDeckLoader] Loaded {_allCards.Count} total cards");
        Console.WriteLine($"[CardDeckLoader] Loaded {_npcConversationDecks.Count} NPC conversation decks");
        Console.WriteLine($"[CardDeckLoader] Loaded {_npcGoalDecks.Count} NPC goal decks");
        Console.WriteLine($"[CardDeckLoader] Loaded {_npcExchangeDecks.Count} NPC exchange decks");
        Console.WriteLine($"[CardDeckLoader] Loaded {_playerObservationCards.Count} player observation cards");
        Console.WriteLine($"[CardDeckLoader] Loaded {_travelCards.Count} travel cards");
    }
    
    /// <summary>
    /// Load card templates from card_templates.json
    /// </summary>
    private void LoadCardTemplates()
    {
        var filePath = Path.Combine(_contentPath, "card_templates.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: card_templates.json not found");
            return;
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("templates", out var templates))
        {
            foreach (var prop in templates.EnumerateObject())
            {
                var cardId = prop.Name;
                var template = prop.Value;
                
                var card = CreateCardFromTemplate(cardId, template);
                if (card != null)
                {
                    _allCards[cardId] = card;
                }
            }
        }
        
        Console.WriteLine($"[CardDeckLoader] Loaded {_allCards.Count} card templates");
    }
    
    /// <summary>
    /// Load NPC conversation deck mappings from npc_conversation_decks.json
    /// </summary>
    private void LoadNPCConversationDecks()
    {
        var filePath = Path.Combine(_contentPath, "npc_conversation_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_conversation_decks.json not found");
            return;
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("conversationDecks", out var decks))
        {
            foreach (var prop in decks.EnumerateObject())
            {
                var npcId = prop.Name;
                var deckData = prop.Value;
                
                if (deckData.TryGetProperty("cards", out var cards))
                {
                    var cardIds = new List<string>();
                    foreach (var card in cards.EnumerateArray())
                    {
                        cardIds.Add(card.GetString());
                    }
                    _npcConversationDecks[npcId] = cardIds;
                }
            }
        }
    }
    
    /// <summary>
    /// Load NPC goal decks from npc_goal_decks.json
    /// </summary>
    private void LoadNPCGoalDecks()
    {
        var filePath = Path.Combine(_contentPath, "npc_goal_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_goal_decks.json not found");
            return;
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("goalDecks", out var decks))
        {
            foreach (var prop in decks.EnumerateObject())
            {
                var npcId = prop.Name;
                var goals = new List<ConversationCard>();
                
                foreach (var goalData in prop.Value.EnumerateArray())
                {
                    var goal = CreateGoalCard(goalData);
                    if (goal != null)
                    {
                        goals.Add(goal);
                        // Also add to global card dictionary
                        _allCards[goal.Id] = goal;
                    }
                }
                
                _npcGoalDecks[npcId] = goals;
            }
        }
    }
    
    /// <summary>
    /// Load NPC exchange decks from npc_exchange_decks.json
    /// </summary>
    private void LoadNPCExchangeDecks()
    {
        var filePath = Path.Combine(_contentPath, "npc_exchange_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_exchange_decks.json not found");
            return;
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("exchangeDecks", out var decks))
        {
            foreach (var prop in decks.EnumerateObject())
            {
                var npcId = prop.Name;
                var exchanges = new List<ConversationCard>();
                
                foreach (var exchangeData in prop.Value.EnumerateArray())
                {
                    var exchange = CreateExchangeCard(exchangeData);
                    if (exchange != null)
                    {
                        exchanges.Add(exchange);
                        // Also add to global card dictionary
                        _allCards[exchange.Id] = exchange;
                    }
                }
                
                _npcExchangeDecks[npcId] = exchanges;
            }
        }
    }
    
    /// <summary>
    /// Load player observation cards from player_observation_cards.json
    /// </summary>
    private void LoadPlayerObservationCards()
    {
        var filePath = Path.Combine(_contentPath, "player_observation_cards.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: player_observation_cards.json not found");
            return;
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("observationCards", out var cards))
        {
            foreach (var cardData in cards.EnumerateArray())
            {
                var card = CreateObservationCard(cardData);
                if (card != null)
                {
                    _playerObservationCards.Add(card);
                    // Also add to global card dictionary
                    _allCards[card.Id] = card;
                }
            }
        }
    }
    
    /// <summary>
    /// Create a ConversationCard from template JSON
    /// </summary>
    private ConversationCard CreateCardFromTemplate(string id, JsonElement template)
    {
        try
        {
            // Parse card type
            var typeStr = template.GetProperty("type").GetString();

            // Parse connection type
            var hasCardType = template.TryGetProperty("cardType", out var cardTypeElem);
            var connectionStr = template.TryGetProperty("connectionType", out var connElem) 
                ? connElem.GetString() : "Trust";
            var cardType = hasCardType ? ParseCardType(cardTypeElem.GetString()) : CardType.Normal;
            var cardTokenType = ParseCardTokenType(connectionStr);
            
            // Parse persistence
            var persistenceStr = template.TryGetProperty("persistence", out var persElem) 
                ? persElem.GetString() : "Persistent";
            var persistence = ParsePersistence(persistenceStr);
            
            // Get numeric values
            var weight = template.TryGetProperty("weight", out var weightElem) ? weightElem.GetInt32() : 1;
            var baseComfort = template.TryGetProperty("baseComfort", out var comfortElem) ? comfortElem.GetInt32() : 0;
            var depth = template.TryGetProperty("depth", out var depthElem) ? depthElem.GetInt32() : 0;
            
            // Get description
            var description = template.TryGetProperty("description", out var descElem) ? descElem.GetString() : "";
            
            // Check for state card properties
            EmotionalState? successState = null;
            if (template.TryGetProperty("successState", out var successStateElem))
            {
                var stateStr = successStateElem.GetString();
                if (Enum.TryParse<EmotionalState>(stateStr, true, out var state))
                {
                    successState = state;
                }
            }
            
            var isStateCard = template.TryGetProperty("isStateCard", out var stateCardElem) && stateCardElem.GetBoolean();
            
            
            // Parse template type
            var templateType = ParseTemplateType(id);
            
            // Parse category if specified
            CardCategory category = CardCategory.Comfort; // Default
            if (template.TryGetProperty("category", out var categoryElem))
            {
                var categoryStr = categoryElem.GetString();
                if (Enum.TryParse<CardCategory>(categoryStr, true, out var parsedCategory))
                {
                    category = parsedCategory;
                }
            }
            else if (isStateCard)
            {
                category = CardCategory.State;
            }
            
            // Parse patience bonus for patience cards
            var patienceBonus = 0;
            if (template.TryGetProperty("patienceBonus", out var patienceElem))
            {
                patienceBonus = patienceElem.GetInt32();
            }
            
            return new ConversationCard
            {
                Id = id,
                TemplateId = templateType,
                Mechanics = CardMechanicsType.Standard,
                Category = category.ToString(),
                Context = new CardContext
                {
                    Personality = PersonalityType.STEADFAST, // Default, will be overridden per NPC
                    EmotionalState = EmotionalState.NEUTRAL,
                    NPCName = "",
                    GeneratesLetterOnSuccess = template.TryGetProperty("generatesLetter", out var letterElem) && letterElem.GetBoolean()
                },
                Type = cardType,
                TokenType = cardTokenType,
                Persistence = persistence,
                Weight = weight,
                BaseComfort = baseComfort,
                IsStateCard = isStateCard,
                SuccessState = successState,
                PatienceBonus = patienceBonus,
                DisplayName = template.TryGetProperty("displayName", out var displayNameElem) 
                    ? displayNameElem.GetString() 
                    : id.Replace("_", " "), // Fallback to formatted ID
                Description = description,
                SuccessRate = 70 - (weight * 10) // Base success rate calculation
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating card from template {id}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Create a goal card from JSON
    /// </summary>
    private ConversationCard CreateGoalCard(JsonElement goalData)
    {
        try
        {
            var id = goalData.GetProperty("id").GetString();
            var template = goalData.GetProperty("template").GetString();
            var type = goalData.GetProperty("type").GetString();
            var category = goalData.GetProperty("category").GetString();
            var weight = goalData.GetProperty("weight").GetInt32();
            
            // Get valid states
            var validStates = new List<EmotionalState>();
            if (goalData.TryGetProperty("validStates", out var states))
            {
                foreach (var state in states.EnumerateArray())
                {
                    var stateStr = state.GetString();
                    if (stateStr == "ANY")
                    {
                        // Add all states
                        foreach (EmotionalState s in Enum.GetValues<EmotionalState>())
                        {
                            validStates.Add(s);
                        }
                    }
                    else if (Enum.TryParse<EmotionalState>(stateStr, true, out var emotionalState))
                    {
                        validStates.Add(emotionalState);
                    }
                }
            }
            
            // Determine goal type
            ConversationType goalType = template switch
            {
                "LetterGoal" => ConversationType.Promise,
                "ResolutionGoal" => ConversationType.Resolution,
                "PromiseGoal" => ConversationType.Promise,
                "CommerceGoal" => ConversationType.Commerce,
                _ => ConversationType.Promise
            };
            
            // Parse category from JSON or default based on goal type
            CardCategory parsedCategory = CardCategory.Promise; // Default for goals
            if (category != null && Enum.TryParse<CardCategory>(category, true, out var cat))
            {
                parsedCategory = cat;
            }
            
            return new ConversationCard
            {
                Id = id,
                TemplateId = "GoalCard",
                Mechanics = CardMechanicsType.Promise,
                Category = parsedCategory.ToString(),
                Context = new CardContext
                {
                    Personality = PersonalityType.STEADFAST,
                    EmotionalState = EmotionalState.NEUTRAL,
                    NPCName = "",
                    ValidStates = validStates
                },
                Type = ParseCardType(type),
                Persistence = PersistenceType.Persistent,
                Weight = weight,
                BaseComfort = 0,
                IsGoalCard = true,
                GoalCardType = goalType.ToString(),
                DisplayName = goalData.TryGetProperty("displayName", out var displayNameElem) 
                    ? displayNameElem.GetString() 
                    : id.Replace("_", " "), // Fallback to formatted ID
                Description = GetGoalDescription(goalData),
                SuccessRate = 50 // Goal cards have base 50% success
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating goal card: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Create an exchange card from JSON
    /// </summary>
    private ConversationCard CreateExchangeCard(JsonElement exchangeData)
    {
        try
        {
            var id = exchangeData.GetProperty("id").GetString();
            var type = exchangeData.GetProperty("type").GetString();
            
            // Get offer and request
            var offer = exchangeData.GetProperty("offer");
            var request = exchangeData.GetProperty("request");
            
            var offerResource = offer.GetProperty("resource").GetString();
            var offerAmount = offer.GetProperty("amount").GetInt32();
            var requestResource = request.GetProperty("resource").GetString();
            var requestAmount = request.GetProperty("amount").GetInt32();
            
            var description = $"Trade {requestAmount} {requestResource} for {offerAmount} {offerResource}";
            
            // Create ResourceExchange objects for Cost and Reward
            var costResourceType = ParseResourceType(requestResource);
            var rewardResourceType = ParseResourceType(offerResource);
            
            var costExchange = new ResourceExchange
            {
                ResourceType = costResourceType,
                Amount = requestAmount,
                IsAbsolute = false
            };
            
            var rewardExchange = new ResourceExchange
            {
                ResourceType = rewardResourceType,
                Amount = offerAmount,
                IsAbsolute = false
            };
            
            // Create ExchangeData with proper Cost and Reward
            var exchangeDataObj = new ExchangeData
            {
                ExchangeName = id.Replace("_", " "),
                NPCPersonality = PersonalityType.MERCANTILE,
                Cost = new Dictionary<ResourceType, int> { { costExchange.ResourceType, costExchange.Amount } },
                Reward = new Dictionary<ResourceType, int> { { rewardExchange.ResourceType, rewardExchange.Amount } },
                BaseSuccessRate = 100,
                CanBarter = false,
                TemplateId = "SimpleExchange"
            };
            
            return new ConversationCard
            {
                Id = id,
                TemplateId = "SimpleExchange",
                Mechanics = CardMechanicsType.Exchange,
                Context = new CardContext
                {
                    Personality = PersonalityType.MERCANTILE,
                    EmotionalState = EmotionalState.NEUTRAL,
                    NPCName = "",
                    ExchangeOffer = new ExchangeOffer 
                    {
                        Id = id,
                        Name = $"{offerAmount} {offerResource}",
                        Cost = new Dictionary<ResourceType, int> { { costResourceType, requestAmount } },
                        Reward = new Dictionary<ResourceType, int> { { rewardResourceType, offerAmount } }
                    },
                    ExchangeRequest = $"{requestAmount} {requestResource}",
                    // Set the ExchangeData properly
                    ExchangeData = exchangeDataObj,
                },
                Type = CardType.Normal,
                Persistence = PersistenceType.Fleeting,
                Weight = 0, // Exchange cards have no weight
                BaseComfort = 0,
                DisplayName = exchangeData.TryGetProperty("displayName", out var displayNameElem) 
                    ? displayNameElem.GetString() 
                    : id.Replace("_", " "), // Fallback to formatted ID
                Description = description,
                SuccessRate = 100, // Exchange cards always succeed if affordable
                IsExchange = true, // Mark as exchange card
                Category = CardCategory.Exchange.ToString() // Set the category
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating exchange card: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Create an observation card from JSON
    /// </summary>
    private ConversationCard CreateObservationCard(JsonElement cardData)
    {
        try
        {
            var id = cardData.GetProperty("id").GetString();
            var location = cardData.GetProperty("location").GetString();
            var spot = cardData.TryGetProperty("spot", out var spotElem) ? spotElem.GetString() : "";
            var description = cardData.GetProperty("description").GetString();
            
            // Parse state change if present
            EmotionalState? targetState = null;
            if (cardData.TryGetProperty("createsState", out var stateElem))
            {
                if (Enum.TryParse<EmotionalState>(stateElem.GetString(), true, out var state))
                {
                    targetState = state;
                }
            }
            
            return new ConversationCard
            {
                Id = id,
                TemplateId = "ObservationShare",
                Mechanics = CardMechanicsType.Standard,
                Context = new CardContext
                {
                    Personality = PersonalityType.STEADFAST,
                    EmotionalState = EmotionalState.NEUTRAL,
                    NPCName = "",
                    ObservationLocation = location,
                    ObservationSpot = spot
                },
                Type = CardType.Observation,
                Persistence = PersistenceType.Fleeting,
                Weight = 1,
                BaseComfort = 2,
                IsObservation = true,
                ObservationSource = $"{location}/{spot}",
                SuccessState = targetState,
                IsStateCard = targetState.HasValue,
                DisplayName = description,
                Description = description,
                SuccessRate = 85 // Observation cards have high success rate
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating observation card: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Load travel cards from travel_cards.json
    /// </summary>
    private void LoadTravelCards()
    {
        _travelCards = _travelCardLoader.LoadTravelCards();
    }
    
    // Helper methods for parsing enums
    
    private CardType ParseCardType(string connectionStr)
    {
        return connectionStr?.ToLower() switch
        {
            "normal" => CardType.Normal,
            "observation" => CardType.Observation,
            "goal" => CardType.Goal,
            _ => CardType.Normal
        };
    }

    private ConnectionType ParseCardTokenType(string connectionStr)
    {
        return connectionStr?.ToLower() switch
        {
            "trust" => ConnectionType.Trust,
            "commerce" => ConnectionType.Commerce,
            "status" => ConnectionType.Status,
            "shadow" => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }
    
    private PersistenceType ParsePersistence(string persistenceStr)
    {
        return persistenceStr switch
        {
            "Persistent" => PersistenceType.Persistent,
            "Fleeting" => PersistenceType.Fleeting,
            "Opportunity" => PersistenceType.Fleeting, // Map old Opportunity to Fleeting
            "Burden" => PersistenceType.Persistent,
            _ => PersistenceType.Persistent
        };
    }
    
    private string ParseTemplateType(string id)
    {
        // Just return the ID as the template - no mapping needed
        return id;
    }
    
    private string GetGoalDescription(JsonElement goalData)
    {
        if (goalData.TryGetProperty("letterContent", out var letter))
        {
            var subject = letter.GetProperty("subject").GetString();
            var recipient = letter.GetProperty("recipient").GetString();
            return $"{subject} to {recipient}";
        }
        
        if (goalData.TryGetProperty("requirement", out var req))
        {
            return req.GetString();
        }
        
        return "Complete goal";
    }
    
    private ResourceType ParseResourceType(string resourceStr)
    {
        return resourceStr?.ToLower() switch
        {
            "coins" => ResourceType.Coins,
            "health" => ResourceType.Health,
            "food" => ResourceType.Hunger, // "food" in JSON maps to Hunger resource
            "hunger" => ResourceType.Hunger,
            "attention" => ResourceType.Attention,
            "trust" => ResourceType.TrustToken,
            "commerce" => ResourceType.CommerceToken,
            "status" => ResourceType.StatusToken,
            "shadow" => ResourceType.ShadowToken,
            _ => ResourceType.Coins // Default to coins if unknown
        };
    }
    
    // Public accessors for loaded data
    public Dictionary<string, ConversationCard> GetAllCards() => new(_allCards);
    public Dictionary<string, List<string>> GetNPCConversationDecks() => new(_npcConversationDecks);
    public Dictionary<string, List<ConversationCard>> GetNPCGoalDecks() => new(_npcGoalDecks);
    public Dictionary<string, List<ConversationCard>> GetNPCExchangeDecks() => new(_npcExchangeDecks);
    public List<ConversationCard> GetPlayerObservationCards() => new(_playerObservationCards);
    public List<TravelCard> GetTravelCards() => new(_travelCards);
    
    /// <summary>
    /// Get conversation cards for a specific NPC
    /// </summary>
    public List<ConversationCard> GetConversationCardsForNPC(string npcId)
    {
        var cards = new List<ConversationCard>();
        
        if (_npcConversationDecks.TryGetValue(npcId, out var cardIds))
        {
            foreach (var cardId in cardIds)
            {
                if (_allCards.TryGetValue(cardId, out var card))
                {
                    cards.Add(card);
                }
                else
                {
                    Console.WriteLine($"Warning: Card {cardId} not found for NPC {npcId}");
                }
            }
        }
        
        return cards;
    }
    
    /// <summary>
    /// Get goal cards for a specific NPC
    /// </summary>
    public List<ConversationCard> GetGoalCardsForNPC(string npcId)
    {
        if (_npcGoalDecks.TryGetValue(npcId, out var cards))
        {
            return new List<ConversationCard>(cards);
        }
        return new List<ConversationCard>();
    }
    
    /// <summary>
    /// Get exchange cards for a specific NPC
    /// </summary>
    public List<ConversationCard> GetExchangeCardsForNPC(string npcId)
    {
        if (_npcExchangeDecks.TryGetValue(npcId, out var cards))
        {
            return new List<ConversationCard>(cards);
        }
        return new List<ConversationCard>();
    }
}