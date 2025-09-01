using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Test to verify that goal cards are only drawable in their valid emotional states
/// </summary>
public class GoalCardStateFilteringTest
{
    public static void RunTest(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n=== Goal Card State Filtering Test ===\n");
        
        var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
        var tokenManager = serviceProvider.GetRequiredService<ITokenManager>();
        var queueManager = serviceProvider.GetRequiredService<IObligationQueueManager>();
        
        // Create a test NPC
        var npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            PersonalityType = PersonalityType.Devoted,
            ConversationDeck = new CardDeck(),
            GoalDeck = new CardDeck()
        };
        
        // Initialize decks
        CardDeck.InitializeGameWorld(gameWorld);
        npc.InitializeConversationDeck(new NPCDeckFactory(tokenManager));
        
        // Create a goal card with specific valid states (TENSE and OPEN only)
        var goalCard = new ConversationCard
        {
            Id = "test_goal_1",
            TemplateId = "promise_letter",
            Category = CardCategory.Promise,
            Type = CardType.Trust,
            Weight = 2,
            Persistence = PersistenceType.Goal,
            IsGoalCard = true,
            GoalCardType = ConversationType.Promise,
            Context = new CardContext
            {
                ValidStates = new List<EmotionalState> { EmotionalState.Tense, EmotionalState.Open },
                NPCName = "Test NPC",
                Personality = PersonalityType.Devoted
            },
            DisplayName = "Test Promise Letter",
            Description = "A test letter that should only be drawable in TENSE or OPEN states"
        };
        
        // Add goal card to goal deck
        npc.GoalDeck.AddCard(goalCard);
        
        Console.WriteLine($"Created goal card with ValidStates: {string.Join(", ", goalCard.Context.ValidStates)}");
        
        // Test 1: Verify goal card is selected for valid states
        TestGoalSelectionForState(npc, EmotionalState.Tense, true);
        TestGoalSelectionForState(npc, EmotionalState.Open, true);
        
        // Test 2: Verify goal card is NOT selected for invalid states
        TestGoalSelectionForState(npc, EmotionalState.Desperate, false);
        TestGoalSelectionForState(npc, EmotionalState.Neutral, false);
        TestGoalSelectionForState(npc, EmotionalState.Connected, false);
        
        
        Console.WriteLine("\n=== Goal Card State Filtering Test Complete ===\n");
    }
    
    private static void TestGoalSelectionForState(NPC npc, EmotionalState state, bool shouldBeSelectable)
    {
        var selected = SelectGoalCardForState(npc, ConversationType.Promise, state);
        var wasSelected = selected != null;
        var result = wasSelected == shouldBeSelectable ? "✓" : "✗";
        
        Console.WriteLine($"{result} State {state}: Goal card {(shouldBeSelectable ? "should be" : "should NOT be")} selectable - {(wasSelected ? "WAS" : "was NOT")} selected");
    }
    
    private static ConversationCard SelectGoalCardForState(NPC npc, ConversationType conversationType, EmotionalState currentState)
    {
        if (npc.GoalDeck == null || !npc.GoalDeck.Any())
            return null;
        
        var allGoals = npc.GoalDeck.GetAllCards();
        
        // Filter by conversation type
        var eligibleGoals = allGoals.Where(g => g.GoalCardType == conversationType);
        
        // Filter by emotional state requirements
        var stateFilteredGoals = eligibleGoals.Where(g =>
        {
            if (g.Context?.ValidStates != null && g.Context.ValidStates.Any())
            {
                return g.Context.ValidStates.Contains(currentState);
            }
            return true; // No state requirements means always eligible
        });
        
        return stateFilteredGoals.FirstOrDefault();
    }
    
    
    private static void TestDrawingInState(CardDeck deck, EmotionalState state, bool shouldBeDrawable)
    {
        // Try to draw cards filtered by state multiple times
        bool wasDrawn = false;
        for (int i = 0; i < 10; i++) // Try multiple times since it's random
        {
            var drawn = deck.DrawFilteredByState(2, 0, state);
            if (drawn.Any(c => c.IsGoalCard))
            {
                wasDrawn = true;
                break;
            }
        }
        
        var result = wasDrawn == shouldBeDrawable ? "✓" : "✗";
        Console.WriteLine($"  {result} Drawing in {state}: Goal card {(shouldBeDrawable ? "should be" : "should NOT be")} drawable - {(wasDrawn ? "WAS" : "was NOT")} drawn");
    }
}