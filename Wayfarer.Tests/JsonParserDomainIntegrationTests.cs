using System.IO;
using System.Text.Json;
using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Integration tests that verify the entire JSON→Parser→Domain→Manager pipeline works correctly.
    /// These tests ensure that JSON content is properly structured, parsers work correctly,
    /// domain objects are valid, and managers can operate with the parsed data.
    /// </summary>
    public class JsonParserDomainIntegrationTests
    {
        /// <summary>
        /// Test that all route JSON files can be parsed into valid RouteOption domain objects
        /// and that the domain objects have expected properties for use by TravelManager
        /// </summary>
        [Fact]
        public void RouteJsonParsing_Should_ProduceValidDomainObjects_ForTravelManager()
        {
            // Arrange
            string routesJsonPath = Path.Combine("Content", "Templates", "routes.json");

            // Act & Assert - JSON file exists and is readable
            Assert.True(File.Exists(routesJsonPath), $"Routes JSON file not found at {routesJsonPath}");
            string routesJson = File.ReadAllText(routesJsonPath);
            Assert.NotEmpty(routesJson);

            // Act & Assert - JSON can be parsed into array
            using JsonDocument doc = JsonDocument.Parse(routesJson);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
            JsonElement[] routeElements = doc.RootElement.EnumerateArray().ToArray();
            Assert.NotEmpty(routeElements);

            // Act & Assert - Each route element can be parsed into valid RouteOption
            List<RouteOption> parsedRoutes = new List<RouteOption>();
            foreach (JsonElement routeElement in routeElements)
            {
                string singleRouteJson = routeElement.GetRawText();
                RouteOption route = RouteOptionParser.ParseRouteOption(singleRouteJson);

                // Validate required properties are set
                Assert.NotEmpty(route.Id);
                Assert.NotEmpty(route.Name);
                Assert.NotEmpty(route.Origin);
                Assert.NotEmpty(route.Destination);
                Assert.True(route.BaseCoinCost >= 0);
                Assert.True(route.BaseStaminaCost >= 0);
                Assert.True(route.TimeBlockCost >= 0);
                Assert.True(route.MaxItemCapacity > 0);

                // Validate enum parsing worked correctly
                Assert.True(Enum.IsDefined(typeof(TravelMethods), route.Method));

                // Validate terrain categories (if any) are valid enum values
                foreach (TerrainCategory category in route.TerrainCategories)
                {
                    Assert.True(Enum.IsDefined(typeof(TerrainCategory), category));
                }

                parsedRoutes.Add(route);
            }

            // Act & Assert - TravelManager can work with parsed routes
            GameWorld gameWorld = CreateTestGameWorld();
            TravelManager travelManager = CreateTravelManager(gameWorld);

            // Add parsed routes to game world for testing
            foreach (RouteOption route in parsedRoutes)
            {
                AddRouteToGameWorld(gameWorld, route);
            }

            // Verify TravelManager can get available routes
            List<RouteOption> availableRoutes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            Assert.NotEmpty(availableRoutes);

            // Verify TravelManager can calculate costs for routes
            foreach (RouteOption route in availableRoutes)
            {
                int staminaCost = travelManager.CalculateStaminaCost(route);
                int coinCost = travelManager.CalculateCoinCost(route);

                Assert.True(staminaCost >= 0);
                Assert.True(coinCost >= 0);
            }
        }

        /// <summary>
        /// Test that all item JSON files can be parsed into valid Item domain objects
        /// and that the domain objects have expected properties for use by ItemRepository and MarketManager
        /// </summary>
        [Fact]
        public void ItemJsonParsing_Should_ProduceValidDomainObjects_ForMarketManager()
        {
            // Arrange
            string itemsJsonPath = Path.Combine("Content", "Templates", "items.json");

            // Act & Assert - JSON file exists and is readable
            Assert.True(File.Exists(itemsJsonPath), $"Items JSON file not found at {itemsJsonPath}");
            string itemsJson = File.ReadAllText(itemsJsonPath);
            Assert.NotEmpty(itemsJson);

            // Act & Assert - JSON can be parsed into array
            using JsonDocument doc = JsonDocument.Parse(itemsJson);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
            JsonElement[] itemElements = doc.RootElement.EnumerateArray().ToArray();
            Assert.NotEmpty(itemElements);

            // Act & Assert - Each item element can be parsed into valid Item
            List<Item> parsedItems = new List<Item>();
            foreach (JsonElement itemElement in itemElements)
            {
                string singleItemJson = itemElement.GetRawText();
                Item item = ItemParser.ParseItem(singleItemJson);

                // Validate required properties are set
                Assert.NotEmpty(item.Id);
                Assert.NotEmpty(item.Name);
                Assert.True(item.Weight >= 0);
                Assert.True(item.BuyPrice >= 0);
                Assert.True(item.SellPrice >= 0);
                Assert.True(item.InventorySlots > 0);
                Assert.NotEmpty(item.LocationId);
                Assert.NotEmpty(item.SpotId);

                // Validate equipment categories (if any) are valid enum values
                foreach (EquipmentCategory category in item.Categories)
                {
                    Assert.True(Enum.IsDefined(typeof(EquipmentCategory), category));
                }

                parsedItems.Add(item);
            }

            // Act & Assert - ItemRepository and MarketManager can work with parsed items
            GameWorld gameWorld = CreateTestGameWorld();
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Add parsed items using repository methods (proper architecture)
            itemRepository.AddItems(parsedItems);
            LocationSystem locationSystem = new LocationSystem(gameWorld, new LocationRepository(gameWorld));
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, new LocationRepository(gameWorld));
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            // Verify ItemRepository can find items
            foreach (Item item in parsedItems)
            {
                Item foundItem = itemRepository.GetItemByName(item.Name);
                Assert.NotNull(foundItem);
                Assert.Equal(item.Id, foundItem.Id);
            }

            // Verify ItemRepository can get items for trading
            List<Item> availableItems = itemRepository.GetItemsForLocation("town_square");
            Assert.NotEmpty(availableItems);

            // Verify MarketManager can calculate prices
            foreach (Item item in availableItems)
            {
                int buyPrice = marketManager.GetItemPrice("town_square", item.Id, true);
                int sellPrice = marketManager.GetItemPrice("town_square", item.Id, false);

                Assert.True(buyPrice >= 0);
                Assert.True(sellPrice >= 0);
            }
        }

        /// <summary>
        /// Test that equipment categories in items correctly match terrain categories in routes
        /// for the logical blocking system to work
        /// </summary>
        [Fact]
        public void EquipmentCategories_Should_MatchTerrainCategories_ForLogicalBlocking()
        {
            // Arrange - Parse both routes and items
            string routesJson = File.ReadAllText(Path.Combine("Content", "Templates", "routes.json"));
            string itemsJson = File.ReadAllText(Path.Combine("Content", "Templates", "items.json"));

            using JsonDocument routesDoc = JsonDocument.Parse(routesJson);
            using JsonDocument itemsDoc = JsonDocument.Parse(itemsJson);

            // Act - Parse all routes and items
            List<RouteOption> routes = new List<RouteOption>();
            foreach (JsonElement routeElement in routesDoc.RootElement.EnumerateArray())
            {
                RouteOption route = RouteOptionParser.ParseRouteOption(routeElement.GetRawText());
                routes.Add(route);
            }

            List<Item> items = new List<Item>();
            foreach (JsonElement itemElement in itemsDoc.RootElement.EnumerateArray())
            {
                Item item = ItemParser.ParseItem(itemElement.GetRawText());
                items.Add(item);
            }

            // Assert - Verify logical relationships exist

            // Find routes that require climbing equipment
            List<RouteOption> climbingRoutes = routes
                .Where(r => r.TerrainCategories.Contains(TerrainCategory.Requires_Climbing))
                .ToList();

            // Find items that provide climbing equipment
            List<Item> climbingItems = items
                .Where(i => i.Categories.Contains(EquipmentCategory.Climbing_Equipment))
                .ToList();

            // If there are climbing routes, there should be climbing equipment items
            if (climbingRoutes.Any())
            {
                Assert.NotEmpty(climbingItems);
            }

            // Test the logical blocking system integration
            GameWorld gameWorld = CreateTestGameWorld();
            Player player = gameWorld.GetPlayer();
            ItemRepository itemRepository = new ItemRepository(gameWorld);

            // Add parsed items and routes to the clean GameWorld for this integration test
            itemRepository.AddItems(items);
            // Note: Routes would need RouteRepository if available, but for this test we'll work with what we have

            // Test route access without equipment
            foreach (RouteOption route in climbingRoutes)
            {
                bool canAccess = route.CanTravel(itemRepository, player, 0);
                // Should be blocked without climbing equipment
                Assert.False(canAccess);
            }

            // Test route access with equipment
            foreach (Item climbingItem in climbingItems)
            {
                player.Inventory.AddItem(climbingItem.Id);

                foreach (RouteOption route in climbingRoutes)
                {
                    bool canAccess = route.CanTravel(itemRepository, player, 0);
                    // Should be allowed with climbing equipment
                    Assert.True(canAccess);
                }

                player.Inventory.RemoveItem(climbingItem.Id);
            }
        }

        /// <summary>
        /// Test that the JSON content supports the current game mechanics
        /// by verifying essential game scenarios work end-to-end
        /// </summary>
        [Fact]
        public void JsonContent_Should_SupportEssentialGameScenarios()
        {
            // Arrange - Set up complete game world with JSON content
            GameWorld gameWorld = CreateTestGameWorld();
            LoadJsonContentIntoGameWorld(gameWorld);

            ItemRepository itemRepository = new ItemRepository(gameWorld);
            TravelManager travelManager = CreateTravelManager(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, new LocationRepository(gameWorld));
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, new LocationRepository(gameWorld));
            MarketManager marketManager = new MarketManager(gameWorld, locationSystem, itemRepository, contractProgression);

            Player player = gameWorld.GetPlayer();
            player.Coins = 100; // Start with some money
            player.Stamina = 6;  // Full stamina

            // Scenario 1: Player should be able to travel between basic locations
            List<RouteOption> routes = travelManager.GetAvailableRoutes("dusty_flagon", "town_square");
            Assert.NotEmpty(routes);

            RouteOption walkingRoute = routes.FirstOrDefault(r => r.Method == TravelMethods.Walking);
            Assert.NotNull(walkingRoute);

            bool canTravel = travelManager.CanTravel(walkingRoute);
            Assert.True(canTravel);

            // Scenario 2: Player should be able to buy and sell items
            List<Item> marketItems = itemRepository.GetItemsForLocation("town_square");
            Assert.NotEmpty(marketItems);

            Item cheapItem = marketItems.OrderBy(i => marketManager.GetItemPrice("town_square", i.Id, true)).First();
            int buyPrice = marketManager.GetItemPrice("town_square", cheapItem.Id, true);

            Assert.True(buyPrice <= player.Coins); // Should be able to afford at least one item

            // Scenario 3: Equipment categories should provide logical route access
            Item equipmentItem = marketItems.FirstOrDefault(i => i.Categories.Any());
            if (equipmentItem != null)
            {
                // Before having equipment
                List<RouteOption> allRoutes = GetAllRoutesFromJson();
                List<RouteOption> restrictedRoutes = allRoutes
                    .Where(r => r.TerrainCategories.Any())
                    .ToList();

                if (restrictedRoutes.Any())
                {
                    // Add equipment to inventory
                    player.Inventory.AddItem(equipmentItem.Name);

                    // Some routes should now be accessible
                    bool anyRouteAccessible = restrictedRoutes.Any(r => r.CanTravel(itemRepository, player, 0));
                    Assert.True(anyRouteAccessible || !restrictedRoutes.Any());
                }
            }
        }

        #region Helper Methods

        private GameWorld CreateTestGameWorld()
        {
            // Create clean GameWorld for JSON parsing tests - don't load JSON data since these tests 
            // are specifically testing JSON parsing and want to control their own data loading
            var gameWorld = new GameWorld();
            
            // Initialize basic collections but don't load from JSON
            gameWorld.WorldState.locations = new List<Location>();
            gameWorld.WorldState.Items = new List<Item>();
            gameWorld.WorldState.Contracts = new List<Contract>();
            
            // Add minimal test locations needed for these tests
            gameWorld.WorldState.locations.AddRange(new[]
            {
                new Location("dusty_flagon", "The Dusty Flagon"),
                new Location("town_square", "Town Square"),
                new Location("workshop", "Workshop")
            });
            
            // Initialize player
            Player player = gameWorld.GetPlayer();
            player.Initialize("Test Player", Professions.Merchant, Genders.Male);
            player.Coins = 50;
            
            // Set basic time state
            gameWorld.WorldState.CurrentDay = 1;
            gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;
            gameWorld.WorldState.CurrentWeather = WeatherCondition.Clear;
            
            // Set starting location
            Location startLocation = gameWorld.WorldState.locations.First(l => l.Id == "dusty_flagon");
            player.CurrentLocation = startLocation;
            gameWorld.WorldState.SetCurrentLocation(startLocation, null);
            
            // Initialize empty contract list
            gameWorld.ActiveContracts = new List<Contract>();
            
            return gameWorld;
        }

        private TravelManager CreateTravelManager(GameWorld gameWorld)
        {
            LocationRepository locationRepository = new LocationRepository(gameWorld);
            ActionRepository actionRepository = new ActionRepository(gameWorld);
            LocationSystem locationSystem = new LocationSystem(gameWorld, locationRepository);
            ItemRepository itemRepository = new ItemRepository(gameWorld);
            ContractRepository contractRepository = new ContractRepository(gameWorld);
            ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
            ActionFactory actionFactory = new ActionFactory(actionRepository, gameWorld, itemRepository, contractRepository, contractValidation);

            ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
            return new TravelManager(
                gameWorld,
                locationSystem,
                actionRepository,
                locationRepository,
                actionFactory,
                itemRepository,
                contractProgression
            );
        }

        private void AddRouteToGameWorld(GameWorld gameWorld, RouteOption route)
        {
            LocationRepository locationRepository = new LocationRepository(gameWorld);

            // Ensure origin and destination locations exist
            Location originLocation = locationRepository.GetLocation(route.Origin);
            if (originLocation == null)
            {
                originLocation = new Location(route.Origin, route.Origin);
                locationRepository.AddLocation(originLocation);
            }

            Location destLocation = locationRepository.GetLocation(route.Destination);
            if (destLocation == null)
            {
                destLocation = new Location(route.Destination, route.Destination);
                locationRepository.AddLocation(destLocation);
            }

            // Add route connection
            LocationConnection connection = originLocation.Connections
                .FirstOrDefault(c => c.DestinationLocationId == route.Destination);

            if (connection == null)
            {
                connection = new LocationConnection
                {
                    DestinationLocationId = route.Destination,
                    RouteOptions = new List<RouteOption>()
                };
                originLocation.Connections.Add(connection);
            }

            connection.RouteOptions.Add(route);
        }

        private void LoadJsonContentIntoGameWorld(GameWorld gameWorld)
        {
            // Load routes
            string routesJson = File.ReadAllText(Path.Combine("Content", "Templates", "routes.json"));
            using JsonDocument routesDoc = JsonDocument.Parse(routesJson);

            foreach (JsonElement routeElement in routesDoc.RootElement.EnumerateArray())
            {
                RouteOption route = RouteOptionParser.ParseRouteOption(routeElement.GetRawText());
                AddRouteToGameWorld(gameWorld, route);
            }

            // Load items using repository (proper architecture)
            string itemsJson = File.ReadAllText(Path.Combine("Content", "Templates", "items.json"));
            using JsonDocument itemsDoc = JsonDocument.Parse(itemsJson);

            ItemRepository itemRepository = new ItemRepository(gameWorld);
            foreach (JsonElement itemElement in itemsDoc.RootElement.EnumerateArray())
            {
                Item item = ItemParser.ParseItem(itemElement.GetRawText());
                itemRepository.AddItem(item);
            }
        }

        private List<RouteOption> GetAllRoutesFromJson()
        {
            string routesJson = File.ReadAllText(Path.Combine("Content", "Templates", "routes.json"));
            using JsonDocument doc = JsonDocument.Parse(routesJson);

            List<RouteOption> routes = new List<RouteOption>();
            foreach (JsonElement routeElement in doc.RootElement.EnumerateArray())
            {
                RouteOption route = RouteOptionParser.ParseRouteOption(routeElement.GetRawText());
                routes.Add(route);
            }

            return routes;
        }

        #endregion
    }
}