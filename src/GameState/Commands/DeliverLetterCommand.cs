/// <summary>
/// Command for delivering a letter from position 1 in the queue
/// </summary>
public class DeliverLetterCommand : BaseGameCommand
{
    private readonly Letter _letter;
    private readonly LetterQueueManager _queueManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;


    public DeliverLetterCommand(
        Letter letter,
        LetterQueueManager queueManager,
        StandingObligationManager obligationManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _letter = letter;
        _queueManager = queueManager;
        _obligationManager = obligationManager;
        _tokenManager = tokenManager;
        _messageSystem = messageSystem;

        Description = $"Deliver letter from {letter.SenderName} to {letter.RecipientName}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        if (_letter == null)
            return CommandValidationResult.Failure("No letter to deliver");

        if (_letter.QueuePosition != 1)
            return CommandValidationResult.Failure("Can only deliver from position 1");

        if (_letter.State != LetterState.Collected)
            return CommandValidationResult.Failure(
                $"Letter not collected! Visit {_letter.SenderName} first",
                true,
                $"Visit {_letter.SenderName} to collect the physical letter");

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();


        // Calculate payment with bonuses
        int basePayment = _letter.Payment;
        int bonusPayment = _obligationManager.CalculateTotalCoinBonus(_letter);
        int totalPayment = basePayment + bonusPayment;

        // Award payment
        player.ModifyCoins(totalPayment);

        // 50% chance to earn a token
        Random random = new Random();
        bool earnedToken = random.Next(2) == 0;
        if (earnedToken && !string.IsNullOrEmpty(_letter.SenderId))
        {
            _tokenManager.AddTokensToNPC(_letter.TokenType, 1, _letter.SenderId);
        }

        // Track delivery
        _queueManager.RecordLetterDelivery(_letter);

        // Remove from queue
        _queueManager.RemoveLetterFromQueue(1);

        // Build success message
        string message = $"Delivered letter! Earned {totalPayment} coins";
        if (bonusPayment > 0)
        {
            message += $" (base {basePayment} + {bonusPayment} bonus)";
        }
        if (earnedToken)
        {
            message += $" and 1 {_letter.TokenType} token!";
        }

        _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);

        return CommandResult.Success(message, new
        {
            TotalPayment = totalPayment,
            BasePayment = basePayment,
            BonusPayment = bonusPayment,
            EarnedToken = earnedToken,
            TokenType = _letter.TokenType
        });
    }

}