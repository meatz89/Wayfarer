using System.Collections.Generic;

/// <summary>
/// ViewModel for the Letter Queue Display - contains only display data
/// </summary>
public class LetterQueueViewModel
{
    public List<QueueSlotViewModel> QueueSlots { get; init; } = new();
    public QueueStatusViewModel Status { get; init; }
    public QueueActionsViewModel Actions { get; init; }

    // Current time for morning swap availability
    public TimeBlocks CurrentTimeBlock { get; init; }
    public int CurrentDay { get; init; }
    public int LastMorningSwapDay { get; init; }
}

/// <summary>
/// ViewModel for a single queue slot
/// </summary>
public class QueueSlotViewModel
{
    public int Position { get; init; }
    public bool IsOccupied { get; init; }
    public LetterViewModel Letter { get; init; }

    // Action availability
    public bool CanDeliver { get; init; }
    public bool CanSkip { get; init; }
    public SkipActionViewModel SkipAction { get; init; }
}

/// <summary>
/// ViewModel for a letter in the queue
/// </summary>
public class LetterViewModel
{
    public string Id { get; init; }
    public string SenderName { get; init; }
    public string RecipientName { get; init; }
    public int DeadlineInHours { get; init; }
    public int Payment { get; init; }
    public string TokenType { get; init; }
    public string TokenIcon { get; init; }
    public string Size { get; init; }
    public string SizeIcon { get; init; }
    public bool IsPatronLetter { get; init; }
    public bool IsCollected { get; init; }
    public string PhysicalConstraints { get; init; }
    public string PhysicalIcon { get; init; }

    // Special letter properties
    public bool IsSpecial { get; init; }
    public string SpecialIcon { get; init; }
    public string SpecialDescription { get; init; }

    // Visual indicators
    public string DeadlineClass { get; init; }
    public string DeadlineIcon { get; init; }
    public string DeadlineDescription { get; init; }
    public string LeverageIndicator { get; init; }
    public string LeverageTooltip { get; init; }

    // Enhanced leverage information
    public bool HasLeverage { get; init; }
    public int LeverageStrength { get; init; }
    public int TokenBalance { get; init; }
    public string LeverageClass { get; init; }
    public int OriginalPosition { get; init; }
    public int CurrentPosition { get; init; }

    // Obligation effects
    public bool HasPaymentBonus { get; init; }
    public int PaymentBonusAmount { get; init; }
    public string PaymentBonusSource { get; init; }
    public bool HasDeadlineExtension { get; init; }
    public int DeadlineExtensionHours { get; init; }
    public string DeadlineExtensionSource { get; init; }
    public bool HasPositionModifier { get; init; }
    public int PositionModifierAmount { get; init; }
    public string PositionModifierSource { get; init; }
    public List<string> ActiveObligationEffects { get; init; } = new();
}

/// <summary>
/// ViewModel for skip action details
/// </summary>
public class SkipActionViewModel
{
    public int BaseCost { get; init; }
    public int Multiplier { get; init; }
    public int TotalCost { get; init; }
    public string TokenType { get; init; }
    public int AvailableTokens { get; init; }
    public bool HasEnoughTokens { get; init; }
    public string MultiplierReason { get; init; }
}

/// <summary>
/// ViewModel for queue status summary
/// </summary>
public class QueueStatusViewModel
{
    public int LetterCount { get; init; }
    public int MaxCapacity { get; init; }
    public int ExpiredCount { get; init; }
    public int UrgentCount { get; init; }
    public int WarningCount { get; init; }
}

/// <summary>
/// ViewModel for queue management actions
/// </summary>
public class QueueActionsViewModel
{
    public bool CanMorningSwap { get; init; }
    public string MorningSwapReason { get; init; }

    public bool HasBottomLetter { get; init; }
    public int TotalAvailableTokens { get; init; }

    // Token options for purge
    public List<TokenOptionViewModel> PurgeTokenOptions { get; init; } = new();
}

/// <summary>
/// ViewModel for token selection options
/// </summary>
public class TokenOptionViewModel
{
    public string TokenType { get; init; }
    public string TokenIcon { get; init; }
    public int Available { get; init; }
}

/// <summary>
/// Strongly typed leverage information
/// </summary>
public class LeverageInfo
{
    public string Indicator { get; set; }
    public string Tooltip { get; set; }
    public int TokenBalance { get; set; }
    public bool HasLeverage { get; set; }
    public int LeverageStrength { get; set; } // 0-3 for negative leverage
}

/// <summary>
/// Strongly typed token selection for purge action
/// </summary>
public class TokenSelection
{
    public ConnectionType TokenType { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Strongly typed NPC token balance
/// </summary>
public class NPCTokenBalance
{
    public List<TokenBalance> Balances { get; set; } = new();

    public int GetBalance(ConnectionType tokenType)
    {
        TokenBalance? balance = Balances.FirstOrDefault(b => b.TokenType == tokenType);
        return balance?.Amount ?? 0;
    }

    public int TotalTokens => Balances.Sum(b => b.Amount);
}

/// <summary>
/// Individual token balance
/// </summary>
public class TokenBalance
{
    public ConnectionType TokenType { get; set; }
    public int Amount { get; set; }
}