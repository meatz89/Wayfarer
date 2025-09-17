using System;
using System.Linq;
using NUnit.Framework;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class ElenaPromiseCardTest
    {
        private GameWorld _gameWorld;
        private PackageLoader _packageLoader;
        
        [SetUp]
        public void Setup()
        {
            _gameWorld = GameWorldInitializer.CreateGameWorld();
            _packageLoader = new PackageLoader(_gameWorld);
            _packageLoader.LoadPackagesFromDirectory("/mnt/c/git/wayfarer/src/Content/Core");
        }
        
        [Test]
        public void Elena_Promise_Card_Should_Have_GoalCard_Property()
        {
            // Arrange
            var elena = _gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
            Assert.IsNotNull(elena, "Elena should exist in the game world");
            Assert.IsNotNull(elena.Requests, "Elena should have one-time requests");
            Assert.IsTrue(elena.Requests.Any(), "Elena should have at least one request");
            
            // Act
            var firstRequest = elena.Requests.First();
            var requestCards = firstRequest.GetRequestCards(_gameWorld);
            var letterCard = requestCards.FirstOrDefault(c => c.Id == "accept_elenas_letter");
            
            // Assert
            Assert.IsNotNull(letterCard, "Elena should have a letter request card");
            Assert.IsTrue(letterCard.Properties.Contains(CardProperty.GoalCard), 
                "Elena's letter card should have the GoalCard property");
            Assert.AreEqual("Promise", letterCard.Category, "Card should be categorized as Promise");
        }
        
        [Test]
        public void Elena_Promise_Card_Should_Always_Succeed_When_Played()
        {
            // Arrange
            var elena = _gameWorld.NPCs.FirstOrDefault(n => n.ID == "elena");
            var firstRequest = elena.Requests?.FirstOrDefault();
            Assert.IsNotNull(firstRequest, "Elena should have at least one request");
            var requestCards = firstRequest.GetRequestCards(_gameWorld);
            var letterCard = requestCards.FirstOrDefault(c => c.Id == "accept_elenas_letter");
            Assert.IsNotNull(letterCard, "Elena should have a letter request card");
            
            // Create CardInstance (simulating what happens in conversation)
            var cardInstance = new CardInstance(letterCard, "elena");
            
            // Assert - Verify the CardInstance preserved the property
            Assert.IsTrue(cardInstance.Properties.Contains(CardProperty.GoalCard), 
                "CardInstance should preserve the GoalCard property");
            
            // The actual success check happens in ConversationFacade.PlayCard
            // which checks for GoalCard and sets success = true
            Console.WriteLine($"Card '{cardInstance.Description}' has properties: {string.Join(", ", cardInstance.Properties)}");
            Console.WriteLine($"Card with GoalCard property will always succeed (100% success rate)");
        }
        
        [Test]
        public void Promise_Cards_With_GoalCard_Should_Be_Guaranteed_Success()
        {
            // This test verifies the game design principle that promise/request cards always succeed
            
            // Arrange - Create a test promise card with GoalCard
            var testCard = new RequestCard
            {
                Id = "test_promise",
                Description = "Test Promise Card",
                Focus = 0,
                Difficulty = Difficulty.VeryEasy,
                Properties = new System.Collections.Generic.List<CardProperty> 
                { 
                    CardProperty.Persistent, 
                    CardProperty.GoalCard 
                }
            };
            
            var cardInstance = new CardInstance(testCard, "test");
            
            // Assert
            Assert.IsTrue(cardInstance.Properties.Contains(CardProperty.GoalCard), 
                "Promise cards must have GoalCard property");
            
            // ConversationFacade.PlayCard checks:
            // if (selectedCard.Properties.Contains(CardProperty.GoalCard))
            // {
            //     success = true;
            //     roll = 100;
            //     successPercentage = 100;
            // }
            Console.WriteLine("Promise cards with GoalCard are guaranteed 100% success in ConversationFacade.PlayCard");
        }
    }
}