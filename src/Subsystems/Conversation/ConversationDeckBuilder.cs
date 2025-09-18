using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds conversation decks from conversation type cards and NPC signature cards.
/// NO PLAYER DECK - cards come from conversation types defined in JSON.
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
    /// Create a conversation deck from conversation type cards and NPC signature cards.
    /// Request drives everything - it determines the conversation type and connection type.
    /// </summary>
    public (SessionCardDeck deck, List<CardInstance> requestCards) CreateConversationDeck(
        NPC npc,
        string requestId,
        List<CardInstance> observationCards = null)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Get the request which drives everything
        NPCRequest request = npc.GetRequestById(requestId);
        if (request == null)
        {
            throw new ArgumentException($"Request {requestId} not found for NPC {npc.ID}");
        }

        // Get conversation type from request
        if (!_gameWorld.ConversationTypes.TryGetValue(request.ConversationTypeId, out ConversationTypeDefinition conversationType))
        {
            Console.WriteLine($"[ConversationDeckBuilder] WARNING: Conversation type '{request.ConversationTypeId}' not found, using fallback");
            // Create a fallback conversation type if not found
            conversationType = CreateFallbackConversationType();
        }

        // Get card deck for this conversation type
        if (!_gameWorld.CardDecks.TryGetValue(conversationType.DeckId, out CardDeckDefinition cardDeck))
        {
            Console.WriteLine($"[ConversationDeckBuilder] WARNING: Card deck '{conversationType.DeckId}' not found, using empty deck");
            cardDeck = new CardDeckDefinition { Id = "fallback", CardIds = new List<string>() };
        }

        // Create card instances from the conversation type's deck
        List<CardInstance> deckInstances = CreateInstancesFromCardIds(cardDeck.CardIds, npc.ID);

        // Get token count for this NPC and connection type
        int tokenCount = _tokenManager.GetTokenCount(request.ConnectionType, npc.ID);

        // Add signature cards based on token count (not thresholds)
        int signatureCardCount = GetSignatureCardCountByTokens(tokenCount);
        List<CardInstance> signatureInstances = GetSignatureCards(npc, signatureCardCount);
        deckInstances.AddRange(signatureInstances);

        // Create session deck
        SessionCardDeck deck = SessionCardDeck.CreateFromInstances(deckInstances, sessionId);

        // Add observation cards if provided
        if (observationCards != null && observationCards.Any())
        {
            foreach (CardInstance card in observationCards)
            {
                deck.AddCard(card);
            }
        }

        // Process request cards
        List<CardInstance> requestCardInstances = CreateRequestCardInstances(request, npc);

        // Add request cards to deck's request pile
        foreach (CardInstance requestCard in requestCardInstances)
        {
            deck.AddRequestCard(requestCard);
        }

        // Process promise cards and add them to the main deck
        List<CardInstance> promiseCardInstances = CreatePromiseCardInstances(request, npc);
        foreach (CardInstance promiseCard in promiseCardInstances)
        {
            deck.AddCard(promiseCard); // Promise cards go in main deck for shuffling
        }

        // Shuffle the deck after all cards have been added
        deck.ShuffleDrawPile();

        // Return deck with empty request cards (they're in the deck's request pile)
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Create instances from card IDs in the conversation type's deck
    /// </summary>
    private List<CardInstance> CreateInstancesFromCardIds(List<string> cardIds, string ownerId)
    {
        List<CardInstance> instances = new List<CardInstance>();

        foreach (string cardId in cardIds)
        {
            if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard cardTemplate))
            {
                CardInstance instance = new CardInstance(cardTemplate, ownerId);
                instances.Add(instance);
            }
            else
            {
                Console.WriteLine($"[ConversationDeckBuilder] Warning: Card ID '{cardId}' not found in AllCardDefinitions");
            }
        }

        return instances;
    }

    /// <summary>
    /// Get signature card count based on token count (NO THRESHOLDS)
    /// </summary>
    private int GetSignatureCardCountByTokens(int tokens)
    {
        return tokens switch
        {
            0 => 0,
            <= 2 => 1,
            <= 5 => 2,
            <= 9 => 3,
            <= 14 => 4,
            _ => 5
        };
    }

    /// <summary>
    /// Get NPC signature cards up to the specified count
    /// </summary>
    private List<CardInstance> GetSignatureCards(NPC npc, int count)
    {
        // NPCs no longer have progression decks - signature cards removed
        return new List<CardInstance>();
    }

    /// <summary>
    /// Create request card instances with proper rapport thresholds
    /// </summary>
    private List<CardInstance> CreateRequestCardInstances(NPCRequest request, NPC npc)
    {
        List<CardInstance> requestCards = new List<CardInstance>();

        foreach (string requestCardId in request.RequestCardIds)
        {
            if (!_gameWorld.AllCardDefinitions.TryGetValue(requestCardId, out ConversationCard requestCard))
            {
                Console.WriteLine($"[ConversationDeckBuilder] Warning: Request card ID '{requestCardId}' not found");
                continue;
            }

            // Create BurdenGoal type card for requests
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
                CardType = CardType.BurdenGoal // Mark as BurdenGoal
            };

            CardInstance instance = new CardInstance(burdenGoalTemplate, npc.ID);

            // Store context for rapport threshold and request tracking
            instance.Context = new CardContext
            {
                RapportThreshold = requestCard.RapportThreshold,
                RequestId = request.Id
            };

            // Request cards start as unplayable until rapport threshold is met
            instance.IsPlayable = false;

            requestCards.Add(instance);
        }

        return requestCards;
    }

    /// <summary>
    /// Create promise card instances
    /// </summary>
    private List<CardInstance> CreatePromiseCardInstances(NPCRequest request, NPC npc)
    {
        List<CardInstance> promiseCards = new List<CardInstance>();

        foreach (string promiseCardId in request.PromiseCardIds)
        {
            if (!_gameWorld.AllCardDefinitions.TryGetValue(promiseCardId, out ConversationCard promiseCard))
            {
                Console.WriteLine($"[ConversationDeckBuilder] Warning: Promise card ID '{promiseCardId}' not found");
                continue;
            }

            CardInstance instance = new CardInstance(promiseCard, npc.ID);
            promiseCards.Add(instance);
        }

        return promiseCards;
    }

    /// <summary>
    /// Create a fallback conversation type for backward compatibility
    /// </summary>
    private ConversationTypeDefinition CreateFallbackConversationType()
    {
        return new ConversationTypeDefinition
        {
            Id = "fallback_friendly",
            Name = "Friendly Chat",
            Description = "A casual conversation",
            DeckId = "deck_friendly_balanced",
            Category = "social",
            AttentionCost = 1
        };
    }
}