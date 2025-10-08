using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds conversation decks from conversation type cards.
/// NO PLAYER DECK - cards come from conversation types defined in JSON.
/// </summary>
public class SocialChallengeDeckBuilder
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;

    public SocialChallengeDeckBuilder(GameWorld gameWorld, TokenMechanicsManager tokenManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    /// <summary>
    /// Create a conversation deck from conversation type cards.
    /// Request drives everything - it determines the conversation type and connection type.
    /// </summary>
    public (SocialSessionCardDeck deck, List<CardInstance> GoalCards) CreateConversationDeck(
        NPC npc,
        string requestId)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Get the request which drives everything
        GoalCard request = npc.GetRequestById(requestId);
        if (request == null)
        {
            throw new ArgumentException($"Request {requestId} not found for NPC {npc.ID}");
        }

        // THREE PARALLEL SYSTEMS: Get Social engagement type and conversation deck
        if (!_gameWorld.SocialChallengeTypes.TryGetValue(request.ChallengeTypeId, out SocialChallengeType challengeType))
        {
            throw new InvalidOperationException($"[ConversationDeckBuilder] Social engagement type '{request.ChallengeTypeId}' not found in GameWorld.SocialChallengeTypes");
        }

        if (!_gameWorld.SocialChallengeDecks.TryGetValue(challengeType.DeckId, out SocialChallengeDeck deckDefinition))
        {
            throw new InvalidOperationException($"[ConversationDeckBuilder] Conversation deck '{challengeType.DeckId}' not found in GameWorld.SocialChallengeDecks");
        }

        // Build card instances from engagement deck (no depth distribution - deck has explicit card list)
        List<CardInstance> deckInstances = deckDefinition.BuildCardInstances(_gameWorld);

        // Equipment category filtering: Remove cards requiring equipment player doesn't have (parallel to Mental/Physical)
        Player player = _gameWorld.GetPlayer();
        List<EquipmentCategory> playerEquipmentCategories = GetPlayerEquipmentCategories(player);
        deckInstances = deckInstances.Where(instance =>
        {
            SocialCard template = instance.SocialCardTemplate;
            if (template == null) return false;

            if (template.EquipmentCategory != EquipmentCategory.None)
            {
                if (!playerEquipmentCategories.Contains(template.EquipmentCategory))
                {
                    return false;
                }
            }

            return true;
        }).ToList();

        // Filter signature cards based on token requirements
        deckInstances = FilterSignatureCardsByTokenRequirements(deckInstances, npc);

        // Create session deck
        SocialSessionCardDeck deck = SocialSessionCardDeck.CreateFromInstances(deckInstances, sessionId);

        // Process request cards
        List<CardInstance> GoalCardInstances = CreateGoalCardInstances(request, npc);

        // Add request cards to deck's request pile
        foreach (CardInstance GoalCard in GoalCardInstances)
        {
            deck.AddGoalCard(GoalCard);
        }

        // Shuffle the deck after all cards have been added
        deck.ShuffleDeckPile();

        // Return deck with empty request cards (they're in the deck's request pile)
        return (deck, new List<CardInstance>());
    }

    /// <summary>
    /// Create request card instances from request goals
    /// </summary>
    private List<CardInstance> CreateGoalCardInstances(GoalCard request, NPC npc)
    {
        List<CardInstance> GoalCards = new List<CardInstance>();

        // Load cards from goals (goals reference cards from _cards.json)
        foreach (NPCRequestGoal goal in request.Goals)
        {
            // Find the card referenced by this goal (CardId references card in _cards.json)
            GoalCard? goalCard = _gameWorld.GoalCards.Where(c => c.Id == goal.CardId).FirstOrDefault();
            if (goalCard == null)
            {
                throw new InvalidOperationException($"[ConversationDeckBuilder] Goal card '{goal.CardId}' not found in AllCardDefinitions. Ensure card is defined in _cards.json and referenced in NPC goal.");
            }

            goalCard.NpcId = npc.ID;

            // Create instance from the goal card
            CardInstance instance = new CardInstance(goalCard);

            // Store context for momentum threshold and request tracking
            instance.Context = new CardContext
            {
                MomentumThreshold = goal.MomentumThreshold,
                RequestId = request.Id
            };

            // Request cards start as unplayable until momentum threshold is met
            instance.IsPlayable = false;

            GoalCards.Add(instance);
            Console.WriteLine($"[ConversationDeckBuilder] Added request card '{goalCard.Title}' (threshold: {goal.MomentumThreshold}) to request pile");
        }

        return GoalCards;
    }

    /// <summary>
    /// Filter signature cards based on token requirements with the NPC
    /// </summary>
    private List<CardInstance> FilterSignatureCardsByTokenRequirements(List<CardInstance> instances, NPC npc)
    {
        List<CardInstance> filteredInstances = new List<CardInstance>();

        foreach (CardInstance instance in instances)
        {
            SocialCard card = instance.SocialCardTemplate;

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

    /// <summary>
    /// Get equipment categories provided by player's current items (parallel to Mental/Physical)
    /// </summary>
    private List<EquipmentCategory> GetPlayerEquipmentCategories(Player player)
    {
        List<EquipmentCategory> categories = new List<EquipmentCategory>();
        foreach (string itemId in player.Inventory.GetAllItems())
        {
            Item item = _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item?.ProvidedEquipmentCategories != null)
            {
                categories.AddRange(item.ProvidedEquipmentCategories);
            }
        }
        return categories.Distinct().ToList();
    }

}