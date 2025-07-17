using System.Linq;
using Xunit;
using Wayfarer.GameState;
using Wayfarer.Content;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests.GameState
{
    public class LetterQueueManagerTests
    {
        private GameWorld CreateTestGameWorld()
        {
            var gameWorld = new GameWorld();
            // The GameWorld constructor already creates a player with an initialized LetterQueue
            var player = gameWorld.GetPlayer();
            player.Name = "TestPlayer";
            
            return gameWorld;
        }

        [Fact]
        public void RemoveLetterFromQueue_ShiftsLettersUpCorrectly()
        {
            // Arrange
            var gameWorld = CreateTestGameWorld();
            var letterTemplateRepo = new LetterTemplateRepository(gameWorld);
            var npcRepo = new NPCRepository(gameWorld);
            var messageSystem = new MessageSystem();
            var obligationManager = new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepo);
            var manager = new LetterQueueManager(gameWorld, letterTemplateRepo, npcRepo, messageSystem, obligationManager);

            // Add letters to positions 1, 3, 5, and 7
            var letter1 = new Letter { SenderName = "A", RecipientName = "B", Deadline = 5, Payment = 10, TokenType = ConnectionType.Trust };
            var letter3 = new Letter { SenderName = "C", RecipientName = "D", Deadline = 4, Payment = 12, TokenType = ConnectionType.Trade };
            var letter5 = new Letter { SenderName = "E", RecipientName = "F", Deadline = 3, Payment = 15, TokenType = ConnectionType.Noble };
            var letter7 = new Letter { SenderName = "G", RecipientName = "H", Deadline = 2, Payment = 20, TokenType = ConnectionType.Shadow };

            manager.AddLetterToQueue(letter1, 1);
            manager.AddLetterToQueue(letter3, 3);
            manager.AddLetterToQueue(letter5, 5);
            manager.AddLetterToQueue(letter7, 7);

            // Act - Remove letter from position 3
            bool result = manager.RemoveLetterFromQueue(3);

            // Assert
            Assert.True(result);
            
            // Letter 1 should still be in position 1
            Assert.Equal(letter1, manager.GetLetterAt(1));
            Assert.Equal(1, letter1.QueuePosition);
            
            // Position 2 should still be empty
            Assert.Null(manager.GetLetterAt(2));
            
            // Letter 5 should have moved to position 3
            Assert.Equal(letter5, manager.GetLetterAt(3));
            Assert.Equal(3, letter5.QueuePosition);
            
            // Letter 7 should have moved to position 4
            Assert.Equal(letter7, manager.GetLetterAt(4));
            Assert.Equal(4, letter7.QueuePosition);
            
            // Positions 5, 6, 7, and 8 should be empty
            Assert.Null(manager.GetLetterAt(5));
            Assert.Null(manager.GetLetterAt(6));
            Assert.Null(manager.GetLetterAt(7));
            Assert.Null(manager.GetLetterAt(8));
        }

        [Fact]
        public void RemoveLetterFromQueue_FromPosition1_ShiftsAllLettersUp()
        {
            // Arrange
            var gameWorld = CreateTestGameWorld();
            var letterTemplateRepo = new LetterTemplateRepository(gameWorld);
            var npcRepo = new NPCRepository(gameWorld);
            var messageSystem = new MessageSystem();
            var obligationManager = new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepo);
            var manager = new LetterQueueManager(gameWorld, letterTemplateRepo, npcRepo, messageSystem, obligationManager);

            // Fill positions 1-4
            var letters = new Letter[4];
            for (int i = 0; i < 4; i++)
            {
                letters[i] = new Letter 
                { 
                    SenderName = $"Sender{i}", 
                    RecipientName = $"Recipient{i}", 
                    Deadline = 5 - i, 
                    Payment = 10 + i, 
                    TokenType = ConnectionType.Trust 
                };
                manager.AddLetterToQueue(letters[i], i + 1);
            }

            // Act - Remove letter from position 1
            bool result = manager.RemoveLetterFromQueue(1);

            // Assert
            Assert.True(result);
            
            // All letters should have shifted up by one
            for (int i = 0; i < 3; i++)
            {
                var letter = manager.GetLetterAt(i + 1);
                Assert.Equal(letters[i + 1], letter);
                Assert.Equal(i + 1, letter.QueuePosition);
            }
            
            // Remaining positions should be empty
            for (int i = 4; i <= 8; i++)
            {
                Assert.Null(manager.GetLetterAt(i));
            }
        }

        [Fact]
        public void RemoveLetterFromQueue_LastPosition_NoShiftingNeeded()
        {
            // Arrange
            var gameWorld = CreateTestGameWorld();
            var letterTemplateRepo = new LetterTemplateRepository(gameWorld);
            var npcRepo = new NPCRepository(gameWorld);
            var messageSystem = new MessageSystem();
            var obligationManager = new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepo);
            var manager = new LetterQueueManager(gameWorld, letterTemplateRepo, npcRepo, messageSystem, obligationManager);

            // Add letters to positions 6, 7, and 8
            var letter6 = new Letter { SenderName = "A", RecipientName = "B", Deadline = 3, Payment = 10, TokenType = ConnectionType.Trust };
            var letter7 = new Letter { SenderName = "C", RecipientName = "D", Deadline = 2, Payment = 12, TokenType = ConnectionType.Trade };
            var letter8 = new Letter { SenderName = "E", RecipientName = "F", Deadline = 1, Payment = 15, TokenType = ConnectionType.Noble };

            manager.AddLetterToQueue(letter6, 6);
            manager.AddLetterToQueue(letter7, 7);
            manager.AddLetterToQueue(letter8, 8);

            // Act - Remove letter from position 8
            bool result = manager.RemoveLetterFromQueue(8);

            // Assert
            Assert.True(result);
            
            // Letters 6 and 7 should remain in their positions
            Assert.Equal(letter6, manager.GetLetterAt(6));
            Assert.Equal(6, letter6.QueuePosition);
            
            Assert.Equal(letter7, manager.GetLetterAt(7));
            Assert.Equal(7, letter7.QueuePosition);
            
            // Position 8 should now be empty
            Assert.Null(manager.GetLetterAt(8));
        }

        [Fact]
        public void ProcessDailyDeadlines_RemovesExpiredLetters_AndShiftsQueue()
        {
            // Arrange
            var gameWorld = CreateTestGameWorld();
            var letterTemplateRepo = new LetterTemplateRepository(gameWorld);
            var npcRepo = new NPCRepository(gameWorld);
            var messageSystem = new MessageSystem();
            var obligationManager = new StandingObligationManager(gameWorld, messageSystem, letterTemplateRepo);
            var manager = new LetterQueueManager(gameWorld, letterTemplateRepo, npcRepo, messageSystem, obligationManager);

            // Add letters with different deadlines
            var letter1 = new Letter { SenderName = "A", RecipientName = "B", Deadline = 1, Payment = 10, TokenType = ConnectionType.Trust };
            var letter2 = new Letter { SenderName = "C", RecipientName = "D", Deadline = 3, Payment = 12, TokenType = ConnectionType.Trade };
            var letter3 = new Letter { SenderName = "E", RecipientName = "F", Deadline = 1, Payment = 15, TokenType = ConnectionType.Noble };
            var letter4 = new Letter { SenderName = "G", RecipientName = "H", Deadline = 2, Payment = 20, TokenType = ConnectionType.Shadow };

            manager.AddLetterToQueue(letter1, 1);
            manager.AddLetterToQueue(letter2, 2);
            manager.AddLetterToQueue(letter3, 3);
            manager.AddLetterToQueue(letter4, 4);

            // Act - Process daily deadlines (will decrement all deadlines by 1)
            manager.ProcessDailyDeadlines();

            // Assert
            // Letters 1 and 3 should have been removed (deadline was 1, now 0)
            // Letter 2 should now be in position 1 with deadline 2
            Assert.Equal(letter2, manager.GetLetterAt(1));
            Assert.Equal(1, letter2.QueuePosition);
            Assert.Equal(2, letter2.Deadline);
            
            // Letter 4 should now be in position 2 with deadline 1
            Assert.Equal(letter4, manager.GetLetterAt(2));
            Assert.Equal(2, letter4.QueuePosition);
            Assert.Equal(1, letter4.Deadline);
            
            // Remaining positions should be empty
            for (int i = 3; i <= 8; i++)
            {
                Assert.Null(manager.GetLetterAt(i));
            }
        }
    }
}