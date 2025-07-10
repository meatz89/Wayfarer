using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Test-driven development for Contract System Time Pressure enhancements.
    /// These tests validate the enhanced time pressure mechanics that create strategic
    /// decision-making around contract acceptance, timing, and completion rewards.
    /// 
    /// User Story: Contract Time Pressure Enhancement
    /// Priority: HIGH - Core strategic gameplay mechanic
    /// Game Design Goal: Create structured goals with cascading consequences
    /// </summary>
    public class ContractTimePressureTests
    {
        /// <summary>
        /// Test that accepting a contract consumes a time block
        /// Acceptance Criteria: Contract acceptance takes time, forcing strategic decisions
        /// </summary>
        [Fact]
        public void AcceptingContract_Should_Consume_TimeBlock()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            
            TimeManager timeManager = gameWorld.TimeManager;
            int initialTimeBlocks = timeManager.RemainingTimeBlocks;
            
            // Act
            bool accepted = contractSystem.AcceptContract(contract);
            
            // Assert
            Assert.True(accepted, "Contract should be accepted successfully");
            Assert.Equal(initialTimeBlocks - 1, timeManager.RemainingTimeBlocks);
            Assert.Contains(contract, gameWorld.ActiveContracts);
        }

        /// <summary>
        /// Test that completing a contract consumes a time block
        /// Acceptance Criteria: Contract completion takes time, adding to delivery pressure
        /// </summary>
        [Fact]
        public void CompletingContract_Should_Consume_TimeBlock()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            
            // Set up player at destination with required items
            Player player = gameWorld.GetPlayer();
            player.Inventory.AddItem("herbs");
            gameWorld.WorldState.SetCurrentLocation(new Location(contract.DestinationLocation, "Test Location"), null);
            gameWorld.ActiveContracts.Add(contract);
            
            TimeManager timeManager = gameWorld.TimeManager;
            int initialTimeBlocks = timeManager.RemainingTimeBlocks;
            
            // Act
            bool completed = contractSystem.CompleteContract(contract);
            
            // Assert
            Assert.True(completed, "Contract should be completed successfully");
            Assert.Equal(initialTimeBlocks - 1, timeManager.RemainingTimeBlocks);
            Assert.DoesNotContain(contract, gameWorld.ActiveContracts);
        }

        /// <summary>
        /// Test that early delivery provides reputation benefit (not payment bonus)
        /// Acceptance Criteria: Players rewarded for efficient planning through reputation
        /// </summary>
        [Fact]
        public void EarlyDelivery_Should_Provide_ReputationBonus()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5, payment: 10);
            
            // Complete contract 2 days early (day 3 instead of day 5)
            gameWorld.WorldState.CurrentDay = 3;
            Player player = gameWorld.GetPlayer();
            player.Inventory.AddItem("herbs");
            gameWorld.WorldState.SetCurrentLocation(new Location(contract.DestinationLocation, "Test Location"), null);
            gameWorld.ActiveContracts.Add(contract);
            
            int initialCoins = player.Coins;
            int initialReputation = player.Reputation;
            
            // Act
            bool completed = contractSystem.CompleteContract(contract);
            
            // Assert
            Assert.True(completed, "Contract should be completed successfully");
            
            // Emergent design: Payment is fixed, reputation improves for future opportunities
            Assert.Equal(initialCoins + contract.Payment, player.Coins); // No bonus, just contract payment
            Assert.Equal(initialReputation + 1, player.Reputation); // +1 reputation for on-time/early delivery
        }

        /// <summary>
        /// Test that late delivery reduces reputation (not payment)
        /// Acceptance Criteria: Failure consequences create meaningful risk through reputation
        /// </summary>
        [Fact]
        public void LateDelivery_Should_Reduce_Reputation()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5, payment: 10);
            
            // Complete contract 1 day late (day 6 instead of day 5)
            gameWorld.WorldState.CurrentDay = 6;
            Player player = gameWorld.GetPlayer();
            player.Inventory.AddItem("herbs");
            gameWorld.WorldState.SetCurrentLocation(new Location(contract.DestinationLocation, "Test Location"), null);
            gameWorld.ActiveContracts.Add(contract);
            
            int initialCoins = player.Coins;
            int initialReputation = player.Reputation;
            
            // Act
            bool completed = contractSystem.CompleteContract(contract);
            
            // Assert
            Assert.True(completed, "Late contracts can still be completed");
            
            // Emergent design: Payment is fixed, reputation suffers for future opportunities
            Assert.Equal(initialCoins + contract.Payment, player.Coins); // Full payment still received
            Assert.Equal(initialReputation - 1, player.Reputation); // -1 reputation per day late
        }

        /// <summary>
        /// Test that contracts cannot be accepted without sufficient time blocks
        /// Acceptance Criteria: Time block constraints prevent last-minute contract loading
        /// </summary>
        [Fact]
        public void ContractAcceptance_Should_Fail_Without_TimeBlocks()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Contract contract = CreateTestContract(startDay: 1, dueDay: 5);
            
            // Exhaust all time blocks
            TimeManager timeManager = gameWorld.TimeManager;
            while (timeManager.RemainingTimeBlocks > 0)
            {
                timeManager.ConsumeTimeBlock(1);
            }
            
            // Act
            bool accepted = contractSystem.AcceptContract(contract);
            
            // Assert
            Assert.False(accepted, "Cannot accept contract without time blocks");
            Assert.DoesNotContain(contract, gameWorld.ActiveContracts);
        }

        /// <summary>
        /// Test that contract acceptance has time window restrictions
        /// Acceptance Criteria: Some contracts only available during specific time windows
        /// </summary>
        [Fact]
        public void Contract_Should_Respect_TimeWindow_Restrictions()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            
            // Contract only available during morning time block
            Contract morningContract = CreateTestContract(startDay: 1, dueDay: 5);
            morningContract.AvailableTimeBlocks = new List<TimeBlocks> { TimeBlocks.Morning };
            
            // Act & Assert - Should work during morning
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Morning;
            Assert.True(morningContract.IsAvailable(1, TimeBlocks.Morning));
            
            // Act & Assert - Should fail during afternoon
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Afternoon;
            Assert.False(morningContract.IsAvailable(1, TimeBlocks.Afternoon));
        }

        /// <summary>
        /// Test that reputation affects contract availability and payment
        /// Acceptance Criteria: Failed contracts reduce reputation, affecting future opportunities
        /// </summary>
        [Fact]
        public void FailedContracts_Should_Affect_Reputation_And_FutureContracts()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            Player player = gameWorld.GetPlayer();
            
            Contract contract = CreateTestContract(startDay: 1, dueDay: 2);
            gameWorld.ActiveContracts.Add(contract);
            
            int initialReputation = player.Reputation;
            
            // Act - Let contract expire
            gameWorld.WorldState.CurrentDay = 3; // Past due date
            contractSystem.CheckForFailedContracts();
            
            // Assert
            Assert.True(contract.IsFailed, "Contract should be marked as failed");
            Assert.True(player.Reputation < initialReputation, "Reputation should decrease after failed contract");
        }

        /// <summary>
        /// Test that difficulty scaling affects contract time pressure
        /// Acceptance Criteria: Later contracts have tighter deadlines for increased challenge
        /// </summary>
        [Fact]
        public void Contract_Difficulty_Should_Scale_WithProgression()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem());
            
            // Early game contract (day 5)
            gameWorld.WorldState.CurrentDay = 5;
            Contract earlyContract = contractSystem.GenerateContract();
            
            // Late game contract (day 20)
            gameWorld.WorldState.CurrentDay = 20;
            Contract lateContract = contractSystem.GenerateContract();
            
            // Assert
            int earlyDuration = earlyContract.DueDay - earlyContract.StartDay;
            int lateDuration = lateContract.DueDay - lateContract.StartDay;
            
            Assert.True(lateDuration < earlyDuration, "Later contracts should have tighter deadlines");
            Assert.True(lateContract.Payment > earlyContract.Payment, "Later contracts should offer higher rewards");
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test game world with proper initialization
        /// </summary>
        private static GameWorld CreateTestGameWorld()
        {
            GameWorld gameWorld = new GameWorld();
            Player player = gameWorld.GetPlayer();
            player.Initialize("TestPlayer", Professions.Merchant, Genders.Male);
            
            // Set up basic world state
            gameWorld.WorldState.CurrentDay = 1;
            gameWorld.WorldState.CurrentTimeWindow = TimeBlocks.Morning;
            gameWorld.WorldState.SetCurrentLocation(new Location("town_square", "Town Square"), null);
            
            return gameWorld;
        }

        /// <summary>
        /// Creates a test contract with specified parameters
        /// </summary>
        private static Contract CreateTestContract(int startDay, int dueDay, int payment = 10)
        {
            return new Contract
            {
                Id = "test_contract",
                Description = "Test delivery contract",
                RequiredItems = new List<string> { "herbs" },
                DestinationLocation = "dusty_flagon",
                StartDay = startDay,
                DueDay = dueDay,
                Payment = payment,
                FailurePenalty = "Test penalty",
                IsCompleted = false,
                IsFailed = false
            };
        }

        #endregion
    }
}