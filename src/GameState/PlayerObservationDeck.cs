using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Player's personal observation deck with expiration tracking.
/// Maximum 20 cards, each with acquisition time for decay management.
/// Cards expire after 24-48 hours and are automatically removed.
/// </summary>
public class PlayerObservationDeck
{

    private readonly List<ObservationCardEntry> _cards;
    private const int MaxCards = 20;

    public PlayerObservationDeck()
    {
        _cards = new List<ObservationCardEntry>();
    }

    /// <summary>
    /// Add an observation card to the deck
    /// Returns false if deck is full
    /// </summary>
    public bool AddCard(ObservationCard card, int currentDay, TimeBlocks currentTimeBlock)
    {
        // Remove expired cards first
        RemoveExpiredCards(currentDay, currentTimeBlock);

        // Check deck size limit
        if (_cards.Count >= MaxCards)
        {
            Console.WriteLine($"[PlayerObservationDeck] Deck full ({MaxCards} cards), cannot add new card");
            return false;
        }

        ObservationCardEntry entry = new ObservationCardEntry
        {
            Card = card,
            AcquiredAt = card.CreatedAt,
            DayAcquired = currentDay,
            TimeBlockAcquired = currentTimeBlock
        };

        _cards.Add(entry);
        Console.WriteLine($"[PlayerObservationDeck] Added observation card {card.Id}, deck now has {_cards.Count} cards");
        return true;
    }

    /// <summary>
    /// Get all active (non-expired) observation cards
    /// </summary>
    public List<ObservationCard> GetActiveCards(int currentDay, TimeBlocks currentTimeBlock)
    {
        RemoveExpiredCards(currentDay, currentTimeBlock);
        return _cards.Select(e => e.Card).ToList();
    }

    /// <summary>
    /// Get observation cards as conversation cards for use in conversations
    /// Updates decay states and filters out expired cards
    /// </summary>
    public List<ConversationCard> GetAsConversationCards(DateTime currentGameTime, int currentDay, TimeBlocks currentTimeBlock)
    {
        RemoveExpiredCards(currentDay, currentTimeBlock);

        List<ConversationCard> conversationCards = new List<ConversationCard>();

        foreach (ObservationCardEntry entry in _cards)
        {
            ObservationCard obsCard = entry.Card;

            // Update decay state based on current game time
            obsCard.UpdateDecayState(currentGameTime);

            // Skip expired cards
            if (!obsCard.IsPlayable)
                continue;

            // Return the underlying conversation card
            conversationCards.Add(obsCard.ConversationCard);
        }

        return conversationCards;
    }

    /// <summary>
    /// Remove a specific card after it's been played
    /// </summary>
    public void RemoveCard(string cardId)
    {
        ObservationCardEntry? entry = _cards.FirstOrDefault(e => e.Card.Id == cardId);
        if (entry != null)
        {
            _cards.Remove(entry);
            Console.WriteLine($"[PlayerObservationDeck] Removed card {cardId}, {_cards.Count} cards remaining");
        }
    }

    /// <summary>
    /// Remove all expired cards from the deck
    /// </summary>
    public void RemoveExpiredCards(int currentDay, TimeBlocks currentTimeBlock)
    {
        int expiredCount = _cards.RemoveAll(e => e.IsExpired(currentDay, currentTimeBlock));
        if (expiredCount > 0)
        {
            Console.WriteLine($"[PlayerObservationDeck] Removed {expiredCount} expired cards");
        }
    }

    /// <summary>
    /// Clear all cards (for new game, etc)
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
        Console.WriteLine("[PlayerObservationDeck] Cleared all observation cards");
    }

    /// <summary>
    /// Get current card count
    /// </summary>
    public int Count => _cards.Count;

    /// <summary>
    /// Check if deck is full
    /// </summary>
    public bool IsFull => _cards.Count >= MaxCards;

    /// <summary>
    /// Get info about deck state for UI
    /// </summary>
    public DeckInfo GetDeckInfo()
    {
        return new DeckInfo(_cards.Count, MaxCards);
    }

    /// <summary>
    /// Get detailed card info for UI display
    /// </summary>
    public List<ObservationCardDetail> GetCardDetails(int currentDay, TimeBlocks currentTimeBlock)
    {
        RemoveExpiredCards(currentDay, currentTimeBlock);

        List<ObservationCardDetail> details = new List<ObservationCardDetail>();
        foreach (ObservationCardEntry entry in _cards)
        {
            int daysElapsed = currentDay - entry.DayAcquired;
            int timeBlocksElapsed = (daysElapsed * 6) + (currentTimeBlock - entry.TimeBlockAcquired);
            int segmentsElapsed = timeBlocksElapsed * 8; // 8 segments per time block (4 hours * 2 segments per hour)
            int segmentsRemaining = Math.Max(0, 96 - segmentsElapsed); // 96 segments default expiration (48 hours * 2)

            details.Add(new ObservationCardDetail(entry.Card, segmentsRemaining));
        }

        return details;
    }
}