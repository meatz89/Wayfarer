using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Central database for all card definitions loaded from JSON files
/// This is the single source of truth for all card data
/// </summary>
public class CardDatabase
{
    // Base deck categories
    public Dictionary<string, List<ConversationCard>> BaseDeck { get; set; }
    
    // NPC-specific special cards
    public Dictionary<string, List<ConversationCard>> NPCSpecialCards { get; set; }
    
    // Location-based observation cards
    public Dictionary<string, Dictionary<string, List<ObservationCard>>> LocationObservations { get; set; }
    
    // NPC goal decks
    public Dictionary<string, List<GoalCard>> NPCGoals { get; set; }
    
    // Track all cards for validation
    private List<ConversationCard> allConversationCards;
    private List<ObservationCard> allObservationCards;
    private List<GoalCard> allGoalCards;
    
    public CardDatabase()
    {
        BaseDeck = new Dictionary<string, List<ConversationCard>>();
        NPCSpecialCards = new Dictionary<string, List<ConversationCard>>();
        LocationObservations = new Dictionary<string, Dictionary<string, List<ObservationCard>>>();
        NPCGoals = new Dictionary<string, List<GoalCard>>();
        
        allConversationCards = new List<ConversationCard>();
        allObservationCards = new List<ObservationCard>();
        allGoalCards = new List<GoalCard>();
    }
    
    /// <summary>
    /// Load all card data from JSON files
    /// </summary>
    public static CardDatabase LoadFromJson(string contentPath)
    {
        var database = new CardDatabase();
        
        // Load cards.json (conversation cards)
        string cardsPath = Path.Combine(contentPath, "cards.json");
        if (File.Exists(cardsPath))
        {
            string cardsJson = File.ReadAllText(cardsPath);
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var cardsDoc = JsonDocument.Parse(cardsJson, options);
            database.LoadConversationCards(cardsDoc.RootElement);
        }
        
        // Load observations.json
        string obsPath = Path.Combine(contentPath, "observations.json");
        if (File.Exists(obsPath))
        {
            string obsJson = File.ReadAllText(obsPath);
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var obsDoc = JsonDocument.Parse(obsJson, options);
            database.LoadObservationCards(obsDoc.RootElement);
        }
        
        // Load goals.json
        string goalsPath = Path.Combine(contentPath, "goals.json");
        if (File.Exists(goalsPath))
        {
            string goalsJson = File.ReadAllText(goalsPath);
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var goalsDoc = JsonDocument.Parse(goalsJson, options);
            database.LoadGoalCards(goalsDoc.RootElement);
        }
        
        // Validate all cards
        database.ValidateAllCards();
        
        return database;
    }
    
    private void LoadConversationCards(JsonElement root)
    {
        // Load base deck categories
        if (root.TryGetProperty("baseDeck", out JsonElement baseDeckElement))
        {
            foreach (var category in baseDeckElement.EnumerateObject())
            {
                var categoryCards = new List<ConversationCard>();
                foreach (var cardElement in category.Value.EnumerateArray())
                {
                    var card = ParseConversationCard(cardElement);
                    categoryCards.Add(card);
                    allConversationCards.Add(card);
                }
                BaseDeck[category.Name] = categoryCards;
            }
        }
        
        // Load NPC special cards
        if (root.TryGetProperty("npcSpecialCards", out JsonElement npcCardsElement))
        {
            foreach (var npc in npcCardsElement.EnumerateObject())
            {
                var npcCards = new List<ConversationCard>();
                foreach (var cardElement in npc.Value.EnumerateArray())
                {
                    var card = ParseConversationCard(cardElement);
                    npcCards.Add(card);
                    allConversationCards.Add(card);
                }
                NPCSpecialCards[npc.Name] = npcCards;
            }
        }
    }
    
    private ConversationCard ParseConversationCard(JsonElement element)
    {
        var card = new ConversationCard
        {
            Id = element.GetProperty("id").GetString(),
            Name = element.GetProperty("name").GetString(),
            Description = element.GetProperty("description").GetString(),
            Weight = element.GetProperty("weight").GetInt32(),
            DialogueFragment = element.GetProperty("dialogueFragment").GetString()
        };
        
        // Parse token type
        string tokenTypeStr = element.GetProperty("tokenType").GetString();
        card.TokenType = Enum.Parse<TokenType>(tokenTypeStr, true);
        card.ConnectionType = ConversationCard.ConvertTokenToConnection(card.TokenType);
        
        // Parse difficulty
        string difficultyStr = element.GetProperty("difficulty").GetString();
        var difficulty = Enum.Parse<Difficulty>(difficultyStr, true);
        card.Difficulty = difficulty;
        card.Difficulty_Legacy = difficulty;
        
        // Parse effect type
        string effectTypeStr = element.GetProperty("effectType").GetString();
        card.EffectType = Enum.Parse<CardEffectType>(effectTypeStr, true);
        
        // Parse effect value or scaling formula
        if (element.TryGetProperty("effectValue", out JsonElement effectValueElement))
        {
            card.EffectValue = effectValueElement.GetInt32();
        }
        
        if (element.TryGetProperty("scalingFormula", out JsonElement scalingFormulaElement))
        {
            card.ScalingFormula = scalingFormulaElement.GetString();
            card.EffectFormula = card.ScalingFormula;
        }
        
        // Parse atmosphere change
        if (element.TryGetProperty("atmosphereChange", out JsonElement atmosphereElement))
        {
            string atmosphereStr = atmosphereElement.GetString();
            card.AtmosphereChange = Enum.Parse<AtmosphereType>(atmosphereStr, true);
        }
        
        // Parse persistence
        if (element.TryGetProperty("persistent", out JsonElement persistentElement))
        {
            card.IsFleeting = !persistentElement.GetBoolean();
            card.Persistence = persistentElement.GetBoolean() ? 
                PersistenceType.Persistent : PersistenceType.Fleeting;
        }
        
        // Set default card type
        card.Type = CardType.Normal;
        
        return card;
    }
    
    private void LoadObservationCards(JsonElement root)
    {
        if (!root.TryGetProperty("locationObservations", out JsonElement locationsElement))
            return;
        
        foreach (var location in locationsElement.EnumerateObject())
        {
            var locationDict = new Dictionary<string, List<ObservationCard>>();
            
            foreach (var context in location.Value.EnumerateObject())
            {
                var contextCards = new List<ObservationCard>();
                
                foreach (var cardElement in context.Value.EnumerateArray())
                {
                    var card = ParseObservationCard(cardElement, location.Name, context.Name);
                    contextCards.Add(card);
                    allObservationCards.Add(card);
                }
                
                locationDict[context.Name] = contextCards;
            }
            
            LocationObservations[location.Name] = locationDict;
        }
    }
    
    private ObservationCard ParseObservationCard(JsonElement element, string location, string context)
    {
        var card = new ObservationCard
        {
            Id = element.GetProperty("id").GetString(),
            Name = element.GetProperty("name").GetString(),
            Description = element.GetProperty("description").GetString(),
            LocationDiscovered = location,
            ObservationId = element.GetProperty("id").GetString(),
            DialogueFragment = element.GetProperty("dialogueFragment").GetString()
        };
        
        // Parse unique effect
        string effectStr = element.GetProperty("uniqueEffect").GetString();
        card.UniqueEffect = Enum.Parse<ObservationEffectType>(effectStr, true);
        
        // Parse expiration
        card.ExpirationHours = element.GetProperty("expirationHours").GetInt32();
        card.SetExpiration(card.ExpirationHours);
        
        // Parse token type if present
        if (element.TryGetProperty("tokenType", out JsonElement tokenElement))
        {
            string tokenTypeStr = tokenElement.GetString();
            card.TokenType = Enum.Parse<TokenType>(tokenTypeStr, true);
        }
        
        // Observation cards have fixed properties
        card.Weight = element.TryGetProperty("weight", out JsonElement weightElement) ? 
            weightElement.GetInt32() : 1;
        card.Difficulty = element.TryGetProperty("difficulty", out JsonElement diffElement) ? 
            Enum.Parse<Difficulty>(diffElement.GetString(), true) : Difficulty.VeryEasy;
        
        return card;
    }
    
    private void LoadGoalCards(JsonElement root)
    {
        if (!root.TryGetProperty("npcGoals", out JsonElement npcGoalsElement))
            return;
        
        foreach (var npc in npcGoalsElement.EnumerateObject())
        {
            var npcGoalList = new List<GoalCard>();
            
            foreach (var goalElement in npc.Value.EnumerateArray())
            {
                var goal = ParseGoalCard(goalElement, npc.Name);
                npcGoalList.Add(goal);
                allGoalCards.Add(goal);
            }
            
            NPCGoals[npc.Name] = npcGoalList;
        }
    }
    
    private GoalCard ParseGoalCard(JsonElement element, string npcId)
    {
        var card = new GoalCard
        {
            Id = element.GetProperty("id").GetString(),
            Name = element.GetProperty("name").GetString(),
            Description = element.GetProperty("description").GetString(),
            DialogueFragment = element.GetProperty("dialogueFragment").GetString(),
            GoalContext = $"Goal for {npcId}"
        };
        
        // Parse goal type
        string goalTypeStr = element.GetProperty("goalType").GetString();
        card.GoalType = Enum.Parse<GoalType>(goalTypeStr, true);
        
        // Parse token type
        string tokenTypeStr = element.GetProperty("tokenType").GetString();
        card.TokenType = Enum.Parse<TokenType>(tokenTypeStr, true);
        card.ConnectionType = ConversationCard.ConvertTokenToConnection(card.TokenType);
        
        // Parse weight
        int weight = element.GetProperty("weight").GetInt32();
        card.SetWeight(weight);
        
        // Parse creates obligation
        string obligationStr = element.GetProperty("createsObligation").GetString();
        card.CreatesObligation = Enum.Parse<ObligationType>(obligationStr, true);
        
        // Parse success terms
        if (element.TryGetProperty("successTerms", out JsonElement successElement))
        {
            card.SuccessTerms = new SuccessTerms
            {
                DeadlineHours = successElement.GetProperty("deadlineHours").GetInt32(),
                QueuePosition = successElement.GetProperty("queuePosition").GetInt32(),
                Payment = successElement.GetProperty("payment").GetInt32()
            };
            
            if (successElement.TryGetProperty("destinationLocation", out JsonElement destElement))
                card.SuccessTerms.DestinationLocation = destElement.GetString();
            
            if (successElement.TryGetProperty("requiredNpc", out JsonElement npcElement))
                card.SuccessTerms.RequiredNpc = npcElement.GetString();
            
            if (successElement.TryGetProperty("burdenToRemove", out JsonElement burdenElement))
                card.SuccessTerms.BurdenToRemove = burdenElement.GetString();
        }
        
        // Parse failure terms
        if (element.TryGetProperty("failureTerms", out JsonElement failureElement))
        {
            card.FailureTerms = new FailureTerms
            {
                DeadlineHours = failureElement.GetProperty("deadlineHours").GetInt32(),
                QueuePosition = failureElement.GetProperty("queuePosition").GetInt32(),
                Payment = failureElement.GetProperty("payment").GetInt32()
            };
        }
        
        // Parse required token
        if (element.TryGetProperty("requiredToken", out JsonElement reqTokenElement))
        {
            card.RequiresToken = true;
            card.RequiredToken = Enum.Parse<TokenType>(reqTokenElement.GetString(), true);
        }
        
        return card;
    }
    
    private void ValidateAllCards()
    {
        // Validate conversation cards
        foreach (var card in allConversationCards)
        {
            if (!card.HasSingleEffect() && card.EffectType != CardEffectType.SetAtmosphere)
            {
                throw new Exception($"Card {card.Id} has multiple effects!");
            }
            
            if (string.IsNullOrEmpty(card.Id))
            {
                throw new Exception("Card missing ID!");
            }
            
            if (string.IsNullOrEmpty(card.Name))
            {
                throw new Exception($"Card {card.Id} missing name!");
            }
        }
        
        // Validate observation cards
        foreach (var card in allObservationCards)
        {
            if (card.Weight != 1)
            {
                throw new Exception($"Observation card {card.Id} must have weight 1!");
            }
            
            if (card.IsFleeting)
            {
                throw new Exception($"Observation card {card.Id} must be persistent!");
            }
        }
        
        // Validate goal cards
        foreach (var card in allGoalCards)
        {
            if (card.Weight < 5 || card.Weight > 6)
            {
                throw new Exception($"Goal card {card.Id} must have weight 5 or 6!");
            }
            
            if (!card.IsFleeting)
            {
                throw new Exception($"Goal card {card.Id} must be fleeting!");
            }
            
            if (!card.HasFinalWord)
            {
                throw new Exception($"Goal card {card.Id} must have final word!");
            }
        }
        
        Console.WriteLine($"CardDatabase validated: {allConversationCards.Count} conversation cards, " +
                         $"{allObservationCards.Count} observation cards, {allGoalCards.Count} goal cards");
    }
    
    /// <summary>
    /// Get all base deck cards for deck building
    /// </summary>
    public List<ConversationCard> GetAllBaseDeckCards()
    {
        var cards = new List<ConversationCard>();
        foreach (var category in BaseDeck.Values)
        {
            cards.AddRange(category);
        }
        return cards;
    }
    
    /// <summary>
    /// Get cards by category
    /// </summary>
    public List<ConversationCard> GetCardsByCategory(string category)
    {
        return BaseDeck.ContainsKey(category) ? BaseDeck[category] : new List<ConversationCard>();
    }
    
    /// <summary>
    /// Get NPC special cards
    /// </summary>
    public List<ConversationCard> GetNPCSpecialCards(string npcId)
    {
        return NPCSpecialCards.ContainsKey(npcId) ? NPCSpecialCards[npcId] : new List<ConversationCard>();
    }
    
    /// <summary>
    /// Get observation cards for a location
    /// </summary>
    public List<ObservationCard> GetLocationObservations(string locationId, string context = null)
    {
        if (!LocationObservations.ContainsKey(locationId))
            return new List<ObservationCard>();
        
        var locationObs = LocationObservations[locationId];
        
        if (string.IsNullOrEmpty(context))
        {
            // Return all observations for location
            var all = new List<ObservationCard>();
            foreach (var contextCards in locationObs.Values)
            {
                all.AddRange(contextCards);
            }
            return all;
        }
        
        return locationObs.ContainsKey(context) ? locationObs[context] : new List<ObservationCard>();
    }
    
    /// <summary>
    /// Get goal cards for an NPC
    /// </summary>
    public List<GoalCard> GetNPCGoals(string npcId)
    {
        return NPCGoals.ContainsKey(npcId) ? NPCGoals[npcId] : new List<GoalCard>();
    }
}