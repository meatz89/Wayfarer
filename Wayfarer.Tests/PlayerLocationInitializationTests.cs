using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests to ensure Player.CurrentLocation and Player.CurrentLocationSpot 
    /// are NEVER null after game initialization.
    /// Critical for systems that depend on these values being valid.
    /// </summary>
    public class PlayerLocationInitializationTests
    {
        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // Add configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"DefaultAIProvider", "None"} // Disable AI
                })
                .Build();
            services.AddSingleton(configuration);
            services.AddLogging();

            
            services.ConfigureServices();

            return services.BuildServiceProvider();
        }

        [Fact]
        public void PlayerLocation_ShouldNeverBeNull_AfterLocationSystemInitialize()
        {
            // CRITICAL: Player.CurrentLocation must NEVER be null
            // Systems depend on this value being valid

            // Arrange: Get  services
            IServiceProvider serviceProvider = CreateServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Act: Initialize location system (this is called during game initialization)
            Location startLocation = locationSystem.Initialize().Result;

            // Assert: Player MUST have valid current location
            Player player = gameWorld.GetPlayer();
            Assert.NotNull(startLocation);
            Assert.NotNull(player.CurrentLocation);
            Assert.Equal(startLocation.Id, player.CurrentLocation.Id);

            // Player must have valid current location spot
            Assert.NotNull(player.CurrentLocationSpot);
            Assert.NotNull(player.CurrentLocationSpot.SpotID);
            Assert.Equal(startLocation.Id, player.CurrentLocationSpot.LocationId);
        }

        [Fact]
        public void PlayerLocation_ShouldNeverBeNull_AfterCharacterCreation()
        {
            // CRITICAL: Even during character creation, location values must be valid

            // Arrange: Setup services and initialize locations
            IServiceProvider serviceProvider = CreateServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Initialize locations first
            Location startLocation = locationSystem.Initialize().Result;

            // Act: Simulate character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Assert: Location values must remain valid throughout character creation
            Assert.NotNull(player.CurrentLocation);
            Assert.NotNull(player.CurrentLocationSpot);
            Assert.Equal(startLocation.Id, player.CurrentLocation.Id);
        }

        [Fact]
        public void PlayerLocation_ShouldNeverBeNull_AfterGameWorldManagerStartGame()
        {
            // CRITICAL: After StartGame(), player must have valid location state

            // Arrange: Setup complete system
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Initialize locations before character creation
            Location startLocation = locationSystem.Initialize().Result;

            // Complete character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Act: Start the game
            gameWorldManager.StartGame();

            // Assert: Player must have valid location state
            Assert.NotNull(player.CurrentLocation);
            Assert.NotNull(player.CurrentLocationSpot);

            // WorldState must also have valid current location
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(gameWorld.WorldState.CurrentLocationSpot);

            // Player and WorldState locations should be consistent
            Assert.Equal(player.CurrentLocation.Id, gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal(player.CurrentLocationSpot.SpotID, gameWorld.WorldState.CurrentLocationSpot.SpotID);
        }

        [Fact]
        public void PlayerLocation_ShouldProvideValidLocationSpots()
        {
            // CRITICAL: Player location spot must have available actions and be valid

            // Arrange: Setup system
            IServiceProvider serviceProvider = CreateServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            ActionRepository actionRepository = serviceProvider.GetRequiredService<ActionRepository>();

            // Initialize
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();

            // Assert: Player location spot must be valid and usable
            Assert.NotNull(player.CurrentLocationSpot);
            Assert.NotNull(player.CurrentLocationSpot.SpotID);
            Assert.True(player.CurrentLocationSpot.SpotID.Length > 0);

            // Location spot must belong to current location
            Assert.Equal(player.CurrentLocation.Id, player.CurrentLocationSpot.LocationId);

            // Location spot must be in the location's available spots
            Assert.Contains(player.CurrentLocationSpot.SpotID, player.CurrentLocation.AvailableSpots.Select(s => s.SpotID));

            // Location spot must have valid actions (may be 0 but should not throw)
            List<ActionDefinition> actions = actionRepository.GetActionsForSpot(player.CurrentLocationSpot.SpotID);
            Assert.NotNull(actions);
        }

        [Fact]
        public void PlayerLocation_ShouldSupportSystemOperations()
        {
            // CRITICAL: Player location must support all system operations without null exceptions

            // Arrange: Setup complete system
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();

            // Initialize and start game
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);
            gameWorldManager.StartGame();

            // Act & Assert: All system operations must work without null exceptions

            // Travel system operations
            List<Location> connectedLocations = locationRepository.GetConnectedLocations(player.CurrentLocation.Id);
            Assert.NotNull(connectedLocations);

            // Location system operations
            List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(player.CurrentLocation.Id);
            Assert.NotNull(locationSpots);
            Assert.True(locationSpots.Count > 0);

            // Game world manager operations
            Assert.True(gameWorldManager.CanMoveToSpot(player.CurrentLocationSpot.SpotID));

            // Player knowledge operations
            Assert.True(player.KnownLocations.Count > 0);
            Assert.True(player.KnownLocationSpots.Count > 0);
            Assert.Contains(player.CurrentLocation.Id, player.KnownLocations);
            Assert.Contains(player.CurrentLocationSpot.SpotID, player.KnownLocationSpots);
        }

        [Fact]
        public void LocationInitialization_ShouldPreventNullPointerExceptions()
        {
            // DOCUMENTS: Common operations that would fail with null locations

            // Arrange: Setup system
            IServiceProvider serviceProvider = CreateServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Initialize
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();

            // Act & Assert: Operations that would crash with null locations should work

            // These operations must not throw NullReferenceException
            Exception exception = Record.Exception(() =>
            {
                string locationId = player.CurrentLocation.Id;
                string locationName = player.CurrentLocation.Name;
                string spotId = player.CurrentLocationSpot.SpotID;
                string spotName = player.CurrentLocationSpot.Name;
                bool spotClosed = player.CurrentLocationSpot.IsClosed;
                List<LocationSpot> spots = player.CurrentLocation.AvailableSpots;
            });

            Assert.Null(exception);

            // String operations that depend on non-null values
            Assert.NotNull(player.CurrentLocation.Id);
            Assert.NotNull(player.CurrentLocation.Name);
            Assert.NotNull(player.CurrentLocationSpot.SpotID);
            Assert.NotNull(player.CurrentLocationSpot.Name);
            Assert.True(player.CurrentLocation.Id.Length > 0);
            Assert.True(player.CurrentLocationSpot.SpotID.Length > 0);
        }
    }
}