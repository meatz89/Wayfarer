using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Represents an NPC's conversation deck - their unique set of possible interactions
/// Decks evolve through letter delivery and conversation outcomes
/// </summary>
public class NPCDeck
{
    public string NpcId { get; set; }
    public List<ConversationCard> Cards { get; set; } = new List<ConversationCard>();
    
    // Maximum deck size to prevent bloat
    public const int MaxDeckSize = 20;
    
    public NPCDeck(string npcId)
    {
        NpcId = npcId;
        InitializeStartingDeck();
    }
    
    /// <summary>
    /// Initialize with universal starting cards + personality-specific cards
    /// </summary>
    private void InitializeStartingDeck()
    {
        // Universal starting cards (from docs)
        Cards.AddRange(new[]
        {
            new ConversationCard
            {
                Id = "small_talk",
                Name = "Small Talk",
                Description = "Simple social courtesy",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 2,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "listen",
                Name = "Listen",
                Description = "Give them your full attention",
                Difficulty = 3,
                PatienceCost = 1,
                ComfortGain = 3,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "how_are_things",
                Name = "How Are Things?",
                Description = "Check on their current situation",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 1,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "offer_help",
                Name = "Offer Help",
                Description = "Show willingness to assist",
                Difficulty = 4,
                PatienceCost = 2,
                ComfortGain = 4,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "quick_exit",
                Name = "Quick Exit",
                Description = "Politely end the conversation",
                Difficulty = 0,
                PatienceCost = 0,
                ComfortGain = 0,
                Category = RelationshipCardCategory.Basic
            },
            // Negative cards (deck pollution)
            new ConversationCard
            {
                Id = "awkward_silence",
                Name = "Awkward Silence",
                Description = "Uncomfortable pause in conversation",
                Difficulty = 8,
                PatienceCost = 0,
                ComfortGain = -2,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "bad_memory",
                Name = "Bad Memory",
                Description = "Brings up an unfortunate past event",
                Difficulty = 6,
                PatienceCost = 1,
                ComfortGain = -1,
                Category = RelationshipCardCategory.Basic
            }
        });
    }
    
    /// <summary>
    /// Draw 5 cards from deck for this conversation round
    /// </summary>
    public List<ConversationCard> DrawCards(Dictionary<ConnectionType, int> currentTokens, int currentComfort = 0)
    {
        var availableCards = Cards.Where(card => card.CanPlay(currentTokens, currentComfort)).ToList();
        
        // Shuffle available cards
        var random = new Random();
        availableCards = availableCards.OrderBy(x => random.Next()).ToList();
        
        // Draw up to 5 cards, always include Quick Exit
        var drawnCards = new List<ConversationCard>();
        var exitCard = availableCards.FirstOrDefault(c => c.Id == "quick_exit");
        if (exitCard != null)
        {
            drawnCards.Add(exitCard);
            availableCards.Remove(exitCard);
        }
        
        // Fill remaining slots with other cards
        drawnCards.AddRange(availableCards.Take(4));
        
        return drawnCards;
    }
    
    /// <summary>
    /// Add a card to the deck (from letter delivery rewards)
    /// </summary>
    public void AddCard(ConversationCard card)
    {
        if (Cards.Count >= MaxDeckSize) return;
        
        // Don't add duplicates
        if (Cards.Any(c => c.Id == card.Id)) return;
        
        Cards.Add(card);
    }
    
    /// <summary>
    /// Remove a card from the deck (clearing negatives)
    /// </summary>
    public void RemoveCard(string cardId)
    {
        Cards.RemoveAll(c => c.Id == cardId);
    }
    
    /// <summary>
    /// Calculate starting patience for conversations
    /// Negative cards reduce starting patience by 0.5 each
    /// </summary>
    public double GetPatiencePenalty()
    {
        var negativeCards = Cards.Where(c => c.ComfortGain < 0).Count();
        return negativeCards * 0.5;
    }
}