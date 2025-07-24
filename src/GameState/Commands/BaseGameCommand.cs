/// <summary>
/// Base class for game commands providing common functionality.
/// </summary>
public abstract class BaseGameCommand : IGameCommand
{
    public string CommandId { get; }
    public string CommandType { get; protected set; }
    public string Description { get; protected set; }

    protected BaseGameCommand()
    {
        CommandId = Guid.NewGuid().ToString();
    }

    public abstract CommandValidationResult CanExecute(GameWorld gameWorld);

    public abstract Task<CommandResult> ExecuteAsync(GameWorld gameWorld);

    /// <summary>
    /// Helper method to check if player has sufficient resources.
    /// </summary>
    protected CommandValidationResult ValidatePlayerResources(Player player, int coinsRequired = 0, int staminaRequired = 0)
    {
        if (coinsRequired > 0 && player.Coins < coinsRequired)
        {
            return CommandValidationResult.Failure(
                $"Insufficient coins. Need {coinsRequired}, have {player.Coins}",
                canBeRemedied: true,
                remediationHint: $"Earn {coinsRequired - player.Coins} more coins"
            );
        }

        if (staminaRequired > 0 && player.Stamina < staminaRequired)
        {
            return CommandValidationResult.Failure(
                $"Insufficient stamina. Need {staminaRequired}, have {player.Stamina}",
                canBeRemedied: true,
                remediationHint: "Rest to restore stamina"
            );
        }

        return CommandValidationResult.Success();
    }
}