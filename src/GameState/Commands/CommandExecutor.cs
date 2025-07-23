using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// Manages the execution of game commands, including history and undo functionality.
/// </summary>
public class CommandExecutor
{
    private readonly ILogger<CommandExecutor> _logger;
    private readonly Stack<IGameCommand> _executedCommands;
    private readonly int _maxHistorySize;
    
    public IReadOnlyList<IGameCommand> CommandHistory => _executedCommands.ToList();
    
    public CommandExecutor(ILogger<CommandExecutor> logger, int maxHistorySize = 100)
    {
        _logger = logger;
        _executedCommands = new Stack<IGameCommand>();
        _maxHistorySize = maxHistorySize;
    }
    
    /// <summary>
    /// Executes a command with full validation and error handling.
    /// </summary>
    public async Task<CommandResult> ExecuteAsync(IGameCommand command, GameWorld gameWorld)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        if (gameWorld == null)
            throw new ArgumentNullException(nameof(gameWorld));
            
        _logger.LogDebug("Executing command: {CommandType} - {Description}", 
            command.GetType().Name, command.Description);
        
        try
        {
            // Validate before execution
            var validation = command.CanExecute(gameWorld);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Command validation failed: {Reason}", validation.FailureReason);
                return CommandResult.FailureResult(validation.FailureReason);
            }
            
            // Execute the command
            var result = await command.ExecuteAsync(gameWorld);
            
            if (result.Success)
            {
                // Add to history if successful and supports undo
                if (command.CanUndo)
                {
                    _executedCommands.Push(command);
                    
                    // Trim history if it exceeds max size
                    while (_executedCommands.Count > _maxHistorySize)
                    {
                        var oldCommands = _executedCommands.ToArray();
                        _executedCommands.Clear();
                        foreach (var cmd in oldCommands.Take(_maxHistorySize))
                        {
                            _executedCommands.Push(cmd);
                        }
                    }
                }
                
                _logger.LogInformation("Command executed successfully: {CommandType}", command.GetType().Name);
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
            return CommandResult.FailureResult($"Command execution error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Undoes the last executed command that supports undo.
    /// </summary>
    public async Task<CommandResult> UndoLastAsync(GameWorld gameWorld)
    {
        if (!_executedCommands.Any())
        {
            return CommandResult.FailureResult("No commands to undo");
        }
        
        var command = _executedCommands.Pop();
        
        try
        {
            _logger.LogDebug("Undoing command: {CommandType} - {Description}", 
                command.GetType().Name, command.Description);
                
            await command.UndoAsync(gameWorld);
            
            _logger.LogInformation("Command undone successfully: {CommandType}", command.GetType().Name);
            return CommandResult.SuccessResult($"Undone: {command.Description}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error undoing command: {CommandType}", command.GetType().Name);
            
            // Put the command back on the stack since undo failed
            _executedCommands.Push(command);
            
            return CommandResult.FailureResult($"Undo failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clears the command history.
    /// </summary>
    public void ClearHistory()
    {
        _executedCommands.Clear();
        _logger.LogDebug("Command history cleared");
    }
    
    /// <summary>
    /// Gets the number of undoable commands in history.
    /// </summary>
    public int GetUndoableCommandCount() => _executedCommands.Count;
}