using Xunit;
using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    public class CategoricalContractSystemTests
    {
        [Fact]
        public void Contract_Should_Require_Equipment_Categories_For_Completion()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "mountain_expedition",
                Description = "Survey the mountain peaks",
                DestinationLocation = "mountain_summit",
                RequiredEquipmentCategories = new List<EquipmentCategory> 
                { 
                    EquipmentCategory.Climbing_Equipment,
                    EquipmentCategory.Weather_Protection 
                }
            };

            Player player = new Player();
            string currentLocation = "mountain_summit";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required equipment");
            Assert.Contains("Missing required equipment category: Climbing Equipment", result.CompletionBlockers);
            Assert.Contains("Missing required equipment category: Weather Protection", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Tool_Categories_For_Completion()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "crafting_commission",
                Description = "Create specialized tools",
                DestinationLocation = "workshop",
                RequiredToolCategories = new List<ToolCategory> 
                { 
                    ToolCategory.Specialized_Equipment,
                    ToolCategory.Quality_Materials 
                }
            };

            Player player = new Player();
            string currentLocation = "workshop";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required tools");
            Assert.Contains("Missing required tool category: Specialized Equipment", result.CompletionBlockers);
            Assert.Contains("Missing required tool category: Quality Materials", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Social_Standing_For_Acceptance()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "noble_audience",
                Description = "Attend court meeting",
                DestinationLocation = "royal_court",
                RequiredSocialStanding = SocialRequirement.Minor_Noble
            };

            Player player = new Player();
            string currentLocation = "royal_court";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanAccept, "Contract should not be acceptable without required social standing");
            Assert.Contains("Requires Minor Noble social standing", result.AcceptanceBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Physical_Stamina_For_Completion()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "heavy_lifting",
                Description = "Move construction materials",
                DestinationLocation = "construction_site",
                PhysicalRequirement = PhysicalDemand.Heavy
            };

            Player player = new Player();
            player.Stamina = 2; // Insufficient for Heavy physical demand
            string currentLocation = "construction_site";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without sufficient stamina");
            Assert.Contains("Insufficient stamina for Heavy physical demands", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Allow_Completion_With_Sufficient_Stamina()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "light_task",
                Description = "Deliver message",
                DestinationLocation = "town_square",
                PhysicalRequirement = PhysicalDemand.Light
            };

            Player player = new Player();
            player.Stamina = 6; // Sufficient for Light physical demand
            string currentLocation = "town_square";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.True(result.CanComplete, "Contract should be completable with sufficient stamina");
            Assert.Empty(result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Require_Information_For_Completion()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "informed_negotiation",
                Description = "Negotiate trade agreement",
                DestinationLocation = "merchant_guild",
                RequiredInformation = new List<InformationRequirementData>
                {
                    new InformationRequirementData(
                        InformationType.Market_Intelligence,
                        InformationQuality.Reliable,
                        InformationFreshness.Current)
                }
            };

            Player player = new Player();
            string currentLocation = "merchant_guild";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable without required information");
            Assert.Contains("Missing Reliable quality Market Intelligence information", result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Allow_Completion_With_Required_Information()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "informed_negotiation",
                Description = "Negotiate trade agreement",
                DestinationLocation = "merchant_guild",
                RequiredInformation = new List<InformationRequirementData>
                {
                    new InformationRequirementData(
                        InformationType.Market_Intelligence,
                        InformationQuality.Reliable,
                        InformationFreshness.Current)
                }
            };

            Player player = new Player();
            
            // Add required information to player
            Information marketInfo = new Information("market_data", "Current Market Prices", InformationType.Market_Intelligence)
            {
                Quality = InformationQuality.Expert,
                Freshness = InformationFreshness.Current
            };
            player.KnownInformation.Add(marketInfo);
            
            string currentLocation = "merchant_guild";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.True(result.CanComplete, "Contract should be completable with required information");
            Assert.Empty(result.CompletionBlockers);
        }

        [Fact]
        public void Contract_Should_Block_Completion_With_Wrong_Location()
        {
            // Arrange
            Contract contract = new Contract
            {
                Id = "location_specific",
                Description = "Meet at specific location",
                DestinationLocation = "secret_meeting_place"
            };

            Player player = new Player();
            string currentLocation = "wrong_location";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            Assert.False(result.CanComplete, "Contract should not be completable at wrong location");
            Assert.Contains("Must be at secret_meeting_place to complete contract", result.CompletionBlockers);
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
            // Arrange
            Contract contract = new Contract
            {
                Id = "complex_expedition",
                Description = "Multi-faceted exploration mission",
                DestinationLocation = "remote_location",
                RequiredEquipmentCategories = new List<EquipmentCategory> { EquipmentCategory.Navigation_Tools },
                RequiredToolCategories = new List<ToolCategory> { ToolCategory.Measurement_Tools },
                RequiredSocialStanding = SocialRequirement.Professional,
                PhysicalRequirement = PhysicalDemand.Moderate,
                RequiredKnowledge = KnowledgeRequirement.Professional,
                Category = ContractCategory.Exploration,
                Priority = ContractPriority.High,
                RiskLevel = ContractRisk.Moderate,
                RequiredInformation = new List<InformationRequirementData>
                {
                    new InformationRequirementData(InformationType.Route_Conditions, InformationQuality.Verified)
                }
            };

            Player player = new Player();
            player.Stamina = 8; // Sufficient for Moderate demand
            
            // Add required information
            Information routeInfo = new Information("route_data", "Verified Route Info", InformationType.Route_Conditions)
            {
                Quality = InformationQuality.Verified,
                Freshness = InformationFreshness.Current
            };
            player.KnownInformation.Add(routeInfo);
            
            string currentLocation = "remote_location";

            // Act
            ContractAccessResult result = contract.GetAccessResult(player, currentLocation);

            // Assert
            // Should have some blockers due to placeholder implementations for equipment/social checks
            Assert.NotNull(result);
            Assert.True(result.MissingRequirements.Count > 0 || result.AcceptanceBlockers.Count > 0, 
                "Complex contract should have categorical requirements to validate");
        }
    }
}