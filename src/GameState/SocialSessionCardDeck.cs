using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HIGHLANDER PRINCIPLE: This is the ONE AND ONLY card deck system for conversations.
/// ALL card collections live here. NO EXCEPTIONS.
/// Uses Pile abstraction consistently - NEVER expose List<CardInstance> directly.
/// NO compatibility layers, NO legacy code, NO fallbacks.
///
/// This class manages the correct pile system:
/// - Deck (cards to draw from)
/// - Mind (hand - cards currently available to play)
/// - Spoken (conversation memory - where played cards go)
/// - Request pile (cards waiting for momentum threshold)
/// </summary>
public class SocialSessionCardDeck
{
    // CORRECT PILE SYSTEM: Deck → Mind → Spoken
    private readonly Pile goalPile = new();      // Cards to draw from
    private readonly Pile deckPile = new();      // Cards to draw from
    private readonly Pile spokenPile = new();    // Spoken pile - conversation memory
    private readonly Pile mindPile = new();      // Mind pile - cards in hand
    private readonly Pile requestPile = new();   // Cards waiting for momentum threshold

    private readonly string npcId;
    // DETERMINISTIC SYSTEM: No random number generation needed

    public SocialSessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    // ENCAPSULATED: Read-only access to pile contents through Pile's IReadOnlyList property
    public IReadOnlyList<CardInstance> HandCards => mindPile.Cards;
    public IReadOnlyList<CardInstance> GoalCards => requestPile.Cards;
    public IReadOnlyList<CardInstance> SpokenCards => spokenPile.Cards;

    // Read-only properties for deck state
    public int RemainingDeckCards => deckPile.Count;
    public int SpokenPileCount => spokenPile.Count;
    public int TotalDeckCards => deckPile.Count + spokenPile.Count;
    public int HandSize => mindPile.Count;
    public int RequestPileSize => requestPile.Count;

    /// <summary>
    /// Create a deck from card templates
    /// </summary>
    public static SocialSessionCardDeck CreateFromTemplates(List<SocialCard> templates, string npcId)
    {
        SocialSessionCardDeck deck = new SocialSessionCardDeck(npcId);
        foreach (SocialCard template in templates)
        {
            CardInstance cardInstance = new CardInstance(template);
            deck.deckPile.Add(cardInstance);
        }
        return deck;
    }

    /// <summary>
    /// Create a deck from existing card instances (preserves XP)
    /// </summary>
    public static SocialSessionCardDeck CreateFromInstances(List<CardInstance> instances, string npcId)
    {
        SocialSessionCardDeck deck = new SocialSessionCardDeck(npcId);
        foreach (CardInstance instance in instances)
        {
            // Preserve context
            CardInstance sessionInstance = new CardInstance(instance.SocialCardTemplate)
            {
                InstanceId = instance.InstanceId,
                Context = instance.Context,
                IsPlayable = instance.IsPlayable
            };
            deck.deckPile.Add(sessionInstance);
        }
        return deck;
    }

    /// <summary>
    /// Add a card directly to the Deck pile
    /// </summary>
    public void AddCard(CardInstance card)
    {
        if (card != null)
            deckPile.Add(card);
    }

    /// <summary>
    /// Add a request/goal card that requires rapport threshold
    /// </summary>
    public void AddGoalCard(CardInstance card)
    {
        if (card != null)
            requestPile.Add(card);
    }

    /// <summary>
    /// Draw a single card (does NOT add to hand automatically)
    /// </summary>
    public CardInstance DrawCard()
    {
        // Reshuffle if needed - ONLY Standard cards reshuffle
        if (deckPile.Count == 0 && spokenPile.Count > 0)
        {
            ReshuffleSpokenPile();
        }

        if (deckPile.Count == 0)
            return null;

        CardInstance card = deckPile.DrawTop();
        AssignPreRoll(card);
        return card;
    }

    /// <summary>
    /// Draw cards directly to hand with tier-based filtering
    /// Uses ConversationSession to get unlocked tiers and stat bonuses for depth access
    /// </summary>
    public void DrawToHand(int count, SocialSession session, PlayerStats playerStats)
    {
        int maxDepth = session.GetUnlockedMaxDepth();int cardsDrawn = 0;

        for (int i = 0; i < count; i++)
        {
            // Check if there are accessible cards before attempting draw
            bool hasAccessibleCards = deckPile.Cards.Any(card => CanAccessCard(card.SocialCardTemplate, session, playerStats));

            if (!hasAccessibleCards && spokenPile.Count > 0)
            {ReshuffleSpokenPile();
            }

            // If still no cards after reshuffling, we're out of cards entirely
            if (deckPile.Count == 0)
            {break;
            }

            // Apply tier-based filtering
            CardInstance card = DrawNextAccessibleCard(session, playerStats);

            if (card != null)
            {
                AssignPreRoll(card);
                mindPile.Add(card);
                cardsDrawn++;
            }
            else
            {break;
            }
        }

        // Log the actual draw result
        if (cardsDrawn < count)
        {}
        else
        {}
    }

    /// <summary>
    /// Play a card with correct Echo/Statement persistence behavior:
    /// - Echo cards: Return immediately to deck (always available)
    /// - Statement cards: Go to spoken pile permanently (one-time use)
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null)
        {return;
        }

        // Track total cards before operation
        int totalCardsBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;

        // Check if card exists in mind (hand) before removing
        if (!mindPile.Contains(card))
        {return;
        }

        mindPile.Remove(card);

        // Route card based on persistence type
        if (card.SocialCardTemplate.Persistence == PersistenceType.Echo)
        {
            // Echo cards return to BOTTOM of deck (drawn last, not immediately)
            deckPile.Add(card);}
        else
        {
            // Statement cards go to spoken pile permanently (one-time use)
            spokenPile.Add(card);}

        // Validate total card count remains constant
        int totalCardsAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;if (totalCardsBefore != totalCardsAfter)
        {}
    }

    /// <summary>
    /// Move a card to Spoken pile without playing it (for special effects)
    /// </summary>
    public void MoveToSpoken(CardInstance card)
    {
        if (card != null && !spokenPile.Contains(card))
        {
            spokenPile.Add(card);
        }
    }

    /// <summary>
    /// Remove a card from Mind and move to Spoken pile
    /// </summary>
    public void MoveFromMindToSpoken(CardInstance card)
    {
        if (card == null) return;

        int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;mindPile.Remove(card);
        spokenPile.Add(card);

        int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        if (totalBefore != totalAfter)
        {}
    }

    /// <summary>
    /// Check request pile and move cards to mind if momentum threshold met
    /// Returns the list of cards that were moved
    /// </summary>
    public List<CardInstance> CheckRequestThresholds(int currentMomentum)
    {
        List<CardInstance> toMove = requestPile.Cards
            .Where(c => c.Context?.threshold <= currentMomentum)
            .ToList();

        List<CardInstance> movedCards = new List<CardInstance>();

        foreach (CardInstance? card in toMove)
        {
            int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;

            requestPile.Remove(card);
            card.IsPlayable = true; // NOW playable - threshold met
            mindPile.Add(card);movedCards.Add(card);

            int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
            if (totalBefore != totalAfter)
            {}
        }

        return movedCards;
    }

    /// <summary>
    /// Shuffle the Deck pile
    /// </summary>
    public void ShuffleDeckPile()
    {
        deckPile.Shuffle();
    }

    /// <summary>
    /// Legacy reshuffle method - NO LONGER NEEDED
    /// Echo cards return immediately to deck when played, never go to spoken pile
    /// Only Statement cards go to spoken pile permanently
    /// </summary>
    private void ReshuffleSpokenPile()
    {
        // Echo cards should never be in spoken pile due to correct PlayCard implementation
        List<CardInstance> echoCards = spokenPile.Cards
            .Where(card => card?.SocialCardTemplate?.Persistence == PersistenceType.Echo)
            .ToList();

        if (echoCards.Any())
        {// Remove them as emergency fix
            foreach (CardInstance card in echoCards)
            {
                spokenPile.Remove(card);
            }
            deckPile.AddRange(echoCards);
            deckPile.Shuffle();
        }
        else
        {}
    }

    /// <summary>
    /// Assign pre-rolled dice value to a card
    /// </summary>
    private void AssignPreRoll(CardInstance card)
    {
        if (card == null) return;

        if (card.Context == null)
            card.Context = new CardContext();

        // DETERMINISTIC SYSTEM: No pre-rolled values needed - removed dice logic
    }

    /// <summary>
    /// Check if cards are available (in deck or can reshuffle Echo cards from spoken)
    /// </summary>
    public bool HasCardsAvailable()
    {
        bool hasInDeck = deckPile.Count > 0;
        bool hasReshufflableInSpoken = spokenPile.Cards.Any(card => card?.SocialCardTemplate?.Persistence == PersistenceType.Echo);

        return hasInDeck || hasReshufflableInSpoken;
    }

    /// <summary>
    /// Clear all piles (for cleanup)
    /// </summary>
    public void Clear()
    {
        deckPile.Clear();
        spokenPile.Clear();
        mindPile.Clear();
        requestPile.Clear();
    }

    /// <summary>
    /// Reset for a new conversation (moves all cards back to Deck pile)
    /// </summary>
    public void ResetForNewConversation()
    {
        // Move all cards from all piles back to deck
        deckPile.AddRange(spokenPile.DrawAll());
        deckPile.AddRange(mindPile.DrawAll());
        deckPile.AddRange(requestPile.DrawAll());

        // Shuffle for fresh start
        deckPile.Shuffle();
    }

    /// <summary>
    /// Check if a card is in the mind (hand)
    /// </summary>
    public bool IsCardInMind(CardInstance card)
    {
        return mindPile.Contains(card);
    }

    /// <summary>
    /// Check if a card is in the request pile
    /// </summary>
    public bool IsCardInRequestPile(CardInstance card)
    {
        return requestPile.Contains(card);
    }

    /// <summary>
    /// Add a card directly to mind (use carefully - prefer DrawToHand)
    /// </summary>
    public void AddCardToMind(CardInstance card)
    {
        if (card != null)
        {
            int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
            mindPile.Add(card);
            int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;

            if (totalBefore + 1 != totalAfter)
            {}
        }
    }

    /// <summary>
    /// Add multiple cards directly to mind (for Threading effect)
    /// </summary>
    public void AddCardsToMind(List<CardInstance> cards)
    {
        if (cards == null || cards.Count == 0) return;

        int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;foreach (CardInstance card in cards)
        {
            if (card != null)
            {
                mindPile.Add(card);
            }
        }

        int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        if (totalBefore + cards.Count != totalAfter)
        {}
    }

    /// <summary>
    /// Force discard down to maximum hand size (7 cards)
    /// Removes excess cards from mind back to deck pile
    /// </summary>
    public void DiscardDown(int maxSize = 7)
    {
        if (mindPile.Count <= maxSize) return;

        int cardsToDiscard = mindPile.Count - maxSize;List<CardInstance> cardsToRemove = new List<CardInstance>();

        // For now, discard from the end of hand (could be made more sophisticated later)
        for (int i = 0; i < cardsToDiscard; i++)
        {
            if (mindPile.Count > maxSize)
            {
                CardInstance cardToDiscard = mindPile.Cards.Last();
                cardsToRemove.Add(cardToDiscard);
                mindPile.Remove(cardToDiscard);
            }
        }

        // Add discarded cards back to deck pile
        deckPile.AddRange(cardsToRemove);}

    /// <summary>
    /// Process card persistence - ALL cards persist in refined system
    /// Standard and Banish cards both stay in Spoken pile after playing
    /// NO cards are lost during LISTEN actions
    /// </summary>
    public void ProcessCardPersistence()
    {
        // In the refined system, ALL cards persist on SPEAK and LISTEN
        // Cards only go to Spoken pile when explicitly played
        // No cards are removed during LISTEN actions
        // The difference between Standard and Banish is only in reshuffling behavior// Nothing to process - all cards stay where they are during LISTEN
    }

    /// <summary>
    /// Check if a card can be accessed based on unlocked tiers and stat specialization bonuses
    /// Tier system: Tier 1 (depths 1-2), Tier 2 (depths 3-4), Tier 3 (depths 5-6), Tier 4 (depths 7-8)
    /// Tiers unlock at Understanding thresholds (0/6/12/18) and persist once unlocked
    /// </summary>
    private bool CanAccessCard(SocialCard card, SocialSession session, PlayerStats playerStats)
    {
        if (card == null) return false;

        int cardDepth = (int)card.Depth;
        int tierMaxDepth = session.GetUnlockedMaxDepth(); // Based on unlocked tiers, not momentum directly

        // Universal access via unlocked tiers
        if (cardDepth <= tierMaxDepth) return true;

        // Specialist bonus access - can access deeper cards if stat matches
        if (card.BoundStat.HasValue && playerStats != null)
        {
            int statBonus = playerStats.GetDepthBonus(card.BoundStat.Value);
            if (cardDepth <= tierMaxDepth + statBonus) return true;
        }

        return false;
    }

    /// <summary>
    /// Check if a card can be played RIGHT NOW (tier + Statement requirements + Initiative cost).
    /// This is used by UI to determine which cards in hand are playable vs locked.
    /// </summary>
    public bool CanPlayCard(CardInstance card, SocialSession session, PlayerStats playerStats)
    {
        if (card == null) return false;

        // Check tier access
        if (!CanAccessCard(card.SocialCardTemplate, session, playerStats))
            return false;

        // Check Statement requirements (signature card lock)
        if (!card.SocialCardTemplate.MeetsStatementRequirements(session))
            return false;

        // Check Initiative cost
        if (card.SocialCardTemplate.InitiativeCost > session.CurrentInitiative)
            return false;

        return true;
    }

    /// <summary>
    /// Draw the next accessible card from the deck pile based on tier filtering
    /// Maintains deck order - draws the first accessible card from top of deck
    /// Returns null if no accessible cards are available
    /// </summary>
    private CardInstance DrawNextAccessibleCard(SocialSession session, PlayerStats playerStats)
    {
        int maxDepth = session.GetUnlockedMaxDepth();

        // Go through deck in order (top to bottom) and find first accessible card
        foreach (CardInstance card in deckPile.Cards)
        {
            if (CanAccessCard(card.SocialCardTemplate, session, playerStats))
            {
                // Found the first accessible card - remove and return it
                deckPile.Remove(card);
                return card;
            }
        }

        // No accessible cards found
        return null;
    }

    // NO DIRECT PILE ACCESS - ALL OPERATIONS GO THROUGH METHODS!
}