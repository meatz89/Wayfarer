using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class FleetingCardTests
    {
        [Test]
        public void TestFleetingCardsRemovedAfterSpeak()
        {
            // Arrange
            var handManager = new HandManager();
            var persistentCard = new ConversationCard 
            { 
                Id = "persistent_1",
                Name = "Persistent Card",
                Properties = new List<CardProperty> { CardProperty.Persistent }
            };
            var fleetingCard = new ConversationCard 
            { 
                Id = "fleeting_1",
                Name = "Fleeting Card",
                Properties = new List<CardProperty> { CardProperty.Fleeting }
            };
            
            var deck = new List<ConversationCard> { persistentCard, fleetingCard };
            handManager.InitializeDeck(deck);
            handManager.DrawCards(2); // Draw both cards
            
            // Act - SPEAK action (play the persistent card)
            bool continueConversation = handManager.OnSpeakAction(persistentCard);
            
            // Assert
            Assert.IsTrue(continueConversation, "Conversation should continue");
            Assert.AreEqual(0, handManager.CurrentHand.Count, "Hand should be empty after SPEAK");
            Assert.IsFalse(handManager.CurrentHand.Contains(fleetingCard), "Fleeting card should be removed");
            Assert.IsFalse(handManager.CurrentHand.Contains(persistentCard), "Played card should be removed");
        }
        
        [Test]
        public void TestFleetingCardsPreservedOnListen()
        {
            // Arrange
            var handManager = new HandManager();
            var fleetingCard = new ConversationCard 
            { 
                Id = "fleeting_1",
                Name = "Fleeting Card",
                Properties = new List<CardProperty> { CardProperty.Fleeting }
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { fleetingCard });
            handManager.DrawCards(1);
            
            // Act - LISTEN action
            handManager.OnListenAction();
            
            // Assert
            Assert.IsTrue(handManager.CurrentHand.Contains(fleetingCard), 
                "Fleeting card should be preserved on LISTEN");
            Assert.AreEqual(1, handManager.CountFleetingCards(), 
                "Fleeting card count should remain 1");
        }
        
        [Test]
        public void TestGoalCardExhaustsOnUnplayedAction()
        {
            // Arrange
            var handManager = new HandManager();
            var goalCard = new ConversationCard 
            { 
                Id = "goal_1",
                Name = "Goal Card",
                Properties = new List<CardProperty> { CardProperty.Fleeting, CardProperty.Opportunity },
                ExhaustEffect = new CardEffect { Type = CardEffectType.EndConversation }
            };
            var otherCard = new ConversationCard 
            { 
                Id = "other_1",
                Name = "Other Card",
                Properties = new List<CardProperty> { CardProperty.Persistent }
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { goalCard, otherCard });
            handManager.DrawCards(2);
            
            // Act - Play the other card, goal should exhaust with EndConversation effect
            bool continueConversation = handManager.OnSpeakAction(otherCard);
            
            // Assert
            Assert.IsFalse(continueConversation, 
                "Conversation should fail due to goal exhaust effect");
            Assert.AreEqual(0, handManager.CurrentHand.Count, 
                "Hand should be cleared after goal exhaust");
        }
        
        [Test]
        public void TestMultipleFleetingCardsAllRemoved()
        {
            // Arrange
            var handManager = new HandManager();
            var cards = new List<ConversationCard>
            {
                new ConversationCard { Id = "fleeting_1", Properties = new List<CardProperty> { CardProperty.Fleeting } },
                new ConversationCard { Id = "fleeting_2", Properties = new List<CardProperty> { CardProperty.Fleeting } },
                new ConversationCard { Id = "fleeting_3", Properties = new List<CardProperty> { CardProperty.Fleeting } },
                new ConversationCard { Id = "persistent_1", Properties = new List<CardProperty> { CardProperty.Persistent } }
            };
            
            handManager.InitializeDeck(cards);
            handManager.DrawCards(4);
            
            // Act - Play the persistent card
            var persistentCard = handManager.CurrentHand.First(c => !c.IsFleeting);
            handManager.OnSpeakAction(persistentCard);
            
            // Assert
            Assert.AreEqual(0, handManager.CurrentHand.Count, 
                "All cards should be removed (played + fleeting)");
            Assert.AreEqual(0, handManager.CountFleetingCards(), 
                "No fleeting cards should remain");
        }
        
        [Test]
        public void TestFleetingDistribution()
        {
            // This test validates that approximately 25% of cards are fleeting
            var deck = new List<ConversationCard>();
            
            // Create a sample deck
            for (int i = 0; i < 20; i++)
            {
                deck.Add(new ConversationCard
                {
                    Id = $"card_{i}",
                    Name = $"Card {i}",
                    Properties = i % 4 == 0 
                        ? new List<CardProperty> { CardProperty.Fleeting }
                        : new List<CardProperty> { CardProperty.Persistent }
                });
            }
            
            int fleetingCount = deck.Count(c => c.IsFleeting);
            float percentage = (float)fleetingCount / deck.Count;
            
            // Assert - Should be approximately 25% fleeting (20-30% range)
            Assert.IsTrue(percentage >= 0.20f && percentage <= 0.30f,
                $"Fleeting distribution {percentage:P} should be between 20-30%");
        }
        
        [Test]
        public void TestPlayingFleetingCardRemovesItself()
        {
            // Arrange
            var handManager = new HandManager();
            var fleetingCard = new ConversationCard 
            { 
                Id = "fleeting_1",
                Name = "Fleeting Card",
                Properties = new List<CardProperty> { CardProperty.Fleeting }
            };
            var otherFleetingCard = new ConversationCard 
            { 
                Id = "fleeting_2",
                Name = "Other Fleeting",
                Properties = new List<CardProperty> { CardProperty.Fleeting }
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { fleetingCard, otherFleetingCard });
            handManager.DrawCards(2);
            
            // Act - Play one fleeting card
            handManager.OnSpeakAction(fleetingCard);
            
            // Assert
            Assert.AreEqual(0, handManager.CurrentHand.Count, 
                "All fleeting cards should be removed including the played one");
            Assert.AreEqual(2, handManager.DiscardPile.Count,
                "Both fleeting cards should be in discard");
        }
        
        [Test]
        public void TestFleetingCardWithoutExhaustEffectDoesNotFailConversation()
        {
            // Arrange
            var handManager = new HandManager();
            var fleetingCard = new ConversationCard 
            { 
                Id = "fleeting_1",
                Name = "Fleeting Card",
                Properties = new List<CardProperty> { CardProperty.Fleeting },
                ExhaustEffect = CardEffect.None // No exhaust effect
            };
            var otherCard = new ConversationCard 
            { 
                Id = "other_1",
                Name = "Other Card",
                Properties = new List<CardProperty> { CardProperty.Persistent }
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { fleetingCard, otherCard });
            handManager.DrawCards(2);
            
            // Act - Play the other card, fleeting exhausts with no effect
            bool continueConversation = handManager.OnSpeakAction(otherCard);
            
            // Assert
            Assert.IsTrue(continueConversation, 
                "Conversation should continue since exhaust has no effect");
            Assert.AreEqual(0, handManager.CurrentHand.Count,
                "All cards should be removed (played + exhausted)");
        }
    }
}