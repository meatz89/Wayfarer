using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds NPC conversation decks by filtering the base deck based on personality types.
/// Filters cards where their personalityTypes array contains the NPC's personalityType or "ALL".
/// </summary>
public class NPCDeckBuilder
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random;

    public NPCDeckBuilder(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _random = new Random();
    }

    /// <summary>
    /// Build an NPC's conversation deck by filtering cards based on their personality type
    /// </summary>
    /// <param name="personalityType">The NPC's personality type</param>
    /// <returns>A deck of exactly 20 cards filtered and shuffled for the NPC</returns>
    public List<ConversationCard> BuildNPCDeck(PersonalityType personalityType)
    {
        // Get all conversation cards from gameWorld
        List<ConversationCard> allCards = _gameWorld.AllCardDefinitions.Values.ToList();

        // Filter cards by personality type
        List<ConversationCard> filteredCards = FilterCardsByPersonalityType(allCards, personalityType);

        // Shuffle the filtered cards for variety
        List<ConversationCard> shuffledCards = ShuffleCards(filteredCards);

        // Take exactly 20 cards for the NPC's deck
        List<ConversationCard> npcDeck = shuffledCards.Take(20).ToList();

        // If we have less than 20 cards after filtering, fill with "ALL" cards to reach 20
        if (npcDeck.Count < 20)
        {
            List<ConversationCard> allTypeCards = allCards
                .Where(card => HasPersonalityType(card, "ALL"))
                .Except(npcDeck)
                .ToList();
            
            allTypeCards = ShuffleCards(allTypeCards);
            npcDeck.AddRange(allTypeCards.Take(20 - npcDeck.Count));
        }

        return npcDeck;
    }

    /// <summary>
    /// Filter cards to only include those that match the NPC's personality type or are available to "ALL"
    /// </summary>
    private List<ConversationCard> FilterCardsByPersonalityType(List<ConversationCard> cards, PersonalityType personalityType)
    {
        string personalityString = personalityType.ToString();

        return cards.Where(card => 
            HasPersonalityType(card, personalityString) || 
            HasPersonalityType(card, "ALL")
        ).ToList();
    }

    /// <summary>
    /// Check if a card has the specified personality type in its personalityTypes array.
    /// </summary>
    private bool HasPersonalityType(ConversationCard card, string personalityType)
    {
        if (card.PersonalityTypes == null || card.PersonalityTypes.Count == 0)
        {
            // If no personality types specified, treat as available to ALL
            return personalityType.Equals("ALL", StringComparison.OrdinalIgnoreCase);
        }

        return card.PersonalityTypes.Any(pt => 
            pt.Equals(personalityType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Shuffle a list of cards using Fisher-Yates algorithm
    /// </summary>
    private List<ConversationCard> ShuffleCards(List<ConversationCard> cards)
    {
        List<ConversationCard> shuffled = new List<ConversationCard>(cards);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        return shuffled;
    }
}