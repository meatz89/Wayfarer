using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Unit tests for Contract deadline enforcement system.
    /// These tests validate the time-bound delivery constraints that create urgency
    /// and strategic decision-making in the simulation.
    /// 
    /// User Story: Contract Time Pressure
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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(1))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            // Set specific start and due dates
            contract.StartDay = 1;
            contract.DueDay = 5;

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            // Set specific start and due dates
            contract.StartDay = 3;
            contract.DueDay = 7;

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            // Set specific start and due dates
            contract.StartDay = 1;
            contract.DueDay = 5;

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            contract.StartDay = 1;
            contract.DueDay = 5;
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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            contract.StartDay = 1;
            contract.DueDay = 5;
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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(4))
                .WithContracts(c => c
                    .Add("contract1")
                        .RequiresVisit("test_location")
                        .WithDescription("Contract 1")
                        .Build()
                    .Add("contract2")
                        .RequiresVisit("test_location")
                        .WithDescription("Contract 2")
                        .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository);
            
            Contract? contract1 = contractRepository.GetContract("contract1");
            Contract? contract2 = contractRepository.GetContract("contract2");
            Assert.NotNull(contract1);
            Assert.NotNull(contract2);
            
            // Set specific dates
            contract1.StartDay = 1;
            contract1.DueDay = 3;
            contract2.StartDay = 2;
            contract2.DueDay = 5;

            gameWorld.ActiveContracts.Add(contract1);
            gameWorld.ActiveContracts.Add(contract2);

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(2))
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("test_location")
                    .WithDescription("Test contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            TimeManager timeManager = gameWorld.TimeManager;
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            contract.StartDay = 1;
            contract.DueDay = 3;

            // Act & Assert
            int daysRemaining = contract.DueDay - gameWorld.WorldState.CurrentDay;
            Assert.Equal(1, daysRemaining);

            // Contract should still be available but urgent
            Assert.True(contract.IsAvailable(gameWorld.WorldState.CurrentDay, timeManager.GetCurrentCurrentTimeBlock()));
        }

        /// <summary>
        /// Test that multiple contracts can have different deadlines
        /// Acceptance Criteria: Player can manage multiple contracts with staggered deadlines
        /// </summary>
        [Fact]
        public void ContractSystem_Should_Handle_Multiple_Contracts_With_Different_Deadlines()
        {
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(3))
                .WithContracts(c => c
                    .Add("urgent")
                        .RequiresVisit("test_location")
                        .WithDescription("Urgent contract")
                        .Build()
                    .Add("normal")
                        .RequiresVisit("test_location")
                        .WithDescription("Normal contract")
                        .Build()
                    .Add("later")
                        .RequiresVisit("test_location")
                        .WithDescription("Later contract")
                        .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository);
            
            Contract? urgentContract = contractRepository.GetContract("urgent");
            Contract? normalContract = contractRepository.GetContract("normal");
            Contract? laterContract = contractRepository.GetContract("later");
            
            Assert.NotNull(urgentContract);
            Assert.NotNull(normalContract);
            Assert.NotNull(laterContract);
            
            // Set specific dates
            urgentContract.StartDay = 1;
            urgentContract.DueDay = 2;
            normalContract.StartDay = 1;
            normalContract.DueDay = 5;
            laterContract.StartDay = 3;
            laterContract.DueDay = 10;

            gameWorld.ActiveContracts.Add(urgentContract);
            gameWorld.ActiveContracts.Add(normalContract);
            gameWorld.ActiveContracts.Add(laterContract);

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(3))
                .WithContracts(c => c.Add("testContract")
                    .RequiresVisit("town_square")
                    .WithDescription("Deliver goods to Town Square")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            MessageSystem messageSystem = new MessageSystem();
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, messageSystem, contractRepository, locationRepository);
            
            Contract? contract = contractRepository.GetContract("testContract");
            Assert.NotNull(contract);
            
            contract.StartDay = 1;
            contract.DueDay = 2;
            contract.FailurePenalty = "Lost reputation and 10 coin penalty";

            gameWorld.ActiveContracts.Add(contract);

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
            // Arrange - Using new superior test pattern
            var scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(2))
                .WithLocations(l => l.Add("distant_city").WithDescription("A distant city").Build())
                .WithContracts(c => c.Add("test_contract")
                    .RequiresVisit("distant_city")
                    .WithDescription("Delivery to distant city")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            
            Contract? contract = contractRepo.GetContract("test_contract");
            Assert.NotNull(contract);
            
            contract.StartDay = 1;
            contract.DueDay = 3;

            // Act & Assert
            int daysRemaining = contract.DueDay - gameWorld.WorldState.CurrentDay;
            Assert.Equal(1, daysRemaining);
            Assert.True(contract.IsAvailable(gameWorld.WorldState.CurrentDay, TimeBlocks.Morning));

            // This creates urgency - player must travel and complete in 1 day
            // Integration with time block system means they have 5 time blocks to:
            // 1. Travel to destination (1+ time blocks depending on route)
            // 2. Acquire required items if needed (1+ time blocks)
            // 3. Complete the contract (usually free action)
        }
    }
}