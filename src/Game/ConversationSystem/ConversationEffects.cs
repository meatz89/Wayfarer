using System;

/// <summary>
/// Effect for reordering a letter in the queue during conversations.
/// Spends tokens and moves the letter to target position.
/// </summary>
public class LetterReorderEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly int _targetPosition;
    private readonly int _tokenCost;
    private readonly ConnectionType _tokenType;
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly string _npcId;

    public LetterReorderEffect(
        string letterId, 
        int targetPosition, 
        int tokenCost, 
        ConnectionType tokenType,
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        string npcId)
    {
        _letterId = letterId;
        _targetPosition = targetPosition;
        _tokenCost = tokenCost;
        _tokenType = tokenType;
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _npcId = npcId;
    }

    public void Apply(ConversationState state)
    {
        // Spend tokens for the reorder
        if (_tokenCost > 0)
        {
            bool spent = _tokenManager.SpendTokensWithNPC(_tokenType, _tokenCost, _npcId);
            if (!spent)
            {
                throw new InvalidOperationException($"Failed to spend {_tokenCost} {_tokenType} tokens for letter reorder");
            }
        }

        // Find current position of the letter
        int? currentPosition = _queueManager.GetLetterPosition(_letterId);
        if (!currentPosition.HasValue)
        {
            throw new InvalidOperationException($"Letter {_letterId} not found in queue");
        }

        // Move letter to target position
        Letter letter = _queueManager.GetLetterAt(currentPosition.Value);
        _queueManager.RemoveLetterFromQueue(currentPosition.Value);
        _queueManager.MoveLetterToPosition(letter, _targetPosition);
    }

    public string GetDescriptionForPlayer()
    {
        if (_tokenCost > 0)
        {
            return $"Letter prioritized (spent {_tokenCost} {_tokenType} tokens)";
        }
        else
        {
            return "Letter prioritized";
        }
    }
}

/// <summary>
/// Effect for gaining tokens during conversations.
/// </summary>
public class GainTokensEffect : IMechanicalEffect
{
    private readonly ConnectionType _tokenType;
    private readonly int _amount;
    private readonly string _npcId;
    private readonly ConnectionTokenManager _tokenManager;

    public GainTokensEffect(
        ConnectionType tokenType, 
        int amount, 
        string npcId,
        ConnectionTokenManager tokenManager)
    {
        _tokenType = tokenType;
        _amount = amount;
        _npcId = npcId;
        _tokenManager = tokenManager;
    }

    public void Apply(ConversationState state)
    {
        _tokenManager.AddTokensToNPC(_tokenType, _amount, _npcId);
    }

    public string GetDescriptionForPlayer()
    {
        return $"Strengthened {_tokenType} connection";
    }
}

/// <summary>
/// Effect for burning/spending tokens during conversations.
/// </summary>
public class BurnTokensEffect : IMechanicalEffect
{
    private readonly ConnectionType _tokenType;
    private readonly int _amount;
    private readonly string _npcId;
    private readonly ConnectionTokenManager _tokenManager;

    public BurnTokensEffect(
        ConnectionType tokenType, 
        int amount, 
        string npcId,
        ConnectionTokenManager tokenManager)
    {
        _tokenType = tokenType;
        _amount = amount;
        _npcId = npcId;
        _tokenManager = tokenManager;
    }

    public void Apply(ConversationState state)
    {
        bool spent = _tokenManager.SpendTokensWithNPC(_tokenType, _amount, _npcId);
        if (!spent)
        {
            throw new InvalidOperationException($"Failed to spend {_amount} {_tokenType} tokens");
        }
    }

    public string GetDescriptionForPlayer()
    {
        return $"Used {_tokenType} influence";
    }
}

/// <summary>
/// Effect for discovering new information during conversations.
/// </summary>
public class DiscoverInformationEffect : IMechanicalEffect
{
    private readonly Information _info;
    private readonly Player _player;

    public DiscoverInformationEffect(Information info, Player player)
    {
        _info = info;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        _player.AddKnownInformation(_info);
    }

    public string GetDescriptionForPlayer()
    {
        return "Learned something valuable";
    }
}

/// <summary>
/// Effect for advancing time during conversations.
/// </summary>
public class ConversationTimeEffect : IMechanicalEffect
{
    private readonly int _minutes;
    private readonly ITimeManager _timeManager;

    public ConversationTimeEffect(int minutes, ITimeManager timeManager)
    {
        _minutes = minutes;
        _timeManager = timeManager;
    }

    public void Apply(ConversationState state)
    {
        _timeManager.AdvanceTime(_minutes);
    }

    public string GetDescriptionForPlayer()
    {
        return _minutes > 30 ? "The conversation takes time" : "";
    }
}