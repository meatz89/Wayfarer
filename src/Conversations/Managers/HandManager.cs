using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the player's hand of cards during conversations.
/// Handles exhaust mechanics for Fleeting and Opportunity cards.
/// </summary>
public class HandManager
{
    private List<ConversationCard> currentHand = new();
    private List<ConversationCard> drawPile = new();
    private List<ConversationCard> discardPile = new();
    private List<ConversationCard> exhaustedPile = new();
    
    public IReadOnlyList<ConversationCard> CurrentHand => currentHand.AsReadOnly();
    public IReadOnlyList<ConversationCard> DrawPile => drawPile.AsReadOnly();
    public IReadOnlyList<ConversationCard> DiscardPile => discardPile.AsReadOnly();
    public IReadOnlyList<ConversationCard> ExhaustedPile => exhaustedPile.AsReadOnly();
    
    /// <summary>
    /// Process effects of SPEAK action on hand
    /// </summary>
    /// <param name="playedCard">The card that was played (can be null)</param>
    /// <returns>True if conversation should continue, false if exhaust effect ended it</returns>
    public bool OnSpeakAction(ConversationCard playedCard)
    {
        // Remove played card from hand if it was played
        if (playedCard != null && currentHand.Contains(playedCard))
        {
            currentHand.Remove(playedCard);
            discardPile.Add(playedCard);
        }
        
        // Exhaust ALL fleeting cards after SPEAK (including goals with Fleeting + Opportunity)
        var fleetingCards = currentHand
            .Where(c => c.IsFleeting && c != playedCard)
            .ToList();
        
        foreach (var card in fleetingCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustEffect?.Type != CardEffectType.None)
            {
                var continueConversation = ExecuteExhaustEffect(card);
                if (!continueConversation)
                {
                    // Exhaust effect ended conversation (e.g., goal card)
                    currentHand.Remove(card);
                    exhaustedPile.Add(card);
                    return false;
                }
            }
            
            currentHand.Remove(card);
            exhaustedPile.Add(card); // Track separately from discard
        }
        
        return true; // Conversation continues
    }
    
    /// <summary>
    /// Process LISTEN action - exhaust opportunity cards
    /// </summary>
    /// <returns>True if conversation should continue, false if exhaust effect ended it</returns>
    public bool OnListenAction()
    {
        // Exhaust all Opportunity cards (including goals with Fleeting + Opportunity)
        var opportunityCards = currentHand
            .Where(c => c.IsOpportunity)
            .ToList();
        
        foreach (var card in opportunityCards)
        {
            // Execute exhaust effect if it exists
            if (card.ExhaustEffect?.Type != CardEffectType.None)
            {
                var continueConversation = ExecuteExhaustEffect(card);
                if (!continueConversation)
                {
                    // Exhaust effect ended conversation
                    currentHand.Remove(card);
                    exhaustedPile.Add(card);
                    return false;
                }
            }
            
            currentHand.Remove(card);
            exhaustedPile.Add(card);
        }
        
        return true;
    }
    
    /// <summary>
    /// Draw cards from draw pile to hand
    /// </summary>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count && drawPile.Count > 0; i++)
        {
            var card = drawPile[0];
            drawPile.RemoveAt(0);
            currentHand.Add(card);
        }
        
        // Reshuffle discard if draw pile empty and we still need cards
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            ReshuffleDiscardIntoDraw();
        }
    }
    
    /// <summary>
    /// Add a card directly to hand (for special effects)
    /// </summary>
    public void AddCardToHand(ConversationCard card)
    {
        currentHand.Add(card);
    }
    
    /// <summary>
    /// Add cards directly to hand
    /// </summary>
    public void AddCardsToHand(IEnumerable<ConversationCard> cards)
    {
        currentHand.AddRange(cards);
    }
    
    /// <summary>
    /// Execute a card's exhaust effect
    /// </summary>
    /// <param name="card">The card being exhausted</param>
    /// <returns>True if conversation should continue, false if it should end</returns>
    private bool ExecuteExhaustEffect(ConversationCard card)
    {
        if (card.ExhaustEffect == null || card.ExhaustEffect.Type == CardEffectType.None)
            return true; // No exhaust effect, conversation continues

        switch (card.ExhaustEffect.Type)
        {
            case CardEffectType.EndConversation:
                // Goal cards typically have this - conversation ends in failure
                return false; // Signal conversation should end

            case CardEffectType.SetAtmosphere:
                // Would need atmosphere manager reference to apply
                // For standalone HandManager, just log
                Console.WriteLine($"[HandManager] Card exhaust would set atmosphere: {card.ExhaustEffect.Value}");
                return true;

            case CardEffectType.DrawCards:
                if (int.TryParse(card.ExhaustEffect.Value, out int count))
                {
                    DrawCards(count);
                }
                return true;

            case CardEffectType.AddComfort:
                // Would need comfort manager reference to apply
                // For standalone HandManager, just log
                Console.WriteLine($"[HandManager] Card exhaust would add comfort: {card.ExhaustEffect.Value}");
                return true;

            case CardEffectType.AddWeight:
                // Would need weight manager reference to apply
                // For standalone HandManager, just log
                Console.WriteLine($"[HandManager] Card exhaust would add weight: {card.ExhaustEffect.Value}");
                return true;

            default:
                // Unknown exhaust effect, log and continue
                Console.WriteLine($"[HandManager] Unknown exhaust effect type: {card.ExhaustEffect.Type}");
                return true;
        }
    }
    
    /// <summary>
    /// Count fleeting cards in current hand
    /// </summary>
    public int CountFleetingCards()
    {
        return currentHand.Count(c => c.IsFleeting);
    }
    
    /// <summary>
    /// Count opportunity cards in current hand
    /// </summary>
    public int CountOpportunityCards()
    {
        return currentHand.Count(c => c.IsOpportunity);
    }
    
    /// <summary>
    /// Check if any goal cards (Fleeting + Opportunity) are in hand
    /// </summary>
    public bool HasUnplayedGoalCards()
    {
        return currentHand.Any(c => c.IsGoal);
    }
    
    /// <summary>
    /// Get all fleeting cards in hand
    /// </summary>
    public List<ConversationCard> GetFleetingCards()
    {
        return currentHand.Where(c => c.IsFleeting).ToList();
    }
    
    /// <summary>
    /// Get all opportunity cards in hand
    /// </summary>
    public List<ConversationCard> GetOpportunityCards()
    {
        return currentHand.Where(c => c.IsOpportunity).ToList();
    }
    
    /// <summary>
    /// Get all goal cards (Fleeting + Opportunity) in hand
    /// </summary>
    public List<ConversationCard> GetGoalCards()
    {
        return currentHand.Where(c => c.IsGoal).ToList();
    }
    
    /// <summary>
    /// Initialize deck for a new conversation
    /// </summary>
    public void InitializeDeck(List<ConversationCard> deck, ConversationCard goalForConversation = null)
    {
        // Clear everything
        currentHand.Clear();
        drawPile.Clear();
        discardPile.Clear();
        exhaustedPile.Clear();
        
        // Add all cards to draw pile
        drawPile.AddRange(deck);
        
        // If there's a goal for this conversation, add it
        if (goalForConversation != null)
        {
            drawPile.Add(goalForConversation);
        }
        
        // Shuffle the draw pile
        ShuffleDrawPile();
    }
    
    /// <summary>
    /// Remove a specific card from hand (for special effects)
    /// </summary>
    public bool RemoveCardFromHand(ConversationCard card)
    {
        if (currentHand.Contains(card))
        {
            currentHand.Remove(card);
            discardPile.Add(card);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Clear all cards from hand to discard
    /// </summary>
    public void DiscardHand()
    {
        discardPile.AddRange(currentHand);
        currentHand.Clear();
    }
    
    /// <summary>
    /// Check if we can draw more cards
    /// </summary>
    public bool CanDrawCards()
    {
        return drawPile.Count > 0 || discardPile.Count > 0;
    }
    
    /// <summary>
    /// Get total cards remaining (draw + discard)
    /// </summary>
    public int GetTotalCardsRemaining()
    {
        return drawPile.Count + discardPile.Count;
    }
    
    private void ShuffleDrawPile()
    {
        drawPile = drawPile.OrderBy(_ => Random.Shared.Next()).ToList();
    }
    
    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
    }
}

/// <summary>
/// Exception thrown when conversation must end due to exhaust effect
/// </summary>
public class ConversationEndedException : Exception
{
    public ConversationEndedException(string message) : base(message) { }
}