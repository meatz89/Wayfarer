using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the player's hand of cards during conversations.
/// Handles fleeting card removal after SPEAK actions and Final Word enforcement.
/// </summary>
public class HandManager
{
    private List<ConversationCard> currentHand = new();
    private List<ConversationCard> drawPile = new();
    private List<ConversationCard> discardPile = new();
    
    public IReadOnlyList<ConversationCard> CurrentHand => currentHand.AsReadOnly();
    public IReadOnlyList<ConversationCard> DrawPile => drawPile.AsReadOnly();
    public IReadOnlyList<ConversationCard> DiscardPile => discardPile.AsReadOnly();
    
    /// <summary>
    /// Process effects of SPEAK action on hand
    /// </summary>
    /// <param name="playedCard">The card that was played (can be null)</param>
    /// <returns>True if conversation should continue, false if Final Word triggered</returns>
    public bool OnSpeakAction(ConversationCard playedCard)
    {
        // Remove played card from hand if it was played
        if (playedCard != null && currentHand.Contains(playedCard))
        {
            currentHand.Remove(playedCard);
            discardPile.Add(playedCard);
        }
        
        // Check for unplayed goal with Final Word BEFORE removing fleeting
        var unplayedGoals = currentHand
            .Where(c => c.HasFinalWord && c != playedCard)
            .ToList();
            
        if (unplayedGoals.Any())
        {
            // Final Word triggers - conversation fails
            // Remove all cards from hand to discard
            discardPile.AddRange(currentHand);
            currentHand.Clear();
            return false; // Signal conversation failure
        }
        
        // Remove ALL fleeting cards after SPEAK (whether played or not)
        var fleetingCards = currentHand.Where(c => c.IsFleeting).ToList();
        foreach (var fleeting in fleetingCards)
        {
            currentHand.Remove(fleeting);
            discardPile.Add(fleeting);
        }
        
        return true; // Conversation continues
    }
    
    /// <summary>
    /// Process LISTEN action - fleeting cards are NOT removed
    /// </summary>
    public void OnListenAction()
    {
        // Fleeting cards are NOT removed on LISTEN
        // This is critical - they persist until next SPEAK
        // This allows strategic timing of when to play them
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
    /// Count fleeting cards in current hand
    /// </summary>
    public int CountFleetingCards()
    {
        return currentHand.Count(c => c.IsFleeting);
    }
    
    /// <summary>
    /// Check if any goal cards with Final Word are in hand
    /// </summary>
    public bool HasUnplayedFinalWordGoals()
    {
        return currentHand.Any(c => c.HasFinalWord);
    }
    
    /// <summary>
    /// Get all fleeting cards in hand
    /// </summary>
    public List<ConversationCard> GetFleetingCards()
    {
        return currentHand.Where(c => c.IsFleeting).ToList();
    }
    
    /// <summary>
    /// Get all goal cards with Final Word in hand
    /// </summary>
    public List<ConversationCard> GetFinalWordGoals()
    {
        return currentHand.Where(c => c.HasFinalWord).ToList();
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
/// Exception thrown when conversation must end due to Final Word
/// </summary>
public class ConversationEndedException : Exception
{
    public ConversationEndedException(string message) : base(message) { }
}