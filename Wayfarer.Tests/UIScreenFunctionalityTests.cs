using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests UI screen functionality after character creation
    /// Reproduces issues with Travel, Market, Rest, and Contracts screens
    /// </summary>
    public class UIScreenFunctionalityTests
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

        private (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) InitializeGameToMainGameplay()
        {
            // Complete game initialization flow
            IServiceProvider serviceProvider = CreateServiceProvider();
            GameWorld gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            LocationSystem locationSystem = serviceProvider.GetRequiredService<LocationSystem>();
            GameWorldManager gameWorldManager = serviceProvider.GetRequiredService<GameWorldManager>();

            // Initialize location system
            Location startLocation = locationSystem.Initialize().Result;

            // Character creation
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Character", Professions.Merchant, Genders.Male);

            // Start game (what happens when "Begin Your Journey" is clicked)
            gameWorldManager.StartGame();

            return (gameWorld, locationSystem, gameWorldManager);
        }

        [Fact]
        public void MainGameplay_ShouldHaveValidLocationAfterInitialization()
        {
            // Arrange & Act
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();

            // Simulate MainGameplayView.GetCurrentLocation()
            Location currentLocation = gameWorld.WorldState.CurrentLocation;

            // Assert: Location should be valid for UI rendering
            Assert.NotNull(currentLocation);
            Assert.Equal("dusty_flagon", currentLocation.Id);
            Assert.NotNull(currentLocation.Name);
            Assert.NotEmpty(currentLocation.Name);
        }

        [Fact]
        public void TravelScreen_ShouldHaveValidDependencies()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            Location currentLocation = gameWorld.WorldState.CurrentLocation;

            // Act: Simulate TravelSelection component initialization
            // This tests what happens when user clicks "Travel" button

            // Get required services for TravelSelection
            IServiceProvider serviceProvider = CreateServiceProvider();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            ItemRepository itemRepository = serviceProvider.GetRequiredService<ItemRepository>();

            // Test TravelSelection dependencies
            Assert.NotNull(currentLocation);
            Assert.NotNull(travelManager);
            Assert.NotNull(itemRepository);

            // Test GetTravelableLocations functionality
            List<Location> allLocations = gameWorldManager.GetPlayerKnownLocations();
            Assert.NotNull(allLocations);
            Assert.NotEmpty(allLocations);

            // Test route availability
            Exception routeException = Record.Exception(() =>
            {
                foreach (Location location in allLocations)
                {
                    if (location.Id != currentLocation.Id)
                    {
                        List<RouteOption> routes = travelManager.GetAvailableRoutes(currentLocation.Id, location.Id);
                        // Routes may be empty but should not throw exceptions
                        Assert.NotNull(routes);
                    }
                }
            });
            Assert.Null(routeException);
        }

        [Fact]
        public void MarketScreen_ShouldHaveValidDependencies()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            Location currentLocation = gameWorld.WorldState.CurrentLocation;

            // Act: Simulate Market component initialization
            // This tests what happens when user clicks "Market" button

            IServiceProvider serviceProvider = CreateServiceProvider();
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            ItemRepository itemRepository = serviceProvider.GetRequiredService<ItemRepository>();

            // Test Market dependencies
            Assert.NotNull(currentLocation);
            Assert.NotNull(marketManager);
            Assert.NotNull(itemRepository);

            // Test market functionality
            Exception marketException = Record.Exception(() =>
            {
                List<Item> availableItems = marketManager.GetAvailableItems(currentLocation.Id);
                Assert.NotNull(availableItems);

                // Test player inventory access
                Player player = gameWorld.GetPlayer();
                Assert.NotNull(player.Inventory);
                Assert.NotNull(player.Inventory.ItemSlots);

                // Test weight calculation
                int totalWeight = gameWorldManager.CalculateTotalWeight();
                Assert.True(totalWeight >= 0);
            });
            Assert.Null(marketException);
        }

        [Fact]
        public void RestScreen_ShouldHaveValidDependencies()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            Location currentLocation = gameWorld.WorldState.CurrentLocation;

            // Act: Simulate RestUI component initialization
            // This tests what happens when user clicks "Rest" button

            IServiceProvider serviceProvider = CreateServiceProvider();
            RestManager restManager = serviceProvider.GetRequiredService<RestManager>();

            // Test Rest dependencies
            Assert.NotNull(currentLocation);
            Assert.NotNull(restManager);

            // Test rest functionality
            Exception restException = Record.Exception(() =>
            {
                List<RestOption> restOptions = restManager.GetAvailableRestOptions();
                Assert.NotNull(restOptions);

                // Test player resource access
                Player player = gameWorld.GetPlayer();
                Assert.True(player.Coins >= 0);
                Assert.True(player.Stamina >= 0);
            });
            Assert.Null(restException);
        }

        [Fact]
        public void ContractScreen_ShouldHaveValidDependencies()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();

            // Act: Simulate ContractUI component initialization
            // This tests what happens when user clicks "Contracts" button

            IServiceProvider serviceProvider = CreateServiceProvider();
            ContractRepository contractRepository = serviceProvider.GetRequiredService<ContractRepository>();

            // Test Contract dependencies
            Assert.NotNull(contractRepository);

            // Test contract functionality
            Exception contractException = Record.Exception(() =>
            {
                List<Contract> availableContracts = contractRepository.GetAvailableContracts(gameWorld.CurrentDay, gameWorld.CurrentTimeBlock);
                Assert.NotNull(availableContracts);

                // Test game world time properties
                Assert.True(gameWorld.CurrentDay > 0);
                // Time block can be Dawn - that's valid, just verify it's a proper enum value
                Assert.True(Enum.IsDefined(typeof(TimeBlocks), gameWorld.CurrentTimeBlock));
            });
            Assert.Null(contractException);
        }

        [Fact]
        public void AllScreens_ShouldNotThrowNullReferenceExceptions()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            Location currentLocation = gameWorld.WorldState.CurrentLocation;
            IServiceProvider serviceProvider = CreateServiceProvider();

            // Act & Assert: Test that all screen components can be initialized without exceptions

            // Test Travel screen components
            Assert.NotNull(currentLocation);
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();
            Assert.NotNull(travelManager);

            // Test Market screen components
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();
            Assert.NotNull(marketManager);

            // Test Rest screen components
            RestManager restManager = serviceProvider.GetRequiredService<RestManager>();
            Assert.NotNull(restManager);

            // Test Contract screen components
            ContractRepository contractRepository = serviceProvider.GetRequiredService<ContractRepository>();
            Assert.NotNull(contractRepository);

            // Test that GetCurrentLocation() returns valid data for all screens
            Assert.NotNull(currentLocation.Id);
            Assert.NotNull(currentLocation.Name);

            // Test player state is valid for all screens
            Player player = gameWorld.GetPlayer();
            Assert.NotNull(player);
            Assert.NotNull(player.Inventory);
            Assert.True(player.Coins >= 0);
            Assert.True(player.Stamina >= 0);
        }

        [Fact]
        public void TravelScreen_ShouldHandleUserInteractions()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            IServiceProvider serviceProvider = CreateServiceProvider();
            TravelManager travelManager = serviceProvider.GetRequiredService<TravelManager>();

            Location currentLocation = gameWorld.WorldState.CurrentLocation;
            List<Location> knownLocations = gameWorldManager.GetPlayerKnownLocations();

            // Act: Test travel interactions
            foreach (Location destination in knownLocations)
            {
                if (destination.Id != currentLocation.Id)
                {
                    // Test getting routes to destination
                    Exception routeException = Record.Exception(() =>
                    {
                        List<RouteOption> routes = travelManager.GetAvailableRoutes(currentLocation.Id, destination.Id);

                        if (routes.Any())
                        {
                            RouteOption firstRoute = routes.First();
                            // Test route validation
                            bool canTravel = travelManager.CanTravel(firstRoute);
                            // Should not throw exceptions
                        }
                    });
                    Assert.Null(routeException);
                }
            }
        }

        [Fact]
        public void MarketScreen_ShouldHandleTradeInteractions()
        {
            // Arrange
            (GameWorld gameWorld, LocationSystem locationSystem, GameWorldManager gameWorldManager) = InitializeGameToMainGameplay();
            IServiceProvider serviceProvider = CreateServiceProvider();
            MarketManager marketManager = serviceProvider.GetRequiredService<MarketManager>();

            Location currentLocation = gameWorld.WorldState.CurrentLocation;

            // Act: Test market interactions
            Exception tradeException = Record.Exception(() =>
            {
                List<Item> availableItems = marketManager.GetAvailableItems(currentLocation.Id);

                foreach (Item? item in availableItems.Take(3)) // Test first few items
                {
                    // Test buy validation
                    bool canBuy = marketManager.CanBuyItem(item.Id ?? item.Name, currentLocation.Id);

                    // Test sell validation  
                    Player player = gameWorld.GetPlayer();
                    bool canSell = player.Inventory.HasItem(item.Name);

                    // Should not throw exceptions during validation
                }
            });
            Assert.Null(tradeException);
        }
    }
}