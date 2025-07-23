using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wayfarer.GameState.Commands;
using Wayfarer.GameState.StateContainers;
using Wayfarer.GameState.Validation;

namespace Wayfarer.GameState;

/// <summary>
/// Central manager for game state operations using command pattern and state containers.
/// Ensures all state changes are validated, atomic, and traceable.
/// </summary>
public class GameStateManager
{
    private readonly ILogger<GameStateManager> _logger;
    private readonly CommandExecutor _commandExecutor;
    private readonly GameStateValidator _stateValidator;
    private readonly GameWorld _gameWorld;
    
    public GameStateManager(
        ILogger<GameStateManager> logger,
        ILogger<CommandExecutor> commandLogger,
        GameWorld gameWorld)
    {
        _logger = logger;
        _gameWorld = gameWorld;
        _commandExecutor = new CommandExecutor(commandLogger);
        _stateValidator = new GameStateValidator();
    }
    
    /// <summary>
    /// Executes a command with full validation.
    /// </summary>
    public async Task<CommandResult> ExecuteCommandAsync(IGameCommand command)
    {
        // Validate state before command
        var preValidation = _stateValidator.ValidateGameState(_gameWorld);
        if (!preValidation.IsValid)
        {
            _logger.LogWarning("Pre-command validation failed: {Errors}", 
                string.Join("; ", preValidation.Errors));
        }
        
        // Execute command
        var result = await _commandExecutor.ExecuteAsync(command, _gameWorld);
        
        // Validate state after command
        if (result.Success)
        {
            var postValidation = _stateValidator.ValidateGameState(_gameWorld);
            if (!postValidation.IsValid)
            {
                _logger.LogError("Post-command validation failed: {Errors}", 
                    string.Join("; ", postValidation.Errors));
                
                // Attempt to undo if validation fails
                if (command.CanUndo)
                {
                    await _commandExecutor.UndoLastAsync(_gameWorld);
                    return CommandResult.FailureResult("Command resulted in invalid state and was rolled back");
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Spends player coins using command pattern.
    /// </summary>
    public async Task<CommandResult> SpendCoinsAsync(int amount, string reason)
    {
        var command = new SpendCoinsCommand(amount, reason);
        return await ExecuteCommandAsync(command);
    }
    
    /// <summary>
    /// Spends player stamina using command pattern.
    /// </summary>
    public async Task<CommandResult> SpendStaminaAsync(int amount, string activity)
    {
        var command = new SpendStaminaCommand(amount, activity);
        return await ExecuteCommandAsync(command);
    }
    
    /// <summary>
    /// Advances game time using command pattern.
    /// </summary>
    public async Task<CommandResult> AdvanceTimeAsync(int hours, string reason, bool force = false)
    {
        var command = new AdvanceTimeCommand(hours, reason, force);
        return await ExecuteCommandAsync(command);
    }
    
    /// <summary>
    /// Modifies player inventory using command pattern.
    /// </summary>
    public async Task<CommandResult> AddItemAsync(string itemId, string reason = "found")
    {
        var command = new ModifyInventoryCommand(itemId, ModifyInventoryCommand.InventoryOperation.Add, reason);
        return await ExecuteCommandAsync(command);
    }
    
    /// <summary>
    /// Removes item from player inventory using command pattern.
    /// </summary>
    public async Task<CommandResult> RemoveItemAsync(string itemId, string reason = "used")
    {
        var command = new ModifyInventoryCommand(itemId, ModifyInventoryCommand.InventoryOperation.Remove, reason);
        return await ExecuteCommandAsync(command);
    }
    
    /// <summary>
    /// Executes a complex transaction using composite command.
    /// </summary>
    public async Task<CommandResult> ExecuteTransactionAsync(string description, Action<CompositeCommand> buildTransaction)
    {
        var composite = new CompositeCommand(description);
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
    /// Undoes the last command if possible.
    /// </summary>
    public async Task<CommandResult> UndoLastCommandAsync()
    {
        return await _commandExecutor.UndoLastAsync(_gameWorld);
    }
    
    /// <summary>
    /// Gets the current time state.
    /// </summary>
    public TimeState GetTimeState()
    {
        return new TimeState(_gameWorld.CurrentDay, _gameWorld.TimeManager.CurrentTimeHours);
    }
    
    /// <summary>
    /// Gets the current player resource state.
    /// </summary>
    public PlayerResourceState GetPlayerResourceState()
    {
        var player = _gameWorld.GetPlayer();
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
    
    /// <summary>
    /// Validates the current game state.
    /// </summary>
    public ValidationResult ValidateGameState()
    {
        return _stateValidator.ValidateGameState(_gameWorld);
    }
}