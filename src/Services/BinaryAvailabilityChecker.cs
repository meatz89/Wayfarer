using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// DETERMINISTIC availability checker ensuring all actions have binary availability.
/// An action is either AVAILABLE (player can perform it) or LOCKED (cannot perform).
/// NO variable costs, NO partial availability, NO randomness.
/// </summary>
public class BinaryAvailabilityChecker
{
    private readonly TokenMechanicsManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly GameWorld _gameWorld;

    public BinaryAvailabilityChecker(
        TokenMechanicsManager tokenManager,
        ITimeManager timeManager,
        GameWorld gameWorld)
    {
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Check if a conversation choice is available.
    /// Validates: tier, tokens, time, NPC state.
    /// </summary>
    public AvailabilityResult CheckConversationChoice(
        ConversationChoiceRequirements requirements,
        AvailabilityContext context)
    {
        // Validation: context must be valid
        if (context?.Player == null || context?.NPC == null)
        {
            return new AvailabilityResult(false, "Invalid conversation context");
        }

        // 1. Check tier requirement (MOST RESTRICTIVE FIRST)
        if (requirements.RequiredTier > context.Player.CurrentTier)
        {
            return new AvailabilityResult(false, GetTierLockMessage(requirements.RequiredTier));
        }

        // 2. Check token requirements (exact amounts needed)
        foreach (TokenAmountRequirement tokenReq in requirements.TokenRequirements)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(context.NPC.ID);
            int available = tokens.GetValueOrDefault(tokenReq.Type, 0);

            if (available < tokenReq.Amount)
            {
                return new AvailabilityResult(false,
                    $"Requires {tokenReq.Amount} {tokenReq.Type} with {context.NPC.Name}");
            }
        }

        // 3. Check time requirements (certain actions only at specific times)
        if (requirements.TimeRequirements.Any())
        {
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            if (!requirements.TimeRequirements.Contains(currentTime))
            {
                return new AvailabilityResult(false,
                    $"Only available during {string.Join("/", requirements.TimeRequirements)}");
            }
        }

        // 4. Check NPC state requirements
        if (requirements.RequiredNPCStates.Any())
        {
            if (!requirements.RequiredNPCStates.Contains(context.NPCState))
            {
                return new AvailabilityResult(false,
                    $"{context.NPC.Name} must be {string.Join(" or ", requirements.RequiredNPCStates)}");
            }
        }

        // 5. Check queue state requirements
        if (requirements.RequiresQueueSpace)
        {
            int queueSize = context.QueueManager?.GetActiveObligations()?.Length ?? 0;
            if (queueSize >= 8)
            {
                return new AvailabilityResult(false, "Queue is full");
            }
        }

        // All requirements met
        return new AvailabilityResult(true, null);
    }

    /// <summary>
    /// Check if a location action is available.
    /// Validates: tier, tags, time, resources.
    /// </summary>
    public AvailabilityResult CheckLocationAction(
        LocationActionRequirements requirements,
        AvailabilityContext context)
    {
        // Validation: context must be valid
        if (context?.Player == null || context?.Location == null)
        {
            return new AvailabilityResult(false, "Invalid location context");
        }

        // 1. Check tier requirement (MOST RESTRICTIVE FIRST)
        if (requirements.RequiredTier > context.Player.CurrentTier)
        {
            return new AvailabilityResult(false, GetTierLockMessage(requirements.RequiredTier));
        }

        // 2. Check location tag requirements
        if (requirements.RequiredLocationTags.Any())
        {
            HashSet<string> locationTags = GetLocationTags(context.Location, context.LocationSpot);
            bool hasRequiredTag = requirements.RequiredLocationTags
                .Any(tag => locationTags.Contains(tag));

            if (!hasRequiredTag)
            {
                return new AvailabilityResult(false,
                    $"Location needs: {string.Join(" or ", requirements.RequiredLocationTags)}");
            }
        }

        // 3. Check time requirements
        if (requirements.TimeRequirements.Any())
        {
            TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
            if (!requirements.TimeRequirements.Contains(currentTime))
            {
                return new AvailabilityResult(false,
                    $"Only available during {string.Join("/", requirements.TimeRequirements)}");
            }
        }

        // 4. Check resource requirements (coins)
        if (requirements.CoinCost > 0)
        {
            if (context.Player.Coins < requirements.CoinCost)
            {
                return new AvailabilityResult(false,
                    $"Requires {requirements.CoinCost} coins");
            }
        }

        // 5. Check attention availability (for location actions that cost attention)
        if (requirements.RequiresAttention && context.TimeBlockAttentionManager != null)
        {
            (int current, int max) attentionState = context.TimeBlockAttentionManager.GetAttentionState();
            if (attentionState.current == 0)
            {
                return new AvailabilityResult(false, "No attention remaining");
            }
        }

        // All requirements met
        return new AvailabilityResult(true, null);
    }

    /// <summary>
    /// Unified check for any interactive choice implementing IInteractiveChoice.
    /// Routes to appropriate specific checker based on interaction type.
    /// </summary>
    public AvailabilityResult CheckInteractiveChoice(
        IInteractiveChoice choice,
        AvailabilityContext context)
    {
        // Special cases that are always available
        if (choice.Type == InteractionType.ConversationFree ||
            choice.Type == InteractionType.SystemAction)
        {
            return new AvailabilityResult(true, null);
        }

        // Check attention availability first (universal requirement)
        if (choice.AttentionCost > 0 && context.TimeBlockAttentionManager != null)
        {
            (int current, int max) attentionState = context.TimeBlockAttentionManager.GetAttentionState();
            if (attentionState.current < choice.AttentionCost)
            {
                return new AvailabilityResult(false,
                    $"Requires {choice.AttentionCost} attention");
            }
        }

        // Route to specific checkers based on type
        switch (choice.Type)
        {
            case InteractionType.ConversationHelp:
            case InteractionType.ConversationNegotiate:
            case InteractionType.ConversationInvestigate:
                // Build requirements from choice properties
                ConversationChoiceRequirements convReqs = BuildConversationRequirements(choice);
                return CheckConversationChoice(convReqs, context);

            case InteractionType.LocationMove:
            case InteractionType.LocationWait:
            case InteractionType.LocationInteract:
                // Build requirements from choice properties
                LocationActionRequirements locReqs = BuildLocationRequirements(choice);
                return CheckLocationAction(locReqs, context);

            case InteractionType.ObserveEnvironment:
            case InteractionType.ObserveNPC:
            case InteractionType.ObserveDetail:
                // Observations require T2 (Associate) by default
                if (context.Player.CurrentTier < TierLevel.T2)
                {
                    return new AvailabilityResult(false,
                        "Requires Associate standing to observe deeply");
                }
                return new AvailabilityResult(true, null);

            default:
                // Unknown type defaults to available
                return new AvailabilityResult(true, null);
        }
    }

    /// <summary>
    /// Get standardized tier lock message.
    /// </summary>
    private string GetTierLockMessage(TierLevel requiredTier)
    {
        return requiredTier switch
        {
            TierLevel.T2 => "Requires Associate standing",
            TierLevel.T3 => "Requires Confidant standing",
            _ => "Available to all"
        };
    }

    /// <summary>
    /// Extract all tags from location and spot.
    /// </summary>
    private HashSet<string> GetLocationTags(Location location, LocationSpot spot)
    {
        HashSet<string> tags = new HashSet<string>();

        // Add location service tags
        if (location.AvailableServices != null)
        {
            foreach (ServiceTypes service in location.AvailableServices)
            {
                tags.Add(service.ToString().ToLower());
            }
        }

        // Add spot domain tags
        if (spot?.DomainTags != null)
        {
            foreach (string tag in spot.DomainTags)
            {
                tags.Add(tag.ToLower());
            }
        }

        // Add time-based tags
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        if (currentTime == TimeBlocks.Morning &&
            location.MorningProperties?.Contains("market_day") == true)
        {
            tags.Add("market");
        }

        // Add atmosphere-based tags
        if (location.Atmosphere.HasValue)
        {
            tags.Add(location.Atmosphere.Value.GetPropertyValue().ToLower());
        }

        return tags;
    }

    /// <summary>
    /// Build conversation requirements from an interactive choice.
    /// This is used when checking choices through the unified interface.
    /// </summary>
    private ConversationChoiceRequirements BuildConversationRequirements(IInteractiveChoice choice)
    {
        ConversationChoiceRequirements reqs = new ConversationChoiceRequirements();

        // Infer tier from interaction type
        reqs.RequiredTier = choice.Type switch
        {
            InteractionType.ConversationHelp => TierLevel.T1,      // Help is always available
            InteractionType.ConversationNegotiate => TierLevel.T1, // Basic negotiate is T1
            InteractionType.ConversationInvestigate => TierLevel.T2, // Investigate requires T2
            _ => TierLevel.T1
        };

        // High attention costs usually indicate higher tier requirements
        if (choice.AttentionCost >= 3)
        {
            reqs.RequiredTier = TierLevel.T3;
        }
        else if (choice.AttentionCost >= 2 && reqs.RequiredTier < TierLevel.T2)
        {
            reqs.RequiredTier = TierLevel.T2;
        }

        return reqs;
    }

    /// <summary>
    /// Build location requirements from an interactive choice.
    /// </summary>
    private LocationActionRequirements BuildLocationRequirements(IInteractiveChoice choice)
    {
        LocationActionRequirements reqs = new LocationActionRequirements();

        // Default tier is T1 for most location actions
        reqs.RequiredTier = TierLevel.T1;

        // Wait actions require attention
        if (choice.Type == InteractionType.LocationWait)
        {
            reqs.RequiresAttention = false; // Wait is free when exhausted
        }

        return reqs;
    }
}

/// <summary>
/// Result of availability check with binary outcome.
/// </summary>
public class AvailabilityResult
{
    public bool IsAvailable { get; }
    public string LockReason { get; }

    public AvailabilityResult(bool isAvailable, string lockReason)
    {
        IsAvailable = isAvailable;
        LockReason = lockReason;

        // Validation: locked actions MUST have a reason
        if (!isAvailable && string.IsNullOrEmpty(lockReason))
        {
            throw new ArgumentException("Locked actions must provide a lock reason");
        }
    }
}

/// <summary>
/// Context containing all data needed for availability checks.
/// </summary>
public class AvailabilityContext
{
    public Player Player { get; set; }
    public NPC NPC { get; set; }
    public NPCEmotionalState NPCState { get; set; }
    public Location Location { get; set; }
    public LocationSpot LocationSpot { get; set; }
    public ObligationQueueManager QueueManager { get; set; }
    public TimeBlockAttentionManager TimeBlockAttentionManager { get; set; }
    public TimeBlocks CurrentTime { get; set; }
}

/// <summary>
/// Requirements for conversation choices.
/// All fields have sensible defaults for T1 availability.
/// </summary>
public class ConversationChoiceRequirements
{
    public TierLevel RequiredTier { get; set; } = TierLevel.T1;
    public List<TokenAmountRequirement> TokenRequirements { get; set; } = new();
    public List<TimeBlocks> TimeRequirements { get; set; } = new();
    public List<NPCEmotionalState> RequiredNPCStates { get; set; } = new();
    public bool RequiresQueueSpace { get; set; } = false;
}

/// <summary>
/// Requirements for location actions.
/// </summary>
public class LocationActionRequirements
{
    public TierLevel RequiredTier { get; set; } = TierLevel.T1;
    public List<string> RequiredLocationTags { get; set; } = new();
    public List<TimeBlocks> TimeRequirements { get; set; } = new();
    public int CoinCost { get; set; } = 0;
    public bool RequiresAttention { get; set; } = false;
}

/// <summary>
/// Token amount requirement specification for availability checks.
/// </summary>
public class TokenAmountRequirement
{
    public ConnectionType Type { get; set; }
    public int Amount { get; set; }

    public TokenAmountRequirement(ConnectionType type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}