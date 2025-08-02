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
    private readonly NetworkUnlockManager _networkUnlockManager;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly InformationDiscoveryManager _informationDiscoveryManager;


    public DeliverLetterCommand(
        Letter letter,
        LetterQueueManager queueManager,
        StandingObligationManager obligationManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem,
        NetworkUnlockManager networkUnlockManager = null,
        RouteDiscoveryManager routeDiscoveryManager = null,
        InformationDiscoveryManager informationDiscoveryManager = null)
    {
        _letter = letter;
        _queueManager = queueManager;
        _obligationManager = obligationManager;
        _tokenManager = tokenManager;
        _messageSystem = messageSystem;
        _networkUnlockManager = networkUnlockManager;
        _routeDiscoveryManager = routeDiscoveryManager;
        _informationDiscoveryManager = informationDiscoveryManager;

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
        
        // Handle special letter effects
        if (_letter.SpecialType != LetterSpecialType.None)
        {
            HandleSpecialLetterEffect(_letter, gameWorld);
        }

        return CommandResult.Success(message, new
        {
            TotalPayment = totalPayment,
            BasePayment = basePayment,
            BonusPayment = bonusPayment,
            EarnedToken = earnedToken,
            TokenType = _letter.TokenType,
            SpecialType = _letter.SpecialType
        });
    }
    
    private void HandleSpecialLetterEffect(Letter letter, GameWorld gameWorld)
    {
        switch (letter.SpecialType)
        {
            case LetterSpecialType.Introduction:
                if (!string.IsNullOrEmpty(letter.UnlocksNPCId) && _networkUnlockManager != null)
                {
                    // Find the introducer NPC (sender)
                    var introducer = gameWorld.WorldState.NPCs.FirstOrDefault(n => n.Name == letter.SenderName);
                    if (introducer != null)
                    {
                        bool unlocked = _networkUnlockManager.UnlockNetworkContact(introducer.ID, letter.UnlocksNPCId);
                        if (unlocked)
                        {
                            var unlockedNpc = gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == letter.UnlocksNPCId);
                            _messageSystem.AddSystemMessage(
                                $"🤝 Letter of Introduction! You can now interact with {unlockedNpc?.Name ?? "a new contact"}!",
                                SystemMessageTypes.Success
                            );
                        }
                    }
                }
                break;
                
            case LetterSpecialType.AccessPermit:
                if (!string.IsNullOrEmpty(letter.UnlocksLocationId))
                {
                    var player = gameWorld.GetPlayer();
                    if (!player.UnlockedLocationIds.Contains(letter.UnlocksLocationId))
                    {
                        player.UnlockedLocationIds.Add(letter.UnlocksLocationId);
                        _messageSystem.AddSystemMessage(
                            $"🔓 Access Permit! You can now visit restricted location: {letter.UnlocksLocationId}!",
                            SystemMessageTypes.Success
                        );
                    }
                }
                break;
                
            case LetterSpecialType.Endorsement:
                if (letter.BonusDuration > 0)
                {
                    // Record endorsement in player's memory system
                    var player = gameWorld.GetPlayer();
                    int endDate = gameWorld.CurrentDay + letter.BonusDuration;
                    
                    player.AddMemory(
                        $"endorsement_{letter.SenderId}_{letter.Tier}",
                        GetEndorsementEffectDescription(letter),
                        gameWorld.CurrentDay,
                        letter.Tier
                    );
                    
                    // Store expiration for future checking
                    player.AddMemory(
                        $"endorsement_{letter.SenderId}_{letter.Tier}_expires",
                        endDate.ToString(),
                        gameWorld.CurrentDay,
                        1
                    );
                    
                    _messageSystem.AddSystemMessage(
                        $"📜 Letter of Endorsement! You receive {GetEndorsementBenefitSummary(letter)} for {letter.BonusDuration} days!",
                        SystemMessageTypes.Success
                    );
                }
                break;
                
            case LetterSpecialType.Information:
                if (!string.IsNullOrEmpty(letter.InformationId) && _informationDiscoveryManager != null)
                {
                    bool discovered = _informationDiscoveryManager.DiscoverInformation(letter.InformationId);
                    if (discovered)
                    {
                        _messageSystem.AddSystemMessage(
                            $"🔍 Confidential Information revealed! Check your discoveries.",
                            SystemMessageTypes.Success
                        );
                    }
                }
                break;
        }
    }
    
    private string GetEndorsementEffectDescription(Letter letter)
    {
        return letter.Tier switch
        {
            1 => "Minor social standing improvement - Status letters pay better",
            2 or 3 => "Moderate social privileges - Status letters get priority handling",
            4 or 5 => "Major social influence - Significant advantages for Status work",
            _ => "Social privileges"
        };
    }
    
    private string GetEndorsementBenefitSummary(Letter letter)
    {
        return letter.Tier switch
        {
            1 => "better pay for Status letters",
            2 or 3 => "priority handling for Status letters",
            4 or 5 => "significant Status letter advantages",
            _ => "temporary benefits"
        };
    }

}