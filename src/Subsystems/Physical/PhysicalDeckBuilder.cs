/// <summary>
/// Builds Physical tactical system decks from engagement types
/// Parallel to ConversationDeckBuilder in Social system
/// </summary>
public class PhysicalDeckBuilder
{
    private readonly GameWorld _gameWorld;

    public PhysicalDeckBuilder(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Build deck from engagement deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns deck to draw from and starting hand with knowledge and injury cards
    /// </summary>
    public PhysicalDeckBuildResult BuildDeckWithStartingHand(
        PhysicalChallengeDeck challengeDeck, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Use Physical engagement deck directly (no lookup needed)
        PhysicalChallengeDeck deckDefinition = challengeDeck;

        // Build card instances from engagement deck (parallel to ConversationDeckBuilder pattern)
        List<CardInstance> deck = deckDefinition.BuildCardInstances(_gameWorld);

        // Equipment category filtering: Remove cards requiring equipment player doesn't have
        List<EquipmentCategory> playerEquipmentCategories = GetPlayerEquipmentCategories(player);
        deck = deck.Where(instance =>
        {
            PhysicalCard template = instance.PhysicalCardTemplate;
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

        // Add injury cards to deck (debuffs from past failures - Physical debt system)
        foreach (string injuryCardId in player.InjuryCardIds)
        {
            PhysicalCard injuryTemplate = null;
            if (injuryTemplate != null)
            {
                CardInstance injuryInstance = new CardInstance(injuryTemplate)
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    PhysicalCardTemplate = injuryTemplate
                };
                deck.Add(injuryInstance);
            }
        }

        return new PhysicalDeckBuildResult(deck, startingHand);
    }

    /// <summary>
    /// Create situation card instances for Physical challenges
    /// Situation cards are self-contained templates - no lookup required
    /// Situation cards start unplayable until Breakthrough threshold met
    /// </summary>
    private List<CardInstance> CreateSituationCardInstances(Situation situation)
    {
        List<CardInstance> situationCardInstances = new List<CardInstance>();

        foreach (SituationCard situationCard in situation.SituationCards)
        {
            // Create CardInstance directly from SituationCard (self-contained template)
            CardInstance instance = new CardInstance(situationCard);

            // Set context for threshold checking (Physical system uses Breakthrough threshold)
            instance.Context = new CardContext
            {
                threshold = situationCard.threshold,
                RequestId = situation.Id
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
