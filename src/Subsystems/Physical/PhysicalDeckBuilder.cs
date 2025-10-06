using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Build deck from engagement type's specific deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns (deck to draw from, starting hand with knowledge and injury cards)
    /// </summary>
    public (List<CardInstance> deck, List<CardInstance> startingHand) BuildDeckWithStartingHand(PhysicalChallengeType challengeType, string locationId, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Get Physical engagement deck from engagement type
        if (!_gameWorld.PhysicalChallengeDecks.TryGetValue(challengeType.DeckId, out PhysicalChallengeDeck deckDefinition))
        {
            throw new InvalidOperationException($"[PhysicalDeckBuilder] Physical engagement deck '{challengeType.DeckId}' not found in GameWorld.PhysicalChallengeDecks");
        }

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
            PhysicalCard injuryTemplate = _gameWorld.PhysicalCards
                .FirstOrDefault(e => e.Card.Id == injuryCardId)?.Card;

            if (injuryTemplate != null)
            {
                CardInstance injuryInstance = new CardInstance
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    PhysicalCardTemplate = injuryTemplate
                };
                deck.Add(injuryInstance);
            }
        }

        // Get location and add signature deck knowledge cards to STARTING HAND
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location?.SignatureDeck != null)
        {
            List<SignatureKnowledgeCard> knowledgeCards = location.SignatureDeck.GetCardsForTacticalType(TacticalSystemType.Physical);
            foreach (SignatureKnowledgeCard sigCard in knowledgeCards)
            {
                PhysicalCard knowledgeTemplate = _gameWorld.PhysicalCards
                    .FirstOrDefault(e => e.Card.Id == sigCard.CardId)?.Card;

                if (knowledgeTemplate != null)
                {
                    CardInstance knowledgeInstance = new CardInstance
                    {
                        InstanceId = Guid.NewGuid().ToString(),
                        PhysicalCardTemplate = knowledgeTemplate
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
