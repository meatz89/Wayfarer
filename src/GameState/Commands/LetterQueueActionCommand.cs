using System;
using System.Collections.Generic;
using System.Threading.Tasks;


/// <summary>
/// Base command for letter queue actions (skip, priority move, extend deadline, purge)
/// </summary>
public abstract class LetterQueueActionCommand : BaseGameCommand
{
    protected readonly LetterQueueManager _queueManager;
    protected readonly ConnectionTokenManager _tokenManager;
    protected readonly MessageSystem _messageSystem;

    protected LetterQueueActionCommand(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _messageSystem = messageSystem;
    }
}

/// <summary>
/// Command for morning swap action
/// </summary>
public class MorningSwapCommand : LetterQueueActionCommand
{
    private readonly int _position1;
    private readonly int _position2;
    public MorningSwapCommand(
        int position1,
        int position2,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
        : base(queueManager, tokenManager, messageSystem)
    {
        _position1 = position1;
        _position2 = position2;
        Description = $"Swap letters in positions {position1} and {position2}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        if (gameWorld.CurrentTimeBlock != TimeBlocks.Dawn)
            return CommandValidationResult.Failure("Morning swap only available at dawn");

        Player player = gameWorld.GetPlayer();
        if (player.LastMorningSwapDay == gameWorld.CurrentDay)
            return CommandValidationResult.Failure("Already used morning swap today");

        if (!_queueManager.CanSwapPositions(_position1, _position2))
            return CommandValidationResult.Failure("Cannot swap these positions");

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        if (_queueManager.TryMorningSwap(_position1, _position2))
        {
            _messageSystem.AddSystemMessage(
                $"Swapped letters in positions {_position1} and {_position2}!",
                SystemMessageTypes.Success);
            return CommandResult.Success($"Swapped positions {_position1} and {_position2}");
        }

        return CommandResult.Failure("Failed to swap positions");
    }

}

/// <summary>
/// Command for priority move action
/// </summary>
public class PriorityMoveCommand : LetterQueueActionCommand
{
    private readonly int _fromPosition;
    private readonly Letter _letter;

    public PriorityMoveCommand(
        int fromPosition,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
        : base(queueManager, tokenManager, messageSystem)
    {
        _fromPosition = fromPosition;
        _letter = queueManager.GetLetterAt(fromPosition);
        Description = $"Priority move letter from position {fromPosition} to position 1";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        if (_fromPosition < 2 || _fromPosition > 8)
            return CommandValidationResult.Failure("Invalid position");

        if (_queueManager.GetLetterAt(1) != null)
            return CommandValidationResult.Failure("Position 1 must be empty");

        if (_letter == null)
            return CommandValidationResult.Failure("No letter at that position");

        const int cost = 5;
        if (_tokenManager.GetTokenCount(_letter.TokenType) < cost)
            return CommandValidationResult.Failure(
                $"Not enough {_letter.TokenType} tokens (need {cost})",
                true,
                $"Earn more {_letter.TokenType} tokens");

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        const int cost = 5;


        // Spend tokens and perform move
        _tokenManager.SpendTokens(_letter.TokenType, cost);

        if (_queueManager.TryPriorityMove(_fromPosition))
        {
            _messageSystem.AddSystemMessage(
                $"Moved letter to position 1! Spent {cost} {_letter.TokenType} tokens",
                SystemMessageTypes.Success);
            return CommandResult.Success($"Priority move successful");
        }

        // Refund if failed
        _tokenManager.AddTokens(_letter.TokenType, cost);
        return CommandResult.Failure("Failed to perform priority move");
    }

}

/// <summary>
/// Command for extending letter deadline
/// </summary>
public class ExtendDeadlineCommand : LetterQueueActionCommand
{
    private readonly int _position;
    private readonly Letter _letter;

    public ExtendDeadlineCommand(
        int position,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
        : base(queueManager, tokenManager, messageSystem)
    {
        _position = position;
        _letter = queueManager.GetLetterAt(position);
        Description = $"Extend deadline for letter at position {position}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        if (_position < 1 || _position > 8)
            return CommandValidationResult.Failure("Invalid position");

        if (_letter == null)
            return CommandValidationResult.Failure("No letter at that position");

        const int cost = 2;
        if (_tokenManager.GetTokenCount(_letter.TokenType) < cost)
            return CommandValidationResult.Failure(
                $"Not enough {_letter.TokenType} tokens (need {cost})",
                true,
                $"Earn more {_letter.TokenType} tokens");

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        const int cost = 2;


        // Spend tokens and extend deadline
        _tokenManager.SpendTokens(_letter.TokenType, cost);

        if (_queueManager.TryExtendDeadline(_position))
        {
            _messageSystem.AddSystemMessage(
                $"Extended deadline by 2 days! Spent {cost} {_letter.TokenType} tokens",
                SystemMessageTypes.Success);
            return CommandResult.Success($"Deadline extended");
        }

        // Refund if failed
        _tokenManager.AddTokens(_letter.TokenType, cost);
        return CommandResult.Failure("Failed to extend deadline");
    }

}

/// <summary>
/// Command for purging bottom letter
/// </summary>
public class PurgeBottomLetterCommand : LetterQueueActionCommand
{
    private readonly Dictionary<ConnectionType, int> _tokenSelection;
    private Letter _purgedLetter;

    public PurgeBottomLetterCommand(
        Dictionary<ConnectionType, int> tokenSelection,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
        : base(queueManager, tokenManager, messageSystem)
    {
        _tokenSelection = tokenSelection;
        Description = "Purge bottom letter from queue";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Letter bottomLetter = _queueManager.GetLetterAt(8);
        if (bottomLetter == null)
            return CommandValidationResult.Failure("No letter at position 8");

        // Validate token selection
        int totalTokens = 0;
        foreach ((ConnectionType tokenType, int count) in _tokenSelection)
        {
            if (count < 0)
                return CommandValidationResult.Failure("Invalid token count");

            if (_tokenManager.GetTokenCount(tokenType) < count)
                return CommandValidationResult.Failure($"Not enough {tokenType} tokens");

            totalTokens += count;
        }

        if (totalTokens != 3)
            return CommandValidationResult.Failure("Must select exactly 3 tokens");

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        // Store letter for tracking
        _purgedLetter = _queueManager.GetLetterAt(8);

        // Spend tokens
        foreach ((ConnectionType tokenType, int count) in _tokenSelection)
        {
            _tokenManager.SpendTokens(tokenType, count);
        }

        // Trigger purge conversation
        await _queueManager.TriggerPurgeConversation();

        return CommandResult.Success("Initiated letter purge");
    }

}