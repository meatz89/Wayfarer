using System.Threading.Tasks;


/// <summary>
/// Interface for all game commands that modify state.
/// Commands encapsulate state changes and provide validation.
/// </summary>
public interface IGameCommand
{
    /// <summary>
    /// Unique identifier for this command instance.
    /// </summary>
    string CommandId { get; }

    /// <summary>
    /// Human-readable description of what this command does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Validates whether this command can be executed in the current game state.
    /// </summary>
    CommandValidationResult CanExecute(GameWorld gameWorld);

    /// <summary>
    /// Executes the command and modifies the game state.
    /// </summary>
    Task<CommandResult> ExecuteAsync(GameWorld gameWorld);
}

/// <summary>
/// Result of command validation.
/// </summary>
public class CommandValidationResult
{
    public bool IsValid { get; init; }
    public string FailureReason { get; init; }
    public bool CanBeRemedied { get; init; }
    public string RemediationHint { get; init; }

    public static CommandValidationResult Success()
    {
        return new() { IsValid = true };
    }

    public static CommandValidationResult Failure(string reason, bool canBeRemedied = false, string remediationHint = null)
    {
        return new()
        {
            IsValid = false,
            FailureReason = reason,
            CanBeRemedied = canBeRemedied,
            RemediationHint = remediationHint
        };
    }
}

/// <summary>
/// Result of command execution.
/// </summary>
public class CommandResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public object Data { get; init; }
    public string ErrorMessage { get; init; }

    public static CommandResult Success(string message = null, object data = null)
    {
        return new()
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static CommandResult Failure(string errorMessage)
    {
        return new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}