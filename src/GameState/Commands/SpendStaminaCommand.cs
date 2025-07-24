/// <summary>
/// Command to spend stamina from the player.
/// </summary>
public class SpendStaminaCommand : BaseGameCommand
{
    private readonly int _amount;
    private readonly string _activity;

    public SpendStaminaCommand(int amount, string activity = "activity")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        _amount = amount;
        _activity = activity;

        Description = $"Spend {_amount} stamina on {_activity}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        if (player.Stamina < _amount)
        {
            return CommandValidationResult.Failure(
                $"Insufficient stamina: need {_amount}, have {player.Stamina}",
                canBeRemedied: true,
                remediationHint: "Rest at an inn or consume food to restore stamina"
            );
        }

        // Check for dangerous activities requiring minimum stamina
        if (_activity.Contains("dangerous", StringComparison.OrdinalIgnoreCase) && player.Stamina < 4)
        {
            return CommandValidationResult.Failure(
                "Too exhausted for dangerous activities (need at least 4 stamina)",
                canBeRemedied: true,
                remediationHint: "Rest to restore stamina to at least 4"
            );
        }

        return CommandValidationResult.Success();
    }

    public override Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.Failure(validation.FailureReason));
        }

        Player player = gameWorld.GetPlayer();

        // Modify state
        player.ModifyStamina(-_amount);

        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage($"Spent {_amount} stamina on {_activity}"));

        return Task.FromResult(CommandResult.Success(
            $"Successfully spent {_amount} stamina",
            new { RemainingStamina = player.Stamina }
        ));
    }

}