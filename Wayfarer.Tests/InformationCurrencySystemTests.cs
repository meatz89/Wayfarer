using Xunit;
using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;
using Wayfarer.Content;

namespace Wayfarer.Tests
{
    public class InformationCurrencySystemTests
    {
        [Fact]
        public void GameWorldInitializer_Should_Load_Information_From_JSON()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");

            // Act
            GameWorld gameWorld = initializer.LoadGame();

            // Assert
            Assert.NotNull(gameWorld.WorldState.Informations);
            Assert.True(gameWorld.WorldState.Informations.Count > 0, "Expected information entries to be loaded from JSON");

            // Verify specific sample information was loaded
            Information? grainPrices = gameWorld.WorldState.Informations
                .FirstOrDefault(info => info.Id == "grain_prices_millbrook");
            
            Assert.NotNull(grainPrices);
            Assert.Equal("Current Grain Prices at Millbrook Market", grainPrices.Title);
            Assert.Equal(InformationType.Market_Intelligence, grainPrices.Type);
            Assert.Equal(InformationQuality.Expert, grainPrices.Quality);
            Assert.Equal(InformationFreshness.Current, grainPrices.Freshness);
        }

        [Fact]
        public void InformationRepository_Should_Find_Information_By_ID()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();
            InformationRepository repository = new InformationRepository(gameWorld);

            // Act
            Information? information = repository.GetInformationById("mountain_route_blocked");

            // Assert
            Assert.NotNull(information);
            Assert.Equal("Mountain Pass Weather Conditions", information.Title);
            Assert.Equal(InformationType.Route_Conditions, information.Type);
            Assert.Equal(InformationQuality.Verified, information.Quality);
            Assert.Equal(InformationFreshness.Breaking, information.Freshness);
        }

        [Fact]
        public void InformationRepository_Should_Find_Information_By_Categorical_Properties()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();
            InformationRepository repository = new InformationRepository(gameWorld);

            // Act - Find all market intelligence information
            List<Information> marketInfo = repository.FindInformationMatching(
                InformationType.Market_Intelligence, 
                InformationQuality.Reliable, 
                InformationFreshness.Recent);

            // Assert
            Assert.NotEmpty(marketInfo);
            Assert.All(marketInfo, info => Assert.Equal(InformationType.Market_Intelligence, info.Type));
            Assert.All(marketInfo, info => Assert.True(info.Quality >= InformationQuality.Reliable));
            Assert.All(marketInfo, info => Assert.True(info.Freshness >= InformationFreshness.Recent));
        }

        [Fact]
        public void Information_Should_Calculate_Value_Based_On_Categorical_Properties()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();

            // Act
            Information? expertInfo = gameWorld.WorldState.Informations
                .FirstOrDefault(info => info.Quality == InformationQuality.Expert);
            Information? rumorInfo = gameWorld.WorldState.Informations
                .FirstOrDefault(info => info.Quality == InformationQuality.Rumor);

            // Assert
            Assert.NotNull(expertInfo);
            Assert.NotNull(rumorInfo);
            
            int expertValue = expertInfo.CalculateCurrentValue();
            int rumorValue = rumorInfo.CalculateCurrentValue();
            
            Assert.True(expertValue > rumorValue, "Expert information should be more valuable than rumors");
        }

        [Fact]
        public void Information_Should_Have_Logical_Categorical_Relationships()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();

            // Act
            Information? routeInfo = gameWorld.WorldState.Informations
                .FirstOrDefault(info => info.Type == InformationType.Route_Conditions);

            // Assert
            Assert.NotNull(routeInfo);
            Assert.NotEmpty(routeInfo.RelatedLocationIds);
            
            // Route information should reference related locations
            Assert.Contains(routeInfo.RelatedLocationIds, locId => !string.IsNullOrEmpty(locId));
        }

        [Fact]
        public void InformationRequirement_Should_Block_Actions_Without_Required_Information()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();
            
            InformationRequirement requirement = new InformationRequirement(
                InformationType.Market_Intelligence,
                InformationQuality.Reliable,
                InformationFreshness.Current,
                null);

            // Act
            bool isMet = requirement.IsMet(gameWorld);

            // Assert
            Assert.False(isMet, "Requirement should not be met without required information");
        }

        [Fact]
        public void InformationRequirement_Should_Allow_Actions_With_Sufficient_Information()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            GameWorld gameWorld = initializer.LoadGame();
            Player player = gameWorld.GetPlayer();
            
            Information marketInfo = new Information("test_market_info", "Test Market Info", InformationType.Market_Intelligence)
            {
                Quality = InformationQuality.Expert,
                Freshness = InformationFreshness.Current
            };
            
            player.KnownInformation.Add(marketInfo);

            InformationRequirement requirement = new InformationRequirement(
                InformationType.Market_Intelligence,
                InformationQuality.Reliable,
                InformationFreshness.Current,
                null);

            // Act
            bool isMet = requirement.IsMet(gameWorld);

            // Assert
            Assert.True(isMet, "Requirement should be met with sufficient information");
        }

        [Fact]
        public void InformationEffect_Should_Add_Information_To_Player_Knowledge()
        {
            // Arrange
            Player player = new Player();
            
            Information newInfo = new Information("new_secret", "New Secret", InformationType.Location_Secrets)
            {
                Quality = InformationQuality.Verified,
                Freshness = InformationFreshness.Current,
                Content = "A hidden cache of supplies"
            };

            InformationEffect effect = new InformationEffect(newInfo, false);
            EncounterState encounterState = new EncounterState(player, 0, 0, 0);

            // Act
            effect.Apply(encounterState);

            // Assert
            Assert.True(player.KnownInformation.Count > 0, "Player should have received information");
            Information addedInfo = player.KnownInformation.First(info => info.Id == "new_secret");
            Assert.NotNull(addedInfo);
            Assert.Equal("New Secret", addedInfo.Title);
            Assert.Equal(InformationType.Location_Secrets, addedInfo.Type);
            Assert.Equal(InformationQuality.Verified, addedInfo.Quality);
        }

        [Fact]
        public void Information_Categorical_Types_Should_Cover_All_Game_Systems()
        {
            // Arrange
            List<InformationType> expectedTypes = new List<InformationType>
            {
                InformationType.Market_Intelligence,
                InformationType.Route_Conditions,
                InformationType.Social_Gossip,
                InformationType.Professional_Knowledge,
                InformationType.Location_Secrets,
                InformationType.Political_News,
                InformationType.Personal_History,
                InformationType.Resource_Availability
            };

            // Act
            InformationType[] allTypes = Enum.GetValues<InformationType>();

            // Assert
            Assert.Equal(expectedTypes.Count, allTypes.Length);
            foreach (InformationType expectedType in expectedTypes)
            {
                Assert.Contains(expectedType, allTypes);
            }
        }
    }
}