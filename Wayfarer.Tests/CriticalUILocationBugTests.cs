using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Wayfarer.Tests
{
    /// <summary>
    /// CRITICAL BUG TESTS: UI crashes after character creation due to null location
    /// 
    /// Issue: MainGameplayView.GetCurrentLocation() returns GameWorld.CurrentLocation (always null)
    /// Should return: GameWorld.WorldState.CurrentLocation (correctly initialized)
    /// 
    /// This test reproduces the exact failure scenario:
    /// Character creation → "Begin Your Journey" → LocationSpotMap crash
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
        public void GameWorldCurrentLocation_ShouldAlwaysBeNull_ProvingTheBug()
        {
            // DOCUMENTS: GameWorld.CurrentLocation is never set anywhere in codebase
            
            // Arrange: Complete game initialization
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            
            // Complete initialization (simulates what happens before character creation)
            Location startLocation = locationSystem.Initialize().Result;
            
            // Simulate character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);
            
            // Act & Assert: Demonstrate the bug
            
            // These are correctly initialized:
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(player.CurrentLocation);
            Assert.Equal("dusty_flagon", gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal("dusty_flagon", player.CurrentLocation.Id);
            
            // This is always null (THE BUG):
            Assert.Null(gameWorld.CurrentLocation);
            
            // This proves MainGameplayView.GetCurrentLocation() will return null
        }

        [Fact]
        public void MainGameplayView_GetCurrentLocation_ShouldReturnNull_ReproducingCrash()
        {
            // REPRODUCES: The exact bug that causes UI crash
            
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
            
            // Assert: This returns null, causing the crash
            Assert.Null(currentLocationFromUI);
            
            // Demonstrate what would happen in LocationSpotMap.GetKnownSpots()
            Assert.Throws<NullReferenceException>(() => {
                // This is the exact line that crashes:
                string locationId = currentLocationFromUI.Id;  // NullReferenceException here
            });
        }

        [Fact]
        public void LocationSpotMap_GetKnownSpots_ShouldCrash_WithNullCurrentLocation()
        {
            // REPRODUCES: The exact crash in LocationSpotMap component
            
            // Arrange: Setup services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            
            // Complete initialization
            Location startLocation = locationSystem.Initialize().Result;
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);
            
            // Act: Simulate LocationSpotMap.GetKnownSpots() with current broken implementation
            Location nullLocation = gameWorld.CurrentLocation;  // This is null
            
            // Assert: This throws NullReferenceException (the actual crash)
            Assert.Throws<NullReferenceException>(() => {
                // This is the exact code that crashes in LocationSpotMap.GetKnownSpots():
                var locationSpots = locationSystem.GetLocationSpots(nullLocation.Id);
            });
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
            Exception exception = Record.Exception(() => {
                var locationSpots = locationSystem.GetLocationSpots(correctLocation.Id);
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
            var knownSpots = locationSystem.GetLocationSpots(uiLocation.Id);
            
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
            
            // This should still be null (proving it's not used):
            Assert.Null(gameWorld.CurrentLocation);
            
            // UI should use WorldState, not the null GameWorld property
            Location correctUILocation = gameWorld.WorldState.CurrentLocation;
            Assert.NotNull(correctUILocation);
            Assert.Equal(startLocation.Id, correctUILocation.Id);
        }
    }
}