using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Service that provides UI data and handles UI actions for the Letter Queue
/// Bridges between UI and game logic, ensuring UI has no direct access to game state
/// </summary>
public class LetterQueueUIService
{
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly CommandExecutor _commandExecutor;
    private readonly MessageSystem _messageSystem;
    private readonly NPCRepository _npcRepository;
    private readonly ITimeManager _timeManager;

    public LetterQueueUIService(
        GameWorld gameWorld,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        StandingObligationManager obligationManager,
        CommandExecutor commandExecutor,
        MessageSystem messageSystem,
        NPCRepository npcRepository,
        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _obligationManager = obligationManager;
        _commandExecutor = commandExecutor;
        _messageSystem = messageSystem;
        _npcRepository = npcRepository;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Get letter queue view model
    /// </summary>
    public LetterQueueViewModel GetQueueViewModel()
    {
        Player player = _gameWorld.GetPlayer();
        ITimeManager currentTime = _timeManager;

        LetterQueueViewModel viewModel = new LetterQueueViewModel
        {
            CurrentTimeBlock = currentTime.GetCurrentTimeBlock(),
            CurrentDay = _gameWorld.CurrentDay,
            LastMorningSwapDay = player.LastMorningSwapDay,
            QueueSlots = new List<QueueSlotViewModel>(),
            Status = GetQueueStatus(),
            Actions = GetQueueActions()
        };

        // Build queue slots
        for (int position = 1; position <= 8; position++)
        {
            Letter? letter = _queueManager.GetLetterAt(position);
            bool canSkip = position > 1 && letter != null && _queueManager.GetLetterAt(1) == null;

            // Calculate skip action details
            SkipActionViewModel? skipAction = null;
            if (canSkip)
            {
                int baseCost = position - 1;
                int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
                int tokenCost = baseCost * multiplier;
                int availableTokens = _tokenManager.GetTokenCount(letter.TokenType);

                // Build detailed multiplier reason
                string multiplierReason = null;
                if (multiplier > 1)
                {
                    var activeObligations = _obligationManager.GetActiveObligations()
                        .Where(o => o.HasEffect(ObligationEffect.TrustSkipDoubleCost) && o.AppliesTo(letter.TokenType))
                        .ToList();
                    
                    if (activeObligations.Any())
                    {
                        var obligationNames = activeObligations.Select(o => o.Name);
                        multiplierReason = $"×{multiplier} from: {string.Join(", ", obligationNames)}";
                    }
                    else
                    {
                        multiplierReason = $"×{multiplier} from active obligations";
                    }
                }

                skipAction = new SkipActionViewModel
                {
                    BaseCost = baseCost,
                    Multiplier = multiplier,
                    TotalCost = tokenCost,
                    TokenType = letter.TokenType.ToString(),
                    AvailableTokens = availableTokens,
                    HasEnoughTokens = availableTokens >= tokenCost,
                    MultiplierReason = multiplierReason
                };
            }

            QueueSlotViewModel slot = new QueueSlotViewModel
            {
                Position = position,
                IsOccupied = letter != null,
                Letter = letter != null ? ConvertToLetterViewModel(letter) : null,
                CanDeliver = position == 1 && letter?.State == LetterState.Collected,
                CanSkip = canSkip,
                SkipAction = skipAction
            };

            viewModel.QueueSlots.Add(slot);
        }

        return viewModel;
    }

    private LetterViewModel ConvertToLetterViewModel(Letter letter)
    {
        (string indicator, string tooltip) leverage = GetLeverageInfo(letter);

        return new LetterViewModel
        {
            Id = letter.Id,
            SenderName = letter.SenderName,
            RecipientName = letter.RecipientName,
            DeadlineInDays = letter.DeadlineInDays,
            Payment = letter.Payment,
            TokenType = letter.TokenType.ToString(),
            TokenIcon = GetTokenIcon(letter.TokenType),
            Size = letter.Size.ToString(),
            SizeIcon = GetSizeIcon(letter.Size),
            IsPatronLetter = letter.IsFromPatron,
            IsCollected = letter.State == LetterState.Collected,
            PhysicalConstraints = letter.GetPhysicalConstraintsDescription(),
            PhysicalIcon = GetPhysicalIcon(letter.PhysicalProperties),
            IsSpecial = letter.IsSpecial,
            SpecialIcon = letter.GetSpecialIcon(),
            SpecialDescription = letter.GetSpecialDescription(),
            DeadlineClass = GetDeadlineClass(letter.DeadlineInDays),
            DeadlineIcon = GetDeadlineIcon(letter.DeadlineInDays),
            DeadlineDescription = GetDeadlineDescription(letter.DeadlineInDays),
            LeverageIndicator = leverage.indicator,
            LeverageTooltip = leverage.tooltip
        };
    }

    private QueueStatusViewModel GetQueueStatus()
    {
        return new QueueStatusViewModel
        {
            LetterCount = _queueManager.GetLetterCount(),
            MaxCapacity = 8,
            ExpiredCount = _queueManager.GetExpiringLetters(0).Length,
            UrgentCount = _queueManager.GetExpiringLetters(1).Length,
            WarningCount = _queueManager.GetExpiringLetters(2).Length
        };
    }

    private QueueActionsViewModel GetQueueActions()
    {
        Player player = _gameWorld.GetPlayer();
        ITimeManager currentTime = _timeManager;

        QueueActionsViewModel actions = new QueueActionsViewModel
        {
            CanMorningSwap = currentTime.GetCurrentTimeBlock() == TimeBlocks.Dawn &&
                           player.LastMorningSwapDay != _gameWorld.CurrentDay,
            MorningSwapReason = GetMorningSwapReason(),
            HasBottomLetter = _queueManager.GetLetterAt(8) != null,
            TotalAvailableTokens = GetTotalAvailableTokens(),
            PurgeTokenOptions = GetPurgeTokenOptions()
        };

        return actions;
    }

    private string GetMorningSwapReason()
    {
        Player player = _gameWorld.GetPlayer();
        ITimeManager currentTime = _timeManager;

        if (currentTime.GetCurrentTimeBlock() != TimeBlocks.Dawn)
            return "Only available at dawn";
        if (player.LastMorningSwapDay == _gameWorld.CurrentDay)
            return "Already used today";
        return "Once per day at dawn";
    }

    private List<TokenOptionViewModel> GetPurgeTokenOptions()
    {
        List<TokenOptionViewModel> options = new List<TokenOptionViewModel>();

        foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
        {
            int available = _tokenManager.GetTokenCount(tokenType);
            if (available > 0)
            {
                options.Add(new TokenOptionViewModel
                {
                    TokenType = tokenType.ToString(),
                    TokenIcon = GetTokenIcon(tokenType),
                    Available = available
                });
            }
        }

        return options;
    }

    /// <summary>
    /// Execute letter delivery
    /// </summary>
    public async Task<bool> DeliverLetterAsync(int position)
    {
        if (position != 1) return false;

        Letter letter = _queueManager.GetLetterAt(position);
        if (letter == null) return false;

        DeliverLetterCommand command = new DeliverLetterCommand(
            letter,
            _queueManager,
            _obligationManager,
            _tokenManager,
            _messageSystem);

        CommandResult result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }

    /// <summary>
    /// Trigger skip conversation
    /// </summary>
    public async Task TriggerSkipConversationAsync(int position)
    {
        await _queueManager.TriggerSkipConversation(position);
    }

    /// <summary>
    /// Execute morning swap
    /// </summary>
    public async Task<bool> MorningSwapAsync(int position1, int position2)
    {
        MorningSwapCommand command = new MorningSwapCommand(
            position1,
            position2,
            _queueManager,
            _tokenManager,
            _messageSystem);

        CommandResult result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute priority move
    /// </summary>
    public async Task<bool> PriorityMoveAsync(int fromPosition)
    {
        PriorityMoveCommand command = new PriorityMoveCommand(
            fromPosition,
            _queueManager,
            _tokenManager,
            _messageSystem);

        CommandResult result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute deadline extension
    /// </summary>
    public async Task<bool> ExtendDeadlineAsync(int position)
    {
        ExtendDeadlineCommand command = new ExtendDeadlineCommand(
            position,
            _queueManager,
            _tokenManager,
            _messageSystem);

        CommandResult result = await _commandExecutor.ExecuteAsync(command);
        return result.IsSuccess;
    }

    /// <summary>
    /// Store purge token selection and trigger conversation
    /// </summary>
    public async Task InitiatePurgeAsync(Dictionary<string, int> tokenSelection)
    {
        // Convert string keys to enum
        Dictionary<ConnectionType, int> enumSelection = new Dictionary<ConnectionType, int>();
        foreach ((string tokenTypeStr, int count) in tokenSelection)
        {
            if (Enum.TryParse<ConnectionType>(tokenTypeStr, out ConnectionType tokenType))
            {
                enumSelection[tokenType] = count;
            }
        }

        // Store for later use in conversation
        string json = System.Text.Json.JsonSerializer.Serialize(enumSelection);
        _gameWorld.SetMetadata("PendingPurgeTokens", json);

        // Trigger conversation
        await _queueManager.TriggerPurgeConversation();
    }

    // Helper methods for visual indicators
    private string GetTokenIcon(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => "❤️",
            ConnectionType.Commerce => "🪙",
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌑",
            _ => "❓"
        };
    }

    private string GetSizeIcon(SizeCategory size)
    {
        return size switch
        {
            SizeCategory.Small => "📄",
            SizeCategory.Medium => "📦",
            SizeCategory.Large => "📦",
            _ => "📦"
        };
    }

    private string GetPhysicalIcon(LetterPhysicalProperties properties)
    {
        if (properties.HasFlag(LetterPhysicalProperties.Fragile)) return "🔮";
        if (properties.HasFlag(LetterPhysicalProperties.Heavy)) return "⚖️";
        if (properties.HasFlag(LetterPhysicalProperties.Perishable)) return "🍃";
        if (properties.HasFlag(LetterPhysicalProperties.Valuable)) return "💎";
        if (properties.HasFlag(LetterPhysicalProperties.RequiresProtection)) return "☔";
        if (properties.HasFlag(LetterPhysicalProperties.Bulky)) return "📏";
        return "📋";
    }

    private string GetDeadlineClass(int deadline)
    {
        return deadline switch
        {
            <= 0 => "deadline-expired",
            1 => "deadline-urgent",
            2 => "deadline-warning",
            _ => "deadline-normal"
        };
    }

    private string GetDeadlineIcon(int deadline)
    {
        return deadline switch
        {
            <= 0 => "💀",
            1 => "🚨",
            2 => "⚠️",
            _ => "⏰"
        };
    }

    private string GetDeadlineDescription(int deadline)
    {
        return deadline switch
        {
            <= 0 => "EXPIRED",
            1 => "Due TODAY",
            2 => "Due in 2 days",
            _ => $"Due in {deadline} days"
        };
    }

    private (string indicator, string tooltip) GetLeverageInfo(Letter letter)
    {
        NPC? npc = _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == letter.SenderName);
        if (npc == null) return ("", "");

        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        int balance = tokens[letter.TokenType];

        string indicator = balance switch
        {
            <= -3 => "🔴🔴🔴",
            -2 => "🔴🔴",
            -1 => "🔴",
            0 => "",
            >= 4 => "🟢",
            _ => ""
        };

        string tooltip = balance < 0
            ? $"You owe {letter.SenderName} {Math.Abs(balance)} {letter.TokenType} tokens - they have leverage!"
            : balance >= 4
                ? $"Strong relationship ({balance} tokens) - reduced leverage"
                : "";

        return (indicator, tooltip);
    }

    private int GetTotalAvailableTokens()
    {
        return Enum.GetValues<ConnectionType>()
            .Sum(type => _tokenManager.GetTokenCount(type));
    }
}