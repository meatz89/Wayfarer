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
                IsFleeting = false 
            };
            var fleetingCard = new ConversationCard 
            { 
                Id = "fleeting_1",
                Name = "Fleeting Card",
                IsFleeting = true 
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
                IsFleeting = true 
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
        public void TestFinalWordTriggersOnUnplayedGoal()
        {
            // Arrange
            var handManager = new HandManager();
            var goalCard = new ConversationCard 
            { 
                Id = "goal_1",
                Name = "Goal Card",
                HasFinalWord = true, 
                IsFleeting = true 
            };
            var otherCard = new ConversationCard 
            { 
                Id = "other_1",
                Name = "Other Card",
                IsFleeting = false 
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { goalCard, otherCard });
            handManager.DrawCards(2);
            
            // Act - Play the other card, not the goal
            bool continueConversation = handManager.OnSpeakAction(otherCard);
            
            // Assert
            Assert.IsFalse(continueConversation, 
                "Conversation should fail due to Final Word not being played");
            Assert.AreEqual(0, handManager.CurrentHand.Count, 
                "Hand should be cleared after Final Word failure");
        }
        
        [Test]
        public void TestMultipleFleetingCardsAllRemoved()
        {
            // Arrange
            var handManager = new HandManager();
            var cards = new List<ConversationCard>
            {
                new ConversationCard { Id = "fleeting_1", IsFleeting = true },
                new ConversationCard { Id = "fleeting_2", IsFleeting = true },
                new ConversationCard { Id = "fleeting_3", IsFleeting = true },
                new ConversationCard { Id = "persistent_1", IsFleeting = false }
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
                    IsFleeting = i % 4 == 0 // 25% fleeting
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
                IsFleeting = true 
            };
            var otherFleetingCard = new ConversationCard 
            { 
                Id = "fleeting_2",
                Name = "Other Fleeting",
                IsFleeting = true 
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
        public void TestGoalWithoutFinalWordDoesNotFailConversation()
        {
            // Arrange
            var handManager = new HandManager();
            var goalCard = new ConversationCard 
            { 
                Id = "goal_1",
                Name = "Goal Card",
                HasFinalWord = false, // No Final Word
                IsFleeting = true 
            };
            var otherCard = new ConversationCard 
            { 
                Id = "other_1",
                Name = "Other Card",
                IsFleeting = false 
            };
            
            handManager.InitializeDeck(new List<ConversationCard> { goalCard, otherCard });
            handManager.DrawCards(2);
            
            // Act - Play the other card
            bool continueConversation = handManager.OnSpeakAction(otherCard);
            
            // Assert
            Assert.IsTrue(continueConversation, 
                "Conversation should continue since goal has no Final Word");
            Assert.AreEqual(0, handManager.CurrentHand.Count,
                "Only fleeting cards should be removed");
        }
    }
}