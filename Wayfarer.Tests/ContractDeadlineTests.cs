using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Unit tests for Contract deadline enforcement system.
    /// These tests validate the time-bound delivery constraints that create urgency
    /// and strategic decision-making in the economic simulation.
    /// 
    /// User Story: Contract Time Pressure ()
    /// Priority: MEDIUM - Adds strategic depth to time management
    /// </summary>
    public class ContractDeadlineTests
    {
        /// <summary>
        /// Test that contracts are available within their time window
        /// Acceptance Criteria: Contracts show availability based on start/due dates
        /// </summary>
        [Fact]
        public void Contract_Should_Be_Available_Within_Time_Window()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            
            // Act & Assert - Contract should be available on days 1-5
            Assert.True(contract.IsAvailable(1, TimeBlocks.Morning));
            Assert.True(contract.IsAvailable(3, TimeBlocks.Afternoon));
            Assert.True(contract.IsAvailable(5, TimeBlocks.Evening));
        }

        /// <summary>
        /// Test that contracts are not available before start date
        /// Acceptance Criteria: Contracts cannot be taken before their start date
        /// </summary>
        [Fact]
        public void Contract_Should_Not_Be_Available_Before_Start_Date()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 3, dueDay: 7);
            
            // Act & Assert - Contract should not be available on days 1-2
            Assert.False(contract.IsAvailable(1, TimeBlocks.Morning));
            Assert.False(contract.IsAvailable(2, TimeBlocks.Evening));
        }

        /// <summary>
        /// Test that contracts are not available after due date
        /// Acceptance Criteria: Contracts cannot be taken after their due date
        /// </summary>
        [Fact]
        public void Contract_Should_Not_Be_Available_After_Due_Date()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            
            // Act & Assert - Contract should not be available after day 5
            Assert.False(contract.IsAvailable(6, TimeBlocks.Morning));
            Assert.False(contract.IsAvailable(10, TimeBlocks.Evening));
        }

        /// <summary>
        /// Test that completed contracts are not available
        /// Acceptance Criteria: Completed contracts don't show up in available contracts
        /// </summary>
        [Fact]
        public void Contract_Should_Not_Be_Available_When_Completed()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            contract.IsCompleted = true;
            
            // Act & Assert - Completed contract should not be available even within time window
            Assert.False(contract.IsAvailable(3, TimeBlocks.Morning));
        }

        /// <summary>
        /// Test that failed contracts are not available
        /// Acceptance Criteria: Failed contracts don't show up in available contracts
        /// </summary>
        [Fact]
        public void Contract_Should_Not_Be_Available_When_Failed()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            contract.IsFailed = true;
            
            // Act & Assert - Failed contract should not be available even within time window
            Assert.False(contract.IsAvailable(3, TimeBlocks.Morning));
        }

        /// <summary>
        /// Test that ContractSystem identifies failed contracts correctly
        /// Acceptance Criteria: Contracts automatically fail when deadline passes
        /// </summary>
        [Fact]
        public void ContractSystem_Should_Identify_Failed_Contracts_After_Deadline()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            MessageSystem messageSystem = new MessageSystem();
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, messageSystem, contractRepository);
            
            Contract contract1 = CreateTestContract("contract1", startDay: 1, dueDay: 3);
            Contract contract2 = CreateTestContract("contract2", startDay: 2, dueDay: 5);
            
            gameWorld.ActiveContracts.Add(contract1);
            gameWorld.ActiveContracts.Add(contract2);
            gameWorld.CurrentDay = 4; // Day 4 - contract1 should fail, contract2 still active
            
            // Act
            contractSystem.CheckForFailedContracts();
            
            // Assert
            Assert.True(contract1.IsFailed);
            Assert.False(contract2.IsFailed);
            Assert.Single(gameWorld.ActiveContracts); // Only contract2 should remain
            Assert.Equal(contract2, gameWorld.ActiveContracts.First());
        }

        /// <summary>
        /// Test that contract deadline warnings integrate with time block system
        /// Acceptance Criteria: Player can see how many days remain for each contract
        /// </summary>
        [Fact]
        public void Contract_Deadline_Should_Integrate_With_Time_Block_System()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            TimeManager timeManager = new TimeManager(gameWorld.GetPlayer(), gameWorld.WorldState);
            
            Contract contract = CreateTestContract(startDay: 1, dueDay: 3);
            gameWorld.CurrentDay = 2; // One day before deadline
            
            // Act & Assert
            int daysRemaining = contract.DueDay - gameWorld.CurrentDay;
            Assert.Equal(1, daysRemaining);
            
            // Contract should still be available but urgent
            Assert.True(contract.IsAvailable(gameWorld.CurrentDay, timeManager.GetCurrentTimeWindow()));
        }

        /// <summary>
        /// Test that multiple contracts can have different deadlines
        /// Acceptance Criteria: Player can manage multiple contracts with staggered deadlines
        /// </summary>
        [Fact]
        public void ContractSystem_Should_Handle_Multiple_Contracts_With_Different_Deadlines()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            MessageSystem messageSystem = new MessageSystem();
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, messageSystem, contractRepository);
            
            Contract urgentContract = CreateTestContract("urgent", startDay: 1, dueDay: 2);
            Contract normalContract = CreateTestContract("normal", startDay: 1, dueDay: 5);
            Contract laterContract = CreateTestContract("later", startDay: 3, dueDay: 10);
            
            gameWorld.ActiveContracts.Add(urgentContract);
            gameWorld.ActiveContracts.Add(normalContract);
            gameWorld.ActiveContracts.Add(laterContract);
            gameWorld.CurrentDay = 3; // Day 3
            
            // Act
            contractSystem.CheckForFailedContracts();
            
            // Assert
            Assert.True(urgentContract.IsFailed); // Due day 2, now day 3
            Assert.False(normalContract.IsFailed); // Due day 5, still time
            Assert.False(laterContract.IsFailed); // Due day 10, still time
            Assert.Equal(2, gameWorld.ActiveContracts.Count); // 2 contracts remaining
        }

        /// <summary>
        /// Test that failure penalty messages are generated correctly
        /// Acceptance Criteria: Player receives clear feedback when contracts fail
        /// </summary>
        [Fact]
        public void ContractSystem_Should_Generate_Failure_Messages()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            MessageSystem messageSystem = new MessageSystem();
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, messageSystem, contractRepository);
            
            Contract contract = CreateTestContract("testContract", startDay: 1, dueDay: 2);
            contract.Description = "Deliver goods to Town Square";
            contract.FailurePenalty = "Lost reputation and 10 coin penalty";
            
            gameWorld.ActiveContracts.Add(contract);
            gameWorld.CurrentDay = 3; // Past deadline
            
            // Act
            contractSystem.CheckForFailedContracts();
            
            // Assert
            ActionResultMessages messages = messageSystem.GetAndClearChanges();
            Assert.Single(messages.SystemMessages);
            Assert.Contains("Deliver goods to Town Square", messages.SystemMessages[0].Message);
            Assert.Contains("failed", messages.SystemMessages[0].Message);
            Assert.Contains("Lost reputation and 10 coin penalty", messages.SystemMessages[0].Message);
        }

        /// <summary>
        /// Test that contract deadlines create time pressure for travel decisions
        /// Acceptance Criteria: Players must consider travel time when accepting contracts
        /// </summary>
        [Fact]
        public void Contract_Deadline_Should_Create_Time_Pressure_For_Travel_Planning()
        {
            // Arrange
            Contract contract = CreateTestContract(startDay: 1, dueDay: 3);
            contract.DestinationLocation = "distant_city";
            
            // Simulate player on day 2 with 1 day remaining
            int currentDay = 2;
            int daysRemaining = contract.DueDay - currentDay;
            
            // Act & Assert
            Assert.Equal(1, daysRemaining);
            Assert.True(contract.IsAvailable(currentDay, TimeBlocks.Morning));
            
            // This creates urgency - player must travel and complete in 1 day
            // Integration with time block system means they have 5 time blocks to:
            // 1. Travel to destination (1+ time blocks depending on route)
            // 2. Acquire required items if needed (1+ time blocks)
            // 3. Complete the contract (usually free action)
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test contract with specified parameters
        /// </summary>
        private static Contract CreateTestContract(int startDay = 1, int dueDay = 5)
        {
            return CreateTestContract($"test_contract_{startDay}_{dueDay}", startDay, dueDay);
        }

        /// <summary>
        /// Creates a test contract with specified ID and dates
        /// </summary>
        private static Contract CreateTestContract(string id, int startDay, int dueDay)
        {
            return new Contract
            {
                Id = id,
                Description = $"Test contract {id}",
                StartDay = startDay,
                DueDay = dueDay,
                Payment = 50,
                FailurePenalty = "Test penalty",
                DestinationLocation = "test_location",
                RequiredItems = new List<string>(),
                RequiredLocations = new List<string>()
            };
        }

        /// <summary>
        /// Creates a test game world with basic configuration
        /// </summary>
        private static GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            
            gameWorld.CurrentDay = 1;
            gameWorld.CurrentTimeBlock = TimeBlocks.Morning;
            
            return gameWorld;
        }

        #endregion
    }
}