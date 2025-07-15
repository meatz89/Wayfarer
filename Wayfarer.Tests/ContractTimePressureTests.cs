using Wayfarer.Game.MainSystem;

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
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .DueInDays(4)
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);

            TimeManager timeManager = gameWorld.TimeManager;
            int initialTimeBlocks = timeManager.RemainingTimeBlocks;

            // Act
            bool accepted = contractSystem.AcceptContract(contract);

            // Assert
            Assert.True(accepted, "Contract should be accepted successfully");
            Assert.Equal(initialTimeBlocks - 1, timeManager.RemainingTimeBlocks);
            List<Contract> activeContracts = contractRepository.GetActiveContracts();
            Assert.Contains(contract, activeContracts);
        }

        /// <summary>
        /// Test that completing a contract consumes a time block
        /// Acceptance Criteria: Contract completion takes time, adding to delivery pressure
        /// </summary>
        [Fact]
        public void CompletingContract_Should_Consume_TimeBlock()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithItem("herbs"))
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);
            ContractProgressionService progressionService = new ContractProgressionService(contractRepository, itemRepository, locationRepository, gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, progressionService, new NPCRepository(gameWorld), locationRepository);

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);

            // Accept the contract first
            contractRepository.AddActiveContract(contract);
            Player player = gameWorld.GetPlayer();
            player.DiscoverContract(contract.Id);

            TimeManager timeManager = gameWorld.TimeManager;
            int initialTimeBlocks = timeManager.RemainingTimeBlocks;

            // Act - Simulate selling herbs which should complete the contract
            progressionService.CheckMarketProgression("herbs", "dusty_flagon", TransactionType.Sell, 1, 5, player);

            // Manually call completion (normally triggered by progression service)
            bool completed = contractSystem.CompleteContract(contract);

            // Assert
            Assert.True(completed, "Contract should be completed successfully");
            Assert.Equal(initialTimeBlocks - 1, timeManager.RemainingTimeBlocks);
            List<Contract> activeContracts = contractRepository.GetActiveContracts();
            Assert.DoesNotContain(contract, activeContracts);
        }

        /// <summary>
        /// Test that early delivery provides reputation benefit (not payment bonus)
        /// Acceptance Criteria: Players rewarded for efficient planning through reputation
        /// </summary>
        [Fact]
        public void EarlyDelivery_Should_Provide_ReputationBonus()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithItem("herbs").WithCoins(0).WithReputation(5))
                .WithTimeState(t => t.Day(3))
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .Pays(10)
                    .DueInDays(2) // Due on day 5 (start day 1 + 4 days)
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);
            ItemRepository itemRepository2 = new ItemRepository(gameWorld);
            LocationRepository locationRepository2 = new LocationRepository(gameWorld);
            ContractProgressionService progressionService = new ContractProgressionService(contractRepository, itemRepository2, locationRepository2, gameWorld);

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);
            contract.StartDay = 1;
            contract.DueDay = 5;

            contractRepository.AddActiveContract(contract);
            Player player = gameWorld.GetPlayer();
            player.DiscoverContract(contract.Id);

            int initialCoins = player.Coins;
            int initialReputation = player.Reputation;

            // Act - Complete contract early (day 3 instead of day 5)
            progressionService.CheckMarketProgression("herbs", "dusty_flagon", TransactionType.Sell, 1, 5, player);
            bool completed = contractSystem.CompleteContract(contract);

            // Assert
            Assert.True(completed, "Contract should be completed successfully");
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
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("dusty_flagon").WithItem("herbs").WithCoins(0).WithReputation(5))
                .WithTimeState(t => t.Day(6)) // Late delivery
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .Pays(10)
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);
            ItemRepository itemRepository2 = new ItemRepository(gameWorld);
            LocationRepository locationRepository2 = new LocationRepository(gameWorld);
            ContractProgressionService progressionService = new ContractProgressionService(contractRepository, itemRepository2, locationRepository2, gameWorld);

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);
            contract.StartDay = 1;
            contract.DueDay = 5; // Due yesterday

            contractRepository.AddActiveContract(contract);
            Player player = gameWorld.GetPlayer();
            player.DiscoverContract(contract.Id);

            int initialCoins = player.Coins;
            int initialReputation = player.Reputation;

            // Act - Complete contract late
            progressionService.CheckMarketProgression("herbs", "dusty_flagon", TransactionType.Sell, 1, 5, player);
            bool completed = contractSystem.CompleteContract(contract);

            // Assert
            Assert.True(completed, "Late contracts can still be completed");
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
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);

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
            List<Contract> activeContracts = contractRepository.GetActiveContracts();
            Assert.DoesNotContain(contract, activeContracts);
        }

        /// <summary>
        /// Test that contract acceptance has time window restrictions
        /// Acceptance Criteria: Some contracts only available during specific time windows
        /// </summary>
        [Fact]
        public void Contract_Should_Respect_TimeWindow_Restrictions()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.TimeBlock(TimeBlocks.Morning))
                .WithContracts(c => c.Add("morning_contract")
                    .WithDescription("Morning only contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);

            Contract? contract = contractRepository.GetContract("morning_contract");
            Assert.NotNull(contract);

            // Contract only available during morning time block
            contract.AvailableTimeBlocks = new List<TimeBlocks> { TimeBlocks.Morning };

            // Act & Assert - Should work during morning
            Assert.True(contract.IsAvailable(1, TimeBlocks.Morning));

            // Act & Assert - Should fail during afternoon
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Afternoon;
            Assert.False(contract.IsAvailable(1, TimeBlocks.Afternoon));
        }

        /// <summary>
        /// Test that reputation affects contract availability and payment
        /// Acceptance Criteria: Failed contracts reduce reputation, affecting future opportunities
        /// </summary>
        [Fact]
        public void FailedContracts_Should_Affect_Reputation_And_FutureContracts()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithReputation(5))
                .WithTimeState(t => t.Day(1))
                .WithContracts(c => c.Add("test_contract")
                    .WithDescription("Test delivery contract")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);
            Player player = gameWorld.GetPlayer();

            Contract? contract = contractRepository.GetContract("test_contract");
            Assert.NotNull(contract);
            contract.StartDay = 1;
            contract.DueDay = 2;

            contractRepository.AddActiveContract(contract);
            player.DiscoverContract(contract.Id);

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
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square"))
                .WithTimeState(t => t.Day(5));

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractSystem contractSystem = new ContractSystem(gameWorld, new MessageSystem(), contractRepository, locationRepository, itemRepository);

            // Early game contract (day 5)
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
    }
}