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
    /// Build deck from engagement type's specific deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns (deck to draw from, starting hand with knowledge cards)
    /// </summary>
    public (List<CardInstance> deck, List<CardInstance> startingHand) BuildDeckWithStartingHand(MentalChallengeType challengeType, string locationId, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Get Mental engagement deck from engagement type
        if (!_gameWorld.MentalChallengeDecks.TryGetValue(challengeType.DeckId, out MentalChallengeDeck deckDefinition))
        {
            throw new InvalidOperationException($"[MentalDeckBuilder] Mental engagement deck '{challengeType.DeckId}' not found in GameWorld.MentalChallengeDecks");
        }

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

        // Get location and add signature deck knowledge cards to STARTING HAND
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location?.SignatureDeck != null)
        {
            List<SignatureKnowledgeCard> knowledgeCards = location.SignatureDeck.GetCardsForTacticalType(TacticalSystemType.Mental);
            foreach (SignatureKnowledgeCard sigCard in knowledgeCards)
            {
                MentalCard knowledgeTemplate = _gameWorld.MentalCards
                    .FirstOrDefault(e => e.Card.Id == sigCard.CardId)?.Card;

                if (knowledgeTemplate != null)
                {
                    CardInstance knowledgeInstance = new CardInstance
                    {
                        InstanceId = Guid.NewGuid().ToString(),
                        MentalCardTemplate = knowledgeTemplate
                    };
                    startingHand.Add(knowledgeInstance);
                }
            }
        }

        return (deck, startingHand);
    }


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
