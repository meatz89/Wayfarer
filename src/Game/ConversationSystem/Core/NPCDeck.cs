using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.Game.ConversationSystem.Core
{
    /// <summary>
    /// Represents an NPC's personal deck of conversation cards
    /// </summary>
    public class NPCDeck
    {
        public string NPCId { get; set; }
        public List<ConversationCard> Cards { get; set; }
        public DateTime LastModified { get; set; }

        public NPCDeck()
        {
            Cards = new List<ConversationCard>();
            LastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// Factory for creating NPC decks
    /// </summary>
    public class NPCDeckFactory
    {
        private readonly NPCRelationshipTracker relationshipTracker;

        public NPCDeckFactory(NPCRelationshipTracker relationshipTracker)
        {
            this.relationshipTracker = relationshipTracker;
        }

        public NPCDeck CreateDeck(NPC npc)
        {
            var deck = new NPCDeck
            {
                NPCId = npc.ID,
                Cards = new List<ConversationCard>()
            };

            // Add base cards based on personality
            AddPersonalityCards(deck, npc.Personality);

            // Add relationship cards if any
            var relationship = relationshipTracker.GetRelationship(npc.ID);
            if (relationship != null)
            {
                AddRelationshipCards(deck, relationship);
            }

            return deck;
        }

        private void AddPersonalityCards(NPCDeck deck, PersonalityType personality)
        {
            switch (personality)
            {
                case PersonalityType.DEVOTED:
                    deck.Cards.Add(CreateCard("Loyalty", CardType.Trust, 3));
                    deck.Cards.Add(CreateCard("Support", CardType.Trust, 2));
                    break;
                case PersonalityType.MERCANTILE:
                    deck.Cards.Add(CreateCard("Deal", CardType.Commerce, 3));
                    deck.Cards.Add(CreateCard("Trade", CardType.Commerce, 2));
                    break;
                case PersonalityType.PROUD:
                    deck.Cards.Add(CreateCard("Honor", CardType.Status, 3));
                    deck.Cards.Add(CreateCard("Respect", CardType.Status, 2));
                    break;
                case PersonalityType.CUNNING:
                    deck.Cards.Add(CreateCard("Secret", CardType.Shadow, 3));
                    deck.Cards.Add(CreateCard("Scheme", CardType.Shadow, 2));
                    break;
                case PersonalityType.STEADFAST:
                    deck.Cards.Add(CreateCard("Promise", CardType.Trust, 3));
                    deck.Cards.Add(CreateCard("Duty", CardType.Status, 2));
                    break;
            }
        }

        private void AddRelationshipCards(NPCDeck deck, NPCRelationship relationship)
        {
            if (relationship.Trust > 5)
            {
                deck.Cards.Add(CreateCard("Confide", CardType.Trust, 4));
            }
            if (relationship.Commerce > 5)
            {
                deck.Cards.Add(CreateCard("Partnership", CardType.Commerce, 4));
            }
            if (relationship.Status > 5)
            {
                deck.Cards.Add(CreateCard("Alliance", CardType.Status, 4));
            }
            if (relationship.Shadow > 5)
            {
                deck.Cards.Add(CreateCard("Conspiracy", CardType.Shadow, 4));
            }
        }

        private ConversationCard CreateCard(string name, CardType type, int comfort)
        {
            return new ConversationCard
            {
                Name = name,
                Type = type,
                ComfortValue = comfort,
                Persistence = PersistenceType.Reusable,
                Description = $"A {type} card worth {comfort} comfort"
            };
        }
    }

    /// <summary>
    /// Represents connection tokens for NPCs
    /// </summary>
    public class NPCConnectionTokens
    {
        public string NPCId { get; set; }
        public int Trust { get; set; }
        public int Commerce { get; set; }
        public int Status { get; set; }
        public int Shadow { get; set; }

        public int GetTokenCount(ConnectionType type)
        {
            return type switch
            {
                ConnectionType.Trust => Trust,
                ConnectionType.Commerce => Commerce,
                ConnectionType.Status => Status,
                ConnectionType.Shadow => Shadow,
                _ => 0
            };
        }

        public void ModifyTokens(ConnectionType type, int amount)
        {
            switch (type)
            {
                case ConnectionType.Trust:
                    Trust = Math.Max(0, Trust + amount);
                    break;
                case ConnectionType.Commerce:
                    Commerce = Math.Max(0, Commerce + amount);
                    break;
                case ConnectionType.Status:
                    Status = Math.Max(0, Status + amount);
                    break;
                case ConnectionType.Shadow:
                    Shadow = Math.Max(0, Shadow + amount);
                    break;
            }
        }
    }
}