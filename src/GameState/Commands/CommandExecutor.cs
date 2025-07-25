using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Manages the execution of game commands.
/// </summary>
public class CommandExecutor
{
    private readonly ILogger<CommandExecutor> _logger;
    private readonly GameWorld _gameWorld;
    private readonly NarrativeManager _narrativeManager;

    public CommandExecutor(ILogger<CommandExecutor> logger, GameWorld gameWorld, NarrativeManager narrativeManager = null)
    {
        _logger = logger;
        _gameWorld = gameWorld;
        _narrativeManager = narrativeManager;
    }

    /// <summary>
    /// Executes a command with full validation and error handling.
    /// </summary>
    public async Task<CommandResult> ExecuteAsync(IGameCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        _logger.LogDebug("Executing command: {CommandType} - {Description}",
            command.GetType().Name, command.Description);

        try
        {
            // Validate before execution
            CommandValidationResult validation = command.CanExecute(_gameWorld);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Command validation failed: {Reason}", validation.FailureReason);
                return CommandResult.Failure(validation.FailureReason);
            }

            // Execute the command
            CommandResult result = await command.ExecuteAsync(_gameWorld);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Command executed successfully: {CommandType}", command.GetType().Name);
                
                // Notify narrative manager of successful command completion
                if (_narrativeManager != null)
                {
                    _narrativeManager.OnCommandCompleted(command, result);
                }
            }
            else
            {
                _logger.LogWarning("Command execution failed: {ErrorMessage}", result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {CommandType}", command.GetType().Name);
            return CommandResult.Failure($"Command execution error: {ex.Message}");
        }
    }
}