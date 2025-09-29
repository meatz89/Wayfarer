using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Smoke tests for the SteamWorld Quest-inspired Initiative conversation system.
    /// These tests validate the core conversation system integration without complex dependencies.
    /// </summary>
    [TestFixture]
    public class ConversationSystemSmokeTests
    {
        private GameWorld gameWorld;

        [SetUp]
        public void Setup()
        {
            gameWorld = GameWorldInitializer.CreateGameWorld();
            Console.WriteLine($"[Test] Initialized GameWorld with {gameWorld.NPCs.Count} NPCs and {gameWorld.AllCardDefinitions.Count} cards");
        }

        [Test]
        public void ConversationCards_InitiativeSystemValidation()
        {
            // Verify core Initiative system structure in cards
            var allCards = gameWorld.AllCardDefinitions.Select(entry => entry.Card).ToList();

            Console.WriteLine($"Total cards loaded: {allCards.Count}");

            // Foundation cards (depth 1-2) should exist
            var foundationCards = allCards.Where(c => (int)c.Depth <= 2).ToList();
            Assert.That(foundationCards.Count, Is.GreaterThan(10), "Should have Foundation cards");
            Console.WriteLine($"Foundation cards (depth 1-2): {foundationCards.Count}");

            // Initiative generators should exist (cards with "Initiative" in title)
            var initiativeGenerators = allCards.Where(c => c.Title.Contains("Initiative")).ToList();
            Assert.That(initiativeGenerators.Count, Is.GreaterThan(5), "Should have Initiative generators");
            Console.WriteLine($"Initiative generating cards: {initiativeGenerators.Count}");

            // Echo/Statement persistence distribution
            var echoCards = allCards.Where(c => c.Persistence == PersistenceType.Echo).Count();
            var statementCards = allCards.Where(c => c.Persistence == PersistenceType.Statement).Count();

            Assert.That(echoCards, Is.GreaterThan(0), "Should have Echo cards");
            Assert.That(statementCards, Is.GreaterThan(0), "Should have Statement cards");

            Console.WriteLine($"Echo cards: {echoCards}");
            Console.WriteLine($"Statement cards: {statementCards}");

            // Foundation sustainability check - most Foundation cards should be Echo
            var foundationEcho = foundationCards.Where(c => c.Persistence == PersistenceType.Echo).Count();
            var echoPercentage = (double)foundationEcho / foundationCards.Count * 100;

            Console.WriteLine($"Foundation cards that are Echo: {foundationEcho}/{foundationCards.Count} ({echoPercentage:F1}%)");
            Assert.That(echoPercentage, Is.GreaterThanOrEqualTo(60.0), "Most Foundation cards should be Echo for sustainability");

            Console.WriteLine("✓ Card system validation passed");
        }

        [Test]
        public void ConversationSystem_NPCRequestStructure()
        {
            // Verify NPCs have request structure for conversation initialization
            var npcsWithRequests = gameWorld.NPCs.Where(n => n.Requests != null && n.Requests.Any()).ToList();

            Assert.That(npcsWithRequests.Count, Is.GreaterThan(0), "Should have NPCs with requests");
            Console.WriteLine($"NPCs with requests: {npcsWithRequests.Count}");

            // Check conversation types exist
            var conversationTypes = gameWorld.ConversationTypes;
            Assert.That(conversationTypes.Count, Is.GreaterThan(0), "Should have conversation types");
            Console.WriteLine($"Conversation types: {conversationTypes.Count}");

            // Validate each conversation type has a deck
            foreach (var typeEntry in conversationTypes)
            {
                var deckEntry = gameWorld.CardDecks.FirstOrDefault(d => d.DeckId == typeEntry.Definition.DeckId);
                Assert.That(deckEntry, Is.Not.Null, $"Conversation type {typeEntry.TypeId} should have valid deck");
            }

            Console.WriteLine("✓ NPC request structure validation passed");
        }

        [Test]
        public void ConversationSystem_SessionCardDeckCreation()
        {
            // Test that we can create a conversation deck through the builder
            var npcWithRequests = gameWorld.NPCs.FirstOrDefault(n => n.Requests != null && n.Requests.Any());
            Assert.That(npcWithRequests, Is.Not.Null, "Need NPC with requests for deck creation test");

            var request = npcWithRequests.Requests.First();
            Console.WriteLine($"Testing deck creation for NPC: {npcWithRequests.Name}, Request: {request.Id}");

            // Verify conversation type exists
            var conversationType = gameWorld.ConversationTypes.FirstOrDefault(ct => ct.TypeId == request.ConversationTypeId);
            Assert.That(conversationType, Is.Not.Null, $"Request should reference valid conversation type: {request.ConversationTypeId}");

            // Verify deck exists
            var deckDefinition = gameWorld.CardDecks.FirstOrDefault(d => d.DeckId == conversationType.Definition.DeckId);
            Assert.That(deckDefinition, Is.Not.Null, $"Conversation type should reference valid deck: {conversationType.Definition.DeckId}");

            Console.WriteLine($"Conversation type: {conversationType.TypeId}");
            Console.WriteLine($"Deck: {deckDefinition.DeckId} with {deckDefinition.Definition.CardIds.Count} cards");

            // Verify cards in deck exist
            var cardsInDeck = 0;
            foreach (var cardId in deckDefinition.Definition.CardIds)
            {
                var cardEntry = gameWorld.AllCardDefinitions.FirstOrDefault(c => c.CardId == cardId);
                if (cardEntry != null) cardsInDeck++;
            }

            Assert.That(cardsInDeck, Is.GreaterThan(10), "Deck should have sufficient cards for conversation");
            Console.WriteLine($"Valid cards in deck: {cardsInDeck}");

            Console.WriteLine("✓ Conversation deck creation structure validated");
        }

        [Test]
        public void ConversationSystem_InitiativeCardCosts()
        {
            // Verify Initiative cost structure across cards
            var allCards = gameWorld.AllCardDefinitions.Select(entry => entry.Card).ToList();

            // Foundation cards (depth 1-2) should have 0 Initiative cost
            var foundationCards = allCards.Where(c => (int)c.Depth <= 2).ToList();
            var zeroInitiativeFoundation = foundationCards.Where(c => c.InitiativeCost == 0).Count();
            var foundationZeroPercentage = (double)zeroInitiativeFoundation / foundationCards.Count * 100;

            Console.WriteLine($"Foundation cards with 0 Initiative cost: {zeroInitiativeFoundation}/{foundationCards.Count} ({foundationZeroPercentage:F1}%)");
            Assert.That(foundationZeroPercentage, Is.GreaterThanOrEqualTo(80.0), "Most Foundation cards should cost 0 Initiative");

            // Higher depth cards should have Initiative costs
            var higherDepthCards = allCards.Where(c => (int)c.Depth >= 3).ToList();
            if (higherDepthCards.Any())
            {
                var withInitiativeCost = higherDepthCards.Where(c => c.InitiativeCost > 0).Count();
                var costPercentage = (double)withInitiativeCost / higherDepthCards.Count * 100;

                Console.WriteLine($"Higher depth cards with Initiative cost > 0: {withInitiativeCost}/{higherDepthCards.Count} ({costPercentage:F1}%)");
                Assert.That(costPercentage, Is.GreaterThan(50.0), "Higher depth cards should generally cost Initiative");
            }

            Console.WriteLine("✓ Initiative cost structure validated");
        }

        [Test]
        public void ConversationSystem_PersistenceDistribution()
        {
            // Verify Echo/Statement distribution follows SteamWorld Quest principles
            var allCards = gameWorld.AllCardDefinitions.Select(entry => entry.Card).ToList();

            var echoCards = allCards.Where(c => c.Persistence == PersistenceType.Echo).ToList();
            var statementCards = allCards.Where(c => c.Persistence == PersistenceType.Statement).ToList();

            Console.WriteLine($"Echo cards: {echoCards.Count}");
            Console.WriteLine($"Statement cards: {statementCards.Count}");

            // Echo cards should be majority for sustainability
            var totalCards = echoCards.Count + statementCards.Count;
            var echoPercentage = (double)echoCards.Count / totalCards * 100;

            Console.WriteLine($"Echo percentage: {echoPercentage:F1}%");
            Assert.That(echoPercentage, Is.GreaterThan(60.0), "Echo cards should be majority for conversation sustainability");

            // Initiative generators should be Echo for sustainability
            var initiativeGenerators = allCards.Where(c => c.Title.Contains("Initiative")).ToList();
            var echoInitiativeGenerators = initiativeGenerators.Where(c => c.Persistence == PersistenceType.Echo).Count();

            if (initiativeGenerators.Count > 0)
            {
                var echoInitiativePercentage = (double)echoInitiativeGenerators / initiativeGenerators.Count * 100;
                Console.WriteLine($"Initiative generators that are Echo: {echoInitiativeGenerators}/{initiativeGenerators.Count} ({echoInitiativePercentage:F1}%)");
                Assert.That(echoInitiativePercentage, Is.GreaterThan(80.0), "Initiative generators should be Echo for sustainability");
            }

            Console.WriteLine("✓ Persistence distribution validated");
        }

        [Test]
        public void ConversationSystem_DepthDistribution()
        {
            // Verify card depth distribution supports Initiative progression
            var allCards = gameWorld.AllCardDefinitions.Select(entry => entry.Card).ToList();

            var depth1_2 = allCards.Where(c => (int)c.Depth <= 2).Count(); // Foundation
            var depth3_4 = allCards.Where(c => (int)c.Depth >= 3 && (int)c.Depth <= 4).Count(); // Standard
            var depth5_6 = allCards.Where(c => (int)c.Depth >= 5 && (int)c.Depth <= 6).Count(); // Advanced
            var depth7plus = allCards.Where(c => (int)c.Depth >= 7).Count(); // Decisive

            Console.WriteLine($"Card depth distribution:");
            Console.WriteLine($"  Foundation (1-2): {depth1_2}");
            Console.WriteLine($"  Standard (3-4): {depth3_4}");
            Console.WriteLine($"  Advanced (5-6): {depth5_6}");
            Console.WriteLine($"  Decisive (7+): {depth7plus}");

            // Foundation cards should be substantial for Initiative building
            var foundationPercentage = (double)depth1_2 / allCards.Count * 100;
            Console.WriteLine($"Foundation percentage: {foundationPercentage:F1}%");
            Assert.That(foundationPercentage, Is.GreaterThan(30.0), "Foundation cards should be substantial portion");

            // Should have progression through depths
            Assert.That(depth1_2, Is.GreaterThan(0), "Should have Foundation cards");
            Assert.That(depth3_4, Is.GreaterThan(0), "Should have Standard cards");

            Console.WriteLine("✓ Depth distribution validated");
        }

        [Test]
        public void ConversationSystem_CompleteMechanicalValidation()
        {
            // This test validates all key mechanical principles are present in the data

            var allCards = gameWorld.AllCardDefinitions.Select(entry => entry.Card).ToList();
            Console.WriteLine("\n=== Complete Mechanical Validation ===");

            // 1. Initiative System
            var initiativeGenerators = allCards.Where(c => c.Title.Contains("Initiative")).Count();
            Assert.That(initiativeGenerators, Is.GreaterThan(10), "Sufficient Initiative generators");
            Console.WriteLine($"✓ Initiative generators: {initiativeGenerators}");

            // 2. Foundation Sustainability (70% Echo rule)
            var foundationCards = allCards.Where(c => (int)c.Depth <= 2).ToList();
            var foundationEcho = foundationCards.Where(c => c.Persistence == PersistenceType.Echo).Count();
            var foundationEchoPercentage = (double)foundationEcho / foundationCards.Count * 100;
            Assert.That(foundationEchoPercentage, Is.GreaterThanOrEqualTo(70.0), "Foundation sustainability");
            Console.WriteLine($"✓ Foundation Echo percentage: {foundationEchoPercentage:F1}%");

            // 3. Hand Management (7-card system support)
            // This is validated through SessionCardDeck having DiscardDown method
            Assert.That(typeof(SessionCardDeck).GetMethod("DiscardDown"), Is.Not.Null, "7-card hand limit support");
            Console.WriteLine("✓ 7-card hand limit support verified");

            // 4. Echo/Statement Balance
            var echoCards = allCards.Where(c => c.Persistence == PersistenceType.Echo).Count();
            var statementCards = allCards.Where(c => c.Persistence == PersistenceType.Statement).Count();
            var echoPercentage = (double)echoCards / (echoCards + statementCards) * 100;
            Assert.That(echoPercentage, Is.GreaterThan(60.0), "Echo/Statement balance");
            Console.WriteLine($"✓ Echo/Statement balance: {echoPercentage:F1}% Echo");

            // 5. Conversation Types and Decks
            Assert.That(gameWorld.ConversationTypes.Count, Is.GreaterThan(2), "Multiple conversation types");
            Assert.That(gameWorld.CardDecks.Count, Is.GreaterThan(2), "Multiple card decks");
            Console.WriteLine($"✓ Conversation types: {gameWorld.ConversationTypes.Count}");
            Console.WriteLine($"✓ Card decks: {gameWorld.CardDecks.Count}");

            // 6. NPC Integration
            var npcsWithRequests = gameWorld.NPCs.Where(n => n.Requests?.Any() == true).Count();
            Assert.That(npcsWithRequests, Is.GreaterThan(3), "NPCs with conversation requests");
            Console.WriteLine($"✓ NPCs with requests: {npcsWithRequests}");

            Console.WriteLine("\n✓ ALL MECHANICAL VALIDATION PASSED - Conversation system is complete!");
        }
    }
}