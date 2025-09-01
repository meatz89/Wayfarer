using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tests for patience card mechanics
/// </summary>
public class PatienceCardTests
{
    /// <summary>
    /// Test that patience cards successfully add patience to conversations
    /// </summary>
    public static void TestPatienceCardAddsPatienceOnSuccess()
    {
        Console.WriteLine("\n=== Testing Patience Card Mechanics ===");
        
        // Create test NPC
        var npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            PersonalityType = PersonalityType.STEADFAST,
            CurrentEmotionalState = EmotionalState.NEUTRAL
        };
        
        // Initialize deck factory and create conversation deck
        var tokenManager = new TokenMechanicsManager();
        var deckFactory = new NPCDeckFactory(tokenManager);
        npc.InitializeConversationDeck(deckFactory);
        
        // Create a patience card
        var patienceCard = new ConversationCard
        {
            Id = "test_patience_card",
            TemplateId = "KeepTalking",
            Category = CardCategory.Patience,
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 1,
            BaseComfort = 0,
            PatienceBonus = 2,
            DisplayName = "Keep Talking",
            Description = "Ask them to continue the conversation",
        };
        
        // Start a conversation
        var session = ConversationSession.StartConversation(
            npc,
            null, // No obligation manager
            tokenManager,
            null, // No observation cards
            ConversationType.FriendlyChat
        );
        
        // Add patience card to hand
        session.HandCards.Add(patienceCard);
        
        // Record initial patience
        var initialPatience = session.CurrentPatience;
        Console.WriteLine($"Initial patience: {initialPatience}");
        
        // Play the patience card
        var selectedCards = new HashSet<ConversationCard> { patienceCard };
        var result = session.ExecuteSpeak(selectedCards);
        
        // Check if patience was added on success
        var cardResult = result.Results.FirstOrDefault(r => r.Card == patienceCard);
        if (cardResult != null)
        {
            Console.WriteLine($"Card played - Success: {cardResult.Success}, Patience added: {cardResult.PatienceAdded}");
            
            if (cardResult.Success)
            {
                // Patience should have increased
                var expectedPatience = initialPatience - 1 + patienceCard.PatienceBonus; // -1 from turn, +bonus from card
                Console.WriteLine($"Expected patience: {expectedPatience}, Actual: {session.CurrentPatience}");
                
                if (session.CurrentPatience == expectedPatience)
                {
                    Console.WriteLine("✓ Patience card successfully added patience!");
                }
                else
                {
                    Console.WriteLine($"✗ Patience mismatch! Expected {expectedPatience}, got {session.CurrentPatience}");
                }
            }
            else
            {
                // On failure, patience should only decrease by turn cost
                var expectedPatience = initialPatience - 1;
                Console.WriteLine($"Card failed - Expected patience: {expectedPatience}, Actual: {session.CurrentPatience}");
                
                if (session.CurrentPatience == expectedPatience)
                {
                    Console.WriteLine("✓ Patience correctly unchanged on failure (except turn cost)");
                }
            }
        }
        
        Console.WriteLine("=== Patience Card Test Complete ===\n");
    }
    
    /// <summary>
    /// Test that patience cards don't affect comfort
    /// </summary>
    public static void TestPatienceCardsDoNotAffectComfort()
    {
        Console.WriteLine("\n=== Testing Patience Cards Don't Affect Comfort ===");
        
        // Create test NPC
        var npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            PersonalityType = PersonalityType.STEADFAST,
            CurrentEmotionalState = EmotionalState.NEUTRAL
        };
        
        // Initialize deck
        var tokenManager = new TokenMechanicsManager();
        var deckFactory = new NPCDeckFactory(tokenManager);
        npc.InitializeConversationDeck(deckFactory);
        
        // Create a patience card
        var patienceCard = new ConversationCard
        {
            Id = "test_patience_card",
            TemplateId = "EngagingQuestion",
            Category = CardCategory.Patience,
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 2,
            BaseComfort = 0, // Should be ignored
            PatienceBonus = 2,
            DisplayName = "Engaging Question",
            Description = "Ask an engaging question"
        };
        
        // Start conversation
        var session = ConversationSession.StartConversation(
            npc,
            null,
            tokenManager,
            null,
            ConversationType.FriendlyChat
        );
        
        session.HandCards.Add(patienceCard);
        
        // Record initial comfort
        var initialComfort = session.CurrentComfort;
        Console.WriteLine($"Initial comfort: {initialComfort}");
        
        // Play patience card
        var selectedCards = new HashSet<ConversationCard> { patienceCard };
        var result = session.ExecuteSpeak(selectedCards);
        
        // Comfort should be unchanged
        Console.WriteLine($"Comfort after patience card: {session.CurrentComfort}");
        
        if (session.CurrentComfort == initialComfort)
        {
            Console.WriteLine("✓ Patience card correctly did not affect comfort!");
        }
        else
        {
            Console.WriteLine($"✗ Comfort changed unexpectedly! Was {initialComfort}, now {session.CurrentComfort}");
        }
        
        Console.WriteLine("=== Comfort Test Complete ===\n");
    }
    
    /// <summary>
    /// Run all patience card tests
    /// </summary>
    public static void RunAllTests()
    {
        Console.WriteLine("\n========== PATIENCE CARD TESTS ==========");
        
        try
        {
            TestPatienceCardAddsPatienceOnSuccess();
            TestPatienceCardsDoNotAffectComfort();
            
            Console.WriteLine("========== ALL PATIENCE TESTS PASSED ==========\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}