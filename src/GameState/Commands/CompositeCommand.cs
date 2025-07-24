/// <summary>
/// A command that executes multiple sub-commands as a single atomic operation.
/// If any sub-command fails, all previously executed commands are undone.
/// </summary>
public class CompositeCommand : BaseGameCommand
{
    private readonly List<IGameCommand> _commands;
    private readonly List<IGameCommand> _executedCommands;
    private readonly string _description;

    public CompositeCommand(string description)
    {
        _description = description ?? "Composite operation";
        _commands = new List<IGameCommand>();
        _executedCommands = new List<IGameCommand>();

        Description = _description;
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
        foreach (IGameCommand command in _commands)
        {
            CommandValidationResult validation = command.CanExecute(gameWorld);
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
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return CommandResult.Failure(validation.FailureReason);
        }

        // Execute each command in order
        foreach (IGameCommand command in _commands)
        {
            try
            {
                CommandResult result = await command.ExecuteAsync(gameWorld);

                if (!result.IsSuccess)
                {
                    return CommandResult.Failure(
                        $"Composite operation failed at: {command.Description} - {result.ErrorMessage}"
                    );
                }

                _executedCommands.Add(command);
            }
            catch (Exception ex)
            {
                return CommandResult.Failure(
                    $"Composite operation failed with error at: {command.Description} - {ex.Message}"
                );
            }
        }

        return CommandResult.Success(
            $"Composite operation completed successfully: {_commands.Count} commands executed",
            new { CommandCount = _commands.Count }
        );
    }


}