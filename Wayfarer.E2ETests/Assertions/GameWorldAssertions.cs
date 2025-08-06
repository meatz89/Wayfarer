using System;
using System.Linq;

namespace Wayfarer.E2ETests.Assertions
{
    /// <summary>
    /// Assertion helpers for validating GameWorld state in tests.
    /// </summary>
    public static class GameWorldAssertions
    {
        /// <summary>
        /// Asserts that the player is at the specified location and spot.
        /// </summary>
        public static void AssertPlayerAt(GameWorld world, string locationId, string spotId)
        {
            Player player = world.Player;
            if (player.CurrentLocationId != locationId || player.CurrentSpotId != spotId)
            {
                throw new AssertionException(
                    $"Expected player at {locationId}/{spotId}, but was at {player.CurrentLocationId}/{player.CurrentSpotId}");
            }
        }
        
        /// <summary>
        /// Asserts that the player has a specific letter in their queue.
        /// </summary>
        public static void AssertHasLetter(GameWorld world, string letterId)
        {
            bool hasLetter = world.LetterQueueManager.PlayerQueue.Letters
                .Any(l => l.ID == letterId);
                
            if (!hasLetter)
            {
                string letterIds = string.Join(", ", 
                    world.LetterQueueManager.PlayerQueue.Letters.Select(l => l.ID));
                throw new AssertionException(
                    $"Expected letter '{letterId}' in queue, but queue contains: [{letterIds}]");
            }
        }
        
        /// <summary>
        /// Asserts that the player does not have a specific letter.
        /// </summary>
        public static void AssertDoesNotHaveLetter(GameWorld world, string letterId)
        {
            bool hasLetter = world.LetterQueueManager.PlayerQueue.Letters
                .Any(l => l.ID == letterId);
                
            if (hasLetter)
            {
                throw new AssertionException(
                    $"Expected letter '{letterId}' NOT in queue, but it was found");
            }
        }
        
        /// <summary>
        /// Asserts the player's token count with a specific NPC.
        /// </summary>
        public static void AssertTokenCount(GameWorld world, string npcId, TokenType tokenType, int expected)
        {
            int actual = world.ConnectionTokenManager.GetTokenCount(npcId, tokenType);
            if (actual != expected)
            {
                throw new AssertionException(
                    $"Expected {expected} {tokenType} tokens with {npcId}, but had {actual}");
            }
        }
        
        /// <summary>
        /// Asserts the player's coin count.
        /// </summary>
        public static void AssertCoins(GameWorld world, int expected)
        {
            int actual = world.Player.Coins;
            if (actual != expected)
            {
                throw new AssertionException(
                    $"Expected {expected} coins, but had {actual}");
            }
        }
        
        /// <summary>
        /// Asserts the player's stamina.
        /// </summary>
        public static void AssertStamina(GameWorld world, int expected)
        {
            int actual = world.Player.Stamina;
            if (actual != expected)
            {
                throw new AssertionException(
                    $"Expected {expected} stamina, but had {actual}");
            }
        }
        
        /// <summary>
        /// Asserts the current time.
        /// </summary>
        public static void AssertTime(GameWorld world, int expectedDay, int expectedHour)
        {
            TimeManager time = world.TimeManager;
            if (time.CurrentDay != expectedDay || time.CurrentHour != expectedHour)
            {
                throw new AssertionException(
                    $"Expected Day {expectedDay} Hour {expectedHour}, but was Day {time.CurrentDay} Hour {time.CurrentHour}");
            }
        }
        
        /// <summary>
        /// Asserts that a conversation is active.
        /// </summary>
        public static void AssertConversationActive(ConversationStateManager convState)
        {
            if (!convState.ConversationPending || convState.PendingConversationManager == null)
            {
                throw new AssertionException("Expected active conversation, but none found");
            }
        }
        
        /// <summary>
        /// Asserts that no conversation is active.
        /// </summary>
        public static void AssertNoConversation(ConversationStateManager convState)
        {
            if (convState.ConversationPending || convState.PendingConversationManager != null)
            {
                throw new AssertionException("Expected no conversation, but one is active");
            }
        }
        
        /// <summary>
        /// Asserts the letter queue size.
        /// </summary>
        public static void AssertQueueSize(GameWorld world, int expected)
        {
            int actual = world.LetterQueueManager.PlayerQueue.Letters.Count;
            if (actual != expected)
            {
                throw new AssertionException(
                    $"Expected {expected} letters in queue, but had {actual}");
            }
        }
    }
    
    /// <summary>
    /// Custom exception for assertion failures.
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}