using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Initializes NPC conversation decks from the CardDatabase
/// Called ONCE at game initialization to create all NPC decks
/// </summary>
public class DeckInitializer
{
    private readonly CardDatabase cardDatabase;
    private readonly Random random;
    
    public DeckInitializer(CardDatabase database)
    {
        this.cardDatabase = database;
        this.random = new Random();
    }
    
    /// <summary>
    /// Initialize all NPC decks at game start
    /// Each NPC gets an independent deck instance with cards matching their personality
    /// </summary>
    public void InitializeAllNPCDecks(GameWorld gameWorld)
    {
        foreach (var npc in gameWorld.NPCs)
        {
            // Create conversation deck
            var conversationDeck = CreateDeckInstance(npc.ID, npc.PersonalityType);
            npc.ConversationDeck = conversationDeck;
            
            // Create goal deck
            var goalDeck = CreateGoalDeck(npc.ID);
            npc.GoalDeck = goalDeck;
            
            Console.WriteLine($"Initialized deck for {npc.Name}: {conversationDeck.Count} conversation cards, {goalDeck.Count} goal cards");
        }
    }
    
    /// <summary>
    /// Create a 20-card deck instance for an NPC
    /// </summary>
    private List<ConversationCard> CreateDeckInstance(string npcId, PersonalityType personality)
    {
        var deck = new List<ConversationCard>();
        var primaryType = GetPrimaryTokenType(personality);
        
        // 6 Fixed comfort cards (4-5 matching type, 1-2 others)
        var fixedComfortCards = cardDatabase.GetCardsByCategory("fixedComfort");
        deck.AddRange(SelectCards(fixedComfortCards, primaryType, 6, 0.75f));
        
        // 4 Scaled comfort cards (ALL matching type)
        var scaledComfortCards = cardDatabase.GetCardsByCategory("scaledComfort");
        deck.AddRange(SelectCards(scaledComfortCards, primaryType, 4, 1.0f));
        
        // 2 Draw cards (matching type)
        var utilityCards = cardDatabase.GetCardsByCategory("utility");
        var drawCards = utilityCards.Where(c => c.EffectType == CardEffectType.DrawCards).ToList();
        deck.AddRange(SelectCards(drawCards, primaryType, 2, 1.0f));
        
        // 2 Weight-add cards (matching type)
        var weightCards = utilityCards.Where(c => c.EffectType == CardEffectType.AddWeight).ToList();
        deck.AddRange(SelectCards(weightCards, primaryType, 2, 1.0f));
        
        // 3 Setup/atmosphere cards (mixed types)
        var setupCards = cardDatabase.GetCardsByCategory("setup");
        deck.AddRange(SelectCards(setupCards, null, 3, 0.0f));
        
        // 2 Dramatic cards (matching type, fleeting)
        var dramaticCards = cardDatabase.GetCardsByCategory("dramatic");
        deck.AddRange(SelectCards(dramaticCards, primaryType, 2, 1.0f));
        
        // Add NPC special cards if any
        var specialCards = cardDatabase.GetNPCSpecialCards(npcId);
        if (specialCards.Count > 0)
        {
            // Replace some base cards with special cards
            int replaceCount = Math.Min(specialCards.Count, 3);
            for (int i = 0; i < replaceCount && deck.Count > 0; i++)
            {
                deck.RemoveAt(deck.Count - 1);
            }
            
            foreach (var special in specialCards.Take(replaceCount))
            {
                deck.Add(special.DeepClone());
            }
        }
        
        // Ensure exactly 20 cards
        while (deck.Count > 20)
        {
            deck.RemoveAt(random.Next(deck.Count));
        }
        
        while (deck.Count < 20)
        {
            // Add more cards from base deck
            var fillerCards = fixedComfortCards.Where(c => c.TokenType == primaryType).ToList();
            if (fillerCards.Count > 0)
            {
                deck.Add(fillerCards[random.Next(fillerCards.Count)].DeepClone());
            }
            else
            {
                // Fallback to any card
                deck.Add(fixedComfortCards[random.Next(fixedComfortCards.Count)].DeepClone());
            }
        }
        
        // Verify deck composition
        ValidateDeckComposition(deck, npcId, primaryType);
        
        return deck;
    }
    
    /// <summary>
    /// Select cards from a pool with preference for matching token type
    /// </summary>
    private List<ConversationCard> SelectCards(List<ConversationCard> pool, TokenType? preferredType, int count, float matchRatio)
    {
        var selected = new List<ConversationCard>();
        
        if (preferredType.HasValue && matchRatio > 0)
        {
            int matchCount = (int)(count * matchRatio);
            
            // Select matching type cards
            var matchingCards = pool.Where(c => c.TokenType == preferredType.Value).ToList();
            var selectedMatching = matchingCards
                .OrderBy(_ => random.Next())
                .Take(matchCount)
                .Select(c => c.DeepClone())
                .ToList();
            
            selected.AddRange(selectedMatching);
            
            // Fill rest with other types
            if (selected.Count < count)
            {
                var otherCards = pool.Where(c => c.TokenType != preferredType.Value).ToList();
                var selectedOthers = otherCards
                    .OrderBy(_ => random.Next())
                    .Take(count - selected.Count)
                    .Select(c => c.DeepClone())
                    .ToList();
                
                selected.AddRange(selectedOthers);
            }
        }
        else
        {
            // Random selection
            selected.AddRange(pool
                .OrderBy(_ => random.Next())
                .Take(count)
                .Select(c => c.DeepClone()));
        }
        
        // If we couldn't get enough cards, fill from the entire pool
        while (selected.Count < count && pool.Count > 0)
        {
            var filler = pool[random.Next(pool.Count)].DeepClone();
            selected.Add(filler);
        }
        
        return selected;
    }
    
    /// <summary>
    /// Create goal deck for an NPC
    /// </summary>
    private List<GoalCard> CreateGoalDeck(string npcId)
    {
        var goals = cardDatabase.GetNPCGoals(npcId);
        
        // Deep clone all goal cards for this NPC
        return goals.Select(g => g.DeepClone() as GoalCard).ToList();
    }
    
    /// <summary>
    /// Get primary token type based on personality
    /// </summary>
    private TokenType GetPrimaryTokenType(PersonalityType personality)
    {
        return personality switch
        {
            PersonalityType.DEVOTED => TokenType.Trust,
            PersonalityType.MERCANTILE => TokenType.Commerce,
            PersonalityType.PROUD => TokenType.Status,
            PersonalityType.CUNNING => TokenType.Shadow,
            PersonalityType.STEADFAST => (TokenType)random.Next(4), // Random for steadfast
            _ => TokenType.Trust
        };
    }
    
    /// <summary>
    /// Validate that deck meets composition requirements
    /// </summary>
    private void ValidateDeckComposition(List<ConversationCard> deck, string npcId, TokenType primaryType)
    {
        // Check total count
        if (deck.Count != 20)
        {
            Console.WriteLine($"WARNING: {npcId} deck has {deck.Count} cards instead of 20!");
        }
        
        // Check that 75% match personality type
        int matchingCount = deck.Count(c => c.TokenType == primaryType);
        float matchRatio = (float)matchingCount / deck.Count;
        
        if (matchRatio < 0.70f) // Allow some variance
        {
            Console.WriteLine($"WARNING: {npcId} deck only has {matchRatio:P0} cards matching primary type {primaryType}");
        }
        
        // Check persistence ratio (75% should be persistent)
        int persistentCount = deck.Count(c => !c.IsFleeting);
        float persistentRatio = (float)persistentCount / deck.Count;
        
        if (persistentRatio < 0.70f)
        {
            Console.WriteLine($"WARNING: {npcId} deck only has {persistentRatio:P0} persistent cards");
        }
        
        // Check for required card types
        bool hasDrawCards = deck.Any(c => c.EffectType == CardEffectType.DrawCards);
        bool hasWeightCards = deck.Any(c => c.EffectType == CardEffectType.AddWeight);
        bool hasAtmosphereCards = deck.Any(c => c.EffectType == CardEffectType.SetAtmosphere);
        
        if (!hasDrawCards)
            Console.WriteLine($"WARNING: {npcId} deck missing draw cards!");
        if (!hasWeightCards)
            Console.WriteLine($"WARNING: {npcId} deck missing weight cards!");
        if (!hasAtmosphereCards)
            Console.WriteLine($"WARNING: {npcId} deck missing atmosphere cards!");
    }
    
    /// <summary>
    /// Initialize observation cards for a location
    /// Called when entering a new location
    /// </summary>
    public List<ObservationCard> GetLocationObservations(string locationId, string timeOfDay, string withNpc = null)
    {
        var observations = new List<ObservationCard>();
        
        // Get time-based observations
        var timeContext = ConvertTimeToContext(timeOfDay);
        observations.AddRange(cardDatabase.GetLocationObservations(locationId, timeContext));
        
        // Get NPC-specific observations if with an NPC
        if (!string.IsNullOrEmpty(withNpc))
        {
            string npcContext = $"with{char.ToUpper(withNpc[0])}{withNpc.Substring(1)}";
            observations.AddRange(cardDatabase.GetLocationObservations(locationId, npcContext));
        }
        
        // Get general observations
        observations.AddRange(cardDatabase.GetLocationObservations(locationId, "general"));
        
        // Deep clone all observations to create instances
        return observations.Select(o => DeepCloneObservation(o)).ToList();
    }
    
    private string ConvertTimeToContext(string timeOfDay)
    {
        return timeOfDay?.ToLower() switch
        {
            "dawn" => "morning",
            "morning" => "morning",
            "afternoon" => "afternoon",
            "evening" => "evening",
            "night" => "night",
            "latenight" => "night",
            _ => "general"
        };
    }
    
    private ObservationCard DeepCloneObservation(ObservationCard original)
    {
        return new ObservationCard
        {
            Id = original.Id,
            Name = original.Name,
            Description = original.Description,
            DialogueFragment = original.DialogueFragment,
            TokenType = original.TokenType,
            UniqueEffect = original.UniqueEffect,
            ExpirationHours = original.ExpirationHours,
            LocationDiscovered = original.LocationDiscovered,
            ObservationId = original.ObservationId,
            Weight = original.Weight,
            Difficulty = original.Difficulty
        };
    }
}