using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds conversation decks from player cards, NPC progression cards, observation cards, and goal cards.
/// Extracted from ConversationFacade to follow single responsibility principle.
/// </summary>
public class ConversationDeckBuilder
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly Random _random = new Random();

    public ConversationDeckBuilder(GameWorld gameWorld, TokenMechanicsManager tokenManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    /// <summary>
    /// Create a conversation deck from NPC templates (no filtering by type)
    /// Returns both the deck and any request cards that should start in hand
    /// </summary>
    public (SessionCardDeck deck, List<CardInstance> requestCards) CreateConversationDeck(
        NPC npc,
        ConversationType conversationType,
        string goalCardId = null,
        List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Start with player's conversation deck (persistent CardInstances with XP)
        Player player = _gameWorld.GetPlayer();
        List<CardInstance> playerInstances = new List<CardInstance>();

        // Get all player's card instances (these have XP)
        if (player.ConversationDeck != null && player.ConversationDeck.Count > 0)
        {
            playerInstances.AddRange(player.ConversationDeck.GetAllInstances());
        }
        else
        {
            // Critical error - player has no conversation abilities!
            Console.WriteLine("[ConversationDeckBuilder] ERROR: Player has no conversation deck! Check PackageLoader initialization.");
            // Continue anyway to avoid crash, but conversation will be unplayable
        }

        // Create session deck from player's instances (preserves XP)
        SessionCardDeck deck = SessionCardDeck.CreateFromInstances(playerInstances, sessionId);

        // Add unlocked NPC progression cards as new instances
        List<ConversationCard> unlockedProgressionCards = GetUnlockedProgressionCards(npc);
        foreach (var progressionCard in unlockedProgressionCards)
        {
            CardInstance progressionInstance = new CardInstance(progressionCard, npc.ID);
            deck.AddCard(progressionInstance);
        }

        // Safety check - ensure we have at least some cards
        if (playerInstances.Count == 0 && unlockedProgressionCards.Count == 0)
        {
            Console.WriteLine($"[ConversationDeckBuilder] WARNING: Creating conversation with {npc.Name} but deck has NO cards!");
        }

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Get request cards based on conversation type from JSON data
        // For Request conversations, this loads ALL cards from the Request bundle
        List<CardInstance> requestCards = SelectGoalCardsForConversationType(npc, conversationType, goalCardId, deck);

        // HIGHLANDER: Add request cards directly to deck's request pile
        foreach (var requestCard in requestCards)
        {
            deck.AddRequestCard(requestCard);
        }

        // Now shuffle the deck after all cards (including promise cards) have been added
        deck.ShuffleDrawPile();

        // HIGHLANDER: Return only deck, not separate request cards
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Get unlocked NPC progression cards based on current token counts
    /// </summary>
    private List<ConversationCard> GetUnlockedProgressionCards(NPC npc)
    {
        List<ConversationCard> unlockedCards = new List<ConversationCard>();

        if (npc.ProgressionDeck == null)
            return unlockedCards;

        // Get current token counts for this NPC
        Dictionary<ConnectionType, int> tokenCounts = GetNpcTokenCounts(npc);

        // Check each card's unlock requirements
        foreach (ConversationCard card in npc.ProgressionDeck.GetAllCards())
        {
            if (card.RequiredTokenType.HasValue && card.MinimumTokensRequired > 0)
            {
                ConnectionType requiredType = card.RequiredTokenType.Value;
                int currentTokens = tokenCounts.GetValueOrDefault(requiredType, 0);

                if (currentTokens >= card.MinimumTokensRequired)
                {
                    unlockedCards.Add(card);
                }
            }
            else
            {
                // No token requirement - always unlocked
                unlockedCards.Add(card);
            }
        }

        return unlockedCards;
    }

    /// <summary>
    /// Select appropriate goal card from JSON data based on conversation type
    /// </summary>
    private List<CardInstance> SelectGoalCardsForConversationType(NPC npc, ConversationType conversationType, string goalCardId, SessionCardDeck deck)
    {
        List<CardInstance> requestCards = new List<CardInstance>();

        // If specific card ID provided, this might be a request ID - find that request
        if (!string.IsNullOrEmpty(goalCardId) && npc.Requests != null)
        {
            // First check if it's a request ID
            var request = npc.GetRequestById(goalCardId);
            if (request != null && request.IsAvailable())
            {
                // Load ALL cards from the Request bundle

                // Add ALL request cards to be returned for active pile
                foreach (var requestCardId in request.RequestCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(requestCardId, out var requestCard))
                    {
                        Console.WriteLine($"[ConversationDeckBuilder] Warning: Request card ID '{requestCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }

                    // Create a new template with BurdenGoal type based on the original
                    ConversationCard burdenGoalTemplate = new ConversationCard
                    {
                        Id = requestCard.Id,
                        Description = requestCard.Description,
                        Focus = requestCard.Focus,
                        Difficulty = requestCard.Difficulty,
                        TokenType = requestCard.TokenType,
                        Persistence = requestCard.Persistence,
                        SuccessType = requestCard.SuccessType,
                        FailureType = requestCard.FailureType,
                        ExhaustType = requestCard.ExhaustType,
                        DialogueFragment = requestCard.DialogueFragment,
                        VerbPhrase = requestCard.VerbPhrase,
                        PersonalityTypes = requestCard.PersonalityTypes,
                        LevelBonuses = requestCard.LevelBonuses,
                        MinimumTokensRequired = requestCard.MinimumTokensRequired,
                        RapportThreshold = requestCard.RapportThreshold,
                        QueuePosition = requestCard.QueuePosition,
                        InstantRapport = requestCard.InstantRapport,
                        RequestId = requestCard.RequestId,
                        IsSkeleton = requestCard.IsSkeleton,
                        SkeletonSource = requestCard.SkeletonSource,
                        RequiredTokenType = requestCard.RequiredTokenType,
                        CardType = CardType.BurdenGoal // Override to mark as BurdenGoal
                    };

                    // Create a new card instance with BurdenGoal template
                    CardInstance instance = new CardInstance(burdenGoalTemplate, npc.ID);

                    // Store the rapport threshold and request ID in the card context
                    instance.Context = new CardContext
                    {
                        RapportThreshold = requestCard.RapportThreshold,
                        RequestId = request.Id
                    };

                    // Request cards start as Unplayable until rapport threshold is met
                    instance.IsPlayable = false;

                    requestCards.Add(instance);
                }

                // Add promise cards to the deck for shuffling (not returned)
                foreach (var promiseCardId in request.PromiseCardIds)
                {
                    // Retrieve the card from GameWorld - single source of truth
                    if (!_gameWorld.AllCardDefinitions.TryGetValue(promiseCardId, out var promiseCard))
                    {
                        Console.WriteLine($"[ConversationDeckBuilder] Warning: Promise card ID '{promiseCardId}' not found in GameWorld.AllCardDefinitions");
                        continue;
                    }

                    CardInstance promiseInstance = new CardInstance(promiseCard, npc.ID);
                    deck.AddCard(promiseInstance); // Add to deck for shuffling into draw pile
                }

                return requestCards; // Return all request cards for active pile
            }
        }

        // Fallback to existing logic if no specific card ID provided
        switch (conversationType)
        {
            case ConversationType.FriendlyChat:
                // For FriendlyChat, select from NPC's connection token goal cards
                var goalCard = SelectConnectionTokenGoalCard(npc);
                return goalCard != null ? new List<CardInstance> { goalCard } : new List<CardInstance>();

            case ConversationType.Delivery:
                // For Delivery, the goal card is generated based on the letter being delivered
                // This is handled by the obligation system when the delivery conversation starts
                return new List<CardInstance>();

            case ConversationType.Resolution:
                // For Resolution, select from burden resolution cards
                var burdenCard = SelectBurdenResolutionCard(npc);
                return burdenCard != null ? new List<CardInstance> { burdenCard } : new List<CardInstance>();

            default:
                return new List<CardInstance>();
        }
    }

    /// <summary>
    /// Select a connection token goal card from NPC's goal deck
    /// </summary>
    private CardInstance SelectConnectionTokenGoalCard(NPC npc)
    {
        // Connection token goal cards should be in the NPC's one-time requests
        // These are cards that grant connection tokens when played at rapport threshold
        if (npc.Requests == null || !npc.Requests.Any())
            return null;

        // Look for cards with CardType Promise in available requests
        var availableRequests = npc.GetAvailableRequests();
        if (!availableRequests.Any())
            return null;

        // Get all promise cards from all available requests
        List<ConversationCard> goalCards = new List<ConversationCard>();
        foreach (var request in availableRequests)
        {
            // Retrieve promise cards from GameWorld using IDs
            var promiseCards = request.GetPromiseCards(_gameWorld);
            goalCards.AddRange(promiseCards.Where(card => card.CardType == CardType.Promise));
        }

        if (!goalCards.Any())
            return null;

        ConversationCard selectedGoal = goalCards[_random.Next(goalCards.Count)];
        CardInstance goalInstance = new CardInstance(selectedGoal, npc.ID);

        // Store the rapport threshold in the card context (same as Elena's letter)
        if (goalInstance.Context == null)
            goalInstance.Context = new CardContext();

        // Use the rapport threshold from the card itself (from JSON)
        goalInstance.Context.RapportThreshold = selectedGoal.RapportThreshold;

        return goalInstance;
    }

    /// <summary>
    /// Select a burden resolution card for Resolution conversations
    /// </summary>
    private CardInstance SelectBurdenResolutionCard(NPC npc)
    {
        // For now, return null - burden resolution not fully implemented
        return null;
    }

    /// <summary>
    /// Get current token counts for an NPC
    /// </summary>
    public Dictionary<ConnectionType, int> GetNpcTokenCounts(NPC npc)
    {
        Dictionary<ConnectionType, int> tokenCounts = new Dictionary<ConnectionType, int>
        {
            { ConnectionType.Trust, _tokenManager.GetTokenCount(ConnectionType.Trust, npc.ID) },
            { ConnectionType.Commerce, _tokenManager.GetTokenCount(ConnectionType.Commerce, npc.ID) },
            { ConnectionType.Status, _tokenManager.GetTokenCount(ConnectionType.Status, npc.ID) },
            { ConnectionType.Shadow, _tokenManager.GetTokenCount(ConnectionType.Shadow, npc.ID) }
        };
        return tokenCounts;
    }
}