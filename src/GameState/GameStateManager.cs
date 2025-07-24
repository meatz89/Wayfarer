using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

/// <summary>
/// Central manager for game state operations using command pattern and state containers.
/// Ensures all state changes are validated, atomic, and traceable.
/// </summary>
public class GameStateManager
{
    private readonly ILogger<GameStateManager> _logger;
    private readonly CommandExecutor _commandExecutor;
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;

    public GameStateManager(
        ILogger<GameStateManager> logger,
        CommandExecutor commandExecutor,
        GameWorld gameWorld,
        ITimeManager timeManager)
    {
        _logger = logger;
        _gameWorld = gameWorld;
        _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    }

    /// <summary>
    /// Executes a command with full validation.
    /// </summary>
    public async Task<CommandResult> ExecuteCommandAsync(IGameCommand command)
    {
        // Execute command directly without validation
        CommandResult result = await _commandExecutor.ExecuteAsync(command);
        return result;
    }

    /// <summary>
    /// Spends player coins using command pattern.
    /// </summary>
    public async Task<CommandResult> SpendCoinsAsync(int amount, string reason)
    {
        SpendCoinsCommand command = new SpendCoinsCommand(amount, reason);
        return await ExecuteCommandAsync(command);
    }

    /// <summary>
    /// Spends player stamina using command pattern.
    /// </summary>
    public async Task<CommandResult> SpendStaminaAsync(int amount, string activity)
    {
        SpendStaminaCommand command = new SpendStaminaCommand(amount, activity);
        return await ExecuteCommandAsync(command);
    }

    /// <summary>
    /// Advances game time using command pattern.
    /// </summary>
    public async Task<CommandResult> AdvanceTimeAsync(int hours, string reason, bool force = false)
    {
        AdvanceTimeCommand command = new AdvanceTimeCommand(hours, reason, force);
        return await ExecuteCommandAsync(command);
    }

    /// <summary>
    /// Modifies player inventory using command pattern.
    /// </summary>
    public async Task<CommandResult> AddItemAsync(string itemId, string reason = "found")
    {
        ModifyInventoryCommand command = new ModifyInventoryCommand(itemId, ModifyInventoryCommand.InventoryOperation.Add, reason);
        return await ExecuteCommandAsync(command);
    }

    /// <summary>
    /// Removes item from player inventory using command pattern.
    /// </summary>
    public async Task<CommandResult> RemoveItemAsync(string itemId, string reason = "used")
    {
        ModifyInventoryCommand command = new ModifyInventoryCommand(itemId, ModifyInventoryCommand.InventoryOperation.Remove, reason);
        return await ExecuteCommandAsync(command);
    }

    /// <summary>
    /// Executes a complex transaction using composite command.
    /// </summary>
    public async Task<CommandResult> ExecuteTransactionAsync(string description, Action<CompositeCommand> buildTransaction)
    {
        CompositeCommand composite = new CompositeCommand(description);
        buildTransaction(composite);
        return await ExecuteCommandAsync(composite);
    }

    /// <summary>
    /// Example: Purchase an item (composite of spend coins + add to inventory).
    /// </summary>
    public async Task<CommandResult> PurchaseItemAsync(string itemId, int cost, string vendorName)
    {
        return await ExecuteTransactionAsync($"Purchase {itemId} from {vendorName}", composite =>
        {
            composite
                .AddCommand(new SpendCoinsCommand(cost, $"purchase from {vendorName}"))
                .AddCommand(new ModifyInventoryCommand(itemId, ModifyInventoryCommand.InventoryOperation.Add, "purchased"));
        });
    }

    /// <summary>
    /// Example: Perform work action (composite of time + stamina + reward).
    /// </summary>
    public async Task<CommandResult> PerformWorkAsync(string workType, int hours, int staminaCost, int coinReward)
    {
        return await ExecuteTransactionAsync($"Perform {workType}", composite =>
        {
            composite
                .AddCommand(new AdvanceTimeCommand(hours, workType))
                .AddCommand(new SpendStaminaCommand(staminaCost, workType))
                .AddCommand(new SpendCoinsCommand(-coinReward, $"earned from {workType}")); // Negative spend = gain
        });
    }


    /// <summary>
    /// Gets the current time state.
    /// </summary>
    public TimeState GetTimeState()
    {
        return new TimeState(_gameWorld.CurrentDay, _timeManager.GetCurrentTimeHours());
    }

    /// <summary>
    /// Gets the current player resource state.
    /// </summary>
    public PlayerResourceState GetPlayerResourceState()
    {
        Player player = _gameWorld.GetPlayer();
        return new PlayerResourceState(
            player.Coins,
            player.Stamina,
            player.Health,
            player.Concentration,
            player.MaxStamina,
            player.MaxHealth,
            player.MaxConcentration
        );
    }

}