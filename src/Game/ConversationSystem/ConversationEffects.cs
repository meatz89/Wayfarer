using System;
using System.Collections.Generic;

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
            var tokenIcon = _tokenType switch
            {
                ConnectionType.Trust => "❤",
                ConnectionType.Commerce => "🪙",
                ConnectionType.Status => "👑",
                _ => "?"
            };
            return $"✓ Move letter to position {_targetPosition} | {tokenIcon} -{_tokenCost} {_tokenType} token{(_tokenCost != 1 ? "s" : "")}";
        }
        else
        {
            return $"✓ Move letter to position {_targetPosition}";
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

    public int Amount => _amount; // Expose for filtering

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
        var tokenIcon = _tokenType switch
        {
            ConnectionType.Trust => "♥",
            ConnectionType.Commerce => "🪙",
            ConnectionType.Status => "👑",
            _ => "?"
        };
        return $"{tokenIcon} +{_amount} {_tokenType} token{(_amount != 1 ? "s" : "")}";
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
        var tokenIcon = _tokenType switch
        {
            ConnectionType.Trust => "♥",
            ConnectionType.Commerce => "🪙",
            ConnectionType.Status => "👑",
            _ => "?"
        };
        return $"{tokenIcon} -{_amount} {_tokenType} token{(_amount != 1 ? "s" : "")}";
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
        if (_timeManager != null)
        {
            // Use the new AdvanceTimeMinutes method to preserve minute granularity
            _timeManager.AdvanceTimeMinutes(_minutes);
        }
    }

    public string GetDescriptionForPlayer()
    {
        if (_minutes >= 60)
            return $"⏱ +{_minutes / 60} hour{(_minutes >= 120 ? "s" : "")} conversation";
        else if (_minutes > 0)
            return $"⏱ +{_minutes} minutes conversation";
        return "";
    }
}

/// <summary>
/// Effect for temporarily removing a letter from the queue.
/// </summary>
public class RemoveLetterTemporarilyEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;

    public RemoveLetterTemporarilyEffect(string letterId, LetterQueueManager queueManager)
    {
        _letterId = letterId;
        _queueManager = queueManager;
    }

    public void Apply(ConversationState state)
    {
        var position = _queueManager.GetLetterPosition(_letterId);
        if (position.HasValue)
        {
            _queueManager.RemoveLetterFromQueue(position.Value);
            // TODO: Add to temporary storage for later retrieval
        }
    }

    public string GetDescriptionForPlayer()
    {
        return "📜 Letter held temporarily (can be retrieved later)";
    }
}

/// <summary>
/// Effect for accepting a new letter into the queue.
/// </summary>
public class AcceptLetterEffect : IMechanicalEffect
{
    private readonly Letter _letter;
    private readonly LetterQueueManager _queueManager;

    public AcceptLetterEffect(Letter letter, LetterQueueManager queueManager)
    {
        _letter = letter;
        _queueManager = queueManager;
    }

    public void Apply(ConversationState state)
    {
        // Add to next available position
        var currentCount = _queueManager.GetActiveLetters().Count();
        _queueManager.AddLetterToQueue(_letter, currentCount + 1);
    }

    public string GetDescriptionForPlayer()
    {
        return $"📜 +1 letter to queue (from {_letter.SenderName})";
    }
}

/// <summary>
/// Effect for extending a letter's deadline.
/// </summary>
public class ExtendDeadlineEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly int _daysToAdd;
    private readonly LetterQueueManager _queueManager;

    public ExtendDeadlineEffect(string letterId, int daysToAdd, LetterQueueManager queueManager)
    {
        _letterId = letterId;
        _daysToAdd = daysToAdd;
        _queueManager = queueManager;
    }

    public void Apply(ConversationState state)
    {
        var position = _queueManager.GetLetterPosition(_letterId);
        if (position.HasValue)
        {
            var letter = _queueManager.GetLetterAt(position.Value);
            letter.DeadlineInHours += _daysToAdd * 24;
        }
    }

    public string GetDescriptionForPlayer()
    {
        return $"⏱ +{_daysToAdd * 24} hours to deadline";
    }
}

/// <summary>
/// Effect for sharing information with an NPC.
/// </summary>
public class ShareInformationEffect : IMechanicalEffect
{
    private readonly RouteOption _route;
    private readonly NPC _npc;

    public ShareInformationEffect(RouteOption route, NPC npc)
    {
        _route = route;
        _npc = npc;
    }

    public void Apply(ConversationState state)
    {
        _npc.AddKnownRoute(_route);
    }

    public string GetDescriptionForPlayer()
    {
        return $"ℹ Shared route: {_route.Name}";
    }
}

/// <summary>
/// Effect for creating a standing obligation.
/// </summary>
public class CreateObligationEffect : IMechanicalEffect
{
    private readonly string _obligationId;
    private readonly string _npcId;
    private readonly Player _player;

    public CreateObligationEffect(string obligationId, string npcId, Player player)
    {
        _obligationId = obligationId;
        _npcId = npcId;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        // TODO: Implement obligation system properly
        // For now, just track internally
        // _player.AddObligation(new StandingObligation 
        // { 
        //     ObligationId = _obligationId,
        //     Description = $"Permanent priority for {_npcId}'s letters"
        // });
    }

    public string GetDescriptionForPlayer()
    {
        return "⛓ Created permanent obligation (priority for their letters)";
    }
}

/// <summary>
/// Effect for unlocking routes through conversation.
/// </summary>
public class UnlockRoutesEffect : IMechanicalEffect
{
    private readonly List<RouteOption> _routes;
    private readonly Player _player;

    public UnlockRoutesEffect(List<RouteOption> routes, Player player)
    {
        _routes = routes;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        foreach (var route in _routes)
        {
            _player.AddKnownRoute(route);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return $"Learned {_routes.Count} new route(s)";
    }
}

/// <summary>
/// Effect for unlocking new NPCs.
/// </summary>
public class UnlockNPCEffect : IMechanicalEffect
{
    private readonly NPC _npcToUnlock;
    private readonly GameWorld _gameWorld;

    public UnlockNPCEffect(NPC npcToUnlock, GameWorld gameWorld)
    {
        _npcToUnlock = npcToUnlock;
        _gameWorld = gameWorld;
    }

    public void Apply(ConversationState state)
    {
        if (_npcToUnlock != null)
        {
            _gameWorld.AddNPC(_npcToUnlock);
            state.Player.AddKnownNPC(_npcToUnlock.ID);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return $"Met {_npcToUnlock?.Name ?? "someone new"}";
    }
}

/// <summary>
/// Effect for unlocking new locations.
/// </summary>
public class UnlockLocationEffect : IMechanicalEffect
{
    private readonly string _locationId;
    private readonly GameWorld _gameWorld;

    public UnlockLocationEffect(string locationId, GameWorld gameWorld)
    {
        _locationId = locationId;
        _gameWorld = gameWorld;
    }

    public void Apply(ConversationState state)
    {
        state.Player.AddKnownLocation(_locationId);
    }

    public string GetDescriptionForPlayer()
    {
        return $"Discovered new location";
    }
}

/// <summary>
/// Effect for discovering new routes during conversations.
/// </summary>
public class DiscoverRouteEffect : IMechanicalEffect
{
    private readonly RouteOption _route;
    private readonly Player _player;

    public DiscoverRouteEffect(RouteOption route, Player player)
    {
        _route = route;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        _player.AddKnownRoute(_route);
    }

    public string GetDescriptionForPlayer()
    {
        return $"Discovered route: {_route?.Name ?? "new path"}";
    }
}