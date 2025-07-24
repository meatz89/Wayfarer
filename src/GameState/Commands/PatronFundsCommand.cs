/// <summary>
/// Command to request funds from patron (costs connection tokens)
/// </summary>
public class PatronFundsCommand : BaseGameCommand
{
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly GameConfiguration _config;


    public PatronFundsCommand(
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem,
        GameConfiguration config)
    {
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        Description = "Request funds from patron";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player is at a location where they can write
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Not at any location");
        }

        // Check if player has accepted patron obligation
        if (!player.HasPatron)
        {
            return CommandValidationResult.Failure(
                "You don't have a patron",
                true,
                "Accept a patron's offer first");
        }

        // Check time availability (1 hour to write letter)
        if (!gameWorld.TimeManager.CanPerformAction(1))
        {
            return CommandValidationResult.Failure(
                "Not enough time remaining",
                true,
                "Rest or wait until tomorrow");
        }

        // Check token cost
        int tokenCost = _config.Patron.FundRequestCost;
        int patronTokens = _tokenManager.GetTokensWithNPC("patron").GetValueOrDefault(ConnectionType.Noble);

        if (patronTokens < tokenCost)
        {
            return CommandValidationResult.Failure(
                $"Not enough patron leverage (need {tokenCost}, have {patronTokens})",
                true,
                "Build leverage by completing patron letters");
        }

        // Check if already requested funds recently (cooldown)
        int daysSinceLastRequest = gameWorld.CurrentDay - player.LastPatronFundDay;
        if (daysSinceLastRequest < 7)
        {
            int daysRemaining = 7 - daysSinceLastRequest;
            return CommandValidationResult.Failure(
                $"Patron requires {daysRemaining} more days before next request",
                true,
                $"Wait {daysRemaining} more days");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Spend resources
        int tokensSpent = _config.Patron.FundRequestCost;
        int coinsReceived = _config.Patron.FundRequestAmount;

        gameWorld.TimeManager.SpendHours(1);
        _tokenManager.SpendTokens(ConnectionType.Noble, tokensSpent, "patron");

        // Add coins
        player.ModifyCoins(coinsReceived);

        // Update last request day
        player.LastPatronFundDay = gameWorld.CurrentDay;

        // Narrative feedback
        _messageSystem.AddSystemMessage(
            "📜 You write a careful letter to your patron explaining your needs...",
            SystemMessageTypes.Info
        );

        _messageSystem.AddSystemMessage(
            $"💰 Received {coinsReceived} coins from patron! (-{tokensSpent} leverage)",
            SystemMessageTypes.Success
        );

        // Add context based on remaining leverage
        int remainingTokens = _tokenManager.GetTokensWithNPC("patron").GetValueOrDefault(ConnectionType.Noble);
        if (remainingTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                "⚠️ Your patron's patience wears thin. You owe them for this favor.",
                SystemMessageTypes.Warning
            );
        }
        else if (remainingTokens == 0)
        {
            _messageSystem.AddSystemMessage(
                "📊 You've spent all your leverage. Complete patron letters to rebuild trust.",
                SystemMessageTypes.Info
            );
        }

        return CommandResult.Success(
            "Patron funds received",
            new
            {
                CoinsReceived = coinsReceived,
                TokensSpent = tokensSpent,
                RemainingLeverage = remainingTokens,
                NextRequestDay = gameWorld.CurrentDay + 7
            }
        );
    }

}