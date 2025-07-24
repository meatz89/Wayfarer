using System.Threading.Tasks;


/// <summary>
/// Command to spend coins from the player's purse.
/// </summary>
public class SpendCoinsCommand : BaseGameCommand
{
    private readonly int _amount;
    private readonly string _reason;

    public SpendCoinsCommand(int amount, string reason = "purchase")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        _amount = amount;
        _reason = reason;

        Description = $"Spend {_amount} coins for {_reason}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        if (player.Coins < _amount)
        {
            return CommandValidationResult.Failure(
                $"Insufficient coins: need {_amount}, have {player.Coins}",
                canBeRemedied: true,
                remediationHint: $"Earn {_amount - player.Coins} more coins"
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
        player.ModifyCoins(-_amount);

        // Add to event log
        gameWorld.SystemMessages.Add(new SystemMessage($"Spent {_amount} coins on {_reason}"));

        return Task.FromResult(CommandResult.Success(
            $"Successfully spent {_amount} coins",
            new { RemainingCoins = player.Coins }
        ));
    }

}