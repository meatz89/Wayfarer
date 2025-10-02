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

        // Create card instances using depth distribution filtering
        List<CardInstance> deckInstances = CreateInstancesWithDepthDistribution(cardDeck.CardIds, conversationType.Distribution, npc.ID);

        // Filter signature cards based on token requirements
        deckInstances = FilterSignatureCardsByTokenRequirements(deckInstances, npc);

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

        // Shuffle the deck after all cards have been added
        deck.ShuffleDeckPile();

        // Return deck with empty request cards (they're in the deck's request pile)
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Create instances from card IDs using depth distribution filtering
    /// </summary>
    private List<CardInstance> CreateInstancesWithDepthDistribution(List<string> cardIds, DepthDistribution distribution, string ownerId)
    {
        // Get all available cards
        List<ConversationCard> availableCards = new List<ConversationCard>();
        foreach (string cardId in cardIds)
        {
            CardDefinitionEntry? cardEntry = _gameWorld.AllCardDefinitions.FindById(cardId);
            if (cardEntry != null)
            {
                availableCards.Add(cardEntry.Card);
            }
        }

        // Apply depth distribution
        List<ConversationCard> filteredCards = ApplyDepthDistribution(availableCards, distribution);

        // Convert to card instances
        List<CardInstance> instances = new List<CardInstance>();
        foreach (ConversationCard card in filteredCards)
        {
            CardInstance instance = new CardInstance(card, ownerId);
            instances.Add(instance);
        }

        Console.WriteLine($"[ConversationDeckBuilder] Applied depth distribution: {filteredCards.Count} cards selected from {availableCards.Count} available");
        return instances;
    }

    /// <summary>
    /// Apply depth distribution to filter cards
    /// </summary>
    private List<ConversationCard> ApplyDepthDistribution(List<ConversationCard> allCards, DepthDistribution distribution)
    {
        // Group cards by depth ranges
        var foundationCards = allCards.Where(c => (int)c.Depth <= 2).ToList();
        var standardCards = allCards.Where(c => (int)c.Depth >= 3 && (int)c.Depth <= 4).ToList();
        var advancedCards = allCards.Where(c => (int)c.Depth >= 5 && (int)c.Depth <= 6).ToList();
        var decisiveCards = allCards.Where(c => (int)c.Depth >= 7).ToList();

        List<ConversationCard> selectedCards = new List<ConversationCard>();

        // Target deck size (could be configurable)
        int targetDeckSize = 40;

        // Calculate how many cards to select from each category
        int foundationCount = (int)(targetDeckSize * distribution.Foundation);
        int standardCount = (int)(targetDeckSize * distribution.Standard);
        int advancedCount = (int)(targetDeckSize * distribution.Advanced);
        int decisiveCount = (int)(targetDeckSize * distribution.Decisive);

        // Select cards from each category (taking first N cards for deterministic behavior)
        selectedCards.AddRange(foundationCards.Take(foundationCount));
        selectedCards.AddRange(standardCards.Take(standardCount));
        selectedCards.AddRange(advancedCards.Take(advancedCount));
        selectedCards.AddRange(decisiveCards.Take(decisiveCount));

        Console.WriteLine($"[ConversationDeckBuilder] Depth distribution applied: " +
                         $"Foundation:{foundationCount}/{foundationCards.Count}, " +
                         $"Standard:{standardCount}/{standardCards.Count}, " +
                         $"Advanced:{advancedCount}/{advancedCards.Count}, " +
                         $"Decisive:{decisiveCount}/{decisiveCards.Count}");

        return selectedCards;
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
    /// Create request card instances from request goals
    /// </summary>
    private List<CardInstance> CreateRequestCardInstances(NPCRequest request, NPC npc)
    {
        List<CardInstance> requestCards = new List<CardInstance>();

        // Load cards from goals (goals reference cards from 02_cards.json)
        foreach (NPCRequestGoal goal in request.Goals)
        {
            // Find the card referenced by this goal (CardId references card in 02_cards.json)
            CardDefinitionEntry? cardEntry = _gameWorld.AllCardDefinitions.FindById(goal.CardId);
            if (cardEntry == null)
            {
                throw new InvalidOperationException($"[ConversationDeckBuilder] Goal card '{goal.CardId}' not found in AllCardDefinitions. Ensure card is defined in 02_cards.json and referenced in NPC goal.");
            }
            ConversationCard goalCard = cardEntry.Card;

            // Create instance from the goal card
            CardInstance instance = new CardInstance(goalCard, npc.ID);

            // Store context for momentum threshold and request tracking
            instance.Context = new CardContext
            {
                MomentumThreshold = goal.MomentumThreshold,
                RequestId = request.Id
            };

            // Request cards start as unplayable until momentum threshold is met
            instance.IsPlayable = false;

            requestCards.Add(instance);
            Console.WriteLine($"[ConversationDeckBuilder] Added request card '{goalCard.Title}' (threshold: {goal.MomentumThreshold}) to request pile");
        }

        return requestCards;
    }

    /// <summary>
    /// Filter signature cards based on token requirements with the NPC
    /// </summary>
    private List<CardInstance> FilterSignatureCardsByTokenRequirements(List<CardInstance> instances, NPC npc)
    {
        List<CardInstance> filteredInstances = new List<CardInstance>();

        foreach (CardInstance instance in instances)
        {
            ConversationCard card = instance.ConversationCardTemplate;

            // Check if this is a signature card (has token requirements)
            if (card.TokenRequirements != null && card.TokenRequirements.Any())
            {
                // Get player's tokens with this NPC
                Dictionary<string, int> playerTokens = GetPlayerTokensWithNPC(npc);

                // Check if token requirements are met
                if (card.CanAccessWithTokens(playerTokens))
                {
                    filteredInstances.Add(instance);
                }
                // If token requirements not met, exclude this signature card from deck
            }
            else
            {
                // Regular card without token requirements - always include
                filteredInstances.Add(instance);
            }
        }

        return filteredInstances;
    }

    /// <summary>
    /// Get player's current token counts with the specified NPC
    /// </summary>
    private Dictionary<string, int> GetPlayerTokensWithNPC(NPC npc)
    {
        Dictionary<string, int> tokens = new Dictionary<string, int>();

        // Get all connection types and their token counts
        foreach (ConnectionType connectionType in Enum.GetValues<ConnectionType>())
        {
            int tokenCount = _tokenManager.GetTokenCount(connectionType, npc.ID);
            tokens[connectionType.ToString()] = tokenCount;
        }

        return tokens;
    }

}