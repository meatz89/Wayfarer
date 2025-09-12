using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class LetterDeliveryCardTests
{
    private GameWorld CreateTestWorld()
    {
        var world = new GameWorld();
        
        // Create test NPCs
        var elena = new NPC
        {
            ID = "elena_merchant",
            Name = "Elena",
            PersonalityType = PersonalityType.MERCANTILE,
            SpotId = "market",
            LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trust }
        };
        
        var bertram = new NPC
        {
            ID = "bertram_innkeeper",
            Name = "Bertram",
            PersonalityType = PersonalityType.STEADFAST,
            SpotId = "inn",
            LetterTokenTypes = new List<ConnectionType> { ConnectionType.Commerce }
        };
        
        world.NPCs.Add(elena);
        world.NPCs.Add(bertram);
        
        // Create player
        var player = new Player
        {
            Name = "TestPlayer",
            Coins = 10,
            Health = 100,
            Hunger = 100,
            ObligationQueue = new DeliveryObligation[8]
        };
        world.SetPlayer(player);
        
        return world;
    }
    
    [Fact]
    public void StartConversation_WithLetterForNPC_CreatesDeliveryCard()
    {
        // Arrange
        var world = CreateTestWorld();
        var queueManager = new ObligationQueueManager(
            world, 
            new NPCRepository(world.NPCs),
            new MessageSystem(),
            new StandingObligationManager(new GameConfiguration()),
            new TokenMechanicsManager(world),
            new GameConfiguration(),
            new BasicGameRuleEngine(),
            new TimeManager(world, new MessageSystem())
        );
        
        var tokenManager = new TokenMechanicsManager(world);
        
        // Create a letter from Elena to Bertram
        var letter = new DeliveryObligation
        {
            Id = "test_letter_1",
            SenderId = "elena_merchant",
            SenderName = "Elena",
            RecipientId = "bertram_innkeeper",
            RecipientName = "Bertram",
            TokenType = ConnectionType.Trust,
            DeadlineInSegments = 4, // 4 segments
            Payment = 10,
            EmotionalFocus = EmotionalFocus.MEDIUM
        };
        
        // Add letter to queue
        queueManager.AddObligation(letter);
        
        // Act - Start conversation with Bertram (the recipient)
        var bertram = world.NPCs.First(n => n.ID == "bertram_innkeeper");
        var session = ConversationSession.StartConversation(
            bertram,
            queueManager,
            tokenManager,
            null, // no observation cards
            ConversationType.Standard,
            world.GetPlayerResourceState()
        );
        
        // Assert
        Assert.NotNull(session);
        Assert.NotEmpty(session.HandCards);
        
        // Check for delivery card
        var deliveryCard = session.HandCards.FirstOrDefault(c => c.CanDeliverLetter);
        Assert.NotNull(deliveryCard);
        Assert.Equal("test_letter_1", deliveryCard.DeliveryObligationId);
        Assert.Equal($"Deliver letter from Elena", deliveryCard.DisplayName);
        Assert.Equal(0, deliveryCard.Focus); // Free to play
        Assert.True(deliveryCard.BaseFlow > 0); // Gives flow reward
        Assert.Equal(100, deliveryCard.SuccessRate); // Always succeeds
    }
    
    [Fact]
    public void StartConversation_WithMultipleLetters_CreatesMultipleDeliveryCards()
    {
        // Arrange
        var world = CreateTestWorld();
        var queueManager = new ObligationQueueManager(
            world,
            new NPCRepository(world.NPCs),
            new MessageSystem(),
            new StandingObligationManager(new GameConfiguration()),
            new TokenMechanicsManager(world),
            new GameConfiguration(),
            new BasicGameRuleEngine(),
            new TimeManager(world, new MessageSystem())
        );
        
        var tokenManager = new TokenMechanicsManager(world);
        
        // Create multiple letters for the same recipient
        var letter1 = new DeliveryObligation
        {
            Id = "letter_1",
            SenderId = "elena_merchant",
            SenderName = "Elena",
            RecipientId = "bertram_innkeeper",
            RecipientName = "Bertram",
            TokenType = ConnectionType.Trust,
            DeadlineInSegments = 4, // 4 segments
            Payment = 10,
            EmotionalFocus = EmotionalFocus.LOW
        };
        
        var letter2 = new DeliveryObligation
        {
            Id = "letter_2",
            SenderId = "unknown_sender",
            SenderName = "Unknown",
            RecipientId = "bertram_innkeeper",
            RecipientName = "Bertram",
            TokenType = ConnectionType.Commerce,
            DeadlineInSegments = 2, // 2 segments
            Payment = 20,
            EmotionalFocus = EmotionalFocus.HIGH
        };
        
        queueManager.AddObligation(letter1);
        queueManager.AddObligation(letter2);
        
        // Act
        var bertram = world.NPCs.First(n => n.ID == "bertram_innkeeper");
        var session = ConversationSession.StartConversation(
            bertram,
            queueManager,
            tokenManager,
            null,
            ConversationType.Standard,
            world.GetPlayerResourceState()
        );
        
        // Assert
        var deliveryCards = session.HandCards.Where(c => c.CanDeliverLetter).ToList();
        Assert.Equal(2, deliveryCards.Count);
        
        // Check each card has correct ID and properties
        Assert.Contains(deliveryCards, c => c.DeliveryObligationId == "letter_1");
        Assert.Contains(deliveryCards, c => c.DeliveryObligationId == "letter_2");
        
        // Check flow rewards match importance
        var lowImportanceCard = deliveryCards.First(c => c.DeliveryObligationId == "letter_1");
        var highImportanceCard = deliveryCards.First(c => c.DeliveryObligationId == "letter_2");
        Assert.True(highImportanceCard.BaseFlow > lowImportanceCard.BaseFlow);
    }
    
    [Fact]
    public void StartConversation_WithNoLettersForNPC_NoDeliveryCards()
    {
        // Arrange
        var world = CreateTestWorld();
        var queueManager = new ObligationQueueManager(
            world,
            new NPCRepository(world.NPCs),
            new MessageSystem(),
            new StandingObligationManager(new GameConfiguration()),
            new TokenMechanicsManager(world),
            new GameConfiguration(),
            new BasicGameRuleEngine(),
            new TimeManager(world, new MessageSystem())
        );
        
        var tokenManager = new TokenMechanicsManager(world);
        
        // Create a letter for Elena, not Bertram
        var letter = new DeliveryObligation
        {
            Id = "test_letter",
            SenderId = "bertram_innkeeper",
            SenderName = "Bertram",
            RecipientId = "elena_merchant",
            RecipientName = "Elena",
            TokenType = ConnectionType.Trust,
            DeadlineInSegments = 4, // 4 segments
            Payment = 10
        };
        
        queueManager.AddObligation(letter);
        
        // Act - Start conversation with Bertram (NOT the recipient)
        var bertram = world.NPCs.First(n => n.ID == "bertram_innkeeper");
        var session = ConversationSession.StartConversation(
            bertram,
            queueManager,
            tokenManager,
            null,
            ConversationType.Standard,
            world.GetPlayerResourceState()
        );
        
        // Assert
        var deliveryCards = session.HandCards.Where(c => c.CanDeliverLetter);
        Assert.Empty(deliveryCards);
    }
    
    [Fact]
    public void CrisisConversation_WithLetterForNPC_CreatesDeliveryCard()
    {
        // Arrange
        var world = CreateTestWorld();
        var queueManager = new ObligationQueueManager(
            world,
            new NPCRepository(world.NPCs),
            new MessageSystem(),
            new StandingObligationManager(new GameConfiguration()),
            new TokenMechanicsManager(world),
            new GameConfiguration(),
            new BasicGameRuleEngine(),
            new TimeManager(world, new MessageSystem())
        );
        
        var tokenManager = new TokenMechanicsManager(world);
        
        // Create a letter for Bertram
        var letter = new DeliveryObligation
        {
            Id = "crisis_letter",
            SenderId = "elena_merchant",
            SenderName = "Elena",
            RecipientId = "bertram_innkeeper",
            RecipientName = "Bertram",
            TokenType = ConnectionType.Trust,
            DeadlineInSegments = 2, // 2 segments
            Payment = 15,
            EmotionalFocus = EmotionalFocus.CRITICAL
        };
        
        queueManager.AddObligation(letter);
        
        // Act - Start crisis conversation
        var bertram = world.NPCs.First(n => n.ID == "bertram_innkeeper");
        var session = ConversationSession.StartCrisis(
            bertram,
            queueManager,
            tokenManager,
            null
        );
        
        // Assert
        var deliveryCard = session.HandCards.FirstOrDefault(c => c.CanDeliverLetter);
        Assert.NotNull(deliveryCard);
        Assert.Equal("crisis_letter", deliveryCard.DeliveryObligationId);
        Assert.Equal(10, deliveryCard.BaseFlow); // Critical = 10 flow
    }
}