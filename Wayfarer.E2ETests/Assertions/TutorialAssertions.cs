using System;

namespace Wayfarer.E2ETests.Assertions
{
    /// <summary>
    /// Assertion helpers specific to tutorial state validation.
    /// </summary>
    public static class TutorialAssertions
    {
        /// <summary>
        /// Asserts that the tutorial is active.
        /// </summary>
        public static void AssertTutorialActive(GameWorld world)
        {
            if (world.TutorialState == null || !world.TutorialState.IsActive)
            {
                throw new AssertionException("Expected tutorial to be active, but it was not");
            }
        }
        
        /// <summary>
        /// Asserts that the tutorial is at a specific stage.
        /// </summary>
        public static void AssertTutorialStage(GameWorld world, TutorialStage expectedStage)
        {
            if (world.TutorialState == null)
            {
                throw new AssertionException("Tutorial state is null");
            }
            
            TutorialStage actualStage = world.TutorialState.CurrentStage;
            if (actualStage != expectedStage)
            {
                throw new AssertionException(
                    $"Expected tutorial stage {expectedStage}, but was {actualStage}");
            }
        }
        
        /// <summary>
        /// Asserts that the tutorial is complete.
        /// </summary>
        public static void AssertTutorialComplete(GameWorld world)
        {
            if (world.TutorialState == null || world.TutorialState.IsActive)
            {
                throw new AssertionException("Expected tutorial to be complete, but it was still active");
            }
            
            if (world.TutorialState.CurrentStage != TutorialStage.Complete)
            {
                throw new AssertionException(
                    $"Expected tutorial stage Complete, but was {world.TutorialState.CurrentStage}");
            }
        }
        
        /// <summary>
        /// Asserts that the player has the tutorial starting state.
        /// </summary>
        public static void AssertTutorialStartState(GameWorld world)
        {
            Player player = world.Player;
            
            // Should start with no money
            if (player.Coins != 0)
            {
                throw new AssertionException(
                    $"Tutorial player should start with 0 coins, but had {player.Coins}");
            }
            
            // Should start with full stamina
            if (player.Stamina != player.MaxStamina)
            {
                throw new AssertionException(
                    $"Tutorial player should start with full stamina, but had {player.Stamina}/{player.MaxStamina}");
            }
            
            // Should have no letters
            if (world.LetterQueueManager.PlayerQueue.Letters.Count > 0)
            {
                throw new AssertionException(
                    "Tutorial player should start with no letters");
            }
            
            // Should start at Lower Ward Square
            AssertPlayerAt(world, "lower_ward", "lower_ward_square");
        }
        
        /// <summary>
        /// Asserts that Tam's tutorial letter was received correctly.
        /// </summary>
        public static void AssertReceivedTutorialLetter(GameWorld world)
        {
            // Should have exactly one letter
            AssertQueueSize(world, 1);
            
            // Should be Tam's letter
            Letter letter = world.LetterQueueManager.PlayerQueue.Letters[0];
            if (!letter.ID.Contains("tam") && !letter.ID.Contains("tutorial"))
            {
                throw new AssertionException(
                    $"Expected Tam's tutorial letter, but got '{letter.ID}'");
            }
            
            // Should have payment of 5 coins
            if (letter.Payment != 5)
            {
                throw new AssertionException(
                    $"Tutorial letter should pay 5 coins, but pays {letter.Payment}");
            }
            
            // Should have gained Trust token with Tam
            AssertTokenCount(world, "tam_beggar", TokenType.Trust, 1);
        }
        
        private static void AssertPlayerAt(GameWorld world, string locationId, string spotId)
        {
            GameWorldAssertions.AssertPlayerAt(world, locationId, spotId);
        }
        
        private static void AssertQueueSize(GameWorld world, int expected)
        {
            GameWorldAssertions.AssertQueueSize(world, expected);
        }
        
        private static void AssertTokenCount(GameWorld world, string npcId, TokenType tokenType, int expected)
        {
            GameWorldAssertions.AssertTokenCount(world, npcId, tokenType, expected);
        }
    }
}