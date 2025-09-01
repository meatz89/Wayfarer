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
        string filePath = Path.Combine(_contentPath, "card_templates.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: card_templates.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        if (data.TryGetProperty("templates", out JsonElement templates))
        {
            foreach (JsonProperty prop in templates.EnumerateObject())
            {
                string cardId = prop.Name;
                JsonElement template = prop.Value;

                ConversationCard card = CreateCardFromTemplate(cardId, template);
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
        string filePath = Path.Combine(_contentPath, "npc_conversation_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_conversation_decks.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        if (data.TryGetProperty("conversationDecks", out JsonElement decks))
        {
            foreach (JsonProperty prop in decks.EnumerateObject())
            {
                string npcId = prop.Name;
                JsonElement deckData = prop.Value;

                if (deckData.TryGetProperty("cards", out JsonElement cards))
                {
                    List<string> cardIds = new List<string>();
                    foreach (JsonElement card in cards.EnumerateArray())
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
        string filePath = Path.Combine(_contentPath, "npc_goal_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_goal_decks.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        if (data.TryGetProperty("goalDecks", out JsonElement decks))
        {
            foreach (JsonProperty prop in decks.EnumerateObject())
            {
                string npcId = prop.Name;
                List<ConversationCard> goals = new List<ConversationCard>();

                foreach (JsonElement goalData in prop.Value.EnumerateArray())
                {
                    ConversationCard goal = CreateGoalCard(goalData);
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
        string filePath = Path.Combine(_contentPath, "npc_exchange_decks.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: npc_exchange_decks.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        if (data.TryGetProperty("exchangeDecks", out JsonElement decks))
        {
            foreach (JsonProperty prop in decks.EnumerateObject())
            {
                string npcId = prop.Name;
                List<ConversationCard> exchanges = new List<ConversationCard>();

                foreach (JsonElement exchangeData in prop.Value.EnumerateArray())
                {
                    ConversationCard exchange = CreateExchangeCard(exchangeData);
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
        string filePath = Path.Combine(_contentPath, "player_observation_cards.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: player_observation_cards.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        if (data.TryGetProperty("observationCards", out JsonElement cards))
        {
            foreach (JsonElement cardData in cards.EnumerateArray())
            {
                ConversationCard card = CreateObservationCard(cardData);
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
            string? typeStr = template.GetProperty("type").GetString();

            // Parse connection type
            bool hasCardType = template.TryGetProperty("cardType", out JsonElement cardTypeElem);
            string? connectionStr = template.TryGetProperty("connectionType", out JsonElement connElem)
                ? connElem.GetString() : "Trust";
            CardType cardType = hasCardType ? ParseCardType(cardTypeElem.GetString()) : CardType.Normal;
            ConnectionType cardTokenType = ParseCardTokenType(connectionStr);

            // Parse persistence
            string? persistenceStr = template.TryGetProperty("persistence", out JsonElement persElem)
                ? persElem.GetString() : "Persistent";
            PersistenceType persistence = ParsePersistence(persistenceStr);

            // Get numeric values
            int weight = template.TryGetProperty("weight", out JsonElement weightElem) ? weightElem.GetInt32() : 1;
            int baseComfort = template.TryGetProperty("baseComfort", out JsonElement comfortElem) ? comfortElem.GetInt32() : 0;
            int depth = template.TryGetProperty("depth", out JsonElement depthElem) ? depthElem.GetInt32() : 0;

            // Get description
            string? description = template.TryGetProperty("description", out JsonElement descElem) ? descElem.GetString() : "";

            // Check for state card properties
            EmotionalState? successState = null;
            if (template.TryGetProperty("successState", out JsonElement successStateElem))
            {
                string? stateStr = successStateElem.GetString();
                if (Enum.TryParse<EmotionalState>(stateStr, true, out EmotionalState state))
                {
                    successState = state;
                }
            }

            bool isStateCard = template.TryGetProperty("isStateCard", out JsonElement stateCardElem) && stateCardElem.GetBoolean();


            // Parse template type
            string templateType = ParseTemplateType(id);

            // Parse category if specified
            CardCategory category = CardCategory.Comfort; // Default
            if (template.TryGetProperty("category", out JsonElement categoryElem))
            {
                string? categoryStr = categoryElem.GetString();
                if (Enum.TryParse<CardCategory>(categoryStr, true, out CardCategory parsedCategory))
                {
                    category = parsedCategory;
                }
            }
            else if (isStateCard)
            {
                category = CardCategory.State;
            }

            // Parse patience bonus for patience cards
            int patienceBonus = 0;
            if (template.TryGetProperty("patienceBonus", out JsonElement patienceElem))
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
                    GeneratesLetterOnSuccess = template.TryGetProperty("generatesLetter", out JsonElement letterElem) && letterElem.GetBoolean()
                },
                Type = cardType,
                TokenType = cardTokenType,
                Persistence = persistence,
                Weight = weight,
                BaseComfort = baseComfort,
                IsStateCard = isStateCard,
                SuccessState = successState,
                PatienceBonus = patienceBonus,
                DisplayName = template.TryGetProperty("displayName", out JsonElement displayNameElem)
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
            string? id = goalData.GetProperty("id").GetString();
            string? template = goalData.GetProperty("template").GetString();
            string? type = goalData.GetProperty("type").GetString();
            string? category = goalData.GetProperty("category").GetString();
            int weight = goalData.GetProperty("weight").GetInt32();

            // Get valid states
            List<EmotionalState> validStates = new List<EmotionalState>();
            if (goalData.TryGetProperty("validStates", out JsonElement states))
            {
                foreach (JsonElement state in states.EnumerateArray())
                {
                    string? stateStr = state.GetString();
                    if (stateStr == "ANY")
                    {
                        // Add all states
                        foreach (EmotionalState s in Enum.GetValues<EmotionalState>())
                        {
                            validStates.Add(s);
                        }
                    }
                    else if (Enum.TryParse<EmotionalState>(stateStr, true, out EmotionalState emotionalState))
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
            if (category != null && Enum.TryParse<CardCategory>(category, true, out CardCategory cat))
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
                DisplayName = goalData.TryGetProperty("displayName", out JsonElement displayNameElem)
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
            string? id = exchangeData.GetProperty("id").GetString();
            string? type = exchangeData.GetProperty("type").GetString();

            // Get offer and request
            JsonElement offer = exchangeData.GetProperty("offer");
            JsonElement request = exchangeData.GetProperty("request");

            string? offerResource = offer.GetProperty("resource").GetString();
            int offerAmount = offer.GetProperty("amount").GetInt32();
            string? requestResource = request.GetProperty("resource").GetString();
            int requestAmount = request.GetProperty("amount").GetInt32();

            string description = $"Trade {requestAmount} {requestResource} for {offerAmount} {offerResource}";

            // Create ResourceExchange objects for Cost and Reward
            ResourceType costResourceType = ParseResourceType(requestResource);
            ResourceType rewardResourceType = ParseResourceType(offerResource);

            ResourceExchange costExchange = new ResourceExchange
            {
                ResourceType = costResourceType,
                Amount = requestAmount,
                IsAbsolute = false
            };

            ResourceExchange rewardExchange = new ResourceExchange
            {
                ResourceType = rewardResourceType,
                Amount = offerAmount,
                IsAbsolute = false
            };

            // Create ExchangeData with proper Cost and Reward
            ExchangeData exchangeDataObj = new ExchangeData
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
                DisplayName = exchangeData.TryGetProperty("displayName", out JsonElement displayNameElem)
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
            string? id = cardData.GetProperty("id").GetString();
            string? location = cardData.GetProperty("location").GetString();
            string? spot = cardData.TryGetProperty("spot", out JsonElement spotElem) ? spotElem.GetString() : "";
            string? description = cardData.GetProperty("description").GetString();

            // Parse state change if present
            EmotionalState? targetState = null;
            if (cardData.TryGetProperty("createsState", out JsonElement stateElem))
            {
                if (Enum.TryParse<EmotionalState>(stateElem.GetString(), true, out EmotionalState state))
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
        if (goalData.TryGetProperty("letterContent", out JsonElement letter))
        {
            string? subject = letter.GetProperty("subject").GetString();
            string? recipient = letter.GetProperty("recipient").GetString();
            return $"{subject} to {recipient}";
        }

        if (goalData.TryGetProperty("requirement", out JsonElement req))
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
    public Dictionary<string, ConversationCard> GetAllCards()
    {
        return new(_allCards);
    }

    public Dictionary<string, List<string>> GetNPCConversationDecks()
    {
        return new(_npcConversationDecks);
    }

    public Dictionary<string, List<ConversationCard>> GetNPCGoalDecks()
    {
        return new(_npcGoalDecks);
    }

    public Dictionary<string, List<ConversationCard>> GetNPCExchangeDecks()
    {
        return new(_npcExchangeDecks);
    }

    public List<ConversationCard> GetPlayerObservationCards()
    {
        return new(_playerObservationCards);
    }

    public List<TravelCard> GetTravelCards()
    {
        return new(_travelCards);
    }

    /// <summary>
    /// Get conversation cards for a specific NPC
    /// </summary>
    public List<ConversationCard> GetConversationCardsForNPC(string npcId)
    {
        List<ConversationCard> cards = new List<ConversationCard>();

        if (_npcConversationDecks.TryGetValue(npcId, out List<string>? cardIds))
        {
            foreach (string cardId in cardIds)
            {
                if (_allCards.TryGetValue(cardId, out ConversationCard? card))
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
        if (_npcGoalDecks.TryGetValue(npcId, out List<ConversationCard>? cards))
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
        if (_npcExchangeDecks.TryGetValue(npcId, out List<ConversationCard>? cards))
        {
            return new List<ConversationCard>(cards);
        }
        return new List<ConversationCard>();
    }
}