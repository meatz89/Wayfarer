using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests;

/// <summary>
/// Fluent API for building test scenarios declaratively.
/// Allows tests to describe WHAT game state they need, not HOW to create it.
/// 
/// Example usage:
/// var scenario = new TestScenarioBuilder()
///     .WithPlayer(p => p.StartAt("dusty_flagon").WithCoins(50).WithItem("herbs"))
///     .WithContracts(c => c.Add("herb_delivery").RequiresSell("herbs", "town_square").Pays(10))
///     .WithTimeState(t => t.Day(1).TimeBlock(TimeBlocks.Morning));
/// 
/// GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);
/// </summary>
public class TestScenarioBuilder
{
    private readonly List<Action<GameWorld>> _configurators = new List<Action<GameWorld>>();

    /// <summary>
    /// Configure player starting state
    /// </summary>
    public TestScenarioBuilder WithPlayer(Action<PlayerBuilder> playerConfig)
    {
        PlayerBuilder playerBuilder = new PlayerBuilder();
        playerConfig(playerBuilder);

        _configurators.Add(gameWorld => playerBuilder.ApplyToPlayer(gameWorld.GetPlayer(), gameWorld));

        return this;
    }

    /// <summary>
    /// Configure contracts available in the game world
    /// </summary>
    public TestScenarioBuilder WithContracts(Action<ContractBuilder> contractConfig)
    {
        ContractBuilder contractBuilder = new ContractBuilder();
        contractConfig(contractBuilder);

        _configurators.Add(gameWorld => contractBuilder.ApplyToGameWorld(gameWorld));

        return this;
    }

    /// <summary>
    /// Configure time and weather state
    /// </summary>
    public TestScenarioBuilder WithTimeState(Action<TimeBuilder> timeConfig)
    {
        TimeBuilder timeBuilder = new TimeBuilder();
        timeConfig(timeBuilder);

        _configurators.Add(gameWorld => timeBuilder.ApplyToGameWorld(gameWorld));

        return this;
    }

    /// <summary>
    /// Configure additional items available in the world
    /// </summary>
    public TestScenarioBuilder WithItems(Action<ItemBuilder> itemConfig)
    {
        ItemBuilder itemBuilder = new ItemBuilder();
        itemConfig(itemBuilder);

        _configurators.Add(gameWorld => itemBuilder.ApplyToGameWorld(gameWorld));

        return this;
    }

    /// <summary>
    /// Configure additional locations in the world
    /// </summary>
    public TestScenarioBuilder WithLocations(Action<LocationBuilder> locationConfig)
    {
        LocationBuilder locationBuilder = new LocationBuilder();
        locationConfig(locationBuilder);

        _configurators.Add(gameWorld => locationBuilder.ApplyToGameWorld(gameWorld));

        return this;
    }

    /// <summary>
    /// Apply all configured settings to the game world
    /// </summary>
    public void ApplyToGameWorld(GameWorld gameWorld)
    {
        // Start with basic test data
        TestGameWorldInitializer.SetupBasicTestData(gameWorld);

        // Apply all custom configurations
        foreach (Action<GameWorld> configurator in _configurators)
        {
            configurator(gameWorld);
        }
    }

    /// <summary>
    /// Build the scenario (returns this for fluent API consistency)
    /// </summary>
    public TestScenarioBuilder Build()
    {
        return this;
    }
}

/// <summary>
/// Builder for player configuration
/// </summary>
public class PlayerBuilder
{
    private string _startLocationId = "dusty_flagon";
    private int _coins = 50;
    private int _stamina = 10;
    private int _actionPoints = 18;
    private int _maxActionPoints = 18;
    private int _reputation = 0;
    private readonly List<string> _inventory = new List<string>();
    private readonly List<string> _knownContracts = new List<string>();

    public PlayerBuilder StartAt(string locationId)
    {
        _startLocationId = locationId;
        return this;
    }

    public PlayerBuilder WithCoins(int coins)
    {
        _coins = coins;
        return this;
    }

    public PlayerBuilder WithStamina(int stamina)
    {
        _stamina = stamina;
        return this;
    }

    public PlayerBuilder WithActionPoints(int actionPoints)
    {
        _actionPoints = actionPoints;
        return this;
    }

    public PlayerBuilder WithMaxActionPoints(int maxActionPoints)
    {
        _maxActionPoints = maxActionPoints;
        return this;
    }

    public PlayerBuilder WithReputation(int reputation)
    {
        _reputation = reputation;
        return this;
    }

    public PlayerBuilder WithItem(string itemId)
    {
        _inventory.Add(itemId);
        return this;
    }

    public PlayerBuilder WithItems(params string[] itemIds)
    {
        _inventory.AddRange(itemIds);
        return this;
    }

    public PlayerBuilder KnowsContract(string contractId)
    {
        _knownContracts.Add(contractId);
        return this;
    }

    public void ApplyToPlayer(Player player, GameWorld gameWorld)
    {
        // Set basic stats
        player.Coins = _coins;
        player.Stamina = _stamina;
        player.ActionPoints = _actionPoints;
        player.MaxActionPoints = _maxActionPoints;
        player.Reputation = _reputation;

        // Add inventory items
        foreach (string item in _inventory)
        {
            player.Inventory.AddItem(item);
        }

        // Add known contracts
        foreach (string contractId in _knownContracts)
        {
            player.DiscoverContract(contractId);
        }

        // Set location
        Location? startLocation = gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == _startLocationId);
        if (startLocation != null)
        {
            player.CurrentLocation = startLocation;
            gameWorld.WorldState.SetCurrentLocation(startLocation, null);
        }
    }
}

/// <summary>
/// Builder for contract configuration with convenient helper methods
/// </summary>
public class ContractBuilder
{
    private readonly List<Contract> _contracts = new List<Contract>();

    /// <summary>
    /// Add a contract with manual ID specification
    /// </summary>
    public ContractSingleBuilder Add(string contractId)
    {
        return new ContractSingleBuilder(this, contractId);
    }


    internal void AddContract(Contract contract)
    {
        _contracts.Add(contract);
    }

    public void ApplyToGameWorld(GameWorld gameWorld)
    {
        if (gameWorld.WorldState.Contracts == null)
        {
            gameWorld.WorldState.Contracts = new List<Contract>();
        }

        gameWorld.WorldState.Contracts.AddRange(_contracts);
    }
}

/// <summary>
/// Builder for a single contract
/// </summary>
public class ContractSingleBuilder
{
    private readonly ContractBuilder _parent;
    private readonly Contract _contract;
    private static int _contractCounter = 1;

    public ContractSingleBuilder(ContractBuilder parent, string contractId)
    {
        _parent = parent;
        _contract = CreateContract(contractId);
    }

    public ContractSingleBuilder(ContractBuilder parent)
    {
        _parent = parent;
        string autoId = $"test_contract_{_contractCounter++}";
        _contract = CreateContract(autoId);
    }

    private static Contract CreateContract(string contractId)
    {
        return new Contract
        {
            Id = contractId,
            Description = $"Test contract: {contractId}",
            StartDay = 1,
            DueDay = 6,
            Payment = 10,
            FailurePenalty = "Loss of reputation",
            IsCompleted = false,
            IsFailed = false,
        };
    }

    public ContractSingleBuilder Pays(int payment)
    {
        _contract.Payment = payment;
        return this;
    }

    public ContractSingleBuilder DueInDays(int days)
    {
        _contract.DueDay = _contract.StartDay + days;
        return this;
    }

    public ContractSingleBuilder WithDescription(string description)
    {
        _contract.Description = description;
        return this;
    }

    public ContractBuilder Build()
    {
        _parent.AddContract(_contract);
        return _parent;
    }
}

/// <summary>
/// Builder for time and weather state
/// </summary>
public class TimeBuilder
{
    private int _day = 1;
    private TimeBlocks _timeBlock = TimeBlocks.Morning;
    private WeatherCondition _weather = WeatherCondition.Clear;
    private int _usedTimeBlocks = 0;
    private int? _explicitHour = null;

    public TimeBuilder Day(int day)
    {
        _day = day;
        return this;
    }

    public TimeBuilder TimeBlock(TimeBlocks timeBlock)
    {
        _timeBlock = timeBlock;
        return this;
    }

    public TimeBuilder Hour(int hour)
    {
        _explicitHour = hour;
        // Also set the corresponding time block for consistency
        // Dawn: 6-8, Morning: 9-11, Afternoon: 12-15, Evening: 16-19, Night: 20-5
        if (hour >= 6 && hour < 9)
            _timeBlock = TimeBlocks.Dawn;
        else if (hour >= 9 && hour < 12)
            _timeBlock = TimeBlocks.Morning;
        else if (hour >= 12 && hour < 16)
            _timeBlock = TimeBlocks.Afternoon;
        else if (hour >= 16 && hour < 20)
            _timeBlock = TimeBlocks.Evening;
        else
            _timeBlock = TimeBlocks.Night;

        return this;
    }

    public TimeBuilder Weather(WeatherCondition weather)
    {
        _weather = weather;
        return this;
    }

    public TimeBuilder UsedTimeBlocks(int used)
    {
        _usedTimeBlocks = used;
        return this;
    }

    public void ApplyToGameWorld(GameWorld gameWorld)
    {
        gameWorld.WorldState.CurrentDay = _day;
        gameWorld.WorldState.CurrentWeather = _weather;

        // Use explicit hour if set, otherwise calculate from time block
        int targetHour = _explicitHour ?? _timeBlock switch
        {
            TimeBlocks.Dawn => 6,      // 6:00 AM
            TimeBlocks.Morning => 9,   // 9:00 AM  
            TimeBlocks.Afternoon => 12, // 12:00 PM
            TimeBlocks.Evening => 16,   // 4:00 PM
            TimeBlocks.Night => 20,     // 8:00 PM
            _ => 6
        };

        // Use the existing TimeManager from GameWorld
        gameWorld.TimeManager.SetNewTime(targetHour);
    }
}

/// <summary>
/// Builder for additional items
/// </summary>
public class ItemBuilder
{
    private readonly List<Item> _items = new List<Item>();

    public ItemSingleBuilder Add(string itemId)
    {
        return new ItemSingleBuilder(this, itemId);
    }

    internal void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void ApplyToGameWorld(GameWorld gameWorld)
    {
        if (gameWorld.WorldState.Items == null)
        {
            gameWorld.WorldState.Items = new List<Item>();
        }

        gameWorld.WorldState.Items.AddRange(_items);
    }
}

/// <summary>
/// Builder for a single item
/// </summary>
public class ItemSingleBuilder
{
    private readonly ItemBuilder _parent;
    private readonly Item _item;

    public ItemSingleBuilder(ItemBuilder parent, string itemId)
    {
        _parent = parent;
        _item = new Item
        {
            Id = itemId,
            Name = itemId,
            BuyPrice = 10,
            SellPrice = 8,
            Weight = 1,
            Description = $"Test item: {itemId}",
            Categories = new List<EquipmentCategory>(),
            ItemCategories = new List<ItemCategory> { ItemCategory.Trade_Goods }
        };
    }

    public ItemSingleBuilder WithName(string name)
    {
        _item.Name = name;
        return this;
    }

    public ItemSingleBuilder SellsAt(string locationId, int price)
    {
        // Note: This would need integration with MarketManager pricing system
        // For now, we set the base sell price
        _item.SellPrice = price;
        return this;
    }

    public ItemSingleBuilder BuysAt(string locationId, int price)
    {
        // Note: This would need integration with MarketManager pricing system
        // For now, we set the base buy price
        _item.BuyPrice = price;
        return this;
    }

    public ItemSingleBuilder WithWeight(int weight)
    {
        _item.Weight = weight;
        return this;
    }

    public ItemBuilder Build()
    {
        _parent.AddItem(_item);
        return _parent;
    }
}

/// <summary>
/// Builder for additional locations
/// </summary>
public class LocationBuilder
{
    private readonly List<Location> _locations = new List<Location>();

    public LocationSingleBuilder Add(string locationId)
    {
        return new LocationSingleBuilder(this, locationId);
    }

    internal void AddLocation(Location location)
    {
        _locations.Add(location);
    }

    public void ApplyToGameWorld(GameWorld gameWorld)
    {
        if (gameWorld.WorldState.locations == null)
        {
            gameWorld.WorldState.locations = new List<Location>();
        }

        gameWorld.WorldState.locations.AddRange(_locations);
    }
}

/// <summary>
/// Builder for a single location
/// </summary>
public class LocationSingleBuilder
{
    private readonly LocationBuilder _parent;
    private readonly Location _location;

    public LocationSingleBuilder(LocationBuilder parent, string locationId)
    {
        _parent = parent;
        _location = new Location(locationId, locationId)
        {
            Description = $"Test location: {locationId}"
        };
    }

    public LocationSingleBuilder WithName(string name)
    {
        // Note: Location.Name is private set, so we can't change it after construction
        // This would require a different approach or constructor overload
        return this;
    }

    public LocationSingleBuilder WithDescription(string description)
    {
        _location.Description = description;
        return this;
    }

    public LocationBuilder Build()
    {
        _parent.AddLocation(_location);
        return _parent;
    }
}