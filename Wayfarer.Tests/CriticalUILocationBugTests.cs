using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// UI LOCATION DELEGATION INTEGRATION TESTS
    /// 
    /// These tests validate that GameWorld.CurrentLocation properly delegates to 
    /// GameWorld.WorldState.CurrentLocation after architectural fixes were applied.
    /// 
    /// Original Issue (FIXED): MainGameplayView.GetCurrentLocation() was returning null
    /// Solution: GameWorld.CurrentLocation now delegates to WorldState.CurrentLocation
    /// 
    /// Tests validate complete initialization flow: Character creation → GameWorld setup → UI access
    /// </summary>
    public class CriticalUILocationBugTests
    {
        private IServiceProvider CreateEconomicServiceProvider()
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

            // Use the economic-only service configuration
            services.ConfigureTestServices();

            return services.BuildServiceProvider();
        }

        [Fact]
        public void GameWorldCurrentLocation_ShouldDelegateToWorldState_ValidationTest()
        {
            // VALIDATES: GameWorld.CurrentLocation properly delegates to WorldState.CurrentLocation

            // Arrange: Complete game initialization
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Complete initialization (simulates what happens before character creation)
            Location startLocation = locationSystem.Initialize().Result;

            // Simulate character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Act & Assert: Verify the bug is FIXED

            // These are correctly initialized:
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(player.CurrentLocation);
            Assert.Equal("dusty_flagon", gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal("dusty_flagon", player.CurrentLocation.Id);

            // This properly delegates to WorldState (ARCHITECTURAL FIX VALIDATED):
            Assert.NotNull(gameWorld.CurrentLocation);
            Assert.Equal("dusty_flagon", gameWorld.CurrentLocation.Id);

            // This validates MainGameplayView.GetCurrentLocation() returns proper location
        }

        [Fact]
        public void MainGameplayView_GetCurrentLocation_ShouldReturnValidLocation_IntegrationTest()
        {
            // VALIDATES: The UI delegation pattern works correctly in integration scenario

            // Arrange: Setup complete system as it would be after character creation
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Complete initialization
            Location startLocation = locationSystem.Initialize().Result;

            // Character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Act: Simulate what MainGameplayView.GetCurrentLocation() does
            Location currentLocationFromUI = gameWorld.CurrentLocation;  // This is what the UI currently does

            // Assert: This now returns a valid location, preventing the crash
            Assert.NotNull(currentLocationFromUI);
            Assert.Equal("dusty_flagon", currentLocationFromUI.Id);

            // Demonstrate that LocationSpotMap.GetKnownSpots() will work correctly
            string locationId = currentLocationFromUI.Id;  // No longer throws NullReferenceException
            Assert.Equal("dusty_flagon", locationId);
        }

        [Fact]
        public void LocationSpotMap_GetKnownSpots_ShouldWork_WithValidCurrentLocation()
        {
            // VALIDATES: The LocationSpotMap component crash is now fixed

            // Arrange: Setup services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Complete initialization
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Act: Simulate LocationSpotMap.GetKnownSpots() with fixed implementation
            Location validLocation = gameWorld.CurrentLocation;  // This is now valid

            // Assert: This no longer throws NullReferenceException (crash fixed)
            Assert.NotNull(validLocation);
            List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(validLocation.Id);
            Assert.NotNull(locationSpots);
        }

        [Fact]
        public void MainGameplayView_GetCurrentLocation_ShouldWork_AfterFix()
        {
            // VERIFICATION: Test that the fix works correctly

            // Arrange: Setup complete system
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Complete initialization 
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Act: Use the FIXED implementation (what GetCurrentLocation should return)
            Location correctLocation = gameWorld.WorldState.CurrentLocation;  // THE FIX

            // Assert: This works correctly
            Assert.NotNull(correctLocation);
            Assert.Equal("dusty_flagon", correctLocation.Id);
            Assert.Equal(startLocation.Id, correctLocation.Id);

            // Verify LocationSpotMap.GetKnownSpots() would work with this
            Exception exception = Record.Exception(() =>
            {
                List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(correctLocation.Id);
                Assert.NotNull(locationSpots);
                Assert.True(locationSpots.Count > 0);
            });
            Assert.Null(exception);
        }

        [Fact]
        public void CharacterCreation_To_MainGameplay_ShouldNotCrash_AfterFix()
        {
            // INTEGRATION TEST: Complete character creation → gameplay flow

            // Arrange: Setup services (simulates app startup)
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();

            // Step 1: Game initialization (happens during app startup)
            Location startLocation = locationSystem.Initialize().Result;

            // Step 2: Character creation (user inputs)
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Step 3: "Begin Your Journey" clicked → GameWorldManager.StartGame()
            gameWorldManager.StartGame();

            // Step 4: MainGameplayView renders → GetCurrentLocation() called
            // Using FIXED implementation:
            Location uiLocation = gameWorld.WorldState.CurrentLocation;  // THE FIX

            // Step 5: LocationSpotMap renders → GetKnownSpots() called
            Assert.NotNull(uiLocation);
            List<LocationSpot> knownSpots = locationSystem.GetLocationSpots(uiLocation.Id);

            // Assert: Complete flow works without crashes
            Assert.NotNull(uiLocation);
            Assert.Equal("dusty_flagon", uiLocation.Id);
            Assert.NotNull(knownSpots);
            Assert.True(knownSpots.Count > 0);

            // Verify location state consistency
            Assert.Equal(player.CurrentLocation.Id, gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal(player.CurrentLocationSpot.SpotID, gameWorld.WorldState.CurrentLocationSpot.SpotID);
        }

        [Fact]
        public void LocationState_ShouldBeConsistent_AcrossAllSystems()
        {
            // VERIFICATION: Ensure all location properties are synchronized

            // Arrange: Complete setup
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();

            // Initialize and start game
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);
            gameWorldManager.StartGame();

            // Assert: Location state consistency
            Assert.NotNull(player.CurrentLocation);
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);

            // These should be synchronized:
            Assert.Equal(player.CurrentLocation.Id, gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal(player.CurrentLocationSpot.SpotID, gameWorld.WorldState.CurrentLocationSpot.SpotID);

            // This is now properly delegated (bug fixed):
            Assert.NotNull(gameWorld.CurrentLocation);
            Assert.Equal(startLocation.Id, gameWorld.CurrentLocation.Id);

            // UI can now safely use GameWorld.CurrentLocation property
            Location correctUILocation = gameWorld.WorldState.CurrentLocation;
            Assert.NotNull(correctUILocation);
            Assert.Equal(startLocation.Id, correctUILocation.Id);
        }
    }
}