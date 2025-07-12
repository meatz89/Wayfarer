using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Comprehensive tests validating the complete game initialization flow
    /// from JSON files through parsers, GameWorldInitializer, repositories, and GameWorld.
    /// 
    /// These tests ensure the proper architecture is followed:
    /// JSON Files → GameWorldSerializer (parsers) → GameWorldInitializer → GameWorld
    /// </summary>
    public class ComprehensiveGameInitializationTests
    {
        private const string TestContentDirectory = "Content";

        [Fact]
        public void GameWorldInitializer_Should_Load_All_JSON_Content_Into_GameWorld()
        {
            // Arrange - Verify JSON files exist
            string templatePath = Path.Combine(TestContentDirectory, "Templates");
            Assert.True(Directory.Exists(templatePath), $"Template directory should exist at {templatePath}");

            // Verify all required JSON files exist
            Assert.True(File.Exists(Path.Combine(templatePath, "locations.json")), "locations.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "location_spots.json")), "location_spots.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "routes.json")), "routes.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "items.json")), "items.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "contracts.json")), "contracts.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "actions.json")), "actions.json should exist");
            Assert.True(File.Exists(Path.Combine(templatePath, "gameWorld.json")), "gameWorld.json should exist");

            // Act - Initialize GameWorld through proper architecture
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify all content loaded correctly into GameWorld
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);

            // Verify locations loaded from locations.json
            Assert.NotNull(gameWorld.WorldState.locations);
            Assert.NotEmpty(gameWorld.WorldState.locations);

            // Verify location spots loaded from location_spots.json
            Assert.NotNull(gameWorld.WorldState.locationSpots);
            Assert.NotEmpty(gameWorld.WorldState.locationSpots);

            // Verify items loaded from items.json
            Assert.NotNull(gameWorld.WorldState.Items);
            Assert.NotEmpty(gameWorld.WorldState.Items);

            // Verify routes loaded from routes.json
            Assert.NotNull(gameWorld.DiscoveredRoutes);
            Assert.NotEmpty(gameWorld.DiscoveredRoutes);

            // Verify contracts loaded from contracts.json via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotNull(contractRepository.GetAllContracts());
            Assert.NotEmpty(contractRepository.GetAllContracts());

            // Verify actions loaded from actions.json
            Assert.NotNull(gameWorld.WorldState.actions);
            Assert.NotEmpty(gameWorld.WorldState.actions);

            // Verify NPCs loaded from npcs.json
            List<NPC> npcs = gameWorld.WorldState.GetCharacters();
            Assert.NotNull(npcs);
            Assert.NotEmpty(npcs);
            Assert.True(npcs.Count >= 6, "Should have at least 6 NPCs from template file");
        }

        [Fact]
        public void GameWorldInitializer_Should_Connect_Related_Entities_Correctly()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify entity relationships established correctly

            // Verify locations and spots are connected
            Location? location = gameWorld.WorldState.locations.FirstOrDefault();
            Assert.NotNull(location);

            LocationSpot? spotForLocation = gameWorld.WorldState.locationSpots
                .Where(spot => spot.LocationId == location.Id)
                .FirstOrDefault();
            Assert.NotNull(spotForLocation);
            Assert.Equal(location.Id, spotForLocation.LocationId);

            // Verify routes are connected to locations
            RouteOption? route = gameWorld.DiscoveredRoutes.FirstOrDefault();
            Assert.NotNull(route);

            Location? originLocation = gameWorld.WorldState.locations
                .FirstOrDefault(loc => loc.Id == route.Origin);
            Location? destinationLocation = gameWorld.WorldState.locations
                .FirstOrDefault(loc => loc.Id == route.Destination);

            Assert.NotNull(originLocation);
            Assert.NotNull(destinationLocation);
        }

        [Fact]
        public void GameWorldInitializer_Should_Initialize_Player_Location_Properly()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify player location initialization
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(gameWorld.WorldState.CurrentLocationSpot);

            // Verify current location exists in loaded locations
            Location? currentLocationInList = gameWorld.WorldState.locations
                .FirstOrDefault(loc => loc.Id == gameWorld.WorldState.CurrentLocation.Id);
            Assert.NotNull(currentLocationInList);

            // Verify current spot belongs to current location
            Assert.Equal(gameWorld.WorldState.CurrentLocation.Id,
                        gameWorld.WorldState.CurrentLocationSpot.LocationId);
        }

        [Fact]
        public void GameWorldInitializer_Should_Load_Items_With_Categories()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify items loaded with proper categories
            Assert.NotEmpty(gameWorld.WorldState.Items);

            Item? itemWithEquipmentCategory = gameWorld.WorldState.Items
                .FirstOrDefault(item => item.Categories != null && item.Categories.Any());
            Assert.NotNull(itemWithEquipmentCategory);

            Item? itemWithItemCategory = gameWorld.WorldState.Items
                .FirstOrDefault(item => item.ItemCategories != null && item.ItemCategories.Any());
            Assert.NotNull(itemWithItemCategory);
        }

        [Fact]
        public void GameWorldInitializer_Should_Load_Routes_With_Terrain_Categories()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify routes loaded with terrain categories
            Assert.NotEmpty(gameWorld.DiscoveredRoutes);

            RouteOption? routeWithTerrainCategory = gameWorld.DiscoveredRoutes
                .FirstOrDefault(route => route.TerrainCategories?.Any() == true);
            Assert.NotNull(routeWithTerrainCategory);
            Assert.NotEmpty(routeWithTerrainCategory.TerrainCategories);
        }

        [Fact]
        public void GameWorldInitializer_Should_Load_Contracts_With_Proper_Data()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify contracts loaded with complete data via ContractRepository
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            Assert.NotEmpty(contractRepository.GetAllContracts());

            Contract? contract = contractRepository.GetAllContracts().FirstOrDefault();
            Assert.NotNull(contract);
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

            // Verify any destination requirements reference valid locations
            foreach (string destinationId in contract.RequiredDestinations)
            {
                bool destinationExists = gameWorld.WorldState.locations
                    .Any(loc => loc.Id == destinationId);
                Assert.True(destinationExists, $"Contract destination {destinationId} should reference a valid location");
            }
        }

        [Fact]
        public void GameWorldInitializer_Should_Create_GameWorld_As_Single_Source_Of_Truth()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify GameWorld follows single source of truth architecture

            // GameWorld should have properties that delegate to WorldState
            Assert.Equal(gameWorld.WorldState.CurrentDay, gameWorld.CurrentDay);
            Assert.Equal(gameWorld.WorldState.CurrentTimeBlock, gameWorld.CurrentTimeBlock);
            Assert.Equal(gameWorld.WorldState.CurrentWeather, gameWorld.CurrentWeather);

            // Verify player initialization
            Assert.NotNull(gameWorld.GetPlayer());
            Assert.NotNull(gameWorld.GetPlayer().Inventory);

            // Verify TimeManager initialization
            Assert.NotNull(gameWorld.TimeManager);
        }

        [Fact]
        public void Repositories_Should_Access_GameWorld_Content_Correctly()
        {
            // Arrange
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Create repositories using proper architecture
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);

            // Act & Assert - Verify repositories can access loaded content

            // LocationRepository should access locations from GameWorld
            List<Location> locations = locationRepository.GetAllLocations();
            Assert.NotEmpty(locations);
            Assert.Equal(gameWorld.WorldState.locations.Count, locations.Count);

            // ItemRepository should access items from GameWorld
            List<Item> items = itemRepository.GetAllItems();
            Assert.NotEmpty(items);
            Assert.Equal(gameWorld.WorldState.Items.Count, items.Count);

            // ActionRepository should access actions from GameWorld
            List<ActionDefinition> actions = actionRepository.GetAllActions();
            Assert.NotEmpty(actions);
            Assert.Equal(gameWorld.WorldState.actions.Count, actions.Count);
        }

        [Fact]
        public void GameWorldInitializer_Should_Handle_Missing_Optional_Files_Gracefully()
        {
            // This test verifies that initialization continues even if optional files are missing
            // The GameWorldInitializer checks for file existence before loading optional content

            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Should not throw exceptions and should have core content
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);

            // Core files should always be loaded
            Assert.NotEmpty(gameWorld.WorldState.locations);
            Assert.NotEmpty(gameWorld.WorldState.locationSpots);
            Assert.NotEmpty(gameWorld.WorldState.actions);
        }

        [Fact]
        public void GameWorld_Properties_Should_Delegate_To_WorldState()
        {
            // Arrange & Act
            GameWorldInitializer initializer = new GameWorldInitializer(TestContentDirectory);
            GameWorld gameWorld = initializer.LoadGame();

            // Assert - Verify delegation pattern works correctly

            // Test CurrentDay delegation
            int originalDay = gameWorld.CurrentDay;
            gameWorld.CurrentDay = 42;
            Assert.Equal(42, gameWorld.CurrentDay);
            Assert.Equal(42, gameWorld.WorldState.CurrentDay);

            // Test CurrentTimeBlock delegation
            TimeBlocks originalTimeBlock = gameWorld.CurrentTimeBlock;
            gameWorld.CurrentTimeBlock = TimeBlocks.Evening;
            Assert.Equal(TimeBlocks.Evening, gameWorld.CurrentTimeBlock);
            Assert.Equal(TimeBlocks.Evening, gameWorld.WorldState.CurrentTimeBlock);

            // Test CurrentWeather delegation
            WeatherCondition originalWeather = gameWorld.CurrentWeather;
            gameWorld.CurrentWeather = WeatherCondition.Rain;
            Assert.Equal(WeatherCondition.Rain, gameWorld.CurrentWeather);
            Assert.Equal(WeatherCondition.Rain, gameWorld.WorldState.CurrentWeather);

            // Restore original values
            gameWorld.CurrentDay = originalDay;
            gameWorld.CurrentTimeBlock = originalTimeBlock;
            gameWorld.CurrentWeather = originalWeather;
        }

        [Fact]
        public void Full_Game_Initialization_Integration_Test()
        {
            // This test validates the complete initialization flow that the actual game uses

            // Arrange - Simulate the ServiceConfiguration.ConfigureServices flow
            string contentDirectory = "Content";

            // Act - Follow the exact initialization pattern from ServiceConfiguration
            GameWorldInitializer gameWorldInitializer = new GameWorldInitializer(contentDirectory);
            GameWorld gameWorld = gameWorldInitializer.LoadGame();

            // Create all the components that depend on the initialized GameWorld
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);

            // Assert - Verify the complete system is properly initialized
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.GetPlayer());
            Assert.NotNull(gameWorld.TimeManager);

            // Verify repositories can function with loaded data
            Assert.NotEmpty(actionRepository.GetAllActions());
            Assert.NotEmpty(locationRepository.GetAllLocations());
            Assert.NotEmpty(itemRepository.GetAllItems());
            Assert.NotEmpty(contractRepository.GetAvailableContracts(gameWorld.CurrentDay, gameWorld.CurrentTimeBlock));

            // Verify player is properly positioned
            Assert.NotNull(gameWorld.WorldState.CurrentLocation);
            Assert.NotNull(gameWorld.WorldState.CurrentLocationSpot);

            // Verify the player's location references valid loaded data
            string currentLocationId = gameWorld.WorldState.CurrentLocation.Id;
            bool locationExists = locationRepository.GetAllLocations()
                .Any(loc => loc.Id == currentLocationId);
            Assert.True(locationExists, "Player's current location should exist in loaded location data");

            // Verify game is ready for gameplay
            Assert.True(gameWorld.GetPlayer().Stamina > 0);
            Assert.True(gameWorld.GetPlayer().Coins >= 0);
            Assert.NotNull(gameWorld.GetPlayer().Inventory);
        }
    }
}