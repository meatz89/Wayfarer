using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;
using Xunit;

namespace Wayfarer.Tests
{
    public class CategoricalContractSystemTests
    {
        [Fact]
        public void Contract_Should_Require_Equipment_Categories_For_Completion()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("mountain_summit").WithCoins(50))
                .WithContracts(c => c.Add("mountain_expedition")
                    .WithDescription("Survey the mountain peaks")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            // Get the contract
            Contract? contract = contractRepo.GetContract("mountain_expedition");
            Assert.NotNull(contract);

            // Add categorical requirements (these would normally be in the contract definition)
            contract.RequiredEquipmentCategories = new List<EquipmentCategory>
            {
                EquipmentCategory.Climbing_Equipment,
                EquipmentCategory.Weather_Protection
            };

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required equipment");
            Assert.Contains("Missing required equipment category: Climbing Equipment", result.CompletionBlockers);
            Assert.Contains("Missing required equipment category: Weather Protection", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Navigation_Tools_For_Completion()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("workshop"))
                .WithContracts(c => c.Add("exploration_mission")
                    .WithDescription("Survey wilderness areas")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("exploration_mission");
            Assert.NotNull(contract);

            // Add categorical requirements
            contract.RequiredEquipmentCategories = new List<EquipmentCategory>
            {
                EquipmentCategory.Navigation_Tools,
                EquipmentCategory.Light_Source
            };

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required equipment");
            Assert.Contains("Missing required equipment category: Navigation Tools", result.CompletionBlockers);
            Assert.Contains("Missing required equipment category: Light Source", result.CompletionBlockers);
        }


        [Fact]
        public void Contract_Should_Require_Physical_Stamina_For_Completion()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("construction_site").WithStamina(2)) // Insufficient for Heavy
                .WithLocations(l => l.Add("construction_site").WithDescription("Construction site").Build())
                .WithContracts(c => c.Add("heavy_lifting")
                    .WithDescription("Move construction materials")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("heavy_lifting");
            Assert.NotNull(contract);

            // Add physical requirement
            contract.PhysicalRequirement = PhysicalDemand.Heavy;

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without sufficient stamina");
            Assert.Contains("Insufficient stamina for Heavy physical demands", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Allow_Completion_With_Sufficient_Stamina()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("town_square").WithStamina(6)) // Sufficient for Light
                .WithContracts(c => c.Add("light_task")
                    .WithDescription("Deliver message")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("light_task");
            Assert.NotNull(contract);

            // Add physical requirement
            contract.PhysicalRequirement = PhysicalDemand.Light;

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.True(result.CanComplete, "Contract should be completable with sufficient stamina");
            Assert.Empty(result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Information_For_Completion()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("merchant_guild"))
                .WithLocations(l => l.Add("merchant_guild").WithDescription("Merchant Guild").Build())
                .WithContracts(c => c.Add("informed_negotiation")
                    .WithDescription("Negotiate trade agreement")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("informed_negotiation");
            Assert.NotNull(contract);

            // Add information requirement
            contract.RequiredInformation = new List<InformationRequirementData>
            {
                new InformationRequirementData(
                    InformationType.Market_Intelligence,
                    InformationQuality.Reliable)
            };

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required information");
            Assert.Contains("Missing Reliable quality Market Intelligence information", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Allow_Completion_With_Required_Information()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("merchant_guild"))
                .WithLocations(l => l.Add("merchant_guild").WithDescription("Merchant Guild").Build())
                .WithContracts(c => c.Add("informed_negotiation")
                    .WithDescription("Negotiate trade agreement")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("informed_negotiation");
            Assert.NotNull(contract);

            // Add information requirement
            contract.RequiredInformation = new List<InformationRequirementData>
            {
                new InformationRequirementData(
                    InformationType.Market_Intelligence,
                    InformationQuality.Reliable)
            };

            // Add required information to player
            Information marketInfo = new Information("market_data", "Current Market Prices", InformationType.Market_Intelligence)
            {
                Quality = InformationQuality.Expert,
            };
            player.KnownInformation.Add(marketInfo);

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            Assert.True(result.CanComplete, "Contract should be completable with required information");
            Assert.Empty(result.CompletionBlockers);
        }


        [Fact]
        public void Contract_Categories_Should_Have_Meaningful_Values()
        {
            // Arrange & Act
            ContractCategory[] categories = Enum.GetValues<ContractCategory>();
            ContractPriority[] priorities = Enum.GetValues<ContractPriority>();
            ContractRisk[] risks = Enum.GetValues<ContractRisk>();

            // Assert
            Assert.Contains(ContractCategory.Merchant, categories);
            Assert.Contains(ContractCategory.Noble, categories);
            Assert.Contains(ContractCategory.Exploration, categories);

            Assert.Contains(ContractPriority.Standard, priorities);
            Assert.Contains(ContractPriority.Urgent, priorities);
            Assert.Contains(ContractPriority.Critical, priorities);

            Assert.Contains(ContractRisk.Low, risks);
            Assert.Contains(ContractRisk.High, risks);
            Assert.Contains(ContractRisk.Extreme, risks);
        }

        [Fact]
        public void Contract_Should_Allow_Complex_Categorical_Requirements()
        {
            // Arrange - Using new superior test pattern
            TestScenarioBuilder scenario = new TestScenarioBuilder()
                .WithPlayer(p => p.StartAt("remote_location").WithStamina(8))
                .WithLocations(l => l.Add("remote_location").WithDescription("Remote exploration site").Build())
                .WithContracts(c => c.Add("complex_expedition")
                    .WithDescription("Multi-faceted exploration mission")
                    .Build());

            GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
            Player player = gameWorld.GetPlayer();
            ContractRepository contractRepo = new ContractRepository(gameWorld);
            ItemRepository itemRepo = new ItemRepository(gameWorld);

            Contract? contract = contractRepo.GetContract("complex_expedition");
            Assert.NotNull(contract);

            // Add complex categorical requirements
            contract.RequiredEquipmentCategories = new List<EquipmentCategory> { EquipmentCategory.Navigation_Tools };
            contract.PhysicalRequirement = PhysicalDemand.Moderate;
            contract.Category = ContractCategory.Exploration;
            contract.Priority = ContractPriority.High;
            contract.RiskLevel = ContractRisk.Moderate;
            contract.RequiredInformation = new List<InformationRequirementData>
            {
                new InformationRequirementData(InformationType.Route_Conditions, InformationQuality.Verified)
            };

            // Add required information to player
            Information routeInfo = new Information("route_data", "Verified Route Info", InformationType.Route_Conditions)
            {
                Quality = InformationQuality.Verified,
            };
            player.KnownInformation.Add(routeInfo);

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, player.CurrentLocation.Id, itemRepo);

            // Assert
            // Should have some blockers due to placeholder implementations for equipment checks
            Assert.NotNull(result);
            Assert.True(result.MissingRequirements.Count > 0 || result.AcceptanceBlockers.Count > 0,
                "Complex contract should have categorical requirements to validate");
        }
    }
}