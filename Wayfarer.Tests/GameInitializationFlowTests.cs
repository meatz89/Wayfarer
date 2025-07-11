using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Integration tests documenting the complete game initialization flow
    /// from JSON content loading through character creation to first location arrival.
    /// These tests use the real implementation (no mocks) to document how the system works.
    /// </summary>
    public class GameInitializationFlowTests
    {
        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // Add configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"DefaultAIProvider", "Ollama"}
                })
                .Build();
            services.AddSingleton(configuration);
            services.AddLogging();

            // Use the real ServiceConfiguration
            services.ConfigureServices();

            return services.BuildServiceProvider();
        }

        [Fact]
        public void Step1_GameWorldInitializer_ShouldLoadAllJSONTemplatesCorrectly()
        {
            // DOCUMENTS: How JSON content is loaded into GameWorld

            // Act: Use the actual GameWorldInitializer with fixed path
            string contentDirectory = "Content"; // Fixed path from ServiceConfiguration
            GameWorldInitializer gameWorldInitializer = new GameWorldInitializer(contentDirectory);
            GameWorld gameWorld = gameWorldInitializer.LoadGame();

            // Assert: Verify all content types are loaded
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);

            // Locations loaded from locations.json
            Assert.NotNull(gameWorld.WorldState.locations);
            Assert.True(gameWorld.WorldState.locations.Count > 0, "Should load locations from locations.json");

            // Location spots loaded from location_spots.json
            Assert.NotNull(gameWorld.WorldState.locationSpots);
            Assert.True(gameWorld.WorldState.locationSpots.Count > 0, "Should load location spots from location_spots.json");

            // Actions loaded from actions.json
            Assert.NotNull(gameWorld.WorldState.actions);
            Assert.True(gameWorld.WorldState.actions.Count > 0, "Should load actions from actions.json");

            // Items loaded from items.json
            Assert.NotNull(gameWorld.WorldState.Items);
            Assert.True(gameWorld.WorldState.Items.Count > 0, "Should load items from items.json");

            // Routes loaded from routes.json
            Assert.NotNull(gameWorld.DiscoveredRoutes);
            Assert.True(gameWorld.DiscoveredRoutes.Count > 0, "Should load routes from routes.json");

            // Contracts loaded from contracts.json via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotNull(contractRepository.GetAllContracts());
            Assert.True(contractRepository.GetAllContracts().Count > 0, "Should load contracts from contracts.json");
        }

        [Fact]
        public void Step2_ServiceConfiguration_ShouldCreateFullyConfiguredServices()
        {
            // DOCUMENTS: How ServiceConfiguration creates all services with proper DI

            // Act: Create service provider using real ServiceConfiguration
            IServiceProvider serviceProvider = CreateServiceProvider();

            // Assert: Verify all critical services are registered and can be resolved
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            Assert.NotNull(gameWorld);
            Assert.True(gameWorld.WorldState.locations.Count > 0);

            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            Assert.NotNull(locationRepository);

            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            Assert.NotNull(locationSystem);

            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            Assert.NotNull(gameWorldManager);

            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            Assert.NotNull(travelManager);

            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            Assert.NotNull(marketManager);

            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();
            Assert.NotNull(contractSystem);
        }

        [Fact]
        public void Step3_LocationSystem_ShouldInitializePlayerKnowledgeCorrectly()
        {
            // DOCUMENTS: How LocationSystem.Initialize() sets up initial player knowledge

            // Arrange: Get services from real DI container
            IServiceProvider serviceProvider = CreateServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Act: Initialize the location system (this should be called during game start)
            Location startLocation = locationSystem.Initialize().Result;

            // Assert: Verify player starts with knowledge of initial locations and spots
            Player player = gameWorld.GetPlayer();
            Assert.NotNull(startLocation);

            // Player should know about the starting location
            Assert.True(player.KnownLocations.Count > 0, "Player should know at least the starting location");
            Assert.Contains(startLocation.Id, player.KnownLocations);

            // Player should know about spots in known locations
            Assert.True(player.KnownLocationSpots.Count > 0, "Player should know some location spots");

            // Starting location should have available spots
            Assert.NotNull(startLocation.AvailableSpots);
            Assert.True(startLocation.AvailableSpots.Count > 0, "Starting location should have spots");
        }

        [Fact]
        public void Step4_CharacterCreation_ShouldSetupPlayerWithValidArchetype()
        {
            // DOCUMENTS: How character creation sets up the player

            // Arrange: Get services
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Act: Simulate character creation process
            // This represents what happens when player selects character options
            Player player = gameWorld.GetPlayer();

            // Set archetype (this would normally happen in character creation UI)
            player.Archetype = Professions.Merchant; // Example archetype
            // Note: Gender is readonly after initialization
            player.Name = "Test Character";

            // Assert: Verify player has proper initial state
            Assert.Equal(Professions.Merchant, player.Archetype);
            Assert.Equal("Test Character", player.Name);

            // Player should start with initial resources
            Assert.True(player.Coins >= 0, "Player should have initial coins");
            Assert.True(player.Stamina > 0, "Player should have initial stamina");
            Assert.True(player.MaxStamina > 0, "Player should have max stamina");

            // Player should have inventory
            Assert.NotNull(player.Inventory);
            Assert.NotNull(player.Inventory.ItemSlots);
        }

        [Fact]
        public void Step5_GameWorldManager_StartGame_ShouldSetupCompleteInitialState()
        {
            // DOCUMENTS: How GameWorldManager.StartGame() initializes everything

            // Arrange: Get services and setup character
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Setup character as if character creation completed
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            player.Name = "Test Character";

            // Act: Start the game (this is called after character creation)
            gameWorldManager.StartGame();

            // Assert: Verify complete game state is initialized

            // Player should be at a valid location
            Assert.NotNull(player.CurrentLocation);
            Assert.NotNull(player.CurrentLocationSpot);

            // WorldState should have current location set
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(gameWorld.WorldState.CurrentLocationSpot);
            Assert.Equal(player.CurrentLocation.Id, gameWorld.WorldState.CurrentLocation.Id);
            Assert.Equal(player.CurrentLocationSpot.SpotID, gameWorld.WorldState.CurrentLocationSpot.SpotID);

            // Time should be initialized
            Assert.True(gameWorld.WorldState.CurrentDay >= 1);
            Assert.True(gameWorld.WorldState.CurrentTimeHours >= 0);

            // Player should have available cards
            Assert.NotNull(player.AvailableCards);
        }

        [Fact]
        public void Step6_LocationSpotMap_GetKnownSpots_ShouldReturnValidSpots()
        {
            // DOCUMENTS: How LocationSpotMap gets spots for display (the original failing component)

            // Arrange: Setup complete game state
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // Complete initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            gameWorldManager.StartGame();

            // Act: Simulate what LocationSpotMap.GetKnownSpots() does
            Location currentLocation = player.CurrentLocation;
            List<LocationSpot> knownSpots = locationSystem.GetLocationSpots(currentLocation.Id);

            // Assert: This should now work without exceptions
            Assert.NotNull(knownSpots);
            Assert.True(knownSpots.Count > 0, "Current location should have available spots");

            // Each spot should be properly linked to the location
            foreach (LocationSpot spot in knownSpots)
            {
                Assert.NotNull(spot.SpotID);
                Assert.Equal(currentLocation.Id, spot.LocationId);
            }
        }

        [Fact]
        public void Step7_ActionSystem_ShouldProvideActionsAtLocationSpots()
        {
            // DOCUMENTS: How actions are generated and available at location spots

            // Arrange: Setup complete game state
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            ActionRepository actionRepository = serviceProvider.GetRequiredService<ActionRepository>();

            // Complete initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            gameWorldManager.StartGame();

            // Act: Get actions at current location spot
            Location currentLocation = player.CurrentLocation;
            LocationSpot currentSpot = player.CurrentLocationSpot;

            List<ActionDefinition> availableActions = actionRepository.GetActionsForSpot(currentSpot.SpotID);

            // Assert: Player should have actions available
            Assert.NotNull(availableActions);
            Assert.True(availableActions.Count > 0, "Location spot should have available actions");

            // Each action should be properly configured
            foreach (ActionDefinition action in availableActions)
            {
                Assert.NotNull(action.Id);
                Assert.NotNull(action.Name);
                Assert.True(action.ActionPointCost >= 0);
            }
        }

        [Fact]
        public void Step8_ContractSystem_ShouldProvideInitialContracts()
        {
            // DOCUMENTS: How contracts are available from game start

            // Arrange: Setup complete game state
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();

            // Complete initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            gameWorldManager.StartGame();

            // Act: Check contracts are loaded globally
            // Note: ContractSystem may not have GetAvailableContracts method
            // but contracts should be loaded in GameWorld.AllContracts

            // Assert: Global contracts should be loaded

            // Global contracts should be loaded via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotNull(contractRepository.GetAllContracts());
            Assert.True(contractRepository.GetAllContracts().Count > 0, "Should have contracts loaded from JSON");

            // Each contract should be properly configured
            foreach (Contract contract in contractRepository.GetAllContracts())
            {
                Assert.NotNull(contract.Id);
                Assert.NotNull(contract.Description);
                Assert.True(contract.Payment > 0);
                Assert.True(contract.DueDay > 0);
                
                // Verify completion action pattern (at least one completion requirement)
                bool hasCompletionRequirement = 
                    contract.RequiredTransactions.Count > 0 ||
                    contract.RequiredDestinations.Count > 0 ||
                    contract.RequiredNPCConversations.Count > 0 ||
                    contract.RequiredLocationActions.Count > 0;
                
                
                Assert.True(hasCompletionRequirement, $"Contract {contract.Id} should have at least one completion requirement");
            }
        }

        [Fact]
        public void Step9_TravelSystem_ShouldProvideRouteOptionsFromStartingLocation()
        {
            // DOCUMENTS: How travel routes are available from the starting location

            // Arrange: Setup complete game state
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();

            // Complete initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            gameWorldManager.StartGame();

            // Act: Get travel options from current location
            Location currentLocation = player.CurrentLocation;
            List<Location> connectedLocations = locationRepository.GetConnectedLocations(currentLocation.Id);

            // Assert: Player should have travel options
            Assert.NotNull(connectedLocations);

            // Starting location should connect to other locations
            if (connectedLocations.Count > 0)
            {
                Location destinationLocation = connectedLocations.First();

                // Should be able to get route options between locations
                List<RouteOption> routeOptions = travelManager.GetAvailableRoutes(
                    currentLocation.Id, destinationLocation.Id);

                Assert.NotNull(routeOptions);
                Assert.True(routeOptions.Count > 0, "Should have route options between connected locations");

                // Each route should be properly configured
                foreach (RouteOption route in routeOptions)
                {
                    Assert.NotNull(route.Id);
                    Assert.NotNull(route.Name);
                    Assert.Equal(currentLocation.Id, route.Origin);
                    Assert.Equal(destinationLocation.Id, route.Destination);
                    Assert.True(route.BaseCoinCost >= 0);
                    Assert.True(route.BaseStaminaCost >= 0);
                    Assert.True(route.TimeBlockCost >= 0);
                }
            }
        }

        [Fact]
        public void Step10_MarketSystem_ShouldProvideItemsForTradingAtLocations()
        {
            // DOCUMENTS: How market/trading system works at locations

            // Arrange: Setup complete game state
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();

            // Complete initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            gameWorldManager.StartGame();

            // Act: Check market functionality at current location
            Location currentLocation = player.CurrentLocation;

            // Should have items loaded in world state
            Assert.NotNull(gameWorld.WorldState.Items);
            Assert.True(gameWorld.WorldState.Items.Count > 0, "Should have items loaded from items.json");

            // Should be able to check item prices at location
            string firstItemId = gameWorld.WorldState.Items.First().Name;
            int buyPrice = marketManager.GetItemPrice(currentLocation.Id, firstItemId, true);
            int sellPrice = marketManager.GetItemPrice(currentLocation.Id, firstItemId, false);

            // Prices may be -1 (not available) but system should work without exceptions
            Assert.True(buyPrice >= -1, "Buy price should be valid or -1 for unavailable");
            Assert.True(sellPrice >= -1, "Sell price should be valid or -1 for unavailable");
        }

        [Fact]
        public void IntegrationTest_CompleteGameInitializationFlow()
        {
            // DOCUMENTS: Complete flow from service creation to playable game state

            // Act: Execute complete initialization sequence
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // 1. Content should be loaded (happens in ServiceConfiguration)
            Assert.True(gameWorld.WorldState.locations.Count > 0);
            Assert.True(gameWorld.WorldState.locationSpots.Count > 0);
            Assert.True(gameWorld.WorldState.actions.Count > 0);
            Assert.True(gameWorld.WorldState.Items.Count > 0);

            // 2. Character creation
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            player.Name = "Integration Test Character";

            // 3. Initialize location system
            Location startLocation = locationSystem.Initialize().Result;
            Assert.NotNull(startLocation);

            // 4. Start game
            gameWorldManager.StartGame();

            // 5. Verify player is in playable state
            Assert.NotNull(player.CurrentLocation);
            Assert.NotNull(player.CurrentLocationSpot);
            Assert.True(player.KnownLocations.Count > 0);
            Assert.True(player.KnownLocationSpots.Count > 0);

            // 6. Verify all game systems are functional

            // LocationSpotMap should work (original failing component)
            LocationSystem locationSystemService = serviceProvider.GetRequiredService<LocationSystem>();
            List<LocationSpot> spots = locationSystemService.GetLocationSpots(player.CurrentLocation.Id);
            Assert.True(spots.Count > 0);

            // Actions should be available
            ActionRepository actionRepository = serviceProvider.GetRequiredService<ActionRepository>();
            List<ActionDefinition> actions = actionRepository.GetActionsForSpot(player.CurrentLocationSpot.SpotID);
            Assert.True(actions.Count > 0);

            // Contracts should be loaded via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.True(contractRepository.GetAllContracts().Count > 0);

            // Travel system should work
            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            List<Location> connectedLocations = locationRepository.GetConnectedLocations(player.CurrentLocation.Id);
            // May be 0 connected locations for starting location, but should not throw

            // Market system should work
            Assert.True(gameWorld.WorldState.Items.Count > 0);
        }
    }
}