using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

public class TravelTimeConsumptionTests
{
    private GameWorld CreateTestGameWorld()
    {
        GameWorldInitializer initializer = new GameWorldInitializer("Content");
        return initializer.LoadGame();
    }

    [Fact]
    public void TravelToLocation_Should_Consume_Time_Blocks()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
        ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
        RouteRepository routeRepository = new RouteRepository(gameWorld);
        TravelManager travelManager = new TravelManager(
            gameWorld,
            new LocationSystem(gameWorld, locationRepository),
            new ActionRepository(gameWorld),
            locationRepository,
            new ActionFactory(new ActionRepository(gameWorld), gameWorld, itemRepository, contractRepository, contractValidation),
            itemRepository,
            contractProgression,
            new TransportCompatibilityValidator(itemRepository),
            routeRepository
        );

        int initialTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
        
        // Find a valid route from the JSON data
        string currentLocationId = gameWorld.CurrentLocation.Id;
        List<RouteOption> availableRoutes = travelManager.GetAvailableRoutes(currentLocationId, "town_square");
        
        // Skip test if no routes available (this should not happen with proper test data)
        if (!availableRoutes.Any())
        {
            Assert.True(false, "No available routes found for testing");
            return;
        }

        RouteOption testRoute = availableRoutes.First();
        int expectedTimeBlockCost = testRoute.TimeBlockCost;

        // Act
        travelManager.TravelToLocation("town_square", "main_square", testRoute);

        // Assert
        int finalTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
        Assert.Equal(initialTimeBlocks + expectedTimeBlockCost, finalTimeBlocks);
    }

    [Fact]
    public void AdvanceTimeBlocks_Should_Update_TimeManager()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        int initialTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
        int blocksToAdvance = 2;

        // Get access to the private method through reflection
        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
        ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
        RouteRepository routeRepository = new RouteRepository(gameWorld);
        var travelManager = new TravelManager(
            gameWorld,
            new LocationSystem(gameWorld, locationRepository),
            new ActionRepository(gameWorld),
            locationRepository,
            new ActionFactory(new ActionRepository(gameWorld), gameWorld, itemRepository, contractRepository, contractValidation),
            itemRepository,
            contractProgression,
            new TransportCompatibilityValidator(itemRepository),
            routeRepository
        );

        // We can't directly test the private AdvanceTimeBlocks method,
        // but we can test TimeManager.ConsumeTimeBlock directly
        gameWorld.TimeManager.ConsumeTimeBlock(blocksToAdvance);

        // Assert
        int finalTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
        Assert.Equal(initialTimeBlocks + blocksToAdvance, finalTimeBlocks);
    }

    [Fact]
    public void Travel_Should_Respect_Time_Block_Limits()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        
        // Consume all available time blocks except 1
        while (gameWorld.TimeManager.UsedTimeBlocks < TimeManager.MaxDailyTimeBlocks - 1)
        {
            gameWorld.TimeManager.ConsumeTimeBlock(1);
        }

        LocationRepository locationRepository = new LocationRepository(gameWorld);
        ItemRepository itemRepository = new ItemRepository(gameWorld);
        ContractRepository contractRepository = new ContractRepository(gameWorld);
        ContractValidationService contractValidation = new ContractValidationService(contractRepository, itemRepository);
        ContractProgressionService contractProgression = new ContractProgressionService(contractRepository, itemRepository, locationRepository);
        RouteRepository routeRepository = new RouteRepository(gameWorld);
        TravelManager travelManager = new TravelManager(
            gameWorld,
            new LocationSystem(gameWorld, locationRepository),
            new ActionRepository(gameWorld),
            locationRepository,
            new ActionFactory(new ActionRepository(gameWorld), gameWorld, itemRepository, contractRepository, contractValidation),
            itemRepository,
            contractProgression,
            new TransportCompatibilityValidator(itemRepository),
            routeRepository
        );

        // Find a route that would exceed the daily limit
        string currentLocationId = gameWorld.CurrentLocation.Id;
        List<RouteOption> availableRoutes = travelManager.GetAvailableRoutes(currentLocationId, "town_square");
        
        if (!availableRoutes.Any())
        {
            Assert.True(false, "No available routes found for testing");
            return;
        }

        RouteOption testRoute = availableRoutes.First();
        
        // If the route costs more than 1 time block, it should throw an exception
        if (testRoute.TimeBlockCost > 1)
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                travelManager.TravelToLocation("town_square", "main_square", testRoute)
            );
        }
        else
        {
            // If route only costs 1 block, it should succeed
            int initialTimeBlocks = gameWorld.TimeManager.UsedTimeBlocks;
            travelManager.TravelToLocation("town_square", "main_square", testRoute);
            Assert.Equal(initialTimeBlocks + 1, gameWorld.TimeManager.UsedTimeBlocks);
        }
    }
}