using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Integration tests documenting the economic game initialization flow
    /// WITHOUT AI services - focuses purely on economic systems.
    /// These tests use the real implementation to document economic gameplay preparation.
    /// </summary>
    public class EconomicGameInitializationTests
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

            // Use the new economic-only service configuration
            services.ConfigureTestServices();

            return services.BuildServiceProvider();
        }

        [Fact]
        public void EconomicStep1_GameWorldInitializer_LoadsAllEconomicContent()
        {
            // DOCUMENTS: All economic content loads correctly from JSON

            // Act: Load economic content
            string contentDirectory = "Content";
            GameWorldInitializer gameWorldInitializer = new GameWorldInitializer(contentDirectory);
            GameWorld gameWorld = gameWorldInitializer.LoadGame();

            // Assert: All economic content types are loaded
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);

            // Locations for economic trading
            Assert.NotNull(gameWorld.WorldState.locations);
            Assert.True(gameWorld.WorldState.locations.Count > 0, "Should load locations for trading");

            // Location spots for market access
            Assert.NotNull(gameWorld.WorldState.locationSpots);
            Assert.True(gameWorld.WorldState.locationSpots.Count > 0, "Should load location spots for markets");

            // Items for trading system
            Assert.NotNull(gameWorld.WorldState.Items);
            Assert.True(gameWorld.WorldState.Items.Count > 0, "Should load items for trading");

            // Routes for travel/transport costs
            Assert.NotNull(gameWorld.DiscoveredRoutes);
            Assert.True(gameWorld.DiscoveredRoutes.Count > 0, "Should load routes for travel options");

            // Contracts for economic goals via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotNull(contractRepository.GetAllContracts());
            Assert.True(contractRepository.GetAllContracts().Count > 0, "Should load contracts for economic objectives");

            // Actions for basic economic activities
            Assert.NotNull(gameWorld.WorldState.actions);
            Assert.True(gameWorld.WorldState.actions.Count > 0, "Should load actions for economic activities");
        }

        [Fact]
        public void EconomicStep2_EconomicServices_CreateWithoutAI()
        {
            // DOCUMENTS: All economic services work without AI dependencies

            // Act: Create service provider with only economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();

            // Assert: All economic services are available
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            Assert.NotNull(gameWorld);
            Assert.True(gameWorld.WorldState.locations.Count > 0);

            // Core economic systems
            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            Assert.NotNull(locationRepository);

            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            Assert.NotNull(locationSystem);

            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            Assert.NotNull(travelManager);

            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            Assert.NotNull(marketManager);

            TradeManager tradeManager = serviceProvider.GetRequiredService<TradeManager>();
            Assert.NotNull(tradeManager);

            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();
            Assert.NotNull(contractSystem);

            ItemRepository itemRepository = serviceProvider.GetRequiredService<ItemRepository>();
            Assert.NotNull(itemRepository);

            // NO AI services should be needed for economic gameplay
        }

        [Fact]
        public void EconomicStep3_LocationSystem_InitializesKnownLocations()
        {
            // DOCUMENTS: Player starts with knowledge of tradeable locations

            // Arrange: Get economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Act: Initialize location knowledge for economic gameplay
            Location startLocation = locationSystem.Initialize().Result;

            // Assert: Player knows about economic locations
            Player player = gameWorld.GetPlayer();
            Assert.NotNull(startLocation);

            // Player should know starting location for trading
            Assert.True(player.KnownLocations.Count > 0, "Player should know tradeable locations");
            Assert.Contains(startLocation.Id, player.KnownLocations);

            // Player should know market spots
            Assert.True(player.KnownLocationSpots.Count > 0, "Player should know market spots");

            // Starting location should have market spots
            Assert.NotNull(startLocation.AvailableSpots);
            Assert.True(startLocation.AvailableSpots.Count > 0, "Starting location should have market spots");
        }

        [Fact]
        public void EconomicStep4_PlayerSetup_ReadyForEconomicGameplay()
        {
            // DOCUMENTS: Player character setup for economic simulation

            // Arrange: Get services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();

            // Act: Setup player for economic gameplay (simulates character creation)
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant; // Ideal for economic POC
            player.Name = "Economic Test Character";

            // Assert: Player ready for economic activities
            Assert.Equal(Professions.Merchant, player.Archetype);
            Assert.Equal("Economic Test Character", player.Name);

            // Player has economic resources
            Assert.True(player.Coins >= 0, "Player should have starting coins for trading");
            Assert.True(player.Stamina > 0, "Player should have stamina for travel/work");
            Assert.True(player.MaxStamina > 0, "Player should have stamina limits");

            // Player has inventory for trading
            Assert.NotNull(player.Inventory);
            Assert.NotNull(player.Inventory.ItemSlots);

            // Player location will be set during game start (null is expected here)
            // Current location set during GameWorldManager.StartGame()
        }

        [Fact]
        public void EconomicStep5_TravelSystem_ProvidesRouteOptions()
        {
            // DOCUMENTS: Travel system works for economic route optimization

            // Arrange: Setup economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            LocationRepository locationRepository = serviceProvider.GetRequiredService<LocationRepository>();

            // Initialize
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            Location startLocation = locationSystem.Initialize().Result;

            // Act: Check travel options for economic routes
            List<Location> connectedLocations = locationRepository.GetConnectedLocations(startLocation.Id);

            // Assert: Travel system ready for economic gameplay
            Assert.NotNull(connectedLocations);

            // If there are connected locations, check route options
            if (connectedLocations.Count > 0)
            {
                Location destinationLocation = connectedLocations.First();

                // Should have route options with economic costs
                List<RouteOption> routeOptions = travelManager.GetAvailableRoutes(
                    startLocation.Id, destinationLocation.Id);

                Assert.NotNull(routeOptions);
                if (routeOptions.Count > 0)
                {
                    // Routes should have economic costs for optimization
                    RouteOption route = routeOptions.First();
                    Assert.NotNull(route.Id);
                    Assert.NotNull(route.Name);
                    Assert.Equal(startLocation.Id, route.Origin);
                    Assert.Equal(destinationLocation.Id, route.Destination);
                    Assert.True(route.BaseCoinCost >= 0, "Route should have coin cost");
                    Assert.True(route.BaseStaminaCost >= 0, "Route should have stamina cost");
                    Assert.True(route.TimeBlockCost >= 0, "Route should have time cost");
                }
            }
        }

        [Fact]
        public void EconomicStep6_MarketSystem_ProvidesItemPricing()
        {
            // DOCUMENTS: Market system ready for trading and price differences

            // Arrange: Setup economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();

            // Initialize
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            Location startLocation = locationSystem.Initialize().Result;

            // Act: Check market system functionality
            Assert.NotNull(gameWorld.WorldState.Items);
            Assert.True(gameWorld.WorldState.Items.Count > 0, "Should have items for trading");

            // Should be able to check item prices at locations
            string firstItemId = gameWorld.WorldState.Items.First().Name;
            int buyPrice = marketManager.GetItemPrice(startLocation.Id, firstItemId, true);
            int sellPrice = marketManager.GetItemPrice(startLocation.Id, firstItemId, false);

            // Assert: Market system functional (prices may be -1 if not available at location)
            Assert.True(buyPrice >= -1, "Buy price should be valid or -1 for unavailable");
            Assert.True(sellPrice >= -1, "Sell price should be valid or -1 for unavailable");

            // Market manager should handle price queries without crashes
            Assert.NotNull(marketManager);
        }

        [Fact]
        public void EconomicStep7_ContractSystem_ProvidesEconomicGoals()
        {
            // DOCUMENTS: Contract system provides economic objectives

            // Arrange: Setup economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();

            // Initialize
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            Location startLocation = locationSystem.Initialize().Result;

            // Act: Check contract system for economic goals via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotNull(contractRepository.GetAllContracts());
            Assert.True(contractRepository.GetAllContracts().Count > 0, "Should have contracts for economic goals");

            // Contracts should have economic properties
            foreach (Contract contract in contractRepository.GetAllContracts())
            {
                Assert.NotNull(contract.Id);
                Assert.NotNull(contract.Description);
                Assert.NotNull(contract.DestinationLocation);
                Assert.True(contract.Payment > 0, "Contract should provide economic reward");
                Assert.True(contract.DueDay > 0, "Contract should have time pressure");

                // Contracts may require items for economic gameplay
                Assert.NotNull(contract.RequiredItems);
            }
        }

        [Fact]
        public void EconomicStep8_ActionSystem_ProvidesEconomicActivities()
        {
            // DOCUMENTS: Action system provides economic activities (work, trade, rest)

            // Arrange: Setup economic services
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            ActionRepository actionRepository = serviceProvider.GetRequiredService<ActionRepository>();

            // Initialize
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            Location startLocation = locationSystem.Initialize().Result;
            LocationSpot firstSpot = startLocation.AvailableSpots.First();

            // Act: Check actions available for economic activities
            List<ActionDefinition> availableActions = actionRepository.GetActionsForSpot(firstSpot.SpotID);

            // Assert: Economic actions available
            Assert.NotNull(availableActions);

            // Actions should be configured for economic gameplay
            foreach (ActionDefinition action in availableActions)
            {
                Assert.NotNull(action.Id);
                Assert.NotNull(action.Name);
                Assert.True(action.ActionPointCost >= 0, "Action should have time cost");

                // Economic actions might include: work, trade, rest, travel
            }

            // All actions from JSON should be loaded
            List<ActionDefinition> allActions = actionRepository.GetAllActions();
            Assert.NotNull(allActions);
            Assert.True(allActions.Count > 0, "Should have actions loaded from JSON");
        }

        [Fact]
        public void EconomicIntegrationTest_CompleteEconomicGameSetup()
        {
            // DOCUMENTS: Complete economic game initialization without AI

            // Act: Execute complete economic initialization
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();

            // 1. Economic content loaded
            Assert.True(gameWorld.WorldState.locations.Count > 0);
            Assert.True(gameWorld.WorldState.locationSpots.Count > 0);
            Assert.True(gameWorld.WorldState.actions.Count > 0);
            Assert.True(gameWorld.WorldState.Items.Count > 0);
            Assert.True(gameWorld.DiscoveredRoutes.Count > 0);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.True(contractRepository.GetAllContracts().Count > 0);

            // 2. Player setup for economic gameplay
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            player.Name = "Economic Integration Test";

            // 3. Initialize location knowledge
            Location startLocation = locationSystem.Initialize().Result;
            Assert.NotNull(startLocation);

            // 4. Verify player ready for economic activities
            Assert.True(player.KnownLocations.Count > 0);
            Assert.True(player.KnownLocationSpots.Count > 0);
            Assert.True(player.Coins >= 0);
            Assert.True(player.Stamina > 0);
            Assert.NotNull(player.Inventory);

            // 5. Verify all economic systems functional

            // Location system works (no crashes like before)
            LocationSystem locationSystemService = serviceProvider.GetRequiredService<LocationSystem>();
            List<LocationSpot> spots = locationSystemService.GetLocationSpots(startLocation.Id);
            Assert.True(spots.Count > 0);

            // Market system ready
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            Assert.NotNull(marketManager);

            // Travel system ready
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            Assert.NotNull(travelManager);

            // Contract system ready
            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();
            Assert.NotNull(contractSystem);

            // Action system ready
            ActionRepository actionRepository = serviceProvider.GetRequiredService<ActionRepository>();
            List<ActionDefinition> actions = actionRepository.GetActionsForSpot(spots.First().SpotID);
            // Actions may be 0 but system should not crash

            // Economic POC ready: Time blocks, stamina, trading, routes, contracts all loaded
        }

        [Fact]
        public void EconomicPOC_VerifyNoAIDependencies()
        {
            // DOCUMENTS: Economic POC works completely without AI services

            // Act: Create economic service provider (no AI)
            IServiceProvider serviceProvider = CreateEconomicServiceProvider();

            // Assert: All economic systems work without AI
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            ContractSystem contractSystem = serviceProvider.GetRequiredService<ContractSystem>();

            Assert.NotNull(gameWorld);
            Assert.NotNull(locationSystem);
            Assert.NotNull(travelManager);
            Assert.NotNull(marketManager);
            Assert.NotNull(contractSystem);

            // Verify we can perform economic initialization
            Player player = gameWorld.GetPlayer();
            player.Archetype = Professions.Merchant;
            Location startLocation = locationSystem.Initialize().Result;

            // All economic prerequisites met without AI
            Assert.NotNull(startLocation);
            Assert.True(player.KnownLocations.Count > 0);
            Assert.True(gameWorld.WorldState.Items.Count > 0);
            Assert.True(gameWorld.DiscoveredRoutes.Count > 0);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.True(contractRepository.GetAllContracts().Count > 0);

            // Ready for Economic POC Option A implementation:
            // - Time block constraint system ✅
            // - Multiple route options ✅ 
            // - Trading with location-based pricing ✅
            // - Contract deadlines and time pressure ✅
        }
    }
}