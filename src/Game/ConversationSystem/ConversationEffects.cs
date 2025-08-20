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
    private readonly TokenMechanicsManager _tokenManager;
    private readonly string _npcId;

    public LetterReorderEffect(
        string letterId,
        int targetPosition,
        int tokenCost,
        ConnectionType tokenType,
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager,
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        string npcName = GetNpcDisplayName(_npcId);
        MechanicalEffectDescription desc = new MechanicalEffectDescription
        {
            Text = _tokenCost > 0
                ? $"Move to slot {_targetPosition} (burn {_tokenCost} {_tokenType} with {npcName})"
                : $"Move letter to position {_targetPosition}",
            Category = EffectCategory.LetterReorder,
            LetterPosition = _targetPosition,
            LetterId = _letterId,
            NpcId = _npcId
        };

        if (_tokenCost > 0)
        {
            desc.TokenType = _tokenType;
            desc.TokenAmount = _tokenCost;
        }

        return new List<MechanicalEffectDescription> { desc };
    }

    private string GetNpcDisplayName(string npcId)
    {
        return npcId switch
        {
            "elena" => "Elena",
            "marcus" => "Brother Marcus",
            "lord_aldwin" => "Lord Aldwin",
            "captain_thorne" => "Captain Thorne",
            "sister_agatha" => "Sister Agatha",
            _ => "them"
        };
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
    private readonly TokenMechanicsManager _tokenManager;

    public int Amount => _amount; // Expose for filtering

    public GainTokensEffect(
        ConnectionType tokenType,
        int amount,
        string npcId,
        TokenMechanicsManager tokenManager)
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        // Get NPC name for clarity
        string npcName = GetNpcDisplayName(_npcId);

        return new List<MechanicalEffectDescription>
        {
            new MechanicalEffectDescription
            {
                Text = $"+{_amount} {_tokenType} with {npcName}",
                Category = EffectCategory.TokenGain,
                TokenType = _tokenType,
                TokenAmount = _amount,
                NpcId = _npcId
            }
        };
    }

    private string GetNpcDisplayName(string npcId)
    {
        // TODO: Get from NPC repository or pass in constructor
        // For now, use a reasonable default
        return npcId switch
        {
            "elena" => "Elena",
            "marcus" => "Brother Marcus",
            "lord_aldwin" => "Lord Aldwin",
            "captain_thorne" => "Captain Thorne",
            "sister_agatha" => "Sister Agatha",
            _ => "them"
        };
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
    private readonly TokenMechanicsManager _tokenManager;

    public BurnTokensEffect(
        ConnectionType tokenType,
        int amount,
        string npcId,
        TokenMechanicsManager tokenManager)
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        // Get NPC name for clarity
        string npcName = GetNpcDisplayName(_npcId);

        return new List<MechanicalEffectDescription>
        {
            new MechanicalEffectDescription
            {
                Text = $"Burn {_amount} {_tokenType} with {npcName}",
                Category = EffectCategory.TokenSpend,
                TokenType = _tokenType,
                TokenAmount = _amount,
                NpcId = _npcId
            }
        };
    }

    private string GetNpcDisplayName(string npcId)
    {
        // TODO: Get from NPC repository or pass in constructor
        // For now, use a reasonable default
        return npcId switch
        {
            "elena" => "Elena",
            "marcus" => "Brother Marcus",
            "lord_aldwin" => "Lord Aldwin",
            "captain_thorne" => "Captain Thorne",
            "sister_agatha" => "Sister Agatha",
            _ => "them"
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        string text;
        if (_minutes >= 60)
            text = $"+{_minutes / 60} hour{(_minutes >= 120 ? "s" : "")} conversation";
        else if (_minutes > 0)
            text = $"+{_minutes} minutes conversation";
        else
            text = "";

        return new List<MechanicalEffectDescription>
        {
            new MechanicalEffectDescription
            {
                Text = text,
                Category = EffectCategory.TimePassage,
                TimeMinutes = _minutes
            }
        };
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
        int? position = _queueManager.GetLetterPosition(_letterId);
        if (position.HasValue)
        {
            _queueManager.RemoveLetterFromQueue(position.Value);
            // TODO: Add to temporary storage for later retrieval
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Letter held temporarily (can be retrieved later)",
                Category = EffectCategory.LetterRemove
            }
        };
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
        int currentCount = _queueManager.GetActiveLetters().Count();
        _queueManager.AddLetterToQueue(_letter, currentCount + 1);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"+1 letter to queue (from {_letter.SenderName})",
                Category = EffectCategory.LetterAdd,
                LetterId = _letter.Id
            }
        };
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
        int? position = _queueManager.GetLetterPosition(_letterId);
        if (position.HasValue)
        {
            Letter letter = _queueManager.GetLetterAt(position.Value);
            letter.DeadlineInHours += _daysToAdd * 24;
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"+{_daysToAdd * 24} hours to deadline",
                Category = EffectCategory.DeadlineExtend,
                TimeMinutes = _daysToAdd * 24 * 60,
                LetterId = _letterId
            }
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Shared route: {_route.Name}",
                Category = EffectCategory.InformationReveal,
                RouteName = _route.Name
            }
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Created permanent obligation (priority for their letters)",
                Category = EffectCategory.ObligationCreate,
                IsObligationBinding = true,
                NpcId = _npcId
            }
        };
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
        foreach (RouteOption route in _routes)
        {
            _player.AddKnownRoute(route);
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Learned {_routes.Count} new route(s)",
                Category = EffectCategory.RouteUnlock
            }
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Met {_npcToUnlock?.Name ?? "someone new"}",
                Category = EffectCategory.NpcUnlock,
                NpcId = _npcToUnlock?.ID
            }
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Discovered new location",
                Category = EffectCategory.LocationUnlock,
                LocationId = _locationId
            }
        };
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Discovered route: {_route?.Name ?? "new path"}",
                Category = EffectCategory.RouteUnlock,
                RouteName = _route?.Name
            }
        };
    }
}

/// <summary>
/// No change to game state - maintains current situation
/// </summary>
public class MaintainStateEffect : IMechanicalEffect
{
    public void Apply(ConversationState state) { }
    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = "Maintains current state",
            Category = EffectCategory.StateChange
        }
    };
    }
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
    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = "Opens negotiation",
            Category = EffectCategory.NegotiationOpen
        }
    };
    }
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
        MemoryFlag memory = new MemoryFlag
        {
            Key = $"info_{_infoType}_{Guid.NewGuid()}",
            Description = _information,
            Importance = 5
        };
        state.Player.Memories.Add(memory);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = $"Gain {_infoType}: \"{_information}\"",
            Category = EffectCategory.InformationGain,
            IsInformationRevealed = true
        }
    };
    }
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

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = $"+{_minutes} minutes conversation",
            Category = EffectCategory.TimePassage,
            TimeMinutes = _minutes
        }
    };
    }
}

/// <summary>
/// Create a binding obligation/promise
/// </summary>
public class CreateBindingObligationEffect : IMechanicalEffect
{
    private readonly string _npcId;
    private readonly string _obligationText;
    private readonly ConnectionType _obligationType;

    public CreateBindingObligationEffect(string npcId, string obligationText, ConnectionType obligationType = ConnectionType.Trust)
    {
        _npcId = npcId;
        _obligationText = obligationText;
        _obligationType = obligationType;
    }

    public void Apply(ConversationState state)
    {
        // Create categorical obligation based on relationship type
        StandingObligation obligation = new StandingObligation
        {
            ID = $"crisis_obligation_{_npcId}_{_obligationType}",
            Name = _obligationText,
            Description = _obligationText,
            Source = _npcId,
            RelatedTokenType = _obligationType,
            RelatedNPCId = _npcId,
            IsActive = true,
            DayAccepted = state.GameWorld?.CurrentDay ?? 1,
            DaysSinceAccepted = 0
        };

        // Add categorical effects based on obligation type
        switch (_obligationType)
        {
            case ConnectionType.Trust:
                obligation.BenefitEffects.Add(ObligationEffect.PatronLettersPosition1);
                break;
            case ConnectionType.Commerce:
                obligation.BenefitEffects.Add(ObligationEffect.CommercePriority);
                break;
            case ConnectionType.Status:
                obligation.ConstraintEffects.Add(ObligationEffect.NoStatusRefusal);
                break;
            case ConnectionType.Shadow:
                obligation.ConstraintEffects.Add(ObligationEffect.CannotRefuseLetters);
                break;
        }

        state.Player.StandingObligations.Add(obligation);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        string npcName = GetNpcDisplayName(_npcId);
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"⚠️ Binding Obligation to {npcName} (permanent)",
                Category = EffectCategory.ObligationCreate,
                IsObligationBinding = true,
                NpcId = _npcId
            }
        };
    }

    private string GetNpcDisplayName(string npcId)
    {
        return npcId switch
        {
            "elena" => "Elena",
            "marcus" => "Brother Marcus",
            "lord_aldwin" => "Lord Aldwin",
            "captain_thorne" => "Captain Thorne",
            "sister_agatha" => "Sister Agatha",
            _ => "them"
        };
    }
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
        MemoryFlag memory = new MemoryFlag
        {
            Key = $"investigation_{Guid.NewGuid()}",
            Description = $"Investigation revealed: {_topic}",
            Importance = 10
        };
        state.Player.Memories.Add(memory);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = $"🔍 Uncover: {_topic}",
            Category = EffectCategory.InformationGain,
            IsInformationRevealed = true
        }
    };
    }
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
        RouteOption route = new RouteOption
        {
            Id = Guid.NewGuid().ToString(),
            Name = _routeName,
            TravelTimeMinutes = 1,
            Description = $"Fast route to Noble Quarter via {_routeName}"
        };

        string fromLocation = state.Player.CurrentLocationSpot?.LocationId ?? "market_square";

        if (!state.Player.KnownRoutes.ContainsKey(fromLocation))
            state.Player.KnownRoutes.Add(fromLocation, new List<RouteOption>());

        state.Player.KnownRoutes[fromLocation].Add(route);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = $"Unlock route: {_routeName}",
            Category = EffectCategory.RouteUnlock,
            RouteName = _routeName
        }
    };
    }
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
    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = _reason,
            Category = EffectCategory.StateChange
        }
    };
    }
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

/// <summary>
/// Opens the queue management interface during conversation.
/// Allows direct manipulation without leaving conversation.
/// </summary>
public class OpenQueueInterfaceEffect : IMechanicalEffect
{
    public void Apply(ConversationState state)
    {
        // This effect triggers UI state change
        // The actual UI handling happens in the conversation screen
        state.IsQueueInterfaceOpen = true;
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
        new MechanicalEffectDescription {
            Text = "Open queue management interface",
            Category = EffectCategory.InterfaceAction
        }
    };
    }
}

/// <summary>
/// Restores NPC relationship from Betrayed state to a specific target relationship.
/// Simplified betrayal recovery system - immediate but costly restoration.
/// </summary>
public class RestoreRelationshipEffect : IMechanicalEffect
{
    private readonly string _npcId;
    private readonly NPCRelationship _targetRelationship;
    private readonly GameWorld _gameWorld;

    public RestoreRelationshipEffect(string npcId, NPCRelationship targetRelationship, GameWorld gameWorld)
    {
        _npcId = npcId;
        _targetRelationship = targetRelationship;
        _gameWorld = gameWorld;
    }

    public void Apply(ConversationState state)
    {
        NPC? npc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == _npcId);
        if (npc == null) return;

        // Only restore if currently in Betrayed state
        if (npc.PlayerRelationship == NPCRelationship.Betrayed)
        {
            // Use proper NPCStateOperations for state change
            NPCState currentState = NPCState.FromNPC(npc);
            NPCOperationResult result = NPCStateOperations.UpdateRelationship(currentState, _targetRelationship);

            if (result.IsSuccess)
            {
                // Update mutable NPC object
                npc.PlayerRelationship = _targetRelationship;
            }
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        string relationshipName = _targetRelationship switch
        {
            NPCRelationship.Unfriendly => "Unfriendly",
            NPCRelationship.Neutral => "Neutral",
            NPCRelationship.Wary => "Wary",
            _ => _targetRelationship.ToString()
        };

        return new List<MechanicalEffectDescription>
        {
            new MechanicalEffectDescription
            {
                Text = $"Restore relationship to {relationshipName}",
                Category = EffectCategory.RelationshipChange
            }
        };
    }
}