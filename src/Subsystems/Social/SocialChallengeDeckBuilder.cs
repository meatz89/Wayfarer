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
    public SocialDeckBuildResult CreateConversationDeck(
        NPC npc,
        string requestId)
    {
        string sessionId = Guid.NewGuid().ToString();

        // Get the situation which drives everything - from centralized GameWorld storage
        Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations).FirstOrDefault(sit => sit.Id == requestId);
        if (situation == null)
        {
            throw new ArgumentException($"Situation {requestId} not found in GameWorld.Situations");
        }

        // THREE PARALLEL SYSTEMS: Get Social engagement deck directly (no Types, just Decks)
        SocialChallengeDeck deckDefinition = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId);
        if (deckDefinition == null)
        {
            throw new InvalidOperationException($"[ConversationDeckBuilder] Conversation deck '{situation.DeckId}' not found in GameWorld.SocialChallengeDecks");
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

        // Process situation cards (victory conditions)
        List<CardInstance> situationCardInstances = CreateSituationCardInstances(situation, npc);

        // Add situation cards to deck's request pile
        foreach (CardInstance situationCard in situationCardInstances)
        {
            deck.AddSituationCard(situationCard);
        }

        // Shuffle the deck after all cards have been added
        deck.ShuffleDeckPile();

        // Return deck with empty request cards (they're in the deck's request pile)
        return new SocialDeckBuildResult(deck, new List<CardInstance>());
    }

    /// <summary>
    /// Create situation card instances from situation's victory conditions
    /// Situation cards are self-contained templates - no lookup required
    /// </summary>
    private List<CardInstance> CreateSituationCardInstances(Situation situation, NPC npc)
    {
        List<CardInstance> situationCardInstances = new List<CardInstance>();

        foreach (SituationCard situationCard in situation.SituationCards)
        {
            // Create CardInstance directly from SituationCard (self-contained template)
            CardInstance instance = new CardInstance(situationCard);

            // Set context for threshold checking
            instance.Context = new CardContext
            {
                threshold = situationCard.threshold,
                RequestId = situation.Id
            };

            // Situation cards start unplayable until momentum threshold met
            instance.IsPlayable = false;

            situationCardInstances.Add(instance);
        }

        return situationCardInstances;
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
            Item item = _gameWorld.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item?.ProvidedEquipmentCategories != null)
            {
                categories.AddRange(item.ProvidedEquipmentCategories);
            }
        }
        return categories.Distinct().ToList();
    }

}