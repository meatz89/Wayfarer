/// <summary>
/// Builds Mental tactical system decks from engagement types
/// Parallel to ConversationDeckBuilder in Social system
/// </summary>
public class MentalDeckBuilder
{
    private readonly GameWorld _gameWorld;

    public MentalDeckBuilder(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Build deck from engagement deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns deck to draw from and starting hand with knowledge cards
    /// </summary>
    public MentalDeckBuildResult BuildDeckWithStartingHand(MentalChallengeDeck challengeDeck, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Use Mental engagement deck directly (no lookup needed)
        MentalChallengeDeck deckDefinition = challengeDeck;

        // Build card instances from engagement deck (parallel to ConversationDeckBuilder pattern)
        List<CardInstance> deck = deckDefinition.BuildCardInstances(_gameWorld);

        // Equipment category filtering: Remove cards requiring equipment player doesn't have
        List<EquipmentCategory> playerEquipmentCategories = GetPlayerEquipmentCategories(player);
        deck = deck.Where(instance =>
        {
            MentalCard template = instance.MentalCardTemplate;
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

        return new MentalDeckBuildResult(deck, startingHand);
    }

    /// <summary>
    /// Create situation card instances for Mental challenges
    /// Situation cards are self-contained templates - no lookup required
    /// Situation cards start unplayable until Progress threshold met
    /// </summary>
    private List<CardInstance> CreateSituationCardInstances(Situation situation)
    {
        List<CardInstance> situationCardInstances = new List<CardInstance>();

        foreach (SituationCard situationCard in situation.SituationCards)
        {
            // Create CardInstance directly from SituationCard (self-contained template)
            CardInstance instance = new CardInstance(situationCard);

            // Set context for threshold checking (Mental system uses Progress threshold)
            instance.Context = new CardContext
            {
                threshold = situationCard.threshold,
                // HIGHLANDER: Situation has no Id property, use Name as natural key
                RequestId = situation.Name
            };

            // Situation cards start unplayable until threshold met
            instance.IsPlayable = false;

            situationCardInstances.Add(instance);
        }

        return situationCardInstances;
    }

    private List<EquipmentCategory> GetPlayerEquipmentCategories(Player player)
    {
        List<EquipmentCategory> categories = new List<EquipmentCategory>();
        // HIGHLANDER: Inventory.GetAllItems() returns Item objects, not IDs
        foreach (Item item in player.Inventory.GetAllItems())
        {
            if (item == null)
                throw new InvalidOperationException("Null item found in inventory");

            categories.AddRange(item.ProvidedEquipmentCategories);
        }
        return categories.Distinct().ToList();
    }
}
