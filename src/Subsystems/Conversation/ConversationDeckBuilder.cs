using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds conversation decks from conversation type cards.
/// NO PLAYER DECK - cards come from conversation types defined in JSON.
/// </summary>
public class ConversationDeckBuilder
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;

    public ConversationDeckBuilder(GameWorld gameWorld, TokenMechanicsManager tokenManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    /// <summary>
    /// Create a conversation deck from conversation type cards.
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
        ConversationTypeEntry? typeEntry = _gameWorld.ConversationTypes.FindById(request.ConversationTypeId);
        if (typeEntry == null)
        {
            throw new InvalidOperationException($"[ConversationDeckBuilder] Conversation type '{request.ConversationTypeId}' not found. Request must have valid conversation type.");
        }
        ConversationTypeDefinition conversationType = typeEntry.Definition;

        // Get card deck for this conversation type
        CardDeckDefinitionEntry? deckEntry = _gameWorld.CardDecks.FindById(conversationType.DeckId);
        if (deckEntry == null)
        {
            throw new InvalidOperationException($"[ConversationDeckBuilder] Card deck '{conversationType.DeckId}' not found. Conversation type must reference valid deck.");
        }
        CardDeckDefinition cardDeck = deckEntry.Definition;

        // Create card instances from the conversation type's deck
        List<CardInstance> deckInstances = CreateInstancesFromCardIds(cardDeck.CardIds, npc.ID);

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
        deck.ShuffleDeckPile();

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
            CardDefinitionEntry? cardEntry = _gameWorld.AllCardDefinitions.FindById(cardId);
            if (cardEntry != null)
            {
                CardInstance instance = new CardInstance(cardEntry.Card, ownerId);
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
    /// Create request card instances with proper rapport thresholds
    /// </summary>
    private List<CardInstance> CreateRequestCardInstances(NPCRequest request, NPC npc)
    {
        List<CardInstance> requestCards = new List<CardInstance>();

        foreach (string requestCardId in request.RequestCardIds)
        {
            CardDefinitionEntry? cardEntry = _gameWorld.AllCardDefinitions.FindById(requestCardId);
            if (cardEntry == null)
            {
                Console.WriteLine($"[ConversationDeckBuilder] Warning: Request card ID '{requestCardId}' not found");
                continue;
            }
            ConversationCard requestCard = cardEntry.Card;

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
                DialogueFragment = requestCard.DialogueFragment,
                VerbPhrase = requestCard.VerbPhrase,
                PersonalityTypes = requestCard.PersonalityTypes,
                LevelBonuses = requestCard.LevelBonuses,
                MinimumTokensRequired = requestCard.MinimumTokensRequired,
                MomentumThreshold = requestCard.MomentumThreshold,
                CardType = CardType.Letter
            };

            CardInstance instance = new CardInstance(burdenGoalTemplate, npc.ID);

            // Store context for momentum threshold and request tracking
            instance.Context = new CardContext
            {
                MomentumThreshold = requestCard.MomentumThreshold,
                RequestId = request.Id
            };

            // Request cards start as unplayable until momentum threshold is met
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
            CardDefinitionEntry? cardEntry = _gameWorld.AllCardDefinitions.FindById(promiseCardId);
            if (cardEntry == null)
            {
                Console.WriteLine($"[ConversationDeckBuilder] Warning: Promise card ID '{promiseCardId}' not found");
                continue;
            }
            ConversationCard promiseCard = cardEntry.Card;

            CardInstance instance = new CardInstance(promiseCard, npc.ID);
            promiseCards.Add(instance);
        }

        return promiseCards;
    }

}