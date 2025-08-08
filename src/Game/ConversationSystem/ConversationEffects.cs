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

/// <summary>
/// No change to game state - maintains current situation
/// </summary>
public class MaintainStateEffect : IMechanicalEffect
{
    public void Apply(ConversationState state) { }
    public string GetDescriptionForPlayer() => "→ Maintains current state";
}

/// <summary>
/// Opens queue negotiation interface
/// </summary>
public class OpenNegotiationEffect : IMechanicalEffect
{
    public void Apply(ConversationState state) 
    {
        // Mark that negotiation should be opened (handled by UI)
        state.AdvanceDuration(1);
    }
    public string GetDescriptionForPlayer() => "✓ Opens negotiation";
}

/// <summary>
/// Gain information or rumor
/// </summary>
public class GainInformationEffect : IMechanicalEffect
{
    private readonly string _information;
    private readonly string _infoType;

    public GainInformationEffect(string information, InfoType infoType)
    {
        _information = information;
        _infoType = infoType.ToString().ToLower();
    }

    public void Apply(ConversationState state)
    {
        // Store information as a memory
        var memory = new MemoryFlag
        {
            Key = $"info_{_infoType}_{Guid.NewGuid()}",
            Description = _information,
            Importance = 5
        };
        state.Player.Memories.Add(memory);
    }

    public string GetDescriptionForPlayer() => $"ℹ Gain {_infoType}: \"{_information}\"";
}

/// <summary>
/// Time passes during conversation
/// </summary>
public class TimePassageEffect : IMechanicalEffect
{
    private readonly int _minutes;

    public TimePassageEffect(int minutes)
    {
        _minutes = minutes;
    }

    public void Apply(ConversationState state)
    {
        // Advance conversation duration
        state.DurationCounter += _minutes / 5; // Convert to conversation rounds
    }

    public string GetDescriptionForPlayer() => $"⏱ +{_minutes} minutes conversation";
}

/// <summary>
/// Create a binding obligation/promise
/// </summary>
public class CreateBindingObligationEffect : IMechanicalEffect
{
    private readonly string _npcId;
    private readonly string _obligationText;

    public CreateBindingObligationEffect(string npcId, string obligationText)
    {
        _npcId = npcId;
        _obligationText = obligationText;
    }

    public void Apply(ConversationState state)
    {
        // Create obligation that affects queue entry position
        var obligation = new StandingObligation
        {
            ID = Guid.NewGuid().ToString(),
            Name = _obligationText,
            Description = _obligationText,
            Source = _npcId
        };
        state.Player.StandingObligations.Add(obligation);
    }

    public string GetDescriptionForPlayer() => "⛓ Creates Binding Obligation";
}

/// <summary>
/// Deep investigation reveals important information
/// </summary>
public class DeepInvestigationEffect : IMechanicalEffect
{
    private readonly string _topic;

    public DeepInvestigationEffect(string topic)
    {
        _topic = topic;
    }

    public void Apply(ConversationState state)
    {
        // Reveal hidden information as a memory
        var memory = new MemoryFlag
        {
            Key = $"investigation_{Guid.NewGuid()}",
            Description = $"Investigation revealed: {_topic}",
            Importance = 10
        };
        state.Player.Memories.Add(memory);
    }

    public string GetDescriptionForPlayer() => $"🔍 Deep investigation: {_topic}";
}

/// <summary>
/// Unlock a new route
/// </summary>
public class UnlockRouteEffect : IMechanicalEffect
{
    private readonly string _routeName;

    public UnlockRouteEffect(string routeName)
    {
        _routeName = routeName;
    }

    public void Apply(ConversationState state)
    {
        // Add route to player's known routes
        var route = new RouteOption
        {
            Id = Guid.NewGuid().ToString(),
            Name = _routeName,
            TravelTimeHours = 1,
            Description = $"Fast route to Noble Quarter via {_routeName}"
        };
        
        var fromLocation = state.Player.CurrentLocationSpot?.LocationId ?? "market_square";
        
        if (!state.Player.KnownRoutes.ContainsKey(fromLocation))
            state.Player.KnownRoutes.Add(fromLocation, new List<RouteOption>());
            
        state.Player.KnownRoutes[fromLocation].Add(route);
    }

    public string GetDescriptionForPlayer() => $"🗺 Unlock route: {_routeName}";
}

/// <summary>
/// Choice is locked due to requirements
/// </summary>
public class LockedEffect : IMechanicalEffect
{
    private readonly string _reason;

    public LockedEffect(string reason)
    {
        _reason = reason;
    }

    public void Apply(ConversationState state) { }
    public string GetDescriptionForPlayer() => _reason;
}

// Info type enum for information effects
public enum InfoType
{
    Rumor,
    Secret,
    Route,
    Schedule,
    Weakness
}