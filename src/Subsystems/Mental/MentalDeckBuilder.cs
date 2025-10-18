using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Returns (deck to draw from, starting hand with knowledge cards)
    /// </summary>
    public (List<CardInstance> deck, List<CardInstance> startingHand) BuildDeckWithStartingHand(MentalChallengeDeck challengeDeck, Player player)
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

        return (deck, startingHand);
    }

    /// <summary>
    /// Create goal card instances for Mental challenges
    /// Goal cards are self-contained templates - no lookup required
    /// Goal cards start unplayable until Progress threshold met
    /// </summary>
    private List<CardInstance> CreateGoalCardInstances(Goal goal)
    {
        List<CardInstance> goalCardInstances = new List<CardInstance>();

        foreach (GoalCard goalCard in goal.GoalCards)
        {
            // Create CardInstance directly from GoalCard (self-contained template)
            CardInstance instance = new CardInstance(goalCard);

            // Set context for threshold checking (Mental system uses Progress threshold)
            instance.Context = new CardContext
            {
                threshold = goalCard.threshold,
                RequestId = goal.Id
            };

            // Goal cards start unplayable until threshold met
            instance.IsPlayable = false;

            goalCardInstances.Add(instance);
        }

        return goalCardInstances;
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
