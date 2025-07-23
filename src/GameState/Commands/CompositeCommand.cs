using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// A command that executes multiple sub-commands as a single atomic operation.
/// If any sub-command fails, all previously executed commands are undone.
/// </summary>
public class CompositeCommand : BaseGameCommand
{
    private readonly List<IGameCommand> _commands;
    private readonly List<IGameCommand> _executedCommands;
    private readonly string _description;
    
    public override string Description => _description;
    public override bool CanUndo => _commands.All(c => c.CanUndo);
    
    public CompositeCommand(string description)
    {
        _description = description ?? "Composite operation";
        _commands = new List<IGameCommand>();
        _executedCommands = new List<IGameCommand>();
    }
    
    /// <summary>
    /// Adds a command to the composite operation.
    /// </summary>
    public CompositeCommand AddCommand(IGameCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
            
        _commands.Add(command);
        return this; // Fluent interface
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // All commands must be valid for the composite to be valid
        foreach (var command in _commands)
        {
            var validation = command.CanExecute(gameWorld);
            if (!validation.IsValid)
            {
                return CommandValidationResult.Failure(
                    $"Sub-command validation failed: {command.Description} - {validation.FailureReason}",
                    validation.CanBeRemedied,
                    validation.RemediationHint
                );
            }
        }
        
        return CommandValidationResult.Success();
    }
    
    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        _executedCommands.Clear();
        
        // Validate all commands first
        var validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return CommandResult.FailureResult(validation.FailureReason);
        }
        
        // Execute each command in order
        foreach (var command in _commands)
        {
            try
            {
                var result = await command.ExecuteAsync(gameWorld);
                
                if (!result.Success)
                {
                    // Roll back all previously executed commands
                    await RollbackExecutedCommands(gameWorld);
                    
                    return CommandResult.FailureResult(
                        $"Composite operation failed at: {command.Description} - {result.ErrorMessage}"
                    );
                }
                
                _executedCommands.Add(command);
            }
            catch (Exception ex)
            {
                // Roll back on any exception
                await RollbackExecutedCommands(gameWorld);
                
                return CommandResult.FailureResult(
                    $"Composite operation failed with error at: {command.Description} - {ex.Message}"
                );
            }
        }
        
        return CommandResult.SuccessResult(
            $"Composite operation completed successfully: {_commands.Count} commands executed",
            new { CommandCount = _commands.Count }
        );
    }
    
    public override async Task UndoAsync(GameWorld gameWorld)
    {
        if (!CanUndo)
            throw new InvalidOperationException("One or more sub-commands do not support undo");
            
        // Undo in reverse order
        var errors = new List<string>();
        
        foreach (var command in _executedCommands.AsEnumerable().Reverse())
        {
            try
            {
                await command.UndoAsync(gameWorld);
            }
            catch (Exception ex)
            {
                errors.Add($"{command.Description}: {ex.Message}");
            }
        }
        
        _executedCommands.Clear();
        
        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Errors occurred during undo: {string.Join("; ", errors)}"
            );
        }
    }
    
    private async Task RollbackExecutedCommands(GameWorld gameWorld)
    {
        // Undo in reverse order, ignoring any errors during rollback
        foreach (var command in _executedCommands.AsEnumerable().Reverse())
        {
            try
            {
                if (command.CanUndo)
                {
                    await command.UndoAsync(gameWorld);
                }
            }
            catch
            {
                // Log but continue rollback
            }
        }
        
        _executedCommands.Clear();
    }
}